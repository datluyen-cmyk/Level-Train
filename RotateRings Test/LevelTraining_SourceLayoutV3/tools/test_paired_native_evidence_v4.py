"""Integrity/censoring tests for paired bounded computer evidence."""
import copy
import unittest

from summarize_paired_native_evidence_v4 import binary_string, hash_bundle, compare_reports, row_summary


RULE = "a"*64
NEW_RULE = "b"*64


def report(identifier):
    return {"rulesHash": RULE, "inputHash": "input", "unityVersion": "6000.0.78f1", "fixedDeltaTime": 1/60,
            "maxStates": 128, "wallTimeBudgetMs": 15000, "completedLevels": 1, "totalLevels": 1,
            "levels": [{"sourcePosition": 2, "sourceId": identifier, "partCount": 2, "status": "solved", "goal": True,
                        "replayGoal": True, "moveCount": 3, "actions": [{"kind": "rotate", "part": 1, "delta": 90},
                        {"kind": "rotate", "part": 2, "delta": 45}, {"kind": "rotate", "part": 1, "delta": -30}],
                        "exploredStates": 4, "generatedActions": 12, "attemptedActions": 8, "appliedActions": 8,
                        "searchElapsedMs": 20, "actionTrace": []}]}


def compare(a=None, b=None, current=RULE):
    a = report(10002) if a is None else a
    b = report(7800002) if b is None else b
    return compare_reports(a, b, {2: {"id": 10002, "partIds": [1, 2]}},
                           {2: {"id": 7800002, "partIds": [1, 2]}}, "input", "input", current)


def captured_row():
    row = report(10002)["levels"][0]
    poses = [(0, 20), (0, 45), (20, 5)]
    for i, (action, (before, after)) in enumerate(zip(row["actions"], poses)):
        action.update(replayWitnessCaptured=True, actualAngleBeforeDegrees=before, actualAngleAfterDegrees=after,
                      actualSlideBefore=0, actualSlideAfter=0, actualMovementConsumed=after-before,
                      replayBlocked=i == 0, replayBlockerOwner=2 if i == 0 else -1, replayBlockerShapeIndex=0 if i == 0 else -1,
                      replayBlockerRole="body" if i == 0 else "", activeBindingCountBefore=3-i, activeBindingCountAfter=2-i,
                      removedIdsBefore=[], removedIdsAfter=[1, 2] if i == 2 else [], removedIdsDelta=[1, 2] if i == 2 else [])
    row["actions"][1]["blocked"] = True  # Legacy flag may represent a bomb, not physical clamp.
    return row


class PairedNativeEvidence(unittest.TestCase):
    def test_csharp_binarywriter_hash_with_multibyte_7bit_length(self):
        # Independently produced by System.IO.BinaryWriter and SHA256 in PowerShell.
        self.assertEqual(hash_bundle(["α"*70], [bytes([0, 127, 255])], "u", 1/60),
                         "43e6313e8d141b50fc9bf2b10ff204ebee33724d7a1b189617fc6f76412ea270")
        self.assertEqual(binary_string("é"*65)[:2], bytes([130, 1]))

    def test_matching_inputs_rules_and_budgets_pair(self):
        result = compare()
        self.assertTrue(result["inputAndReportedRulesBudgetPairingValid"])
        self.assertTrue(result["currentRuleMatch"])
        self.assertTrue(result["levels"][0]["pairedCompleteSolveWitnesses"])
        path = result["levels"][0]["source"]["solvedPath"]
        self.assertEqual(path["partSwitches"], 2)
        self.assertEqual(path["distinctActedRootCount"], 2)
        self.assertEqual(path["perRootRequestedRotationSignReversals"], 1)
        self.assertEqual(path["consecutiveSameRootRequestedRotationReversals"], 0)

    def test_mismatched_rules_are_unpaired(self):
        b = report(7800002)
        b["rulesHash"] = NEW_RULE
        result = compare(b=b)
        self.assertFalse(result["inputAndReportedRulesBudgetPairingValid"])
        self.assertIsNone(result["levels"][0]["candidateOverSourceRatios"]["boundedSolverSegments"])

    def test_same_historical_rules_remain_labeled_historical(self):
        result = compare(current=NEW_RULE)
        self.assertTrue(result["inputAndReportedRulesBudgetPairingValid"])
        self.assertFalse(result["currentRuleMatch"])
        self.assertFalse(result["historicalRulesSnapshotByteVerified"])
        self.assertTrue(result["status"].startswith("HISTORICAL_"))

    def test_stale_input_hash_unpairs(self):
        b = report(7800002)
        b["inputHash"] = "stale"
        self.assertFalse(compare(b=b)["inputAndReportedRulesBudgetPairingValid"])

    def test_different_search_budgets_unpair(self):
        for key in ("maxStates", "wallTimeBudgetMs"):
            b = report(7800002)
            b[key] += 1
            self.assertFalse(compare(b=b)["inputAndReportedRulesBudgetPairingValid"])

    def test_unknown_negative_one_move_count_is_missing_and_censored(self):
        b = report(7800002)
        b["levels"][0].update(status="unknown", goal=False, replayGoal=False, moveCount=-1, actions=[])
        result = compare(b=b)
        row = result["levels"][0]
        self.assertTrue(row["candidate"]["censored"])
        self.assertIsNone(row["candidate"]["solvedPath"])
        self.assertEqual(row["candidate"]["reportedMoveCountRaw"], -1)
        self.assertTrue(all(value is None for value in row["candidateOverSourceRatios"].values()))

    def test_missing_row_censored(self):
        b = report(7800002)
        b.update(levels=[], completedLevels=0)
        result = compare(b=b)
        self.assertEqual(result["levels"][0]["candidate"]["status"], "missing")
        self.assertIsNone(result["levels"][0]["candidate"]["solvedPath"])

    def test_duplicate_report_position_rejected(self):
        b = report(7800002)
        b["levels"].append(copy.deepcopy(b["levels"][0]))
        with self.assertRaisesRegex(ValueError, "Duplicate report"):
            compare(b=b)

    def test_pack_identity_mismatch_unpairs(self):
        b = report(7800002)
        b["levels"][0]["sourceId"] = 999
        self.assertFalse(compare(b=b)["inputAndReportedRulesBudgetPairingValid"])

    def test_solved_action_count_mismatch_rejected(self):
        row = report(10002)["levels"][0]
        row["moveCount"] = 0
        with self.assertRaisesRegex(ValueError, "action count"):
            row_summary(row)

    def test_zero_source_denominator_yields_null_ratio(self):
        a = report(10002)
        a["levels"][0]["searchElapsedMs"] = 0
        self.assertIsNone(compare(a=a)["levels"][0]["candidateOverSourceRatios"]["searchElapsedMs"])

    def test_independent_replay_reached_motion_separates_intent_and_blocking(self):
        result = row_summary(captured_row())
        self.assertEqual(result["solvedPath"]["requestedAngularDeltaMagnitudeDegrees"], 165)
        replay = result["independentReplay"]
        self.assertEqual(replay["actualAngularTravelMagnitudeDegrees"], 80)
        self.assertEqual(replay["physicalReplayBlockedSegmentCount"], 1)
        self.assertFalse(replay["steps"][1]["replayBlocked"])
        self.assertEqual(replay["perRootActualMovementSignReversals"], 1)
        self.assertEqual(replay["removedRootCountFromDeltas"], 2)
        self.assertEqual(replay["bindingCountDecreaseSum"], 3)

    def test_partial_replay_capture_is_missing_not_zero(self):
        row = captured_row()
        row["actions"][0]["replayWitnessCaptured"] = False
        self.assertIsNone(row_summary(row)["independentReplay"])

    def test_forged_reached_motion_rejected(self):
        row = captured_row()
        row["actions"][0]["actualMovementConsumed"] = 90
        with self.assertRaisesRegex(ValueError, "reached before/after"):
            row_summary(row)

    def test_forged_replay_removal_delta_rejected(self):
        row = captured_row()
        row["actions"][-1]["removedIdsDelta"] = []
        with self.assertRaisesRegex(ValueError, "removed-ID delta"):
            row_summary(row)

    def test_replay_binding_state_discontinuity_rejected(self):
        row = captured_row()
        row["actions"][1]["activeBindingCountBefore"] = 99
        with self.assertRaisesRegex(ValueError, "state discontinuity"):
            row_summary(row)

    def test_solver_policy_id_mismatch_unpairs(self):
        a, b = report(10002), report(7800002)
        a["policyId"], b["policyId"] = "policy-a", "policy-b"
        self.assertFalse(compare(a=a, b=b)["inputAndReportedRulesBudgetPairingValid"])

    def test_missing_actual_search_budget_unpairs(self):
        a, b = report(10002), report(7800002)
        a.pop("maxStates")
        b.pop("maxStates")
        self.assertFalse(compare(a=a, b=b)["inputAndReportedRulesBudgetPairingValid"])


if __name__ == "__main__":
    unittest.main()

"""Read-only audit probes for the v5 virtual playtest evidence contract.

Expected failures characterize gaps in the current summarizer. No Unity code
or live report is mutated by these synthetic fixtures.
"""
import copy
import unittest

from summarize_virtual_playtest_v5 import summarize


def report_fixture():
    profile = {"profileId": "u0", "seed": 101, "policyFamily": "test",
               "partition": "selection"}
    pair = {"referencePairId": "pair0", "referenceId": 10, "candidateId": 900}
    rows = []
    for role, level_id, won, termination in (
            ("reference", 10, True, "won"), ("candidate", 900, False, "step_limit")):
        rows.append({"sessionId": "pair0-"+role+"-u0", "profileId": "u0", "policyFamily": "test",
            "cohortId": "pilot", "role": role, "referencePairId": "pair0",
            "levelId": level_id, "levelSha256": "level-hash", "sessionSeed": 101 ^ (10*48611),
            "won": won, "termination": termination, "actionCount": 0, "actions": []})
    return {"schemaVersion": 5, "rulesHash": "rules", "policySuiteHash": "policy",
        "inputPackSha256": "source", "candidatePackSha256": "candidate", "templatesSha256": "templates",
        "complete": True, "status": "complete", "completedSessions": 2, "totalSessions": 2,
        "config": {"users": 1, "seed": 101, "phase": "pilot", "candidatePackPath": "candidate.json", "pairs": [pair]},
        "profiles": [profile], "sessions": rows}


class VirtualPlaytestAuditProbes(unittest.TestCase):
    def test_pair_seeds_are_checked_against_profile_and_reference_id(self):
        report = report_fixture()
        report["sessions"][0]["sessionSeed"] = 123456
        report["sessions"][1]["sessionSeed"] = 123456
        with self.assertRaisesRegex(ValueError, "seed"):
            summarize(report, requested_users=1, bootstrap_samples=20)

    def test_complete_run_with_engine_error_is_not_population_complete(self):
        report = report_fixture()
        row = report["sessions"][1]
        row.update(won=False, termination="engine_error")
        summary = summarize(report, requested_users=1, bootstrap_samples=20)
        self.assertFalse(summary["requestedPopulationRun"])

    def test_session_level_ids_match_configured_roles(self):
        report = report_fixture()
        report["sessions"][1]["levelId"] = 10
        with self.assertRaisesRegex(ValueError, "level"):
            summarize(report, requested_users=1, bootstrap_samples=20)

    def test_counters_match_retained_session_rows(self):
        report = report_fixture()
        report["completedSessions"] = 1
        with self.assertRaisesRegex(ValueError, "session"):
            summarize(report, requested_users=1, bootstrap_samples=20)


if __name__ == "__main__":
    unittest.main()

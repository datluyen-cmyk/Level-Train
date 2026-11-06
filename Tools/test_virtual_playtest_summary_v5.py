"""Synthetic report contract tests; these are not game-playtest evidence."""
import copy
import unittest

from summarize_virtual_playtest_v5 import summarize


def fixture():
    report = {"schemaVersion": 5, "rulesHash": "test-rules", "policySuiteHash": "test-suite",
        "inputPackSha256": "test-pack", "templatesSha256": "test-templates",
        "profiles": [{"profileId": "p1", "policyFamily": "visible"}], "sessions": []}
    for pair in (1, 2):
        for role in ("reference", "candidate"):
            won = role == "reference"
            report["sessions"].append({"sessionId": str(pair)+role, "profileId": "p1", "policyFamily": "visible",
                "cohortId": "test", "referencePairId": pair, "role": role,
                "sessionSeed": pair, "levelId": str(pair)+role, "levelSha256": "synthetic",
                "won": won, "termination": "won" if won else "step_limit", "actionCount": 1,
                "actions": [{"kind": "tap"}], "physicalBlocks": 0})
    return report


class SummaryContractTests(unittest.TestCase):
    def test_population_needs_complete_profile_task_coverage(self):
        report = fixture()
        report["config"] = {"pairs": [{"referencePairId": 1}, {"referencePairId": 2}]}
        report["candidatePackSha256"] = "test-candidates"
        report["complete"] = False
        self.assertFalse(summarize(report, requested_users=1, bootstrap_samples=20)["requestedPopulationRun"])
        report["complete"] = True
        self.assertTrue(summarize(report, requested_users=1, bootstrap_samples=20)["requestedPopulationRun"])
        report["sessions"].pop()
        self.assertFalse(summarize(report, requested_users=1, bootstrap_samples=20)["requestedPopulationRun"])

    def test_sessions_do_not_masquerade_as_population(self):
        result = summarize(fixture(), bootstrap_samples=20)
        self.assertEqual(result["actualSessions"], 4)
        self.assertEqual(result["actualProfilesWithSessions"], 1)
        self.assertFalse(result["requestedPopulationRun"])
        self.assertEqual(result["paired"]["pairedUsers"], 1)
        self.assertEqual(result["paired"]["userWeightedPairedDifferences"]["winRate"]["ci95"], [-1., -1.])

    def test_seed_mismatch_rejects_false_pairing(self):
        report = fixture()
        report["sessions"][1]["sessionSeed"] = 99
        with self.assertRaisesRegex(ValueError, "session seed"):
            summarize(report)

    def test_censored_failures_stay_in_denominator(self):
        result = summarize(fixture(), bootstrap_samples=20)
        self.assertEqual(result["allSessions"]["winRateOfAllSessions"], .5)
        self.assertEqual(result["allSessions"]["budgetCensoredSessions"], 2)

    def test_incomplete_and_engine_error_pairs_stay_visible(self):
        report = fixture()
        report["sessions"].pop()
        report["sessions"][1]["termination"] = "engine_error"
        result = summarize(report, bootstrap_samples=20)
        self.assertEqual(result["paired"]["engineErrorPairs"], 1)
        self.assertEqual(result["paired"]["incompletePairs"], 1)
        self.assertEqual(result["allSessions"]["engineErrorSessions"], 1)

    def test_missing_or_stale_contract_counts_reject(self):
        report = fixture()
        report["sessions"][0]["actionCount"] = 3
        with self.assertRaisesRegex(ValueError, "Action count"):
            summarize(report)
        report = fixture()
        report["rulesHash"] = ""
        with self.assertRaisesRegex(ValueError, "hash"):
            summarize(report)


if __name__ == "__main__":
    unittest.main()

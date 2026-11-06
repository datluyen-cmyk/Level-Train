"""Summarize real native virtual-player episodes; never fabricate sessions.

Paired confidence intervals resample virtual users, keeping that user's whole
level portfolio together. They describe bot Monte Carlo uncertainty only.
"""
from collections import Counter, defaultdict
from pathlib import Path
import argparse
import hashlib
import json
import math

import numpy as np


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def describe(rows):
    if not rows:
        return {"sessions": 0}
    good = [s for s in rows if s["termination"] != "engine_error"]
    won = [s for s in good if s["won"]]
    def quantiles(selected, key):
        return dict(zip(("p10", "p50", "p90"), map(float, np.quantile(
            [s.get(key, 0) for s in selected], [.1, .5, .9])))) if selected else None
    return {"sessions": len(rows), "engineErrorSessions": len(rows)-len(good),
        "actualProfileCount": len({s["profileId"] for s in rows}),
        "won": len(won), "winRateOfAllSessions": len(won)/len(rows),
        "winRateExcludingEngineErrors": len(won)/len(good) if good else None,
        "terminationCounts": dict(Counter(s["termination"] for s in rows)),
        "actionCountAllSessionQuantiles": quantiles(rows, "actionCount"),
        "actionCountSuccessfulSessionQuantiles": quantiles(won, "actionCount"),
        "wallMsAllSessions": sum(s.get("elapsedWallMs", 0) for s in rows),
        "physicalBlocks": sum(s.get("physicalBlocks", 0) for s in rows),
        "invalidActions": sum(s.get("invalidActions", 0) for s in rows),
        "hints": sum(s.get("hints", 0) for s in rows),
        "undoCount": sum(s.get("undoCount", 0) for s in rows),
        "cacheHits": sum(s.get("cacheHits", 0) for s in rows),
        "cacheMisses": sum(s.get("cacheMisses", 0) for s in rows),
        "budgetCensoredSessions": sum(s["termination"] in ("step_limit", "wall_time", "invalid_action_limit") for s in rows)}


def validate(report):
    if report.get("schemaVersion") != 5:
        raise ValueError("Unsupported native playtest schema")
    for key in ("rulesHash", "policySuiteHash", "inputPackSha256", "templatesSha256"):
        if not report.get(key):
            raise ValueError("Missing native playtest hash: "+key)
    profiles = {p["profileId"]: p for p in report["profiles"]}
    if len(profiles) != len(report["profiles"]):
        raise ValueError("Duplicate virtual profile IDs")
    config = report.get("config", {})
    if "users" in config and int(config["users"]) != len(profiles):
        raise ValueError("Defined profile count disagrees with configured virtual users")
    if "seed" in config:
        for profile_id, profile in profiles.items():
            suffix = profile_id.removeprefix("virtual-")
            if suffix.isdigit() and "seed" in profile:
                index = int(suffix)
                expected_profile_seed = (int(config["seed"]) + index*7919) & 0xFFFFFFFF
                if expected_profile_seed >= 0x80000000:
                    expected_profile_seed -= 0x100000000
                if int(profile["seed"]) != expected_profile_seed:
                    raise ValueError("Profile seed does not match configured seed/index")
    seen = set()
    pair_by_id = {}
    for pair in config.get("pairs", []):
        pair_id = str(pair.get("referencePairId", ""))
        if not pair_id or pair_id in pair_by_id:
            raise ValueError("Missing or duplicate configured reference pair ID")
        pair_by_id[pair_id] = pair
    for row in report["sessions"]:
        if row["sessionId"] in seen:
            raise ValueError("Duplicate virtual session ID")
        seen.add(row["sessionId"])
        if row["profileId"] not in profiles or row["policyFamily"] != profiles[row["profileId"]]["policyFamily"]:
            raise ValueError("Unknown/mismatched profile")
        if row["role"] not in ("reference", "candidate") or not row.get("levelSha256"):
            raise ValueError("Missing native level hash or invalid role")
        if row["won"] != (row["termination"] == "won"):
            raise ValueError("Contradictory episode outcome")
        if row["termination"] not in ("won", "step_limit", "wall_time", "no_available_action", "invalid_action_limit", "engine_error", "dropout"):
            raise ValueError("Unknown termination")
        if row["actionCount"] != len(row["actions"]):
            raise ValueError("Action count disagrees with retained episode trace")
        if any(not isinstance(row.get(key, 0), (int, float)) or not math.isfinite(row.get(key, 0)) or row.get(key, 0) < 0
               for key in ("actionCount", "physicalBlocks", "invalidActions", "hints", "undoCount", "elapsedWallMs", "cacheHits", "cacheMisses")):
            raise ValueError("Invalid native session counters")
        if pair_by_id:
            pair = pair_by_id.get(str(row["referencePairId"]))
            if pair is None:
                raise ValueError("Session references an unconfigured level pair")
            expected_level = pair.get("referenceId") if row["role"] == "reference" else pair.get("candidateId")
            if expected_level is not None and str(row.get("levelId")) != str(expected_level):
                raise ValueError("Session level ID does not match configured pair role")
            profile = profiles[row["profileId"]]
            if "seed" in profile and pair.get("referenceId") is not None:
                # Mirror unchecked C# Int32 arithmetic, including overflow in
                # referenceId * 48611, before checking the recorded paired seed.
                mask = 0xFFFFFFFF
                expected_seed = ((int(profile["seed"]) & mask) ^
                                 ((int(pair["referenceId"]) * 48611) & mask)) & mask
                if expected_seed >= 0x80000000:
                    expected_seed -= 0x100000000
                if int(row["sessionSeed"]) != expected_seed:
                    raise ValueError("Session seed does not match profile/reference pair seed")

    if "completedSessions" in report or "totalSessions" in report:
        if "completedSessions" not in report or "totalSessions" not in report:
            raise ValueError("Native session counters are incomplete")
        if int(report["completedSessions"]) != len(report["sessions"]):
            raise ValueError("Completed session counter disagrees with retained session rows")
        if pair_by_id:
            role_count = 2 if report.get("candidatePackSha256") else 1
            expected_total = len(profiles) * len(pair_by_id) * role_count
            if int(report["totalSessions"]) != expected_total:
                raise ValueError("Total session counter disagrees with configured profile tasks")
    return profiles


def paired(rows, bootstrap_samples, seed):
    pair_groups = defaultdict(dict)
    for s in rows:
        key = (s["profileId"], s.get("cohortId"), s["referencePairId"])
        if s["role"] in pair_groups[key]:
            raise ValueError("Duplicate role in user/level pair")
        pair_groups[key][s["role"]] = s
    by_user, incomplete, errors, details = defaultdict(list), 0, 0, []
    for key, group in pair_groups.items():
        if set(group) != {"reference", "candidate"}:
            incomplete += 1
            continue
        a, b = group["reference"], group["candidate"]
        if a["sessionSeed"] != b["sessionSeed"]:
            raise ValueError("Paired episodes do not share a session seed")
        if "engine_error" in (a["termination"], b["termination"]):
            errors += 1
            continue
        delta = [float(b["won"])-float(a["won"]), b["actionCount"]-a["actionCount"],
            b.get("physicalBlocks", 0)-a.get("physicalBlocks", 0)]
        by_user[key[0]].append(delta)
        details.append({"profileId": key[0], "cohortId": key[1], "referencePairId": key[2],
            "referenceWon": a["won"], "candidateWon": b["won"],
            "candidateMinusReference": delta})
    result = {"completePairs": len(details), "incompletePairs": incomplete,
        "engineErrorPairs": errors, "pairedUsers": len(by_user),
        "differenceDirection": "candidate minus reference", "pairDetails": details}
    if by_user:
        user_means = np.asarray([np.mean(v, axis=0) for v in by_user.values()])
        rng = np.random.default_rng(seed)
        bootstrap = np.array([user_means[rng.integers(len(user_means), size=len(user_means))].mean(axis=0)
            for _ in range(bootstrap_samples)])
        result["userWeightedPairedDifferences"] = {
            name: {"mean": float(user_means[:, i].mean()),
                "ci95": np.quantile(bootstrap[:, i], [.025, .975]).tolist()}
            for i, name in enumerate(("winRate", "allSessionActionCount", "physicalBlocks"))}
        result["intervalMeaning"] = "Monte Carlo uncertainty under these bot policies; shared policy biases and bot-to-human gap are not covered"
    return result


def summarize(report, requested_users=1000, bootstrap_samples=1000, seed=20261106):
    profiles = validate(report)
    sessions = report["sessions"]
    actual_users = {s["profileId"] for s in sessions}
    grouped = defaultdict(list)
    for s in sessions:
        grouped[(s["policyFamily"], s["role"])].append(s)
    per_level = defaultdict(list)
    for s in sessions:
        per_level[(str(s["levelId"]), s["role"], str(s["referencePairId"]))].append(s)
    config = report.get("config", {})
    expected_roles = ("reference", "candidate") if report.get("candidatePackSha256") else ("reference",)
    expected_pairs = {(str(p["referencePairId"]), role) for p in config.get("pairs", []) for role in expected_roles}
    observed_coverage = defaultdict(set)
    for s in sessions:
        observed_coverage[s["profileId"]].add((str(s["referencePairId"]), s["role"]))
    coverage_complete = bool(expected_pairs) and all(observed_coverage[p] == expected_pairs for p in profiles)
    expected_session_count = len(profiles)*len(expected_pairs)
    attempted_coverage = bool(expected_pairs) and coverage_complete and len(sessions) == expected_session_count
    counter_consistency = True
    if "completedSessions" in report or "totalSessions" in report:
        counter_consistency = ("completedSessions" in report and "totalSessions" in report and
            int(report.get("completedSessions", -1)) == len(sessions) and
            int(report.get("totalSessions", -1)) == expected_session_count)
    engine_error_sessions = sum(s["termination"] == "engine_error" for s in sessions)
    run_complete = (report.get("complete") is True and attempted_coverage and counter_consistency
                    and engine_error_sessions == 0)
    partitions = defaultdict(list)
    family_rows = defaultdict(list)
    for s in sessions:
        partitions[profiles[s["profileId"]].get("partition", "unspecified")].append(s)
        family_rows[s["policyFamily"]].append(s)
    return {"schemaVersion": 5, "status": "NATIVE_VIRTUAL_PLAYER_EPISODE_SUMMARY",
        "requestedVirtualUsers": requested_users, "definedProfiles": len(profiles),
        "actualProfilesWithSessions": len(actual_users), "actualSessions": len(sessions),
        "profileCountReached": len(actual_users) >= requested_users,
        "nativeRunComplete": report.get("complete") is True,
        "allDefinedProfileTaskCoverageComplete": coverage_complete,
        "attemptedCoverage": attempted_coverage,
        "counterConsistency": counter_consistency,
        "engineErrorSessions": engine_error_sessions,
        "requestedPopulationRun": len(actual_users) >= requested_users and run_complete,
        "populationMeaning": "seeded algorithmic player profiles, not humans or independently trained LLMs",
        "policyFamilyCounts": dict(Counter(profiles[p]["policyFamily"] for p in actual_users)),
        "allSessions": describe(sessions),
        "byPolicyFamilyAndRole": [{"policyFamily": key[0], "role": key[1], **describe(rows)} for key, rows in sorted(grouped.items())],
        "byLevel": [{"levelId": key[0], "role": key[1], "referencePairId": key[2], **describe(rows)} for key, rows in sorted(per_level.items())],
        "paired": paired(sessions, bootstrap_samples, seed),
        "byPartition": {key: {"allSessions": describe(rows), "paired": paired(rows, bootstrap_samples, seed)}
            for key, rows in partitions.items()},
        "pairedByPolicyFamily": {key: paired(rows, bootstrap_samples, seed) for key, rows in family_rows.items()},
        "bootstrap": {"samples": bootstrap_samples, "seed": seed, "resamplingUnit": "virtual user; all levels kept together"},
        "hashes": {key: report[key] for key in ("rulesHash", "policySuiteHash", "inputPackSha256", "templatesSha256")},
        "limits": ["No bounded policy failure is classified as proof of unsolvability.",
            "Observed simulation wall time is not measured or modeled human completion time.",
            "All-session action counts are censored by budgets; successful-session distributions are reported separately.",
            "No source/candidate equality, human difficulty or generation acceptance follows solely from these intervals."]}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--input", type=Path, required=True)
    parser.add_argument("--output-dir", type=Path, required=True)
    parser.add_argument("--requested-users", type=int, default=1000)
    parser.add_argument("--bootstrap-samples", type=int, default=1000)
    parser.add_argument("--seed", type=int, default=20261106)
    args = parser.parse_args()
    report = json.loads(args.input.read_text(encoding="utf8"))
    result = summarize(report, args.requested_users, args.bootstrap_samples, args.seed)
    result["hashes"].update(nativeEpisodeReport=sha(args.input), summarizer=sha(Path(__file__)))
    args.output_dir.mkdir(parents=True, exist_ok=False)
    (args.output_dir/"summary.json").write_text(json.dumps(result, indent=2), encoding="utf8")
    print(json.dumps({key: result[key] for key in ("actualProfilesWithSessions", "actualSessions", "requestedPopulationRun")}))


if __name__ == "__main__":
    main()

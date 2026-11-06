"""Hash-bound paired bounded computer-solver evidence; never human difficulty."""
from pathlib import Path
import argparse
import hashlib
import json
import math
import re
import struct


INPUT_NAMES = ["source_parity_pack.json", "source_templates.json"]


def sha(raw):
    return hashlib.sha256(raw).hexdigest()


def binary_string(text):
    raw = (text or "").encode("utf8")
    size, prefix = len(raw), bytearray()
    while size >= 128:
        prefix.append((size&127)|128)
        size >>= 7
    prefix.append(size)
    return bytes(prefix)+raw


def hash_bundle(names, contents, unity_version, fixed_dt):
    """C# BinaryWriter:7-bit UTF8 strings,float32,int32 length,raw bytes."""
    if len(names) != len(contents):
        raise ValueError("Bundle lengths differ")
    blob = bytearray(binary_string(unity_version))
    blob.extend(struct.pack("<f", fixed_dt))
    for name, raw in zip(names, contents):
        blob.extend(binary_string(name))
        blob.extend(struct.pack("<i", len(raw)))
        blob.extend(raw)
    return sha(blob)


def rule_names(rules_root):
    source = (rules_root/"RotateRings/Editor/SourceSolverV3.cs").read_text(encoding="utf8")
    match = re.search(r'static string HashRuleBundle\(\).*?string\[\] relativePaths\s*=\s*\{(.*?)\};', source, re.S)
    if not match:
        raise ValueError("Cannot inspect SourceSolverV3.HashRuleBundle names")
    names = re.findall(r'"([^"]+)"', match.group(1))
    if not names or len(names) != len(set(names)):
        raise ValueError("Invalid rule bundle names")
    return names


def pack_metadata(raw):
    def compact(value):
        if "templateId" in value and "id" in value:
            return {"id": value["id"]}
        if "parts" in value and "position" in value:
            return {"id": value["id"], "position": value["position"], "partIds": [p["id"] for p in value["parts"]]}
        return value
    levels = json.loads(raw, object_hook=compact)["levels"]
    positions = [l["position"] for l in levels]
    if len(positions) != len(set(positions)):
        raise ValueError("Duplicate positions in input pack")
    return {l["position"]: l for l in levels}


def unique_rows(report):
    out = {}
    for row in report.get("levels", []):
        position = row["sourcePosition"]
        if position in out:
            raise ValueError("Duplicate report sourcePosition: "+str(position))
        out[position] = row
    return out


def nonnegative(value):
    return value if isinstance(value, (int, float)) and math.isfinite(value) and value >= 0 else None


def independent_replay_summary(actions, explicit_steps=None):
    steps = actions if explicit_steps is None else explicit_steps
    if len(steps) != len(actions) or not all(s.get("replayWitnessCaptured") is True for s in steps):
        return None
    angular, linear, reversals = 0., 0., 0
    last_sign, captured = {}, []
    previous_removed = None
    previous_bindings = None
    for action, step in zip(actions, steps):
        if step.get("part") != action.get("part") or step.get("kind") != action.get("kind"):
            raise ValueError("Independent replay step identity differs from solver path")
        fields = ("actualAngleBeforeDegrees", "actualAngleAfterDegrees", "actualSlideBefore", "actualSlideAfter", "actualMovementConsumed")
        if any(not isinstance(step.get(k), (int, float)) or not math.isfinite(step[k]) for k in fields):
            raise ValueError("Independent replay movement missing/nonfinite")
        kind = action["kind"]
        if kind not in ("rotate", "slide"):
            raise ValueError("Unsupported independent replay action kind")
        expected = (step["actualAngleAfterDegrees"]-step["actualAngleBeforeDegrees"] if kind == "rotate"
                    else step["actualSlideAfter"]-step["actualSlideBefore"])
        movement = step["actualMovementConsumed"]
        if not math.isclose(movement, expected, rel_tol=1e-5, abs_tol=1e-4):
            raise ValueError("Independent replay movement does not match reached before/after poses")
        before, after, delta = (set(step.get(k) or []) for k in ("removedIdsBefore", "removedIdsAfter", "removedIdsDelta"))
        if not before.issubset(after) or after-before != delta:
            raise ValueError("Independent replay removed-ID delta mismatch")
        binding_before, binding_after = step.get("activeBindingCountBefore"), step.get("activeBindingCountAfter")
        if nonnegative(binding_before) is None or nonnegative(binding_after) is None:
            raise ValueError("Independent replay active binding counts missing")
        if previous_removed is not None and (before != previous_removed or binding_before != previous_bindings):
            raise ValueError("Independent replay state discontinuity between path steps")
        previous_removed, previous_bindings = after, binding_after
        if kind == "rotate":
            angular += abs(movement)
        else:
            linear += abs(movement)
        key = (step["part"], kind)
        if movement:
            sign = 1 if movement > 0 else -1
            reversals += key in last_sign and sign != last_sign[key]
            last_sign[key] = sign
        captured.append({k: step.get(k) for k in ("kind", "part", *fields, "replayBlocked", "replayBlockerOwner", "replayBlockerShapeIndex", "replayBlockerRole",
                         "activeBindingCountBefore", "activeBindingCountAfter", "removedIdsBefore", "removedIdsAfter", "removedIdsDelta")})
    return {"capturedSegmentCount": len(steps), "actualAngularTravelMagnitudeDegrees": angular, "actualSlideTravelMagnitudeNativeUnits": linear,
            "perRootActualMovementSignReversals": reversals, "physicalReplayBlockedSegmentCount": sum(bool(s.get("replayBlocked")) for s in steps),
            "bindingChangingSegmentCount": sum(s["activeBindingCountBefore"] != s["activeBindingCountAfter"] for s in steps),
            "bindingCountDecreaseSum": sum(max(0, s["activeBindingCountBefore"]-s["activeBindingCountAfter"]) for s in steps),
            "bindingCountIncreaseSum": sum(max(0, s["activeBindingCountAfter"]-s["activeBindingCountBefore"]) for s in steps),
            "removalSegmentCount": sum(bool(s.get("removedIdsDelta")) for s in steps), "removedRootCountFromDeltas": sum(len(s.get("removedIdsDelta") or []) for s in steps),
            "steps": captured, "semantics": "Reached production replay movement and physical CollisionWorld blocked flags; independent from requested intent and legacy bomb-inclusive blocked flag"}


def row_summary(row):
    if row is None:
        return {"status": "missing", "solvedPath": None, "independentReplay": None, "censored": True, "reason": "No reported row", "boundedSearch": None}
    solved = (row.get("status") == "solved" and row.get("goal") is True and row.get("replayGoal") is True
              and nonnegative(row.get("moveCount")) is not None and not row.get("validationError") and not row.get("error"))
    actions = row.get("actions", [])
    if solved and len(actions) != row["moveCount"]:
        raise ValueError("Solved action count differs from moveCount at "+str(row["sourcePosition"]))
    counters = {key: nonnegative(row.get(key)) for key in ("exploredStates", "generatedActions", "replayedActions", "searchElapsedMs", "maximumFrontier",
                "attemptedActions", "appliedActions", "noOpActions", "rejectedInputActions", "bindingChangingTransitions", "maximumRemovedParts", "bestRemainingRootCount", "bestActiveBindings")}
    trace = row.get("actionTrace", [])
    counters.update({"actionTraceLoggedEntries": len(trace), "actionTraceObservedBlockedTrueEntries": sum("blocked=True" in t for t in trace),
                     "actionTraceTruncatedOrIncomplete": counters["attemptedActions"] is None or len(trace) < counters["attemptedActions"]})
    result = {"status": row.get("status"), "sourceId": row.get("sourceId"), "partCount": row.get("partCount"),
              "censored": not solved, "reason": None if solved else "No complete replayed solve witness under this finite search budget; path metrics missing/censored",
              "reportedMoveCountRaw": row.get("moveCount"), "solvedPath": None, "independentReplay": None, "boundedSearch": counters}
    if not solved:
        return result
    roots = [a["part"] for a in actions]
    rotation = [a for a in actions if a["kind"] == "rotate"]
    last_sign, per_root_reversals = {}, 0
    consecutive_reversals = 0
    for i, a in enumerate(actions):
        if a["kind"] != "rotate" or not a.get("delta"):
            continue
        sign = 1 if a["delta"] > 0 else -1
        if a["part"] in last_sign and last_sign[a["part"]] != sign:
            per_root_reversals += 1
        last_sign[a["part"]] = sign
        if i and actions[i-1]["kind"] == "rotate" and actions[i-1]["part"] == a["part"] and actions[i-1].get("delta", 0)*a["delta"] < 0:
            consecutive_reversals += 1
    result["solvedPath"] = {"boundedSolverSegments": row["moveCount"], "distinctActedRootCount": len(set(roots)), "distinctActedRootIds": sorted(set(roots)),
        "partSwitches": sum(a != b for a, b in zip(roots, roots[1:])), "perRootRequestedRotationSignReversals": per_root_reversals,
        "consecutiveSameRootRequestedRotationReversals": consecutive_reversals,
        "requestedAngularDeltaMagnitudeDegrees": sum(abs(a["delta"]) for a in rotation), "rotationSegmentCount": len(rotation),
        "slideSegmentCount": sum(a["kind"] == "slide" for a in actions),
        "reportedBlockedOrBombEventSegmentCount": sum(bool(a.get("blocked")) for a in actions),
        "reportedNonnegativeBlockerOwnerSegmentCount": sum(a.get("blockerOwner", -1) >= 0 for a in actions),
        "guidedSegmentCount": sum(bool(a.get("guided")) for a in actions), "quickReleaseSegmentCount": sum(bool(a.get("quickRelease")) for a in actions)}
    result["independentReplay"] = independent_replay_summary(actions, row.get("replaySteps"))
    return result


def compare_reports(source, candidate, source_pack, candidate_pack, source_input_hash, candidate_input_hash, current_rules_hash):
    errors, warnings = [], []
    for name, report, pack, computed in (("source", source, source_pack, source_input_hash), ("candidate", candidate, candidate_pack, candidate_input_hash)):
        if report.get("inputHash") != computed:
            errors.append(name+":stale_or_mismatched_input_hash")
        if report.get("rulesHash") != current_rules_hash:
            warnings.append(name+":historical_rules_do_not_match_current_bundle")
        if not re.fullmatch(r"[0-9a-f]{64}", report.get("rulesHash", "")):
            errors.append(name+":missing_or_invalid_reported_rules_hash")
        if any(nonnegative(report.get(key)) is None or report.get(key) <= 0 for key in ("maxStates", "wallTimeBudgetMs")):
            errors.append(name+":missing_or_invalid_search_budget")
        rows = unique_rows(report)
        if report.get("completedLevels") != len(rows):
            errors.append(name+":completed_row_count_mismatch")
        for position, row in rows.items():
            metadata = pack.get(position)
            if metadata is None or metadata["id"] != row.get("sourceId") or len(metadata["partIds"]) != row.get("partCount"):
                errors.append(name+":row_input_identity_mismatch:"+str(position))
            elif any(a.get("part") not in metadata["partIds"] for a in row.get("actions", [])):
                errors.append(name+":action_root_missing_from_input:"+str(position))
    for key in ("rulesHash", "unityVersion", "fixedDeltaTime", "maxStates", "wallTimeBudgetMs", "policyId", "defaultMaxStates", "defaultWallTimeBudgetMs", "moveCountSemantics", "actionMovementSemantics"):
        if source.get(key) != candidate.get(key):
            errors.append("pair:mismatched_"+key)
    sr, cr = unique_rows(source), unique_rows(candidate)
    rows = []
    for position in sorted(set(sr)|set(cr)):
        a, b = row_summary(sr.get(position)), row_summary(cr.get(position))
        comparable = not errors and not a["censored"] and not b["censored"]
        ratios, reason = {}, None
        if not comparable:
            reason = "Hash/rules/budget pairing invalid" if errors else "At least one solved path is missing/censored"
        for key in ("boundedSolverSegments", "distinctActedRootCount", "partSwitches", "requestedAngularDeltaMagnitudeDegrees"):
            av, bv = (a["solvedPath"] or {}).get(key), (b["solvedPath"] or {}).get(key)
            ratios[key] = bv/av if comparable and av is not None and av > 0 and bv is not None else None
        for key in ("exploredStates", "generatedActions", "attemptedActions", "appliedActions", "searchElapsedMs"):
            av, bv = (a["boundedSearch"] or {}).get(key), (b["boundedSearch"] or {}).get(key)
            ratios[key] = bv/av if comparable and av is not None and av > 0 and bv is not None else None
        actual_comparable = comparable and a["independentReplay"] is not None and b["independentReplay"] is not None
        actual_ratios = {}
        for key in ("actualAngularTravelMagnitudeDegrees", "actualSlideTravelMagnitudeNativeUnits", "physicalReplayBlockedSegmentCount", "perRootActualMovementSignReversals", "bindingChangingSegmentCount"):
            av, bv = (a["independentReplay"] or {}).get(key), (b["independentReplay"] or {}).get(key)
            actual_ratios[key] = bv/av if actual_comparable and av is not None and av > 0 and bv is not None else None
        rows.append({"position": position, "source": a, "candidate": b, "pairedCompleteSolveWitnesses": comparable,
                     "candidateOverSourceRatios": ratios, "ratioMissingReason": reason,
                     "candidateOverSourceActualReplayRatios": actual_ratios,
                     "actualReplayRatioMissingReason": None if actual_comparable else "Independent reached-movement replay evidence is missing/censored or pair validation failed",
                     "difficultyEquivalence": "UNESTABLISHED", "accepted": False})
    current_match = source.get("rulesHash") == candidate.get("rulesHash") == current_rules_hash
    status = ("CURRENT_RULES_PAIRED_BOUNDED_COMPUTER_SOLVER_OBSERVATIONS" if current_match else "HISTORICAL_SAME_REPORTED_RULES_PAIRED_OBSERVATIONS") if not errors else "UNPAIRED_EVIDENCE"
    return {"schemaVersion": 4, "status": status,
            "inputAndReportedRulesBudgetPairingValid": not errors, "currentRuleMatch": current_match,
            "historicalRulesSnapshotByteVerified": current_match,
            "validationErrors": errors, "warnings": warnings, "levels": rows,
            "budgets": {"source": {k: source.get(k) for k in ("maxStates", "wallTimeBudgetMs")}, "candidate": {k: candidate.get(k) for k in ("maxStates", "wallTimeBudgetMs")}},
            "reportedRulesHash": {"source": source.get("rulesHash"), "candidate": candidate.get("rulesHash")}, "verifiedRulesHash": current_rules_hash,
            "solverPolicy": {side: {k: report.get(k) for k in ("policyId", "defaultMaxStates", "defaultWallTimeBudgetMs", "moveCountSemantics", "actionMovementSemantics")} for side, report in (("source", source), ("candidate", candidate))},
            "acceptedGeneratedCount": 0, "numericContentGatesApplied": [],
            "limits": ["Segments are bounded computer-solver action witnesses, not human drags, minimal moves, human difficulty or acceptance.",
                       "Equal segment counts do not establish equal difficulty; acted roots, switches, reversals, blocking/order and mechanic constraints also matter.",
                       "Unknown/missing paths remain null and censored. Raw-1 moveCount is never treated as zero; bounded search failure is not unsolvability.",
                       "Requested angular delta magnitude is requested motion, not measured travel when contact or quick release truncates it.",
                       "Solver action.blocked includes physical blocking or spent-bomb events; trace observations are bounded search logs, not complete selected-path contact telemetry.",
                       "Effort ratios are descriptive under identical budgets and verified rules/inputs; timing depends on machine/editor state and does not measure player effort.",
                       "Historical same-rules comparison requires matching reported rules hashes and verified input bundles/budgets. If old rule bytes are unavailable, historicalRulesSnapshotByteVerified is false; it is not current-runtime certification.",
                       "Zero source denominators yield null ratios. No fixed design cutoff or difficulty-equivalence claim is applied."]}


def summarize(source_path, candidate_path, rules_root):
    source_raw, candidate_raw = source_path.read_bytes(), candidate_path.read_bytes()
    source, candidate = json.loads(source_raw), json.loads(candidate_raw)
    cache = {}
    def contents(path):
        path = Path(path).resolve()
        if path not in cache:
            cache[path] = path.read_bytes()
        return cache[path]
    inputs, metadata = [], []
    for report in (source, candidate):
        raws = [contents(report["packPath"]), contents(report["templatesPath"])]
        inputs.append(hash_bundle(INPUT_NAMES, raws, report["unityVersion"], report["fixedDeltaTime"]))
        metadata.append(pack_metadata(raws[0]))
    names = rule_names(rules_root)
    raw_rules = [contents(rules_root/name) for name in names]
    rule_hash = hash_bundle(names, raw_rules, source["unityVersion"], source["fixedDeltaTime"])
    result = compare_reports(source, candidate, *metadata, *inputs, rule_hash)
    result["hashes"] = {"sourceReport": sha(source_raw), "candidateReport": sha(candidate_raw),
                        "sourceInputBundle": inputs[0], "candidateInputBundle": inputs[1], "rulesBundle": rule_hash,
                        "tool": sha(Path(__file__).read_bytes()), "boundFiles": {str(path): sha(raw) for path, raw in cache.items()}}
    result["reportPaths"] = {"source": str(source_path.resolve()), "candidate": str(candidate_path.resolve())}
    return result


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--source-report", required=True, type=Path)
    parser.add_argument("--candidate-report", required=True, type=Path)
    parser.add_argument("--rules-root", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    args = parser.parse_args()
    result = summarize(args.source_report, args.candidate_report, args.rules_root)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, indent=2), encoding="utf8")
    print("PAIRED_NATIVE_EVIDENCE", result["inputAndReportedRulesBudgetPairingValid"], result["currentRuleMatch"], len(result["levels"]), result["validationErrors"], flush=True)


if __name__ == "__main__":
    main()

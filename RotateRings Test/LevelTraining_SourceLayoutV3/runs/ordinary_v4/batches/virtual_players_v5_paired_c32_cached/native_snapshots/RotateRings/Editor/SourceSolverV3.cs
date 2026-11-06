#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

// Bounded source-physics search/replay. Outcomes are observations from the production
// CollisionWorld and BombContacts components; a failed bounded search is never proof
// that a source level is impossible.
public static partial class SourceSolverV3
{
    const string CurrentPolicyId = "source-solver-v3-guided-priority-bounded-search-v1";
    const string DiversityPolicyId = "source-solver-v3-endpoint-diversity-diagnostic-v1";
    [Serializable] sealed class SolverRequest
    {
        public int schemaVersion = 3;
        public string sourcePackPath, templatesPath;
        public int[] sourceIds;
        public int maxStates = 256;
        public int wallTimeBudgetMs = 1000;
        public bool full500;
        public int diversityBucketCapacity = 2;
    }
    [Serializable] sealed class SolverReport
    {
        public int schemaVersion = 3;
        public string status, semantics = "production Physics2D collision/bomb components; bounded action search; solved rows are action witnesses, not optimality or human-difficulty claims; failed search is unknown";
        public string policyId = CurrentPolicyId;
        public int defaultMaxStates = 256;
        public int defaultWallTimeBudgetMs = 1000;
        public string moveCountSemantics = "Count of solver ApplyAction segments in the witness path; not a human drag count or level par.";
        public string actionMovementSemantics = "Requested delta/targetSlide are intent; actual replay fields record reached angle/slide after collision clamping.";
        public string packPath, templatesPath;
        public int maxStates;
        public int wallTimeBudgetMs;
        public bool complete;
        public int completedLevels, totalLevels;
        public string inputHash, rulesHash, unityVersion;
        public float fixedDeltaTime;
        public List<LevelResult> levels = new List<LevelResult>();
    }
    [Serializable] sealed class LevelResult
    {
        public int sourceId, sourcePosition, partCount, exploredStates, generatedActions, replayedActions, moveCount = -1;
        public int maximumRemovedParts, bestRemainingRootCount, bestActiveBindings, maximumFrontier;
        public long searchElapsedMs;
        public string status, proof, error;
        public string validationError, bestStateSummary;
        public bool goal, replayGoal;
        public List<SolverAction> actions = new List<SolverAction>();
        public List<int> finalRemoved = new List<int>();
        public List<int> bestRemovedParts = new List<int>();
        public int bindingChangingTransitions;
        public int diversitySignaturesSeen, diversitySignatureDrops, endpointBucketsSeen;
        public int diversityCapacity = 2;
        public string stopReason;
        public int attemptedActions, appliedActions, noOpActions, rejectedInputActions;
        public List<string> transitionTrace = new List<string>();
        public List<string> actionTrace = new List<string>();
    }
    [Serializable] sealed class SolverAction
    {
        public string kind;
        public int part;
        public float delta, targetSlide;
        public bool blocked;
        public bool guided;
        public int blockerOwner = -1;
        public bool quickRelease;
        public bool replayWitnessCaptured, replayBlocked;
        public float actualAngleBeforeDegrees, actualAngleAfterDegrees;
        public float actualSlideBefore, actualSlideAfter, actualMovementConsumed;
        public int activeBindingCountBefore, activeBindingCountAfter;
        public int[] removedIdsBefore, removedIdsAfter, removedIdsDelta;
        public int replayBlockerOwner = -1, replayBlockerShapeIndex = -1;
        public string replayBlockerRole;
    }

    sealed class SearchState
    {
        public readonly Dictionary<int, float> angles = new Dictionary<int, float>();
        public readonly Dictionary<int, float> slides = new Dictionary<int, float>();
        public readonly HashSet<int> removed = new HashSet<int>();
        public readonly HashSet<int> keys = new HashSet<int>();
        public readonly Dictionary<int, int> chainLocks = new Dictionary<int, int>();
        public readonly HashSet<int> spentBombs = new HashSet<int>();
        public object bindings;
        public SearchState Copy()
        {
            var copy = new SearchState { bindings = CloneBindingSnapshot(bindings) };
            foreach (var p in angles) copy.angles.Add(p.Key, p.Value);
            foreach (var p in slides) copy.slides.Add(p.Key, p.Value);
            foreach (int id in removed) copy.removed.Add(id);
            foreach (int id in keys) copy.keys.Add(id);
            foreach (var p in chainLocks) copy.chainLocks.Add(p.Key, p.Value);
            foreach (int id in spentBombs) copy.spentBombs.Add(id);
            return copy;
        }
    }
    sealed class SearchNode
    {
        public SearchState state;
        public SearchNode parent;
        public SolverAction action;
        public bool guided;
        public bool prioritizedBranch;
        public bool counted;
        public List<SolverAction> candidateActions;
        public int nextCandidate;
    }

    sealed class AsyncBatchJob
    {
        public string responsePath;
        public RotateRingsTemplateCatalog templates;
        public List<RotateRingsLevel> levels;
        public SolverReport report;
        public int nextLevel;
        public bool diversityPolicy;
        public int diversityBucketCapacity = 2;
    }
    [Serializable] sealed class SliderProbeReport
    {
        public int sourceId;
        public List<SliderProbeResult> sliders = new List<SliderProbeResult>();
    }
    [Serializable] sealed class SliderProbeResult
    {
        public int partId;
        public float startSlide, positiveLimit, negativeLimit;
        public int positiveBlockerOwner = -1, negativeBlockerOwner = -1;
        public string positiveBlockerRole, negativeBlockerRole;
        public bool positiveBlocked, negativeBlocked;
        public bool canSolveAtPositiveLimit, canSolveAtNegativeLimit;
        public int activeBindingsAtPositiveLimit, activeBindingsAtNegativeLimit;
    }
    [Serializable] sealed class SliderRingProbeReport
    {
        public int sourceId, sliderId;
        public float sliderPosition;
        public List<RingReleaseProbe> rings = new List<RingReleaseProbe>();
    }
    [Serializable] sealed class SliderFrameBudgetRegressionReport
    {
        public int sourceId, sliderId;
        public float targetSlide;
        public float oneFrameReached, repeatedFramesReached, actionReached;
        public bool oneFrameBlocked, repeatedFramesBlocked, actionBlocked;
        public int oneFrameBlockerOwner = -1, repeatedFramesBlockerOwner = -1;
        public bool releasedAfterRepeatedFrames, actionRemovedSlider;
        public int activeBindingsAfterRepeatedFrames;
        public List<int> removedAfterAction = new List<int>();
    }
    [Serializable] sealed class RingReleaseProbe
    {
        public int ringId;
        public float requestedDelta, reachedDelta;
        public bool blocked, canSolve;
        public int blockerOwner = -1;
        public string blockerRole;
        public int activeBindings;
    }
    static AsyncBatchJob activeBatch;

    public static string ProbeSliderBounds(string packPath, string templatePath, int sourceId, string destination)
    {
        var pack = JsonUtility.FromJson<RotateRingsLevelPack>(File.ReadAllText(packPath));
        var templates = JsonUtility.FromJson<RotateRingsTemplateCatalog>(File.ReadAllText(templatePath));
        var level = pack.levels.Find(item => item.id == sourceId);
        if (level == null) throw new InvalidDataException("Unknown source ID " + sourceId);
        var report = new SliderProbeReport { sourceId = sourceId };
        using (var fixture = new FixtureWorld(level, templates))
        {
            var state = fixture.InitialState();
            foreach (var part in level.parts.Where(RotateRingsRules.IsSlider))
            {
                var result = new SliderProbeResult { partId = part.id, startSlide = fixture.collision.GetSlide(part.id) };
                foreach (int sign in new[] { 1, -1 })
                {
                    fixture.RestoreBranch(state);
                    fixture.collision.BeginGesture(part.id);
                    float position = result.startSlide;
                    bool blocked = false;
                    int guard = 0;
                    while (!blocked && guard++ < 128)
                    {
                        float previous = position;
                        position = fixture.collision.ClampSlideForDeltaTime(part.id, position, position + sign * .36f, 1f / 60f, out blocked);
                        if (Mathf.Abs(position - previous) < 1e-6f) break;
                    }
                    if (sign > 0)
                    {
                        result.positiveLimit = position;
                        result.positiveBlocked = blocked;
                        result.positiveBlockerOwner = fixture.collision.LastBlockerOwner;
                        result.positiveBlockerRole = fixture.collision.LastBlockerRole;
                        result.activeBindingsAtPositiveLimit = fixture.collision.ActiveAuthoredContactCount;
                        result.canSolveAtPositiveLimit = fixture.collision.CanSolve(part.id);
                    }
                    else
                    {
                        result.negativeLimit = position;
                        result.negativeBlocked = blocked;
                        result.negativeBlockerOwner = fixture.collision.LastBlockerOwner;
                        result.negativeBlockerRole = fixture.collision.LastBlockerRole;
                        result.activeBindingsAtNegativeLimit = fixture.collision.ActiveAuthoredContactCount;
                        result.canSolveAtNegativeLimit = fixture.collision.CanSolve(part.id);
                    }
                    fixture.collision.EndGesture(part.id);
                }
                report.sliders.Add(result);
            }
        }
        string json = JsonUtility.ToJson(report, true);
        if (!string.IsNullOrEmpty(destination)) File.WriteAllText(destination, json);
        return json;
    }

    public static string ProbeSliderThenRings(string packPath, string templatePath, int sourceId, int sliderId, int directionSign)
    {
        var pack = JsonUtility.FromJson<RotateRingsLevelPack>(File.ReadAllText(packPath));
        var templates = JsonUtility.FromJson<RotateRingsTemplateCatalog>(File.ReadAllText(templatePath));
        var level = pack.levels.Find(item => item.id == sourceId);
        if (level == null) throw new InvalidDataException("Unknown source ID " + sourceId);
        var report = new SliderRingProbeReport { sourceId = sourceId, sliderId = sliderId };
        using (var fixture = new FixtureWorld(level, templates))
        {
            var state = fixture.InitialState();
            if (!RotateRingsRules.IsSlider(level.parts.Find(part => part.id == sliderId))) throw new InvalidDataException("Part is not a slider: " + sliderId);
            fixture.collision.BeginGesture(sliderId);
            float slide = fixture.collision.GetSlide(sliderId);
            bool blocked = false;
            for (int frame = 0; frame < 128 && !blocked; frame++)
            {
                float next = fixture.collision.ClampSlideForDeltaTime(sliderId, slide, slide + Math.Sign(directionSign) * .36f, 1f / 60f, out blocked);
                if (Mathf.Abs(next - slide) < 1e-6f) break;
                slide = next;
            }
            fixture.collision.EndGesture(sliderId);
            state.slides[sliderId] = fixture.collision.GetSlide(sliderId);
            report.sliderPosition = state.slides[sliderId];
            fixture.SyncGeometry(state);
            foreach (var ring in level.parts.Where(part => part.kind == "ring"))
            {
                fixture.RestoreBranch(state);
                float original = fixture.collision.GetAngle(ring.id);
                foreach (float delta in new[] { -180f, -120f, -90f, -60f, -30f, 30f, 60f, 90f, 120f, 180f })
                {
                    fixture.RestoreBranch(state);
                    fixture.collision.BeginGesture(ring.id);
                    float reached = fixture.collision.ClampRotation(ring.id, original, delta, out bool rotationBlocked);
                    int blockerOwner = fixture.collision.LastBlockerOwner;
                    string blockerRole = fixture.collision.LastBlockerRole;
                    fixture.collision.EndGesture(ring.id, rotationBlocked ? Math.Sign(delta) : 0f);
                    report.rings.Add(new RingReleaseProbe
                    {
                        ringId = ring.id,
                        requestedDelta = delta,
                        reachedDelta = reached - original,
                        blocked = rotationBlocked,
                        canSolve = !rotationBlocked && Mathf.Abs(reached - original - delta) < .05f && fixture.collision.CanSolve(ring.id),
                        blockerOwner = blockerOwner,
                        blockerRole = blockerRole,
                        activeBindings = fixture.collision.ActiveAuthoredContactCount
                    });
                }
            }
        }
        return JsonUtility.ToJson(report, true);
    }

    public static string ProbeSliderFrameBudgetRegression(string packPath, string templatePath, int sourceId, int sliderId, float targetSlide, string destination)
    {
        var pack = JsonUtility.FromJson<RotateRingsLevelPack>(File.ReadAllText(packPath));
        var templates = JsonUtility.FromJson<RotateRingsTemplateCatalog>(File.ReadAllText(templatePath));
        var level = pack.levels.Find(item => item.id == sourceId);
        if (level == null) throw new InvalidDataException("Unknown source ID " + sourceId);
        var report = new SliderFrameBudgetRegressionReport { sourceId = sourceId, sliderId = sliderId, targetSlide = targetSlide };
        using (var fixture = new FixtureWorld(level, templates))
        {
            var initial = fixture.InitialState();
            if (!RotateRingsRules.IsSlider(level.parts.Find(part => part.id == sliderId))) throw new InvalidDataException("Part is not a slider: " + sliderId);

            fixture.RestoreBranch(initial);
            fixture.collision.BeginGesture(sliderId);
            report.oneFrameReached = fixture.collision.ClampSlideForDeltaTime(sliderId, 0f, targetSlide, 1f / 60f, out report.oneFrameBlocked);
            report.oneFrameBlockerOwner = fixture.collision.LastBlockerOwner;
            fixture.collision.EndGesture(sliderId);

            fixture.RestoreBranch(initial);
            fixture.collision.BeginGesture(sliderId);
            float position = 0f;
            bool blocked = false;
            int guard = Mathf.CeilToInt(Mathf.Abs(targetSlide) / .36f) + 4;
            for (int frame = 0; frame < guard && Mathf.Abs(position - targetSlide) > .001f && !blocked; frame++)
            {
                float previous = position;
                position = fixture.collision.ClampSlideForDeltaTime(sliderId, position, targetSlide, 1f / 60f, out blocked);
                if (Mathf.Abs(position - previous) <= 1e-6f) break;
            }
            report.repeatedFramesReached = position;
            report.repeatedFramesBlocked = blocked;
            report.repeatedFramesBlockerOwner = fixture.collision.LastBlockerOwner;
            fixture.collision.EndGesture(sliderId);
            report.repeatedFramesReached = fixture.collision.GetSlide(sliderId);
            report.releasedAfterRepeatedFrames = fixture.collision.CanSolve(sliderId);
            report.activeBindingsAfterRepeatedFrames = fixture.collision.ActiveAuthoredContactCount;

            fixture.RestoreBranch(initial);
            var actionState = initial.Copy();
            var action = new SolverAction { kind = "slide", part = sliderId, targetSlide = targetSlide };
            fixture.ApplyAction(actionState, action, out report.actionBlocked);
            report.actionReached = fixture.collision.GetSlide(sliderId);
            report.actionRemovedSlider = actionState.removed.Contains(sliderId);
            report.removedAfterAction = actionState.removed.OrderBy(id => id).ToList();
        }
        string json = JsonUtility.ToJson(report, true);
        if (!string.IsNullOrEmpty(destination)) File.WriteAllText(destination, json);
        return json;
    }
    sealed class FixtureWorld : IDisposable
    {
        public GameObject root;
        public RotateRingsCollisionWorld collision;
        public RotateRingsBombContacts bombs;
        public RotateRingsLevel level;
        public RotateRingsTemplateCatalog templates;
        public SearchState activeState;
        public FixtureWorld(RotateRingsLevel sourceLevel, RotateRingsTemplateCatalog sourceTemplates)
        {
            level = sourceLevel; templates = sourceTemplates;
            root = new GameObject("Source Solver V3 Fixture") { hideFlags = HideFlags.HideAndDontSave };
            collision = root.AddComponent<RotateRingsCollisionWorld>();
            bombs = root.AddComponent<RotateRingsBombContacts>();
            collision.MotionSample += OnMotionSample;
            collision.Build(level, templates);
            bombs.Build(level, collision);
        }
        public void Dispose() { if (root != null) UnityEngine.Object.DestroyImmediate(root); }
        void OnMotionSample(int owner)
        {
            if (activeState == null) return;
            activeState.angles[owner] = collision.GetAngle(owner);
            if (RotateRingsRules.IsSlider(FindPart(owner))) activeState.slides[owner] = collision.GetSlide(owner);
            if (bombs.ProbeDuringDrag(owner, activeState.spentBombs, activeState.removed, id => RemovePart(activeState, id)))
                Cascade(activeState);
        }
        RotateRingsPart FindPart(int id) => level.parts.Find(p => p.id == id);
        bool RemovePart(SearchState state, int id)
        {
            var part = FindPart(id);
            if (part == null || state.removed.Contains(id)) return false;
            state.removed.Add(id);
            if (RotateRingsRules.IsMovable(part))
                foreach (var other in level.parts)
                    if (state.chainLocks.TryGetValue(other.id, out int count) && count > 0)
                        state.chainLocks[other.id] = other.id == id ? 0 : count - 1;
            if (level.bombs != null)
                for (int i = 0; i < level.bombs.Length; i++)
                    if (level.bombs[i].ownerPartId == id) state.spentBombs.Add(i);
            if (part.key != 0) state.keys.Add(part.key);
            SyncGeometry(state);
            return true;
        }
        void Cascade(SearchState state)
        {
            bool changed = true;
            int guard = 0;
            while (changed && guard++ < level.parts.Count)
            {
                changed = false;
                for (int i = level.parts.Count - 1; i >= 0; i--)
                {
                    var part = level.parts[i];
                    if (!state.removed.Contains(part.id) && collision.CanSolve(part.id))
                        changed |= RemovePart(state, part.id);
                }
            }
        }
        public void SyncGeometry(SearchState state)
        {
            activeState = state;
            collision.ApplyState(state.angles, state.removed, state.slides);
            bombs.ApplyState(state.spentBombs, state.removed);
            Physics2D.SyncTransforms();
        }
        public void RestoreBranch(SearchState state)
        {
            SyncGeometry(state);
            if (state.bindings != null) collision.RestoreBindingState(state.bindings);
        }
        public SearchState InitialState()
        {
            var state = new SearchState();
            foreach (var part in level.parts)
            {
                state.angles[part.id] = part.angle;
                state.chainLocks[part.id] = part.chainLockCount;
            }
            SyncGeometry(state);
            state.bindings = CloneBindingSnapshot(collision.CaptureBindingState());
            activeState = state;
            return state;
        }
        public bool IsGoal(SearchState state) => level.parts.All(p => state.removed.Contains(p.id));
        public bool IsInputLocked(SearchState state, RotateRingsPart part) =>
            (part.lockKey != 0 && !state.keys.Contains(part.lockKey)) ||
            (state.chainLocks.TryGetValue(part.id, out int count) && count > 0);

        public bool ApplyAction(SearchState state, SolverAction action, out bool blocked)
        {
            blocked = false;
            string beforeStateKey = StateKey(level, state);
            RestoreBranch(state);
            var part = FindPart(action.part);
            if (part == null || state.removed.Contains(part.id) || !RotateRingsRules.IsMovable(part) || IsInputLocked(state, part)) return false;
            bool slider = RotateRingsRules.IsSlider(part);
            if (!slider && collision.HasOutgoingBinding(part.id)) return false;
            activeState = state;
            collision.BeginGesture(part.id);
            float before = slider ? collision.GetSlide(part.id) : collision.GetAngle(part.id);
            float rotationBlockDirection = 0f;
            if (action.kind == "tap")
            {
                // A pointer-down/up with no displacement still invokes the native release solve check.
            }
            else if (slider)
            {
                bool stepBlocked = false;
                float target = action.targetSlide;
                float reached = before;
                float requestedFrameBudget = .36f;
                int guard = Math.Max(1, Mathf.CeilToInt(Mathf.Abs(target - before) / requestedFrameBudget) + 2);
                for (int frame = 0; frame < guard && Mathf.Abs(reached - target) > .001f; frame++)
                {
                    float previous = reached;
                    reached = collision.ClampSlideForDeltaTime(part.id, reached, target, 1f / 60f, out stepBlocked);
                    if (stepBlocked || Mathf.Abs(reached - previous) <= 1e-6f || state.removed.Contains(part.id)) break;
                }
                blocked = stepBlocked;
            }
            else
            {
                float remaining = action.delta;
                // One source action can contain many pointer samples; these source angular
                // boundaries are smaller than the CollisionWorld's .5 degree sweep steps.
                int guard = Math.Max(1, Mathf.CeilToInt(Mathf.Abs(remaining) / 5f) + 1);
                for (int frame = 0; frame < guard && Mathf.Abs(remaining) > .001f; frame++)
                {
                    float delta = Mathf.Clamp(remaining, -5f, 5f);
                    float current = collision.GetAngle(part.id);
                    float reached = collision.ClampRotation(part.id, current, delta, out bool stepBlocked);
                    float accepted = reached - current;
                    remaining -= accepted;
                    blocked |= stepBlocked;
                    if (stepBlocked) rotationBlockDirection = Mathf.Sign(delta);
                    if (stepBlocked || Mathf.Abs(accepted) <= 1e-6f || state.removed.Contains(part.id)) break;
                }
            }
            if (!state.removed.Contains(part.id)) collision.EndGesture(part.id, rotationBlockDirection);
            state.angles[part.id] = collision.GetAngle(part.id);
            if (slider) state.slides[part.id] = collision.GetSlide(part.id);
            if (!state.removed.Contains(part.id) && collision.CanSolve(part.id)) RemovePart(state, part.id);
            Cascade(state);
            action.blockerOwner = collision.LastBlockerOwner;
            action.blocked |= blocked;
            state.bindings = CloneBindingSnapshot(collision.CaptureBindingState());
            return beforeStateKey != StateKey(level, state);
        }
    }

    public static string RunPortfolio(string destination)
    {
        return RunPortfolio(destination, 256, false);
    }
    public static string RunPortfolio(string destination, int maxStates, bool full500)
    {
        string root = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Research/physics_v3"));
        string packPath = Path.Combine(full500 ? Path.Combine(root, "full500") : root, "source_parity_pack.json");
        string templatePath = Path.Combine(full500 ? Path.Combine(root, "full500") : root, "source_templates.json");
        return RunBatchCore(packPath, templatePath, null, maxStates, 1000, destination);
    }
    public static string RunBatch(string requestPath, string responsePath)
    {
        var request = JsonUtility.FromJson<SolverRequest>(File.ReadAllText(requestPath));
        if (request == null || request.schemaVersion != 3) throw new InvalidDataException("Unsupported SourceSolverV3 request schema.");
        string root = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Research/physics_v3"));
        string packPath = request.sourcePackPath;
        string templatePath = request.templatesPath;
        if (string.IsNullOrEmpty(packPath)) packPath = Path.Combine(request.full500 ? Path.Combine(root, "full500") : root, "source_parity_pack.json");
        if (string.IsNullOrEmpty(templatePath)) templatePath = Path.Combine(request.full500 ? Path.Combine(root, "full500") : root, "source_templates.json");
        return RunBatchCore(packPath, templatePath, request.sourceIds, request.maxStates, request.wallTimeBudgetMs, responsePath);
    }

    // Separate diagnostic policy: same fixture, ApplyAction and Replay, with diversified
    // candidate scheduling/admission. The default batch entry points remain unchanged.
    public static string RunDiversityBatch(string requestPath, string responsePath)
    {
        var request = JsonUtility.FromJson<SolverRequest>(File.ReadAllText(requestPath));
        if (request == null || request.schemaVersion != 3) throw new InvalidDataException("Unsupported SourceSolverV3 request schema.");
        string root = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Research/physics_v3"));
        string packPath = request.sourcePackPath;
        string templatePath = request.templatesPath;
        if (string.IsNullOrEmpty(packPath)) packPath = Path.Combine(request.full500 ? Path.Combine(root, "full500") : root, "source_parity_pack.json");
        if (string.IsNullOrEmpty(templatePath)) templatePath = Path.Combine(request.full500 ? Path.Combine(root, "full500") : root, "source_templates.json");
        return RunBatchCore(packPath, templatePath, request.sourceIds, request.maxStates, request.wallTimeBudgetMs,
            responsePath, true, request.diversityBucketCapacity);
    }

    // MCP-friendly portfolio runner: return immediately, then process one bounded level
    // per editor update and persist a checkpoint after every level.
    public static string StartBatch(string requestPath, string responsePath)
    {
        return StartBatchInternal(requestPath, responsePath, false);
    }

    public static string StartDiversityBatch(string requestPath, string responsePath)
    {
        return StartBatchInternal(requestPath, responsePath, true);
    }

    static string StartBatchInternal(string requestPath, string responsePath, bool diversityPolicy)
    {
        if (activeBatch != null) throw new InvalidOperationException("A SourceSolverV3 batch is already running.");
        var request = JsonUtility.FromJson<SolverRequest>(File.ReadAllText(requestPath));
        if (request == null || request.schemaVersion != 3) throw new InvalidDataException("Unsupported SourceSolverV3 request schema.");
        string root = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Research/physics_v3"));
        string packPath = request.sourcePackPath;
        string templatePath = request.templatesPath;
        if (string.IsNullOrEmpty(packPath)) packPath = Path.Combine(request.full500 ? Path.Combine(root, "full500") : root, "source_parity_pack.json");
        if (string.IsNullOrEmpty(templatePath)) templatePath = Path.Combine(request.full500 ? Path.Combine(root, "full500") : root, "source_templates.json");
        var pack = JsonUtility.FromJson<RotateRingsLevelPack>(File.ReadAllText(packPath));
        var templates = JsonUtility.FromJson<RotateRingsTemplateCatalog>(File.ReadAllText(templatePath));
        if (pack == null || pack.levels == null || templates == null || templates.templates == null)
            throw new InvalidDataException("Source pack or template catalog was not readable.");
        List<RotateRingsLevel> levels;
        if (request.sourceIds == null || request.sourceIds.Length == 0) levels = pack.levels;
        else
        {
            var duplicates = request.sourceIds.GroupBy(id => id).Where(group => group.Count() > 1).Select(group => group.Key).ToArray();
            if (duplicates.Length > 0) throw new InvalidDataException("Requested source IDs contain duplicates: " + string.Join(",", duplicates));
            var byId = pack.levels.ToDictionary(level => level.id);
            var missing = request.sourceIds.Where(id => !byId.ContainsKey(id)).Distinct().ToArray();
            if (missing.Length > 0) throw new InvalidDataException("Requested source IDs are missing from pack: " + string.Join(",", missing));
            levels = request.sourceIds.Select(id => byId[id]).ToList();
        }
        int diversityCapacity = Math.Max(1, request.diversityBucketCapacity);
        var report = new SolverReport
        {
            status = "running", complete = false,
            packPath = packPath, templatesPath = templatePath,
            maxStates = Math.Max(1, request.maxStates),
            wallTimeBudgetMs = Math.Max(1, request.wallTimeBudgetMs),
            totalLevels = levels.Count,
            inputHash = HashInputFiles(packPath, templatePath),
            rulesHash = HashRuleBundle(),
            unityVersion = Application.unityVersion,
            fixedDeltaTime = 1f / 60f,
            policyId = diversityPolicy ? DiversityPolicyId : CurrentPolicyId,
            defaultMaxStates = 256,
            defaultWallTimeBudgetMs = 1000
        };
        activeBatch = new AsyncBatchJob { responsePath = responsePath, templates = templates, levels = levels, report = report,
            diversityPolicy = diversityPolicy, diversityBucketCapacity = diversityCapacity };
        SaveAsyncReport(activeBatch);
        EditorApplication.update -= TickAsyncBatch;
        EditorApplication.update += TickAsyncBatch;
        return "running; totalLevels=" + levels.Count + "; responsePath=" + responsePath + "; inputHash=" + report.inputHash;
    }

    public static string GetBatchStatus()
    {
        return activeBatch == null ? "{\"status\":\"idle\"}" : JsonUtility.ToJson(activeBatch.report, true);
    }

    static void TickAsyncBatch()
    {
        var job = activeBatch;
        if (job == null) { EditorApplication.update -= TickAsyncBatch; return; }
        if (job.nextLevel >= job.levels.Count)
        {
            job.report.status = job.report.levels.Any(level => level.status == "unknown") ? "partial" : "complete";
            job.report.complete = true;
            SaveAsyncReport(job);
            activeBatch = null;
            EditorApplication.update -= TickAsyncBatch;
            return;
        }
        var level = job.levels[job.nextLevel++];
        var row = new LevelResult { sourceId = level.id, sourcePosition = level.position, partCount = level.parts.Count, status = "unknown", proof = "bounded source-physics search; no unsolvability proof" };
        job.report.levels.Add(row);
        row.diversityCapacity = job.diversityBucketCapacity;
        try { Search(level, job.templates, job.report.maxStates, job.report.wallTimeBudgetMs, row, job.diversityPolicy, job.diversityBucketCapacity); }
        catch (Exception ex) { row.status = "unknown"; row.error = ex.ToString(); }
        job.report.completedLevels = job.nextLevel;
        SaveAsyncReport(job);
    }

    static void SaveAsyncReport(AsyncBatchJob job)
    {
        string json = JsonUtility.ToJson(job.report, true);
        string directory = Path.GetDirectoryName(Path.GetFullPath(job.responsePath));
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        string temporaryPath = job.responsePath + ".tmp";
        File.WriteAllText(temporaryPath, json);
        if (File.Exists(job.responsePath)) File.Delete(job.responsePath);
        File.Move(temporaryPath, job.responsePath);
    }

    static string HashInputFiles(string packPath, string templatePath) =>
        HashBundle(new[] { "source_parity_pack.json", "source_templates.json" },
            new[] { packPath, templatePath }, Application.unityVersion, 1f / 60f);

    static string HashRuleBundle()
    {
        string[] relativePaths = {
            "RotateRings/Scripts/RotateRingsCollisionWorld.cs",
            "RotateRings/Scripts/RotateRingsBombContacts.cs",
            "RotateRings/Scripts/RotateRingsLevelData.cs",
            "RotateRings/Scripts/RotateRingsRuntime.cs",
            "RotateRings/Scripts/RotateRingsNativeInterlockPolicy.cs",
            "RotateRings/Resources/SourceOverlapQueries.json",
            "RotateRings/Editor/SourceSolverV3.cs"
        };
        string[] fullPaths = relativePaths.Select(path => Path.Combine(Application.dataPath, path)).ToArray();
        return HashBundle(relativePaths, fullPaths, Application.unityVersion, 1f / 60f);
    }

    static string HashBundle(string[] names, string[] paths, string unityVersion, float fixedDeltaTime)
    {
        if (names == null || paths == null || names.Length != paths.Length)
            throw new ArgumentException("Hash bundle names and paths must have matching lengths.");
        using (var sha = SHA256.Create())
        using (var stream = new MemoryStream())
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
        {
            writer.Write(unityVersion ?? string.Empty);
            writer.Write(fixedDeltaTime);
            for (int i = 0; i < paths.Length; i++)
            {
                if (!File.Exists(paths[i])) throw new FileNotFoundException("Required solver hash input is missing: " + names[i], paths[i]);
                byte[] contents = File.ReadAllBytes(paths[i]);
                writer.Write(names[i] ?? string.Empty);
                writer.Write(contents.Length);
                writer.Write(contents);
            }
            writer.Flush();
            stream.Position = 0;
            return string.Concat(sha.ComputeHash(stream).Select(value => value.ToString("x2")));
        }
    }

    static string RunBatchCore(string packPath, string templatePath, int[] sourceIds, int maxStates, int wallTimeBudgetMs, string destination,
        bool diversityPolicy = false, int diversityBucketCapacity = 2)
    {
        var pack = JsonUtility.FromJson<RotateRingsLevelPack>(File.ReadAllText(packPath));
        var templates = JsonUtility.FromJson<RotateRingsTemplateCatalog>(File.ReadAllText(templatePath));
        if (pack == null || pack.levels == null || templates == null || templates.templates == null)
            throw new InvalidDataException("Source pack or template catalog was not readable.");
        var report = new SolverReport
        {
            status = "complete",
            policyId = diversityPolicy ? DiversityPolicyId : CurrentPolicyId,
            packPath = packPath,
            templatesPath = templatePath,
            maxStates = Math.Max(1, maxStates),
            wallTimeBudgetMs = Math.Max(1, wallTimeBudgetMs),
            inputHash = HashInputFiles(packPath, templatePath),
            rulesHash = HashRuleBundle(),
            unityVersion = Application.unityVersion,
            fixedDeltaTime = 1f / 60f
        };
        IEnumerable<RotateRingsLevel> levels;
        if (sourceIds == null || sourceIds.Length == 0) levels = pack.levels;
        else
        {
            var byId = pack.levels.ToDictionary(level => level.id);
            var missing = sourceIds.Where(id => !byId.ContainsKey(id)).Distinct().ToArray();
            if (missing.Length > 0) throw new InvalidDataException("Requested source IDs are missing from pack: " + string.Join(",", missing));
            levels = sourceIds.Select(id => byId[id]).ToArray();
        }
        foreach (var level in levels)
        {
            var row = new LevelResult { sourceId = level.id, sourcePosition = level.position, partCount = level.parts.Count, status = "unknown", proof = "bounded source-physics search; no unsolvability proof" };
            report.levels.Add(row);
            row.diversityCapacity = Math.Max(1, diversityBucketCapacity);
            try
            {
                Search(level, templates, report.maxStates, report.wallTimeBudgetMs, row, diversityPolicy, diversityBucketCapacity);
                if (row.status == "unknown") report.status = "partial";
            }
            catch (Exception ex)
            {
                row.status = "unknown";
                row.error = ex.ToString();
                report.status = "partial";
            }
        }
        report.complete = true;
        if (report.levels.Any(level => level.status == "unknown")) report.status = "partial";
        string json = JsonUtility.ToJson(report, true);
        if (!string.IsNullOrEmpty(destination))
        {
            string directory = Path.GetDirectoryName(Path.GetFullPath(destination));
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            File.WriteAllText(destination, json);
        }
        return json;
    }

    static void Search(RotateRingsLevel level, RotateRingsTemplateCatalog templates, int maxStates, int wallTimeBudgetMs,
        LevelResult result, bool diversityPolicy = false, int diversityBucketCapacity = 2)
    {
        using (var fixture = new FixtureWorld(level, templates))
        {
            diversityBucketCapacity = Math.Max(1, diversityBucketCapacity);
            var timer = Stopwatch.StartNew();
            var initial = fixture.InitialState();
            RecordBestProgress(level, initial, result);
            if (result.bestStateSummary == null) RecordBestProgress(level, initial, result, true);
            var rootNode = new SearchNode { state = initial };
            var frontier = new List<SearchNode> { rootNode };
            var visited = new HashSet<string> { StateKey(level, initial) };
            var seenDiversitySignatures = new HashSet<string> { DiversitySignature(level, initial) };
            result.diversitySignaturesSeen = diversityPolicy ? 1 : 0;
            var seenEndpointBuckets = new HashSet<string>();
            bool anyReachableInput = false;
            SearchNode goal = null;
            while (frontier.Count > 0 && result.exploredStates < maxStates && timer.ElapsedMilliseconds < Math.Max(1, wallTimeBudgetMs))
            {
                int bestIndex = 0;
                for (int i = 1; i < frontier.Count; i++)
                    if (CompareSearchNodes(level, frontier[i], frontier[bestIndex]) < 0) bestIndex = i;
                var node = frontier[bestIndex];
                frontier.RemoveAt(bestIndex);
                result.maximumFrontier = Math.Max(result.maximumFrontier, frontier.Count + 1);
                if (!node.counted)
                {
                    node.counted = true;
                    result.exploredStates++;
                }
                if (fixture.IsGoal(node.state)) { goal = node; break; }
                fixture.RestoreBranch(node.state);
                anyReachableInput |= HasPotentialInput(fixture, node.state);
                // Applying one candidate mutates the Physics2D fixture. Freeze the list once;
                // retain its cursor on the parent so successful removal branches can be
                // followed immediately and later backtracked to untried sibling actions.
                if (node.candidateActions == null)
                {
                    var candidates = EnumerateActions(fixture, node.state)
                        .OrderByDescending(action => action.quickRelease)
                        .ThenByDescending(action => action.guided)
                        .ThenBy(action => action.kind == "tap" ? 0 : action.kind == "rotate" ? 1 : 2)
                        .ThenBy(action => action.kind == "slide"
                            ? Mathf.Abs(action.targetSlide - (node.state.slides.TryGetValue(action.part, out float slide) ? slide : 0f))
                            : Mathf.Abs(action.delta)).ToList();
                    node.candidateActions = diversityPolicy ? InterleaveDiverseActions(candidates, node.state) : candidates;
                }
                while (node.nextCandidate < node.candidateActions.Count)
                {
                    if (timer.ElapsedMilliseconds >= Math.Max(1, wallTimeBudgetMs)) break;
                    var action = node.candidateActions[node.nextCandidate++];
                    result.attemptedActions++;
                    result.generatedActions++;
                    var next = node.state.Copy();
                    int spentBefore = next.spentBombs.Count;
                    if (!fixture.ApplyAction(next, action, out bool blocked))
                    {
                        result.noOpActions++;
                        if (result.actionTrace.Count < 96)
                            result.actionTrace.Add("noop part=" + action.part + " kind=" + action.kind + " d=" + action.delta.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) +
                                " slide=" + action.targetSlide.ToString("F3", System.Globalization.CultureInfo.InvariantCulture) + " blocker=" + fixture.collision.LastBlockerOwner + ":" + fixture.collision.LastBlockerRole +
                                " pose=" + fixture.collision.GetAngle(action.part).ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + "/" + fixture.collision.GetSlide(action.part).ToString("F3", System.Globalization.CultureInfo.InvariantCulture));
                        continue;
                    }
                    result.appliedActions++;
                    if (diversityPolicy)
                    {
                        string endpointBucket = ReachedEndpointBucket(level, node.state, next, action, blocked);
                        if (!string.IsNullOrEmpty(endpointBucket) && seenEndpointBuckets.Add(endpointBucket)) result.endpointBucketsSeen++;
                    }
                    next.bindings = CloneBindingSnapshot(fixture.collision.CaptureBindingState());
                    int bindingsBefore = CountActiveBindings(node.state.bindings);
                    int bindingsAfter = CountActiveBindings(next.bindings);
                    if (bindingsBefore != bindingsAfter || next.removed.Count > node.state.removed.Count ||
                        next.keys.Count > node.state.keys.Count || next.spentBombs.Count > node.state.spentBombs.Count)
                    {
                        result.bindingChangingTransitions++;
                        if (result.transitionTrace.Count < 64)
                            result.transitionTrace.Add("from=" + node.state.removed.Count + "/" + bindingsBefore +
                                " action=" + action.kind + ":" + action.part + ":" + action.delta.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) +
                                ":" + action.targetSlide.ToString("F3", System.Globalization.CultureInfo.InvariantCulture) +
                                " blocked=" + blocked + " to=" + next.removed.Count + "/" + bindingsAfter);
                    }
                    if (result.actionTrace.Count < 96)
                        result.actionTrace.Add("applied part=" + action.part + " kind=" + action.kind + " d=" + action.delta.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) +
                            " slide=" + action.targetSlide.ToString("F3", System.Globalization.CultureInfo.InvariantCulture) + " blocked=" + blocked + " blocker=" + fixture.collision.LastBlockerOwner + ":" + fixture.collision.LastBlockerRole +
                            " removed=" + next.removed.Count + " bindings=" + bindingsAfter);
                    RecordBestProgress(level, next, result);
                    if (next.removed.SetEquals(node.state.removed) && next.keys.SetEquals(node.state.keys) &&
                        next.spentBombs.SetEquals(node.state.spentBombs) && StateKey(level, next) == StateKey(level, node.state)) continue;
                    var nextAction = new SolverAction { kind = action.kind, part = action.part, delta = action.delta, targetSlide = action.targetSlide, blocked = action.blocked || blocked || next.spentBombs.Count > spentBefore, blockerOwner = action.blockerOwner };
                    string key = StateKey(level, next);
                    if (!visited.Add(key)) continue;
                    if (diversityPolicy)
                    {
                        string signature = DiversitySignature(level, next);
                        if (seenDiversitySignatures.Add(signature)) result.diversitySignaturesSeen++;
                    }
                    bool removalProgress = next.removed.Count > node.state.removed.Count;
                    bool bindingProgress = CountActiveBindings(next.bindings) < CountActiveBindings(node.state.bindings);
                    var child = new SearchNode
                    {
                        state = next, parent = node, action = nextAction,
                        guided = node.guided || (action.guided && !nextAction.blocked),
                        prioritizedBranch = removalProgress || bindingProgress || (action.guided && !nextAction.blocked)
                    };
                    if (fixture.IsGoal(next)) { goal = child; break; }
                    frontier.Add(child);
                    if (child.prioritizedBranch && !diversityPolicy) break;
                }
                if (goal != null) break;
                if (node.nextCandidate < node.candidateActions.Count) frontier.Add(node);
                int beamWidth = Math.Max(16, Math.Min(128, maxStates));
                if (frontier.Count > beamWidth)
                {
                    frontier.Sort((a, b) => CompareSearchNodes(level, a, b));
                    if (diversityPolicy)
                    {
                        var diverse = new List<SearchNode>(beamWidth);
                        var signatureCounts = new Dictionary<string, int>();
                        foreach (var candidate in frontier)
                        {
                            string signature = DiversitySignature(level, candidate.state);
                            int count = signatureCounts.TryGetValue(signature, out int existing) ? existing : 0;
                            if (count >= diversityBucketCapacity) { result.diversitySignatureDrops++; continue; }
                            signatureCounts[signature] = count + 1;
                            diverse.Add(candidate);
                            if (diverse.Count >= beamWidth) break;
                        }
                        frontier = diverse;
                    }
                    else frontier.RemoveRange(beamWidth, frontier.Count - beamWidth);
                }
            }
            result.searchElapsedMs = timer.ElapsedMilliseconds;
            if (goal == null)
            {
                result.status = "unknown";
                result.stopReason = timer.ElapsedMilliseconds >= Math.Max(1, wallTimeBudgetMs) ? "wall time budget" :
                    result.exploredStates >= maxStates ? "state budget" : "finite sampled frontier exhausted";
                result.proof = timer.ElapsedMilliseconds >= Math.Max(1, wallTimeBudgetMs)
                    ? "wall time budget reached before a goal; bounded search cannot establish unsolvability"
                    : result.exploredStates >= maxStates
                        ? "state bound reached before a goal; bounded search cannot establish unsolvability"
                            : frontier.Count == 0 && !anyReachableInput
                            ? "no registered movable input was found in the explored states; continuous gestures and unenumerated actions remain unproven"
                            : "bounded action candidates exhausted; continuous gestures remain unproven";
                return;
            }
            var path = new List<SolverAction>();
            for (var node = goal; node != null && node.action != null; node = node.parent) path.Add(node.action);
            path.Reverse();
            result.actions = path;
            result.moveCount = path.Count;
            result.status = "solved";
            result.goal = true;
            result.proof = "goal reached using production collision/bomb mechanics; replay pending";
            result.finalRemoved = goal.state.removed.OrderBy(id => id).ToList();
        }
        result.replayGoal = Replay(level, templates, result.actions, out int replayed, out string replayError);
        result.replayedActions = replayed;
        if (result.replayGoal) result.proof = "goal reached and replayed using production collision/bomb mechanics";
        else
        {
            result.status = "unknown";
            result.goal = false;
            result.error = replayError;
            result.validationError = replayError ?? "Replay did not reach the goal.";
            result.proof = "search candidate did not survive an independent production-component replay";
        }
    }

    static void RecordBestProgress(RotateRingsLevel level, SearchState state, LevelResult result, bool force = false)
    {
        int activeBindings = CountActiveBindings(state.bindings);
        float travel = 0f;
        foreach (var part in level.parts)
        {
            if (state.removed.Contains(part.id)) continue;
            if (RotateRingsRules.IsSlider(part))
            {
                if (state.slides.TryGetValue(part.id, out float slide)) travel += Mathf.Abs(slide) * 360f;
            }
            else if (state.angles.TryGetValue(part.id, out float angle)) travel += Mathf.Abs(Mathf.DeltaAngle(part.angle, angle));
        }
        bool better = state.removed.Count > result.maximumRemovedParts ||
            (state.removed.Count == result.maximumRemovedParts && activeBindings < result.bestActiveBindings) ||
            (state.removed.Count == result.maximumRemovedParts && activeBindings == result.bestActiveBindings && travel < ParseTravel(result.bestStateSummary));
        if (!better && !force) return;
        result.maximumRemovedParts = state.removed.Count;
        result.bestRemainingRootCount = level.parts.Count - state.removed.Count;
        result.bestActiveBindings = activeBindings;
        result.bestRemovedParts = state.removed.OrderBy(id => id).ToList();
        result.bestStateSummary = "travel=" + travel.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) + "; " +
            string.Join("; ", level.parts.OrderBy(part => part.id).Select(part =>
                state.removed.Contains(part.id) ? part.id + "=removed" :
                RotateRingsRules.IsSlider(part) ? part.id + "=slide:" + (state.slides.TryGetValue(part.id, out float slide) ? slide : 0f).ToString("F3", System.Globalization.CultureInfo.InvariantCulture) :
                part.id + "=angle:" + (state.angles.TryGetValue(part.id, out float angle) ? angle : part.angle).ToString("F2", System.Globalization.CultureInfo.InvariantCulture)));
    }

    static List<SolverAction> InterleaveDiverseActions(List<SolverAction> actions, SearchState state)
    {
        var buckets = new SortedDictionary<string, Queue<SolverAction>>(StringComparer.Ordinal);
        foreach (var action in actions)
        {
            string bucket;
            if (action.kind == "tap") bucket = action.part + ":tap";
            else if (action.kind == "slide")
            {
                float current = state.slides.TryGetValue(action.part, out float slide) ? slide : 0f;
                float distance = action.targetSlide - current;
                int scale = Mathf.Abs(distance) < .36f ? 0 : Mathf.Abs(distance) < 2.4f ? 1 : 2;
                bucket = action.part + ":slide:" + Math.Sign(distance) + ":" + scale;
            }
            else
            {
                int scale = Mathf.Abs(action.delta) < 30f ? 0 : Mathf.Abs(action.delta) < 90f ? 1 : 2;
                bucket = action.part + (action.guided ? ":guided:" : ":rotate:") + Math.Sign(action.delta) + ":" + scale;
            }
            if (!buckets.TryGetValue(bucket, out Queue<SolverAction> queue)) buckets[bucket] = queue = new Queue<SolverAction>();
            queue.Enqueue(action);
        }
        var result = new List<SolverAction>(actions.Count);
        while (result.Count < actions.Count)
        {
            foreach (var bucket in buckets.Values)
                if (bucket.Count > 0) result.Add(bucket.Dequeue());
        }
        return result;
    }

    static string ReachedEndpointBucket(RotateRingsLevel level, SearchState before, SearchState after, SolverAction action, bool blocked)
    {
        if (action.kind == "slide")
        {
            float value = after.slides.TryGetValue(action.part, out float slide) ? slide : 0f;
            // Endpoint is an observation only: record the actually reached pose and whether
            // production clamping blocked the gesture. It does not assert a global limit.
            int bin = Mathf.RoundToInt(value / .12f);
            return "slider:" + action.part + ":" + bin + ":" + (blocked ? "clamped" : "free");
        }
        if (action.kind == "rotate")
        {
            float angle = after.angles.TryGetValue(action.part, out float reached) ? reached : 0f;
            int sector = Mathf.FloorToInt(Mathf.Repeat(angle, 360f) / 15f);
            return "ring:" + action.part + ":" + sector + (action.guided ? ":guided" : "");
        }
        return null;
    }

    static string DiversitySignature(RotateRingsLevel level, SearchState state)
    {
        var key = new StringBuilder();
        key.Append("R:").Append(string.Join(",", state.removed.OrderBy(id => id))).Append(";K:")
            .Append(string.Join(",", state.keys.OrderBy(id => id))).Append(";B:")
            .Append(string.Join(",", state.spentBombs.OrderBy(id => id))).Append(";L:");
        foreach (var item in state.chainLocks.OrderBy(pair => pair.Key)) key.Append(item.Key).Append('=').Append(item.Value).Append(',');
        key.Append(";P:");
        foreach (var part in level.parts.OrderBy(part => part.id))
        {
            if (state.removed.Contains(part.id)) continue;
            if (RotateRingsRules.IsSlider(part))
            {
                float value = state.slides.TryGetValue(part.id, out float slide) ? slide : 0f;
                key.Append(part.id).Append('s').Append(Mathf.RoundToInt(value / .6f)).Append(',');
            }
            else
            {
                float value = state.angles.TryGetValue(part.id, out float angle) ? angle : part.angle;
                key.Append(part.id).Append('r').Append(Mathf.FloorToInt(Mathf.Repeat(value, 360f) / 30f)).Append(',');
            }
        }
        var bindings = state.bindings as RotateRingsCollisionWorld.BindingStateSnapshot;
        if (bindings != null)
        {
            key.Append(";AR:").Append(string.Join(",", (bindings.activeRingRelationIndices ?? new int[0]).OrderBy(v => v)))
                .Append(";PR:").Append(string.Join(",", (bindings.passiveRingRelationIndices ?? new int[0]).OrderBy(v => v)))
                .Append(";AG:").Append(string.Join(",", (bindings.activeGuideRelationKeys ?? new string[0]).OrderBy(v => v, StringComparer.Ordinal)));
            int pairs = bindings.activeRingContactOwners == null || bindings.activeRingContactColliderIds == null ? 0 :
                Math.Min(bindings.activeRingContactOwners.Length, bindings.activeRingContactColliderIds.Length) / 2;
            var contacts = new List<string>();
            for (int i = 0; i < pairs; i++)
                contacts.Add(bindings.activeRingContactOwners[i * 2] + ":" + bindings.activeRingContactColliderIds[i * 2] + ">" +
                    bindings.activeRingContactOwners[i * 2 + 1] + ":" + bindings.activeRingContactColliderIds[i * 2 + 1]);
            key.Append(";C:").Append(string.Join(",", contacts.OrderBy(value => value, StringComparer.Ordinal)));
        }
        return key.ToString();
    }

    static int CountActiveBindings(object value)
    {
        var bindings = value as RotateRingsCollisionWorld.BindingStateSnapshot;
        if (bindings == null) return 0;
        var ringRelations = new HashSet<int>();
        if (bindings.activeRingRelationIndices != null) ringRelations.UnionWith(bindings.activeRingRelationIndices);
        if (bindings.passiveRingRelationIndices != null) ringRelations.UnionWith(bindings.passiveRingRelationIndices);
        return ringRelations.Count + (bindings.activeGuideRelationKeys == null ? 0 : bindings.activeGuideRelationKeys.Length);
    }

    static float ParseTravel(string summary)
    {
        if (string.IsNullOrEmpty(summary) || !summary.StartsWith("travel=", StringComparison.Ordinal)) return -1f;
        int end = summary.IndexOf(';');
        string value = end < 0 ? summary.Substring(7) : summary.Substring(7, end - 7);
        return float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float travel) ? travel : -1f;
    }

    static int CompareSearchNodes(RotateRingsLevel level, SearchNode a, SearchNode b)
    {
        int progress = b.state.removed.Count.CompareTo(a.state.removed.Count);
        if (progress != 0) return progress;
        int activeBindings = CountActiveBindings(a.state.bindings).CompareTo(CountActiveBindings(b.state.bindings));
        if (activeBindings != 0) return activeBindings;
        int preferred = b.prioritizedBranch.CompareTo(a.prioritizedBranch);
        if (preferred != 0) return preferred;
        int guided = b.guided.CompareTo(a.guided);
        if (guided != 0) return guided;
        int aBlocked = a.action != null && a.action.blocked ? 1 : 0;
        int bBlocked = b.action != null && b.action.blocked ? 1 : 0;
        int blocked = aBlocked.CompareTo(bBlocked);
        if (blocked != 0) return blocked;
        int travel = TotalPoseTravel(level, a.state).CompareTo(TotalPoseTravel(level, b.state));
        if (travel != 0) return travel;
        int aDepth = 0, bDepth = 0;
        for (var n = a; n != null && n.action != null; n = n.parent) aDepth++;
        for (var n = b; n != null && n.action != null; n = n.parent) bDepth++;
        int depth = aDepth.CompareTo(bDepth);
        return depth != 0 ? depth : StateKey(level, a.state).CompareTo(StateKey(level, b.state));
    }

    static float TotalPoseTravel(RotateRingsLevel level, SearchState state)
    {
        float travel = 0f;
        foreach (var part in level.parts)
        {
            if (state.removed.Contains(part.id)) continue;
            if (RotateRingsRules.IsSlider(part))
            {
                if (state.slides.TryGetValue(part.id, out float slide)) travel += Mathf.Abs(slide);
            }
            else if (state.angles.TryGetValue(part.id, out float angle)) travel += Mathf.Abs(Mathf.DeltaAngle(part.angle, angle)) / 180f;
        }
        return travel;
    }

    static bool HasPotentialInput(FixtureWorld fixture, SearchState state)
    {
        foreach (var part in fixture.level.parts)
        {
            if (state.removed.Contains(part.id) || !RotateRingsRules.IsMovable(part) || fixture.IsInputLocked(state, part)) continue;
            if (!RotateRingsRules.IsSlider(part) && fixture.collision.HasOutgoingBinding(part.id)) continue;
            return true;
        }
        return false;
    }

    static float NativeSliderReach(FixtureWorld fixture, RotateRingsPart slider, SearchState state)
    {
        var template = fixture.templates.templates.FirstOrDefault(item => item.id == slider.templateId);
        if (template == null) return 9.6f;
        Vector2 axis = RotateRingsRules.SlideAxis(slider, template).normalized;
        Vector2 origin = fixture.collision.OwnerTransform(slider.id).position;
        float reach = 0f;
        foreach (var part in fixture.level.parts)
        {
            if (state.removed.Contains(part.id)) continue;
            foreach (var collider in fixture.collision.GetOwnerColliders(part.id))
            {
                if (collider == null || !collider.enabled) continue;
                Bounds bounds = collider.bounds;
                Vector2 ext = bounds.extents;
                Vector2 center = bounds.center;
                for (int x = -1; x <= 1; x += 2)
                    for (int y = -1; y <= 1; y += 2)
                    {
                        Vector2 corner = center + Vector2.Scale(ext, new Vector2(x, y));
                        reach = Mathf.Max(reach, Mathf.Abs(Vector2.Dot(corner - origin, axis)));
                    }
            }
        }
        // Keep the existing 9.6-unit search coverage for ordinary fixtures. A fixture
        // with authored geometry farther away gets one extra endpoint just beyond its
        // furthest native collider, rather than every speculative 12–48-unit target.
        return Mathf.Max(9.6f, reach + .36f);
    }

    static void AddGapBearing(List<float> bearings, Vector2 receiverCenter, Vector2 point, float reach)
    {
        Vector2 direction = point - receiverCenter;
        if (direction.sqrMagnitude < 1e-8f || direction.sqrMagnitude > reach * reach) return;
        float bearing = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        int key = Mathf.RoundToInt(Mathf.Repeat(bearing, 360f) * 10f);
        if (!bearings.Exists(value => Mathf.RoundToInt(Mathf.Repeat(value, 360f) * 10f) == key)) bearings.Add(bearing);
    }

    static List<float> SliderGapBearings(FixtureWorld fixture, RotateRingsPart obstacle, Transform receiverRoot, float reach)
    {
        var bearings = new List<float>();
        Vector2 receiverCenter = receiverRoot.position;
        var obstacleTemplate = fixture.templates.templates.FirstOrDefault(template => template.id == obstacle.templateId);
        if (obstacleTemplate == null) throw new InvalidOperationException("Unverified slider template for gap guidance: " + obstacle.templateId);
        Vector2 axis = RotateRingsRules.SlideAxis(obstacle, obstacleTemplate).normalized;
        var colliders = fixture.collision.GetOwnerColliders(obstacle.id);
        for (int shapeIndex = 0; shapeIndex < fixture.collision.GetOwnerShapeCount(obstacle.id); shapeIndex++)
        {
            if (fixture.collision.GetOwnerShapeRole(obstacle.id, shapeIndex) != "body" ||
                !fixture.collision.TryGetShapeWorldCenter(obstacle.id, shapeIndex, out var center)) continue;
            AddGapBearing(bearings, receiverCenter, center, reach);
            if ((center - receiverCenter).sqrMagnitude <= reach * reach)
            {
                float positiveAxisBearing = Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg;
                AddGapBearing(bearings, receiverCenter, receiverCenter + new Vector2(Mathf.Cos(positiveAxisBearing * Mathf.Deg2Rad), Mathf.Sin(positiveAxisBearing * Mathf.Deg2Rad)), reach);
                AddGapBearing(bearings, receiverCenter, receiverCenter - new Vector2(Mathf.Cos(positiveAxisBearing * Mathf.Deg2Rad), Mathf.Sin(positiveAxisBearing * Mathf.Deg2Rad)), reach);
            }
            if (shapeIndex >= colliders.Count || colliders[shapeIndex] == null) continue;
            var collider = colliders[shapeIndex];
            AddGapBearing(bearings, receiverCenter, collider.ClosestPoint(receiverCenter), reach);
            AddGapBearing(bearings, receiverCenter, collider.ClosestPoint(receiverCenter + axis * 100f), reach);
            AddGapBearing(bearings, receiverCenter, collider.ClosestPoint(receiverCenter - axis * 100f), reach);
        }
        return bearings;
    }

    static IEnumerable<float> GapRotationDeltas(RotateRingsPart receiver, RotateRingsOriginalTemplate receiverTemplate,
        SearchState state, int receiverId, float incomingBearing)
    {
        float desired = Mathf.DeltaAngle(state.angles[receiverId] + receiverTemplate.gapDirectionDeg, incomingBearing);
        float gapAngle = receiverTemplate.visualGap > .5f ? receiverTemplate.visualGap : Mathf.Max(0f, receiver.gap);
        float gapHalf = gapAngle * .5f;
        foreach (float margin in new[] { 0f, -gapHalf, gapHalf, -(gapHalf + 8f), gapHalf + 8f })
        {
            float delta = desired + margin;
            if (Mathf.Abs(delta) < 1f) continue;
            yield return delta;
            yield return delta - Mathf.Sign(delta) * 360f;
        }
    }

    static IEnumerable<SolverAction> EnumerateActions(FixtureWorld fixture, SearchState state)
    {
        fixture.RestoreBranch(state);
        foreach (var part in fixture.level.parts)
        {
            if (state.removed.Contains(part.id) || !RotateRingsRules.IsMovable(part) || fixture.IsInputLocked(state, part)) continue;
            bool slider = RotateRingsRules.IsSlider(part);
            if (!slider && fixture.collision.HasOutgoingBinding(part.id)) continue;
            if (fixture.collision.CanSolve(part.id)) yield return new SolverAction { kind = "tap", part = part.id };
            if (!slider)
            {
                if (part.kind == "ring")
                {
                    Transform receiverRoot = fixture.collision.OwnerTransform(part.id);
                    var receiverTemplate = fixture.templates.templates.FirstOrDefault(template => template.id == part.templateId);
                    if (receiverRoot != null && receiverTemplate != null)
                    {
                        var seenGapDeltas = new HashSet<int>();
                        foreach (var contact in fixture.collision.GetActiveAuthoredContacts())
                        {
                            if (contact.kind != "ring-link" || contact.targetId != part.id ||
                                contact.ownerRole != "head_trigger" || contact.targetRole != "body") continue;
                            if (!fixture.collision.TryGetShapeWorldCenter(contact.ownerId, contact.ownerShapeIndex, out var sourceCenter)) continue;
                            Vector2 sourceBearing = sourceCenter - (Vector2)receiverRoot.position;
                            if (sourceBearing.sqrMagnitude < 1e-8f) continue;
                            float incomingBearing = Mathf.Atan2(sourceBearing.y, sourceBearing.x) * Mathf.Rad2Deg;
                            foreach (float delta in GapRotationDeltas(part, receiverTemplate, state, part.id, incomingBearing))
                            {
                                if (!seenGapDeltas.Add(Mathf.RoundToInt(delta * 10f))) continue;
                                yield return new SolverAction { kind = "rotate", part = part.id, delta = delta, guided = true };
                            }
                        }

                        // A slider can already cross an unlinked ring body at the initial
                        // pose. Treat nearby slider body colliders as possible gap occupants
                        // too; the native open-gap query tests pins in the receiver gap even
                        // before they become authored head-trigger bindings.
                        foreach (var obstacle in fixture.level.parts)
                        {
                            if (obstacle.id == part.id || state.removed.Contains(obstacle.id) || !RotateRingsRules.IsSlider(obstacle)) continue;
                            var obstacleTemplate = fixture.templates.templates.FirstOrDefault(template => template.id == obstacle.templateId);
                            float reach = (receiverTemplate.visualRadius + (obstacleTemplate == null ? 1f : obstacleTemplate.visualRadius) + 1f);
                            foreach (float incomingBearing in SliderGapBearings(fixture, obstacle, receiverRoot, reach))
                            {
                                foreach (float delta in GapRotationDeltas(part, receiverTemplate, state, part.id, incomingBearing))
                                {
                                    if (!seenGapDeltas.Add(Mathf.RoundToInt(delta * 10f))) continue;
                                    yield return new SolverAction { kind = "rotate", part = part.id, delta = delta, guided = true };
                                }
                            }
                        }
                    }
                }
                float[] magnitudes = { 5f, 10f, 15f, 30f, 45f, 60f, 90f, 120f, 180f, 270f, 360f };
                foreach (float angle in magnitudes)
                {
                    yield return new SolverAction { kind = "rotate", part = part.id, delta = angle };
                    yield return new SolverAction { kind = "rotate", part = part.id, delta = -angle };
                }
            }
            else
            {
                float current = state.slides.TryGetValue(part.id, out float offset) ? offset : 0f;
                float[] baseTravels = { .06f, .12f, .24f, .36f, .6f, 1.2f, 2.4f, 4.8f, 9.6f };
                foreach (float distance in baseTravels.Concat(new[] { NativeSliderReach(fixture, part, state) }).Distinct().OrderBy(value => value))
                {
                    float positiveTarget = current + distance, negativeTarget = current - distance;
                    yield return new SolverAction { kind = "slide", part = part.id, targetSlide = positiveTarget };
                    yield return new SolverAction { kind = "slide", part = part.id, targetSlide = negativeTarget };
                }
            }
        }
    }

    static bool Replay(RotateRingsLevel level, RotateRingsTemplateCatalog templates, List<SolverAction> actions, out int replayed, out string error)
    {
        replayed = 0; error = null;
        using (var fixture = new FixtureWorld(level, templates))
        {
            var state = fixture.InitialState();
            foreach (var action in actions)
            {
                fixture.RestoreBranch(state);
                float angleBefore = fixture.collision.GetAngle(action.part);
                float slideBefore = fixture.collision.GetSlide(action.part);
                int bindingsBefore = fixture.collision.ActiveAuthoredContactCount;
                var removedBefore = new HashSet<int>(state.removed);
                if (!fixture.ApplyAction(state, action, out bool blocked))
                {
                    CaptureReplayWitness(action, fixture, state, angleBefore, slideBefore, bindingsBefore, removedBefore, blocked);
                    error = "action could not be applied: " + action.kind + " part " + action.part;
                    return false;
                }
                state.bindings = CloneBindingSnapshot(fixture.collision.CaptureBindingState());
                CaptureReplayWitness(action, fixture, state, angleBefore, slideBefore, bindingsBefore, removedBefore, blocked);
                replayed++;
            }
            return fixture.IsGoal(state);
        }
    }

    static void CaptureReplayWitness(SolverAction action, FixtureWorld fixture, SearchState state,
        float angleBefore, float slideBefore, int bindingsBefore, HashSet<int> removedBefore, bool blocked)
    {
        action.replayWitnessCaptured = true;
        action.actualAngleBeforeDegrees = angleBefore;
        action.actualAngleAfterDegrees = fixture.collision.GetAngle(action.part);
        action.actualSlideBefore = slideBefore;
        action.actualSlideAfter = fixture.collision.GetSlide(action.part);
        action.actualMovementConsumed = action.kind == "slide"
            ? action.actualSlideAfter - action.actualSlideBefore
            : action.actualAngleAfterDegrees - action.actualAngleBeforeDegrees;
        action.activeBindingCountBefore = bindingsBefore;
        action.activeBindingCountAfter = fixture.collision.ActiveAuthoredContactCount;
        action.removedIdsBefore = removedBefore.OrderBy(id => id).ToArray();
        action.removedIdsAfter = state.removed.OrderBy(id => id).ToArray();
        action.removedIdsDelta = state.removed.Except(removedBefore).OrderBy(id => id).ToArray();
        action.replayBlocked = blocked;
        action.replayBlockerOwner = fixture.collision.LastBlockerOwner;
        action.replayBlockerShapeIndex = fixture.collision.LastBlockerShapeIndex;
        action.replayBlockerRole = fixture.collision.LastBlockerRole;
    }

    static string StateKey(RotateRingsLevel level, SearchState state)
    {
        var key = new System.Text.StringBuilder();
        foreach (var part in level.parts.OrderBy(p => p.id))
        {
            key.Append(part.id).Append(':');
            if (state.removed.Contains(part.id)) { key.Append('x'); continue; }
            if (RotateRingsRules.IsSlider(part))
            {
                float value = state.slides.TryGetValue(part.id, out float slide) ? slide : 0f;
                key.Append(Mathf.RoundToInt(value * 100f));
            }
            else key.Append(Mathf.RoundToInt(Mathf.Repeat(state.angles[part.id], 360f) * 2f));
            key.Append(',');
            key.Append(state.chainLocks.TryGetValue(part.id, out int count) ? count : 0).Append(';');
        }
        key.Append("K"); foreach (int id in state.keys.OrderBy(v => v)) key.Append(id).Append(',');
        key.Append("B"); foreach (int id in state.spentBombs.OrderBy(v => v)) key.Append(id).Append(',');
        var snapshot = state.bindings as RotateRingsCollisionWorld.BindingStateSnapshot;
        if (snapshot != null)
        {
            key.Append("R"); if (snapshot.activeRingRelationIndices != null) foreach (int id in snapshot.activeRingRelationIndices.OrderBy(v => v)) key.Append(id).Append(',');
            key.Append("P"); if (snapshot.passiveRingRelationIndices != null) foreach (int id in snapshot.passiveRingRelationIndices.OrderBy(v => v)) key.Append(id).Append(',');
            key.Append("C");
            if (snapshot.activeRingContactOwners != null && snapshot.activeRingContactColliderIds != null)
            {
                int pairs = Math.Min(snapshot.activeRingContactOwners.Length, snapshot.activeRingContactColliderIds.Length) / 2;
                var contacts = new List<string>(pairs);
                for (int i = 0; i < pairs; i++) contacts.Add(snapshot.activeRingContactOwners[i * 2] + ":" + snapshot.activeRingContactColliderIds[i * 2] + ">" + snapshot.activeRingContactOwners[i * 2 + 1] + ":" + snapshot.activeRingContactColliderIds[i * 2 + 1]);
                foreach (string contact in contacts.OrderBy(v => v, StringComparer.Ordinal)) key.Append(contact).Append(',');
            }
            key.Append("G"); if (snapshot.activeGuideRelationKeys != null) foreach (string id in snapshot.activeGuideRelationKeys.OrderBy(v => v)) key.Append(id).Append(',');
        }
        return key.ToString();
    }

    static object CloneBindingSnapshot(object value)
    {
        var source = value as RotateRingsCollisionWorld.BindingStateSnapshot;
        if (source == null) return null;
        return new RotateRingsCollisionWorld.BindingStateSnapshot
        {
            activeRingRelationIndices = source.activeRingRelationIndices == null ? null : (int[])source.activeRingRelationIndices.Clone(),
            passiveRingRelationIndices = source.passiveRingRelationIndices == null ? null : (int[])source.passiveRingRelationIndices.Clone(),
            activeGuideRelationKeys = source.activeGuideRelationKeys == null ? null : (string[])source.activeGuideRelationKeys.Clone(),
            activeRingContactOwners = source.activeRingContactOwners == null ? null : (int[])source.activeRingContactOwners.Clone(),
            activeRingContactColliderIds = source.activeRingContactColliderIds == null ? null : (long[])source.activeRingContactColliderIds.Clone()
        };
    }
}
#endif

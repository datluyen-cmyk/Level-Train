#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

// Player policies receive only visible poses/pin positions and their own feedback.
// Fixture state, bindings, solver paths, transition cache and goal checks stay here.
public static partial class SourceSolverV3
{
    [Serializable] public sealed class PlayerPairV5
    {
        public int referenceId, candidateId;
        public string referencePairId;
    }
    [Serializable] public sealed class PlayerRequestV5
    {
        public int schemaVersion = 5, users = 32, seed = 20261106, stepLimit = 48;
        public int wallTimeLimitMs = 10000, transitionCacheLimit = 50000;
        public string sourcePackPath, candidatePackPath, templatesPath, phase = "pilot";
        public string policyVariant = "legacy-stochastic-v1";
        public PlayerPairV5[] pairs;
    }
    [Serializable] public sealed class PlayerProfileV5
    {
        public string profileId, policyFamily, partition;
        public int seed, memory, patience;
        public float skill, exploration, mistakeRate;
    }
    [Serializable] public sealed class PlayerStepV5
    {
        public int step, part;
        public string kind, visibleFeedback;
        public float requestedDelta, targetSlide, reachedAngleBefore, reachedAngleAfter, reachedSlideBefore, reachedSlideAfter;
        public bool physicalBlocked;
        public int[] removedDelta;
    }
    [Serializable] public sealed class PlayerSessionV5
    {
        public string sessionId, profileId, policyFamily, cohortId, role, referencePairId, levelSha256, termination, error;
        public int levelId, sessionSeed, actionCount, physicalBlocks, invalidActions, noEffectActions, hints, undoCount, cacheHits, cacheMisses;
        public bool won;
        public long elapsedWallMs;
        public float cacheHitRate;
        public List<PlayerStepV5> actions = new List<PlayerStepV5>();
    }
    [Serializable] public sealed class PlayerReportV5
    {
        public int schemaVersion = 5, completedSessions, totalSessions;
        public string suiteId = "visible-native-bots-v5", runId, generatedUtc, unityVersion, policySuiteHash, rulesHash;
        public string inputPackSha256, candidatePackSha256, templatesSha256, cacheSchema = "exact-float-state-bindings-action-per-level-v1";
        public string semantics = "Seeded uncalibrated virtual players, not humans; visible-geometry policies never receive stored solutions/bindings; policy failures do not prove unsolvability; no hints or undo modeled. Observation limitation: policies see surviving part kind, pose, radius, gap, and attachment pin points; they do not observe native mesh silhouettes/catch features, bomb visuals/state, or chain-lock cues.";
        public bool complete;
        public string status, sourceStage = "source-vs-constructive only; missing candidate means reference-only pilot";
        public float fixedDeltaTime = 1f / 60f;
        public PlayerRequestV5 config;
        public List<PlayerProfileV5> profiles = new List<PlayerProfileV5>();
        public List<PlayerSessionV5> sessions = new List<PlayerSessionV5>();
    }
    sealed class VisiblePartV5
    {
        public int id;
        public string kind;
        public Vector2 center;
        public float angle, slide, radius, gap, gapDirection;
        public Vector2[] pins;
    }
    sealed class VisibleActionV5
    {
        public SolverAction action;
        public float salience;
        public string key;
    }
    sealed class PlayerTransitionV5
    {
        public SearchState next;
        public bool changed, blocked, rejectedInput;
        public float beforeAngle, afterAngle, beforeSlide, afterSlide;
        public int[] removed;
    }
    sealed class PlayerTaskV5
    {
        public RotateRingsLevel level;
        public PlayerPairV5 pair;
        public string role;
    }
    sealed class PlayerJobV5
    {
        public string output;
        public PlayerReportV5 report;
        public RotateRingsTemplateCatalog templates;
        public List<PlayerTaskV5> tasks;
        public int taskIndex, userIndex;
        public FixtureWorld fixture;
        public SearchState initial;
        public Dictionary<string, PlayerTransitionV5> cache = new Dictionary<string, PlayerTransitionV5>();
    }
    static PlayerJobV5 playerJobV5;

    public static string StartPlayerBatchV5(string requestPath, string responsePath)
    {
        if (playerJobV5 != null || activeBatch != null) throw new InvalidOperationException("Finish the current native QA batch first.");
        if (File.Exists(responsePath)) throw new InvalidOperationException("Refuse to overwrite native player evidence.");
        var request = JsonUtility.FromJson<PlayerRequestV5>(File.ReadAllText(requestPath));
        if (request == null || request.schemaVersion != 5 || request.users < 1 || request.stepLimit < 1 || request.pairs == null || request.pairs.Length == 0)
            throw new InvalidDataException("Invalid native virtual-player request.");
        var sources = JsonUtility.FromJson<RotateRingsLevelPack>(File.ReadAllText(request.sourcePackPath));
        var candidates = string.IsNullOrEmpty(request.candidatePackPath) ? null : JsonUtility.FromJson<RotateRingsLevelPack>(File.ReadAllText(request.candidatePackPath));
        var templates = JsonUtility.FromJson<RotateRingsTemplateCatalog>(File.ReadAllText(request.templatesPath));
        var tasks = new List<PlayerTaskV5>();
        foreach (var pair in request.pairs)
        {
            var reference = sources.levels.Single(level => level.id == pair.referenceId);
            tasks.Add(new PlayerTaskV5 { level = reference, pair = pair, role = "reference" });
            if (candidates != null)
                tasks.Add(new PlayerTaskV5 { level = candidates.levels.Single(level => level.id == pair.candidateId), pair = pair, role = "candidate" });
        }
        string editorRoot = Path.Combine(Application.dataPath, "RotateRings/Editor");
        var report = new PlayerReportV5 {
            runId = Path.GetFileNameWithoutExtension(responsePath), generatedUtc = DateTime.UtcNow.ToString("o"),
            unityVersion = Application.unityVersion, rulesHash = HashRuleBundle(),
            policySuiteHash = PlayerFileHashV5(Path.Combine(editorRoot, "SourceVirtualPlaytestV5.cs")),
            inputPackSha256 = PlayerFileHashV5(request.sourcePackPath), templatesSha256 = PlayerFileHashV5(request.templatesPath),
            candidatePackSha256 = candidates == null ? "" : PlayerFileHashV5(request.candidatePackPath),
            config = request, totalSessions = request.users * tasks.Count, status = "running"
        };
        if (request.policyVariant != "legacy-stochastic-v1" && request.policyVariant != "cached-visible-v2")
            throw new InvalidDataException("Unknown virtual-player policy variant: " + request.policyVariant);
        for (int i = 0; i < request.users; i++) report.profiles.Add(CreatePlayerProfileV5(i, request.seed, request.policyVariant));
        playerJobV5 = new PlayerJobV5 { output = responsePath, report = report, templates = templates, tasks = tasks };
        SavePlayerV5();
        EditorApplication.update -= TickPlayerV5; EditorApplication.update += TickPlayerV5;
        return "running; profiles=" + request.users + "; sessions=" + report.totalSessions;
    }

    public static string GetPlayerBatchStatusV5()
    {
        if (playerJobV5 == null) return "idle";
        return "running; completed=" + playerJobV5.report.completedSessions + "/" + playerJobV5.report.totalSessions;
    }

    public static string StopPlayerBatchV5()
    {
        if (playerJobV5 == null) return "idle";
        playerJobV5.report.status = "stopped-incomplete"; SavePlayerV5();
        playerJobV5.fixture?.Dispose(); playerJobV5 = null; EditorApplication.update -= TickPlayerV5;
        return "stopped; incomplete evidence retained";
    }

    static PlayerProfileV5 CreatePlayerProfileV5(int i, int seed)
    {
        return CreatePlayerProfileV5(i, seed, "legacy-stochastic-v1");
    }

    static PlayerProfileV5 CreatePlayerProfileV5(int i, int seed, string policyVariant)
    {
        var rng = new System.Random(unchecked(seed + i * 7919));
        int family = i % 5;
        string[] names = { "exploratory", "visual_alignment", "persistent_memory", "quick_trial", "heldout_balanced" };
        bool cachedVisible = policyVariant == "cached-visible-v2";
        return new PlayerProfileV5 { profileId = "virtual-" + i.ToString("0000"), seed = unchecked(seed + i * 7919),
            policyFamily = names[family], partition = family == 4 ? "holdout" : "selection",
            skill = (float)(family == 0 ? .15 + .35*rng.NextDouble() : family == 1 ? .65 + .3*rng.NextDouble() : .35 + .5*rng.NextDouble()),
            exploration = (float)(family == 0 ? .5 + .4*rng.NextDouble() : .05 + .35*rng.NextDouble()),
            mistakeRate = (float)(family == 3 ? .15 + .2*rng.NextDouble() : .01 + .15*rng.NextDouble()),
            memory = family == 3 ? 2 + rng.Next(5) : cachedVisible && family == 2 ? 40 + rng.Next(41) : 8 + rng.Next(40),
            patience = family == 3 ? 8 + rng.Next(12) : cachedVisible && family == 2 ? 40 + rng.Next(41) : 20 + rng.Next(40) };
    }

    static List<VisiblePartV5> ObservePlayerV5(FixtureWorld fixture, SearchState state)
    {
        var result = new List<VisiblePartV5>();
        foreach (var p in fixture.level.parts)
        {
            if (state.removed.Contains(p.id)) continue;
            var t = fixture.templates.templates.Single(template => template.id == p.templateId);
            float angle = state.angles[p.id], slide = state.slides.TryGetValue(p.id, out float s) ? s : 0f;
            Vector2 center = new Vector2(p.x, p.y);
            if (RotateRingsRules.IsSlider(p)) center += RotateRingsRules.SlideAxis(p, t)*slide;
            var pins = new List<Vector2>();
            if (t.attachments != null)
                foreach (var a in t.attachments)
                {
                    float radians = (a.angleDeg + angle)*Mathf.Deg2Rad;
                    float distance = t.visualRadius + (a.isInnerMount ? -1f : 1f)*(t.visualWidth*.5f + a.stemLength);
                    pins.Add(center + new Vector2(Mathf.Cos(radians), Mathf.Sin(radians))*distance);
                }
            result.Add(new VisiblePartV5 { id = p.id, kind = p.kind, center = center, angle = angle, slide = slide,
                radius = t.visualRadius, gap = t.visualGap, gapDirection = t.gapDirectionDeg, pins = pins.ToArray() });
        }
        return result;
    }

    static List<VisibleActionV5> VisibleActionsV5(List<VisiblePartV5> visible, PlayerProfileV5 profile)
    {
        var actions = new List<VisibleActionV5>();
        foreach (var p in visible)
        {
            if (p.kind != "ring" && p.kind != "slider") continue;
            void Add(string kind, float delta, float target, float salience)
            {
                var action = new SolverAction { kind = kind, part = p.id, delta = delta, targetSlide = target };
                string key = p.id + ":" + kind + ":" + delta.ToString("R", CultureInfo.InvariantCulture) + ":" + target.ToString("R", CultureInfo.InvariantCulture);
                if (!actions.Any(a => a.key == key)) actions.Add(new VisibleActionV5 { action = action, salience = salience, key = key });
            }
            Add("tap", 0, p.slide, .3f);
            if (p.kind == "slider")
            {
                foreach (float travel in new[] { -4.8f, -1.2f, 1.2f, 4.8f }) Add("slide", 0, p.slide+travel, 1f);
                continue;
            }
            foreach (float delta in new[] { -180f, -90f, -30f, 30f, 90f, 180f }) Add("rotate", delta, 0, .6f);
            // Pin locations are visible mesh features. No authored relation,
            // CanSolve, HasOutgoingBinding or future action outcome is queried.
            foreach (var other in visible)
            {
                if (p.id == other.id) continue;
                foreach (var pin in other.pins)
                {
                    Vector2 d = pin-p.center;
                    if (p.radius <= 0 || Mathf.Abs(d.magnitude-p.radius) > p.radius*.5f) continue;
                    float delta = Mathf.DeltaAngle(p.angle+p.gapDirection, Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg);
                    if (Mathf.Abs(delta) > .1f) Add("rotate", delta, 0, 1f + 3f*profile.skill);
                }
            }
        }
        return actions;
    }

    static string ExactPlayerStateV5(SearchState state)
    {
        var key = new StringBuilder();
        foreach (var angle in state.angles.OrderBy(p => p.Key))
        {
            key.Append(angle.Key).Append(':').Append(angle.Value.ToString("R",CultureInfo.InvariantCulture)).Append('/');
            key.Append(state.slides.TryGetValue(angle.Key,out float slide) ? slide.ToString("R",CultureInfo.InvariantCulture) : "none").Append(';');
        }
        key.Append("removed:").Append(string.Join(",",state.removed.OrderBy(i=>i)));
        key.Append("keys:").Append(string.Join(",",state.keys.OrderBy(i=>i)));
        key.Append("bombs:").Append(string.Join(",",state.spentBombs.OrderBy(i=>i)));
        foreach(var pair in state.chainLocks.OrderBy(p=>p.Key)) key.Append("lock:").Append(pair.Key).Append('=').Append(pair.Value);
        key.Append("bindings:").Append(state.bindings==null ? "null" : JsonUtility.ToJson(state.bindings));
        return key.ToString();
    }

    static PlayerSessionV5 PlayEpisodeV5(PlayerJobV5 job, PlayerTaskV5 task, PlayerProfileV5 profile)
    {
        var config = job.report.config;
        int pairSeed = unchecked(profile.seed ^ (task.pair.referenceId * 48611));
        var rng = new System.Random(pairSeed);
        var row = new PlayerSessionV5 { sessionId = task.pair.referencePairId+"-"+task.role+"-"+profile.profileId,
            profileId = profile.profileId, policyFamily = profile.policyFamily, cohortId = config.phase,
            role = task.role, referencePairId = task.pair.referencePairId, levelId = task.level.id,
            sessionSeed = pairSeed, levelSha256 = PlayerTextHashV5(JsonUtility.ToJson(task.level)), termination = "step_limit" };
        var state = job.initial.Copy();
        var recent = new Queue<string>(); var failures = new HashSet<string>();
        int stagnant = 0;
        var clock = Stopwatch.StartNew();
        try
        {
            for (int step = 0; step < config.stepLimit; step++)
            {
                if (clock.ElapsedMilliseconds > config.wallTimeLimitMs) { row.termination = "wall_time"; break; }
                var observation = ObservePlayerV5(job.fixture, state);
                var choices = VisibleActionsV5(observation, profile);
                if (choices.Count == 0) { row.termination = "no_available_action"; break; }
                // History key contains public poses only, never private binding/cache state.
                string observationKey = string.Join(";",observation.Select(p=>p.id+":"+p.angle.ToString("R")+":"+p.slide.ToString("R")));
                VisibleActionV5 chosen;
                if (config.policyVariant == "cached-visible-v2")
                {
                    bool explore = rng.NextDouble() < profile.exploration;
                    bool makeMistake = rng.NextDouble() < profile.mistakeRate;
                    if (explore || makeMistake) chosen = choices[rng.Next(choices.Count)];
                    else
                    {
                        // Keep salience ordering stable across profiles for cache reuse.
                        // Only remembered no-change feedback penalizes an action.
                        var ranked = choices
                            .Select(c => new { choice = c, value = c.salience - (failures.Contains(observationKey + c.key) ? 1.25f : 0f) })
                            .OrderByDescending(v => v.value)
                            .ThenBy(v => v.choice.key, StringComparer.Ordinal)
                            .ToList();
                        chosen = ranked[0].choice;
                    }
                }
                else
                {
                    // Preserve the original policy path and random draw order for legacy reports.
                    if (rng.NextDouble() < profile.exploration || rng.NextDouble() < profile.mistakeRate)
                        chosen = choices[rng.Next(choices.Count)];
                    else
                    {
                        var ranked = choices.Select(c => new { choice=c, value=(failures.Contains(observationKey+c.key) ? .03f : c.salience)*(float)(.5+rng.NextDouble()) }).OrderByDescending(v=>v.value).ToList();
                        chosen = ranked[0].choice;
                    }
                }
                string stateKey = ExactPlayerStateV5(state)+"|"+chosen.key;
                PlayerTransitionV5 transition;
                if (job.cache.TryGetValue(stateKey,out transition)) row.cacheHits++;
                else
                {
                    row.cacheMisses++;
                    job.fixture.RestoreBranch(state);
                    var next = state.Copy();
                    var inputPart=task.level.parts.Single(p=>p.id==chosen.action.part);
                    bool rejectedInput=job.fixture.IsInputLocked(state,inputPart)||
                        (!RotateRingsRules.IsSlider(inputPart)&&job.fixture.collision.HasOutgoingBinding(inputPart.id));
                    float ba = job.fixture.collision.GetAngle(chosen.action.part), bs = job.fixture.collision.GetSlide(chosen.action.part);
                    bool changed = job.fixture.ApplyAction(next, chosen.action, out bool blocked);
                    transition = new PlayerTransitionV5 { next=next.Copy(), changed=changed, blocked=blocked,rejectedInput=rejectedInput,
                        beforeAngle=ba, beforeSlide=bs, afterAngle=job.fixture.collision.GetAngle(chosen.action.part),
                        afterSlide=job.fixture.collision.GetSlide(chosen.action.part), removed=next.removed.Except(state.removed).OrderBy(i=>i).ToArray() };
                    if(job.cache.Count < config.transitionCacheLimit) job.cache.Add(stateKey,transition);
                }
                row.actions.Add(new PlayerStepV5 { step=step,part=chosen.action.part,kind=chosen.action.kind,requestedDelta=chosen.action.delta,
                    targetSlide=chosen.action.targetSlide,reachedAngleBefore=transition.beforeAngle,reachedAngleAfter=transition.afterAngle,
                    reachedSlideBefore=transition.beforeSlide,reachedSlideAfter=transition.afterSlide,physicalBlocked=transition.blocked,
                    removedDelta=transition.removed,visibleFeedback=transition.removed.Length>0 ? "pieces removed" : transition.changed ? "pose changed" : "no visible change" });
                if(transition.blocked) row.physicalBlocks++;
                if(transition.rejectedInput)row.invalidActions++;
                if(!transition.changed)
                {
                    row.noEffectActions++;
                    string memoryKey=observationKey+chosen.key; failures.Add(memoryKey);recent.Enqueue(memoryKey);
                    while(recent.Count>profile.memory)failures.Remove(recent.Dequeue());
                }
                state=transition.next.Copy();
                stagnant=transition.removed.Length>0 ? 0 : stagnant+1;
                if(job.fixture.IsGoal(state)){row.won=true;row.termination="won";break;}
                if(stagnant>=profile.patience){row.termination="dropout";break;}
            }
        }
        catch(Exception ex){row.termination="engine_error";row.error=ex.ToString();}
        row.actionCount=row.actions.Count;row.elapsedWallMs=clock.ElapsedMilliseconds;
        row.cacheHitRate=(float)row.cacheHits/Math.Max(1,row.cacheHits+row.cacheMisses);
        return row;
    }

    static void TickPlayerV5()
    {
        var job=playerJobV5;
        if(job==null){EditorApplication.update-=TickPlayerV5;return;}
        if(job.taskIndex>=job.tasks.Count)
        {
            job.report.complete=true;job.report.status="complete";SavePlayerV5();
            job.fixture?.Dispose();playerJobV5=null;EditorApplication.update-=TickPlayerV5;return;
        }
        var task=job.tasks[job.taskIndex];
        try
        {
            if(job.fixture==null){job.fixture=new FixtureWorld(task.level,job.templates);job.initial=job.fixture.InitialState();job.cache.Clear();}
            var row=PlayEpisodeV5(job,task,job.report.profiles[job.userIndex]);job.report.sessions.Add(row);
            job.report.completedSessions++;job.userIndex++;
            if(job.userIndex>=job.report.profiles.Count)
            {
                job.fixture.Dispose();job.fixture=null;job.cache.Clear();job.userIndex=0;job.taskIndex++;
            }
            if(job.report.completedSessions%25==0||job.userIndex==0)SavePlayerV5();
        }
        catch(Exception ex)
        {
            job.report.status="engine_error; incomplete: "+ex;SavePlayerV5();
            job.fixture?.Dispose();playerJobV5=null;EditorApplication.update-=TickPlayerV5;
        }
    }

    static void SavePlayerV5()
    {
        var job=playerJobV5;Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(job.output)));
        string temp=job.output+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(job.report));
        if(File.Exists(job.output))File.Delete(job.output);File.Move(temp,job.output);
    }
    static string PlayerFileHashV5(string path)=>PlayerTextHashV5(File.ReadAllBytes(path));
    static string PlayerTextHashV5(string value)=>PlayerTextHashV5(Encoding.UTF8.GetBytes(value));
    static string PlayerTextHashV5(byte[] value)
    {using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(value)).Replace("-","").ToLowerInvariant();}
}
#endif

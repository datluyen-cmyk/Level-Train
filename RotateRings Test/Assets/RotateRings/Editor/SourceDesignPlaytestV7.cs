#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Human-directed source-design playtest harness. It loads a full original source pack
/// into the live runtime and submits sampled gestures through the native collision and
/// release pipeline. Calls are programmatic; this does not exercise OS pointer/raycast input.
/// </summary>
public static class SourceDesignPlaytestV7
{
    const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    const string SessionPathKey = "RotateRings.SourceDesignPlaytestV7.ReportPath";
    const float GestureSampleDegrees = 5f;
    const float SampleDeltaTime = 1f / 60f;

    static RotateRingsRuntime runtime;
    static RotateRingsLevelPack pack;
    static RotateRingsTemplateCatalog templates;
    static Report report;
    static string outputPath;

    [Serializable] public sealed class Report
    {
        public int schemaVersion = 7;
        public string generatedUtc, label, unityVersion, sourcePackPath, templatesPath, outputReportPath;
        public string sourcePackSha256, templatesSha256, rulesSha256, runtimeSha256, scriptSha256;
        public string gameplayMethodology = "Programmatic native gestures: call the live runtime's native collision clamps, motion samples, release, cascade, and goal logic. This does not exercise OS pointer/raycast input and is not an optimal-move proof.";
        public int levelId, position;
        public string title;
        public float difficulty;
        public List<StepRecord> steps = new List<StepRecord>();
    }

    [Serializable] public sealed class StepRecord
    {
        public int index, partId, moveCountBefore, moveCountAfter;
        public string action, outcome, rejectionReason, blockerRole;
        public float requestedAmount, reachedBefore, reachedAfter;
        public int blockerOwner = -1, removedCountBefore, removedCountAfter;
        public int[] removedDelta = Array.Empty<int>();
        public bool accepted, won;
        public Observation before, after;
    }

    [Serializable] public sealed class Observation
    {
        public int levelId, position, moves, remainingParts;
        public float difficulty;
        public bool won;
        public int activeAuthoredBindingCount, activeRingLinkCount, activeGuideLinkCount;
        public List<OwnerPose> poses = new List<OwnerPose>();
        public List<ContactPose> contacts = new List<ContactPose>();
    }

    [Serializable] public sealed class OwnerPose
    {
        public int ownerId, chainLockCount, lockKey;
        public string kind, templateId;
        public bool removed, locked, hasActiveOutgoing, hasActiveIncident, keyCollected;
        public float x, y, z, angle, slide, radius, width, gap, gapDirectionDeg;
        public List<ShapePose> shapes = new List<ShapePose>();
    }

    [Serializable] public sealed class ShapePose
    {
        public string role;
        public float x, y, z, width, height;
    }

    [Serializable] public sealed class ContactPose
    {
        public string kind, ownerRole, targetRole, featureId;
        public int ownerId, targetId, ownerShapeIndex, targetShapeIndex;
    }

    public static string Load(int sourceId, string sourcePackPath, string templatesPath, string outputReportPath, string label)
    {
        RequirePlayMode();
        if (File.Exists(Path.GetFullPath(outputReportPath))) throw new IOException("Refusing to overwrite an existing playtest report: " + outputReportPath);
        runtime = UnityEngine.Object.FindFirstObjectByType<RotateRingsRuntime>();
        if (runtime == null) throw new InvalidOperationException("A live RotateRingsRuntime is required in Play Mode.");

        string fullPackPath = Path.GetFullPath(sourcePackPath);
        string fullTemplatesPath = Path.GetFullPath(templatesPath);
        pack = JsonUtility.FromJson<RotateRingsLevelPack>(File.ReadAllText(fullPackPath));
        templates = JsonUtility.FromJson<RotateRingsTemplateCatalog>(File.ReadAllText(fullTemplatesPath));
        ValidateCorpus(pack, templates, sourceId);

        SetRuntimeField("pack", pack);
        SetRuntimeField("templates", templates);
        SetRuntimeField("referenceQa", true);
        SetRuntimeField("trainingPreview", false);
        InvokeRuntime("StartLevel", sourceId);

        outputPath = Path.GetFullPath(outputReportPath);
        string runtimePath = Path.Combine(Application.dataPath, "RotateRings/Scripts/RotateRingsRuntime.cs");
        string scriptPath = Path.Combine(Application.dataPath, "RotateRings/Editor/SourceDesignPlaytestV7.cs");
        var level = CurrentLevel();
        report = new Report {
            generatedUtc = DateTime.UtcNow.ToString("o"), label = string.IsNullOrWhiteSpace(label) ? DefaultLabel(sourceId) : label,
            unityVersion = Application.unityVersion, sourcePackPath = fullPackPath, templatesPath = fullTemplatesPath,
            outputReportPath = outputPath, sourcePackSha256 = HashFile(fullPackPath), templatesSha256 = HashFile(fullTemplatesPath),
            rulesSha256 = HashRuleBundle(), runtimeSha256 = HashFile(runtimePath), scriptSha256 = HashFile(scriptPath),
            levelId = level.id, position = level.position, title = level.title, difficulty = level.difficulty
        };
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
        SessionState.SetString(SessionPathKey, outputPath);
        SaveReport();
        return "Loaded original source " + sourceId + " (full source-460 corpus); report checkpoint: " + outputPath;
    }

    /// <summary>Returns a diagnostic observation of the current live level.</summary>
    public static string Observe()
    {
        EnsureAttached();
        return JsonUtility.ToJson(CaptureObservation(), true);
    }

    /// <summary>
    /// Submits one rotation (degrees) or slider displacement (world units) directly to the
    /// live native mechanics. Use kind="rotate" for rings and kind="slide" for sliders.
    /// </summary>
    public static string Step(int partId, string kind, float amount)
    {
        EnsureAttached();
        if (string.IsNullOrWhiteSpace(kind) || float.IsNaN(amount) || float.IsInfinity(amount)) throw new ArgumentException("Provide kind=rotate/slide and a finite amount.");
        var level = CurrentLevel();
        var state = GetRuntimeField("state");
        var part = level.parts.Find(p => p.id == partId);
        if (part == null) throw new ArgumentException("Unknown part id " + partId + " in level " + level.id + ".");
        bool slider = RotateRingsRules.IsSlider(part);
        string expectedKind = slider ? "slide" : "rotate";
        if (!string.Equals(kind, expectedKind, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Part " + partId + " requires kind=" + expectedKind + ".");

        var record = NewStep(partId, expectedKind, amount);
        record.before = CaptureObservation();
        record.moveCountBefore = ReadField<int>(state, "moves");
        var removedBefore = new HashSet<int>(ReadField<HashSet<int>>(state, "removed"));
        record.removedCountBefore = removedBefore.Count;
        float poseBefore = slider ? CurrentSlide(state, partId) : CurrentAngle(state, partId);
        record.reachedBefore = poseBefore;

        if (removedBefore.Contains(partId)) return FinishRecord(record, false, "part-already-removed", poseBefore, removedBefore);
        if (!RotateRingsRules.IsMovable(part)) return FinishRecord(record, false, "part-not-movable", poseBefore, removedBefore);
        if (InvokeRuntime<bool>("IsLocked", part)) {
            InvokeRuntime("Wobble", partId);
            return FinishRecord(record, false, "owner-locked", poseBefore, removedBefore);
        }
        if (!slider && InvokeRuntime<bool>("HasActiveOutgoing", partId)) {
            InvokeRuntime("Wobble", partId);
            return FinishRecord(record, false, "active-outgoing-binding", poseBefore, removedBefore);
        }

        BeginNativeGesture(part, slider, poseBefore);
        if (slider) ApplySliderTarget(partId, amount, record);
        else ApplyRotationTarget(partId, amount, record);

        if (ReadField<HashSet<int>>(state, "removed").Contains(partId))
        {
            // Mirror PollInput's native mid-gesture removal path (for example a bomb contact).
            WriteField(state, "moves", ReadField<int>(state, "moves") + 1);
            SetRuntimeField("drag", null);
            var world = GetRuntimeField("world");
            world.GetType().GetMethod("SetHint", InstanceFlags).Invoke(world, new object[] { -1 });
            InvokeRuntime("Cascade", -1);
            InvokeRuntime("UpdateWorld");
            InvokeRuntime("CheckWin");
        }
        else
        {
            // This is the runtime's exact native release/finalize path, including bomb probes,
            // solve eligibility, effects, cascade, victory checks and undo bookkeeping.
            InvokeRuntime("FinishDrag");
        }

        var removedAfter = new HashSet<int>(ReadField<HashSet<int>>(state, "removed"));
        record.moveCountAfter = ReadField<int>(state, "moves");
        record.removedCountAfter = removedAfter.Count;
        record.removedDelta = removedAfter.Except(removedBefore).OrderBy(id => id).ToArray();
        record.reachedAfter = slider ? CurrentSlide(state, partId) : CurrentAngle(state, partId);
        record.won = removedAfter.Count == level.parts.Count;
        bool poseChanged = slider
            ? Mathf.Abs(record.reachedAfter - record.reachedBefore) > .001f
            : Mathf.Abs(Mathf.DeltaAngle(record.reachedBefore, record.reachedAfter)) > .001f;
        record.outcome = record.won ? "won" : record.removedDelta.Length > 0 ? "removed" : poseChanged ? "moved" : "no-change";
        record.accepted = true;
        if (record.outcome == "no-change") record.rejectionReason = record.blockerOwner >= 0 ? "fully-blocked" : "no-effective-movement";
        record.after = CaptureObservation();
        return SaveRecord(record);
    }

    public static string Undo()
    {
        EnsureAttached();
        var record = NewStep(-1, "undo", 0f);
        record.before = CaptureObservation();
        InvokeRuntime("Undo");
        record.accepted = true; record.outcome = "undone"; record.after = CaptureObservation();
        record.moveCountBefore = record.before.moves; record.moveCountAfter = record.after.moves;
        record.removedCountBefore = record.before.poses.Count - record.before.remainingParts;
        record.removedCountAfter = record.after.poses.Count - record.after.remainingParts;
        return SaveRecord(record);
    }

    public static string Restart()
    {
        EnsureAttached();
        var record = NewStep(-1, "restart", 0f);
        record.before = CaptureObservation();
        InvokeRuntime("Restart");
        record.accepted = true; record.outcome = "restarted"; record.after = CaptureObservation();
        record.moveCountBefore = record.before.moves; record.moveCountAfter = record.after.moves;
        record.removedCountBefore = record.before.poses.Count - record.before.remainingParts;
        record.removedCountAfter = record.after.poses.Count - record.after.remainingParts;
        return SaveRecord(record);
    }

    public static string CurrentReportJson()
    {
        EnsureAttached();
        return JsonUtility.ToJson(report, true);
    }

    static void ValidateCorpus(RotateRingsLevelPack sourcePack, RotateRingsTemplateCatalog catalog, int sourceId)
    {
        if (sourcePack == null || sourcePack.schemaVersion != 3 || sourcePack.levels == null || sourcePack.levels.Count != 460)
            throw new InvalidDataException("Expected the schema-3 full ordinary source corpus with exactly 460 levels.");
        if (catalog == null || catalog.schemaVersion != 3 || catalog.templates == null || catalog.templates.Length == 0)
            throw new InvalidDataException("Expected a populated schema-3 source template catalog.");
        var level = sourcePack.levels.Find(item => item.id == sourceId);
        if (level == null || level.parts == null || level.parts.Count == 0) throw new ArgumentException("Source level not found or has no parts: " + sourceId);
        var templateIds = new HashSet<string>(catalog.templates.Where(t => t != null).Select(t => t.id));
        foreach (var part in level.parts)
            if (part == null || string.IsNullOrEmpty(part.templateId) || !templateIds.Contains(part.templateId))
                throw new InvalidDataException("Source level " + sourceId + " references a missing template.");
    }

    static void BeginNativeGesture(RotateRingsPart part, bool slider, float poseBefore)
    {
        var collision = GetRuntimeField("collision");
        collision.GetType().GetMethod("BeginGesture", InstanceFlags).Invoke(collision, new object[] { part.id });
        var world = GetRuntimeField("world");
        world.GetType().GetMethod("SetHint", InstanceFlags).Invoke(world, new object[] { part.id });
        var dragType = typeof(RotateRingsRuntime).GetNestedType("Drag", BindingFlags.NonPublic);
        if (dragType == null) throw new MissingMemberException("RotateRingsRuntime.Drag type is missing.");
        var incident = !slider && InvokeRuntime<bool>("HasActiveIncident", part.id);
        var drag = Activator.CreateInstance(dragType, true);
        WriteField(drag, "id", part.id); WriteField(drag, "slider", slider); WriteField(drag, "beganLinked", incident);
        WriteField(drag, "startSlide", slider ? poseBefore : 0f); WriteField(drag, "startAngle", slider ? 0f : poseBefore);
        WriteField(drag, "delta", slider ? poseBefore : 0f); WriteField(drag, "blockDirection", 0f); WriteField(drag, "valid", true);
        SetRuntimeField("drag", drag);
        InvokeRuntime("PushUndo");
    }

    static void ApplyRotationTarget(int owner, float amount, StepRecord record)
    {
        float remaining = amount;
        while (Mathf.Abs(remaining) > .00001f)
        {
            float delta = Mathf.Sign(remaining) * Mathf.Min(GestureSampleDegrees, Mathf.Abs(remaining));
            object drag = GetRuntimeField("drag");
            float blockDirection = ReadField<float>(drag, "blockDirection");
            if (delta * blockDirection <= 0f)
            {
                WriteField(drag, "blockDirection", 0f);
                var state = GetRuntimeField("state");
                var angles = ReadField<Dictionary<int, float>>(state, "angles");
                float before = angles[owner];
                var collision = GetRuntimeField("collision");
                object[] args = { owner, before, delta, false };
                float accepted = (float)collision.GetType().GetMethod("ClampRotation", InstanceFlags).Invoke(collision, args);
                bool blocked = (bool)args[3];
                angles[owner] = accepted;
                WriteField(drag, "delta", ReadField<float>(drag, "delta") + accepted - before);
                if (blocked) WriteField(drag, "blockDirection", Mathf.Sign(delta));
                CaptureBlocker(record, collision);
                InvokeRuntime("UpdateWorld");
            }
            remaining -= delta;
            if (ReadField<HashSet<int>>(GetRuntimeField("state"), "removed").Contains(owner)) break;
        }
    }

    static void ApplySliderTarget(int owner, float amount, StepRecord record)
    {
        var state = GetRuntimeField("state");
        var slides = ReadField<Dictionary<int, float>>(state, "slides");
        float desired = ReadSlide(slides, owner) + amount;
        for (int sample = 0; sample < 20000; sample++)
        {
            object drag = GetRuntimeField("drag");
            float from = ReadSlide(slides, owner);
            if (Mathf.Abs(desired - from) <= .00001f) break;
            var collision = GetRuntimeField("collision");
            object[] args = { owner, from, desired, SampleDeltaTime, false };
            float accepted = (float)collision.GetType().GetMethod("ClampSlideForDeltaTime", InstanceFlags).Invoke(collision, args);
            bool blocked = (bool)args[4];
            slides[owner] = accepted;
            WriteField(drag, "delta", accepted);
            if (blocked) WriteField(drag, "blockDirection", Mathf.Sign(desired - from));
            CaptureBlocker(record, collision);
            InvokeRuntime("UpdateWorld");
            if (ReadField<HashSet<int>>(state, "removed").Contains(owner)) break;
            if (blocked && Mathf.Abs(accepted - from) < .00001f) break;
            if (Mathf.Abs(accepted - from) < .00001f) break;
        }
    }

    static void CaptureBlocker(StepRecord record, object collision)
    {
        var type = collision.GetType();
        int owner = (int)type.GetProperty("LastBlockerOwner", InstanceFlags).GetValue(collision, null);
        if (owner >= 0) {
            record.blockerOwner = owner;
            record.blockerRole = (string)type.GetProperty("LastBlockerRole", InstanceFlags).GetValue(collision, null);
        }
    }

    static Observation CaptureObservation()
    {
        var level = CurrentLevel();
        var state = GetRuntimeField("state");
        var collision = GetRuntimeField("collision");
        var angles = ReadField<Dictionary<int, float>>(state, "angles");
        var slides = ReadField<Dictionary<int, float>>(state, "slides");
        var removed = ReadField<HashSet<int>>(state, "removed");
        var locks = ReadField<Dictionary<int, int>>(state, "chainLocks");
        var keys = ReadField<HashSet<int>>(state, "keys");
        var observation = new Observation {
            levelId = level.id, position = level.position, difficulty = level.difficulty,
            moves = ReadField<int>(state, "moves"), remainingParts = level.parts.Count - removed.Count, won = removed.Count == level.parts.Count
        };
        var catalogById = templates.templates.Where(t => t != null).ToDictionary(t => t.id, t => t);
        foreach (var part in level.parts)
        {
            catalogById.TryGetValue(part.templateId, out var template);
            var ownerTransform = (Transform)collision.GetType().GetMethod("OwnerTransform", InstanceFlags).Invoke(collision, new object[] { part.id });
            var pose = new OwnerPose {
                ownerId = part.id, kind = part.kind, templateId = part.templateId,
                x = ownerTransform == null ? part.x : ownerTransform.position.x,
                y = ownerTransform == null ? part.y : ownerTransform.position.y,
                z = ownerTransform == null ? part.z : ownerTransform.position.z,
                angle = angles.TryGetValue(part.id, out float a) ? a : part.angle,
                slide = ReadSlide(slides, part.id), removed = removed.Contains(part.id), lockKey = part.lockKey,
                chainLockCount = locks.TryGetValue(part.id, out int count) ? count : 0,
                keyCollected = part.lockKey == 0 || keys.Contains(part.lockKey), radius = template == null ? part.radius : template.radius,
                width = template == null ? part.width : template.width, gap = template == null ? part.gap : template.gap,
                gapDirectionDeg = template == null ? 0f : template.gapDirectionDeg,
                locked = InvokeRuntime<bool>("IsLocked", part),
                hasActiveOutgoing = InvokeRuntime<bool>("HasActiveOutgoing", part.id),
                hasActiveIncident = InvokeRuntime<bool>("HasActiveIncident", part.id)
            };
            var colliders = (List<Collider2D>)collision.GetType().GetMethod("GetOwnerColliders", InstanceFlags).Invoke(collision, new object[] { part.id });
            var roleMethod = collision.GetType().GetMethod("GetOwnerShapeRole", InstanceFlags);
            for (int i = 0; i < colliders.Count; i++)
            {
                var collider = colliders[i];
                if (collider == null || !collider.enabled) continue;
                var bounds = collider.bounds;
                pose.shapes.Add(new ShapePose {
                    role = (string)roleMethod.Invoke(collision, new object[] { part.id, i }),
                    x = bounds.center.x, y = bounds.center.y, z = bounds.center.z, width = bounds.size.x, height = bounds.size.y
                });
            }
            observation.poses.Add(pose);
        }

        var contacts = (System.Collections.IEnumerable)collision.GetType().GetMethod("GetActiveAuthoredContacts", InstanceFlags).Invoke(collision, null);
        foreach (var contact in contacts)
        {
            var c = contact.GetType();
            var item = new ContactPose {
                kind = ReadProperty<string>(contact, "kind"), ownerId = ReadProperty<int>(contact, "ownerId"), targetId = ReadProperty<int>(contact, "targetId"),
                ownerShapeIndex = ReadProperty<int>(contact, "ownerShapeIndex"), targetShapeIndex = ReadProperty<int>(contact, "targetShapeIndex"),
                ownerRole = ReadProperty<string>(contact, "ownerRole"), targetRole = ReadProperty<string>(contact, "targetRole"), featureId = ReadProperty<string>(contact, "featureId")
            };
            observation.contacts.Add(item);
        }
        observation.activeAuthoredBindingCount = (int)collision.GetType().GetProperty("ActiveAuthoredContactCount", InstanceFlags).GetValue(collision, null);
        observation.activeRingLinkCount = observation.contacts.Count(c => c.kind == "ring-link");
        observation.activeGuideLinkCount = observation.contacts.Count(c => c.kind == "guide-link");
        return observation;
    }

    static StepRecord NewStep(int partId, string action, float amount) => new StepRecord {
        index = report.steps.Count + 1, partId = partId, action = action, requestedAmount = amount, blockerOwner = -1
    };

    static string FinishRecord(StepRecord record, bool accepted, string reason, float reached, HashSet<int> removedBefore)
    {
        record.accepted = accepted; record.rejectionReason = reason; record.outcome = "rejected";
        record.reachedAfter = reached; record.moveCountAfter = ReadField<int>(GetRuntimeField("state"), "moves");
        record.removedCountAfter = removedBefore.Count; record.removedDelta = Array.Empty<int>(); record.after = CaptureObservation();
        return SaveRecord(record);
    }

    static string SaveRecord(StepRecord record)
    {
        if (record.before != null && record.moveCountBefore == 0) record.moveCountBefore = record.before.moves;
        if (record.before != null && record.removedCountBefore == 0) record.removedCountBefore = record.before.poses.Count - record.before.remainingParts;
        if (record.after != null && record.moveCountAfter == 0) record.moveCountAfter = record.after.moves;
        if (record.after != null && record.removedCountAfter == 0) record.removedCountAfter = record.after.poses.Count - record.after.remainingParts;
        report.steps.Add(record);
        SaveReport();
        return JsonUtility.ToJson(record, true);
    }

    static void SaveReport() => File.WriteAllText(outputPath, JsonUtility.ToJson(report, true), new UTF8Encoding(false));

    static void EnsureAttached()
    {
        RequirePlayMode();
        if (report == null)
        {
            outputPath = SessionState.GetString(SessionPathKey, "");
            if (string.IsNullOrEmpty(outputPath) || !File.Exists(outputPath)) throw new InvalidOperationException("No active SourceDesignPlaytestV7 report checkpoint. Call Load first.");
            report = JsonUtility.FromJson<Report>(File.ReadAllText(outputPath));
            if (report == null) throw new InvalidDataException("Could not restore the playtest report checkpoint.");
            if (report.steps == null) report.steps = new List<StepRecord>();
            pack = JsonUtility.FromJson<RotateRingsLevelPack>(File.ReadAllText(report.sourcePackPath));
            templates = JsonUtility.FromJson<RotateRingsTemplateCatalog>(File.ReadAllText(report.templatesPath));
        }
        if (runtime == null) runtime = UnityEngine.Object.FindFirstObjectByType<RotateRingsRuntime>();
        if (runtime == null) throw new InvalidOperationException("The playtest report survived a domain reload, but no live runtime is available.");
        if (string.IsNullOrEmpty(outputPath)) outputPath = report.outputReportPath;
        var level = CurrentLevel();
        if (level == null || level.id != report.levelId) throw new InvalidOperationException("Live runtime level no longer matches the checkpoint. Use Load to start a new report.");
    }

    static void RequirePlayMode() { if (!EditorApplication.isPlaying) throw new InvalidOperationException("SourceDesignPlaytestV7 requires Unity Play Mode."); }
    static RotateRingsLevel CurrentLevel() => (RotateRingsLevel)GetRuntimeField("level");
    static float CurrentAngle(object state, int id) => ReadField<Dictionary<int, float>>(state, "angles").TryGetValue(id, out float value) ? value : 0f;
    static float CurrentSlide(object state, int id) => ReadSlide(ReadField<Dictionary<int, float>>(state, "slides"), id);
    static float ReadSlide(Dictionary<int, float> slides, int id) => slides.TryGetValue(id, out float value) ? value : 0f;
    static string DefaultLabel(int sourceId)
    {
        switch (sourceId) {
            case 10060: return "hard-source-position-19";
            case 10069: return "hard-source-position-25";
            case 71009: return "hard-source-position-29";
            case 10061: return "hard-source-position-35";
            case 70033: return "very-hard-source-position-39";
            case 60011: return "very-hard-source-position-49";
            default: return "source-design-playtest";
        }
    }

    static string HashRuleBundle()
    {
        string root = Path.Combine(Application.dataPath, "RotateRings/Scripts");
        string[] files = { "RotateRingsRuntime.cs", "RotateRingsCollisionWorld.cs", "RotateRingsLevelData.cs", "RotateRingsWorldView.cs", "RotateRingsBombContacts.cs", "RotateRingsInputWorld.cs", "RotateRingsNativeInterlockPolicy.cs", "RotateRingsOriginalMotion.cs" };
        var text = new StringBuilder();
        foreach (var file in files.OrderBy(n => n, StringComparer.Ordinal)) text.Append(file).Append(':').Append(HashFile(Path.Combine(root, file))).Append('\n');
        return HashBytes(Encoding.UTF8.GetBytes(text.ToString()));
    }

    static string HashFile(string path) => HashBytes(File.ReadAllBytes(path));
    static string HashBytes(byte[] bytes)
    {
        using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
    }

    static object GetRuntimeField(string name) => typeof(RotateRingsRuntime).GetField(name, InstanceFlags).GetValue(runtime);
    static void SetRuntimeField(string name, object value) => typeof(RotateRingsRuntime).GetField(name, InstanceFlags).SetValue(runtime, value);
    static void WriteField(object target, string name, object value) => target.GetType().GetField(name, InstanceFlags).SetValue(target, value);
    static T ReadField<T>(object target, string name) => (T)target.GetType().GetField(name, InstanceFlags).GetValue(target);
    static T ReadProperty<T>(object target, string name) => (T)target.GetType().GetField(name, InstanceFlags).GetValue(target);

    static void InvokeRuntime(string method, params object[] arguments) => InvokeRuntime<object>(method, arguments);
    static T InvokeRuntime<T>(string method, params object[] arguments)
    {
        var target = typeof(RotateRingsRuntime).GetMethods(InstanceFlags)
            .Where(m => m.Name == method && m.GetParameters().Length >= arguments.Length)
            .FirstOrDefault(m => m.GetParameters().Skip(arguments.Length).All(p => p.IsOptional));
        if (target == null) throw new MissingMethodException(typeof(RotateRingsRuntime).FullName, method);
        var parameters = target.GetParameters();
        if (parameters.Length > arguments.Length) {
            var expanded = new object[parameters.Length];
            Array.Copy(arguments, expanded, arguments.Length);
            for (int i = arguments.Length; i < parameters.Length; i++) expanded[i] = parameters[i].DefaultValue;
            arguments = expanded;
        }
        var result = target.Invoke(runtime, arguments);
        return result == null ? default(T) : (T)result;
    }

}
#endif

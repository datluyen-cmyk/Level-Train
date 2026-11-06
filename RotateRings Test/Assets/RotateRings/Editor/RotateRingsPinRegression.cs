#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

/// <summary>Live regression for source-owned attachment geometry integrated into each part mesh.</summary>
public static class RotateRingsPinRegression {
    const string MenuPath = "Tools/Rotate Rings/QA/Physical Pin Regression";
    const string ReportPath = "Research/physics_v2/pin_visual_regression.json";
    const string ProgressKey = "rr_max_level";

    [Serializable]
    sealed class Assertion {
        public string name;
        public bool passed;
        public string detail;
    }

    [Serializable]
    sealed class Report {
        public bool passed;
        public string status;
        public string startedUtc;
        public string finishedUtc;
        public string unityVersion;
        public int levelId;
        public int sourceId;
        public int targetId;
        public string runtimeSha256;
        public string worldViewSha256;
        public string collisionWorldSha256;
        public string qaHarnessSha256;
        public string levelPackSha256;
        public string templateCatalogSha256;
        public bool playerPrefsRestored;
        public bool runInBackgroundPreserved;
        public List<Assertion> assertions = new List<Assertion>();
        public string error;
    }

    [MenuItem(MenuPath, true)]
    static bool ValidateRun() => EditorApplication.isPlaying && !EditorApplication.isPaused;

    [MenuItem(MenuPath)]
    static void RunMenu() {
        string path = Run();
        Debug.Log("Physical pin regression " + (File.Exists(path) ? "finished" : "failed to write report") + ": " + path);
    }

    public static string Run() {
        var report = new Report {
            status = "running",
            startedUtc = DateTime.UtcNow.ToString("o"),
            unityVersion = Application.unityVersion,
            levelId = 4,
            sourceId = 6,
            targetId = 7,
            runtimeSha256 = Hash(Path.Combine(Application.dataPath, "RotateRings/Scripts/RotateRingsRuntime.cs")),
            worldViewSha256 = Hash(Path.Combine(Application.dataPath, "RotateRings/Scripts/RotateRingsWorldView.cs")),
            collisionWorldSha256 = Hash(Path.Combine(Application.dataPath, "RotateRings/Scripts/RotateRingsCollisionWorld.cs")),
            qaHarnessSha256 = Hash(Path.Combine(Application.dataPath, "RotateRings/Editor/RotateRingsPinRegression.cs")),
            levelPackSha256 = Hash(Path.Combine(Application.dataPath, "RotateRings/Resources/Levels/generated_levels.json")),
            templateCatalogSha256 = Hash(Path.Combine(Application.dataPath, "RotateRings/Resources/Levels/physics_templates.json"))
        };
        string workspace = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
        string reportFile = Path.Combine(workspace, ReportPath.Replace('/', Path.DirectorySeparatorChar));
        bool hadProgress = PlayerPrefs.HasKey(ProgressKey);
        int progressBefore = PlayerPrefs.GetInt(ProgressKey, 0);
        bool runInBackgroundBefore = Application.runInBackground;
        object runtime = null;
        object oldLevel = null;
        object oldState = null;
        object oldUndo = null;
        object oldMode = null;
        int oldLevelIndex = 0;
        bool oldWorldActive = true;
        bool restoreCaptured = false;

        try {
            Require(Application.isPlaying && !EditorApplication.isPaused,
                "Run this Editor regression in unpaused Play Mode.");
            runtime = UnityEngine.Object.FindFirstObjectByType<RotateRingsRuntime>();
            Require(runtime != null, "RotateRingsRuntime is not present in the active Play Mode scene.");
            Require(GetField(runtime, "mode").ToString() == "Game",
                "Open a gameplay level before running the physical pin regression.");
            oldLevel = GetField(runtime, "level");
            oldState = GetField(runtime, "state");
            oldUndo = CloneStack(GetField(runtime, "undo"));
            oldMode = GetField(runtime, "mode");
            oldLevelIndex = ReadInt(runtime, "levelIndex");
            var oldWorld = (RotateRingsWorldView)GetField(runtime, "world");
            oldWorldActive = oldWorld != null && oldWorld.gameObject.activeSelf;
            restoreCaptured = oldLevel != null && oldState != null;

            var level = (RotateRingsLevel)GetField(runtime, "level");
            var state = GetField(runtime, "state");
            var world = (RotateRingsWorldView)GetField(runtime, "world");
            Require(level != null && level.links != null, "A gameplay level with source links is required.");
            var templateCatalog = GetField<RotateRingsTemplateCatalog>(runtime, "templates");
            RotateRingsLink link = null;
            RotateRingsPart source = null, target = null;
            foreach (var candidate in level.links) {
                var candidateSource = level.parts.Find(p => p.id == candidate.source);
                var candidateTarget = level.parts.Find(p => p.id == candidate.target);
                if (candidateSource == null || candidateTarget == null ||
                    !HasVisualAttachments(candidateSource, templateCatalog) ||
                    !Convert.ToBoolean(Invoke(runtime, "IsActive", candidate))) continue;
                link = candidate; source = candidateSource; target = candidateTarget; break;
            }
            Require(source != null && target != null && link != null,
                "The active level has no active link whose source template contains visual attachments.");
            report.levelId = level.id; report.sourceId = source.id; report.targetId = target.id;

            Invoke(runtime, "UpdateWorld");
            var sourceView = FindPart(world, source.id);
            var targetView = FindPart(world, target.id);
            Require(sourceView != null && targetView != null, "Linked part meshes were not built.");
            var sourceRenderer = sourceView.GetComponent<MeshRenderer>();
            var sourceFilter = sourceView.GetComponent<MeshFilter>();
            var targetRenderer = targetView.GetComponent<MeshRenderer>();
            var targetFilter = targetView.GetComponent<MeshFilter>();
            Require(sourceRenderer != null && sourceFilter != null && sourceFilter.sharedMesh != null &&
                targetRenderer != null && targetFilter != null && targetFilter.sharedMesh != null,
                "A linked part is missing its mesh renderer or integrated mesh.");
            var sourceMesh = sourceFilter.sharedMesh;
            var targetMesh = targetFilter.sharedMesh;
            Check(report, sourceRenderer.enabled && sourceMesh.vertexCount > 0,
                "Source attachment is included in the rendered source-part mesh",
                "mesh vertices=" + sourceMesh.vertexCount + ", renderer=" + sourceRenderer.enabled);

            var angles = GetField<Dictionary<int, float>>(state, "angles");
            var removed = GetField<HashSet<int>>(state, "removed");
            float sourceAngle0 = angles[source.id];
            float targetAngle0 = angles[target.id];
            bool activeAtStart = Convert.ToBoolean(Invoke(runtime, "IsActive", link));
            bool inactiveAtGap = false;
            for (int step = 1; step <= 24 && !inactiveAtGap; step++) {
                angles[target.id] = targetAngle0 + step * 15f;
                Invoke(runtime, "UpdateWorld");
                inactiveAtGap = !Convert.ToBoolean(Invoke(runtime, "IsActive", link));
            }
            bool sourceMeshVisibleWithTargetGapOpen = sourceRenderer.enabled && sourceFilter.sharedMesh == sourceMesh;
            angles[target.id] = targetAngle0;
            Invoke(runtime, "UpdateWorld");
            bool activeAfterRelock = Convert.ToBoolean(Invoke(runtime, "IsActive", link));
            Check(report, activeAtStart && inactiveAtGap && activeAfterRelock && sourceMeshVisibleWithTargetGapOpen && sourceRenderer.enabled,
                "Binding unlock and relock leave the integrated source attachment visible",
                "active: start=" + activeAtStart + ", opened=" + !inactiveAtGap + ", relocked=" + activeAfterRelock +
                ", source mesh while opened=" + sourceMeshVisibleWithTargetGapOpen);

            Vector3 attachmentPoint = new Vector3(sourceMesh.bounds.max.x, sourceMesh.bounds.center.y, 0f);
            Vector3 pointBefore = sourceView.TransformPoint(attachmentPoint);
            angles[source.id] = sourceAngle0 + 45f;
            Invoke(runtime, "UpdateWorld");
            Vector3 pointAfter = sourceView.TransformPoint(attachmentPoint);
            float attachmentTravel = Vector3.Distance(pointBefore, pointAfter);
            Check(report, attachmentTravel > .1f && sourceRenderer.enabled && sourceFilter.sharedMesh == sourceMesh,
                "Changing source angle rotates the attachment with its whole-part mesh",
                "pointBefore=" + pointBefore + ", pointAfter=" + pointAfter + ", travel=" + attachmentTravel);
            angles[source.id] = sourceAngle0;
            Invoke(runtime, "UpdateWorld");

            removed.Add(target.id);
            Invoke(runtime, "UpdateWorld");
            bool targetHidden = !targetRenderer.enabled;
            bool sourceMeshSurvivesTargetRemoval = sourceRenderer.enabled && sourceFilter.sharedMesh == sourceMesh;
            removed.Remove(target.id);
            Invoke(runtime, "UpdateWorld");
            Check(report, targetHidden && sourceMeshSurvivesTargetRemoval && sourceRenderer.enabled && targetFilter.sharedMesh == targetMesh,
                "Removing target hides its mesh and keeps the source attachment mesh visible",
                "targetRendererAfterRemoval=" + !targetHidden + ", sourceMeshAfterRemoval=" + sourceMeshSurvivesTargetRemoval);

            Invoke(runtime, "PushUndo");
            removed.Add(source.id);
            Invoke(runtime, "UpdateWorld");
            bool sourceHidden = !sourceRenderer.enabled;
            bool attachmentHidden = sourceHidden;
            Invoke(runtime, "Undo");
            state = GetField(runtime, "state");
            removed = GetField<HashSet<int>>(state, "removed");
            Invoke(runtime, "UpdateWorld");
            bool sourceRestored = !removed.Contains(source.id) && sourceRenderer.enabled && sourceFilter.sharedMesh == sourceMesh;
            bool attachmentRestored = sourceRestored;
            Check(report, sourceHidden && attachmentHidden && sourceRestored && attachmentRestored,
                "Source removal hides its integrated attachment and runtime Undo restores it",
                "sourceHidden=" + sourceHidden + ", attachmentHidden=" + attachmentHidden +
                ", sourceRestored=" + sourceRestored + ", attachmentRestored=" + attachmentRestored);
        } catch (Exception ex) {
            report.error = ex.ToString();
            Check(report, false, "Regression execution completed without exception", ex.GetType().Name + ": " + ex.Message);
        } finally {
            if (restoreCaptured && runtime != null) {
                try {
                    int oldId = ReadInt(oldLevel, "id");
                    Invoke(runtime, "StartLevel", oldId);
                    SetField(runtime, "level", oldLevel);
                    SetField(runtime, "state", oldState);
                    SetField(runtime, "undo", oldUndo);
                    SetField(runtime, "levelIndex", oldLevelIndex);
                    SetField(runtime, "mode", oldMode);
                    var world = (RotateRingsWorldView)GetField(runtime, "world");
                    world.gameObject.SetActive(oldWorldActive);
                    Invoke(runtime, "UpdateWorld");
                    var hud = GetField(runtime, "originalHud") as RotateRingsOriginalHud;
                    if (hud != null) hud.SetLevel(oldLevelIndex + 1);
                } catch (Exception ex) {
                    Check(report, false, "Previous gameplay state restored", ex.GetType().Name + ": " + ex.Message);
                }
            }

            bool progressRestored = PlayerPrefs.HasKey(ProgressKey) == hadProgress &&
                (!hadProgress || PlayerPrefs.GetInt(ProgressKey, int.MinValue) == progressBefore);
            bool backgroundPreserved = Application.runInBackground == runInBackgroundBefore;
            report.playerPrefsRestored = progressRestored;
            report.runInBackgroundPreserved = backgroundPreserved;
            Check(report, progressRestored, "PlayerPrefs progress preserved", "hadKey=" + hadProgress + ", value=" + PlayerPrefs.GetInt(ProgressKey, 0));
            Check(report, backgroundPreserved, "Application.runInBackground preserved", "value=" + Application.runInBackground);
        }

        report.passed = report.assertions.TrueForAll(a => a.passed);
        report.status = report.passed ? "passed" : "failed";
        report.finishedUtc = DateTime.UtcNow.ToString("o");
        Directory.CreateDirectory(Path.GetDirectoryName(reportFile));
        File.WriteAllText(reportFile, JsonUtility.ToJson(report, true));
        return reportFile;
    }

    static Transform FindPart(RotateRingsWorldView world, int id) {
        string prefix = "Part " + id + " (";
        foreach (var item in world.transform) {
            var child = (Transform)item;
            if (child.name.StartsWith(prefix, StringComparison.Ordinal)) return child;
        }
        return null;
    }

    static bool HasVisualAttachments(RotateRingsPart part, RotateRingsTemplateCatalog catalog) {
        if (catalog == null || catalog.templates == null) return false;
        foreach (var template in catalog.templates)
            if (template.id == part.templateId && template.attachments != null && template.attachments.Length > 0) return true;
        return false;
    }

    static void Check(Report report, bool passed, string name, string detail) {
        report.assertions.Add(new Assertion { name = name, passed = passed, detail = detail });
        if (!passed) Debug.LogError("Pin regression assertion failed: " + name + " — " + detail);
    }

    static void Require(bool condition, string message) {
        if (!condition) throw new InvalidOperationException(message);
    }

    static object GetField(object instance, string name) {
        if (instance == null) throw new InvalidOperationException("Cannot read " + name + " from null.");
        var field = instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field == null) throw new MissingFieldException(instance.GetType().FullName, name);
        return field.GetValue(instance);
    }

    static T GetField<T>(object instance, string name) { return (T)GetField(instance, name); }

    static void SetField(object instance, string name, object value) {
        var field = instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field == null) throw new MissingFieldException(instance.GetType().FullName, name);
        field.SetValue(instance, value);
    }

    static int ReadInt(object instance, string name) { return GetField<int>(instance, name); }

    static object Invoke(object instance, string name, params object[] args) {
        var method = instance.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method == null) throw new MissingMethodException(instance.GetType().FullName, name);
        return method.Invoke(instance, args);
    }

    static object CloneStack(object stack) {
        var type = stack.GetType();
        var items = (Array)type.GetMethod("ToArray").Invoke(stack, null);
        var clone = Activator.CreateInstance(type);
        var push = type.GetMethod("Push");
        for (int i = items.Length - 1; i >= 0; i--) push.Invoke(clone, new[] { items.GetValue(i) });
        return clone;
    }

    static string Hash(string path) {
        if (!File.Exists(path)) return string.Empty;
        using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant();
    }
}
#endif

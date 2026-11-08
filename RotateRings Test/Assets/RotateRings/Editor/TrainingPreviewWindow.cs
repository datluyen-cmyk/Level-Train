#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Temporary editor-only entry point for generated training packs. The manifest and
// its referenced data stay outside Resources and never enter production exports.
public sealed class TrainingPreviewWindow : EditorWindow
{
    [Serializable]
    sealed class Manifest
    {
        public int schemaVersion = 1;
        public string revision;
        public string packPath;
        public string sourcePackPath;
        public string templatesPath;
        public string sourceTemplatesPath;
    }

    [Serializable]
    sealed class FeedbackRecord
    {
        public string createdUtc;
        public string revision;
        public int levelId;
        public string category;
        public string note;
    }

    const string DefaultManifest = "LevelTraining_SourceLayoutV3/runs/ordinary_v4/preview/current_preview.json";
    const string PendingManifestKey = "RotateRings.TrainingPreview.Manifest";
    const string PendingLevelKey = "RotateRings.TrainingPreview.LevelId";
    const string PendingOriginalKey = "RotateRings.TrainingPreview.Original";
    const string PendingUnlockAllKey = "RotateRings.TrainingPreview.UnlockAll";
    const string PendingPackOverrideKey = "RotateRings.TrainingPreview.PackOverride";

    string manifestPath;
    string status;
    string error;
    string note = "";
    string[] levelLabels = Array.Empty<string>();
    int selected;
    int feedbackCategory;
    bool unlockAll = true;
    bool originalMode;
    Manifest manifest;
    RotateRingsLevelPack pack;
    RotateRingsLevelPack sourcePack;
    RotateRingsTemplateCatalog templates;
    RotateRingsTemplateCatalog sourceTemplates;

    [MenuItem("Rotate Rings/Training Preview")]
    public static void Open()
    {
        var window = GetWindow<TrainingPreviewWindow>("Training Preview");
        window.minSize = new Vector2(440, 420);
        window.Show();
    }

    // Called by the training GUI over MCP after a round: open the window and
    // load the freshly staged pack (manifestPath null = default current_preview.json).
    public static string OpenAndReload(string manifest = null)
    {
        var window = GetWindow<TrainingPreviewWindow>("Training Preview");
        window.minSize = new Vector2(440, 420);
        window.manifestPath = string.IsNullOrEmpty(manifest)
            ? Path.GetFullPath(Path.Combine(Application.dataPath, "..", DefaultManifest))
            : Path.GetFullPath(manifest);
        window.selected = 0;
        window.originalMode = false;
        window.ReloadManifest();
        window.Show();
        window.Focus();
        return window.error ?? ("loaded " + (window.pack?.levels?.Count ?? 0) + " levels from " + window.manifestPath);
    }

    void OnEnable()
    {
        if (string.IsNullOrEmpty(manifestPath))
            manifestPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", DefaultManifest));
        ReloadManifest();
    }

    void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "TRAINING PREVIEW — provisional generated levels. Difficulty and playability are not yet verified. " +
            "Use the original gameplay controls, then save a note for the training review.", MessageType.Info);

        EditorGUILayout.BeginHorizontal();
        manifestPath = EditorGUILayout.TextField("Preview manifest", manifestPath);
        if (GUILayout.Button("Browse", GUILayout.Width(70)))
        {
            var chosen = EditorUtility.OpenFilePanel("Select training preview manifest", Path.GetDirectoryName(manifestPath), "json");
            if (!string.IsNullOrEmpty(chosen)) manifestPath = chosen;
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Reload manifest")) ReloadManifest();
        if (!string.IsNullOrEmpty(error)) EditorGUILayout.HelpBox(error, MessageType.Error);
        if (pack == null || pack.levels == null || pack.levels.Count == 0) return;

        EditorGUILayout.LabelField("Revision", string.IsNullOrEmpty(manifest.revision) ? "(unspecified)" : manifest.revision);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Play mode", GUILayout.Width(75));
        if (GUILayout.Button(originalMode ? "Training" : "Training ✓", GUILayout.Width(120))) SetPreviewMode(false);
        using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(manifest.sourcePackPath)))
        {
            if (GUILayout.Button(originalMode ? "Original ✓" : "Original", GUILayout.Width(120))) SetPreviewMode(true);
        }
        EditorGUILayout.EndHorizontal();

        if (originalMode && !EnsureSourcePackLoaded()) return;
        var activePack = originalMode ? sourcePack : pack;
        if (activePack == null || activePack.levels == null || activePack.levels.Count == 0) return;
        EditorGUILayout.LabelField("Levels", activePack.levels.Count.ToString());
        selected = EditorGUILayout.Popup("Sample level", Mathf.Clamp(selected, 0, levelLabels.Length - 1), levelLabels);
        var level = activePack.levels[selected];
        EditorGUILayout.LabelField("Difficulty", "Pending comparison with original");
        EditorGUILayout.LabelField("Parts", level.parts == null ? "(original catalog)" : level.parts.Count.ToString());
        if (!string.IsNullOrEmpty(level.referenceId))
            EditorGUILayout.LabelField("Original reference", level.referenceId);
        unlockAll = EditorGUILayout.ToggleLeft("Unlock every preview level", unlockAll);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(originalMode ? "Start first original" : "Start first training")) QueueLaunch(0, originalMode);
        if (GUILayout.Button(originalMode ? "Start selected original" : "Start selected training")) QueueLaunch(selected, originalMode);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        var runtime = EditorApplication.isPlaying ? UnityEngine.Object.FindFirstObjectByType<RotateRingsRuntime>() : null;
        int feedbackLevelId = runtime != null && runtime.TrainingPreviewLevelIdForQa >= 0 ? runtime.TrainingPreviewLevelIdForQa : level.id;
        EditorGUILayout.LabelField("Feedback for level " + feedbackLevelId, EditorStyles.boldLabel);
        feedbackCategory = EditorGUILayout.Popup("Category", feedbackCategory,
            new[] { "Playable", "Too easy", "Too hard", "Visual issue", "Other" });
        note = EditorGUILayout.TextArea(note, GUILayout.MinHeight(70));
        using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(note)))
        {
            if (GUILayout.Button("Save feedback note")) SaveFeedback(feedbackLevelId);
        }
        if (!string.IsNullOrEmpty(status)) EditorGUILayout.HelpBox(status, MessageType.None);
        EditorGUILayout.HelpBox("After a clear, the existing NEXT LEVEL button advances through this preview pack.", MessageType.None);
    }

    void ReloadManifest()
    {
        originalMode = false;
        sourceTemplates = null;
        try
        {
            var fullManifestPath = Path.GetFullPath(manifestPath);
            manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(fullManifestPath));
            if (manifest == null || string.IsNullOrWhiteSpace(manifest.packPath))
            {
                // A level pack (or other file in a staged preview folder) was
                // chosen instead of the manifest: use the folder's manifest.
                string sibling = Path.Combine(Path.GetDirectoryName(fullManifestPath), "preview_manifest.json");
                if (File.Exists(sibling))
                {
                    fullManifestPath = sibling;
                    manifestPath = sibling;
                    manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(fullManifestPath));
                }
                else
                    throw new InvalidDataException("This is not a preview manifest: " + fullManifestPath +
                        ". Choose current_preview.json or preview_manifest.json (not manual_test_pack.json).");
            }
            if (manifest == null || manifest.schemaVersion != 1 || string.IsNullOrWhiteSpace(manifest.packPath) || string.IsNullOrWhiteSpace(manifest.templatesPath))
                throw new InvalidDataException("Expected schemaVersion 1 with packPath and templatesPath.");

            string root = Path.GetDirectoryName(fullManifestPath);
            string packPath = Resolve(root, manifest.packPath);
            string templatesPath = Resolve(root, manifest.templatesPath);
            pack = JsonUtility.FromJson<RotateRingsLevelPack>(File.ReadAllText(packPath));
            // Keep the large original corpus lazy. The training candidate pack
            // is small and is loaded immediately; original levels are parsed
            // only after the user explicitly chooses Original mode.
            sourcePack = null;
            sourceTemplates = null;
            templates = JsonUtility.FromJson<RotateRingsTemplateCatalog>(File.ReadAllText(templatesPath));
            ValidatePreviewData(pack, templates);

            levelLabels = LabelsFor(pack);
            selected = Mathf.Clamp(selected, 0, levelLabels.Length - 1);
            error = null;
            status = "Loaded provisional pack. No completion or difficulty verification is implied.";
        }
        catch (Exception e)
        {
            pack = null;
            sourcePack = null;
            templates = null;
            levelLabels = Array.Empty<string>();
            error = e.Message;
            status = null;
        }
        Repaint();
    }

    static string Resolve(string root, string path) => Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(root, path));

    string[] LabelsFor(RotateRingsLevelPack value)
    {
        if (value == null || value.levels == null) return Array.Empty<string>();
        return value.levels.Select((l, i) => l != null && l.position > 0
            ? string.Format("{0} — {1}", l.position, string.IsNullOrEmpty(l.title) ? (originalMode ? "Original" : "Training") : l.title)
            : string.Format("{0} — {1}", l == null ? i + 1 : l.id, string.IsNullOrEmpty(l == null ? null : l.title) ? "Untitled" : l.title)).ToArray();
    }

    void SetPreviewMode(bool original)
    {
        if (!original)
        {
            originalMode = false;
            levelLabels = LabelsFor(pack);
            selected = Mathf.Clamp(selected, 0, Mathf.Max(0, levelLabels.Length - 1));
            Repaint();
            return;
        }
        if (!EnsureSourcePackLoaded()) return;
        originalMode = true;
        levelLabels = LabelsFor(sourcePack);
        selected = Mathf.Clamp(selected, 0, Mathf.Max(0, levelLabels.Length - 1));
        Repaint();
    }

    bool EnsureSourcePackLoaded()
    {
        if (sourcePack != null) return true;
        try
        {
            if (manifest == null || string.IsNullOrWhiteSpace(manifest.sourcePackPath))
                throw new InvalidDataException("No original source pack is configured in the preview manifest.");
            string root = Path.GetDirectoryName(Path.GetFullPath(manifestPath));
            sourcePack = JsonUtility.FromJson<RotateRingsLevelPack>(File.ReadAllText(Resolve(root, manifest.sourcePackPath)));
            string sourceTemplatesPath = string.IsNullOrWhiteSpace(manifest.sourceTemplatesPath) ? manifest.templatesPath : manifest.sourceTemplatesPath;
            sourceTemplates = JsonUtility.FromJson<RotateRingsTemplateCatalog>(File.ReadAllText(Resolve(root, sourceTemplatesPath)));
            ValidatePreviewData(sourcePack, sourceTemplates, true);
            status = "Original catalog loaded: " + sourcePack.levels.Count + " source levels.";
            return true;
        }
        catch (Exception e)
        {
            sourcePack = null;
            error = "Could not load original catalog: " + e.Message;
            return false;
        }
    }

    static void ValidatePreviewData(RotateRingsLevelPack candidate, RotateRingsTemplateCatalog catalog, bool originalReference = false)
    {
        if (candidate == null || candidate.schemaVersion != 3 || (!originalReference && candidate.sourceOriginalOnly) || candidate.levels == null || candidate.levels.Count == 0)
            throw new InvalidDataException("Preview pack must use schema 3, contain levels, and not be marked sourceOriginalOnly. complete may be false.");
        if (catalog == null || catalog.schemaVersion != 3 || catalog.templates == null || catalog.templates.Length == 0)
            throw new InvalidDataException("Physics templates must use schema 3 and contain templates.");

        var knownTemplates = catalog.templates.Where(t => t != null).Select(t => t.id).ToArray();
        var ids = new System.Collections.Generic.HashSet<int>();
        foreach (var level in candidate.levels)
        {
            if (level == null || !ids.Add(level.id) || (!originalReference && level.sourceOnlyParityFixture) || level.parts == null || level.parts.Count == 0)
                throw new InvalidDataException("Every preview level needs a unique id, playable parts, and sourceOnlyParityFixture=false.");
            foreach (var part in level.parts)
                if (part == null || string.IsNullOrEmpty(part.templateId) || !knownTemplates.Contains(part.templateId))
                    throw new InvalidDataException("Level " + level.id + " references a missing physics template.");
        }
    }

    void QueueLaunch(int index, bool original)
    {
        var activePack = original ? sourcePack : pack;
        if (activePack == null || index < 0 || index >= activePack.levels.Count) return;
        var selectedLevel = activePack.levels[index];
        int levelId = selectedLevel.id;
        if (levelId < 0) return;
        string overridePack = WriteRuntimeLevelPack(activePack, selectedLevel, original);
        if (EditorApplication.isPlaying)
        {
            TrainingPreviewLauncher.Load(manifestPath, levelId, unlockAll, original, overridePack);
            return;
        }

        SessionState.SetString(PendingManifestKey, Path.GetFullPath(manifestPath));
        SessionState.SetInt(PendingLevelKey, levelId);
        SessionState.SetBool(PendingOriginalKey, original);
        SessionState.SetBool(PendingUnlockAllKey, unlockAll);
        SessionState.SetString(PendingPackOverrideKey, overridePack ?? "");
        EditorApplication.isPlaying = true;
        status = "Starting Play Mode and loading sample " + levelId + "…";
    }

    static int ParseReferenceId(RotateRingsLevel level)
    {
        return int.TryParse(level == null ? null : level.referenceId, out int value) ? value : -1;
    }

    string WriteRuntimeLevelPack(RotateRingsLevelPack activePack, RotateRingsLevel selectedLevel, bool original)
    {
        try
        {
            string directory = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(manifestPath)), ".runtime_level_cache");
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, (original ? "original_" : "training_") + selectedLevel.id + ".json");
            var single = new RotateRingsLevelPack { schemaVersion = 3, complete = true, sourceOriginalOnly = false,
                levels = new System.Collections.Generic.List<RotateRingsLevel> { selectedLevel } };
            selectedLevel.sourceOnlyParityFixture = false;
            File.WriteAllText(path, JsonUtility.ToJson(single), System.Text.Encoding.UTF8);
            return path;
        }
        catch (Exception e)
        {
            status = "Single-level cache skipped: " + e.Message;
            return null;
        }
    }

    void SaveFeedback(int levelId)
    {
        try
        {
            string output = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(manifestPath)), "feedback.jsonl");
            var record = new FeedbackRecord {
                createdUtc = DateTime.UtcNow.ToString("o"), revision = manifest.revision,
                levelId = levelId, category = new[] { "Playable", "Too easy", "Too hard", "Visual issue", "Other" }[feedbackCategory],
                note = note.Trim()
            };
            File.AppendAllText(output, JsonUtility.ToJson(record) + Environment.NewLine);
            status = "Saved feedback to " + output;
            note = "";
        }
        catch (Exception e) { error = "Could not save feedback: " + e.Message; }
    }

    [InitializeOnLoad]
    static class TrainingPreviewLauncher
    {
        static TrainingPreviewLauncher() { EditorApplication.playModeStateChanged += OnPlayModeChanged; }

        static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode || string.IsNullOrEmpty(SessionState.GetString(PendingManifestKey, ""))) return;
            EditorApplication.delayCall += () => {
                string path = SessionState.GetString(PendingManifestKey, "");
                int levelId = SessionState.GetInt(PendingLevelKey, -1);
                bool original = SessionState.GetBool(PendingOriginalKey, false);
                bool unlock = SessionState.GetBool(PendingUnlockAllKey, true);
                string overridePack = SessionState.GetString(PendingPackOverrideKey, "");
                SessionState.EraseString(PendingManifestKey);
                SessionState.EraseInt(PendingLevelKey);
                SessionState.EraseBool(PendingOriginalKey);
                SessionState.EraseBool(PendingUnlockAllKey);
                SessionState.EraseString(PendingPackOverrideKey);
                Load(path, levelId, unlock, original, overridePack);
            };
        }

        public static void Load(string manifestPath, int levelId, bool unlockAll, bool original, string packOverride = null)
        {
            try
            {
                var runtime = UnityEngine.Object.FindFirstObjectByType<RotateRingsRuntime>();
                if (runtime == null) throw new InvalidOperationException("RotateRingsRuntime was not created in Play Mode.");
                string root = Path.GetDirectoryName(Path.GetFullPath(manifestPath));
                var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(manifestPath));
                runtime.SetTrainingPreviewUnlockAllForQa(unlockAll);
                string selectedPack = !string.IsNullOrWhiteSpace(manifest.packPath) ? manifest.packPath : packOverride;
                if (string.IsNullOrWhiteSpace(selectedPack)) throw new InvalidDataException("Training preview manifest has no training pack path.");
                string sourcePack = string.IsNullOrWhiteSpace(manifest.sourcePackPath) ? null : Resolve(root, manifest.sourcePackPath);
                string candidatePack = Resolve(root, selectedPack);
                if (!string.IsNullOrWhiteSpace(sourcePack))
                    runtime.LoadTrainingPreviewPairForQa(candidatePack, sourcePack, Resolve(root, manifest.templatesPath),
                        string.IsNullOrWhiteSpace(manifest.sourceTemplatesPath) ? Resolve(root, manifest.templatesPath) : Resolve(root, manifest.sourceTemplatesPath), levelId);
                else
                    runtime.LoadTrainingPreviewForQa(!string.IsNullOrWhiteSpace(packOverride) ? packOverride : candidatePack, Resolve(root, manifest.templatesPath), levelId, original);
                Debug.Log("TRAINING PREVIEW loaded level " + levelId + " from revision " + manifest.revision + ".");
            }
            catch (Exception e) { Debug.LogError("TRAINING PREVIEW failed to load: " + e); }
        }
    }
}
#endif

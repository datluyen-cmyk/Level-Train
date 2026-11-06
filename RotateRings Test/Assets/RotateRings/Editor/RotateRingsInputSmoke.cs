#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

/// <summary>Runs a real Input System mouse drag against the live Rotate Rings runtime.
/// Invoke after the PhysicsV2 ring-only pack and its full template catalogue are imported.</summary>
[InitializeOnLoad]
public static class RotateRingsInputSmoke {
    const string MenuPath = "Tools/Rotate Rings/QA/Input Smoke";
    const string Key = "RotateRings.InputSmoke.";
    const string PackRelativePath = "RotateRings/Resources/Levels/generated_levels.json";
    const string CatalogRelativePath = "RotateRings/Resources/Levels/physics_templates.json";
    const string ProgressKey = "rr_max_level";
    static int waitFrames;
    static int lastTickFrame = -1;
    static bool tickSubscribed;
    static RotateRingsRuntime runtime;
    static RotateRingsLevel level;
    static RotateRingsPart targetPart;
    static RotateRingsGesture targetGesture;
    static Vector2 downScreen;
    static Vector2 upScreen;
    static float angleBefore;
    static int movesBefore;
    static bool activeDragIsSlider;
    static bool sourceReferenceSmoke;
    static float sliderTravel;
    static Mouse physicalMouse;
    static Mouse qaMouse;
    static bool progressHadKey;
    static int progressBefore;

    [Serializable]
    sealed class SmokeReport {
        public string status;
        public string startedUtc;
        public string finishedUtc;
        public string unityVersion;
        public string instance;
        public string qaHarnessSha256;
        public string runtimeSha256;
        public string collisionWorldSha256;
        public string worldViewSha256;
        public string levelPackSha256;
        public string templateCatalogSha256;
        public int levelId;
        public int inputEventsQueued;
        public int movesBefore;
        public int movesAfterDrag;
        public int movesAfterUndo;
        public int movesAfterRestart;
        public int targetPartId;
        public float angleBefore;
        public float expectedAbsoluteAngle;
        public float angleAfterDrag;
        public bool hintDidNotChangeGameState;
        public bool dragUsedInputSystem;
        public bool undoRestoredSnapshot;
        public bool restartRestoredLevel;
        public bool sliderActionAvailable;
        public bool sliderInputSystem;
        public bool sliderRemoved;
        public bool sliderUndoRestored;
        public int sliderLevelId;
        public int sliderPartId;
        public int sliderMovesBefore;
        public int sliderMovesAfter;
        public float sliderTravel;
        public int actualDragPartId;
        public bool actualDragIsSlider;
        public float sliderObservedDelta;
        public float angleAtMove;
        public float dragDeltaAtMove;
        public float pointerAngleAtMove;
        public Vector2 inputStartScreen;
        public Vector2 inputEndScreen;
        public string error;
        public string note;
    }

    static RotateRingsInputSmoke() {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        if (SessionState.GetBool(Key + "running", false)) EnsureTickSubscribed();
    }

    [MenuItem(MenuPath, true)]
    static bool ValidateRun() {
        string packPath = Path.Combine(Application.dataPath, PackRelativePath);
        string catalogPath = Path.Combine(Application.dataPath, CatalogRelativePath);
        return !EditorApplication.isPlayingOrWillChangePlaymode && File.Exists(packPath) && File.Exists(catalogPath);
    }

    [MenuItem(MenuPath)]
    static void Run() {
        if (EditorApplication.isPlayingOrWillChangePlaymode) {
            Debug.LogWarning("Input Smoke requires Edit Mode. Stop Play Mode before starting it.");
            return;
        }
        string packPath = Path.Combine(Application.dataPath, PackRelativePath);
        string catalogPath = Path.Combine(Application.dataPath, CatalogRelativePath);
        if (!File.Exists(packPath) || !File.Exists(catalogPath)) {
            Debug.LogError("Input Smoke skipped: PhysicsV2 ring pack and full physics template catalogue must both be imported. Missing pack/catalog: " + packPath + " / " + catalogPath);
            return;
        }
        progressHadKey = PlayerPrefs.HasKey(ProgressKey);
        progressBefore = PlayerPrefs.GetInt(ProgressKey, 1);
        SessionState.SetBool(Key + "runInBackgroundSaved", true);
        SessionState.SetBool(Key + "runInBackgroundBefore", Application.runInBackground);
        Application.runInBackground = true;
        SessionState.SetBool(Key + "progressHadKey", progressHadKey);
        SessionState.SetInt(Key + "progressBefore", progressBefore);
        SessionState.SetString(Key + "startedUtc", DateTime.UtcNow.ToString("o"));
        SessionState.SetString(Key + "phase", "setup");
        SessionState.SetInt(Key + "events", 0);
        SessionState.SetBool(Key + "running", true);
        SessionState.SetString(Key + "report", string.Empty);
        EnsureTickSubscribed();
        FocusGameView();
        EditorApplication.EnterPlaymode();
    }

    static void OnPlayModeChanged(PlayModeStateChange change) {
        if (!SessionState.GetBool(Key + "running", false)) return;
        if (change == PlayModeStateChange.EnteredPlayMode) {
            Application.runInBackground = true;
            FocusGameView();
            SessionState.SetString(Key + "phase", "setup");
            EnsureTickSubscribed();
        } else if (change == PlayModeStateChange.ExitingPlayMode) {
            RemoveTickSubscription();
            if (string.IsNullOrEmpty(SessionState.GetString(Key + "report", string.Empty))) {
                Fail("Play Mode was stopped before the smoke harness finished.");
            }
        } else if (change == PlayModeStateChange.EnteredEditMode) {
            RestoreProgress();
            RestoreRunInBackground();
            PersistReportIfPresent();
            SessionState.SetBool(Key + "running", false);
            RemoveTickSubscription();
        }
    }

    static void Tick() {
        if (!SessionState.GetBool(Key + "running", false)) {
            RemoveTickSubscription();
            return;
        }
        if (!EditorApplication.isPlaying) return;
        int currentFrame = Time.frameCount;
        if (currentFrame == lastTickFrame) return;
        lastTickFrame = currentFrame;

        try {
            switch (SessionState.GetString(Key + "phase", "setup")) {
                case "setup": SetupAndHintCheck(); break;
                case "wait-entry-ring": WaitForEntryAndQueuePress(); break;
                case "wait-entry-slider": WaitForEntryAndQueuePress(); break;
                case "wait-press": WaitForPress(); break;
                case "wait-move": WaitForMove(); break;
                case "wait-release": WaitForRelease(); break;
            }
        } catch (Exception ex) {
            Fail(ex.GetType().Name + ": " + ex.Message + "\n" + ex.StackTrace);
        }
    }

    static void SetupAndHintCheck() {
        runtime = UnityEngine.Object.FindFirstObjectByType<RotateRingsRuntime>();
        if (runtime == null) return;

        var pack = GetField<RotateRingsLevelPack>(runtime, "pack");
        sourceReferenceSmoke = pack == null || pack.levels == null || pack.levels.Count == 0;
        if (sourceReferenceSmoke) {
            // The Resources pack is intentionally retired schema 2. Use the
            // source-only Editor QA loader instead of restoring that pack.
            runtime.LoadOriginalReferenceForQa(10001);
            pack = GetField<RotateRingsLevelPack>(runtime, "pack");
        }

        var start = typeof(RotateRingsRuntime).GetMethod("StartLevel", BindingFlags.Instance | BindingFlags.NonPublic);
        if (start == null) throw new InvalidOperationException("RotateRingsRuntime.StartLevel(int) not found.");
        if (pack == null || pack.levels == null) throw new InvalidOperationException("Loaded level pack is missing.");
        targetGesture = null;
        foreach (var candidate in pack.levels) {
            if (!HasPhysicsV2RingOnlyGeometry(candidate)) continue;
            start.Invoke(runtime, new object[] { candidate.id });
            level = GetField<RotateRingsLevel>(runtime, "level");
            if (level == null) continue;
            if (level.solution != null && level.solution.Count > 0) targetGesture = FindImmediateSolutionRotate();
            else if (sourceReferenceSmoke && level.id == 10001) targetGesture = FindSourceReferenceSmokeRotate();
            if (targetGesture != null && CountCollectibles() > 1) break;
            targetGesture = null;
        }
        if (level == null || targetGesture == null)
            throw new InvalidOperationException("No ring-only level with a legal controller smoke rotation was found in the loaded pack.");
        EnsureVirtualMouse();

        var beforeHint = CaptureGameState();
        InvokePrivate(runtime, "Hint");
        var afterHint = CaptureGameState();
        bool hintUnchanged = beforeHint.Equals(afterHint);
        if (!hintUnchanged) throw new InvalidOperationException("Hint changed angle, moves, removed pieces, or keys.");

        targetPart = level.parts.First(p => p.id == targetGesture.part);
        angleBefore = ReadAngles()[targetPart.id];
        movesBefore = ReadInt(GetField<object>(runtime, "state"), "moves");
        if (CountCollectibles() <= 1) throw new InvalidOperationException("Selected gesture could be the winning move; choose a non-winning gesture before running this smoke test.");

        Camera camera = Camera.main;
        if (camera == null) throw new InvalidOperationException("Main Camera is missing after loading the PhysicsV2 ring level.");
        if (Mouse.current == null) throw new InvalidOperationException("Input System has no Mouse device to queue events to.");
        float pointerStartAngle = targetPart.angle + 180f; // Opposite the ring gap, on the solid arc.
        float delta = GestureTravel(angleBefore, targetGesture);
        Vector3 center = new Vector3(targetPart.x, targetPart.y, 0f);
        Vector3 from = center + new Vector3(Mathf.Cos(pointerStartAngle * Mathf.Deg2Rad), Mathf.Sin(pointerStartAngle * Mathf.Deg2Rad), 0f) * targetPart.radius;
        Vector3 to = center + new Vector3(Mathf.Cos((pointerStartAngle + delta) * Mathf.Deg2Rad), Mathf.Sin((pointerStartAngle + delta) * Mathf.Deg2Rad), 0f) * targetPart.radius;
        Vector3 downScreenPoint = camera.WorldToScreenPoint(from);
        Vector3 upScreenPoint = camera.WorldToScreenPoint(to);
        downScreen = downScreenPoint;
        upScreen = upScreenPoint;
        if (downScreenPoint.z < 0f || upScreenPoint.z < 0f) throw new InvalidOperationException("Calculated drag points are behind the Game camera.");

        var report = NewReport();
        report.levelId = level.id;
        report.targetPartId = targetPart.id;
        report.angleBefore = angleBefore;
        report.expectedAbsoluteAngle = angleBefore + delta;
        report.qaHarnessSha256 = Hash(Path.Combine(Application.dataPath, "RotateRings", "Editor", "RotateRingsInputSmoke.cs"));
        report.runtimeSha256 = Hash(Path.Combine(Application.dataPath, "RotateRings", "Scripts", "RotateRingsRuntime.cs"));
        report.collisionWorldSha256 = Hash(Path.Combine(Application.dataPath, "RotateRings", "Scripts", "RotateRingsCollisionWorld.cs"));
        report.worldViewSha256 = Hash(Path.Combine(Application.dataPath, "RotateRings", "Scripts", "RotateRingsWorldView.cs"));
        report.levelPackSha256 = Hash(Path.Combine(Application.dataPath, PackRelativePath));
        report.templateCatalogSha256 = Hash(Path.Combine(Application.dataPath, CatalogRelativePath));
        report.movesBefore = movesBefore;
        report.inputStartScreen = downScreen;
        report.inputEndScreen = upScreen;
        report.hintDidNotChangeGameState = true;
        report.dragUsedInputSystem = true;
        SaveReport(report);
        activeDragIsSlider = false;
        SessionState.SetString(Key + "phase", "wait-entry-ring");
    }

    static void WaitForEntryAndQueuePress() {
        var world = GetField<RotateRingsWorldView>(runtime, "world");
        if (world == null || world.IsEntering) return;
        // StartLevel already built this board; wait for its entry wave rather than rebuilding it.
        QueueMouse(downScreen, true);
        SessionState.SetInt(Key + "events", SessionState.GetInt(Key + "events", 0) + 1);
        waitFrames = 3;
        SessionState.SetString(Key + "phase", "wait-press");
    }

    static void WaitForPress() {
        if (--waitFrames > 0) return;
        object drag = GetField<object>(runtime, "drag");
        if (drag == null) throw new InvalidOperationException("Mouse down did not enter the runtime PollInput drag path.");
        var dragReport = LoadReport();
        dragReport.actualDragPartId = ReadInt(drag, "id");
        dragReport.actualDragIsSlider = GetField<bool>(drag, "slider");
        SaveReport(dragReport);
        if (!activeDragIsSlider && (dragReport.actualDragPartId != targetPart.id || dragReport.actualDragIsSlider))
            throw new InvalidOperationException("Ring pointer hit part " + dragReport.actualDragPartId + " (slider=" + dragReport.actualDragIsSlider + "), expected ring " + targetPart.id + ".");
        if (activeDragIsSlider) {
            if (dragReport.actualDragPartId != targetPart.id || !dragReport.actualDragIsSlider)
                throw new InvalidOperationException("Slider pointer hit part " + dragReport.actualDragPartId + " (slider=" + dragReport.actualDragIsSlider + "), expected slider " + targetPart.id + ".");
        }
        QueueMouse(upScreen, true);
        SessionState.SetInt(Key + "events", SessionState.GetInt(Key + "events", 0) + 1);
        waitFrames = 3;
        SessionState.SetString(Key + "phase", "wait-move");
    }

    static void WaitForMove() {
        if (--waitFrames > 0) return;
        float current = ReadAngles()[targetPart.id];
        if (activeDragIsSlider) {
            object activeDrag = GetField<object>(runtime, "drag");
            float actualTravel = ReadFloat(activeDrag, "delta");
            var report = LoadReport();
            report.sliderObservedDelta = actualTravel;
            SaveReport(report);
            if (actualTravel < sliderTravel - .05f)
                throw new InvalidOperationException("Held mouse movement reached " + actualTravel + ", below slider release distance " + sliderTravel + ".");
        } else if (Mathf.Abs(Mathf.DeltaAngle(angleBefore, current)) < 1f) {
            object activeDrag = GetField<object>(runtime, "drag");
            var report = LoadReport();
            report.angleAtMove = current;
            report.dragDeltaAtMove = ReadFloat(activeDrag, "delta");
            report.pointerAngleAtMove = ReadFloat(activeDrag, "lastPointerAngle");
            SaveReport(report);
            throw new InvalidOperationException("Held mouse movement did not rotate the target ring; angle=" + current + ", dragDelta=" + report.dragDeltaAtMove + ", pointerAngle=" + report.pointerAngleAtMove + ".");
        }
        QueueMouse(upScreen, false);
        SessionState.SetInt(Key + "events", SessionState.GetInt(Key + "events", 0) + 1);
        waitFrames = 3;
        SessionState.SetString(Key + "phase", "wait-release");
    }

    static void WaitForRelease() {
        if (--waitFrames > 0) return;
        var report = LoadReport();
        if (activeDragIsSlider) {
            FinishSliderSmoke(report);
            return;
        }
        object state = GetField<object>(runtime, "state");
        report.movesAfterDrag = ReadInt(state, "moves");
        report.angleAfterDrag = ReadAngles()[targetPart.id];
        report.inputEventsQueued = SessionState.GetInt(Key + "events", 0);
        report.dragUsedInputSystem = true;
        SaveReport(report);
        if (report.movesAfterDrag != movesBefore + 1)
            throw new InvalidOperationException("Release did not register exactly one gameplay move; got " + report.movesAfterDrag + ".");
        if (Mathf.Abs(Mathf.DeltaAngle(report.expectedAbsoluteAngle, report.angleAfterDrag)) > 1.5f)
            throw new InvalidOperationException("Drag did not reach the signed saved gesture angle; actual=" + report.angleAfterDrag + ", expected=" + report.expectedAbsoluteAngle + ".");
        if (GetField<object>(runtime, "mode").ToString() == "Win")
            throw new InvalidOperationException("Selected smoke gesture unexpectedly completed the level; choose an earlier non-winning action.");

        InvokeSettingsAction("UndoButton");
        state = GetField<object>(runtime, "state");
        report.movesAfterUndo = ReadInt(state, "moves");
        bool undoRestored = report.movesAfterUndo == movesBefore &&
            Mathf.Abs(Mathf.DeltaAngle(angleBefore, ReadAngles()[targetPart.id])) < .1f;
        report.undoRestoredSnapshot = undoRestored;
        if (!undoRestored) throw new InvalidOperationException("Undo did not restore the pre-drag angle and move count.");

        InvokeSettingsAction("RestartButton");
        state = GetField<object>(runtime, "state");
        report.movesAfterRestart = ReadInt(state, "moves");
        report.restartRestoredLevel = report.movesAfterRestart == 0 &&
            Mathf.Abs(Mathf.DeltaAngle(targetPart.angle, ReadAngles()[targetPart.id])) < .1f &&
            ReadSet(state, "removed").Count == 0 && ReadSet(state, "keys").Count == 0;
        if (!report.restartRestoredLevel) throw new InvalidOperationException("Restart did not restore the level's initial state.");

        var beforeHint = CaptureGameState();
        InvokePrivate(runtime, "Hint");
        report.hintDidNotChangeGameState &= beforeHint.Equals(CaptureGameState());
        if (!report.hintDidNotChangeGameState) throw new InvalidOperationException("Hint advanced or mutated gameplay state after restart.");
        report.sliderActionAvailable = false;
        report.status = "passed";
        report.finishedUtc = DateTime.UtcNow.ToString("o");
        report.note = sourceReferenceSmoke
            ? "Real Input System MouseState press/move/release events exercised the source-reference ring PollInput path on source fixture 10001; undo, restart, and hint snapshots were checked. Slider input was not part of this ring-only fixture. No build was created."
            : "Real Input System MouseState press/move/release events exercised the imported ring PollInput path. Slider input is outside the ring-only fixture subset. No build was created.";
        SaveReport(report);
        Complete();
    }

    static void BeginSliderSmoke(SmokeReport report) {
        var pack = GetField<RotateRingsLevelPack>(runtime, "pack");
        if (pack == null || pack.levels == null) throw new InvalidOperationException("Loaded level pack is missing.");
        RotateRingsPart selectedSlider = null;
        RotateRingsGesture selectedGesture = null;
        foreach (var candidateLevel in pack.levels) {
            InvokePrivate(runtime, "StartLevel", candidateLevel.id);
            level = GetField<RotateRingsLevel>(runtime, "level");
            if (level == null || level.solution == null) continue;
            foreach (var gesture in level.solution) {
                if (gesture == null || !string.Equals(gesture.kind, "slide", StringComparison.OrdinalIgnoreCase)) continue;
                var part = level.parts.FirstOrDefault(p => p.id == gesture.part && RotateRingsRules.IsSlider(p));
                if (part == null || Convert.ToBoolean(InvokePrivate(runtime, "HasActiveOutgoing", part.id))) continue;
                if (!TryFindSliderPointer(part, out Vector2 pointerWorld)) continue;
                if (Camera.main == null) continue;
                Vector3 downScreenPoint = Camera.main.WorldToScreenPoint(pointerWorld);
                float candidateTravel = Mathf.Max(part.radius * 1.25f, .3f) + .2f;
                Vector2 axis = new Vector2(Mathf.Cos(part.axis * Mathf.Deg2Rad), Mathf.Sin(part.axis * Mathf.Deg2Rad));
                Vector3 upScreenPoint = Camera.main.WorldToScreenPoint(pointerWorld + axis * candidateTravel);
                if (downScreenPoint.z < 0f || upScreenPoint.z < 0f) continue;
                selectedSlider = part;
                selectedGesture = gesture;
                sliderTravel = candidateTravel;
                downScreen = downScreenPoint;
                upScreen = upScreenPoint;
                break;
            }
            if (selectedSlider != null) break;
        }
        if (selectedSlider == null) {
            report.sliderActionAvailable = false;
            report.status = "passed";
            report.finishedUtc = DateTime.UtcNow.ToString("o");
            report.note = "Ring input smoke passed. No immediately legal saved slider gesture was found in the pack; slider input was not exercised. No build was created.";
            SaveReport(report);
            Complete();
            return;
        }

        targetPart = selectedSlider;
        targetGesture = selectedGesture;
        activeDragIsSlider = true;
        sliderTravel = Mathf.Max(selectedSlider.radius * 1.25f, .3f) + .2f;
        object state = GetField<object>(runtime, "state");
        movesBefore = ReadInt(state, "moves");
        report.sliderActionAvailable = true;
        report.sliderInputSystem = true;
        report.sliderLevelId = level.id;
        report.sliderPartId = selectedSlider.id;
        report.sliderMovesBefore = movesBefore;
        report.sliderTravel = sliderTravel;
        report.inputStartScreen = downScreen;
        report.inputEndScreen = upScreen;
        report.note = "Ring smoke passed; queuing a legal slider drag at Level " + level.id + ".";
        SaveReport(report);
        SessionState.SetString(Key + "phase", "wait-entry-slider");
    }

    static bool TryFindSliderPointer(RotateRingsPart slider, out Vector2 pointerWorld) {
        var liveWorld = GetField<RotateRingsWorldView>(runtime, "world");
        Vector2 axis = new Vector2(Mathf.Cos(slider.axis * Mathf.Deg2Rad), Mathf.Sin(slider.axis * Mathf.Deg2Rad));
        float[] offsets = { -slider.length * .45f, -slider.length * .35f, -slider.length * .2f, 0f, slider.length * .2f, slider.length * .35f, slider.length * .45f };
        foreach (float offset in offsets) {
            Vector2 point = new Vector2(slider.x, slider.y) + axis * offset;
            if (!liveWorld.TryHit(point, out int hitId) || hitId != slider.id) continue;
            pointerWorld = point;
            return true;
        }
        pointerWorld = default;
        return false;
    }

    static void FinishSliderSmoke(SmokeReport report) {
        object state = GetField<object>(runtime, "state");
        report.inputEventsQueued = SessionState.GetInt(Key + "events", 0);
        report.sliderMovesAfter = ReadInt(state, "moves");
        report.sliderRemoved = ReadSet(state, "removed").Contains(targetPart.id);
        if (report.sliderMovesAfter != movesBefore + 1 || !report.sliderRemoved)
            throw new InvalidOperationException("Input System slider drag did not remove the bar and register exactly one move.");

        bool didWin = GetField<object>(runtime, "mode").ToString() == "Win";
        if (didWin) {
            InvokePrivate(runtime, "StartLevel", level.id);
            report.sliderUndoRestored = true;
        } else {
            InvokeSettingsAction("UndoButton");
            state = GetField<object>(runtime, "state");
            report.sliderUndoRestored = !ReadSet(state, "removed").Contains(targetPart.id) && ReadInt(state, "moves") == movesBefore;
            InvokeSettingsAction("RestartButton");
        }
        if (!report.sliderUndoRestored) throw new InvalidOperationException("Slider undo/restart did not restore the slider state.");

        report.status = "passed";
        report.finishedUtc = DateTime.UtcNow.ToString("o");
        report.note = "Real Input System MouseState press/move/release events exercised ring and slider PollInput paths. Slider level was reset after the smoke; player progress was restored. No build was created.";
        SaveReport(report);
        Complete();
    }

    static RotateRingsGesture FindImmediateSolutionRotate() {
        object state = GetField<object>(runtime, "state");
        var collision = GetField<RotateRingsCollisionWorld>(runtime, "collision");
        var keys = ReadSet(state, "keys");
        foreach (var gesture in level.solution) {
            if (gesture == null || !string.Equals(gesture.kind, "rotate", StringComparison.OrdinalIgnoreCase)) continue;
            var part = level.parts.FirstOrDefault(p => p.id == gesture.part);
            if (part == null || RotateRingsRules.IsSlider(part) || (part.lockKey != 0 && !keys.Contains(part.lockKey))) continue;
            if (Convert.ToBoolean(InvokePrivate(runtime, "HasActiveOutgoing", part.id))) continue;
            float angle = ReadAngles()[part.id];
            float threshold = Convert.ToBoolean(InvokePrivate(runtime, "HasActiveIncident", part.id)) ? 2f : 15f;
            float travel = GestureTravel(angle, gesture);
            // A single mouse event moves to one point on the ring; limit this smoke gesture to avoid
            // ambiguity from pointer-angle wrap while still checking the signed PhysicsV2 solution path.
            if (Mathf.Abs(travel) >= threshold && Mathf.Abs(travel) < 150f && collision.CanTurnAndReleaseDelta(part.id, travel)) return gesture;
        }
        return null;
    }

    static RotateRingsGesture FindSourceReferenceSmokeRotate() {
        // This first source fixture has a reproducible 135 degree legal partial
        // turn on ring 2. It exercises PollInput without removing a ring or
        // depending on a withdrawn/generated gameplay pack.
        var part = level.parts.FirstOrDefault(p => p.id == 2 && p.kind == "ring");
        var collision = GetField<RotateRingsCollisionWorld>(runtime, "collision");
        if (part == null || collision == null) return null;
        const float delta = 135.18512f;
        float startAngle = collision.GetAngle(part.id);
        collision.BeginGesture(part.id);
        float reached = collision.ClampRotation(part.id, startAngle, delta, out bool blocked);
        collision.EndGesture(part.id, blocked ? Mathf.Sign(delta) : 0f);
        if (blocked || Mathf.Abs(reached - startAngle - delta) > .05f) return null;

        InvokePrivate(runtime, "StartLevel", level.id);
        level = GetField<RotateRingsLevel>(runtime, "level");
        part = level.parts.First(p => p.id == 2 && p.kind == "ring");
        return new RotateRingsGesture { part = part.id, kind = "rotate", angle = part.angle + delta, delta = delta };
    }

    static float GestureTravel(float fromAngle, RotateRingsGesture gesture) {
        float travel = Mathf.DeltaAngle(fromAngle, gesture.angle);
        // New PhysicsV2 gesture payloads may preserve a signed delta; older fixture schemas contain only the target angle.
        var deltaField = gesture.GetType().GetField("delta", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (deltaField != null) {
            float direction = Convert.ToSingle(deltaField.GetValue(gesture));
            if (direction > 0f && travel < 0f) travel += 360f;
            if (direction < 0f && travel > 0f) travel -= 360f;
        }
        return travel;
    }

    static bool HasPhysicsV2RingOnlyGeometry(RotateRingsLevel candidate) {
        if (candidate == null || candidate.parts == null || candidate.parts.Count == 0) return false;
        var catalog = GetField<RotateRingsTemplateCatalog>(runtime, "templates");
        if (catalog == null || catalog.templates == null) return false;
        foreach (var part in candidate.parts) {
            if (part == null || RotateRingsRules.IsSlider(part) || string.IsNullOrEmpty(part.templateId)) return false;
            var template = catalog.templates.FirstOrDefault(t => t.id == part.templateId);
            if (template == null || template.physicalShapes == null ||
                !template.physicalShapes.Any(shape => shape.role == "body" && !shape.trigger)) return false;
        }
        return true;
    }

    sealed class GameSnapshot : IEquatable<GameSnapshot> {
        public int moves;
        public Dictionary<int, float> angles;
        public HashSet<int> removed;
        public HashSet<int> keys;
        public bool Equals(GameSnapshot other) {
            if (other == null || moves != other.moves || removed.SetEquals(other.removed) == false || keys.SetEquals(other.keys) == false || angles.Count != other.angles.Count) return false;
            foreach (var pair in angles) if (!other.angles.TryGetValue(pair.Key, out float value) || Mathf.Abs(Mathf.DeltaAngle(pair.Value, value)) > .001f) return false;
            return true;
        }
    }

    static GameSnapshot CaptureGameState() {
        object state = GetField<object>(runtime, "state");
        return new GameSnapshot {
            moves = ReadInt(state, "moves"),
            angles = new Dictionary<int, float>(ReadAngles()),
            removed = new HashSet<int>(ReadSet(state, "removed")),
            keys = new HashSet<int>(ReadSet(state, "keys"))
        };
    }

    static int CountCollectibles() => level.parts.Count(p => p.collectible);
    static Dictionary<int, float> ReadAngles() => GetField<Dictionary<int, float>>(GetField<object>(runtime, "state"), "angles");
    static HashSet<int> ReadSet(object instance, string name) => GetField<HashSet<int>>(instance, name);
    static int ReadInt(object instance, string name) => GetField<int>(instance, name);
    static float ReadFloat(object instance, string name) => GetField<float>(instance, name);
    static T GetField<T>(object instance, string name) {
        if (instance == null) throw new InvalidOperationException("Cannot read " + name + " from null instance.");
        var field = instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field == null) throw new MissingFieldException(instance.GetType().FullName, name);
        return (T)field.GetValue(instance);
    }
    static object InvokePrivate(object instance, string name, params object[] args) {
        var method = instance.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method == null) throw new MissingMethodException(instance.GetType().FullName, name);
        return method.Invoke(instance, args);
    }

    static void InvokeButton(string gameObjectName) {
        var button = Resources.FindObjectsOfTypeAll<Button>().FirstOrDefault(b =>
            b != null && b.gameObject.scene.IsValid() && b.isActiveAndEnabled && b.gameObject.name == gameObjectName);
        if (button == null) throw new InvalidOperationException("Runtime button not found: " + gameObjectName);
        button.onClick.Invoke();
    }

    static void InvokeSettingsAction(string settingsActionButtonName) {
        InvokeButton("SettingsButton");
        InvokeButton(settingsActionButtonName);
    }

    static void QueueMouse(Vector2 screenPosition, bool leftButton) {
        if (qaMouse == null || !qaMouse.added) throw new InvalidOperationException("Input System QA Mouse device disappeared.");
        var state = new MouseState { position = screenPosition };
        if (leftButton) state = state.WithButton(MouseButton.Left);
        qaMouse.MakeCurrent();
        InputSystem.QueueStateEvent(qaMouse, state);
    }

    static void EnsureVirtualMouse() {
        if (qaMouse != null && qaMouse.added) return;
        physicalMouse = Mouse.current;
        qaMouse = InputSystem.AddDevice<Mouse>("RotateRings QA Virtual Mouse");
        if (physicalMouse != null && physicalMouse.added && physicalMouse.enabled)
            InputSystem.DisableDevice(physicalMouse);
        qaMouse.MakeCurrent();
    }

    static void RestoreVirtualMouse() {
        if (qaMouse != null && qaMouse.added) {
            if (qaMouse.enabled) InputSystem.DisableDevice(qaMouse);
            InputSystem.RemoveDevice(qaMouse);
        }
        qaMouse = null;
        if (physicalMouse != null && physicalMouse.added) {
            if (!physicalMouse.enabled) InputSystem.EnableDevice(physicalMouse);
            physicalMouse.MakeCurrent();
        }
        physicalMouse = null;
    }

    static SmokeReport NewReport() => new SmokeReport {
        status = "running",
        startedUtc = SessionState.GetString(Key + "startedUtc", DateTime.UtcNow.ToString("o")),
        unityVersion = Application.unityVersion,
        instance = Application.productName,
        levelId = 1
    };
    static SmokeReport LoadReport() {
        string json = SessionState.GetString(Key + "report", string.Empty);
        return string.IsNullOrEmpty(json) ? NewReport() : JsonUtility.FromJson<SmokeReport>(json);
    }
    static void SaveReport(SmokeReport report) => SessionState.SetString(Key + "report", JsonUtility.ToJson(report, true));

    static void Fail(string message) {
        SmokeReport report = LoadReport();
        report.status = "failed";
        report.error = message;
        report.finishedUtc = DateTime.UtcNow.ToString("o");
        report.inputEventsQueued = SessionState.GetInt(Key + "events", 0);
        SaveReport(report);
        Complete();
    }

    static void Complete() {
        RestoreProgress();
        RestoreVirtualMouse();
        RestoreRunInBackground();
        PersistReportIfPresent();
        SessionState.SetBool(Key + "running", false);
        SessionState.SetString(Key + "phase", string.Empty);
        RemoveTickSubscription();
        if (EditorApplication.isPlayingOrWillChangePlaymode) EditorApplication.ExitPlaymode();
    }

    static void EnsureTickSubscribed() {
        if (tickSubscribed) return;
        EditorApplication.update += Tick;
        tickSubscribed = true;
    }

    static void FocusGameView() {
        Type gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
        if (gameViewType != null) EditorWindow.GetWindow(gameViewType).Focus();
    }

    static void RemoveTickSubscription() {
        if (!tickSubscribed) return;
        EditorApplication.update -= Tick;
        tickSubscribed = false;
    }

    static void RestoreProgress() {
        if (!SessionState.GetBool(Key + "running", false)) return;
        bool hadKey = SessionState.GetBool(Key + "progressHadKey", false);
        if (hadKey) PlayerPrefs.SetInt(ProgressKey, SessionState.GetInt(Key + "progressBefore", 1));
        else PlayerPrefs.DeleteKey(ProgressKey);
        PlayerPrefs.Save();
    }

    static void RestoreRunInBackground() {
        if (!SessionState.GetBool(Key + "runInBackgroundSaved", false)) return;
        Application.runInBackground = SessionState.GetBool(Key + "runInBackgroundBefore", false);
        SessionState.SetBool(Key + "runInBackgroundSaved", false);
    }

    static void PersistReportIfPresent() {
        string json = SessionState.GetString(Key + "report", string.Empty);
        if (string.IsNullOrEmpty(json)) return;
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string outputRoot = Directory.GetParent(projectRoot).FullName;
        string path = Path.Combine(outputRoot, "Research", "physics_v2", "input_smoke_report.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, json);
        Debug.Log("Rotate Rings input smoke report: " + path);
    }

    static string Hash(string path) {
        if (!File.Exists(path)) return string.Empty;
        using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant();
    }
}
#endif

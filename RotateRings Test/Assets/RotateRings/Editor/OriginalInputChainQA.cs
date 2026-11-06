#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Spine.Unity;

/// <summary>Bounded source-fixture contract checks for native input and chain visuals.</summary>
public static class OriginalInputChainQA
{
    const string MenuPath = "Tools/Rotate Rings/QA/Original Input and Chain Contracts";
    static readonly int[] FixtureIds = { 10001, 10010, 10012, 10154, 10109, 10166, 10190, 140004 };
    static readonly int[] FixedPullFixtureIds = { 10010, 10012, 10154, 10109, 10166 };

    [Serializable]
    sealed class Report
    {
        public string status;
        public string generatedUtc;
        public string sourcePack;
        public int fixturesChecked;
        public int movableShapesChecked;
        public int nativeRayMask;
        public int chainFixtureId;
        public int chainSourcePartId;
        public string chainSourcePart;
        public int chainSourceActualCount;
        public List<int> chainSegmentInstanceIds = new List<int>();
        public List<string> chainLifecycleSnapshots = new List<string>();
        public List<string> passed = new List<string>();
        public List<string> failed = new List<string>();
        public string engineExecution = "Not run by this harness; invoke menu item in Unity Editor.";
    }

    [Serializable] sealed class MotionModules { public MotionObject[] objects; }
    [Serializable] sealed class MotionObject { public string type; public long pathId; public MotionSystem tree; }
    [Serializable] sealed class MotionSystem
    {
        public float lengthInSec;
        public InitialModule InitialModule;
        public EmissionModule EmissionModule;
    }
    [Serializable] sealed class InitialModule { public Curve startLifetime; public Curve startDelay; }
    [Serializable] sealed class Curve { public int minMaxState; public float scalar; public float minScalar; }
    [Serializable] sealed class EmissionModule { public bool enabled; }

    sealed class ChainLifecycleRun
    {
        public Report report;
        public RotateRingsLevel fixture;
        public GameObject root;
        public RotateRingsWorldView world;
        public GameObject chainRoot;
        public Transform[] segments = new Transform[3];
        public int[] segmentIds = new int[3];
        public int targetId;
        public float phaseStarted;
        public float breakDuration;
        public int phase;
    }
    static ChainLifecycleRun activeChainRun;

    [MenuItem(MenuPath)]
    static void Run()
    {
        var report = new Report { generatedUtc = DateTime.UtcNow.ToString("o"), nativeRayMask = (1 << 6) | (1 << 7) };
        GameObject temporary = null;
        bool asyncChainRun = false;
        try
        {
            string researchRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Research/physics_v3"));
            string packPath = Path.Combine(researchRoot, "source_parity_pack.json");
            string catalogPath = Path.Combine(researchRoot, "source_templates.json");
            report.sourcePack = packPath;
            var pack = JsonUtility.FromJson<RotateRingsLevelPack>(File.ReadAllText(packPath));
            var catalog = JsonUtility.FromJson<RotateRingsTemplateCatalog>(File.ReadAllText(catalogPath));
            if (pack == null || pack.levels == null || catalog == null || catalog.templates == null)
                throw new InvalidOperationException("Could not load original source fixtures or templates.");

            CheckFixtures(pack, report);
            CheckGuideForwarding(pack, report);
            CheckMotionCleanupEvidence(researchRoot, report);
            CheckNativeRayAndLockDepth(pack, catalog, report, out temporary);
            CheckChainSlots(report, ref temporary);
            CheckAuthoredPinLifecycle(pack, catalog, report);
            if (!Application.isPlaying)
                throw new InvalidOperationException("The WorldView chain lifecycle check requires Unity Play Mode. Enter Play Mode and run this menu item again.");
            StartChainLifecycle(pack, catalog, report);
            asyncChainRun = true;
        }
        catch (Exception exception)
        {
            report.failed.Add(exception.GetType().Name + ": " + exception.Message);
        }
        finally
        {
            if (temporary != null) UnityEngine.Object.DestroyImmediate(temporary);
            if (!asyncChainRun) FinishReport(report);
        }
    }

    static void FinishReport(Report report)
    {
        report.status = report.failed.Count == 0 ? "PASS" : "FAIL";
        string reportPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Research/original_input_chain_qa.json"));
        Directory.CreateDirectory(Path.GetDirectoryName(reportPath));
        File.WriteAllText(reportPath, JsonUtility.ToJson(report, true));
        Debug.Log("Original input/chain contract QA " + report.status + " (" + report.passed.Count + " passed, " + report.failed.Count + " failed): " + reportPath);
        if (report.failed.Count != 0) Debug.LogError(string.Join("\n", report.failed));
    }

    static void StartChainLifecycle(RotateRingsLevelPack pack, RotateRingsTemplateCatalog catalog, Report report)
    {
        if (activeChainRun != null) throw new InvalidOperationException("A chain lifecycle QA run is already active.");
        RotateRingsLevel source = pack.levels.Find(level => level.id == 10166);
        Require(source != null, "Missing source level 10166 for WorldView chain lifecycle QA.");
        RotateRingsPart sourcePart = source.parts.Where(part => part.kind == "ring" && part.chainLockCount >= 3)
            .OrderByDescending(part => part.chainLockCount).FirstOrDefault();
        Require(sourcePart != null, "Source level 10166 has no ring with at least three authored chain locks.");

        // Work on a private clone: preserve the fixture's true source count in the report while driving
        // the supported native three-segment lifecycle through the real WorldView API.
        RotateRingsLevel fixture = JsonUtility.FromJson<RotateRingsLevel>(JsonUtility.ToJson(source));
        RotateRingsPart target = fixture.parts.Find(part => part.id == sourcePart.id);
        int sourceActualCount = target.chainLockCount;
        target.chainLockCount = 3;
        report.chainFixtureId = source.id;
        report.chainSourcePartId = sourcePart.id;
        report.chainSourcePart = sourcePart.sourceId;
        report.chainSourceActualCount = sourceActualCount;

        var runner = new ChainLifecycleRun { report = report, fixture = fixture, targetId = target.id };
        runner.root = new GameObject("Original WorldView Chain Lifecycle QA") { hideFlags = HideFlags.HideAndDontSave };
        runner.root.transform.position = new Vector3(10000f, 10000f, 0f);
        try
        {
            runner.world = runner.root.AddComponent<RotateRingsWorldView>();
            runner.world.Build(fixture, catalog);
            FieldInfo partsField = typeof(RotateRingsWorldView).GetField("parts", BindingFlags.Instance | BindingFlags.NonPublic);
            Require(partsField != null, "Could not inspect WorldView part lifecycle for QA.");
            object partView = partsField.GetValue(runner.world);
            Require(partView != null, "WorldView part table was not initialized.");
            object targetView = ((System.Collections.IDictionary)partView)[target.id];
            Require(targetView != null, "WorldView did not create target ring part " + target.id + ".");
            Type viewType = targetView.GetType();
            FieldInfo chainRootField = viewType.GetField("chainRoot", BindingFlags.Instance | BindingFlags.NonPublic);
            Require(chainRootField != null, "Could not inspect native chain root for QA.");
            runner.chainRoot = (GameObject)chainRootField.GetValue(targetView);
            Require(runner.chainRoot != null, "WorldView did not create the source chain root for the selected ring.");
            for (int slot = 0; slot < 3; slot++)
            {
                runner.segments[slot] = OriginalChainVisualAssets.GetChainSegment(runner.chainRoot, slot);
                Require(runner.segments[slot] != null, "WorldView chain is missing authored segment slot " + slot + ".");
                runner.segmentIds[slot] = runner.segments[slot].GetInstanceID();
                report.chainSegmentInstanceIds.Add(runner.segmentIds[slot]);
            }
            var angles = new Dictionary<int, float>();
            foreach (RotateRingsPart part in fixture.parts) angles[part.id] = part.angle;
            runner.world.ApplyState(angles, new HashSet<int>(), new HashSet<int>(), new List<bool>());
            runner.world.ApplyChainState(ChainCounts(fixture, target.id, 3));
            Require(AllSlotsActive(runner, true, true, true), "Initial Play Mode WorldView chain count 3 must show all authored slots.");

            activeChainRun = runner;
            runner.phase = 0;
            runner.phaseStarted = Time.realtimeSinceStartup;
            report.engineExecution = "Running bounded WorldView chain lifecycle in Unity Play Mode.";
            EditorApplication.update += TickChainLifecycle;
        }
        catch
        {
            if (runner.root != null) UnityEngine.Object.DestroyImmediate(runner.root);
            throw;
        }
    }

    static Dictionary<int, int> ChainCounts(RotateRingsLevel level, int targetId, int targetCount)
    {
        var counts = new Dictionary<int, int>();
        foreach (RotateRingsPart part in level.parts) if (part.kind == "ring") counts[part.id] = part.id == targetId ? targetCount : part.chainLockCount;
        return counts;
    }

    static bool AllSlotsActive(ChainLifecycleRun run, bool a, bool b, bool c)
    {
        return run.segments[0].gameObject.activeSelf == a && run.segments[1].gameObject.activeSelf == b && run.segments[2].gameObject.activeSelf == c;
    }

    static string Snapshot(ChainLifecycleRun run, string label)
    {
        var clips = new List<string>();
        for (int i = 0; i < run.segments.Length; i++)
        {
            SkeletonAnimation animation = run.segments[i].GetComponentInChildren<SkeletonAnimation>(true);
            var track = animation != null ? animation.AnimationState.GetCurrent(0) : null;
            clips.Add(track != null ? track.Animation.Name : "none");
        }
        string value = label + ":active=" +
            (run.segments[0].gameObject.activeSelf ? "1" : "0") +
            (run.segments[1].gameObject.activeSelf ? "1" : "0") +
            (run.segments[2].gameObject.activeSelf ? "1" : "0") +
            ";clips=" + string.Join(",", clips);
        run.report.chainLifecycleSnapshots.Add(value);
        return value;
    }

    static void TickChainLifecycle()
    {
        ChainLifecycleRun run = activeChainRun;
        if (run == null) { EditorApplication.update -= TickChainLifecycle; return; }
        try
        {
            if (!Application.isPlaying) throw new InvalidOperationException("Play Mode ended before chain lifecycle QA completed.");
            float elapsed = Time.realtimeSinceStartup - run.phaseStarted;
            if (Time.realtimeSinceStartup - run.phaseStarted > 12f)
                throw new TimeoutException("WorldView chain lifecycle QA exceeded its 12 second bound at phase " + run.phase + ".");

            switch (run.phase)
            {
                case 0:
                    Snapshot(run, "count3_initial");
                    Require(AllSlotsActive(run, true, true, true), "Count 3 did not keep all slots active.");
                    run.world.ApplyState(new Dictionary<int, float>(), new HashSet<int>(), new HashSet<int>(), new List<bool>(), true, run.targetId);
                    string shake = Snapshot(run, "wobble_shake");
                    Require(CurrentClipName(run.segments[0]) == "rr_suolian_" + DirectionName(run.segments[0]) + "_huangdong" ||
                            CurrentClipName(run.segments[1]) == "rr_suolian_" + DirectionName(run.segments[1]) + "_huangdong" ||
                            CurrentClipName(run.segments[2]) == "rr_suolian_" + DirectionName(run.segments[2]) + "_huangdong",
                        "ApplyState wobble did not start a source huangdong Spine animation: " + shake);
                    run.world.ApplyChainState(ChainCounts(GetFixtureForRun(run), run.targetId, 2));
                    Require(AllSlotsActive(run, true, true, true), "3-to-2 chain break must retain all three slots while the break clip plays.");
                    Snapshot(run, "count3_to2_t0");
                    run.phase = 1; run.phaseStarted = Time.realtimeSinceStartup;
                    break;
                case 1:
                    if (elapsed >= .2f)
                    {
                        string atTwo = Snapshot(run, "count3_to2_t0_2");
                        Require(CurrentClipName(run.segments[2]).Contains("break"), "3-to-2 segment did not enter its source break clip by 0.2s: " + atTwo);
                        Require(run.segments[2].gameObject.activeSelf, "Breaking authored slot 2 must remain visible at 0.2s.");
                        run.breakDuration = CurrentClipDuration(run.segments[2]);
                        Require(run.breakDuration > .2f, "Source chain break clip duration should exceed the 0.2s visibility checkpoint.");
                        run.phase = 2; run.phaseStarted = Time.realtimeSinceStartup;
                    }
                    break;
                case 2:
                    if (elapsed >= run.breakDuration + .08f)
                    {
                        Require(AllSlotsActive(run, true, true, false), "3-to-2 must leave only authored slots 0 and 1 active after slot 2 finishes.");
                        Snapshot(run, "count3_to2_clip_end");
                        run.world.ApplyChainState(ChainCounts(GetFixtureForRun(run), run.targetId, 1));
                        Require(AllSlotsActive(run, true, true, false), "2-to-1 must retain slot 0, keep slot 1 for its break, and never resurrect slot 2.");
                        Snapshot(run, "count2_to1_t0");
                        run.phase = 3; run.phaseStarted = Time.realtimeSinceStartup;
                    }
                    break;
                case 3:
                    if (elapsed >= .2f)
                    {
                        string atTwo = Snapshot(run, "count2_to1_t0_2");
                        Require(CurrentClipName(run.segments[1]).Contains("break"), "2-to-1 segment did not enter its source break clip by 0.2s: " + atTwo);
                        Require(AllSlotsActive(run, true, true, false), "At 0.2s of 2-to-1, slots 0 and 1 must remain visible and slot 2 must remain hidden.");
                        run.breakDuration = CurrentClipDuration(run.segments[1]);
                        Require(run.breakDuration > .2f, "Source chain break clip duration should exceed the 0.2s visibility checkpoint.");
                        run.phase = 4; run.phaseStarted = Time.realtimeSinceStartup;
                    }
                    break;
                case 4:
                    if (elapsed >= run.breakDuration + .08f)
                    {
                        Require(AllSlotsActive(run, true, false, false), "2-to-1 must leave only authored slot 0 active after slot 1 finishes.");
                        Snapshot(run, "count2_to1_clip_end");
                        run.world.ApplyChainState(ChainCounts(GetFixtureForRun(run), run.targetId, 0));
                        Require(run.chainRoot.activeSelf && run.segments[0].gameObject.activeSelf,
                            "Count 0 must keep the chain root and final segment visible while the last break plays.");
                        Require(AllSlotsActive(run, true, false, false), "1-to-0 must preserve only the surviving authored slot until its break finishes.");
                        Snapshot(run, "count1_to0_t0");
                        run.phase = 5; run.phaseStarted = Time.realtimeSinceStartup;
                    }
                    break;
                case 5:
                    if (elapsed >= .2f)
                    {
                        string atTwo = Snapshot(run, "count1_to0_t0_2");
                        Require(run.chainRoot.activeSelf && run.segments[0].gameObject.activeSelf,
                            "Final segment/root must remain visible at 0.2s of the break.");
                        Require(AllSlotsActive(run, true, false, false), "At 0.2s of 1-to-0, slot 0 must remain visible while slots 1 and 2 remain hidden.");
                        run.phase = 7; run.phaseStarted = Time.realtimeSinceStartup;
                    }
                    break;
                case 7:
                    // The source stagger schedules the third broken segment 0.34s after the first.
                    if (elapsed >= .16f)
                    {
                        string atBreak = Snapshot(run, "count1_to0_staggered_break_start");
                        Require(CurrentClipName(run.segments[0]).Contains("break"), "1-to-0 segment did not enter its source break clip after the authored stagger: " + atBreak);
                        run.breakDuration = CurrentClipDuration(run.segments[0]);
                        Require(run.breakDuration > 0f, "Source chain break clip duration should be positive.");
                        run.phase = 6; run.phaseStarted = Time.realtimeSinceStartup;
                    }
                    break;
                case 6:
                    if (elapsed >= run.breakDuration + .08f)
                    {
                        Require(!run.chainRoot.activeSelf && !run.segments[0].gameObject.activeSelf,
                            "Count 0 chain root should hide after the final break clip completes.");
                        Snapshot(run, "count1_to0_clip_end");
                        run.world.ApplyChainState(ChainCounts(GetFixtureForRun(run), run.targetId, 3));
                        Require(run.chainRoot.activeSelf && AllSlotsActive(run, true, true, true),
                            "Undo to count 3 must restore all original segment slots.");
                        for (int i = 0; i < 3; i++)
                            Require(OriginalChainVisualAssets.GetChainSegment(run.chainRoot, i).GetInstanceID() == run.segmentIds[i],
                                "Undo replaced stable native segment instance at slot " + i + ".");
                        Snapshot(run, "undo_to3");
                        run.report.passed.Add("Play Mode WorldView lifecycle on source fixture 10166 part " + run.report.chainSourcePart + " (source count " + run.report.chainSourceActualCount + "): wobble starts huangdong; stable survivor slots persist through 3→2→1→0 without teleporting or resurrecting; broken slots remain visible through the 0.2s checkpoint and hide after source clip duration; count-zero root waits for completion; undo restores the same Spine instances.");
                        FinishChainLifecycle(run.report);
                    }
                    break;
            }
        }
        catch (Exception exception)
        {
            run.report.failed.Add("WorldView chain lifecycle: " + exception.GetType().Name + ": " + exception.Message);
            FinishChainLifecycle(run.report);
        }
    }

    static RotateRingsLevel GetFixtureForRun(ChainLifecycleRun run)
    {
        return run.fixture;
    }

    static float CurrentClipDuration(Transform segment)
    {
        SkeletonAnimation animation = segment.GetComponentInChildren<SkeletonAnimation>(true);
        var track = animation != null ? animation.AnimationState.GetCurrent(0) : null;
        return track != null ? track.Animation.Duration : 0f;
    }

    static string CurrentClipName(Transform segment)
    {
        SkeletonAnimation animation = segment.GetComponentInChildren<SkeletonAnimation>(true);
        var track = animation != null ? animation.AnimationState.GetCurrent(0) : null;
        return track != null && track.Animation != null ? track.Animation.Name : string.Empty;
    }

    static string DirectionName(Transform segment)
    {
        string name = segment.name;
        if (name.StartsWith("low", StringComparison.Ordinal)) return "low";
        if (name.StartsWith("high", StringComparison.Ordinal)) return "high";
        if (name.StartsWith("shu", StringComparison.Ordinal)) return "shu";
        return "mid";
    }

    static void FinishChainLifecycle(Report report)
    {
        ChainLifecycleRun run = activeChainRun;
        EditorApplication.update -= TickChainLifecycle;
        activeChainRun = null;
        report.engineExecution = "Unity Play Mode WorldView chain lifecycle completed.";
        if (run != null && run.root != null)
        {
            if (Application.isPlaying) UnityEngine.Object.Destroy(run.root);
            else UnityEngine.Object.DestroyImmediate(run.root);
        }
        FinishReport(report);
    }

    static void CheckAuthoredPinLifecycle(RotateRingsLevelPack pack, RotateRingsTemplateCatalog catalog, Report report)
    {
        RotateRingsLevel source = pack.levels.Find(level => level.id == 10001);
        Require(source != null, "Missing original fixture 10001 for native pin lifecycle QA.");
        RotateRingsPart ownerSource = source.parts.Find(part => part.id == 1);
        RotateRingsPart receiverSource = source.parts.Find(part => part.id == 2);
        Require(ownerSource != null && receiverSource != null, "Fixture 10001 must contain source ring_1 and ring_2.");
        RotateRingsOriginalTemplate ownerTemplate = Array.Find(catalog.templates, template => template.id == ownerSource.templateId);
        Require(ownerTemplate != null, "Fixture 10001 ring_1 source template is missing.");
        var pinDefinitions = ownerTemplate.physicalShapes.Where(shape => shape.attachmentId == 0 &&
            (shape.role == "stem" || shape.role == "head_wall")).ToArray();
        Require(pinDefinitions.Length >= 3, "Source ring_1 attachment 0 must contain its native stem and head walls.");

        RotateRingsLevel fixture = JsonUtility.FromJson<RotateRingsLevel>(JsonUtility.ToJson(source));
        var root = new GameObject("Original Pin Lifecycle QA (temporary)") { hideFlags = HideFlags.HideAndDontSave };
        root.transform.position = new Vector3(10000f, 10000f, 0f);
        try
        {
            var view = root.AddComponent<RotateRingsWorldView>();
            var collision = root.AddComponent<RotateRingsCollisionWorld>();
            view.Build(fixture, catalog);
            collision.Build(fixture, catalog);
            var angles = fixture.parts.ToDictionary(part => part.id, part => part.angle);
            var removed = new HashSet<int>();
            collision.ApplyState(angles, removed);
            view.ApplyState(angles, removed, new HashSet<int>(), new List<bool>());
            Require(collision.IsBinding(1, 2), "Fixture 10001 initial source pose should have ring_1 attachment 0 bound to ring_2.");

            FieldInfo partsField = typeof(RotateRingsWorldView).GetField("parts", BindingFlags.Instance | BindingFlags.NonPublic);
            Require(partsField != null, "Could not inspect actual WorldView visuals for pin lifecycle QA.");
            object partTable = partsField.GetValue(view);
            object ownerView = ((System.Collections.IDictionary)partTable)[ownerSource.id];
            Require(ownerView != null, "WorldView did not create source ring_1.");
            Type partViewType = ownerView.GetType();
            MeshFilter meshFilter = ((Component)ownerView).GetComponent<MeshFilter>();
            Require(meshFilter != null && meshFilter.sharedMesh != null,
                "WorldView ring_1 native mesh is missing.");
            Mesh ownerMesh = meshFilter.sharedMesh;
            int meshInstanceId = ownerMesh.GetInstanceID();
            Vector3[] meshVertices = ownerMesh.vertices;
            MeshRenderer[] ownerRenderers = ((Component)ownerView).GetComponentsInChildren<MeshRenderer>(true);
            bool[] rendererEnabled = ownerRenderers.Select(renderer => renderer.enabled).ToArray();
            Require(ownerRenderers.Length > 0 && rendererEnabled.All(enabled => enabled),
                "WorldView ring_1 visual should be rendered before and after binding loss.");

            ValidateSourcePinGeometry(collision, ownerTemplate);
            Collider2D[] pinColliders = FindOwnerPinColliders(collision, ownerTemplate);
            bool[] colliderEnabled = pinColliders.Select(collider => collider.enabled).ToArray();
            Bounds[] colliderBounds = pinColliders.Select(collider => collider.bounds).ToArray();
            Require(pinColliders.Length == pinDefinitions.Length && colliderEnabled.All(enabled => enabled),
                "All source ring_1 stem/head-wall pin colliders must start enabled.");

            // Turn receiver ring_2 away without removing either piece or resolving a cascade.
            angles[receiverSource.id] = Mathf.Repeat(receiverSource.angle + 180f, 360f);
            collision.ApplyState(angles, removed);
            view.ApplyState(angles, removed, new HashSet<int>(), new List<bool>());
            Require(!collision.IsBinding(ownerSource.id, receiverSource.id),
                "Rotating ring_2 by 180 degrees should remove the authored ring_1→ring_2 binding without removal.");
            Require(OwnerVisualUnchanged(ownerRenderers, rendererEnabled, meshFilter, meshInstanceId, meshVertices),
                "Losing ring_1→ring_2 binding must not hide, replace, or reshape ring_1 visual geometry.");
            Require(PinCollidersUnchanged(pinColliders, colliderEnabled, colliderBounds),
                "Losing ring_1→ring_2 binding must not disable or move ring_1 stem/head-wall colliders.");
            report.passed.Add("Fixture 10001 native pin remains rendered with unchanged mesh and enabled source stem/head-wall colliders when ring_2 rotates +180° and the attachment binding disappears.");

            angles[receiverSource.id] = receiverSource.angle;
            collision.ApplyState(angles, removed);
            view.ApplyState(angles, removed, new HashSet<int>(), new List<bool>());
            Require(collision.IsBinding(ownerSource.id, receiverSource.id),
                "Restoring ring_2 source pose should restore the authored attachment binding.");
            Require(OwnerVisualUnchanged(ownerRenderers, rendererEnabled, meshFilter, meshInstanceId, meshVertices) &&
                    PinCollidersUnchanged(pinColliders, colliderEnabled, colliderBounds),
                "Restoring a binding must not hide, replace, reshape, disable, or move ring_1 pin geometry.");
            report.passed.Add("Fixture 10001 restoring ring_2 pose restores the authored binding while keeping the same ring_1 mesh/renderers and pin colliders.");

            // Removal owns visual/collider hiding; undo restores both from the unchanged fixture.
            removed.Add(ownerSource.id);
            collision.ApplyState(angles, removed);
            view.ApplyState(angles, removed, new HashSet<int>(), new List<bool>());
            Require(ownerRenderers.All(renderer => !renderer.enabled) && pinColliders.All(collider => !collider.enabled),
                "Removing source ring_1 must hide its visual renderers and disable its own physical pin colliders.");
            report.passed.Add("Fixture 10001 removing ring_1 hides its own visuals and disables its physical pin colliders.");

            removed.Clear();
            collision.ApplyState(angles, removed);
            view.ApplyState(angles, removed, new HashSet<int>(), new List<bool>());
            Require(OwnerVisualUnchanged(ownerRenderers, rendererEnabled, meshFilter, meshInstanceId, meshVertices) &&
                    PinCollidersUnchanged(pinColliders, colliderEnabled, colliderBounds),
                "Undo restore must re-enable the same ring_1 visual mesh and native pin collider geometry.");
            report.passed.Add("Fixture 10001 undo restore re-enables the same ring_1 renderers and pin colliders with authored geometry intact.");
        }
        finally
        {
            if (root != null) UnityEngine.Object.DestroyImmediate(root);
        }
    }

    static void ValidateSourcePinGeometry(RotateRingsCollisionWorld collision, RotateRingsOriginalTemplate template)
    {
        Transform ownerRoot = collision.OwnerTransform(1);
        Require(ownerRoot != null, "CollisionWorld has no source ring_1 transform.");
        for (int index = 0; index < template.physicalShapes.Length; index++)
        {
            RotateRingsPhysicalShape definition = template.physicalShapes[index];
            if (definition.attachmentId != 0 || (definition.role != "stem" && definition.role != "head_wall")) continue;
            Transform child = ownerRoot.Find(definition.role + " " + index);
            Require(child != null, "Native source pin collider node is missing for shape " + index + ".");
            Collider2D collider = child.GetComponent<Collider2D>();
            Require(collider != null && collider.enabled == definition.sourceEnabled && collider.isTrigger == definition.trigger,
                "Native pin collider enabled/trigger state differs from source shape " + index + ".");
            Require(collider.GetType().Name == definition.sourceColliderType,
                "Native pin collider type differs from source shape " + index + ".");
            if (collider is PolygonCollider2D polygon)
            {
                RotateRingsShapePath[] paths = definition.paths != null && definition.paths.Length > 0
                    ? definition.paths : new[] { new RotateRingsShapePath { points = definition.points } };
                Require(polygon.pathCount == paths.Length, "Native pin polygon path count differs from source shape " + index + ".");
                for (int path = 0; path < paths.Length; path++)
                {
                    Vector2[] actual = polygon.GetPath(path);
                    Require(actual.Length == paths[path].points.Length, "Native pin polygon vertex count differs from source shape " + index + ".");
                    for (int point = 0; point < actual.Length; point++)
                        Require(Vector2.Distance(actual[point], paths[path].points[point]) < .00001f,
                            "Native pin polygon outline differs from source shape " + index + " at vertex " + point + ".");
                }
            }
            else if (collider is BoxCollider2D box)
            {
                Vector2[] points = definition.points;
                Require(points != null && points.Length >= 4, "Source pin box shape " + index + " has no exact outline points.");
                Vector2 center = Vector2.zero;
                for (int point = 0; point < points.Length; point++) center += points[point];
                center /= points.Length;
                Vector2 size = new Vector2(Vector2.Distance(points[0], points[1]), Vector2.Distance(points[1], points[2]));
                float angle = Mathf.Atan2(points[1].y - points[0].y, points[1].x - points[0].x) * Mathf.Rad2Deg;
                Require(Vector2.Distance(child.localPosition, center) < .00001f &&
                        Vector2.Distance(box.size, size) < .00001f &&
                        Mathf.Abs(Mathf.DeltaAngle(child.localEulerAngles.z, angle)) < .0001f,
                    "Native pin box center/size/rotation differs from exact source points for shape " + index + ".");
            }
        }
    }

    static Collider2D[] FindOwnerPinColliders(RotateRingsCollisionWorld collision, RotateRingsOriginalTemplate template)
    {
        var colliders = new List<Collider2D>();
        Transform ownerRoot = collision.OwnerTransform(1);
        for (int index = 0; index < template.physicalShapes.Length; index++)
        {
            RotateRingsPhysicalShape definition = template.physicalShapes[index];
            if (definition.attachmentId != 0 || (definition.role != "stem" && definition.role != "head_wall")) continue;
            Transform child = ownerRoot.Find(definition.role + " " + index);
            if (child != null)
            {
                Collider2D collider = child.GetComponent<Collider2D>();
                if (collider != null) colliders.Add(collider);
            }
        }
        return colliders.ToArray();
    }

    static bool OwnerVisualUnchanged(MeshRenderer[] renderers, bool[] enabled,
        MeshFilter meshFilter, int meshInstanceId, Vector3[] vertices)
    {
        if (meshFilter == null || meshFilter.sharedMesh == null || meshFilter.sharedMesh.GetInstanceID() != meshInstanceId) return false;
        Vector3[] current = meshFilter.sharedMesh.vertices;
        if (current.Length != vertices.Length || renderers.Length != enabled.Length) return false;
        for (int i = 0; i < current.Length; i++) if ((current[i] - vertices[i]).sqrMagnitude > 1e-10f) return false;
        for (int i = 0; i < renderers.Length; i++) if (renderers[i] == null || renderers[i].enabled != enabled[i]) return false;
        return true;
    }

    static bool PinCollidersUnchanged(Collider2D[] colliders, bool[] enabled, Bounds[] bounds)
    {
        if (colliders.Length != enabled.Length || colliders.Length != bounds.Length) return false;
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] == null || colliders[i].enabled != enabled[i]) return false;
            Bounds current = colliders[i].bounds;
            if ((current.center - bounds[i].center).sqrMagnitude > 1e-8f ||
                (current.size - bounds[i].size).sqrMagnitude > 1e-8f) return false;
        }
        return true;
    }

    static void CheckFixtures(RotateRingsLevelPack pack, Report report)
    {
        foreach (int id in FixtureIds)
        {
            RotateRingsLevel level = pack.levels.Find(item => item.id == id);
            Require(level != null, "Missing exact source fixture " + id + ".");
            int movableCount = 0;
            foreach (RotateRingsPart part in level.parts)
            {
                if (!RotateRingsRules.IsMovable(part)) continue;
                movableCount++;
                Require(part.inputShapes != null && part.inputShapes.Length == 1,
                    "Fixture " + id + " part " + part.id + " must have its one source DragArea.");
                RotateRingsPhysicalShape shape = part.inputShapes[0];
                Require(shape.sourceColliderType == "PolygonCollider2D" && shape.sourceLayer == 6,
                    "Fixture " + id + " part " + part.id + " is not the source PolygonCollider2D on layer 6.");
                Require(shape.sourceEnabled && shape.paths != null && shape.paths.Length > 0,
                    "Fixture " + id + " part " + part.id + " has a disabled or empty source DragArea.");
                Require(Mathf.Abs(shape.localZ - .1f) < .00001f,
                    "Fixture " + id + " part " + part.id + " does not start at source DragArea Z 0.1.");
                foreach (RotateRingsShapePath path in shape.paths)
                    Require(path.points != null && path.points.Length >= 3,
                        "Fixture " + id + " part " + part.id + " has a malformed source collider path.");
                report.movableShapesChecked++;
            }
            Require(movableCount > 0, "Fixture " + id + " has no movable parts to validate.");
            report.fixturesChecked++;
        }
        Require(report.fixturesChecked == 8, "The bounded input audit must cover exactly eight source fixtures.");
        Require(report.nativeRayMask == 192, "Native pointer ray mask must remain layers 6 and 7 (192).");
        report.passed.Add("8 exact source fixtures preserve one polygon DragArea per ring/slider, source layer 6, source Z 0.1, and all serialized paths.");
        report.passed.Add("Input ray mask is exactly 192 (layers 6 and 7).");
    }

    static void CheckGuideForwarding(RotateRingsLevelPack pack, Report report)
    {
        foreach (int id in FixedPullFixtureIds)
        {
            RotateRingsLevel level = pack.levels.Find(item => item.id == id);
            Require(level != null && level.guideLinks != null && level.guideLinks.Length > 0,
                "Missing fixed-guide/pull fixture " + id + ".");
            foreach (RotateRingsGuideLink guide in level.guideLinks)
            {
                RotateRingsPart guidePart = level.parts.Find(part => part.id == guide.guideOwnerId);
                RotateRingsPart pullPart = level.parts.Find(part => part.id == guide.lockedOwnerId);
                Require(guidePart != null && guidePart.kind == "fixed" && pullPart != null && RotateRingsRules.IsSlider(pullPart),
                    "Fixture " + id + " no longer represents source fixed-guide to Pull forwarding.");
            }
        }

        RotateRingsLevel butterflyLevel = pack.levels.Find(item => item.id == 10190);
        Require(butterflyLevel != null && butterflyLevel.guideLinks != null && butterflyLevel.guideLinks.Length > 0,
            "Missing butterfly guide fixture 10190.");
        foreach (RotateRingsGuideLink guide in butterflyLevel.guideLinks)
        {
            RotateRingsPart guidePart = butterflyLevel.parts.Find(part => part.id == guide.guideOwnerId);
            RotateRingsPart hostPart = butterflyLevel.parts.Find(part => part.sourceId == guide.hostPartId);
            RotateRingsPart lockedPart = butterflyLevel.parts.Find(part => part.id == guide.lockedOwnerId);
            Require(guidePart != null && guidePart.kind == "butterfly" && hostPart != null && hostPart.kind == "butterfly",
                "Butterfly guide must resolve to its FixedButterflyObject host.");
            Require(lockedPart != null && lockedPart.kind == "ring",
                "Butterfly guide fixture is expected to lock rings, not Pull objects.");
            Require(!RotateRingsRules.IsSlider(lockedPart),
                "Butterfly host hit must not be forwarded as a Pull drag.");
        }
        report.passed.Add("Fixed guides forward only to their linked Pulls; butterfly hosts resolve as FixedButterflyObject and do not forward their ring links as Pull drags.");
    }

    static void CheckNativeRayAndLockDepth(RotateRingsLevelPack pack, RotateRingsTemplateCatalog catalog,
        Report report, out GameObject temporary)
    {
        temporary = new GameObject("Original Input QA (temporary)") { hideFlags = HideFlags.HideAndDontSave };
        temporary.transform.position = new Vector3(10000f, 10000f, 0f);
        RotateRingsLevel fixture = pack.levels.Find(item => item.id == 10001);
        RotateRingsPart templatePart = fixture.parts.Find(part => part.kind == "ring");
        RotateRingsPart free = ClonePart(templatePart);
        RotateRingsPart chainLocked = ClonePart(templatePart);
        free.id = 910001; free.sourceId = "qa_free"; free.x = 0f; free.y = 0f; free.chainLockCount = 0; free.lockKey = 0;
        chainLocked.id = 910002; chainLocked.sourceId = "qa_chain_locked"; chainLocked.x = 0f; chainLocked.y = 0f; chainLocked.chainLockCount = 1; chainLocked.lockKey = 0;
        var level = new RotateRingsLevel { id = -9100, parts = new List<RotateRingsPart> { free, chainLocked }, links = new List<RotateRingsLink>() };
        var collision = temporary.AddComponent<RotateRingsCollisionWorld>();
        var input = temporary.AddComponent<RotateRingsInputWorld>();
        input.Build(level, catalog, collision, null);
        var angles = new Dictionary<int, float> { { free.id, free.angle }, { chainLocked.id, chainLocked.angle } };
        var slides = new Dictionary<int, float>();
        var chainCounts = new Dictionary<int, int> { { free.id, 0 }, { chainLocked.id, 1 } };
        var removed = new HashSet<int>();
        var keys = new HashSet<int>();
        input.ApplyState(angles, slides, removed, chainCounts, keys);
        Physics2D.SyncTransforms();

        PolygonCollider2D freeArea = temporary.GetComponentsInChildren<PolygonCollider2D>(true)
            .FirstOrDefault(collider => collider.gameObject.name.EndsWith(free.id.ToString()));
        PolygonCollider2D lockedArea = temporary.GetComponentsInChildren<PolygonCollider2D>(true)
            .FirstOrDefault(collider => collider.gameObject.name.EndsWith(chainLocked.id.ToString()));
        Require(freeArea != null && lockedArea != null, "Synthetic ray fixture did not create both DragAreas.");
        Require(Mathf.Abs(freeArea.transform.localPosition.z - .1f) < .00001f &&
                Mathf.Abs(lockedArea.transform.localPosition.z - .2f) < .00001f,
            "Free/chain-locked ring DragArea depth must be 0.1/0.2.");
        Vector2 point = FindInteriorPoint(freeArea);
        GameObject cameraObject = new GameObject("Original Input QA Camera") { hideFlags = HideFlags.HideAndDontSave };
        cameraObject.transform.SetParent(temporary.transform, false);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.transform.position = new Vector3(point.x, point.y, -10f);
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.pixelRect = new Rect(0f, 0f, 800f, 800f);
        Vector2 screen = camera.WorldToScreenPoint(new Vector3(point.x, point.y, 0f));
        Require(input.TryHit(camera, screen, removed, out int hitOwner) && hitOwner == free.id,
            "Overlapping unlocked/chain-locked DragAreas must pick the nearer free layer-6 collider.");

        removed.Add(free.id);
        input.ApplyState(angles, slides, removed, chainCounts, keys);
        Require(input.TryHit(camera, screen, removed, out hitOwner) && hitOwner == chainLocked.id,
            "Removed DragArea must disable so the remaining locked ring receives the hit.");
        removed.Add(chainLocked.id);
        input.ApplyState(angles, slides, removed, chainCounts, keys);
        Require(!input.TryHit(camera, screen, removed, out hitOwner),
            "All removed DragAreas must stop receiving ray hits.");

        removed.Clear(); chainCounts[chainLocked.id] = 0; chainLocked.lockKey = 123;
        input.ApplyState(angles, slides, removed, chainCounts, keys);
        Require(Mathf.Abs(lockedArea.transform.localPosition.z - .2f) < .00001f,
            "An unresolved key lock must retain source locked DragArea depth.");
        keys.Add(123);
        input.ApplyState(angles, slides, removed, chainCounts, keys);
        Require(Mathf.Abs(lockedArea.transform.localPosition.z - .1f) < .00001f,
            "Resolved key lock must restore normal DragArea depth.");

        var blocker = new GameObject("Original Input QA layer 7 blocker") { hideFlags = HideFlags.HideAndDontSave };
        blocker.transform.SetParent(temporary.transform, true);
        blocker.layer = 7;
        blocker.transform.position = new Vector3(point.x, point.y, .05f);
        var box = blocker.AddComponent<BoxCollider2D>(); box.size = Vector2.one * .2f;
        Physics2D.SyncTransforms();
        Require(!input.TryHit(camera, screen, removed, out hitOwner),
            "A nearer source layer-7 hit must win the combined 192 ray and prevent selecting behind it.");
        report.passed.Add("Native Physics2D ray selection honors free/locked depth, layer-7 nearest-hit ordering, key-lock depth, and removed-area disabling.");
    }

    static void CheckMotionCleanupEvidence(string researchRoot, Report report)
    {
        string modulesPath = Path.Combine(Path.GetDirectoryName(researchRoot), "eff_cycle_boom_modules.json");
        var modules = JsonUtility.FromJson<MotionModules>(File.ReadAllText(modulesPath));
        Require(modules != null && modules.objects != null, "Could not load recovered eff_cycle_boom particle modules.");
        MotionSystem root = modules.objects.Where(item => item.type == "ParticleSystem" && item.tree != null)
            .Select(item => item.tree).FirstOrDefault(system => system.InitialModule != null &&
                system.InitialModule.startLifetime != null && Mathf.Abs(system.InitialModule.startLifetime.scalar - 5f) < .001f);
        MotionSystem visible = modules.objects.Where(item => item.type == "ParticleSystem" && item.tree != null)
            .Select(item => item.tree).FirstOrDefault(system => system.InitialModule != null &&
                system.InitialModule.startLifetime != null && Mathf.Abs(system.InitialModule.startLifetime.scalar - .75f) < .001f);
        Require(root != null && visible != null, "Source particle lifetimes must include root 5s and visible-fragment 0.75s systems.");
        Require(root.InitialModule.startDelay != null && visible.InitialModule.startDelay != null &&
                Mathf.Abs(root.InitialModule.startDelay.scalar) < .001f && Mathf.Abs(visible.InitialModule.startDelay.scalar) < .001f,
            "Source eff_cycle_boom particle start delays should be zero.");
        Require(root.EmissionModule != null && !root.EmissionModule.enabled,
            "The 5s root particle system is a non-emitting cleanup tail, not visible fragments.");
        float sourceCleanup = Mathf.Max(
            root.InitialModule.startDelay.scalar + root.InitialModule.startLifetime.scalar,
            visible.InitialModule.startDelay.scalar + visible.InitialModule.startLifetime.scalar);
        Require(Mathf.Abs(sourceCleanup - 5f) < .001f,
            "Source cleanup is max(startDelay + startLifetime), five seconds; main.duration is not part of this formula.");
        string motionSourcePath = Path.GetFullPath(Path.Combine(Application.dataPath,
            "RotateRings/Scripts/RotateRingsOriginalMotion.cs"));
        string motionSource = File.ReadAllText(motionSourcePath);
        Require(motionSource.Contains("SourcePlaybackDuration(root)") &&
                motionSource.Contains("main.startDelay.constantMax+main.startLifetime.constantMax"),
            "PlayShatter must use source startDelay+startLifetime cleanup timing.");
        report.passed.Add("eff_cycle_boom cleanup timing matches source max(startDelay + startLifetime)=5s; old duration+life=10s calculation is excluded.");
    }

    static void CheckChainSlots(Report report, ref GameObject temporary)
    {
        if (temporary != null) UnityEngine.Object.DestroyImmediate(temporary);
        temporary = new GameObject("Original Chain QA (temporary)") { hideFlags = HideFlags.HideAndDontSave };
        GameObject root = OriginalChainVisualAssets.CreateChainVisual(temporary.transform,
            new OriginalChainVisualAssets.Geometry(48f, 90f, 0f), 2);
        Transform slot0 = OriginalChainVisualAssets.GetChainSegment(root, 0);
        Transform slot1 = OriginalChainVisualAssets.GetChainSegment(root, 1);
        Transform slot2 = OriginalChainVisualAssets.GetChainSegment(root, 2);
        Require(slot0 != null && slot1 != null && slot2 != null, "Native source chain must create all three stable authored slots.");
        Require(!slot0.gameObject.activeSelf && slot1.gameObject.activeSelf && slot2.gameObject.activeSelf,
            "A chain first created at count 2 uses source authored directions second and third.");
        int id0 = slot0.GetInstanceID(), id1 = slot1.GetInstanceID(), id2 = slot2.GetInstanceID();

        OriginalChainVisualAssets.UpdateChainCount(root, 3);
        Require(slot0.gameObject.activeSelf && slot1.gameObject.activeSelf && slot2.gameObject.activeSelf,
            "A 2-to-3 restore must activate all stable authored slots.");
        Require(OriginalChainVisualAssets.GetChainSegment(root, 0).GetInstanceID() == id0 &&
                OriginalChainVisualAssets.GetChainSegment(root, 1).GetInstanceID() == id1 &&
                OriginalChainVisualAssets.GetChainSegment(root, 2).GetInstanceID() == id2,
            "Count changes must retain authored Spine segment identities.");

        // The source 3-to-2 decrement breaks instance[newCount] and keeps indices 0 and 1.
        slot2.gameObject.SetActive(false);
        Require(slot0.gameObject.activeSelf && slot1.gameObject.activeSelf && !slot2.gameObject.activeSelf,
            "A 3-to-2 source break leaves authored slots 0 and 1 alive.");
        // An initially-created 2-lock ring instead contains [second, third], so 2-to-1 breaks its last slot.
        OriginalChainVisualAssets.UpdateChainCount(root, 2);
        Require(!slot0.gameObject.activeSelf && slot1.gameObject.activeSelf && slot2.gameObject.activeSelf,
            "A restored count-2 set must use authored slots 1 and 2.");
        slot2.gameObject.SetActive(false);
        Require(slot1.gameObject.activeSelf && !slot2.gameObject.activeSelf,
            "A 2-to-1 source break retains the authored second direction.");
        report.passed.Add("Native Spine chain helper preserves stable slots and distinguishes initial count-2 layout from 3-to-2 and 2-to-1 source break indexing.");
    }

    static RotateRingsPart ClonePart(RotateRingsPart source)
    {
        return JsonUtility.FromJson<RotateRingsPart>(JsonUtility.ToJson(source));
    }

    static Vector2 FindInteriorPoint(PolygonCollider2D collider)
    {
        Bounds bounds = collider.bounds;
        for (float y = bounds.min.y; y <= bounds.max.y; y += .08f)
            for (float x = bounds.min.x; x <= bounds.max.x; x += .08f)
                if (collider.OverlapPoint(new Vector2(x, y))) return new Vector2(x, y);
        throw new InvalidOperationException("Could not find an interior point in the source DragArea polygon.");
    }

    static void Require(bool condition, string failure)
    {
        if (!condition) throw new InvalidOperationException(failure);
    }
}
#endif

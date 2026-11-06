#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

/// <summary>Focused native-shape regression checks for ring-owned CatchGuide bindings.</summary>
public static class SourceGuideBindingQA
{
    [Serializable] sealed class Check { public string name; public bool passed; public string observed; }
    [Serializable] sealed class Report
    {
        public string status;
        public string generatedUtc;
        public string rulesHash;
        public string sourcePackSha256;
        public string templatesSha256;
        public string queryCatalogSha256;
        public string collisionWorldSha256;
        public string scope = "Production Physics2D guide-query regression on source fixtures 10187 and 10186; no gameplay or level data is modified.";
        public List<Check> checks = new List<Check>();
    }

    static readonly ContactFilter2D NativeGuideFilter = CreateNativeGuideFilter();

    static ContactFilter2D CreateNativeGuideFilter()
    {
        var filter = new ContactFilter2D();
        filter.NoFilter();
        filter.SetLayerMask(128);
        filter.useTriggers = true;
        return filter;
    }

    public static string Run(string destination)
    {
        string researchRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Research/physics_v3"));
        string packPath = Path.Combine(researchRoot, "full500", "source_parity_pack.json");
        string templatesPath = Path.Combine(researchRoot, "full500", "source_templates.json");
        string queryPath = Path.Combine(Application.dataPath, "RotateRings/Resources/SourceOverlapQueries.json");
        string collisionPath = Path.Combine(Application.dataPath, "RotateRings/Scripts/RotateRingsCollisionWorld.cs");
        var pack = JsonUtility.FromJson<RotateRingsLevelPack>(File.ReadAllText(packPath));
        var templates = JsonUtility.FromJson<RotateRingsTemplateCatalog>(File.ReadAllText(templatesPath));
        var report = new Report
        {
            generatedUtc = DateTime.UtcNow.ToString("o"),
            rulesHash = HashRuleBundle(),
            sourcePackSha256 = Hash(packPath),
            templatesSha256 = Hash(templatesPath),
            queryCatalogSha256 = Hash(queryPath),
            collisionWorldSha256 = Hash(collisionPath)
        };

        CheckNativeRingBodyContact(pack, templates, report);
        CheckExtraShapeContact(pack, templates, report, "HardBlockPin", new[] { 1 });
        CheckExtraShapeContact(pack, templates, report, "RingRelativeWall", new[] { 3, 4 });
        CheckButterflyOtherSlotDoesNotReplaceSelectedSlot(pack, templates, report);

        report.status = report.checks.All(check => check.passed) ? "PASS" : "FAIL";
        string json = JsonUtility.ToJson(report, true);
        File.WriteAllText(destination, json);
        return json;
    }

    static void CheckNativeRingBodyContact(RotateRingsLevelPack pack, RotateRingsTemplateCatalog templates, Report report)
    {
        var level = Clone(pack.levels.First(item => item.id == 10187));
        var link = level.guideLinks.First(item => item.guidePartId == "guide_8" && item.lockedOwnerId == 4);
        GameObject root = Build(level, templates, out var world);
        try
        {
            var locked = world.GetOwnerColliders(link.lockedOwnerId);
            var guide = world.GetOwnerColliders(link.guideOwnerId)[link.guideShapeIndices[0]];
            bool querySeesGuide = NativeQuerySees(locked[0], guide);
            bool active = IsGuideActive(world, link);
            report.checks.Add(new Check
            {
                name = "native RingBody query contact keeps the exact ring-to-butterfly-slot binding",
                passed = querySeesGuide && active,
                observed = "source=10187; guide=" + link.guidePartId + "[" + link.guideShapeIndices[0] + "]; locked=4[0:RingBody]; queryHit=" + querySeesGuide + "; active=" + active
            });
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    static void CheckExtraShapeContact(RotateRingsLevelPack pack, RotateRingsTemplateCatalog templates, Report report,
        string role, int[] candidateIndices)
    {
        var level = Clone(pack.levels.First(item => item.id == 10187));
        var link = level.guideLinks.First(item => item.guidePartId == "guide_8" && item.lockedOwnerId == 4);
        var lockedPart = level.parts.First(part => part.id == link.lockedOwnerId);
        var lockedTemplate = templates.templates.First(template => template.id == lockedPart.templateId);
        GameObject root = Build(level, templates, out var world);
        try
        {
            var ownerShapes = world.GetOwnerColliders(link.lockedOwnerId);
            var queryShapes = new[] { ownerShapes[0], ownerShapes[2] };
            var guideShapes = world.GetOwnerColliders(link.guideOwnerId);
            var selectedGuide = guideShapes[link.guideShapeIndices[0]];
            var guideRoot = world.OwnerTransform(link.guideOwnerId);
            Vector3 initialPosition = guideRoot.localPosition;
            int foundIndex = -1;
            string pose = "none";
            int extraOverlapSamples = 0, queryCoveredSamples = 0;
            foreach (int index in candidateIndices)
            {
                if (index >= ownerShapes.Count || index >= lockedTemplate.physicalShapes.Length ||
                    lockedTemplate.physicalShapes[index].sourceRole != role) continue;
                if (FindExtraOnlyPose(world, guideRoot, initialPosition, selectedGuide, ownerShapes[index], queryShapes,
                    out pose, out int overlaps, out int queryCovered))
                {
                    foundIndex = index;
                    extraOverlapSamples = overlaps;
                    queryCoveredSamples = queryCovered;
                    break;
                }
                extraOverlapSamples += overlaps;
                queryCoveredSamples += queryCovered;
            }
            bool found = foundIndex >= 0;
            bool querySeesSelectedGuide = queryShapes.Any(query => NativeQuerySees(query, selectedGuide));
            bool active = IsGuideActive(world, link);
            bool pinOverlapIsQueryCovered = role == "HardBlockPin" && !found && extraOverlapSamples > 0 &&
                queryCoveredSamples == extraOverlapSamples && querySeesSelectedGuide && active;
            bool passed = pinOverlapIsQueryCovered || (found && !querySeesSelectedGuide && !active);
            report.checks.Add(new Check
            {
                name = role == "HardBlockPin"
                    ? "HardBlockPin overlaps are query-covered in the searched source pose neighborhood"
                    : role + "-only overlap does not keep the ring-to-guide binding",
                passed = passed,
                observed = "source=10187; extra=" + role + "[" + (foundIndex >= 0 ? foundIndex.ToString() : "none") + "]; selectedGuide=" + link.guidePartId + "[" + link.guideShapeIndices[0] + "]; pose=" + pose + "; guideExtraOverlapSamples=" + extraOverlapSamples + "; queryCoveredSamples=" + queryCoveredSamples + "; queryBodyOverlapAtRestoredPose=" + querySeesSelectedGuide + "; activeAtRestoredPose=" + active
            });
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    static void CheckButterflyOtherSlotDoesNotReplaceSelectedSlot(RotateRingsLevelPack pack,
        RotateRingsTemplateCatalog templates, Report report)
    {
        var level = Clone(pack.levels.First(item => item.id == 10186));
        var link = level.guideLinks.First(item => item.guidePartId == "guide_8" && item.lockedOwnerId == 7);
        GameObject root = Build(level, templates, out var world);
        try
        {
            var locked = world.GetOwnerColliders(link.lockedOwnerId);
            var queryShapes = new[] { locked[0], locked[2] };
            var slots = world.GetOwnerColliders(link.guideOwnerId);
            int selectedSlot = link.guideShapeIndices[0];
            int otherSlot = selectedSlot == 1 ? 2 : 1;
            var guideRoot = world.OwnerTransform(link.guideOwnerId);
            Vector3 initialPosition = guideRoot.localPosition;
            bool found = FindOtherSlotOnlyPose(world, guideRoot, initialPosition, slots[selectedSlot], slots[otherSlot], queryShapes, out string pose);
            bool otherSlotQueryHit = queryShapes.Any(query => NativeQuerySees(query, slots[otherSlot]));
            bool selectedSlotQueryHit = queryShapes.Any(query => NativeQuerySees(query, slots[selectedSlot]));
            bool active = IsGuideActive(world, link);
            report.checks.Add(new Check
            {
                name = "butterfly other CatchGuideBody slot cannot substitute for the linked slot",
                passed = found && otherSlotQueryHit && !selectedSlotQueryHit && !active,
                observed = "source=10186; guide=" + link.guidePartId + " linkedSlot=" + selectedSlot + " otherSlot=" + otherSlot + "; pose=" + pose + "; otherSlotQueryHit=" + otherSlotQueryHit + "; linkedSlotQueryHit=" + selectedSlotQueryHit + "; active=" + active
            });
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    static bool FindExtraOnlyPose(RotateRingsCollisionWorld world, Transform guideRoot, Vector3 initialPosition,
        Collider2D guide, Collider2D extra, Collider2D[] queryShapes, out string pose,
        out int extraOverlapSamples, out int queryCoveredSamples)
    {
        extraOverlapSamples = 0;
        queryCoveredSamples = 0;
        Vector2 align = extra.bounds.center - guide.bounds.center;
        for (int ring = 0; ring < 3; ring++)
        {
            float range = .5f + ring * .75f;
            const float step = .1f;
            int count = Mathf.CeilToInt(range / step);
            for (int xi = -count; xi <= count; xi++)
            for (int yi = -count; yi <= count; yi++)
            {
                float dx = xi * step, dy = yi * step;
                guideRoot.localPosition = initialPosition + ToLocalOffset(guideRoot, align + new Vector2(dx, dy));
                Physics2D.SyncTransforms();
                var distance = guide.Distance(extra);
                if (!distance.isValid || !distance.isOverlapped || distance.distance >= -.001f) continue;
                extraOverlapSamples++;
                if (queryShapes.Any(query => NativeQuerySees(query, guide)))
                {
                    queryCoveredSamples++;
                    continue;
                }
                world.InvalidateExternalGeometry();
                pose = "dx=" + (align.x + dx).ToString("F3", System.Globalization.CultureInfo.InvariantCulture) + ",dy=" + (align.y + dy).ToString("F3", System.Globalization.CultureInfo.InvariantCulture);
                return true;
            }
        }
        guideRoot.localPosition = initialPosition;
        Physics2D.SyncTransforms();
        world.InvalidateExternalGeometry();
        pose = "no-query-excluded-contact-pose-found";
        return false;
    }

    static bool FindOtherSlotOnlyPose(RotateRingsCollisionWorld world, Transform guideRoot, Vector3 initialPosition,
        Collider2D selected, Collider2D other, Collider2D[] queryShapes, out string pose)
    {
        foreach (var query in queryShapes)
        {
            Vector2 align = query.bounds.center - other.bounds.center;
            for (int ring = 0; ring < 4; ring++)
            {
                float range = .5f + ring * .75f;
                const float step = .1f;
                int count = Mathf.CeilToInt(range / step);
                for (int xi = -count; xi <= count; xi++)
                for (int yi = -count; yi <= count; yi++)
                {
                    float dx = xi * step, dy = yi * step;
                    guideRoot.localPosition = initialPosition + ToLocalOffset(guideRoot, align + new Vector2(dx, dy));
                    Physics2D.SyncTransforms();
                    if (!NativeQuerySees(query, other) || queryShapes.Any(candidate => NativeQuerySees(candidate, selected))) continue;
                    world.InvalidateExternalGeometry();
                    pose = "queryShape=" + Array.IndexOf(queryShapes, query) + ";dx=" + (align.x + dx).ToString("F3", System.Globalization.CultureInfo.InvariantCulture) + ",dy=" + (align.y + dy).ToString("F3", System.Globalization.CultureInfo.InvariantCulture);
                    return true;
                }
            }
        }
        guideRoot.localPosition = initialPosition;
        Physics2D.SyncTransforms();
        world.InvalidateExternalGeometry();
        pose = "no-other-slot-only-pose-found";
        return false;
    }

    static bool NativeQuerySees(Collider2D query, Collider2D expected)
    {
        var results = new Collider2D[128];
        int count = query.Overlap(NativeGuideFilter, results);
        for (int i = 0; i < count; i++) if (results[i] == expected) return true;
        return false;
    }

    static Vector3 ToLocalOffset(Transform transform, Vector2 worldOffset)
    {
        return transform.parent == null ? (Vector3)worldOffset : transform.parent.InverseTransformVector(worldOffset);
    }

    static bool IsGuideActive(RotateRingsCollisionWorld world, RotateRingsGuideLink link)
    {
        var state = world.CaptureBindingState() as RotateRingsCollisionWorld.BindingStateSnapshot;
        string key = link.guideOwnerId + ":" + link.guidePartId + ":" + link.lockedOwnerId;
        return state != null && state.activeGuideRelationKeys != null && state.activeGuideRelationKeys.Contains(key);
    }

    static GameObject Build(RotateRingsLevel level, RotateRingsTemplateCatalog templates, out RotateRingsCollisionWorld world)
    {
        var root = new GameObject("Source Guide Binding QA " + level.id) { hideFlags = HideFlags.HideAndDontSave };
        world = root.AddComponent<RotateRingsCollisionWorld>();
        world.Build(level, templates);
        return root;
    }

    static RotateRingsLevel Clone(RotateRingsLevel level) => JsonUtility.FromJson<RotateRingsLevel>(JsonUtility.ToJson(level));
    static string HashRuleBundle() => typeof(SourceSolverV3).GetMethod("HashRuleBundle", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).Invoke(null, null).ToString();
    static string Hash(string path) { using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant(); }
}
#endif

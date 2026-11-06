#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;

/// <summary>Native Pull query, registered-head-exit, and directional receiver exception diagnostics.</summary>
public static class SourcePullBindingQA
{
    [Serializable] sealed class QueryCatalog { public QueryTemplate[] templates; }
    [Serializable] sealed class QueryTemplate { public string templateId; public Query[] queries; }
    [Serializable] sealed class Query
    {
        public int shapeIndex;
        public string queryOwner, sourceRole;
        public Filter contactFilter;
    }
    [Serializable] sealed class Filter
    {
        public int useTriggers, useLayerMask;
        public Mask layerMask;
        public ContactFilter2D Native()
        {
            var result = new ContactFilter2D(); result.NoFilter();
            result.useTriggers = useTriggers != 0;
            result.SetLayerMask(unchecked((int)(layerMask == null ? 0L : layerMask.m_Bits)));
            result.useLayerMask = useLayerMask != 0;
            return result;
        }
    }
    [Serializable] sealed class Mask { public long m_Bits; }
    [Serializable] sealed class Report
    {
        public string status, generatedUtc, sourcePackSha256, templatesSha256, queryCatalogSha256;
        public string collisionWorldSha256, qaSha256;
        public int sourceId, ringOwner, pullOwner, relationIndex;
        public string ringQueryOwner, pullQueryOwner;
        public int ringQueryShape, pullQueryShape, ringMaskBits, pullMaskBits;
        public bool ringQueryIncludesTriggers, pullQueryIncludesTriggers;
        public bool initialRingQuerySeesPull, initialPullQuerySeesRing, initialRegisteredHeadContact;
        public bool registeredHeadExitFound, otherRingColliderStillVisibleToPullQuery, passiveRelationPrunedAtHeadExit;
        public float registeredHeadExitSlide, registeredHeadExitDx, registeredHeadExitDy;
        public int registeredHeadExitShapeIndex;
        public int headWallSourceId, headWallReceiverId;
        public bool wallAllowedAt0, wallAllowedAt5, wallRejectedAt5Point01;
        public bool wallExceptionRejectedWhenSourceMoves, stemNeverGetsWallException;
        public List<string> failures = new List<string>();
        public string scope = "Native layer-mask query and authored contact diagnostics. The head-wall predicate is inspected against live native shapes; this does not certify a complete legal drag path or level solvability.";
    }

    public static string Run(string destination)
    {
        string project = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string root = Path.GetDirectoryName(project);
        string data = Path.Combine(root, "Research/physics_v3/full500");
        string packPath = Path.Combine(data, "source_parity_pack.json");
        string templatesPath = Path.Combine(data, "source_templates.json");
        string queryPath = Path.Combine(Application.dataPath, "RotateRings/Resources/SourceOverlapQueries.json");
        string collisionPath = Path.Combine(Application.dataPath, "RotateRings/Scripts/RotateRingsCollisionWorld.cs");
        string qaPath = Path.Combine(Application.dataPath, "RotateRings/Editor/SourcePullBindingQA.cs");
        var report = new Report { generatedUtc = DateTime.UtcNow.ToString("o"), sourceId = 10154,
            ringOwner = 8, pullOwner = 7, headWallSourceId = 1, headWallReceiverId = 2 };
        GameObject host = null;
        try
        {
            var pack = JsonUtility.FromJson<RotateRingsLevelPack>(File.ReadAllText(packPath));
            var catalog = JsonUtility.FromJson<RotateRingsTemplateCatalog>(File.ReadAllText(templatesPath));
            var queries = JsonUtility.FromJson<QueryCatalog>(File.ReadAllText(queryPath));
            if (pack == null || catalog == null || queries == null || queries.templates == null)
                throw new InvalidOperationException("Could not read source physics inputs.");
            report.sourcePackSha256 = Hash(packPath); report.templatesSha256 = Hash(templatesPath);
            report.queryCatalogSha256 = Hash(queryPath); report.collisionWorldSha256 = Hash(collisionPath); report.qaSha256 = Hash(qaPath);
            CheckPullMasksAndExit(pack, catalog, queries, report, out host);
            if (host != null) { UnityEngine.Object.DestroyImmediate(host); host = null; }
            CheckReceiverWallPolicy(pack, catalog, report, out host);
        }
        catch (Exception exception) { report.failures.Add(exception.GetType().Name + ": " + exception.Message); }
        finally { if (host != null) UnityEngine.Object.DestroyImmediate(host); }
        report.status = report.failures.Count == 0 ? "PASS" : "FAIL";
        Directory.CreateDirectory(Path.GetDirectoryName(destination));
        File.WriteAllText(destination, JsonUtility.ToJson(report, true));
        return destination;
    }

    static void CheckPullMasksAndExit(RotateRingsLevelPack pack, RotateRingsTemplateCatalog catalog,
        QueryCatalog queryCatalog, Report report, out GameObject host)
    {
        host = null;
        RotateRingsLevel level = pack.levels.Find(item => item.sourceId == "10154");
        Require(level != null, "Source 10154 is missing.");
        int relationIndex = level.links.FindIndex(link => link.source == 8 && link.target == 7);
        Require(relationIndex >= 0, "Source 10154 is missing ring 8 -> Pull 7.");
        report.relationIndex = relationIndex;
        RotateRingsPart ring = level.parts.Find(part => part.id == 8);
        RotateRingsPart pull = level.parts.Find(part => part.id == 7);
        Query ringQuery = FindQuery(queryCatalog, ring.templateId, "RingRelative");
        Query pullQuery = FindQuery(queryCatalog, pull.templateId, "PullObject");
        report.ringQueryOwner = ringQuery.queryOwner; report.pullQueryOwner = pullQuery.queryOwner;
        report.ringQueryShape = ringQuery.shapeIndex; report.pullQueryShape = pullQuery.shapeIndex;
        report.ringMaskBits = unchecked((int)ringQuery.contactFilter.layerMask.m_Bits);
        report.pullMaskBits = unchecked((int)pullQuery.contactFilter.layerMask.m_Bits);
        report.ringQueryIncludesTriggers = ringQuery.contactFilter.useTriggers != 0;
        report.pullQueryIncludesTriggers = pullQuery.contactFilter.useTriggers != 0;
        Require(report.ringMaskBits == (1 << 7) && report.pullMaskBits == (1 << 7), "Native RingRelative/PullObject query masks must select layer 7.");
        Require(report.ringQueryIncludesTriggers && report.pullQueryIncludesTriggers, "Native RingRelative/PullObject queries must include triggers.");

        host = new GameObject("Source Pull Binding QA") { hideFlags = HideFlags.HideAndDontSave };
        host.transform.position = new Vector3(20000f, 20000f, 0f);
        var world = host.AddComponent<RotateRingsCollisionWorld>(); world.Build(level, catalog);
        var ringColliders = world.GetOwnerColliders(8).ToArray();
        var pullColliders = world.GetOwnerColliders(7).ToArray();
        report.initialRingQuerySeesPull = Sees(ringColliders[ringQuery.shapeIndex], pullColliders, ringQuery.contactFilter.Native());
        report.initialPullQuerySeesRing = Sees(pullColliders[pullQuery.shapeIndex], ringColliders, pullQuery.contactFilter.Native());
        report.initialRegisteredHeadContact = DirectContact(world, 8, 2, 7, 0);
        Require(report.initialRingQuerySeesPull, "Native RingRelative probe does not see Pull 7 at its authored pose.");
        Require(report.initialPullQuerySeesRing, "Native PullObject probe does not see ring 8 at its authored pose.");
        Require(report.initialRegisteredHeadContact, "Registered ring head trigger 8[2] does not initially overlap Pull body 7[0].");

        var angles = level.parts.ToDictionary(part => part.id, part => part.angle);
        var removed = new HashSet<int>();
        Vector2 axis = RotateRingsRules.SlideAxis(pull, catalog.templates.First(template => template.id == pull.templateId)).normalized;
        float foundSlide = 0f; int foundShape = -1;
        foreach (int sign in new[] { 1, -1 })
        {
            for (int step = 1; step <= 600; step++)
            {
                float slide = sign * step * .025f;
                var slides = new Dictionary<int, float> { { 7, slide } };
                world.ApplyState(angles, removed, slides);
                if (DirectContact(world, 8, 2, 7, 0)) continue;
                bool pullStillSeesRing = Sees(pullColliders[pullQuery.shapeIndex], ringColliders, pullQuery.contactFilter.Native());
                var snapshot = (RotateRingsCollisionWorld.BindingStateSnapshot)world.CaptureBindingState();
                bool passiveActive = snapshot.passiveRingRelationIndices != null && Array.IndexOf(snapshot.passiveRingRelationIndices, relationIndex) >= 0;
                if (pullStillSeesRing && !passiveActive)
                {
                    foundSlide = slide;
                    for (int i = 0; i < ringColliders.Length; i++)
                        if (ringColliders[i].enabled && Physics2D.Distance(ringColliders[i], pullColliders[0]).isOverlapped) { foundShape = i; break; }
                    report.otherRingColliderStillVisibleToPullQuery = foundShape >= 0;
                    report.passiveRelationPrunedAtHeadExit = true;
                    report.registeredHeadExitFound = true;
                    report.registeredHeadExitSlide = slide;
                    report.registeredHeadExitShapeIndex = foundShape;
                    break;
                }
            }
            if (report.registeredHeadExitFound) break;
        }
        // Also probe arbitrary translated poses. Along the Pull's own axis this
        // particular source fixture clears the entire ring at once; the native
        // exit rule matters most when PullObject can still see a different ring
        // collider after its registered head collider has left.
        if (!report.registeredHeadExitFound)
        {
            Transform pullRoot = world.OwnerTransform(7);
            Vector3 origin = pullRoot.localPosition;
            var hits = new Collider2D[256];
            for (int ix = -40; ix <= 40 && !report.registeredHeadExitFound; ix++)
            for (int iy = -40; iy <= 40 && !report.registeredHeadExitFound; iy++)
            {
                float dx = ix * .25f, dy = iy * .25f;
                world.ApplyState(angles, removed);
                pullRoot.localPosition = origin + new Vector3(dx, dy, 0f);
                Physics2D.SyncTransforms(); world.InvalidateExternalGeometry();
                if (DirectContact(world, 8, 2, 7, 0)) continue;
                int touchingShape = -1;
                for (int i = 0; i < ringColliders.Length; i++)
                    if (i != 2 && ringColliders[i].enabled && Physics2D.Distance(ringColliders[i], pullColliders[0]).isOverlapped)
                    { touchingShape = i; break; }
                int count = pullColliders[pullQuery.shapeIndex].Overlap(pullQuery.contactFilter.Native(), hits);
                bool pullStillSeesRing = false;
                for (int i = 0; i < count; i++) if (Array.IndexOf(ringColliders, hits[i]) >= 0) { pullStillSeesRing = true; break; }
                var snapshot = (RotateRingsCollisionWorld.BindingStateSnapshot)world.CaptureBindingState();
                bool passiveActive = snapshot.passiveRingRelationIndices != null && Array.IndexOf(snapshot.passiveRingRelationIndices, relationIndex) >= 0;
                if (touchingShape < 0 || !pullStillSeesRing || passiveActive) continue;
                report.registeredHeadExitFound = true;
                report.registeredHeadExitDx = dx; report.registeredHeadExitDy = dy;
                report.registeredHeadExitShapeIndex = touchingShape;
                report.otherRingColliderStillVisibleToPullQuery = true;
                report.passiveRelationPrunedAtHeadExit = true;
            }
        }
        if (!report.registeredHeadExitFound) report.failures.Add("No diagnostic pose found where registered ring 8[2] exited, PullObject still saw another ring shape, and passive relation 8->7 was pruned.");
        else if (!report.otherRingColliderStillVisibleToPullQuery) report.failures.Add("The PullObject query did not retain visibility to another ring collider at the registered-head exit pose.");
    }

    static void CheckReceiverWallPolicy(RotateRingsLevelPack pack, RotateRingsTemplateCatalog catalog, Report report, out GameObject host)
    {
        host = null;
        RotateRingsLevel level = pack.levels.Find(item => item.sourceId == "10001");
        Require(level != null, "Source 10001 is missing.");
        RotateRingsLink relation = level.links.Find(link => link.source == 1 && link.target == 2);
        Require(relation != null, "Source 10001 is missing ring 1 -> ring 2.");
        var source = level.parts.Find(part => part.id == 1); var receiver = level.parts.Find(part => part.id == 2);
        host = new GameObject("Source Receiver Wall QA") { hideFlags = HideFlags.HideAndDontSave };
        host.transform.position = new Vector3(21000f, 21000f, 0f);
        var world = host.AddComponent<RotateRingsCollisionWorld>(); world.Build(level, catalog);
        var lookupField = typeof(RotateRingsCollisionWorld).GetField("shapeLookup", BindingFlags.Instance | BindingFlags.NonPublic);
        var policy = typeof(RotateRingsCollisionWorld).GetMethod("ShapePairAllowed", BindingFlags.Instance | BindingFlags.NonPublic);
        Require(lookupField != null && policy != null, "Could not inspect the live shape-pair receiver policy.");
        var lookup = (System.Collections.IDictionary)lookupField.GetValue(world);
        object wall = ShapeFor(lookup, world.GetOwnerColliders(1)[3]);
        object stem = ShapeFor(lookup, world.GetOwnerColliders(1)[1]);
        object body = ShapeFor(lookup, world.GetOwnerColliders(2)[0]);
        var angles = level.parts.ToDictionary(part => part.id, part => part.angle);
        var removed = new HashSet<int>();
        angles[1] = source.angle; angles[2] = receiver.angle; world.ApplyState(angles, removed);
        report.wallAllowedAt0 = (bool)policy.Invoke(world, new[] { wall, body, (object)2 });
        angles[1] = source.angle + 5f; world.ApplyState(angles, removed);
        report.wallAllowedAt5 = (bool)policy.Invoke(world, new[] { wall, body, (object)2 });
        angles[1] = source.angle + 5.01f; world.ApplyState(angles, removed);
        report.wallRejectedAt5Point01 = !(bool)policy.Invoke(world, new[] { wall, body, (object)2 });
        angles[1] = source.angle; world.ApplyState(angles, removed);
        report.wallExceptionRejectedWhenSourceMoves = !(bool)policy.Invoke(world, new[] { wall, body, (object)1 });
        report.stemNeverGetsWallException = !(bool)policy.Invoke(world, new[] { stem, body, (object)2 });
        Require(report.wallAllowedAt0 && report.wallAllowedAt5 && report.wallRejectedAt5Point01,
            "Receiver head-wall exception must allow source offsets through 5 degrees and reject beyond 5 degrees.");
        Require(report.wallExceptionRejectedWhenSourceMoves, "Head-wall exception must apply only while the receiver ring moves.");
        Require(report.stemNeverGetsWallException, "Hard-block stem must never inherit the head-wall exception.");
    }

    static Query FindQuery(QueryCatalog catalog, string templateId, string owner)
    {
        QueryTemplate template = catalog.templates.First(item => item.templateId == templateId);
        return template.queries.First(item => item.queryOwner == owner);
    }
    static bool Sees(Collider2D probe, Collider2D[] targets, ContactFilter2D filter)
    {
        var hits = new Collider2D[256]; int count = probe.Overlap(filter, hits);
        if (count == hits.Length) throw new InvalidOperationException("QA overlap buffer exhausted.");
        for (int i = 0; i < count; i++) if (Array.IndexOf(targets, hits[i]) >= 0) return true;
        return false;
    }
    static bool DirectContact(RotateRingsCollisionWorld world, int source, int sourceShape, int target, int targetShape)
    {
        var a = world.GetOwnerColliders(source); var b = world.GetOwnerColliders(target);
        return sourceShape < a.Count && targetShape < b.Count && a[sourceShape].enabled && b[targetShape].enabled &&
               Physics2D.Distance(a[sourceShape], b[targetShape]).isOverlapped;
    }
    static object ShapeFor(System.Collections.IDictionary lookup, Collider2D collider) => lookup[collider];
    static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    static string Hash(string path)
    {
        using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant();
    }
}
#endif

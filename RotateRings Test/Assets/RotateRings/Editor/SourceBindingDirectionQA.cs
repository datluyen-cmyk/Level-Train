#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

// Diagnostic pose probes, not solution traces. Expected observations use raw
// Collider2D.Overlap and the extracted serialized contracts independently of
// CollisionWorld's private query/persistence helpers.
public static class SourceBindingDirectionQA
{
    [Serializable] sealed class Catalog { public QueryTemplate[] templates; }
    [Serializable] sealed class QueryTemplate { public string templateId; public Query[] queries; }
    [Serializable] sealed class Query { public int shapeIndex; public string queryOwner; public Filter contactFilter; }
    [Serializable] sealed class Mask { public long m_Bits; }
    [Serializable] sealed class Filter
    {
        public int useTriggers, useLayerMask, useDepth, useOutsideDepth, useNormalAngle, useOutsideNormalAngle;
        public Mask layerMask;
        public float minDepth, maxDepth, minNormalAngle, maxNormalAngle;
        public ContactFilter2D Native()
        {
            var f = new ContactFilter2D(); f.NoFilter();
            f.SetLayerMask(unchecked((int)layerMask.m_Bits)); f.useLayerMask = useLayerMask != 0;
            f.useTriggers = useTriggers != 0;
            f.minDepth = minDepth; f.maxDepth = maxDepth;
            f.useDepth = useDepth != 0; f.useOutsideDepth = useOutsideDepth != 0;
            f.minNormalAngle = minNormalAngle; f.maxNormalAngle = maxNormalAngle;
            f.useNormalAngle = useNormalAngle != 0; f.useOutsideNormalAngle = useOutsideNormalAngle != 0;
            return f;
        }
    }
    [Serializable] sealed class Witness
    {
        public string sourceId, kind;
        public int source, target;
        public float targetAngle;
        public float targetDx, targetDy;
        public bool sourceQuery, targetQuery, relativeQuery, directContact;
        public bool legalMotionVerified = false;
    }
    [Serializable] sealed class Report
    {
        public bool passed;
        public int pairs, poses, assertions, failures, asymmetricPoses, bodyOnlyPoses, staticInputChecks;
        public int sourceQueryPoses, targetQueryPoses, relativeQueryPoses, directContactPoses;
        public string sourcePackSha256, templatesSha256, queriesSha256, collisionWorldSha256, qaSha256;
        public string scope = "Isolated original ring pairs at diagnostic poses; does not certify a legal drag path, Android runtime parity, or solvability.";
        public List<Witness> witnesses = new List<Witness>();
        public List<string> errors = new List<string>();
    }

    public static string Run(string destination)
    {
        string project = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string root = Path.GetDirectoryName(project);
        string packPath = Path.Combine(root, "Research/physics_v3/full500/source_parity_pack.json");
        string templatesPath = Path.Combine(root, "Research/physics_v3/full500/source_templates.json");
        if (!File.Exists(templatesPath)) templatesPath = Path.Combine(root, "Research/physics_v3/full500/templates.json");
        string queryPath = Path.Combine(Application.dataPath, "RotateRings/Resources/SourceOverlapQueries.json");
        var report = new Report();
        try
        {
            var pack = JsonUtility.FromJson<RotateRingsLevelPack>(File.ReadAllText(packPath));
            var templates = JsonUtility.FromJson<RotateRingsTemplateCatalog>(File.ReadAllText(templatesPath));
            var contracts = JsonUtility.FromJson<Catalog>(File.ReadAllText(queryPath)).templates.ToDictionary(t => t.templateId, t => t.queries);
            report.sourcePackSha256 = Hash(packPath); report.templatesSha256 = Hash(templatesPath); report.queriesSha256 = Hash(queryPath);
            report.collisionWorldSha256 = Hash(Path.Combine(Application.dataPath, "RotateRings/Scripts/RotateRingsCollisionWorld.cs"));
            report.qaSha256 = Hash(Path.Combine(Application.dataPath, "RotateRings/Editor/SourceBindingDirectionQA.cs"));
            foreach (var source in pack.levels.Where(l => l.sourceId == "10001" || l.sourceId == "10057"))
            foreach (var link in source.links)
            {
                var a = JsonUtility.FromJson<RotateRingsPart>(JsonUtility.ToJson(source.parts.Find(p => p.id == link.source)));
                var b = JsonUtility.FromJson<RotateRingsPart>(JsonUtility.ToJson(source.parts.Find(p => p.id == link.target)));
                if (a.kind != "ring" || b.kind != "ring") continue;
                var pair = new RotateRingsLevel { id = source.id, sourceId = source.sourceId,
                    sourceOnlyParityFixture = true, parts = new List<RotateRingsPart> { a, b },
                    links = new List<RotateRingsLink> { link } };
                var host = new GameObject("Directional binding diagnostic") { hideFlags = HideFlags.HideAndDontSave };
                host.transform.position = new Vector3(1000, 1000, 0);
                try
                {
                    var world = host.AddComponent<RotateRingsCollisionWorld>(); world.Build(pair, templates);
                    var ca = world.OwnerTransform(a.id).GetComponentsInChildren<Collider2D>(true);
                    var cb = world.OwnerTransform(b.id).GetComponentsInChildren<Collider2D>(true);
                    report.pairs++;
                    float bx = b.x, by = b.y;
                    foreach (float dx in new[] { -1.5f, 0f, 1.5f })
                    foreach (float dy in new[] { -1.5f, 0f, 1.5f })
                    for (int step = 0; step < 24; step++)
                    {
                        b.x = bx + dx; b.y = by + dy;
                        float angle = b.angle + 15 * step;
                        var angles = new Dictionary<int, float> { { a.id, a.angle }, { b.id, angle } };
                        world.RestoreBindingState(Seed(true));
                        world.ApplyState(angles, new HashSet<int>());
                        bool aq = Sees(contracts[a.templateId], ca, cb, false);
                        bool bq = Sees(contracts[b.templateId], cb, ca, false);
                        bool rq = Sees(contracts[a.templateId], ca, cb, true);
                        bool direct = Direct(link, ca, cb, templates.templates.First(t => t.id == a.templateId), templates.templates.First(t => t.id == b.templateId));
                        if (aq) report.sourceQueryPoses++; if (bq) report.targetQueryPoses++;
                        if (rq) report.relativeQueryPoses++; if (direct) report.directContactPoses++;
                        var actual = (RotateRingsCollisionWorld.BindingStateSnapshot)world.CaptureBindingState();
                        string context = source.sourceId + " " + a.id + "->" + b.id + " targetAngle=" + angle + " diagnostic translation=" + dx + "," + dy;
                        Check(report, Contains(actual.activeRingRelationIndices), aq || direct, context + " active source query");
                        Check(report, Contains(actual.passiveRingRelationIndices), bq || direct, context + " passive receiver query");
                        Check(report, world.HasOutgoingBinding(a.id), rq, context + " input relative query");
                        var saved = world.CaptureBindingState();
                        world.RestoreBindingState(Seed(false)); world.RestoreBindingState(saved);
                        var restored = (RotateRingsCollisionWorld.BindingStateSnapshot)world.CaptureBindingState();
                        Check(report, Contains(restored.activeRingRelationIndices), Contains(actual.activeRingRelationIndices), context + " active restore");
                        Check(report, Contains(restored.passiveRingRelationIndices), Contains(actual.passiveRingRelationIndices), context + " passive restore");
                        if (!direct && aq != bq)
                        {
                            report.asymmetricPoses++;
                            AddWitness(report, source, a, b, angle, dx, dy, aq, bq, rq, direct, "asymmetric persistence");
                        }
                        if (!direct && aq && !rq)
                        {
                            report.bodyOnlyPoses++;
                            Check(report, world.HasIncidentBinding(a.id), true, context + " body-only overlap still prevents solve");
                            AddWitness(report, source, a, b, angle, dx, dy, aq, bq, rq, direct, "body goal without relative input lock");
                        }
                        if (!direct && rq)
                        {
                            // The static configured map must still guard input after
                            // a separately pruned active-mask snapshot is restored.
                            world.RestoreBindingState(Seed(false));
                            Check(report, world.HasOutgoingBinding(a.id), true, context + " configured input map survives active prune");
                            report.staticInputChecks++;
                        }
                        report.poses++;
                    }
                }
                finally { UnityEngine.Object.DestroyImmediate(host); }
            }
            if (report.pairs == 0 || report.asymmetricPoses == 0 || report.bodyOnlyPoses == 0 || report.staticInputChecks == 0)
            { report.failures++; report.errors.Add("Required asymmetric/body-only/static-map regression coverage was not found."); }
        }
        catch (Exception ex) { report.failures++; report.errors.Add(ex.ToString()); }
        report.passed = report.failures == 0;
        Directory.CreateDirectory(Path.GetDirectoryName(destination));
        File.WriteAllText(destination, JsonUtility.ToJson(report, true));
        return destination;
    }

    static RotateRingsCollisionWorld.BindingStateSnapshot Seed(bool active) => new RotateRingsCollisionWorld.BindingStateSnapshot
    { activeRingRelationIndices = active ? new[] { 0 } : new int[0], passiveRingRelationIndices = active ? new[] { 0 } : new int[0] };
    static bool Contains(int[] values) => values != null && Array.IndexOf(values, 0) >= 0;
    static bool Sees(Query[] queries, Collider2D[] probes, Collider2D[] target, bool relativeOnly)
    {
        var hits = new Collider2D[512];
        foreach (var query in queries)
        {
            if (relativeOnly && query.queryOwner != "RingRelative") continue;
            var probe = probes[query.shapeIndex];
            if (!probe.enabled) continue;
            int count = probe.Overlap(query.contactFilter.Native(), hits);
            if (count == hits.Length) throw new InvalidOperationException("Diagnostic overlap buffer exhausted.");
            for (int i = 0; i < count; i++) if (hits[i].gameObject.layer != 6 && Array.IndexOf(target, hits[i]) >= 0) return true;
        }
        return false;
    }
    static bool Direct(RotateRingsLink link, Collider2D[] a, Collider2D[] b, RotateRingsOriginalTemplate ta, RotateRingsOriginalTemplate tb)
    {
        foreach (int si in link.sourceShapeIndices) foreach (int ti in link.targetShapeIndices)
            if (ta.physicalShapes[si].role == "head_trigger" && tb.physicalShapes[ti].role == "body" &&
                a[si].enabled && b[ti].enabled && Physics2D.Distance(a[si], b[ti]).isOverlapped) return true;
        return false;
    }
    static void Check(Report r, bool actual, bool expected, string context)
    {
        r.assertions++; if (actual == expected) return; r.failures++;
        if (r.errors.Count < 30) r.errors.Add(context + ": expected=" + expected + " actual=" + actual);
    }
    static void AddWitness(Report r, RotateRingsLevel l, RotateRingsPart a, RotateRingsPart b, float angle, float dx, float dy, bool aq, bool bq, bool rq, bool direct, string kind)
    {
        if (r.witnesses.Any(w => w.kind == kind)) return;
        r.witnesses.Add(new Witness { sourceId = l.sourceId, kind = kind, source = a.id, target = b.id,
            targetAngle = angle, targetDx = dx, targetDy = dy, sourceQuery = aq, targetQuery = bq, relativeQuery = rq, directContact = direct });
    }
    static string Hash(string path)
    {
        using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant();
    }
}
#endif

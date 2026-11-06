using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Isolated raw Unity Physics2D probe worker for source-parity research.
// Invoked from the Editor by reflection: SourceNativeOracle.ExecuteBatch(request,response).
public static class SourceNativeOracle
{
    [Serializable] sealed class BatchRequest
    {
        public int schemaVersion;
        public string sourcePackPath, templatesPath;
        public OracleQuery[] queries;
        public OracleMicroFixture[] microFixtures;
    }
    [Serializable] sealed class OracleQuery
    {
        public string queryId, kind, sourceLevelId, microFixtureId;
        public int owner;
        public long bodySourceColliderId;
        public OraclePose[] poses;
        public int[] removed;
        public OracleArgs args;
    }
    [Serializable] sealed class OracleArgs
    {
        public int layerMask;
        public bool useTriggers;
        public OracleVector direction;
        public float distance;
        public int otherOwner, otherShapeIndex;
    }
    [Serializable] sealed class OraclePose { public int id; public float x, y, angle; }
    [Serializable] sealed class OracleVector { public float x, y; }
    [Serializable] sealed class OracleMicroFixture { public string id; public OracleMicroPart[] parts; }
    [Serializable] sealed class OracleMicroPart { public int id; public float x, y, z, angle; public OracleShape[] shapes; public OracleRigidbody sourceRigidbody; }
    [Serializable] sealed class OracleTemplateCatalog { public OracleTemplate[] templates; }
    [Serializable] sealed class OracleTemplate { public string id; public OracleShape[] physicalShapes; }
    [Serializable] sealed class OracleShape
    {
        public long sourceColliderId;
        public string sourceColliderType, role, sourceRole;
        public int sourceLayer;
        public float localZ;
        public bool sourceEnabled, trigger;
        public Vector2 center;
        public float radius, edgeRadius;
        public Vector2[] points;
        public OraclePath[] paths;
        public Vector2 localPosition;
    }
    [Serializable] sealed class OraclePath { public Vector2[] points; }
    [Serializable] sealed class OracleLevelPack { public OracleLevel[] levels; }
    [Serializable] sealed class OracleLevel { public int id; public string sourceId; public OraclePart[] parts; }
    [Serializable] sealed class OraclePart
    {
        public int id;
        public string templateId;
        public float x, y, z, angle;
        public OracleColliderBinding[] sourceColliderBindings;
        public OracleRigidbody sourceRigidbody;
    }
    [Serializable] sealed class OracleRigidbody
    {
        public bool present, simulated, useFullKinematicContacts, useAutoMass;
        public int bodyType, interpolation, sleepMode, collisionDetection, constraints, includeLayers, excludeLayers;
        public float mass, linearDamping, angularDamping, gravityScale;
    }
    [Serializable] sealed class OracleColliderBinding { public long sourceColliderId; public int shapeIndex; }
    [Serializable] sealed class BatchResponse { public int schemaVersion = 3; public OracleResult[] results; }
    [Serializable] sealed class OracleResult
    {
        public string queryId;
        public bool ok;
        public string error;
        public OracleHit[] overlaps, hits;
        public OracleDistance distance;
        public OraclePose pose;
    }
    [Serializable] sealed class OracleHit { public int owner, shapeIndex; public long sourceColliderId; public float distance; }
    [Serializable] sealed class OracleDistance { public bool valid; public float distance; public OracleVector normal; }

    sealed class BuiltPart
    {
        public int id;
        public Transform root;
        public readonly List<Collider2D> colliders = new List<Collider2D>();
        public readonly List<long> colliderIds = new List<long>();
    }

    public static void ExecuteBatch(string requestPath, string responsePath)
    {
        if (string.IsNullOrEmpty(requestPath) || string.IsNullOrEmpty(responsePath))
            throw new ArgumentException("Request and response paths are required.");
        var request = JsonUtility.FromJson<BatchRequest>(File.ReadAllText(requestPath));
        if (request == null || request.schemaVersion != 3) throw new InvalidDataException("Unsupported source-oracle schema.");
        var pack = JsonUtility.FromJson<OracleLevelPack>(File.ReadAllText(request.sourcePackPath));
        var catalog = JsonUtility.FromJson<OracleTemplateCatalog>(File.ReadAllText(request.templatesPath));
        var levels = new Dictionary<string, OracleLevel>(StringComparer.Ordinal);
        foreach (var level in pack.levels) levels[level.sourceId] = level;
        var templates = new Dictionary<string, OracleTemplate>(StringComparer.Ordinal);
        foreach (var template in catalog.templates) templates[template.id] = template;

        var results = new OracleResult[request.queries.Length];
        for (int i = 0; i < request.queries.Length; i++)
        {
            var query = request.queries[i];
            var result = new OracleResult { queryId = query.queryId };
            results[i] = result;
            var roots = new List<GameObject>();
            try
            {
                var built = query.microFixtureId != null
                    ? BuildMicroFixture(query.microFixtureId, request.microFixtures, roots)
                    : BuildSourceLevel(query.sourceLevelId, levels, templates, roots);
                ApplyPoses(query.poses, built);
                if (query.removed != null)
                    foreach (int removedId in query.removed)
                        if (built.TryGetValue(removedId, out var removed))
                            foreach (var collider in removed.colliders) collider.enabled = false;
                Physics2D.SyncTransforms();
                ExecuteQuery(query, result, built);
                result.ok = true;
            }
            catch (Exception exception)
            {
                result.ok = false;
                result.error = exception.GetType().Name + ": " + exception.Message;
            }
            finally
            {
                for (int r = roots.Count - 1; r >= 0; r--)
                    if (roots[r] != null) UnityEngine.Object.DestroyImmediate(roots[r]);
                Physics2D.SyncTransforms();
            }
        }
        var response = new BatchResponse { results = results };
        string directory = Path.GetDirectoryName(Path.GetFullPath(responsePath));
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        string temporaryPath = responsePath + ".tmp";
        File.WriteAllText(temporaryPath, JsonUtility.ToJson(response));
        if (File.Exists(responsePath)) File.Delete(responsePath);
        File.Move(temporaryPath, responsePath);
    }

    static Dictionary<int, BuiltPart> BuildSourceLevel(string sourceLevelId, Dictionary<string, OracleLevel> levels,
        Dictionary<string, OracleTemplate> templates, List<GameObject> roots)
    {
        if (!levels.TryGetValue(sourceLevelId, out var level)) throw new KeyNotFoundException("Unknown source level " + sourceLevelId);
        var result = new Dictionary<int, BuiltPart>();
        foreach (var part in level.parts)
        {
            if (!templates.TryGetValue(part.templateId, out var template)) throw new KeyNotFoundException("Missing template " + part.templateId);
            var built = CreatePart(part.id, part.x, part.y, part.z, part.angle, template.physicalShapes, part.sourceRigidbody, roots);
            var assigned = new bool[built.colliderIds.Count];
            if (part.sourceColliderBindings != null)
                foreach (var binding in part.sourceColliderBindings)
                    if (binding.shapeIndex >= 0 && binding.shapeIndex < built.colliderIds.Count)
                    {
                        built.colliderIds[binding.shapeIndex] = binding.sourceColliderId;
                        assigned[binding.shapeIndex] = true;
                    }
            for (int i = 0; i < assigned.Length; i++)
                if (!assigned[i]) throw new InvalidDataException("Source collider binding missing for part " + part.id + " shape " + i);
            result.Add(part.id, built);
        }
        return result;
    }

    static Dictionary<int, BuiltPart> BuildMicroFixture(string id, OracleMicroFixture[] fixtures, List<GameObject> roots)
    {
        OracleMicroFixture fixture = null;
        if (fixtures != null) foreach (var candidate in fixtures) if (candidate.id == id) { fixture = candidate; break; }
        if (fixture == null) throw new KeyNotFoundException("Unknown micro fixture " + id);
        var result = new Dictionary<int, BuiltPart>();
        foreach (var part in fixture.parts)
            result.Add(part.id, CreatePart(part.id, part.x, part.y, part.z, part.angle, part.shapes, part.sourceRigidbody, roots));
        return result;
    }

    static BuiltPart CreatePart(int id, float x, float y, float z, float angle, OracleShape[] shapes, OracleRigidbody sourceBody, List<GameObject> roots)
    {
        var rootObject = new GameObject("Source oracle part " + id);
        roots.Add(rootObject);
        rootObject.transform.position = new Vector3(x, y, z);
        rootObject.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        ApplySourceRigidbody(rootObject, sourceBody);
        var result = new BuiltPart { id = id, root = rootObject.transform };
        foreach (var shape in shapes)
        {
            var child = new GameObject(shape.role ?? "source collider");
            child.transform.SetParent(rootObject.transform, false);
            child.transform.localPosition = new Vector3(shape.localPosition.x, shape.localPosition.y, shape.localZ);
            child.layer = Mathf.Clamp(shape.sourceLayer, 0, 31);
            string colliderType = shape.sourceColliderType ?? "PolygonCollider2D";
            Collider2D collider;
            if (colliderType == "CircleCollider2D")
            {
                var circle = child.AddComponent<CircleCollider2D>();
                circle.offset = shape.center;
                circle.radius = shape.radius;
                collider = circle;
            }
            else if (colliderType == "BoxCollider2D")
            {
                var box = child.AddComponent<BoxCollider2D>();
                BoundsFromBoxPoints(shape.points, out var center, out var size, out var localAngle);
                child.transform.localPosition += (Vector3)center;
                box.offset = Vector2.zero;
                box.size = size;
                box.edgeRadius = shape.edgeRadius;
                child.transform.localRotation = Quaternion.Euler(0f, 0f, localAngle);
                collider = box;
            }
            else if (colliderType == "PolygonCollider2D")
            {
                var polygon = child.AddComponent<PolygonCollider2D>();
                var paths = shape.paths;
                if (paths != null && paths.Length > 0)
                {
                    polygon.pathCount = paths.Length;
                    for (int p = 0; p < paths.Length; p++) polygon.SetPath(p, paths[p].points);
                }
                else
                {
                    polygon.pathCount = 1;
                    polygon.SetPath(0, shape.points);
                }
                collider = polygon;
            }
            else throw new NotSupportedException("Unsupported source collider type " + colliderType);
            collider.isTrigger = shape.trigger;
            collider.enabled = shape.sourceEnabled;
            result.colliders.Add(collider);
            result.colliderIds.Add(shape.sourceColliderId);
        }
        return result;
    }

    static void ApplySourceRigidbody(GameObject root, OracleRigidbody source)
    {
        if (source == null || !source.present) return;
        var body = root.AddComponent<Rigidbody2D>();
        body.bodyType = (RigidbodyType2D)source.bodyType;
        body.simulated = source.simulated;
        body.useFullKinematicContacts = source.useFullKinematicContacts;
        body.mass = source.mass;
        body.gravityScale = source.gravityScale;
        body.interpolation = (RigidbodyInterpolation2D)source.interpolation;
        body.sleepMode = (RigidbodySleepMode2D)source.sleepMode;
        body.collisionDetectionMode = (CollisionDetectionMode2D)source.collisionDetection;
        body.constraints = (RigidbodyConstraints2D)source.constraints;
        SetBodyProperty(body, "useAutoMass", source.useAutoMass);
        SetBodyProperty(body, "linearDamping", source.linearDamping);
        SetBodyProperty(body, "drag", source.linearDamping);
        SetBodyProperty(body, "angularDamping", source.angularDamping);
        SetBodyProperty(body, "angularDrag", source.angularDamping);
        SetBodyProperty(body, "includeLayers", source.includeLayers);
        SetBodyProperty(body, "excludeLayers", source.excludeLayers);
    }

    static void SetBodyProperty(Rigidbody2D body, string propertyName, object value)
    {
        var property = typeof(Rigidbody2D).GetProperty(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (property == null || !property.CanWrite) return;
        object converted = value;
        if (property.PropertyType == typeof(LayerMask) && value is int)
            converted = (LayerMask)(int)value;
        else if (value != null && property.PropertyType != value.GetType())
            converted = Convert.ChangeType(value, property.PropertyType);
        property.SetValue(body, converted, null);
    }

    static void BoundsFromBoxPoints(Vector2[] points, out Vector2 center, out Vector2 size, out float localAngle)
    {
        if (points == null || points.Length < 4) { center = Vector2.zero; size = Vector2.one; localAngle = 0f; return; }
        center = Vector2.zero;
        for (int i = 0; i < points.Length; i++) center += points[i];
        center /= points.Length;
        Vector2 xAxis = points[1] - points[0];
        Vector2 yAxis = points[2] - points[1];
        size = new Vector2(xAxis.magnitude, yAxis.magnitude);
        localAngle = Mathf.Atan2(xAxis.y, xAxis.x) * Mathf.Rad2Deg;
    }

    static void ApplyPoses(OraclePose[] poses, Dictionary<int, BuiltPart> parts)
    {
        if (poses != null)
            foreach (var pose in poses)
                if (parts.TryGetValue(pose.id, out var part))
                {
                    part.root.position = new Vector3(pose.x, pose.y, part.root.position.z);
                    part.root.rotation = Quaternion.Euler(0f, 0f, pose.angle);
                }
    }

    static Collider2D FindCollider(Dictionary<int, BuiltPart> parts, int owner, long sourceColliderId)
    {
        if (!parts.TryGetValue(owner, out var part)) throw new KeyNotFoundException("Missing owner " + owner);
        for (int i = 0; i < part.colliderIds.Count; i++)
            if (part.colliderIds[i] == sourceColliderId) return part.colliders[i];
        throw new KeyNotFoundException("Collider id " + sourceColliderId + " not found on owner " + owner);
    }

    static void ExecuteQuery(OracleQuery query, OracleResult result, Dictionary<int, BuiltPart> parts)
    {
        Collider2D source = FindCollider(parts, query.owner, query.bodySourceColliderId);
        var requestedPose = query.poses == null ? null : Array.Find(query.poses, p => p.id == query.owner);
        result.pose = requestedPose == null ? null : new OraclePose { id = requestedPose.id, x = requestedPose.x, y = requestedPose.y, angle = requestedPose.angle };
        if (query.kind == "overlaps")
        {
            ContactFilter2D filter = Filter(query.args, true);
            var buffer = new Collider2D[256];
            int count = source.Overlap(filter, buffer);
            if (count == buffer.Length) { buffer = new Collider2D[1024]; count = source.Overlap(filter, buffer); }
            var hits = new List<OracleHit>();
            for (int i = 0; i < count; i++) if (TryMapCollider(parts, buffer[i], out var hit)) hits.Add(hit);
            result.overlaps = hits.ToArray();
        }
        else if (query.kind == "cast")
        {
            ContactFilter2D filter = Filter(query.args, false);
            var buffer = new RaycastHit2D[256];
            int count = source.Cast(new Vector2(query.args.direction.x, query.args.direction.y), filter, buffer, query.args.distance);
            if (count == buffer.Length) { buffer = new RaycastHit2D[1024]; count = source.Cast(new Vector2(query.args.direction.x, query.args.direction.y), filter, buffer, query.args.distance); }
            var hits = new List<OracleHit>();
            for (int i = 0; i < count; i++)
                if (TryMapCollider(parts, buffer[i].collider, out var hit)) { hit.distance = buffer[i].distance; hits.Add(hit); }
            result.hits = hits.ToArray();
        }
        else if (query.kind == "distance")
        {
            if (!parts.TryGetValue(query.args.otherOwner, out var otherPart) || query.args.otherShapeIndex < 0 || query.args.otherShapeIndex >= otherPart.colliders.Count)
                throw new IndexOutOfRangeException("Distance query other shape is out of range.");
            var distance = source.Distance(otherPart.colliders[query.args.otherShapeIndex]);
            result.distance = new OracleDistance
            {
                valid = distance.isValid,
                distance = distance.distance,
                normal = new OracleVector { x = distance.normal.x, y = distance.normal.y }
            };
        }
        else throw new NotSupportedException("Unknown query kind " + query.kind);
    }

    static ContactFilter2D Filter(OracleArgs args, bool defaultTriggers)
    {
        var filter = new ContactFilter2D();
        filter.NoFilter();
        if (args != null && args.layerMask != 0) filter.SetLayerMask(args.layerMask);
        filter.useTriggers = args == null ? defaultTriggers : args.useTriggers;
        return filter;
    }

    static bool TryMapCollider(Dictionary<int, BuiltPart> parts, Collider2D collider, out OracleHit result)
    {
        foreach (var pair in parts)
            for (int i = 0; i < pair.Value.colliders.Count; i++)
                if (pair.Value.colliders[i] == collider)
                {
                    result = new OracleHit { owner = pair.Key, shapeIndex = i, sourceColliderId = pair.Value.colliderIds[i] };
                    return true;
                }
        result = null;
        return false;
    }
}

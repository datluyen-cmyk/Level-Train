using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// Exact authored contact relations are resolved through source feature shape indices.
// Rendering and `CanCollect` never create or remove collision bindings.
[Serializable]
public sealed class RotateRingsAuthoredContactState
{
    public string kind;
    public int ownerId, targetId, ownerShapeIndex, targetShapeIndex;
    public string ownerRole, targetRole, featureId;
}

public sealed class RotateRingsCollisionWorld : MonoBehaviour
{
    public const float MaxAngularStep = .5f;
    public const float MaxLinearStep = .12f;
    public const float PenetrationTolerance = .0005f;

    sealed class Shape
    {
        public int owner, index, attachment;
        public long sourceColliderId;
        public string role;
        public bool trigger, sourceEnabled;
        public bool relativeMarker, relativePhysicalBlock;
        public Collider2D collider;
    }

    struct ContactKey : IEquatable<ContactKey>
    {
        public int ownerA, shapeA, ownerB, shapeB;
        public ContactKey(int oa, int sa, int ob, int sb)
        { ownerA = oa; shapeA = sa; ownerB = ob; shapeB = sb; }
        public bool Equals(ContactKey other) => ownerA == other.ownerA && shapeA == other.shapeA && ownerB == other.ownerB && shapeB == other.shapeB;
        public override bool Equals(object obj) => obj is ContactKey && Equals((ContactKey)obj);
        public override int GetHashCode()
        {
            unchecked { int h = ownerA; h = h * 397 ^ shapeA; h = h * 397 ^ ownerB; return h * 397 ^ shapeB; }
        }
    }

    struct AuthoredContactKey : IEquatable<AuthoredContactKey>
    {
        public int source, target;
        public long sourceColliderId, targetColliderId;
        public AuthoredContactKey(int s, long sc, int t, long tc)
        { source = s; sourceColliderId = sc; target = t; targetColliderId = tc; }
        public bool Equals(AuthoredContactKey other) => source == other.source && sourceColliderId == other.sourceColliderId && target == other.target && targetColliderId == other.targetColliderId;
        public override bool Equals(object obj) => obj is AuthoredContactKey && Equals((AuthoredContactKey)obj);
        public override int GetHashCode()
        { unchecked { int h = source; h = h * 397 ^ sourceColliderId.GetHashCode(); h = h * 397 ^ target; return h * 397 ^ targetColliderId.GetHashCode(); } }
    }

    [Serializable]
    public sealed class BindingStateSnapshot
    {
        public int[] activeRingRelationIndices;
        public int[] passiveRingRelationIndices;
        public string[] activeGuideRelationKeys;
        public int[] activeRingContactOwners;
        public long[] activeRingContactColliderIds;
    }

    [Serializable] sealed class SourcePhysicsMaterialCatalog { public SourcePhysicsMaterial[] materials; }
    [Serializable] sealed class SourcePhysicsMaterial
    {
        public long pathId;
        public string sourceName;
        public float friction, bounciness;
    }

    [Serializable] sealed class SourceOverlapCatalog { public SourceOverlapTemplate[] templates; }
    [Serializable] sealed class SourceOverlapTemplate { public string templateId; public SourceOverlapQuery[] queries; }
    [Serializable] sealed class SourceQueryLayerMask { public long m_Bits; }
    [Serializable] sealed class SourceQueryFilter
    {
        public int useTriggers, useLayerMask, useDepth, useOutsideDepth, useNormalAngle, useOutsideNormalAngle;
        public SourceQueryLayerMask layerMask;
        public float minDepth, maxDepth, minNormalAngle, maxNormalAngle;
        public ContactFilter2D Create()
        {
            var result = new ContactFilter2D();
            result.NoFilter();
            result.useTriggers = useTriggers != 0;
            result.SetLayerMask(unchecked((int)(layerMask == null ? 0L : layerMask.m_Bits)));
            result.useLayerMask = useLayerMask != 0;
            result.minDepth = minDepth; result.maxDepth = maxDepth;
            result.useDepth = useDepth != 0; result.useOutsideDepth = useOutsideDepth != 0;
            result.minNormalAngle = minNormalAngle; result.maxNormalAngle = maxNormalAngle;
            result.useNormalAngle = useNormalAngle != 0; result.useOutsideNormalAngle = useOutsideNormalAngle != 0;
            return result;
        }
    }
    [Serializable] sealed class SourceOverlapQuery
    {
        public int shapeIndex;
        public string queryOwner, sourceRole;
        public SourceQueryFilter contactFilter;
        public int selfShapeIndex;
        public int[] blockShapeIndices, wallShapeIndices;
    }
    sealed class OverlapProbe
    {
        public Shape shape, selfShape;
        public bool relative;
        public ContactFilter2D filter;
        public int[] wallShapes;
        public readonly List<Shape> hits = new List<Shape>();
        public readonly List<Shape> wallHits = new List<Shape>();
    }
    static string cachedOverlapCatalogText;
    static Dictionary<string, SourceOverlapQuery[]> cachedOverlapCatalog;

    sealed class Body
    {
        public RotateRingsPart part;
        public Transform root;
        public float angle, slide;
        public float gestureStartAngle;
        public Vector2 axis, lastMoveDirection;
        public bool removed, inGesture;
        public float gestureStartSlide;
        public int blockMotionSign, persistedBlockMotionSign;
        public readonly List<Shape> shapes = new List<Shape>();
        public readonly List<OverlapProbe> overlapProbes = new List<OverlapProbe>();
        public readonly HashSet<int> queriedOwners = new HashSet<int>();
        public readonly HashSet<int> queriedRelativeOwners = new HashSet<int>();
        public long queryPoseEpoch = -1;
        public long wallQueryPoseEpoch = -1;
        public readonly HashSet<Collider2D> gestureExternalColliders = new HashSet<Collider2D>();
        public float gestureExternalPenetration, gestureInterlockPenetration, gestureStickPenetration;
        public bool hasRelatives;
        public readonly List<Shape> externalHits = new List<Shape>();
        public readonly List<Shape> interlockBodyHits = new List<Shape>();
        public readonly HashSet<Shape> staticBoundColliders = new HashSet<Shape>();
        public readonly HashSet<int> staticBoundPartners = new HashSet<int>();
        // Initial hard contacts can be escaped, but their penetration may never increase.
        public readonly Dictionary<ContactKey, float> gestureBaseline = new Dictionary<ContactKey, float>();
        public readonly Dictionary<ContactKey, float> gestureStickBaseline = new Dictionary<ContactKey, float>();
    }

    sealed class RingRelation
    {
        public int index, source, target, attachment;
        public string feature;
        public int[] sourceShapes, targetShapes;
    }

    sealed class GuideRelation
    {
        public int guideOwner, lockedOwner;
        public string guideId, lockedPartId;
        public int[] guideShapes, lockedShapes;
    }

    readonly Dictionary<int, Body> bodies = new Dictionary<int, Body>();
    readonly Dictionary<Collider2D, Shape> shapeLookup = new Dictionary<Collider2D, Shape>();
    readonly Dictionary<long, PhysicsMaterial2D> sourcePhysicsMaterials = new Dictionary<long, PhysicsMaterial2D>();
    readonly List<PhysicsMaterial2D> ownedPhysicsMaterials = new List<PhysicsMaterial2D>();
    readonly List<RingRelation> ringRelations = new List<RingRelation>();
    readonly List<GuideRelation> guideRelations = new List<GuideRelation>();
    readonly HashSet<int> activeRingRelations = new HashSet<int>();
    readonly HashSet<int> passiveRingRelations = new HashSet<int>();
    readonly HashSet<AuthoredContactKey> activeRingContacts = new HashSet<AuthoredContactKey>();
    readonly HashSet<string> activeGuideRelations = new HashSet<string>();
    Collider2D[] overlaps = new Collider2D[256];
    readonly RaycastHit2D[] castHits = new RaycastHit2D[256];
    bool bindingsDirty = true;
    bool bindingsInitialized;
    bool suppressMotionSample;
    long poseEpoch;
    ContactFilter2D allFilter;
    ContactFilter2D relativeWallFilter;

    public int LastBlockerOwner { get; private set; } = -1;
    public int LastBlockerShapeIndex { get; private set; } = -1;
    public string LastBlockerRole { get; private set; }
    public int ActiveAuthoredContactCount
    {
        get { RefreshBindings(); var edges = new HashSet<int>(activeRingRelations); edges.UnionWith(passiveRingRelations); return edges.Count + activeGuideRelations.Count; }
    }
    public event Action<int> MotionSample;

    static ContactKey ShapePair(int ownerA, int shapeA, int ownerB, int shapeB) => new ContactKey(ownerA, shapeA, ownerB, shapeB);
    static string GuideKey(GuideRelation relation) => relation.guideOwner + ":" + relation.guideId + ":" + relation.lockedOwner;

    public void Build(RotateRingsLevel level, RotateRingsTemplateCatalog catalog)
    {
        Clear();
        if (level == null) throw new ArgumentNullException(nameof(level));
        if (catalog == null || catalog.templates == null)
            throw new InvalidOperationException("Missing source physics template catalog.");
        allFilter = new ContactFilter2D();
        allFilter.NoFilter();
        allFilter.useTriggers = true;
        relativeWallFilter = new ContactFilter2D(); relativeWallFilter.NoFilter();
        relativeWallFilter.useTriggers = true; relativeWallFilter.SetLayerMask(128);
        LoadSourcePhysicsMaterials();
        LoadSourceOverlapQueries();

        var templates = new Dictionary<string, RotateRingsOriginalTemplate>();
        foreach (var template in catalog.templates) templates.Add(template.id, template);

        foreach (var part in level.parts)
        {
            if (string.IsNullOrEmpty(part.templateId) || !templates.TryGetValue(part.templateId, out var template))
                throw new InvalidOperationException("Unverified original template for part " + part.id);
            var root = new GameObject("Physical original piece " + part.id);
            root.transform.SetParent(transform, false);
            ApplySourceRigidbody(root, part.sourceRigidbody);
            var body = new Body
            {
                part = part,
                root = root.transform,
                angle = part.angle,
                axis = RotateRingsRules.IsSlider(part) ? RotateRingsRules.SlideAxis(part, template) : Vector2.zero
            };
            bodies.Add(part.id, body);
            if (template.physicalShapes == null || template.physicalShapes.Length == 0)
                throw new InvalidOperationException("Template has no original colliders: " + template.id);

            for (int shapeIndex = 0; shapeIndex < template.physicalShapes.Length; shapeIndex++)
            {
                var definition = template.physicalShapes[shapeIndex];
                var go = new GameObject(definition.role + " " + shapeIndex);
                go.transform.SetParent(root.transform, false);
                go.transform.localPosition = new Vector3(0f, 0f, definition.localZ);
                if (definition.sourceLayer < 0 || definition.sourceLayer > 31)
                    throw new InvalidOperationException("Invalid source collider layer " + definition.sourceLayer + " in " + template.id);
                go.layer = definition.sourceLayer;
                Collider2D collider;
                string shapeType = !string.IsNullOrEmpty(definition.sourceColliderType)
                    ? definition.sourceColliderType
                    : (string.IsNullOrEmpty(definition.shapeType) ? "PolygonCollider2D" : definition.shapeType);
                if (shapeType.Equals("circle", StringComparison.OrdinalIgnoreCase) || shapeType == "CircleCollider2D")
                {
                    var circle = go.AddComponent<CircleCollider2D>();
                    circle.offset = definition.center;
                    circle.radius = definition.radius;
                    collider = circle;
                }
                else if (shapeType.Equals("box", StringComparison.OrdinalIgnoreCase) || shapeType == "BoxCollider2D")
                {
                    var box = go.AddComponent<BoxCollider2D>();
                    BoundsFromBoxPoints(definition.points, out var center, out var size, out var localAngle);
                    go.transform.localPosition = new Vector3(center.x, center.y, definition.localZ);
                    go.transform.localRotation = Quaternion.Euler(0f, 0f, localAngle);
                    box.offset = Vector2.zero;
                    box.size = size;
                    box.edgeRadius = definition.edgeRadius;
                    collider = box;
                }
                else if (shapeType.Equals("polygon", StringComparison.OrdinalIgnoreCase) || shapeType == "PolygonCollider2D")
                {
                    var polygon = go.AddComponent<PolygonCollider2D>();
                    if (definition.paths != null && definition.paths.Length > 0)
                    {
                        polygon.pathCount = definition.paths.Length;
                        for (int path = 0; path < definition.paths.Length; path++)
                            polygon.SetPath(path, definition.paths[path].points);
                    }
                    else
                    {
                        polygon.pathCount = 1;
                        polygon.SetPath(0, definition.points);
                    }
                    collider = polygon;
                }
                else
                {
                    throw new NotSupportedException("Unsupported source collider shape type '" + definition.shapeType + "' in " + template.id);
                }
                collider.isTrigger = definition.trigger;
                var materialRef = definition.physicsMaterial != null && definition.physicsMaterial.pathId != 0
                    ? definition.physicsMaterial
                    : part.sourceRigidbody == null ? null : part.sourceRigidbody.physicsMaterial;
                if (materialRef != null && materialRef.pathId != 0)
                {
                    if (!sourcePhysicsMaterials.TryGetValue(materialRef.pathId, out var material))
                        throw new InvalidOperationException("Missing source PhysicsMaterial2D path id " + materialRef.pathId);
                    collider.sharedMaterial = material;
                }
                var shape = new Shape
                {
                    owner = part.id,
                    index = shapeIndex,
                    sourceColliderId = SourceColliderId(part, shapeIndex),
                    attachment = definition.attachmentId,
                    role = definition.role,
                    trigger = definition.trigger,
                    sourceEnabled = definition.sourceEnabled,
                    collider = collider
                };
                body.shapes.Add(shape);
                shapeLookup.Add(collider, shape);
            }
            if (part.kind == "ring" || part.kind == "slider")
            {
                if (!cachedOverlapCatalog.TryGetValue(part.templateId, out var queries))
                    throw new InvalidOperationException("Missing original overlap query contract for " + part.templateId);
                foreach (var query in queries)
                {
                    if (query.shapeIndex < 0 || query.shapeIndex >= body.shapes.Count || query.contactFilter == null ||
                        template.physicalShapes[query.shapeIndex].sourceRole != query.sourceRole)
                        throw new InvalidOperationException("Invalid original overlap query shape for " + part.templateId);
                    var probe = new OverlapProbe { shape = body.shapes[query.shapeIndex],
                        relative = query.queryOwner == "RingRelative", filter = query.contactFilter.Create(),
                        wallShapes = query.wallShapeIndices };
                    if (probe.relative)
                    {
                        body.hasRelatives = true;
                        if (query.selfShapeIndex != query.shapeIndex || query.blockShapeIndices == null || query.wallShapeIndices == null)
                            throw new InvalidOperationException("Missing original relative collider contract for " + part.templateId);
                        probe.selfShape = body.shapes[query.selfShapeIndex];
                        probe.selfShape.relativeMarker = true;
                        foreach (int index in query.blockShapeIndices)
                        {
                            if (index < 0 || index >= body.shapes.Count) throw new InvalidOperationException("Invalid relative block collider index.");
                            body.shapes[index].relativeMarker = true;
                            body.shapes[index].relativePhysicalBlock = true;
                        }
                    }
                    body.overlapProbes.Add(probe);
                }
            }
            SetPose(body);
        }

        if (level.links != null)
        {
            for (int i = 0; i < level.links.Count; i++)
            {
                var link = level.links[i];
                if (link.sourceShapeIndices == null || link.targetShapeIndices == null ||
                    link.sourceShapeIndices.Length == 0 || link.targetShapeIndices.Length == 0)
                {
                    if (level.sourceOnlyParityFixture || level.sourceReference)
                        throw new InvalidOperationException("Source link is missing exact feature shape indices: " + link.source + " -> " + link.target);
                    continue;
                }
                var relation = new RingRelation
                {
                    index = i, source = link.source, target = link.target,
                    attachment = link.attachmentId, feature = link.sourceFeature,
                    sourceShapes = link.sourceShapeIndices, targetShapes = link.targetShapeIndices
                };
                ValidateShapeIndices(relation.source, relation.sourceShapes, "ring source feature");
                ValidateShapeIndices(relation.target, relation.targetShapes, "ring target feature");
                ringRelations.Add(relation);
                if (bodies[relation.target].part.kind == "ring" && bodies[relation.source].part.kind == "ring")
                    foreach (var probe in bodies[relation.source].overlapProbes)
                        if (probe.relative && Contains(relation.sourceShapes, probe.selfShape.index))
                        {
                            bodies[relation.target].staticBoundColliders.Add(probe.selfShape);
                            bodies[relation.target].staticBoundPartners.Add(relation.source);
                        }
            }
        }

        if (level.guideLinks != null)
        {
            foreach (var guide in level.guideLinks)
            {
                if (guide.guideShapeIndices == null || guide.guideShapeIndices.Length == 0)
                {
                    if (level.sourceOnlyParityFixture || level.sourceReference)
                        throw new InvalidOperationException("Source guide is missing exact guide shape indices: " + guide.guidePartId);
                    continue;
                }
                if (!bodies.TryGetValue(guide.guideOwnerId, out var guideBody) || !bodies.TryGetValue(guide.lockedOwnerId, out var lockedBody))
                    throw new InvalidOperationException("Guide relation references missing owner: " + guide.guidePartId);
                var lockedShapes = new List<int>();
                // Native CatchGuide contacts come from the main/relative overlap
                // handlers, not every collider registered under the moving root.
                // Hard pins and relative walls also have source identities but
                // must not keep a guide bound after its query bodies leave.
                foreach (var probe in lockedBody.overlapProbes)
                    if (!lockedShapes.Contains(probe.shape.index)) lockedShapes.Add(probe.shape.index);
                if (lockedShapes.Count == 0)
                    throw new InvalidOperationException("Guide relation has no source overlap collider binding: " + guide.guidePartId);
                var relation = new GuideRelation
                {
                    guideOwner = guide.guideOwnerId, lockedOwner = guide.lockedOwnerId,
                    guideId = guide.guidePartId, lockedPartId = guide.lockedPartId,
                    guideShapes = guide.guideShapeIndices, lockedShapes = lockedShapes.ToArray()
                };
                ValidateShapeIndices(relation.guideOwner, relation.guideShapes, "guide feature");
                ValidateShapeIndices(relation.lockedOwner, relation.lockedShapes, "locked-part overlap collider");
                guideRelations.Add(relation);
            }
        }

        Physics2D.SyncTransforms();
        bindingsDirty = true;
        RefreshBindings();
    }

    void LoadSourcePhysicsMaterials()
    {
        var asset = Resources.Load<TextAsset>("SourcePhysicsMaterials");
        if (asset == null) throw new InvalidOperationException("Missing Resources/SourcePhysicsMaterials.json.");
        var catalog = JsonUtility.FromJson<SourcePhysicsMaterialCatalog>(asset.text);
        if (catalog == null || catalog.materials == null) throw new InvalidOperationException("Malformed source PhysicsMaterial2D catalog.");
        foreach (var source in catalog.materials)
        {
            var material = new PhysicsMaterial2D(source.sourceName ?? ("Source Physics Material " + source.pathId));
            material.friction = source.friction;
            material.bounciness = source.bounciness;
            material.hideFlags = HideFlags.HideAndDontSave;
            sourcePhysicsMaterials.Add(source.pathId, material);
            ownedPhysicsMaterials.Add(material);
        }
    }

    static void LoadSourceOverlapQueries()
    {
        var asset = Resources.Load<TextAsset>("SourceOverlapQueries");
        if (asset == null) throw new InvalidOperationException("Missing Resources/SourceOverlapQueries.json.");
        string contents = asset.text;
        if (cachedOverlapCatalog != null && cachedOverlapCatalogText == contents) return;
        var catalog = JsonUtility.FromJson<SourceOverlapCatalog>(contents);
        if (catalog == null || catalog.templates == null) throw new InvalidOperationException("Malformed source overlap query catalog.");
        var parsed = new Dictionary<string, SourceOverlapQuery[]>();
        foreach (var template in catalog.templates) parsed.Add(template.templateId, template.queries);
        cachedOverlapCatalog = parsed;
        cachedOverlapCatalogText = contents;
    }

    static long SourceColliderId(RotateRingsPart part, int shapeIndex)
    {
        if (part.sourceColliderBindings != null)
            foreach (var binding in part.sourceColliderBindings)
                if (binding.shapeIndex == shapeIndex) return binding.sourceColliderId;
        return 0L;
    }

    static void ApplySourceRigidbody(GameObject root, RotateRingsRigidbody source)
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
        var property = typeof(Rigidbody2D).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property == null || !property.CanWrite) return;
        object converted = value;
        if (property.PropertyType == typeof(LayerMask) && value is int)
            converted = (LayerMask)(int)value;
        else if (value != null && property.PropertyType != value.GetType())
            converted = Convert.ChangeType(value, property.PropertyType);
        property.SetValue(body, converted, null);
    }

    void ValidateShapeIndices(int owner, int[] indices, string relation)
    {
        if (!bodies.TryGetValue(owner, out var body)) throw new InvalidOperationException("Missing " + relation + " owner " + owner);
        foreach (int index in indices)
            if (index < 0 || index >= body.shapes.Count)
                throw new InvalidOperationException("Out-of-range " + relation + " shape index " + index + " on owner " + owner);
    }

    void Clear()
    {
        foreach (var body in bodies.Values)
        {
            foreach (var shape in body.shapes) if (shape.collider != null) shape.collider.enabled = false;
            if (body.root != null)
            {
                if (Application.isPlaying) Destroy(body.root.gameObject);
                else DestroyImmediate(body.root.gameObject);
            }
        }
        foreach (var material in ownedPhysicsMaterials) if (material != null) DestroyObject(material);
        ownedPhysicsMaterials.Clear();
        sourcePhysicsMaterials.Clear();
        bodies.Clear();
        shapeLookup.Clear();
        ringRelations.Clear();
        guideRelations.Clear();
        activeRingRelations.Clear();
        passiveRingRelations.Clear();
        activeRingContacts.Clear();
        activeGuideRelations.Clear();
        bindingsDirty = true;
        bindingsInitialized = false;
    }

    static void DestroyObject(UnityEngine.Object value)
    {
        if (Application.isPlaying) Destroy(value);
        else DestroyImmediate(value);
    }

    void OnDestroy() { Clear(); }

    void SetPose(Body body)
    {
        // Any owner's movement/removal can change every other owner's queries.
        // Cache only until the next explicit pose update, never across a move.
        poseEpoch++;
        Vector3 oldPosition = body.root.localPosition;
        var p = body.part;
        body.root.localPosition = new Vector3(p.x + body.axis.x * body.slide, p.y + body.axis.y * body.slide, p.z);
        body.root.localRotation = Quaternion.Euler(0, 0, body.angle);
        Vector2 delta = (Vector2)(body.root.localPosition - oldPosition);
        if (body.inGesture && delta.sqrMagnitude > 1e-12f) body.lastMoveDirection = delta;
        foreach (var shape in body.shapes) shape.collider.enabled = !body.removed && shape.sourceEnabled;
        bindingsDirty = true;
    }

    public void ApplyState(Dictionary<int, float> angles, HashSet<int> removed, Dictionary<int, float> slides = null)
    {
        foreach (var pair in bodies)
        {
            var body = pair.Value;
            body.angle = angles != null && angles.TryGetValue(pair.Key, out float a) ? a : body.part.angle;
            body.removed = removed != null && removed.Contains(pair.Key);
            body.slide = slides != null && slides.TryGetValue(pair.Key, out float s) ? s : 0f;
            SetPose(body);
        }
        Physics2D.SyncTransforms();
        RefreshBindings();
    }

    void EmitMotionSample(Body body)
    {
        if (body.inGesture && !suppressMotionSample && !body.removed && MotionSample != null)
            MotionSample(body.part.id);
    }

    public void BeginGesture(int id)
    {
        if (!bodies.TryGetValue(id, out var body) || body.removed) return;
        body.inGesture = true;
        body.gestureStartAngle = body.angle;
        body.lastMoveDirection = Vector2.zero;
        body.gestureStartSlide = body.slide;
        body.blockMotionSign = 0;
        body.persistedBlockMotionSign = 0;
        body.gestureBaseline.Clear();
        body.gestureStickBaseline.Clear();
        Physics2D.SyncTransforms();
        foreach (var contact in CollectHardContacts(body)) body.gestureBaseline[contact.Key] = contact.Value;
        foreach (var contact in CollectStickContacts(body)) body.gestureStickBaseline[contact.Key] = contact.Value;
        if (body.part.kind == "ring") CaptureNativeMotionBaseline(body);
        bindingsDirty = true;
        RefreshBindings();
    }

    /// <summary>
    /// Applies the source release overlap settle to sliders and clears the gesture
    /// baseline after the backstep. The settled pose is available from GetSlide/GetAngle.
    /// </summary>
    public void EndGesture(int id) => EndGesture(id, 0f);

    public void EndGesture(int id, float rotationBlockDirection)
    {
        if (!bodies.TryGetValue(id, out var body) || !body.inGesture) return;
        if (!body.removed && RotateRingsRules.IsSlider(body.part))
        {
            RollbackAfterStop(body);
            if (body.lastMoveDirection.sqrMagnitude > 1e-12f) ResolveReleaseOverlap(body);
        }
        else if (!body.removed && body.part.kind == "ring" && !Mathf.Approximately(rotationBlockDirection, 0f))
        {
            // RingObject.RollbackRotationAfterStop: back away by 0.1 degree after a
            // blocked drag, or snap to the gesture start when total travel was <= 0.1.
            float travel = Mathf.Abs(Mathf.DeltaAngle(body.gestureStartAngle, body.angle));
            float target = travel <= .1f
                ? body.gestureStartAngle
                : Mathf.Repeat(body.angle - Mathf.Sign(rotationBlockDirection) * .1f, 360f);
            body.angle = target;
            SetPose(body);
            Physics2D.SyncTransforms();
        }
        body.inGesture = false;
        body.gestureBaseline.Clear();
        body.gestureStickBaseline.Clear();
        body.gestureExternalColliders.Clear();
        body.lastMoveDirection = Vector2.zero;
        body.blockMotionSign = 0;
        body.persistedBlockMotionSign = 0;
        Physics2D.SyncTransforms();
        bindingsDirty = true;
        RefreshBindings();
    }

    public float GetAngle(int id) => bodies.TryGetValue(id, out var body) ? body.angle : 0f;
    public float GetSlide(int id) => bodies.TryGetValue(id, out var body) ? body.slide : 0f;

    public List<Collider2D> GetOwnerColliders(int owner)
    {
        var result = new List<Collider2D>();
        if (!bodies.TryGetValue(owner, out var body)) return result;
        foreach (var shape in body.shapes) if (shape.collider != null && shape.collider.enabled) result.Add(shape.collider);
        return result;
    }

    public int ColliderOwner(Collider2D collider)
    {
        return collider != null && shapeLookup.TryGetValue(collider, out var shape) ? shape.owner : -1;
    }

    // Native owner resolution walks through attached BombRelative colliders to
    // their gameplay root. They participate as observed colliders, without
    // becoming a template shape or an independent victory target.
    public void RegisterOwnedExternalCollider(Collider2D collider, int owner, long sourceColliderId, string role)
    {
        if (collider == null) throw new ArgumentNullException(nameof(collider));
        if (!bodies.ContainsKey(owner)) throw new InvalidOperationException("External collider has no gameplay owner: " + owner);
        shapeLookup.Add(collider, new Shape { owner = owner, index = -1, attachment = -1,
            sourceColliderId = sourceColliderId, role = role, trigger = collider.isTrigger,
            sourceEnabled = collider.enabled, collider = collider });
        InvalidateExternalGeometry();
    }

    public void UnregisterOwnedExternalCollider(Collider2D collider)
    {
        if (collider != null && shapeLookup.TryGetValue(collider, out var shape) && shape.index < 0)
        { shapeLookup.Remove(collider); InvalidateExternalGeometry(); }
    }

    public void InvalidateExternalGeometry()
    {
        poseEpoch++;
        bindingsDirty = true;
    }

    public Transform OwnerTransform(int owner)
    {
        return bodies.TryGetValue(owner, out var body) ? body.root : null;
    }

    public bool TryGetShapeWorldCenter(int owner, int shapeIndex, out Vector2 center)
    {
        center = Vector2.zero;
        if (!bodies.TryGetValue(owner, out var body) || shapeIndex < 0 || shapeIndex >= body.shapes.Count) return false;
        var collider = body.shapes[shapeIndex].collider;
        if (collider == null || !collider.enabled) return false;
        center = collider.bounds.center;
        return true;
    }

    public int GetOwnerShapeCount(int owner) => bodies.TryGetValue(owner, out var body) ? body.shapes.Count : 0;

    public string GetOwnerShapeRole(int owner, int shapeIndex)
    {
        return bodies.TryGetValue(owner, out var body) && shapeIndex >= 0 && shapeIndex < body.shapes.Count
            ? body.shapes[shapeIndex].role
            : null;
    }

    public object CaptureBindingState()
    {
        RefreshBindings();
        var snapshot = new BindingStateSnapshot
        {
            activeRingRelationIndices = new int[activeRingRelations.Count],
            passiveRingRelationIndices = new int[passiveRingRelations.Count],
            activeGuideRelationKeys = new string[activeGuideRelations.Count],
            activeRingContactOwners = new int[activeRingContacts.Count * 2],
            activeRingContactColliderIds = new long[activeRingContacts.Count * 2]
        };
        activeRingRelations.CopyTo(snapshot.activeRingRelationIndices);
        passiveRingRelations.CopyTo(snapshot.passiveRingRelationIndices);
        activeGuideRelations.CopyTo(snapshot.activeGuideRelationKeys);
        int contactIndex = 0;
        foreach (var contact in activeRingContacts)
        {
            snapshot.activeRingContactOwners[contactIndex * 2] = contact.source;
            snapshot.activeRingContactOwners[contactIndex * 2 + 1] = contact.target;
            snapshot.activeRingContactColliderIds[contactIndex * 2] = contact.sourceColliderId;
            snapshot.activeRingContactColliderIds[contactIndex * 2 + 1] = contact.targetColliderId;
            contactIndex++;
        }
        return snapshot;
    }

    public void RestoreBindingState(object value)
    {
        var snapshot = value as BindingStateSnapshot;
        if (snapshot == null) throw new ArgumentException("Binding snapshot was not captured by this collision world.", nameof(value));
        activeRingRelations.Clear();
        passiveRingRelations.Clear();
        activeRingContacts.Clear();
        activeGuideRelations.Clear();
        if (snapshot.activeRingRelationIndices != null)
            foreach (int index in snapshot.activeRingRelationIndices)
                if (ringRelations.Exists(r => r.index == index)) activeRingRelations.Add(index);
        if (snapshot.passiveRingRelationIndices != null)
            foreach (int index in snapshot.passiveRingRelationIndices)
                if (ringRelations.Exists(r => r.index == index)) passiveRingRelations.Add(index);
        if (snapshot.activeRingContactOwners != null && snapshot.activeRingContactColliderIds != null)
        {
            int pairs = Math.Min(snapshot.activeRingContactOwners.Length, snapshot.activeRingContactColliderIds.Length) / 2;
            for (int i = 0; i < pairs; i++)
            {
                int source = snapshot.activeRingContactOwners[i * 2];
                int target = snapshot.activeRingContactOwners[i * 2 + 1];
                long sourceColliderId = snapshot.activeRingContactColliderIds[i * 2];
                long targetColliderId = snapshot.activeRingContactColliderIds[i * 2 + 1];
                bool known = ringRelations.Exists(r => r.source == source && r.target == target &&
                    Contains(r.sourceShapes, FindShapeIndex(source, sourceColliderId)) &&
                    Contains(r.targetShapes, FindShapeIndex(target, targetColliderId)));
                if (known) activeRingContacts.Add(new AuthoredContactKey(source, sourceColliderId, target, targetColliderId));
            }
        }
        if (snapshot.activeGuideRelationKeys != null)
            foreach (string key in snapshot.activeGuideRelationKeys)
                if (guideRelations.Exists(r => GuideKey(r) == key)) activeGuideRelations.Add(key);
        bindingsDirty = false;
        bindingsInitialized = true;
    }

    int FindShapeIndex(int owner, long sourceColliderId)
    {
        if (!bodies.TryGetValue(owner, out var body)) return -1;
        foreach (var shape in body.shapes) if (shape.sourceColliderId == sourceColliderId) return shape.index;
        return -1;
    }

    void ResolveReleaseOverlap(Body body)
    {
        Vector2 direction = -body.lastMoveDirection.normalized;
        float originalSlide = body.slide;
        var castFilter = new ContactFilter2D();
        castFilter.NoFilter();
        castFilter.SetLayerMask(128);
        castFilter.useTriggers = false;
        for (int step = 0; step < 20; step++)
        {
            float distance = step == 0 ? .02f : .05f * step + .01f;
            bool blocked = false;
            foreach (var shape in body.shapes)
            {
                if (shape.trigger || shape.collider == null || !shape.collider.enabled) continue;
                int hitCount = shape.collider.Cast(direction, castFilter, castHits, distance);
                for (int i = 0; i < hitCount; i++)
                {
                    var hit = castHits[i].collider;
                    if (!shapeLookup.TryGetValue(hit, out var other) || other.owner == body.part.id || other.trigger || bodies[other.owner].removed) continue;
                    if (ShapePairAllowed(shape, other, body.part.id)) continue;
                    blocked = true;
                    break;
                }
                if (blocked) break;
            }
            if (blocked) continue;
            body.slide = originalSlide + Vector2.Dot(direction, body.axis) * Mathf.Max(0f, distance - .01f);
            SetPose(body);
            Physics2D.SyncTransforms();
            return;
        }
        body.slide = originalSlide;
        SetPose(body);
        Physics2D.SyncTransforms();
    }

    void RollbackAfterStop(Body body)
    {
        if (Mathf.Approximately(body.persistedBlockMotionSign, 0f) || body.axis.sqrMagnitude <= 1e-12f) return;
        Vector2 reverse = -body.axis.normalized * body.persistedBlockMotionSign;
        float travel = Mathf.Abs(body.slide - body.gestureStartSlide);
        if (travel <= 1e-6f) return;
        var castFilter = new ContactFilter2D();
        castFilter.NoFilter();
        castFilter.SetLayerMask(128);
        castFilter.useTriggers = false;
        float nearest = 0f;
        foreach (var shape in body.shapes)
        {
            if (shape.trigger || shape.collider == null || !shape.collider.enabled) continue;
            int hitCount = shape.collider.Cast(reverse, castFilter, castHits, .06f);
            for (int i = 0; i < hitCount; i++)
            {
                var hit = castHits[i];
                if (!shapeLookup.TryGetValue(hit.collider, out var other) || other.owner == body.part.id || other.trigger || bodies[other.owner].removed || ShapePairAllowed(shape, other, body.part.id)) continue;
                if (hit.distance < nearest) nearest = hit.distance;
            }
        }
        float rollback = Mathf.Min(travel, Mathf.Max(0f, .05f - nearest));
        if (rollback <= 0f) return;
        Vector2 oldMoveDirection = body.lastMoveDirection;
        body.slide += Vector2.Dot(reverse * rollback, body.axis);
        SetPose(body);
        body.lastMoveDirection = oldMoveDirection;
        Physics2D.SyncTransforms();
    }

    int Query(Collider2D collider)
    {
        if (collider == null || !collider.enabled) return 0;
        int count = collider.Overlap(allFilter, overlaps);
        if (count == overlaps.Length)
        {
            overlaps = new Collider2D[overlaps.Length * 2];
            return Query(collider);
        }
        return count;
    }

    bool Penetrates(Collider2D a, Collider2D b)
    {
        if (!BoundsMayContact(a, b)) return false;
        var distance = a.Distance(b);
        return distance.isOverlapped && distance.distance < -PenetrationTolerance;
    }

    bool Contacts(Collider2D a, Collider2D b)
    {
        if (!BoundsMayContact(a, b)) return false;
        var distance = a.Distance(b);
        return distance.isOverlapped;
    }

    // Conservative XY broad phase only. The native Distance/Overlap query still
    // decides every potentially touching pair, including the .03 stick margin.
    // Z is deliberately omitted here; Physics2D distance itself works in XY.
    static bool BoundsMayContact(Collider2D a, Collider2D b, float margin = 0f)
    {
        Bounds aa = a.bounds, bb = b.bounds;
        float padding = Mathf.Max(0f, margin) + .0001f;
        return aa.min.x <= bb.max.x + padding && bb.min.x <= aa.max.x + padding &&
               aa.min.y <= bb.max.y + padding && bb.min.y <= aa.max.y + padding;
    }

    bool ShapePairAllowed(Shape a, Shape b, int movingOwner)
    {
        foreach (var relation in ringRelations)
        {
            if (relation.source == a.owner && relation.target == b.owner &&
                Contains(relation.sourceShapes, a.index) && Contains(relation.targetShapes, b.index) && IsRingBindingPair(a, b)) return true;
            if (relation.source == b.owner && relation.target == a.owner &&
                Contains(relation.sourceShapes, b.index) && Contains(relation.targetShapes, a.index) && IsRingBindingPair(a, b)) return true;
            // RingPassiveBlockCheck.CanIgnorePartnerWallInAngle: the receiving
            // ring may pass its configured partner's wall while that partner is
            // within 5 degrees of its authored starting angle. The exception is
            // directional and never applies to the hard-block pin stem.
            if (movingOwner == relation.target && bodies[relation.target].part.kind == "ring")
            {
                Shape wall = relation.source == a.owner && relation.target == b.owner ? a :
                    relation.source == b.owner && relation.target == a.owner ? b : null;
                Shape receiver = wall == a ? b : a;
                if (wall != null && wall.role == "head_wall" && receiver.role == "body" &&
                    Mathf.Abs(Mathf.DeltaAngle(bodies[relation.source].part.angle, bodies[relation.source].angle)) <= 5f)
                    return true;
            }
        }
        foreach (var relation in guideRelations)
        {
            if (relation.lockedOwner == a.owner && relation.guideOwner == b.owner &&
                Contains(relation.lockedShapes, a.index) && Contains(relation.guideShapes, b.index)) return true;
            if (relation.lockedOwner == b.owner && relation.guideOwner == a.owner &&
                Contains(relation.lockedShapes, b.index) && Contains(relation.guideShapes, a.index)) return true;
        }
        return false;
    }

    static bool IsRingBindingPair(Shape a, Shape b)
    {
        return (a.role == "head_trigger" && b.role == "body") ||
               (b.role == "head_trigger" && a.role == "body");
    }

    static bool Contains(int[] values, int value)
    {
        if (values == null) return false;
        for (int i = 0; i < values.Length; i++) if (values[i] == value) return true;
        return false;
    }

    static void BoundsFromBoxPoints(Vector2[] points, out Vector2 center, out Vector2 size, out float localAngle)
    {
        if (points == null || points.Length < 4) throw new InvalidOperationException("A source BoxCollider2D requires its exact four-point outline.");
        center = Vector2.zero;
        for (int i = 0; i < points.Length; i++) center += points[i];
        center /= points.Length;
        Vector2 xAxis = points[1] - points[0];
        Vector2 yAxis = points[2] - points[1];
        size = new Vector2(xAxis.magnitude, yAxis.magnitude);
        localAngle = Mathf.Atan2(xAxis.y, xAxis.x) * Mathf.Rad2Deg;
    }

    Dictionary<ContactKey, float> CollectHardContacts(Body body)
    {
        var contacts = new Dictionary<ContactKey, float>();
        foreach (var shape in body.shapes)
        {
            if (shape.collider == null || !shape.collider.enabled) continue;
            if (shape.trigger) continue;
            int count = Query(shape.collider);
            for (int i = 0; i < count; i++)
            {
                var hit = overlaps[i];
                if (!shapeLookup.TryGetValue(hit, out var other) || other.owner == shape.owner || other.trigger || bodies[other.owner].removed) continue;
                var distance = shape.collider.Distance(hit);
                if (!distance.isOverlapped || distance.distance >= -PenetrationTolerance || ShapePairAllowed(shape, other, body.part.id)) continue;
                contacts[ShapePair(shape.owner, shape.index, other.owner, other.index)] = -distance.distance;
            }
        }
        return contacts;
    }

    Dictionary<ContactKey, float> CollectStickContacts(Body body)
    {
        var contacts = new Dictionary<ContactKey, float>();
        if ((body.part.kind ?? string.Empty) != "ring") return contacts;
        foreach (var shape in body.shapes)
        {
            if (shape.role != "stem" || shape.collider == null || !shape.collider.enabled) continue;
            foreach (var pair in shapeLookup)
            {
                var other = pair.Value;
                if (other.owner == shape.owner || other.trigger || bodies[other.owner].removed ||
                    (bodies[other.owner].part.kind ?? string.Empty) == "ring" || ShapePairAllowed(shape, other, body.part.id)) continue;
                if (!BoundsMayContact(shape.collider, pair.Key, .03f)) continue;
                var distance = shape.collider.Distance(pair.Key);
                if (!distance.isValid) continue;
                float penetration = Mathf.Max(0f, .03f - distance.distance);
                if (penetration > 0f) contacts[ShapePair(shape.owner, shape.index, other.owner, other.index)] = penetration;
            }
        }
        return contacts;
    }

    bool IsBaselineContactAllowed(Body owner, Shape a, Shape b, float penetration)
    {
        if (!owner.inGesture) return false;
        ContactKey key = a.owner == owner.part.id
            ? ShapePair(a.owner, a.index, b.owner, b.index)
            : ShapePair(b.owner, b.index, a.owner, a.index);
        if (!owner.gestureBaseline.TryGetValue(key, out float baseline)) return false;
        bool strictInterlock = (owner.part.kind ?? string.Empty) == "ring" &&
            ((a.role == "stem" && (bodies[b.owner].part.kind ?? string.Empty) == "ring") ||
             (b.role == "stem" && (bodies[a.owner].part.kind ?? string.Empty) == "ring"));
        float allowedGrowth = strictInterlock ? PenetrationTolerance : .02f;
        return penetration <= baseline + allowedGrowth;
    }

    public bool HasHardOverlap(int owner)
    {
        if (!bodies.TryGetValue(owner, out var body) || body.removed) return false;
        if (body.part.kind == "ring") return HasNativeRotationBlock(body);
        foreach (var shape in body.shapes)
        {
            if (shape.trigger || shape.collider == null || !shape.collider.enabled) continue;
            int count = Query(shape.collider);
            for (int i = 0; i < count; i++)
            {
                var hit = overlaps[i];
                if (!shapeLookup.TryGetValue(hit, out var other) || other.owner == owner || other.trigger || bodies[other.owner].removed) continue;
                var distance = shape.collider.Distance(hit);
                if (!distance.isOverlapped || distance.distance >= -PenetrationTolerance) continue;
                if (ShapePairAllowed(shape, other, owner)) continue;
                if (IsBaselineContactAllowed(body, shape, other, -distance.distance)) continue;
                LastBlockerOwner = other.owner;
                LastBlockerShapeIndex = other.index;
                LastBlockerRole = other.role;
                return true;
            }
        }
        return false;
    }

    public bool InitialPlacementValid()
    {
        Physics2D.SyncTransforms();
        foreach (var id in bodies.Keys)
        {
            var body = bodies[id];
            if (body.removed) continue;
            foreach (var contact in CollectHardContacts(body))
                if (contact.Value > PenetrationTolerance) return false;
        }
        return true;
    }

    public float ClampRotation(int owner, float from, float delta, out bool blocked)
    {
        blocked = false;
        LastBlockerOwner = -1; LastBlockerShapeIndex = -1; LastBlockerRole = null;
        if (!bodies.TryGetValue(owner, out var body) || body.removed) return from;
        float accepted = from;
        int steps = Mathf.Max(1, Mathf.CeilToInt(Mathf.Abs(delta) / MaxAngularStep));
        for (int step = 1; step <= steps; step++)
        {
            float candidate = from + delta * step / steps;
            body.angle = candidate; SetPose(body); Physics2D.SyncTransforms();
            if (HasHardOverlap(owner))
            {
                blocked = true;
                float low = accepted, high = candidate;
                for (int n = 0; n < 10 && Mathf.Abs(high - low) > .02f; n++)
                {
                    float mid = (low + high) * .5f;
                    body.angle = mid; SetPose(body); Physics2D.SyncTransforms();
                    if (HasHardOverlap(owner)) high = mid; else low = mid;
                }
                accepted = low; break;
            }
            accepted = candidate;
            RefreshBindings();
            EmitMotionSample(body);
            if (body.removed) { blocked = true; break; }
        }
        body.angle = accepted; SetPose(body); Physics2D.SyncTransforms(); RefreshBindings();
        if (blocked) EmitMotionSample(body);
        return accepted;
    }

    public float ClampSlide(int owner, float from, float to, out bool blocked)
    {
        return ClampSlideForDeltaTime(owner, from, to, Time.deltaTime, out blocked);
    }

    public float ClampSlideForDeltaTime(int owner, float from, float to, float deltaTime, out bool blocked)
    {
        blocked = false;
        LastBlockerOwner = -1; LastBlockerShapeIndex = -1; LastBlockerRole = null;
        if (!bodies.TryGetValue(owner, out var body) || body.removed) return from;
        float wanted = to - from;
        if (Mathf.Abs(wanted) <= 1e-6f) return from;
        float frameBudget = .36f * deltaTime * 60f;
        float remaining = Mathf.Min(Mathf.Abs(wanted), frameBudget);
        float sign = Mathf.Sign(wanted);
        Vector2 direction = body.axis * sign;
        float movedTotal = 0f;
        var castFilter = new ContactFilter2D();
        castFilter.NoFilter();
        castFilter.SetLayerMask(128);
        castFilter.useTriggers = false;
        while (remaining > 1e-5f)
        {
            float stepDistance = Mathf.Min(MaxLinearStep, remaining);
            float nearest = float.PositiveInfinity;
            Collider2D nearestCollider = null;
            Collider2D nearestOwnerShape = null;
            bool hadRawCastHit = false;
            foreach (var shape in body.shapes)
            {
                if (shape.trigger || shape.collider == null || !shape.collider.enabled) continue;
                int hitCount = shape.collider.Cast(direction, castFilter, castHits, stepDistance);
                if (hitCount > 0) hadRawCastHit = true;
                for (int i = 0; i < hitCount; i++)
                {
                    var castHit = castHits[i];
                    var hit = castHit.collider;
                    if (!shapeLookup.TryGetValue(hit, out var other) || other.owner == owner || other.trigger || bodies[other.owner].removed || ShapePairAllowed(shape, other, owner)) continue;
                    if (castHit.distance < nearest)
                    {
                        nearest = castHit.distance;
                        nearestCollider = hit;
                        nearestOwnerShape = shape.collider;
                        LastBlockerOwner = other.owner;
                        LastBlockerShapeIndex = other.index;
                        LastBlockerRole = other.role;
                    }
                }
            }
            float moved;
            if (nearestCollider != null && nearest <= .01f)
            {
                var distance = nearestOwnerShape == null ? (ColliderDistance2D?)null : nearestOwnerShape.Distance(nearestCollider);
                if (distance.HasValue && distance.Value.distance < 0f && Vector2.Dot(direction, distance.Value.normal) < 0f)
                {
                    float escape = Mathf.Min(stepDistance, Mathf.Abs(distance.Value.distance) + .01f);
                    body.slide += sign * escape;
                    movedTotal += escape;
                    SetPose(body); Physics2D.SyncTransforms();
                    RefreshBindings(); EmitMotionSample(body);
                    break;
                }
            }
            if (nearestCollider != null && nearest + .01f < stepDistance)
            {
                moved = Mathf.Max(0f, nearest - .01f);
                blocked = true;
                body.blockMotionSign = (int)sign;
                body.persistedBlockMotionSign = (int)sign;
            }
            else
            {
                if (body.blockMotionSign != 0 && body.blockMotionSign * sign < 0f) body.blockMotionSign = 0;
                moved = hadRawCastHit ? Mathf.Max(0f, stepDistance - .01f) : stepDistance;
            }
            if (moved > 1e-6f)
            {
                body.slide += sign * moved;
                movedTotal += moved;
                SetPose(body); Physics2D.SyncTransforms();
                RefreshBindings(); EmitMotionSample(body);
                if (body.removed) { blocked = true; break; }
            }
            remaining -= moved;
            if (blocked || moved <= 1e-6f) break;
        }
        // Native MoveInSteps stops when a skin-trimmed tail cannot advance.
        // That is not a blocking contact: NotifyPhysicsDragBlocked is emitted
        // only for an allowed cast hit strictly inside the requested step.
        // Preserve the raw-hit skin above, but let the next pointer frame move.
        RefreshBindings();
        return from + sign * movedTotal;
    }

    void RefreshBindings()
    {
        if (!bindingsDirty) return;
        bindingsDirty = false;
        var previousActive = new HashSet<int>(activeRingRelations);
        var previousPassive = new HashSet<int>(passiveRingRelations);
        var previousContacts = new HashSet<AuthoredContactKey>(activeRingContacts);
        activeRingRelations.Clear();
        passiveRingRelations.Clear();
        activeRingContacts.Clear();
        activeGuideRelations.Clear();
        foreach (var relation in ringRelations)
        {
            if (!bodies.TryGetValue(relation.source, out var source) || !bodies.TryGetValue(relation.target, out var target) || source.removed || target.removed) continue;
            bool directContact = false;
            bool previousDirectContact = false;
            foreach (int sourceIndex in relation.sourceShapes)
            {
                var a = source.shapes[sourceIndex];
                foreach (int targetIndex in relation.targetShapes)
                {
                    var b = target.shapes[targetIndex];
                    if (a.role != "head_trigger" || b.role != "body" || !a.collider.enabled || !b.collider.enabled) continue;
                    var key = new AuthoredContactKey(relation.source, a.sourceColliderId, relation.target, b.sourceColliderId);
                    if (previousContacts.Contains(key)) previousDirectContact = true;
                    if (Contacts(a.collider, b.collider))
                    {
                        activeRingContacts.Add(key);
                        directContact = true;
                    }
                }
            }
            // Native active locks are queried from the pin owner's handlers;
            // passive locks are queried independently from the receiving ring.
            // A remaining hard-pin contact must not make these masks identical.
            if (directContact || ((!bindingsInitialized || previousActive.Contains(relation.index)) && RingsStillConnected(source, target)))
                activeRingRelations.Add(relation.index);
            // PullPassiveLockDomain removes a registered collider on its exit
            // even if another collider under that ring still touches the Pull.
            // Its owner-wide query only seeds/prunes bindings which have not
            // just received that direct exit. Ring passive locks differ here.
            bool passiveExit = target.part.kind == "slider" && previousDirectContact && !directContact;
            if (directContact || (!passiveExit && (!bindingsInitialized || previousPassive.Contains(relation.index)) && RingsStillConnected(target, source)))
                passiveRingRelations.Add(relation.index);
        }
        foreach (var relation in guideRelations)
        {
            if (!bodies.TryGetValue(relation.guideOwner, out var guide) || !bodies.TryGetValue(relation.lockedOwner, out var locked) || guide.removed || locked.removed) continue;
            bool connected = false;
            EnsureNativeQueries(locked, false);
            foreach (var probe in locked.overlapProbes)
            {
                foreach (var hit in probe.hits)
                {
                    // Match the selected collider as well as the root: the two
                    // CatchGuide slots of a butterfly share a gameplay owner.
                    if (hit.owner == relation.guideOwner && Contains(relation.guideShapes, hit.index))
                    { connected = true; break; }
                }
                if (connected) break;
            }
            if (connected) activeGuideRelations.Add(GuideKey(relation));
        }
        bindingsInitialized = true;
    }

    bool RingsStillConnected(Body source, Body target)
    {
        string sourceKind = source.part.kind ?? string.Empty;
        string targetKind = target.part.kind ?? string.Empty;
        if (sourceKind == "slider") return NativeRingQuerySees(source, target.part.id, false);
        if (sourceKind != "ring") return false;
        // Native non-ring active lockers are checked through RingRelative
        // handlers only, while ring partners also use the main body handler.
        return NativeRingQuerySees(source, target.part.id, targetKind != "ring");
    }

    bool NativeRingQuerySees(Body source, int targetOwner, bool relativeOnly)
    {
        EnsureNativeQueries(source, false);
        return (relativeOnly ? source.queriedRelativeOwners : source.queriedOwners).Contains(targetOwner);
    }

    void CollectProbeHits(Shape probe, ContactFilter2D filter, List<Shape> output)
    {
        if (probe.collider == null || !probe.collider.enabled) return;
        int count;
        while (true)
        {
            count = probe.collider.Overlap(filter, overlaps);
            if (count < overlaps.Length) break;
            overlaps = new Collider2D[overlaps.Length * 2];
        }
        for (int i = 0; i < count; i++)
            if (shapeLookup.TryGetValue(overlaps[i], out var other) && other.owner != probe.owner &&
                overlaps[i].gameObject.layer != 6 && !bodies[other.owner].removed && !output.Contains(other))
                output.Add(other);
    }

    void EnsureNativeQueries(Body source, bool includeWalls)
    {
        if (source.queryPoseEpoch != poseEpoch)
        {
        source.queriedOwners.Clear();
        source.queriedRelativeOwners.Clear();
        source.externalHits.Clear();
        source.interlockBodyHits.Clear();
        foreach (var probe in source.overlapProbes)
        {
            probe.hits.Clear();
            CollectProbeHits(probe.shape, probe.filter, probe.hits);
            foreach (var other in probe.hits)
            {
                source.queriedOwners.Add(other.owner);
                if (probe.relative) source.queriedRelativeOwners.Add(other.owner);
                CacheObservedShape(source, other, true);
            }
        }
        source.queryPoseEpoch = poseEpoch;
        }
        if (!includeWalls || source.wallQueryPoseEpoch == poseEpoch) return;
        foreach (var probe in source.overlapProbes)
        {
            probe.wallHits.Clear();
            if (!probe.relative) continue;
            foreach (int index in probe.wallShapes)
                CollectProbeHits(source.shapes[index], relativeWallFilter, probe.wallHits);
            foreach (var other in probe.wallHits) CacheObservedShape(source, other, false);
        }
        source.wallQueryPoseEpoch = poseEpoch;
    }

    bool IsExternalObstacle(Shape shape)
    {
        string kind = bodies[shape.owner].part.kind;
        return shape.role != "guide_trigger" && (kind == "slider" || kind == "fixed" || kind == "butterfly");
    }

    void CacheObservedShape(Body source, Shape other, bool direct)
    {
        if (direct && IsExternalObstacle(other) && !source.externalHits.Contains(other)) source.externalHits.Add(other);
        if (bodies[other.owner].part.kind == "ring" && !other.relativeMarker && !source.interlockBodyHits.Contains(other))
            source.interlockBodyHits.Add(other);
    }

    float MaxProbePenetration(Body body, List<Shape> candidates, out Shape blocker)
    {
        float maximum = 0f; blocker = null;
        foreach (var probe in body.shapes)
        {
            if (probe.trigger || !probe.collider.enabled) continue;
            foreach (var other in candidates)
            {
                if (!other.collider.enabled || !BoundsMayContact(probe.collider, other.collider)) continue;
                var distance = probe.collider.Distance(other.collider);
                if (distance.isValid && -distance.distance > maximum)
                { maximum = -distance.distance; blocker = other; }
            }
        }
        return maximum;
    }

    float NativeStickPenetration(Body body, out Shape blocker)
    {
        float maximum = 0f; blocker = null;
        foreach (var pin in body.shapes)
        {
            if (pin.role != "stem" || !pin.collider.enabled) continue;
            foreach (var other in body.externalHits)
                MeasureNativeStick(pin, other, ref maximum, ref blocker);
            // Native also queries each hard pin without a layer/trigger filter.
            int count = Query(pin.collider);
            for (int i = 0; i < count; i++)
                if (shapeLookup.TryGetValue(overlaps[i], out var other) && other.owner != body.part.id &&
                    !bodies[other.owner].removed && IsExternalObstacle(other))
                    MeasureNativeStick(pin, other, ref maximum, ref blocker);
        }
        return maximum;
    }

    void MeasureNativeStick(Shape pin, Shape other, ref float maximum, ref Shape blocker)
    {
        if (!other.collider.enabled || !BoundsMayContact(pin.collider, other.collider, .03f)) return;
        var distance = pin.collider.Distance(other.collider);
        if (!distance.isValid || distance.distance >= .03f) return;
        float value = .03f - distance.distance;
        if (value > maximum) { maximum = value; blocker = other; }
    }

    void CaptureNativeMotionBaseline(Body body)
    {
        EnsureNativeQueries(body, true);
        body.gestureExternalColliders.Clear();
        foreach (var other in body.externalHits) body.gestureExternalColliders.Add(other.collider);
        body.gestureExternalPenetration = MaxProbePenetration(body, body.externalHits, out _);
        body.gestureInterlockPenetration = MaxProbePenetration(body, body.interlockBodyHits, out _);
        body.gestureStickPenetration = NativeStickPenetration(body, out _);
    }

    bool ReportBlocker(Shape shape)
    {
        if (shape == null) return false;
        LastBlockerOwner = shape.owner; LastBlockerShapeIndex = shape.index; LastBlockerRole = shape.role;
        return true;
    }

    bool IgnoreNativeInterlock(Body owner, OverlapProbe probe, Shape other)
    {
        var role = other.role == "head_trigger" ? RotateRingsNativeInterlockPolicy.OtherRole.RingRelativeBody :
            other.role == "head_wall" ? RotateRingsNativeInterlockPolicy.OtherRole.RingRelativeWall :
            other.relativeMarker ? RotateRingsNativeInterlockPolicy.OtherRole.OtherRelative : RotateRingsNativeInterlockPolicy.OtherRole.MainBody;
        bool whitelist = !other.relativeMarker || (other.relativePhysicalBlock && (other.role == "head_trigger" || other.role == "head_wall"));
        return RotateRingsNativeInterlockPolicy.ShouldIgnoreBlock(new RotateRingsNativeInterlockPolicy.Facts(
            probe.relative ? RotateRingsNativeInterlockPolicy.QuerySource.RingRelative : RotateRingsNativeInterlockPolicy.QuerySource.MainBody,
            owner.hasRelatives, other.owner != owner.part.id, other.relativeMarker, role, whitelist,
            owner.staticBoundColliders.Count > 0, owner.staticBoundColliders.Contains(other), owner.staticBoundPartners.Contains(other.owner),
            !bodies[other.owner].removed, Mathf.DeltaAngle(bodies[other.owner].part.angle, bodies[other.owner].angle)));
    }

    bool HasNativeRotationBlock(Body body)
    {
        EnsureNativeQueries(body, true);
        if (body.inGesture)
        {
            foreach (var other in body.externalHits)
                if (!body.gestureExternalColliders.Contains(other.collider)) return ReportBlocker(other);
            if (MaxProbePenetration(body, body.externalHits, out var external) > body.gestureExternalPenetration + .02f)
                return ReportBlocker(external);
        }
        // This native unconditional hard-pin path includes ring-owned triggers.
        foreach (var pin in body.shapes)
        {
            if (pin.role != "stem" || !pin.collider.enabled) continue;
            foreach (var pair in shapeLookup)
            {
                var other = pair.Value;
                if (other.owner == body.part.id || !pair.Key.enabled || bodies[other.owner].removed ||
                    bodies[other.owner].part.kind != "ring" || !BoundsMayContact(pin.collider, pair.Key)) continue;
                var distance = pin.collider.Distance(pair.Key);
                if (distance.isValid && distance.distance < 0f) return ReportBlocker(other);
            }
        }
        float stickBaseline = body.inGesture ? body.gestureStickPenetration : 0f;
        if (NativeStickPenetration(body, out var stick) > stickBaseline + .02f) return ReportBlocker(stick);
        float interlockBaseline = body.inGesture ? body.gestureInterlockPenetration : 0f;
        if (MaxProbePenetration(body, body.interlockBodyHits, out var interlock) > interlockBaseline + .02f)
            return ReportBlocker(interlock);
        foreach (var probe in body.overlapProbes)
        {
            foreach (var other in probe.hits)
                if (bodies[other.owner].part.kind == "ring" && !IgnoreNativeInterlock(body, probe, other)) return ReportBlocker(other);
            foreach (var other in probe.wallHits)
                if (bodies[other.owner].part.kind == "ring" && !IgnoreNativeInterlock(body, probe, other)) return ReportBlocker(other);
        }
        // Carried-bomb contact/explosion is processed by MotionSample at the
        // swept candidate pose; removed owners immediately terminate the drag.
        return false;
    }

    public bool IsBinding(int source, int target)
    {
        RefreshBindings();
        foreach (var relation in ringRelations)
            if (relation.source == source && relation.target == target &&
                (activeRingRelations.Contains(relation.index) || passiveRingRelations.Contains(relation.index))) return true;
        return false;
    }

    public bool HasOutgoingBinding(int owner)
    {
        RefreshBindings();
        foreach (var relation in ringRelations)
            // RingLockActiveService.IsLockedBy reads the configured lockedByMap,
            // not the separately pruned activeLockedByMap used by the solve gate.
            if (relation.source == owner && !bodies[relation.target].removed &&
                NativeRingQuerySees(bodies[owner], relation.target, true)) return true;
        return false;
    }

    public bool HasIncidentBinding(int owner)
    {
        RefreshBindings();
        foreach (var relation in ringRelations)
            if ((relation.source == owner && activeRingRelations.Contains(relation.index)) ||
                (relation.target == owner && passiveRingRelations.Contains(relation.index))) return true;
        foreach (var relation in guideRelations)
            if ((relation.guideOwner == owner || relation.lockedOwner == owner) && activeGuideRelations.Contains(GuideKey(relation))) return true;
        return false;
    }

    public bool CanGuideSolve(int guideOwnerId)
    {
        RefreshBindings();
        bool found = false;
        foreach (var relation in guideRelations)
        {
            if (relation.guideOwner != guideOwnerId) continue;
            found = true;
            if (!bodies.TryGetValue(relation.lockedOwner, out var locked) || locked.removed) continue;
            if (activeGuideRelations.Contains(GuideKey(relation))) return false;
        }
        return found;
    }

    public bool CanSolve(int id)
    {
        if (!bodies.TryGetValue(id, out var body) || body.removed) return false;
        RefreshBindings();
        string kind = body.part.kind ?? string.Empty;
        if (kind == "slider")
        {
            foreach (var relation in ringRelations)
                if (relation.target == id && passiveRingRelations.Contains(relation.index)) return false;
            foreach (var relation in guideRelations)
                if (relation.lockedOwner == id && activeGuideRelations.Contains(GuideKey(relation))) return false;
            return true;
        }
        if (kind == "butterfly") return CanGuideSolve(id);
        if (kind == "fixed")
        {
            bool hasGuide = false;
            foreach (var relation in guideRelations) if (relation.guideOwner == id) { hasGuide = true; break; }
            return hasGuide && CanGuideSolve(id);
        }
        if (kind == "ring") return !HasIncidentBinding(id);
        return !HasIncidentBinding(id);
    }

    public List<RotateRingsAuthoredContactState> GetActiveAuthoredContacts()
    {
        RefreshBindings();
        var result = new List<RotateRingsAuthoredContactState>();
        foreach (var relation in ringRelations)
        {
            if (!activeRingRelations.Contains(relation.index) && !passiveRingRelations.Contains(relation.index)) continue;
            var source = bodies[relation.source]; var target = bodies[relation.target];
            foreach (int si in relation.sourceShapes)
            {
                var a = source.shapes[si];
                foreach (int ti in relation.targetShapes)
                {
                    var b = target.shapes[ti];
                    if (IsRingBindingPair(a, b) && Contacts(a.collider, b.collider)) result.Add(new RotateRingsAuthoredContactState
                    {
                        kind = "ring-link", ownerId = relation.source, targetId = relation.target,
                        ownerShapeIndex = si, targetShapeIndex = ti, ownerRole = a.role,
                        targetRole = b.role, featureId = relation.feature
                    });
                }
            }
        }
        foreach (var relation in guideRelations)
        {
            if (!activeGuideRelations.Contains(GuideKey(relation))) continue;
            var guide = bodies[relation.guideOwner]; var locked = bodies[relation.lockedOwner];
            foreach (int li in relation.lockedShapes)
            {
                var a = locked.shapes[li];
                foreach (int gi in relation.guideShapes)
                {
                    var b = guide.shapes[gi];
                    if (Contacts(a.collider, b.collider)) result.Add(new RotateRingsAuthoredContactState
                    {
                        kind = "guide-link", ownerId = relation.lockedOwner, targetId = relation.guideOwner,
                        ownerShapeIndex = li, targetShapeIndex = gi, ownerRole = a.role,
                        targetRole = b.role, featureId = relation.guideId
                    });
                }
            }
        }
        return result;
    }

    public string GetActiveContactSummary()
    {
        var contacts = GetActiveAuthoredContacts();
        if (contacts.Count == 0) return "no active authored contacts";
        var summary = new System.Text.StringBuilder();
        for (int i = 0; i < contacts.Count; i++)
        {
            var c = contacts[i];
            if (i > 0) summary.Append("; ");
            summary.Append(c.kind).Append(' ').Append(c.ownerId).Append('[').Append(c.ownerShapeIndex).Append(':').Append(c.ownerRole)
                .Append("] -> ").Append(c.targetId).Append('[').Append(c.targetShapeIndex).Append(':').Append(c.targetRole).Append(']')
                .Append(" feature=").Append(c.featureId);
        }
        return summary.ToString();
    }

    public bool CanTurnAndRelease(int owner, float target)
    {
        if (!bodies.TryGetValue(owner, out var body)) return false;
        return CanTurnAndReleaseDelta(owner, Mathf.DeltaAngle(body.angle, target));
    }

    public bool CanTurnAndReleaseDelta(int owner, float delta)
    {
        if (!bodies.TryGetValue(owner, out var body)) return false;
        float original = body.angle;
        bool wasGesture = body.inGesture;
        Vector2 oldMoveDirection = body.lastMoveDirection;
        var oldBaseline = new Dictionary<ContactKey, float>(body.gestureBaseline);
        var oldStickBaseline = new Dictionary<ContactKey, float>(body.gestureStickBaseline);
        var oldExternal = new HashSet<Collider2D>(body.gestureExternalColliders);
        float oldExternalPenetration = body.gestureExternalPenetration;
        float oldInterlockPenetration = body.gestureInterlockPenetration;
        float oldStickPenetration = body.gestureStickPenetration;
        float oldGestureStartAngle = body.gestureStartAngle;
        object bindings = CaptureBindingState();
        bool oldSuppression = suppressMotionSample;
        suppressMotionSample = true;
        BeginGesture(owner);
        bool blocked;
        float reached = ClampRotation(owner, original, delta, out blocked);
        bool result = !blocked && Mathf.Abs(reached - original - delta) < .05f && CanSolve(owner);
        body.angle = original;
        body.inGesture = wasGesture;
        body.lastMoveDirection = oldMoveDirection;
        body.gestureBaseline.Clear(); foreach (var entry in oldBaseline) body.gestureBaseline.Add(entry.Key, entry.Value);
        body.gestureStickBaseline.Clear(); foreach (var entry in oldStickBaseline) body.gestureStickBaseline.Add(entry.Key, entry.Value);
        body.gestureExternalColliders.Clear(); foreach (var collider in oldExternal) body.gestureExternalColliders.Add(collider);
        body.gestureExternalPenetration = oldExternalPenetration;
        body.gestureInterlockPenetration = oldInterlockPenetration;
        body.gestureStickPenetration = oldStickPenetration;
        body.gestureStartAngle = oldGestureStartAngle;
        SetPose(body);
        Physics2D.SyncTransforms();
        RestoreBindingState(bindings);
        suppressMotionSample = oldSuppression;
        return result;
    }

    public bool CanSlideAndRelease(int owner, float signedTargetSlide)
    {
        return CanSlideAndReleaseForDeltaTime(owner, signedTargetSlide, Time.deltaTime);
    }

    public bool CanSlideAndReleaseForDeltaTime(int owner, float signedTargetSlide, float deltaTime)
    {
        if (!bodies.TryGetValue(owner, out var body) || body.removed || !RotateRingsRules.IsSlider(body.part)) return false;
        float original = body.slide;
        bool wasGesture = body.inGesture;
        Vector2 oldMoveDirection = body.lastMoveDirection;
        var oldBaseline = new Dictionary<ContactKey, float>(body.gestureBaseline);
        var oldStickBaseline = new Dictionary<ContactKey, float>(body.gestureStickBaseline);
        object bindings = CaptureBindingState();
        bool oldSuppression = suppressMotionSample;
        suppressMotionSample = true;
        body.inGesture = true;
        if (body.gestureBaseline.Count == 0 && body.gestureStickBaseline.Count == 0)
        {
            Physics2D.SyncTransforms();
            foreach (var contact in CollectHardContacts(body)) body.gestureBaseline[contact.Key] = contact.Value;
            foreach (var contact in CollectStickContacts(body)) body.gestureStickBaseline[contact.Key] = contact.Value;
        }
        float reached = original;
        bool blocked = false;
        float frameBudget = .36f * deltaTime * 60f;
        if (frameBudget <= 1e-6f) frameBudget = .36f;
        int frameLimit = Mathf.Max(1, Mathf.CeilToInt(Mathf.Abs(signedTargetSlide - original) / frameBudget) + 2);
        for (int frame = 0; frame < frameLimit && Mathf.Abs(reached - signedTargetSlide) > .001f; frame++)
        {
            float before = reached;
            reached = ClampSlideForDeltaTime(owner, reached, signedTargetSlide, deltaTime, out blocked);
            if (blocked || Mathf.Abs(reached - before) <= 1e-6f) break;
        }
        bool fullyReached = Mathf.Abs(reached - signedTargetSlide) <= .001f;
        if (fullyReached) EndGesture(owner);
        bool result = fullyReached && !blocked && CanSolve(owner);
        body.slide = original;
        body.inGesture = wasGesture;
        body.lastMoveDirection = oldMoveDirection;
        body.gestureBaseline.Clear(); foreach (var entry in oldBaseline) body.gestureBaseline.Add(entry.Key, entry.Value);
        body.gestureStickBaseline.Clear(); foreach (var entry in oldStickBaseline) body.gestureStickBaseline.Add(entry.Key, entry.Value);
        SetPose(body);
        Physics2D.SyncTransforms();
        RestoreBindingState(bindings);
        suppressMotionSample = oldSuppression;
        return result;
    }
}

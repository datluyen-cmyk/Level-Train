using System;
using Spine;
using Spine.Unity;
using UnityEngine;

/// <summary>
/// Runtime port of eff_junei_unlock_suolian using its original Spine skeleton,
/// atlas, directions, sizing, and animation clips.
/// </summary>
public static class OriginalChainVisualAssets
{
    const string ResourceRoot = "Original/ChainVisual/";
    const float ScaleReferenceOuterRadius = 48f;
    const float ChainEdgeInsetDegrees = 15f;

    static Texture2D sourceTexture;
    static Material sourceMaterial;
    static TextAsset sourceSkeleton;
    static TextAsset sourceAtlasText;
    static SpineAtlasAsset sourceAtlasAsset;
    static SkeletonDataAsset sourceSkeletonDataAsset;

    public struct Geometry
    {
        public float outerRadius;
        public float gapAngleDegrees;
        public float localRotationDegrees;
        public bool boardFlipped;

        public Geometry(float outerRadius, float gapAngleDegrees, float localRotationDegrees, bool boardFlipped = false)
        {
            this.outerRadius = outerRadius;
            this.gapAngleDegrees = gapAngleDegrees;
            this.localRotationDegrees = localRotationDegrees;
            this.boardFlipped = boardFlipped;
        }
    }

    enum Direction { Horizontal, RightSlant, LeftSlant, Vertical }

    struct DirectionData
    {
        public float initAngle;
        public float childAngle;
        public Direction second;
        public Direction third;

        public DirectionData(float initAngle, float childAngle, Direction second, Direction third)
        {
            this.initAngle = initAngle;
            this.childAngle = childAngle;
            this.second = second;
            this.third = third;
        }
    }

    static readonly DirectionData[] Directions =
    {
        new DirectionData(180f, 180f, Direction.LeftSlant, Direction.RightSlant),
        new DirectionData(45f, -40f, Direction.Vertical, Direction.Horizontal),
        new DirectionData(135f, -140f, Direction.Horizontal, Direction.Vertical),
        new DirectionData(90f, -90f, Direction.LeftSlant, Direction.RightSlant)
    };

    /// <summary>Creates missing native segments or updates their geometry under the ring owner.</summary>
    public static GameObject CreateOrUpdateChainVisual(Transform ownerTransform, GameObject currentRoot,
        Geometry geometry, int lockCount)
    {
        if (ownerTransform == null) throw new ArgumentNullException(nameof(ownerTransform));
        EnsureSourceAssets();

        GameObject root = currentRoot;
        if (root == null) root = new GameObject("ChainRoot");
        Transform rootTransform = root.transform;
        rootTransform.SetParent(ownerTransform, false);
        rootTransform.localPosition = new Vector3(0f, 0f, -0.5f);
        float rootAngle = -geometry.localRotationDegrees;
        if (geometry.boardFlipped) rootAngle = Mathf.Repeat(rootAngle + 180f, 360f);
        rootTransform.localRotation = Quaternion.Euler(0f, 0f, rootAngle);
        rootTransform.localScale = Vector3.one;
        root.layer = 7;

        int count = Mathf.Clamp(lockCount, 0, 3);
        float ringRotation = geometry.localRotationDegrees;
        Direction primary = ResolveDirection(Mathf.Repeat(ringRotation + 90f, 360f));
        Direction[] selected = { primary, Directions[(int)primary].second, Directions[(int)primary].third };

        float scale = geometry.outerRadius > 0f ? geometry.outerRadius / ScaleReferenceOuterRadius : 1f;
        for (int i = 0; i < selected.Length; ++i)
        {
            Direction direction = selected[i];
            DirectionData data = Directions[(int)direction];
            float angle = i != 0
                ? ResolveEdgeAngle(data.initAngle, ringRotation, geometry.gapAngleDegrees * 0.5f)
                : ResolveNearestAngle(data.initAngle, Mathf.Repeat(ringRotation + 90f, 360f));

            string segmentName = DirectionName(direction) + "_" + (i + 1);
            Transform segmentTransform = rootTransform.Find(segmentName);
            bool created = segmentTransform == null;
            if (created)
            {
                var segment = new GameObject(segmentName);
                segment.layer = 7;
                segmentTransform = segment.transform;
                segmentTransform.SetParent(rootTransform, false);
            }
            segmentTransform.localPosition = Vector3.zero;
            segmentTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
            segmentTransform.localScale = new Vector3(scale, scale, 1f);
            segmentTransform.gameObject.SetActive(IsSelected(i, count));

            SkeletonAnimation animation = segmentTransform.GetComponentInChildren<SkeletonAnimation>(true);
            if (animation == null)
            {
                var nativeMeshNode = new GameObject("suolian");
                nativeMeshNode.layer = 7;
                Transform nativeMeshTransform = nativeMeshNode.transform;
                nativeMeshTransform.SetParent(segmentTransform, false);
                nativeMeshTransform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
                nativeMeshTransform.localEulerAngles = new Vector3(0f, 0f, data.childAngle);
                animation = SkeletonAnimation.AddToGameObject(nativeMeshNode, sourceSkeletonDataAsset, true);
                if (animation == null) throw new InvalidOperationException("Could not create original chain Spine component.");
                animation.pmaVertexColors = true;
                animation.Initialize(true);
                animation.Update(0f);
                animation.LateUpdate();
                PlayIdle(animation, direction);
                PlayChainAppear(segmentTransform.gameObject);
            }
        }
        UpdateChainCount(root, lockCount);
        return root;
    }

    /// <summary>Changes the visible native segment set while retaining its authored Spine instances.</summary>
    public static void UpdateChainCount(GameObject chainRoot, int lockCount)
    {
        if (chainRoot == null) return;
        int count = Mathf.Clamp(lockCount, 0, 3);
        for (int i = 0; i < 3; i++)
        {
            Transform segment = GetChainSegment(chainRoot, i);
            if (segment != null) segment.gameObject.SetActive(IsSelected(i, count));
        }
    }

    /// <summary>Returns the stable native segment at authored slot 0, 1, or 2.</summary>
    public static Transform GetChainSegment(GameObject chainRoot, int slot)
    {
        if (chainRoot == null || slot < 0 || slot >= 3) return null;
        Transform[] children = chainRoot.transform.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; ++i)
        {
            string name = children[i].name;
            if (IsChainSegmentName(name) && name.EndsWith("_" + (slot + 1), StringComparison.Ordinal)) return children[i];
        }
        return null;
    }

    static bool IsSelected(int slot, int count)
    {
        return count >= 3 || (count == 1 && slot == 0) || (count == 2 && slot != 0);
    }

    public static GameObject CreateChainVisual(Transform ownerTransform, Geometry geometry, int lockCount)
    {
        return CreateOrUpdateChainVisual(ownerTransform, null, geometry, lockCount);
    }

    public static void DestroyChainVisual(GameObject root)
    {
        if (root != null) DestroyObject(root);
    }

    /// <summary>Plays the source one-shot shake animation without queuing an idle clip.</summary>
    public static bool PlayChainShake(GameObject chainRoot)
    {
        return PlayForEachSegment(chainRoot, "huangdong", loop: false, queueIdleAfter: false);
    }

    /// <summary>Plays the source break animation and returns its duration for solve timing.</summary>
    public static float PlayChainBreak(GameObject chainSegment)
    {
        SkeletonAnimation animation = FindAnimation(chainSegment);
        if (animation == null) return 0f;
        string direction = GetDirectionFromSegment(chainSegment);
        string clip = AnimationName(direction, "break");
        Spine.Animation source = FindSourceAnimation(animation, clip);
        if (source == null) source = FindSourceAnimation(animation, AnimationName("mid", "break"));
        if (source == null) return 0f;
        animation.AnimationState.SetAnimation(0, source.Name, false);
        return source.Duration;
    }

    /// <summary>Uses the source appear clip when available; other source directions start idle.</summary>
    public static bool PlayChainAppear(GameObject chainSegment)
    {
        SkeletonAnimation animation = FindAnimation(chainSegment);
        if (animation == null) return false;
        string direction = GetDirectionFromSegment(chainSegment);
        Spine.Animation appear = FindSourceAnimation(animation, AnimationName(direction, "appear"));
        if (appear == null)
        {
            PlayIdle(animation, ParseDirection(chainSegment));
            return false;
        }
        animation.AnimationState.SetAnimation(0, appear.Name, false);
        animation.AnimationState.AddAnimation(0, AnimationName(direction, "jingzhen"), true, 0f);
        return true;
    }

    static void EnsureSourceAssets()
    {
        if (sourceTexture == null)
        {
            sourceTexture = Resources.Load<Texture2D>(ResourceRoot + "spine_junei_suolian");
            if (sourceTexture == null) throw new InvalidOperationException("Original chain Spine atlas is missing.");
        }
        if (sourceSkeleton == null)
        {
            sourceSkeleton = Resources.Load<TextAsset>(ResourceRoot + "ChainSource.skel");
            if (sourceSkeleton == null) throw new InvalidOperationException("Original chain Spine skeleton binary is missing.");
        }
        if (sourceAtlasText == null)
        {
            sourceAtlasText = Resources.Load<TextAsset>(ResourceRoot + "spine_junei_suolian.atlas");
            if (sourceAtlasText == null) throw new InvalidOperationException("Original chain Spine atlas metadata is missing.");
        }
        if (sourceMaterial == null)
        {
            sourceMaterial = Resources.Load<Material>(ResourceRoot + "spine_junei_suolian_Material");
            if (sourceMaterial == null)
            {
                Shader shader = Shader.Find("Spine/ChainStyleLine");
                if (shader == null) throw new InvalidOperationException("Original chain texture shader is missing.");
                sourceMaterial = new Material(shader) { name = "spine_junei_suolian (Original)" };
                sourceMaterial.SetTexture("_MainTex", sourceTexture);
            }
        }
        if (sourceAtlasAsset == null)
        {
            sourceAtlasAsset = Resources.Load<SpineAtlasAsset>(ResourceRoot + "spine_junei_suolian_Atlas");
            if (sourceAtlasAsset == null)
                sourceAtlasAsset = SpineAtlasAsset.CreateRuntimeInstance(sourceAtlasText,
                    new[] { sourceTexture }, sourceMaterial, true);
        }
        if (sourceSkeletonDataAsset == null)
        {
            sourceSkeletonDataAsset = Resources.Load<SkeletonDataAsset>(ResourceRoot + "ChainSource_SkeletonData");
            if (sourceSkeletonDataAsset == null)
                sourceSkeletonDataAsset = SkeletonDataAsset.CreateRuntimeInstance(sourceSkeleton,
                    sourceAtlasAsset, true, 0.01f);
        }
        if (sourceSkeletonDataAsset.GetSkeletonData(false) == null)
            throw new InvalidOperationException("Original chain Spine data failed to initialize.");
    }

    static bool PlayForEachSegment(GameObject root, string clipSuffix, bool loop, bool queueIdleAfter)
    {
        if (root == null) return false;
        SkeletonAnimation[] animations = root.GetComponentsInChildren<SkeletonAnimation>(true);
        bool played = false;
        for (int i = 0; i < animations.Length; ++i)
        {
            SkeletonAnimation animation = animations[i];
            if (animation == null) continue;
            string direction = GetDirectionFromSegment(animation.transform.parent != null
                ? animation.transform.parent.gameObject : animation.gameObject);
            Spine.Animation source = FindSourceAnimation(animation, AnimationName(direction, clipSuffix));
            if (source == null) source = FindSourceAnimation(animation, AnimationName("mid", clipSuffix));
            if (source == null) continue;
            animation.AnimationState.SetAnimation(0, source.Name, loop);
            if (queueIdleAfter)
                animation.AnimationState.AddAnimation(0, AnimationName(direction, "jingzhen"), true, 0f);
            played = true;
        }
        return played;
    }

    static SkeletonAnimation FindAnimation(GameObject segment)
    {
        return segment != null ? segment.GetComponentInChildren<SkeletonAnimation>(true) : null;
    }

    static Spine.Animation FindSourceAnimation(SkeletonAnimation animation, string name)
    {
        if (animation == null || animation.Skeleton == null || animation.Skeleton.Data == null) return null;
        return animation.Skeleton.Data.FindAnimation(name);
    }

    static string GetDirectionFromSegment(GameObject segment)
    {
        if (segment == null) return "mid";
        for (Transform node = segment.transform; node != null; node = node.parent)
        {
            if (node.name.StartsWith("low", StringComparison.Ordinal)) return "low";
            if (node.name.StartsWith("high", StringComparison.Ordinal)) return "high";
            if (node.name.StartsWith("shu", StringComparison.Ordinal)) return "shu";
            if (node.name.StartsWith("mid", StringComparison.Ordinal)) return "mid";
        }
        return "mid";
    }

    static Direction ParseDirection(GameObject segment)
    {
        string name = GetDirectionFromSegment(segment);
        if (name == "low") return Direction.RightSlant;
        if (name == "high") return Direction.LeftSlant;
        if (name == "shu") return Direction.Vertical;
        return Direction.Horizontal;
    }

    static string AnimationName(string direction, string suffix)
    {
        return "rr_suolian_" + direction + "_" + suffix;
    }

    static void PlayIdle(SkeletonAnimation animation, Direction direction)
    {
        string directionName = DirectionName(direction);
        Spine.Animation idle = FindSourceAnimation(animation, AnimationName(directionName, "jingzhen"));
        if (idle == null) idle = FindSourceAnimation(animation, AnimationName("mid", "jingzhen"));
        if (idle != null) animation.AnimationState.SetAnimation(0, idle.Name, true);
    }

    static Direction ResolveDirection(float angle)
    {
        switch (Mathf.RoundToInt(angle / 45f) % 8)
        {
            case 1:
            case 5: return Direction.RightSlant;
            case 2:
            case 6: return Direction.Vertical;
            case 3:
            case 7: return Direction.LeftSlant;
            default: return Direction.Horizontal;
        }
    }

    static float ResolveNearestAngle(float baseAngle, float target)
    {
        float a = Mathf.Repeat(target, 360f);
        float b = Mathf.Repeat(target + 180f, 360f);
        return Mathf.Abs(Mathf.DeltaAngle(baseAngle, a)) <= Mathf.Abs(Mathf.DeltaAngle(baseAngle, b)) ? a : b;
    }

    static float ResolveEdgeAngle(float initAngle, float ringLocalZ, float halfGap)
    {
        float a = ResolveNearestAngle(initAngle, Mathf.Repeat(ringLocalZ + halfGap + ChainEdgeInsetDegrees, 360f));
        float b = ResolveNearestAngle(initAngle, Mathf.Repeat(ringLocalZ - halfGap - ChainEdgeInsetDegrees, 360f));
        return Mathf.Abs(Mathf.DeltaAngle(initAngle, a)) <= Mathf.Abs(Mathf.DeltaAngle(initAngle, b)) ? a : b;
    }

    static string DirectionName(Direction direction)
    {
        switch (direction)
        {
            case Direction.Horizontal: return "mid";
            case Direction.RightSlant: return "low";
            case Direction.LeftSlant: return "high";
            case Direction.Vertical: return "shu";
            default: return direction.ToString();
        }
    }

    static bool IsChainSegmentName(string name)
    {
        return name.StartsWith("mid_", StringComparison.Ordinal) ||
            name.StartsWith("low_", StringComparison.Ordinal) ||
            name.StartsWith("high_", StringComparison.Ordinal) ||
            name.StartsWith("shu_", StringComparison.Ordinal);
    }

    static void DestroyObject(UnityEngine.Object value)
    {
        if (Application.isPlaying) UnityEngine.Object.Destroy(value);
        else UnityEngine.Object.DestroyImmediate(value);
    }
}

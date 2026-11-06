using UnityEngine;

/// <summary>Original serialized RingLevelConfig timing/weighted curves and base eff_cycle_boom modules.
/// Raw provenance: Research/eff_cycle_boom_modules.json and visual_motion_evidence.md.
/// The particle atlas is extracted verbatim; its compiled Android shader is reimplemented
/// for the active texture/tint/alpha-blend features, rather than loaded as a foreign binary.</summary>
public static class RotateRingsOriginalMotion
{
    public const float EntryDuration = .5f;
    public const float WobbleDuration = 0.5f;
    public const float InitialWaitingScale = 0f;
    public const float VictoryDelay = 1f;
    static readonly AnimationCurve entry = new AnimationCurve(new Keyframe(0f, 0f, 5.7763319f, 5.7763319f, 0f, 0.334811181f) { weightedMode = (WeightedMode)3 }, new Keyframe(0.411148697f, 1.03961229f, 0.0182507597f, 0.0182507597f, 0.474737257f, 0.500202656f) { weightedMode = (WeightedMode)3 }, new Keyframe(0.715143979f, 0.984450877f, 0.0129822325f, 0.0129822325f, 0.489575773f, 0.273395568f) { weightedMode = (WeightedMode)3 }, new Keyframe(1f, 1f, 0.00610637525f, 0.00610637525f, 0.718667269f, 0f) { weightedMode = (WeightedMode)3 });
    static readonly AnimationCurve wobble = new AnimationCurve(new Keyframe(0f, 0f, 21.0131741f, 21.0131741f, 0f, 0.109363973f) { weightedMode = (WeightedMode)0 }, new Keyframe(0.119999997f, 1f, -0.24589394f, -0.24589394f, 0f, 0.592932165f) { weightedMode = (WeightedMode)0 }, new Keyframe(0.239999995f, -0.649999976f, 0f, 0f, 0f, 0f) { weightedMode = (WeightedMode)0 }, new Keyframe(0.379999995f, 0.379999995f, 0f, 0f, 0f, 0f) { weightedMode = (WeightedMode)0 }, new Keyframe(0.519999981f, -0.180000007f, 0f, 0f, 0f, 0f) { weightedMode = (WeightedMode)0 }, new Keyframe(0.680000007f, 0.0599999987f, 0f, 0f, 0f, 0f) { weightedMode = (WeightedMode)0 }, new Keyframe(1f, 0f, 0f, 0f, 0f, 0f) { weightedMode = (WeightedMode)0 });
    static Material fragmentMaterial;

    public static float Stagger(int count) => Mathf.Max(Mathf.Min(0.0250000004f, 0.800000012f / Mathf.Max(1,count)), 0.00100000005f);
    /// <param name="t">Normalized individual entry progress.</param>
    public static float EntryScale(float t) => Mathf.LerpUnclamped(0.5f, 1f, entry.Evaluate(Mathf.Clamp01(t)));
    /// <param name="t">Normalized denied-gesture progress.</param>
    public static float WobbleAngle(float t) => 8f * wobble.Evaluate(Mathf.Clamp01(t));

    public static GameObject PlayShatter(Mesh sourceMesh, Transform source, Color color)
    {
        if (sourceMesh == null || source == null || sourceMesh.vertexCount == 0) return null;
        if (fragmentMaterial == null)
        {
            var shader = Resources.Load<Shader>("OriginalMotion/OriginalFragment");
            var texture = Resources.Load<Texture2D>("OriginalMotion/CycleFragmentAtlas");
            if (shader == null || texture == null) { Debug.LogError("Missing extracted original ring fragment resources."); return null; }
            fragmentMaterial = new Material(shader) { name = "mat_cyclefragment_test (source active features)", mainTexture = texture };
            fragmentMaterial.SetColor("_Tint", new Color(1.26218963f, 1.26218963f, 1.26218963f, 1f));
        }
        var root = new GameObject("eff_cycle_boom"); root.SetActive(false);
        root.transform.position = source.position + new Vector3(0f, 0f, -5f);
        root.transform.rotation = Quaternion.Euler(0f,0f,source.eulerAngles.z);
        root.transform.localScale = Vector3.one;
        // The source root particle system has no emission, but its five-second
        // lifetime is included in RingSolveSuccessEffectService's cleanup span.
        var holder = root.AddComponent<ParticleSystem>();
        holder.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        var holderMain = holder.main; holderMain.duration=5f; holderMain.startLifetime=5f; holderMain.loop=false; holderMain.playOnAwake=false; holderMain.simulationSpace=ParticleSystemSimulationSpace.World;holderMain.maxParticles=200;holderMain.scalingMode=ParticleSystemScalingMode.Local;
        var holderEmission=holder.emission;holderEmission.enabled=false;
        var holderShape=holder.shape;holderShape.enabled=false;
        var holderRenderer=holder.GetComponent<ParticleSystemRenderer>();holderRenderer.enabled=false;holderRenderer.enableGPUInstancing=false;holderRenderer.sortingOrder=10;
        var child = new GameObject("cycle_fragment_test"); child.transform.SetParent(root.transform,false);
        var fragments=child.AddComponent<ParticleSystem>();fragments.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        var main=fragments.main; main.duration=5f;main.loop=false;main.playOnAwake=false;main.simulationSpace=ParticleSystemSimulationSpace.World;main.simulationSpeed=1f;main.useUnscaledTime=false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.600000024f, 0.75f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.870000005f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.800000012f, 0.600000024f);
        main.startRotation = new ParticleSystem.MinMaxCurve(-6.28318501f, 6.28318501f);
        main.gravityModifier = new ParticleSystem.MinMaxCurve(9.8345108f, new AnimationCurve(new Keyframe(0f, -1f, 19.2653675f, 19.2653675f, 0.333333343f, 0.255715966f) { weightedMode = (WeightedMode)0 }, new Keyframe(0.0487300791f, 0.707663238f, 0.0857943669f, 0f, 1f, 0.333333343f) { weightedMode = (WeightedMode)3 }, new Keyframe(0.113994226f, 0.484448522f, 0.117407702f, 3.15783978f, 0.621656597f, 0.0916043669f) { weightedMode = (WeightedMode)3 }, new Keyframe(1f, 0.734712362f, -0.0727554336f, -0.0727554336f, 0.209172964f, 0.333333343f) { weightedMode = (WeightedMode)1 }), new AnimationCurve(new Keyframe(0f, -1f, 38.4715881f, 38.4715881f, 0.333333343f, 0.2289574f) { weightedMode = (WeightedMode)0 }, new Keyframe(0.0380976684f, 0.896228552f, -0.107561477f, 0.400920451f, 0.549893439f, 0.56333077f) { weightedMode = (WeightedMode)3 }, new Keyframe(0.106459685f, 0.699842274f, -0.107561477f, 3.97219896f, 0.549893439f, 0.0903396532f) { weightedMode = (WeightedMode)3 }, new Keyframe(1f, 1f, 0f, 0f, 0.333333343f, 0.333333343f) { weightedMode = (WeightedMode)0 }));
        main.startColor = new Color(.893081725f, .893081725f, .893081725f, 1f) * (sourceMesh.colors.Length == sourceMesh.vertexCount ? Color.white : color);
        main.maxParticles = 200; main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        var shape = fragments.shape; shape.enabled = true; shape.shapeType = ParticleSystemShapeType.Mesh; shape.mesh = sourceMesh;
        shape.meshShapeType = ParticleSystemMeshShapeType.Triangle; shape.useMeshColors = true; shape.angle = 25f; shape.sphericalDirectionAmount = .449999988f;
        shape.position = Vector3.zero; shape.rotation = Vector3.zero; shape.scale = Vector3.one; shape.normalOffset = 0f;
        var emission = fragments.emission; emission.enabled = true; emission.rateOverTime = 0f; emission.rateOverDistance = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, new ParticleSystem.MinMaxCurve(50f), 1, .01f) { probability = 1f } });
        var size = fragments.sizeOverLifetime; size.enabled = true; size.separateAxes = false; size.size = new ParticleSystem.MinMaxCurve(1.29999995f, new AnimationCurve(new Keyframe(0f, 0.79608047f, 0.069495067f, 0.069495067f, 0.333333343f, 0.409476966f) { weightedMode = (WeightedMode)2 }, new Keyframe(0.21674192f, 0.907554626f, 0.593083799f, 0.593083799f, 0.333333343f, 0.130474195f) { weightedMode = (WeightedMode)3 }, new Keyframe(1f, 1f, 0.00230286899f, 0.00230286899f, 0.684958577f, 0.333333343f) { weightedMode = (WeightedMode)3 }));
        var rotation = fragments.rotationOverLifetime; rotation.enabled = true; rotation.separateAxes = false; rotation.z = new ParticleSystem.MinMaxCurve(-13.9626341f, 13.9626341f);
        var gradient = new Gradient(); gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) }, new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.763210498f), new GradientAlphaKey(0f, 1f) });
        var fade = fragments.colorOverLifetime; fade.enabled = true; fade.color = gradient;
        var atlas = fragments.textureSheetAnimation; atlas.enabled = true; atlas.mode = ParticleSystemAnimationMode.Grid; atlas.numTilesX = 2; atlas.numTilesY = 2;
        atlas.animation = ParticleSystemAnimationType.WholeSheet; atlas.rowMode = ParticleSystemAnimationRowMode.Random; atlas.cycleCount = 1; atlas.frameOverTime = new ParticleSystem.MinMaxCurve(0.111111112f, 0.999899983f); atlas.startFrame = 0f;
        var velocity = fragments.velocityOverLifetime; velocity.enabled = true; velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.x = new ParticleSystem.MinMaxCurve(0.714968085f, new AnimationCurve(new Keyframe(0f, 0.0127421441f, -0.109356135f, -0.109356135f, 0.333333343f, 0.565312326f) { weightedMode = (WeightedMode)3 }, new Keyframe(0.244222537f, -0.214145422f, -1.46457338f, -1.46457338f, 0.311118275f, 0.400187254f) { weightedMode = (WeightedMode)3 }, new Keyframe(0.409396142f, -1f, 0.0138717974f, 0f, 0.333333343f, 0.147054449f) { weightedMode = (WeightedMode)0 }, new Keyframe(1f, -1f, 0f, 0f, 0.333333343f, 0.333333343f) { weightedMode = (WeightedMode)0 }), new AnimationCurve(new Keyframe(0f, 0.0127421441f, 0.146584585f, 0.146584585f, 0.333333343f, 0.547662497f) { weightedMode = (WeightedMode)3 }, new Keyframe(0.252093256f, 0.224450827f, 1.89148319f, 1.89148319f, 0.301330388f, 0.356336564f) { weightedMode = (WeightedMode)3 }, new Keyframe(0.408396125f, 1f, -0.267151922f, 0f, 0.333333343f, 0.333333343f) { weightedMode = (WeightedMode)0 }, new Keyframe(1f, 1f, 0f, 0f, 0.333333343f, 0.333333343f) { weightedMode = (WeightedMode)0 }));
        velocity.y = new ParticleSystem.MinMaxCurve(1.757182f, new AnimationCurve(new Keyframe(0f, 0f, -27.7218075f, 0f, 0.333333343f, 0.0759494454f) { weightedMode = (WeightedMode)0 }, new Keyframe(1f, 0f, -0f, 0f, 0.333333343f, 0.333333343f) { weightedMode = (WeightedMode)0 }), new AnimationCurve(new Keyframe(0f, 0f, -27.7218571f, 0f, 0.333333343f, 0.218750015f) { weightedMode = (WeightedMode)0 }, new Keyframe(1f, 0f, -0f, 0f, 0.333333343f, 0.333333343f) { weightedMode = (WeightedMode)0 }));
        velocity.z = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f, 0.333333343f, 0.333333343f) { weightedMode = (WeightedMode)0 }, new Keyframe(1f, 0f, 0f, 0f, 0.333333343f, 0.333333343f) { weightedMode = (WeightedMode)0 }), new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f, 0.333333343f, 0.333333343f) { weightedMode = (WeightedMode)0 }, new Keyframe(1f, 0f, 0f, 0f, 0.333333343f, 0.333333343f) { weightedMode = (WeightedMode)0 }));
        var noise = fragments.noise; noise.enabled = true; noise.separateAxes = false; noise.strength = .0799999982f; noise.frequency = .180000007f; noise.damping = true; noise.octaveCount = 1; noise.octaveMultiplier = .5f; noise.octaveScale = 2f; noise.quality = ParticleSystemNoiseQuality.Medium; noise.scrollSpeed = 0f; noise.remapEnabled = false; noise.positionAmount = 1f; noise.rotationAmount = 0f; noise.sizeAmount = 0f;
        var renderer=fragments.GetComponent<ParticleSystemRenderer>(); renderer.sharedMaterial=fragmentMaterial;
        renderer.renderMode=ParticleSystemRenderMode.Billboard;renderer.alignment=ParticleSystemRenderSpace.Local;
        renderer.minParticleSize=0f;renderer.maxParticleSize=10f;renderer.sortingOrder=10;renderer.enableGPUInstancing=false;
        renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
        root.SetActive(true);holder.Play(false);fragments.Play(false);
        Object.Destroy(root,SourcePlaybackDuration(root)); return root;
    }

    static float SourcePlaybackDuration(GameObject effectRoot)
    {
        float duration=0f;
        foreach(var particleSystem in effectRoot.GetComponentsInChildren<ParticleSystem>(true))
        {
            var main=particleSystem.main;
            duration=Mathf.Max(duration,main.startDelay.constantMax+main.startLifetime.constantMax);
        }
        return duration>0f?duration:1f;
    }
}

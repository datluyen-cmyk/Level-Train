"""Bake the extracted base eff_cycle_boom's active modules into a Unity helper."""
from pathlib import Path
import json

ROOT=Path(__file__).resolve().parents[1]
data=json.loads((ROOT/'Research/eff_cycle_boom_modules.json').read_text())
ps=next(o['tree'] for o in data['objects'] if o['pathId']==1668592748615786144)
def f(value):return format(float(value),'.9g')+'f'
def curve(t):
    keys=[]
    for k in t['m_Curve']:
        keys.append('new Keyframe('+', '.join(f(k[x]) for x in ['time','value','inSlope','outSlope','inWeight','outWeight'])+') { weightedMode = (WeightedMode)'+str(k['weightedMode'])+' }')
    return 'new AnimationCurve('+', '.join(keys)+')'
def mm(t):
    mode=t['minMaxState']
    if mode==0:return 'new ParticleSystem.MinMaxCurve('+f(t['scalar'])+')'
    if mode==1:return 'new ParticleSystem.MinMaxCurve('+f(t['scalar'])+', '+curve(t['maxCurve'])+')'
    if mode==2:return 'new ParticleSystem.MinMaxCurve('+f(t['scalar'])+', '+curve(t['minCurve'])+', '+curve(t['maxCurve'])+')'
    return 'new ParticleSystem.MinMaxCurve('+f(t['minScalar'])+', '+f(t['scalar'])+')'
lines=[]
def line(s):lines.append('        '+s)
initial=ps['InitialModule']
for prop,key in [('startLifetime','startLifetime'),('startSpeed','startSpeed'),('startSize','startSize'),('startRotation','startRotation'),('gravityModifier','gravityModifier')]:line('main.'+prop+' = '+mm(initial[key])+';')
line('main.startColor = new Color(.893081725f, .893081725f, .893081725f, 1f) * (sourceMesh.colors.Length == sourceMesh.vertexCount ? Color.white : color);')
line('main.maxParticles = 200; main.scalingMode = ParticleSystemScalingMode.Hierarchy;')
line('var shape = fragments.shape; shape.enabled = true; shape.shapeType = ParticleSystemShapeType.Mesh; shape.mesh = sourceMesh;')
line('shape.meshShapeType = ParticleSystemMeshShapeType.Triangle; shape.useMeshColors = true; shape.angle = 25f; shape.sphericalDirectionAmount = .449999988f;')
line('shape.position = Vector3.zero; shape.rotation = Vector3.zero; shape.scale = Vector3.one; shape.normalOffset = 0f;')
line('var emission = fragments.emission; emission.enabled = true; emission.rateOverTime = 0f; emission.rateOverDistance = 0f;')
line('emission.SetBursts(new[] { new ParticleSystem.Burst(0f, '+mm(ps['EmissionModule']['m_Bursts'][0]['countCurve'])+', 1, .01f) { probability = 1f } });')
line('var size = fragments.sizeOverLifetime; size.enabled = true; size.separateAxes = false; size.size = '+mm(ps['SizeModule']['curve'])+';')
line('var rotation = fragments.rotationOverLifetime; rotation.enabled = true; rotation.separateAxes = false; rotation.z = '+mm(ps['RotationModule']['curve'])+';')
g=ps['ColorModule']['gradient']['maxGradient']
line('var gradient = new Gradient(); gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) }, new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, '+f(g['atime1']/65535)+'), new GradientAlphaKey(0f, 1f) });')
line('var fade = fragments.colorOverLifetime; fade.enabled = true; fade.color = gradient;')
uv=ps['UVModule'];line('var atlas = fragments.textureSheetAnimation; atlas.enabled = true; atlas.mode = ParticleSystemAnimationMode.Grid; atlas.numTilesX = 2; atlas.numTilesY = 2;')
line('atlas.animation = ParticleSystemAnimationType.WholeSheet; atlas.rowMode = ParticleSystemAnimationRowMode.Random; atlas.cycleCount = 1; atlas.frameOverTime = '+mm(uv['frameOverTime'])+'; atlas.startFrame = 0f;')
line('var velocity = fragments.velocityOverLifetime; velocity.enabled = true; velocity.space = ParticleSystemSimulationSpace.Local;')
for key in ['x','y','z']:line('velocity.'+key+' = '+mm(ps['VelocityModule'][key])+';')
n=ps['NoiseModule'];line('var noise = fragments.noise; noise.enabled = true; noise.separateAxes = false; noise.strength = .0799999982f; noise.frequency = .180000007f; noise.damping = true; noise.octaveCount = 1; noise.octaveMultiplier = .5f; noise.octaveScale = 2f; noise.quality = ParticleSystemNoiseQuality.Medium; noise.scrollSpeed = 0f; noise.remapEnabled = false; noise.positionAmount = 1f; noise.rotationAmount = 0f; noise.sizeAmount = 0f;')
generated='\n'.join(lines)
source='''using UnityEngine;

/// <summary>Original authored timing and base eff_cycle_boom modules, baked from APK evidence.
/// Raw provenance: Research/eff_cycle_boom_modules.json and visual_motion_evidence.md.
/// The particle atlas is extracted verbatim; its compiled Android shader is reimplemented
/// for the active texture/tint/alpha-blend features, rather than loaded as a foreign binary.</summary>
public static class RotateRingsOriginalMotion
{
    public const float EntryDuration = .5f;
    public const float WobbleDuration = .32f;
    public const float VictoryDelay = 1f;
    static readonly AnimationCurve entry = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    static readonly AnimationCurve wobble = new AnimationCurve(new Keyframe(0f,0f), new Keyframe(.12f,1f), new Keyframe(.24f,-.65f), new Keyframe(.38f,.38f), new Keyframe(.52f,-.18f), new Keyframe(.68f,.06f), new Keyframe(1f,0f));
    static Material fragmentMaterial;

    public static float Stagger(int count) => Mathf.Max(Mathf.Min(.03f, 1f / Mathf.Max(1,count)), .01f);
    /// <param name="t">Normalized individual entry progress.</param>
    public static float EntryScale(float t) => Mathf.LerpUnclamped(.72f, 1f, entry.Evaluate(Mathf.Clamp01(t)));
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
        // The source root system is inert but its duration/lifetime contributes
        // to RingSolveSuccessService's ten-second cleanup deadline.
        var holder = root.AddComponent<ParticleSystem>();
        holder.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        var holderMain = holder.main; holderMain.duration=5f; holderMain.startLifetime=5f; holderMain.loop=false; holderMain.playOnAwake=false; holderMain.simulationSpace=ParticleSystemSimulationSpace.World;holderMain.maxParticles=200;holderMain.scalingMode=ParticleSystemScalingMode.Local;
        var holderEmission=holder.emission;holderEmission.enabled=false;
        var holderShape=holder.shape;holderShape.enabled=false;
        var holderRenderer=holder.GetComponent<ParticleSystemRenderer>();holderRenderer.enabled=false;holderRenderer.enableGPUInstancing=false;holderRenderer.sortingOrder=10;
        var child = new GameObject("cycle_fragment_test"); child.transform.SetParent(root.transform,false);
        var fragments=child.AddComponent<ParticleSystem>();fragments.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        var main=fragments.main; main.duration=5f;main.loop=false;main.playOnAwake=false;main.simulationSpace=ParticleSystemSimulationSpace.World;main.simulationSpeed=1f;main.useUnscaledTime=false;
__MODULES__
        var renderer=fragments.GetComponent<ParticleSystemRenderer>(); renderer.sharedMaterial=fragmentMaterial;
        renderer.renderMode=ParticleSystemRenderMode.Billboard;renderer.alignment=ParticleSystemRenderSpace.Local;
        renderer.minParticleSize=0f;renderer.maxParticleSize=10f;renderer.sortingOrder=10;renderer.enableGPUInstancing=false;
        renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
        root.SetActive(true);holder.Play(false);fragments.Play(false);
        Object.Destroy(root,10f); return root;
    }
}
'''.replace('__MODULES__',generated)
config=json.loads((ROOT/'Research/shaders/serialized_motion_config_overrides.json').read_text())
config=next(row['tree'] for row in config['matches'] if row['tree']['m_Name']=='RingLevelConfig')
source=source.replace('public const float WobbleDuration = .32f;', 'public const float WobbleDuration = '+f(config['blockedShakeDuration'])+';\n    public const float InitialWaitingScale = '+f(config['initialAllRingsScaleFactor'])+';')
source=source.replace('static readonly AnimationCurve entry = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);','static readonly AnimationCurve entry = '+curve(config['scaleCurve'])+';')
start=source.index('    static readonly AnimationCurve wobble = ')
end=source.index('\n',start)
source=source[:start]+'    static readonly AnimationCurve wobble = '+curve(config['blockedShakeCurve'])+';'+source[end:]
source=source.replace('Mathf.Max(Mathf.Min(.03f, 1f / Mathf.Max(1,count)), .01f)', 'Mathf.Max(Mathf.Min('+f(config['waveInterval'])+', '+f(config['waveMaxEnterDuration'])+' / Mathf.Max(1,count)), '+f(config['waveMinInterval'])+')')
source=source.replace('Mathf.LerpUnclamped(.72f, 1f, entry.Evaluate(Mathf.Clamp01(t)))', 'Mathf.LerpUnclamped('+f(config['startScaleFactor'])+', 1f, entry.Evaluate(Mathf.Clamp01(t)))')
source=source.replace('Original authored timing and base eff_cycle_boom modules, baked from APK evidence.', 'Original serialized RingLevelConfig timing/weighted curves and base eff_cycle_boom modules.')
out=ROOT/'RotateRings Test/Assets/RotateRings/Scripts/RotateRingsOriginalMotion.cs';out.write_text(source,encoding='utf8')
print(out)

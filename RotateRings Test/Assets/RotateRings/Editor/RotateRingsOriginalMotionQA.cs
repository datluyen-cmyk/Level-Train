#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

/// <summary>Source-bound module checks and deterministic live-particle captures.
/// Invoke only in Play Mode after the integration compilation is finished.</summary>
public static class RotateRingsOriginalMotionQA
{
    [Serializable] class Key { public float time,value,inSlope,outSlope,inWeight,outWeight; public int weightedMode; }
    [Serializable] class Curve { public Key[] m_Curve; }
    [Serializable] class Config { public string m_Name; public float waveDuration,waveInterval,waveMaxEnterDuration,waveMinInterval,initialAllRingsScaleFactor,startScaleFactor,blockedShakeDuration,blockedShakeStrength; public Curve scaleCurve,blockedShakeCurve; }
    [Serializable] class Match { public Config tree; }
    [Serializable] class ConfigDump { public Match[] matches; }
    [Serializable] class Sample { public float seconds; public int liveParticles; public string image; }
    [Serializable] class Report
    {
        public bool passed,complete;
        public string status,sourceConfigSha256,atlasSha256,finalPackSha256;
        public List<string> passedChecks=new List<string>();
        public List<string> failedChecks=new List<string>();
        public List<Sample> samples=new List<Sample>();
        public float cleanupElapsedGameSeconds;
        public string scope="Actual Unity Play Mode helper/module checks; deterministic original particle screenshots. Does not advance any gameplay level or invoke UI boosters.";
        public string integrationLimit="Main Runtime/WorldView callbacks, removal visibility and booster buttons require the main runtime smoke test. This isolated helper test does not certify those interactions.";
    }
    static Report report;
    static GameObject effect,source;
    static Mesh sourceMesh;
    static float cleanupStart;
    static double startWall;
    static string reportPath;

    [MenuItem("Rotate Rings/QA/Original Motion Evidence")]
    public static void RunMenu() { Run(); }

    public static string Run()
    {
        if (report!=null && !report.complete) throw new InvalidOperationException("Motion QA already running.");
        if (!Application.isPlaying || EditorApplication.isPaused || !Mathf.Approximately(Time.timeScale,1f))
            throw new InvalidOperationException("Motion QA requires unpaused Play Mode at timeScale1. It does not change the user's play state.");
        string research=Path.GetFullPath(Path.Combine(Application.dataPath,"..","..","Research"));
        string folder=Path.Combine(research,"physics_v2","motion_qa");Directory.CreateDirectory(folder);
        reportPath=Path.Combine(folder,"motion_qa.json");
        report=new Report { status="running source-bound checks" };
        string configPath=Path.Combine(research,"shaders","serialized_motion_config_overrides.json");
        string atlasPath=Path.Combine(Application.dataPath,"RotateRings","Resources","OriginalMotion","CycleFragmentAtlas.png");
        string packPath=Path.Combine(Application.dataPath,"RotateRings","Resources","Levels","generated_levels.json");
        report.sourceConfigSha256=Hash(configPath);report.atlasSha256=Hash(atlasPath);report.finalPackSha256=Hash(packPath);
        try
        {
            var dump=JsonUtility.FromJson<ConfigDump>(File.ReadAllText(configPath));
            Config config=null;foreach(var match in dump.matches) if(match.tree.m_Name=="RingLevelConfig") config=match.tree;
            if(config==null) throw new InvalidOperationException("Original serialized RingLevelConfig missing.");
            var entry=Import(config.scaleCurve);var wobble=Import(config.blockedShakeCurve);
            Check(Near(RotateRingsOriginalMotion.EntryDuration,config.waveDuration),"Entry duration equals serialized config");
            Check(Near(RotateRingsOriginalMotion.InitialWaitingScale,config.initialAllRingsScaleFactor),"Waiting scale equals serialized zero, invisible before own stagger");
            foreach(int count in new[]{1,3,30,100,300})
                Check(Near(RotateRingsOriginalMotion.Stagger(count),Mathf.Max(Mathf.Min(config.waveInterval,config.waveMaxEnterDuration/count),config.waveMinInterval)),"Stagger matches serialized config for count"+count);
            foreach(float t in new[]{0f,.04f,.12f,.2f,.411148697f,.52f,.715143979f,.9f,1f})
            {
                Check(Near(RotateRingsOriginalMotion.EntryScale(t),Mathf.LerpUnclamped(config.startScaleFactor,1f,entry.Evaluate(t))),"Source weighted entry curve at"+t);
                Check(Near(RotateRingsOriginalMotion.WobbleAngle(t),config.blockedShakeStrength*wobble.Evaluate(t)),"Source wobble curve at"+t);
            }
            Check(Near(RotateRingsOriginalMotion.WobbleDuration,config.blockedShakeDuration),"Wobble duration equals serialized .5s");
            Check(Near(RotateRingsOriginalMotion.WobbleAngle(.12f),8f,.01f),"Denied angular wobble reaches8 degrees");
            Check(Near(RotateRingsOriginalMotion.VictoryDelay,1f),"Victory delay equals serialized GameplayManager1 second");

            source=new GameObject("Motion QA emitter source");source.layer=31;
            source.transform.position=new Vector3(0f,0f,0f);source.transform.rotation=Quaternion.Euler(0f,0f,25f);
            sourceMesh=OriginalRingVisualAssets.CreateRingMesh(1.45f,.36f,65f,new Color(.96f,.31f,.39f,1f));
            Vector3 initialScale=source.transform.localScale;
            effect=RotateRingsOriginalMotion.PlayShatter(sourceMesh,source.transform,new Color(.96f,.31f,.39f,1f));
            cleanupStart=Time.time;startWall=EditorApplication.timeSinceStartup;
            Check(effect!=null,"Actual PlayShatter spawns original named hierarchy");
            if(effect==null)throw new InvalidOperationException("PlayShatter returned null.");
            foreach(var t in effect.GetComponentsInChildren<Transform>(true))t.gameObject.layer=31;
            Check(Vector3.Distance(effect.transform.position,source.transform.position+Vector3.back*5f)<.0001f,"World effect position sourceZ minus5");
            Check(Near(Mathf.DeltaAngle(effect.transform.eulerAngles.z,25f),0f),"Effect preserves sourceZ angle");
            Check(Vector3.Distance(initialScale,source.transform.localScale)<.0001f,"Shatter helper does not shrink original source transform");
            var fragments=effect.transform.Find("cycle_fragment_test").GetComponent<ParticleSystem>();
            var main=fragments.main;var shape=fragments.shape;var emission=fragments.emission;var renderer=fragments.GetComponent<ParticleSystemRenderer>();
            Check(shape.shapeType==ParticleSystemShapeType.Mesh && shape.mesh==sourceMesh && shape.useMeshColors,"Shape emits from actual sourceMesh with its vertex colors");
            Check(main.simulationSpace==ParticleSystemSimulationSpace.World,"Source service world simulation override");
            Check(Near(main.startLifetime.constantMin,.6f)&&Near(main.startLifetime.constantMax,.75f),"Original lifetime .6 to .75");
            Check(main.startSpeed.mode==ParticleSystemCurveMode.Constant && Near(main.startSpeed.constant,.87f),"Original speed is constant .87, not inactive minScalar5");
            var bursts=new ParticleSystem.Burst[emission.burstCount];emission.GetBursts(bursts);
            Check(bursts.Length==1 && bursts[0].count.mode==ParticleSystemCurveMode.Constant && Near(bursts[0].count.constant,50f),"Original fixed50-particle burst");
            Check(renderer.renderMode==ParticleSystemRenderMode.Billboard && renderer.alignment==ParticleSystemRenderSpace.Local,"Original billboard renderer and local alignment");
            Check(renderer.sortingOrder==10 && !renderer.enableGPUInstancing,"Original service sorting10 and GPU instancing disabled");
            Check(renderer.sharedMaterial!=null && renderer.sharedMaterial.mainTexture==Resources.Load<Texture2D>("OriginalMotion/CycleFragmentAtlas"),"Renderer uses verbatim extracted tex_sequence_005 atlas");
            var atlas=fragments.textureSheetAnimation;
            Check(atlas.enabled && atlas.numTilesX==2 && atlas.numTilesY==2,"Original atlas2x2 texture-sheet module");
            Check(fragments.noise.enabled && fragments.velocityOverLifetime.enabled && fragments.sizeOverLifetime.enabled && fragments.rotationOverLifetime.enabled,"Original active noise/velocity/size/spin modules");
            // PlayShatter starts the emitter immediately. Stop it before setting the
            // deterministic seed; Unity asserts when randomSeed changes while playing.
            fragments.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            fragments.useAutoRandomSeed=false;fragments.randomSeed=118;
            var holder=effect.GetComponent<ParticleSystem>();
            var cameraGo=new GameObject("Motion QA isolated capture camera");var camera=cameraGo.AddComponent<Camera>();camera.enabled=false;
            camera.transform.position=new Vector3(0f,-.7f,-20f);camera.orthographic=true;camera.orthographicSize=3.5f;camera.aspect=1f;camera.cullingMask=1<<31;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.9568627f,.9647059f,.972549f);camera.nearClipPlane=.01f;camera.farClipPlane=100f;
            var rt=new RenderTexture(640,640,24,RenderTextureFormat.ARGB32);rt.Create();camera.targetTexture=rt;
            try
            {
                foreach(float seconds in new[]{.02f,.12f,.3f,.6f,.8f})
                {
                    holder.Simulate(seconds,true,true,true);holder.Pause(true);
                    string file=Path.Combine(folder,"shatter-"+Mathf.RoundToInt(seconds*1000).ToString("000")+"ms.png");
                    Capture(camera,rt,file);
                    report.samples.Add(new Sample{seconds=seconds,liveParticles=fragments.particleCount,image=file});
                    if(Near(seconds,.02f))Check(fragments.particleCount==50,"Live Unity simulation emits50 particles");
                    if(Near(seconds,.8f))Check(fragments.particleCount==0,"All visible fragments expire by.8 seconds");
                }
            }
            finally
            {
                camera.targetTexture=null;rt.Release();UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(cameraGo);
                // Hide isolated test geometry while checking the real delayed cleanup.
                foreach(var r in effect.GetComponentsInChildren<Renderer>())r.enabled=false;
            }
            report.status="waiting for five-second source particle lifetime cleanup";Save();EditorApplication.update+=WaitCleanup;
        }
        catch(Exception ex)
        {
            report.failedChecks.Add(ex.ToString());Finish();
        }
        return reportPath;
    }
    static void WaitCleanup()
    {
        if(!Application.isPlaying){report.failedChecks.Add("Play Mode ended before delayed cleanup check");Finish();return;}
        float elapsed=Time.time-cleanupStart;
        if(elapsed<4.8f && effect==null){report.failedChecks.Add("Effect destroyed before source max startDelay+startLifetime duration");Finish();return;}
        if(elapsed>=5.25f){report.cleanupElapsedGameSeconds=elapsed;Check(effect==null,"Effect auto-destroyed after source max startDelay+startLifetime duration5s");Finish();}
        else if(EditorApplication.timeSinceStartup-startWall>25d){report.failedChecks.Add("Cleanup check timed out; paused/slowed game time may have interrupted it");Finish();}
    }
    static void Finish()
    {
        EditorApplication.update-=WaitCleanup;
        report.complete=true;report.passed=report.failedChecks.Count==0;report.status=report.passed?"PASS":"FAIL";
        if(effect!=null)UnityEngine.Object.Destroy(effect);if(source!=null)UnityEngine.Object.Destroy(source);if(sourceMesh!=null)UnityEngine.Object.Destroy(sourceMesh);
        Save();Debug.Log("Original motion QA "+report.status+": "+reportPath);
    }
    static bool Near(float a,float b,float tolerance=.0002f)=>Mathf.Abs(a-b)<=tolerance;
    static void Check(bool ok,string text){(ok?report.passedChecks:report.failedChecks).Add(text);}
    static AnimationCurve Import(Curve data)
    {
        var keys=new Keyframe[data.m_Curve.Length];for(int i=0;i<keys.Length;i++){var k=data.m_Curve[i];keys[i]=new Keyframe(k.time,k.value,k.inSlope,k.outSlope,k.inWeight,k.outWeight){weightedMode=(WeightedMode)k.weightedMode};}return new AnimationCurve(keys);
    }
    static string Hash(string path){using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-","").ToLowerInvariant();}
    static void Save(){File.WriteAllText(reportPath,JsonUtility.ToJson(report,true));}
    static void Capture(Camera camera,RenderTexture target,string path)
    {
        var before=RenderTexture.active;var image=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false);
        try{camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,target.width,target.height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());}
        finally{RenderTexture.active=before;UnityEngine.Object.Destroy(image);}
    }
}
#endif

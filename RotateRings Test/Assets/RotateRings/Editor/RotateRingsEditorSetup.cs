#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RotateRingsEditorSetup {
    const string Root="Assets/RotateRings/";
    [MenuItem("Tools/Rotate Rings/Prepare Runtime Assets")]
    public static void PrepareRuntimeAssets() {
        Directory.CreateDirectory(Root+"Resources/Art");Directory.CreateDirectory(Root+"Resources/Audio");Directory.CreateDirectory(Root+"Resources/Levels");
        CopyTexture("btn_level_yellow","level_button.png");CopyTexture("home_entry_bg02","home_entry_button.png");
        CopyTexture("home_entry_bg01","home_entry_round_button.png");CopyTexture("icon_home_firstwin","home_first_win.png");
        CopyTexture("icon_home_leaderboard","home_leaderboard.png");CopyTexture("game_img_key_yellow","key_yellow.png");
        CopyTexture("game_img_key_blue","key_blue.png");CopyTexture("game_img_key_green","key_green.png");CopyTexture("game_img_key_red","key_red.png");
        CopyTexture("game_badge_lock","lock_badge.png");
        CopyTexture("common_btn_sub_settings","settings_button.png");CopyTexture("game_item_bg","game_tile.png");
        CopyAudio("rr_bgm_level2");CopyAudio("rr_sfx_wood_break_1");CopyAudio("rr_sfx_rock_break_1");CopyAudio("rr_sfx_jelly_break_1");
        AssetDatabase.Refresh();
    }
    static void CopyTexture(string prefix,string filename){string[] guids=AssetDatabase.FindAssets(prefix+" t:Texture2D",new[]{Root+"Art"});if(guids.Length==0)return;string src=AssetDatabase.GUIDToAssetPath(guids[0]);string dst=Root+"Resources/Art/"+filename;if(!File.Exists(dst))AssetDatabase.CopyAsset(src,dst);var importer=AssetImporter.GetAtPath(dst) as TextureImporter;if(importer!=null){importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.SaveAndReimport();}}
    static void CopyAudio(string clipName){string[] guids=AssetDatabase.FindAssets(clipName+" t:AudioClip",new[]{Root+"Audio"});if(guids.Length==0)return;string src=AssetDatabase.GUIDToAssetPath(guids[0]);string dst=Root+"Resources/Audio/"+Path.GetFileName(src);if(!File.Exists(dst))AssetDatabase.CopyAsset(src,dst);}

    [MenuItem("Tools/Rotate Rings/Create Play Scene")]
    public static void CreatePlayScene(){PrepareRuntimeAssets();var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var game=new GameObject("Rotate Rings Runtime");game.AddComponent<RotateRingsRuntime>();Directory.CreateDirectory("Assets/Scenes");EditorSceneManager.SaveScene(scene,"Assets/Scenes/RotateRings.unity");Debug.Log("Created Assets/Scenes/RotateRings.unity. Enter Play Mode to launch the puzzle runtime.");}

    [System.Serializable] sealed class ReplayCheck { public int id;public bool passed;public int gestures;public string error; }
    [System.Serializable] sealed class ReplayReport {
        public int schemaVersion=2;public bool complete;public int checkedLevels;public int requestedLevels;
        public string backend="Unity PolygonCollider2D.Distance, 0.5 degree sweep, exact attachment whitelist";
        public string packSha256,catalogSha256,collisionWorldSha256,replayCodeSha256;
        public System.Collections.Generic.List<ReplayCheck> levels=new System.Collections.Generic.List<ReplayCheck>();
    }
    static RotateRingsLevelPack replayPack;
    static RotateRingsTemplateCatalog replayCatalog;
    static ReplayReport replayReport;
    static string replayReportPath;
    [MenuItem("Tools/Rotate Rings/Validate All Pack Solutions")]
    public static void RunFullPackReplayValidation(){
        if(EditorApplication.isPlaying){Debug.LogError("Run physical pack validation in Edit Mode.");return;}
        if(replayPack!=null){Debug.LogWarning("Pack replay is already running.");return;}
        string path=Root+"Resources/Levels/generated_levels.json",catalogPath=Root+"Resources/Levels/physics_templates.json";
        if(!File.Exists(path)||!File.Exists(catalogPath)){Debug.LogError("Physical pack/catalog missing.");return;}
        var data=JsonUtility.FromJson<RotateRingsLevelPack>(File.ReadAllText(path));
        if(data==null||data.schemaVersion!=2||data.levels==null){Debug.LogError("The previous angle-only pack is not physically certified.");return;}
        replayCatalog=JsonUtility.FromJson<RotateRingsTemplateCatalog>(File.ReadAllText(catalogPath));
        replayPack=data;
        replayReport=new ReplayReport{requestedLevels=data.levels.Count,packSha256=Hash(path),catalogSha256=Hash(catalogPath),collisionWorldSha256=Hash(Root+"Scripts/RotateRingsCollisionWorld.cs"),replayCodeSha256=Hash(Root+"Scripts/RotateRingsReplay.cs")};
        replayReportPath=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Research/physics_v2/runtime_qa_report.json"));
        Directory.CreateDirectory(Path.GetDirectoryName(replayReportPath));
        EditorApplication.update+=ReplayNext;
        Debug.Log("Started native collider replay for "+data.levels.Count+" levels; one level per Editor update.");
    }
    static void ReplayNext(){
        if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        if(EditorApplication.isPlaying){EditorApplication.update-=ReplayNext;replayPack=null;Debug.LogError("Pack validation interrupted by Play Mode.");return;}
        if(replayReport.checkedLevels<replayPack.levels.Count){
            var level=replayPack.levels[replayReport.checkedLevels];var result=RotateRingsReplay.Replay(level,replayCatalog);
            replayReport.levels.Add(new ReplayCheck{id=level.id,passed=result.solved,gestures=result.gestures,error=result.error});replayReport.checkedLevels++;
        }
        replayReport.complete=replayReport.checkedLevels==replayPack.levels.Count;
        try{File.WriteAllText(replayReportPath,JsonUtility.ToJson(replayReport,true));}
        catch(System.IO.IOException){return;}
        if(!replayReport.complete)return;
        int passed=replayReport.levels.FindAll(x=>x.passed).Count;
        EditorApplication.update-=ReplayNext;replayPack=null;replayCatalog=null;
        if(passed==replayReport.requestedLevels)Debug.Log("Native collider replay PASS: "+passed+" / "+replayReport.requestedLevels+" levels.");
        else Debug.LogError("Native collider replay FAIL: "+passed+" / "+replayReport.requestedLevels+" levels. See "+replayReportPath);
    }
    static string Hash(string path){using(var sha=System.Security.Cryptography.SHA256.Create())return System.BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-","").ToLowerInvariant();}
}
#endif

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// Reference layouts remain outside the delivered generated-level resource.
// This viewer exists to check the port against original geometry/materials first.
public sealed class OriginalParityPreview : EditorWindow {
    GameObject root;
    Camera previewCamera;
    RenderTexture target;
    RotateRingsLevelPack pack;
    RotateRingsTemplateCatalog catalog;
    int selected;
    string problem;
    static string ResearchRoot => Path.GetFullPath(Path.Combine(Application.dataPath,"../../Research/physics_v3"));

    [MenuItem("Rotate Rings/Original source parity viewer")]
    public static void Open(){GetWindow<OriginalParityPreview>("Original reference QA").Show();}
    void OnEnable(){try{Load();}catch(Exception e){problem=e.ToString();}}
    void Load(){
        pack=JsonUtility.FromJson<RotateRingsLevelPack>(File.ReadAllText(Path.Combine(ResearchRoot,"source_parity_pack.json")));
        catalog=JsonUtility.FromJson<RotateRingsTemplateCatalog>(File.ReadAllText(Path.Combine(ResearchRoot,"source_templates.json")));
    }
    void OnGUI(){
        EditorGUILayout.HelpBox("ORIGINAL REFERENCE — visual parity fixtures only. These layouts are not newly generated levels or verified gameplay.",MessageType.Info);
        if(pack==null){EditorGUILayout.HelpBox(problem??"No fixture corpus.",MessageType.Error);if(GUILayout.Button("Reload"))Load();return;}
        var names=pack.levels.ConvertAll(l=>"Source "+l.id+" / progression "+l.position).ToArray();
        int next=EditorGUILayout.Popup("Reference",selected,names);
        if(next!=selected||(target==null&&problem==null)){selected=next;try{Render(pack.levels[selected]);problem=null;}catch(Exception e){problem=e.ToString();}}
        if(GUILayout.Button("Refresh from source data")){try{Load();Render(pack.levels[selected]);problem=null;}catch(Exception e){problem=e.ToString();}}
        if(problem!=null)EditorGUILayout.HelpBox(problem,MessageType.Error);
        if(target!=null){var rect=GUILayoutUtility.GetRect(300,600,GUILayout.ExpandWidth(true),GUILayout.ExpandHeight(true));GUI.DrawTexture(rect,target,ScaleMode.ScaleToFit);}
    }
    void Render(RotateRingsLevel level){
        Cleanup();
        root=new GameObject("Original reference QA (temporary)"){hideFlags=HideFlags.HideAndDontSave};
        var board=new GameObject("Source board");board.transform.SetParent(root.transform,false);
        var view=board.AddComponent<RotateRingsWorldView>();view.Build(level,catalog);
        var angles=new Dictionary<int,float>();foreach(var p in level.parts)angles[p.id]=p.angle;
        view.ApplyState(angles,new HashSet<int>(),new HashSet<int>(),new List<bool>());
        foreach(var tr in root.GetComponentsInChildren<Transform>(true)){tr.gameObject.layer=31;tr.gameObject.hideFlags=HideFlags.HideAndDontSave;}
        var cameraObject=new GameObject("Source QA camera");cameraObject.transform.SetParent(root.transform,false);previewCamera=cameraObject.AddComponent<Camera>();
        previewCamera.enabled=false;previewCamera.orthographic=true;previewCamera.clearFlags=CameraClearFlags.SolidColor;previewCamera.backgroundColor=new Color(1,.99490005f,.95754719f);previewCamera.cullingMask=1<<31;
        Bounds bounds=view.GeometryBounds();float aspect=.5f;
        previewCamera.orthographicSize=Mathf.Max(bounds.size.y*.5f,bounds.size.x/(2*aspect))*1.10f;
        previewCamera.transform.position=new Vector3(bounds.center.x,bounds.center.y,-40);
        target=new RenderTexture(1080,2160,24,RenderTextureFormat.ARGB32);target.Create();previewCamera.targetTexture=target;previewCamera.Render();
    }
    public static string Capture(int sourceId,string destination){
        var window=GetWindow<OriginalParityPreview>("Original reference QA");window.Load();
        window.selected=window.pack.levels.FindIndex(l=>l.id==sourceId);if(window.selected<0)throw new ArgumentException("Unknown source fixture");
        window.Render(window.pack.levels[window.selected]);var previous=RenderTexture.active;RenderTexture.active=window.target;
        var tex=new Texture2D(window.target.width,window.target.height,TextureFormat.RGBA32,false);tex.ReadPixels(new Rect(0,0,tex.width,tex.height),0,0);tex.Apply();
        Directory.CreateDirectory(Path.GetDirectoryName(destination));File.WriteAllBytes(destination,tex.EncodeToPNG());DestroyImmediate(tex);RenderTexture.active=previous;
        return "Captured original source "+sourceId+" to "+destination;
    }
    void Cleanup(){if(root!=null)DestroyImmediate(root);if(target!=null){target.Release();DestroyImmediate(target);}root=null;target=null;previewCamera=null;}
    void OnDisable(){Cleanup();}
}
#endif

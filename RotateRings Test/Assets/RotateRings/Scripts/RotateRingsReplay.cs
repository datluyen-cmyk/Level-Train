using System.Collections.Generic;
using UnityEngine;

// Engine replay uses the actual exported source PolygonCollider2D geometry.
public static class RotateRingsReplay {
    public sealed class Result { public bool solved; public int gestures; public string error; }
    static RotateRingsTemplateCatalog cachedCatalog;
    public static Result Replay(RotateRingsLevel level){
        if(cachedCatalog==null){var asset=Resources.Load<TextAsset>("Levels/physics_templates");if(asset!=null)cachedCatalog=JsonUtility.FromJson<RotateRingsTemplateCatalog>(asset.text);}
        return Replay(level,cachedCatalog);
    }
    public static Result Replay(RotateRingsLevel level,RotateRingsTemplateCatalog catalog){
        var result=new Result();
        if(level==null||catalog==null){result.error="Missing verified level/template catalog; angle-only validation is disabled.";return result;}
        var root=new GameObject("Physical solution replay"){hideFlags=HideFlags.HideAndDontSave};
        var world=root.AddComponent<RotateRingsCollisionWorld>();
        var angles=new Dictionary<int,float>();var removed=new HashSet<int>();
        try{
            foreach(var part in level.parts)angles.Add(part.id,part.angle);
            world.Build(level,catalog);world.ApplyState(angles,removed);
            if(!world.InitialPlacementValid()){result.error="Initial solid overlap against part "+world.LastBlockerOwner+" ("+world.LastBlockerRole+")";return result;}
            if(level.solution==null){result.error="Missing physical solution";return result;}
            foreach(var gesture in level.solution){
                var part=level.parts.Find(p=>p.id==gesture.part);
                if(part==null||removed.Contains(part.id)){result.error="Missing/removed gesture target "+gesture.part;return result;}
                if(gesture.kind!="rotate"||RotateRingsRules.IsSlider(part)){result.error="Unsupported gesture in this verified ring subset";return result;}
                if(world.HasOutgoingBinding(part.id)){result.error="Active outgoing physical head binding on "+part.id;return result;}
                float from=angles[part.id],travel=gesture.delta!=0?gesture.delta:Mathf.DeltaAngle(from,gesture.angle);
                if(Mathf.Abs(travel)<.1f){result.error="No angular movement";return result;}
                bool blocked;float reached=world.ClampRotation(part.id,from,travel,out blocked);
                if(blocked||Mathf.Abs(reached-from-travel)>.05f){result.error="Swept collision at gesture "+result.gestures+" on part "+part.id+" against "+world.LastBlockerOwner+"/"+world.LastBlockerRole;return result;}
                angles[part.id]=reached;world.ApplyState(angles,removed);result.gestures++;
                bool changed=true;int guard=0;
                while(changed&&guard++<level.parts.Count){
                    changed=false;
                    foreach(var p in level.parts)if(!removed.Contains(p.id)&&!world.HasIncidentBinding(p.id)){
                        removed.Add(p.id);world.ApplyState(angles,removed);changed=true;
                    }
                }
            }
            foreach(var part in level.parts)if(!removed.Contains(part.id)){result.error="Unsolved original piece "+part.id;return result;}
            result.solved=true;return result;
        }catch(System.Exception error){result.error=error.ToString();return result;}
        finally{Object.DestroyImmediate(root);}
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

// The source dispatcher uses one nearest ray intersection over layers 6 and 7.
// Input bands stay separate from the solid geometry used by the motion solver.
public sealed class RotateRingsInputWorld : MonoBehaviour {
    sealed class Area { public RotateRingsPart part; public Transform root; public PolygonCollider2D collider; public float sourceZ; public Vector2 axis; }
    readonly Dictionary<int,Area> areas=new Dictionary<int,Area>();
    readonly Dictionary<Collider2D,int> owners=new Dictionary<Collider2D,int>();
    RotateRingsLevel level;
    RotateRingsCollisionWorld collision;
    RotateRingsBombContacts bombs;

    public void Build(RotateRingsLevel value,RotateRingsTemplateCatalog catalog,RotateRingsCollisionWorld physical,RotateRingsBombContacts bombContacts){
        Clear();level=value;collision=physical;bombs=bombContacts;
        foreach(var part in level.parts){
            if(!RotateRingsRules.IsMovable(part))continue;
            if(part.inputShapes==null||part.inputShapes.Length!=1)throw new InvalidOperationException("Missing original DragArea for part "+part.id);
            var shape=part.inputShapes[0];
            if(shape.sourceColliderType!="PolygonCollider2D"||shape.sourceLayer!=6)throw new InvalidOperationException("Unsupported source DragArea "+part.id);
            var go=new GameObject("Original DragArea "+part.id);go.transform.SetParent(transform,false);go.layer=6;
            var polygon=go.AddComponent<PolygonCollider2D>();polygon.isTrigger=shape.trigger;
            if(shape.paths!=null&&shape.paths.Length>0){polygon.pathCount=shape.paths.Length;for(int i=0;i<shape.paths.Length;i++)polygon.SetPath(i,shape.paths[i].points);}else polygon.SetPath(0,shape.points);
            var template=Array.Find(catalog.templates,t=>t.id==part.templateId);
            areas.Add(part.id,new Area{part=part,root=go.transform,collider=polygon,sourceZ=shape.localZ,axis=RotateRingsRules.IsSlider(part)?RotateRingsRules.SlideAxis(part,template):Vector2.zero});
            owners.Add(polygon,part.id);
        }
    }

    public void ApplyState(Dictionary<int,float> angles,Dictionary<int,float> slides,HashSet<int> removed,Dictionary<int,int> chainLocks){
        ApplyState(angles,slides,removed,chainLocks,null);
    }

    public void ApplyState(Dictionary<int,float> angles,Dictionary<int,float> slides,HashSet<int> removed,Dictionary<int,int> chainLocks,HashSet<int> keys){
        bool changed=false;
        foreach(var pair in areas){var a=pair.Value;var p=a.part;bool ring=p.kind=="ring";
            bool locked=ring&&((chainLocks.TryGetValue(p.id,out int count)&&count>0)||(p.lockKey!=0&&keys!=null&&!keys.Contains(p.lockKey))||collision.HasOutgoingBinding(p.id));
            float slide=slides.TryGetValue(p.id,out float offset)?offset:0f;
            Vector3 position=new Vector3(p.x+a.axis.x*slide,p.y+a.axis.y*slide,p.z+(ring?(locked?.2f:.1f):a.sourceZ));
            float angle=angles.TryGetValue(p.id,out float resolvedAngle)?resolvedAngle:p.angle;
            bool enabled=!removed.Contains(p.id)&&p.inputShapes[0].sourceEnabled;
            if((a.root.localPosition-position).sqrMagnitude>1e-8f||Mathf.Abs(Mathf.DeltaAngle(a.root.localEulerAngles.z,angle))>.0001f||a.collider.enabled!=enabled){
                a.root.localPosition=position;a.root.localRotation=Quaternion.Euler(0,0,angle);a.collider.enabled=enabled;changed=true;
            }
        }
        if(changed)Physics2D.SyncTransforms();
    }

    public bool TryHit(Camera camera,Vector2 screen,HashSet<int> removed,out int owner){
        owner=-1;
        var hit=Physics2D.GetRayIntersection(camera.ScreenPointToRay(screen),float.PositiveInfinity,(1<<6)|(1<<7));
        if(hit.collider==null)return false;
        if(!owners.TryGetValue(hit.collider,out owner))owner=collision.ColliderOwner(hit.collider);
        if(owner<0&&bombs!=null)owner=bombs.ColliderOwner(hit.collider);
        int hitOwner=owner;var part=level.parts.Find(p=>p.id==hitOwner);
        if(part==null||removed.Contains(owner))return false;
        if(RotateRingsRules.IsMovable(part))return true;
        // Source TryResolveCatchGuideDragHandler forwards a fixed guide hit to its Pull.
        if(part.kind=="fixed"&&level.guideLinks!=null)foreach(var guide in level.guideLinks){
            if(guide.guideOwnerId!=owner||removed.Contains(guide.lockedOwnerId))continue;
            var pull=level.parts.Find(p=>p.id==guide.lockedOwnerId);
            if(RotateRingsRules.IsSlider(pull)){owner=pull.id;return true;}
        }
        return false;
    }
    void Clear(){foreach(var a in areas.Values)if(a.root!=null){a.collider.enabled=false;if(Application.isPlaying)Destroy(a.root.gameObject);else DestroyImmediate(a.root.gameObject);}areas.Clear();owners.Clear();}
    void OnDestroy(){Clear();}
}

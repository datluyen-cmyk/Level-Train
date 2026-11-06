using System;
using System.Collections.Generic;
using UnityEngine;

// BombRelative contact and target collection, using the exact source CircleCollider2D.
// Standalone bombs do not add a separate victory target; carried bombs leave with the owner.
public sealed class RotateRingsBombContacts : MonoBehaviour {
    sealed class Bomb {public RotateRingsBomb source;public CircleCollider2D collider;}
    readonly List<Bomb> bombs=new List<Bomb>();
    RotateRingsCollisionWorld world;
    Collider2D[] buffer=new Collider2D[64];
    public void Build(RotateRingsLevel level,RotateRingsCollisionWorld collision){
        Clear();world=collision;
        if(level.bombs==null)return;
        foreach(var source in level.bombs){
            if(source.physicalShape==null||source.physicalShape.shapeType!="circle")throw new InvalidOperationException("Unsupported native bomb collider: "+source.sourceName);
            var go=new GameObject("Native bomb contact "+source.sourceName);go.transform.SetParent(source.ownerPartId<0?transform:world.OwnerTransform(source.ownerPartId),false);go.layer=source.physicalShape.sourceLayer;go.transform.localPosition=new Vector3(0f,0f,source.physicalShape.localZ);
            var collider=go.AddComponent<CircleCollider2D>();collider.offset=source.physicalShape.center;collider.radius=source.physicalShape.radius;collider.isTrigger=source.physicalShape.trigger;collider.enabled=source.physicalShape.sourceEnabled;
            bombs.Add(new Bomb{source=source,collider=collider});
            if(source.ownerPartId>=0)world.RegisterOwnedExternalCollider(collider,source.ownerPartId,source.sourceColliderId,"bomb");
        }
        Physics2D.SyncTransforms();
    }
    public void ApplyState(HashSet<int> spent,HashSet<int> removed){for(int i=0;i<bombs.Count;i++){var b=bombs[i];bool enabled=b.source.physicalShape.sourceEnabled&&!spent.Contains(i)&&!removed.Contains(b.source.ownerPartId);if(b.collider.enabled!=enabled){b.collider.enabled=enabled;world.InvalidateExternalGeometry();}}}
    public int ColliderOwner(Collider2D collider){foreach(var bomb in bombs)if(bomb.collider==collider)return bomb.source.ownerPartId;return -1;}
    int Query(Bomb bomb,bool triggers,bool sourceMask){
        var filter=new ContactFilter2D();filter.NoFilter();filter.useTriggers=triggers;
        if(sourceMask){filter.useLayerMask=true;filter.layerMask=Physics2D.GetLayerCollisionMask(bomb.collider.gameObject.layer);}
        int count=bomb.collider.Overlap(filter,buffer);
        if(count==buffer.Length){buffer=new Collider2D[buffer.Length*2];return Query(bomb,triggers,sourceMask);}return count;
    }
    bool External(Bomb bomb,Collider2D candidate)=>candidate!=null&&candidate.enabled&&world.ColliderOwner(candidate)>=0&&world.ColliderOwner(candidate)!=bomb.source.ownerPartId;
    public bool ProbeDuringDrag(int owner,HashSet<int> spent,HashSet<int> removed,Action<int> solveTarget){
        var probes=world.GetOwnerColliders(owner);
        for(int index=0;index<bombs.Count;index++){
            var bomb=bombs[index];if(!bomb.collider.enabled||spent.Contains(index))continue;
            Collider2D trigger=null;
            int count=Query(bomb,false,true);
            for(int j=0;j<count;j++)if(External(bomb,buffer[j])&&probes.Contains(buffer[j])){trigger=buffer[j];break;}
            if(trigger==null)foreach(var probe in probes){if(!External(bomb,probe))continue;var distance=bomb.collider.Distance(probe);if(distance.isValid&&distance.distance<=.03f){trigger=probe;break;}}
            if(trigger==null){count=Query(bomb,true,false);for(int j=0;j<count;j++)if(buffer[j].gameObject.layer==7&&External(bomb,buffer[j])){trigger=buffer[j];break;}}
            if(trigger==null)continue;
            var targets=new List<int>();
            count=Query(bomb,false,true);for(int j=0;j<count;j++)AddTarget(bomb,buffer[j],removed,targets);
            if(targets.Count==0){count=Query(bomb,true,false);for(int j=0;j<count;j++)AddTarget(bomb,buffer[j],removed,targets);}
            AddTarget(bomb,trigger,removed,targets);
            if(targets.Count==0)continue;
            spent.Add(index);bomb.collider.enabled=false;world.InvalidateExternalGeometry();
            foreach(int target in targets)solveTarget(target);
            if(bomb.source.ownerPartId>=0&&!removed.Contains(bomb.source.ownerPartId))solveTarget(bomb.source.ownerPartId);
            return true;
        }
        return false;
    }
    void AddTarget(Bomb bomb,Collider2D collider,HashSet<int> removed,List<int> targets){if(!External(bomb,collider))return;int id=world.ColliderOwner(collider);if(!removed.Contains(id)&&!targets.Contains(id))targets.Add(id);}
    void Clear(){foreach(var b in bombs)if(b.collider!=null){b.collider.enabled=false;if(world!=null)world.UnregisterOwnedExternalCollider(b.collider);if(Application.isPlaying)Destroy(b.collider.gameObject);else DestroyImmediate(b.collider.gameObject);}bombs.Clear();}
    void OnDestroy(){Clear();}
}

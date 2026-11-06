#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Labeled microfixtures test native BombRelative contracts with real engine queries.
// They are not source levels, training examples, or generated content.
public static class OriginalBombQa {
    [Serializable] sealed class Check {public string name;public bool passed;public string observed;}
    [Serializable] sealed class Report {public string kind="synthetic engine regression microfixtures";public List<Check> checks=new List<Check>();}
    static RotateRingsPhysicalShape Square(string sourceRole=null){return new RotateRingsPhysicalShape{role="body",sourceRole=sourceRole,sourceLayer=7,sourceEnabled=true,points=new[]{new Vector2(-.2f,-.2f),new Vector2(.2f,-.2f),new Vector2(.2f,.2f),new Vector2(-.2f,.2f)}};}
    static RotateRingsBomb Bomb(int owner,Vector2 center){return new RotateRingsBomb{sourceName="QA only",ownerPartId=owner,physicalShape=new RotateRingsPhysicalShape{shapeType="circle",role="bomb_trigger",trigger=true,center=center,radius=.2f,sourceLayer=7,sourceEnabled=true}};}
    static void Fixture(RotateRingsLevel level,out GameObject root,out RotateRingsCollisionWorld physics,out RotateRingsBombContacts contacts){
        foreach(var part in level.parts)if(part.kind=="slider")part.sourceRigidbody=new RotateRingsRigidbody{present=true,bodyType=1,simulated=true,useFullKinematicContacts=true,mass=1,collisionDetection=1,constraints=4};
        root=new GameObject("Synthetic bomb QA"){hideFlags=HideFlags.HideAndDontSave};
        physics=root.AddComponent<RotateRingsCollisionWorld>();
        // Reuse a native Pull template ID so CollisionWorld can enforce the
        // production PullObject overlap contract. The QA geometry remains the
        // small labeled square above; the test is for bomb ordering, not asset parity.
        var catalog=new RotateRingsTemplateCatalog{schemaVersion=3,templates=new[]{new RotateRingsOriginalTemplate{id="native3-1cc50926e27b660340a7",kind="slider",physicalShapes=new[]{Square("StickBody")},movement=new RotateRingsOriginalMovement{lockDirectionType=0,axisLocal=Vector2.up}},new RotateRingsOriginalTemplate{id="qa-fixed",kind="fixed",physicalShapes=new[]{Square()}}}};
        physics.Build(level,catalog);contacts=root.AddComponent<RotateRingsBombContacts>();contacts.Build(level,physics);
    }
    public static string Run(string destination){
        var report=new Report();
        var mover=new RotateRingsPart{id=1,kind="slider",templateId="native3-1cc50926e27b660340a7",angle=-90};
        var level=new RotateRingsLevel{parts=new List<RotateRingsPart>{mover},bombs=new[]{Bomb(-1,new Vector2(1,0))}};
        Fixture(level,out var root,out var physics,out var contacts);
        try{
            var spent=new HashSet<int>();var removed=new HashSet<int>();var angles=new Dictionary<int,float>{{1,-90}};var slides=new Dictionary<int,float>();var trace=new List<string>();
            physics.MotionSample+=owner=>{slides[owner]=physics.GetSlide(owner);bool fired=contacts.ProbeDuringDrag(owner,spent,removed,id=>{removed.Add(id);physics.ApplyState(angles,removed,slides);contacts.ApplyState(spent,removed);});trace.Add("sample="+slides[owner]+", fired="+fired+", removed="+removed.Contains(owner));};
            var castProbe=new ContactFilter2D();castProbe.NoFilter();castProbe.SetLayerMask(128);castProbe.useTriggers=false;var castBuffer=new RaycastHit2D[32];var castTrace=new List<string>();var moverShape=physics.GetOwnerColliders(1)[0];
            foreach(float x in new[]{0f,.11f,.22f,.33f}){physics.OwnerTransform(1).localPosition=new Vector3(x,0f,0f);Physics2D.SyncTransforms();int hitCount=moverShape.Cast(Vector2.right,castProbe,castBuffer,.12f);var hits=new List<string>();for(int i=0;i<hitCount;i++)hits.Add(castBuffer[i].collider.name+"#L"+castBuffer[i].collider.gameObject.layer+"#T"+castBuffer[i].collider.isTrigger+"#d"+castBuffer[i].distance);castTrace.Add("x="+x+":"+string.Join(",",hits));}
            physics.ApplyState(angles,removed,slides);physics.BeginGesture(1);float reached=0f;bool blocked=false;int frames=0;
            while(frames++<12&&!removed.Contains(1)&&!blocked&&reached<2f){float before=reached;reached=physics.ClampSlideForDeltaTime(1,reached,2f,1f/60f,out blocked);trace.Add("frame="+frames+", from="+before+", to="+reached+", blocked="+blocked+", blocker="+physics.LastBlockerOwner+"/"+physics.LastBlockerShapeIndex+"/"+physics.LastBlockerRole);}
            report.checks.Add(new Check{name="frame-sampled drag reaches bomb and removes moving owner before passing through",passed=removed.Contains(1)&&spent.Contains(0)&&reached<1,observed="frames="+frames+", slide="+reached+", blocked="+blocked+", blocker="+physics.LastBlockerOwner+"/"+physics.LastBlockerShapeIndex+"/"+physics.LastBlockerRole+", ownerRemoved="+removed.Contains(1)+", spent="+spent.Count+", cast="+string.Join(";",castTrace)+", trace="+string.Join(";",trace)});
        }finally{UnityEngine.Object.DestroyImmediate(root);}

        level=new RotateRingsLevel{parts=new List<RotateRingsPart>{new RotateRingsPart{id=1,kind="slider",templateId="native3-1cc50926e27b660340a7",angle=-90}}};
        Fixture(level,out root,out physics,out contacts);
        try{physics.BeginGesture(1);float reached=0f;bool blocked=false;int frames=0;while(frames++<5&&!blocked&&reached<1.6f)reached=physics.ClampSlideForDeltaTime(1,reached,1.6f,1f/60f,out blocked);report.checks.Add(new Check{name="repeated clear slider frames progress without a false physical block",passed=frames==6&&!blocked&&reached>1.4f,observed="frames="+frames+", slide="+reached+", blocked="+blocked});}
        finally{UnityEngine.Object.DestroyImmediate(root);}

        level=new RotateRingsLevel{parts=new List<RotateRingsPart>{new RotateRingsPart{id=1,kind="slider",templateId="native3-1cc50926e27b660340a7",angle=-90}},bombs=new[]{Bomb(1,Vector2.zero)}};
        Fixture(level,out root,out physics,out contacts);
        try{var spent=new HashSet<int>();var removed=new HashSet<int>();bool fired=contacts.ProbeDuringDrag(1,spent,removed,id=>removed.Add(id));report.checks.Add(new Check{name="carried bomb ignores its own host colliders",passed=!fired&&spent.Count==0&&removed.Count==0,observed="fired="+fired});}
        finally{UnityEngine.Object.DestroyImmediate(root);}

        level=new RotateRingsLevel{parts=new List<RotateRingsPart>{new RotateRingsPart{id=1,kind="slider",templateId="native3-1cc50926e27b660340a7",angle=-90},new RotateRingsPart{id=2,kind="fixed",templateId="qa-fixed",x=.3f}},bombs=new[]{Bomb(1,Vector2.zero)}};
        Fixture(level,out root,out physics,out contacts);
        try{var spent=new HashSet<int>();var removed=new HashSet<int>();bool fired=contacts.ProbeDuringDrag(2,spent,removed,id=>removed.Add(id));report.checks.Add(new Check{name="external contact removes external fixture and bomb host",passed=fired&&spent.Contains(0)&&removed.Contains(1)&&removed.Contains(2),observed="fired="+fired+", removed="+removed.Count});}
        finally{UnityEngine.Object.DestroyImmediate(root);}
        string json=JsonUtility.ToJson(report,true);File.WriteAllText(destination,json);return json;
    }
}
#endif

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class OriginalSourceQa {
    [Serializable] sealed class FixtureAudit {public int sourceId,parts,expectedColliders,actualColliders,expectedBodies,actualBodies,activeRelations;public bool geometryPreserved,strictInitialPlacement;public string contacts,error;public List<int> immediatelySolvable=new List<int>();}
    [Serializable] sealed class PortfolioAudit {public string status="diagnostic; not source gameplay parity or solution certification";public List<FixtureAudit> fixtures=new List<FixtureAudit>();}
    [Serializable] sealed class GestureAudit {public int sourceId,part;public float from,requested,reached,settled;public bool blocked,canSolve;public int blocker;public string blockerRole,contacts;}
    static string Root=>Path.GetFullPath(Path.Combine(Application.dataPath,"../../Research/physics_v3"));
    static void Read(out RotateRingsLevelPack pack,out RotateRingsTemplateCatalog catalog){pack=JsonUtility.FromJson<RotateRingsLevelPack>(File.ReadAllText(Path.Combine(Root,"source_parity_pack.json")));catalog=JsonUtility.FromJson<RotateRingsTemplateCatalog>(File.ReadAllText(Path.Combine(Root,"source_templates.json")));}
    public static string AuditFixtures(string destination){
        Read(out var pack,out var catalog);var report=new PortfolioAudit();
        foreach(var level in pack.levels){
            var row=new FixtureAudit{sourceId=level.id,parts=level.parts.Count};report.fixtures.Add(row);
            var root=new GameObject("Source geometry QA"){hideFlags=HideFlags.HideAndDontSave};
            try{
                var physics=root.AddComponent<RotateRingsCollisionWorld>();physics.Build(level,catalog);
                row.geometryPreserved=true;
                foreach(var part in level.parts){
                    var nativeBody=part.sourceRigidbody;var actualBody=physics.OwnerTransform(part.id).GetComponent<Rigidbody2D>();
                    if(nativeBody!=null&&nativeBody.present){row.expectedBodies++;if(actualBody==null)row.geometryPreserved=false;else if((int)actualBody.bodyType!=nativeBody.bodyType||actualBody.simulated!=nativeBody.simulated||(int)actualBody.constraints!=nativeBody.constraints||Mathf.Abs(actualBody.gravityScale-nativeBody.gravityScale)>.00001f||Mathf.Abs(actualBody.mass-nativeBody.mass)>.00001f||actualBody.useFullKinematicContacts!=nativeBody.useFullKinematicContacts)row.geometryPreserved=false;}
                    if(actualBody!=null){row.actualBodies++;if(nativeBody!=null&&!nativeBody.present)row.geometryPreserved=false;}
                    var template=Array.Find(catalog.templates,t=>t.id==part.templateId);row.expectedColliders+=template.physicalShapes.Length;
                    var actual=physics.GetOwnerColliders(part.id);row.actualColliders+=actual.Count;
                    if(actual.Count!=template.physicalShapes.Length){row.geometryPreserved=false;continue;}
                    for(int i=0;i<actual.Count;i++){
                        var expected=template.physicalShapes[i];var collider=actual[i];
                        if(collider.isTrigger!=expected.trigger||collider.gameObject.layer!=expected.sourceLayer||collider.enabled!=expected.sourceEnabled)row.geometryPreserved=false;
                        if(expected.shapeType=="circle"){
                            var circle=collider as CircleCollider2D;if(circle==null||Mathf.Abs(circle.radius-expected.radius)>.00001f||Vector2.Distance(circle.offset,expected.center)>.00001f)row.geometryPreserved=false;
                        }else if(expected.sourceColliderType=="BoxCollider2D"){
                            var box=collider as BoxCollider2D;
                            var wanted=expected.points;
                            if(box==null||wanted==null||wanted.Length!=4){row.geometryPreserved=false;continue;}
                            var actualCorners=new[]{new Vector2(-.5f,-.5f),new Vector2(.5f,-.5f),new Vector2(.5f,.5f),new Vector2(-.5f,.5f)};
                            foreach(var corner in actualCorners){var local=physics.OwnerTransform(part.id).InverseTransformPoint(box.transform.TransformPoint(box.offset+Vector2.Scale(corner,box.size)));float nearest=float.MaxValue;foreach(var v in wanted)nearest=Mathf.Min(nearest,Vector2.Distance(local,v));if(nearest>.00001f)row.geometryPreserved=false;}
                        }else{
                            var polygon=collider as PolygonCollider2D;int count=expected.paths!=null&&expected.paths.Length>0?expected.paths.Length:1;
                            if(polygon==null||polygon.pathCount!=count){row.geometryPreserved=false;continue;}
                            for(int path=0;path<count;path++){var points=polygon.GetPath(path);var wanted=expected.paths!=null&&expected.paths.Length>0?expected.paths[path].points:expected.points;if(points.Length!=wanted.Length){row.geometryPreserved=false;continue;}for(int v=0;v<points.Length;v++)if(Vector2.Distance(points[v],wanted[v])>.00001f)row.geometryPreserved=false;}
                        }
                    }
                    if(physics.CanSolve(part.id))row.immediatelySolvable.Add(part.id);
                }
                row.activeRelations=physics.ActiveAuthoredContactCount;row.contacts=physics.GetActiveContactSummary();row.strictInitialPlacement=physics.InitialPlacementValid();
            }catch(Exception e){row.error=e.ToString();row.geometryPreserved=false;}
            finally{UnityEngine.Object.DestroyImmediate(root);}
        }
        File.WriteAllText(destination,JsonUtility.ToJson(report,true));return JsonUtility.ToJson(report,true);
    }
    public static string ProbeGesture(int sourceId,int partId,float amount,string destination){
        Read(out var pack,out var catalog);var level=pack.levels.Find(l=>l.id==sourceId);var part=level.parts.Find(p=>p.id==partId);
        var root=new GameObject("Source motion QA"){hideFlags=HideFlags.HideAndDontSave};
        try{
            var physics=root.AddComponent<RotateRingsCollisionWorld>();physics.Build(level,catalog);physics.BeginGesture(partId);
            var report=new GestureAudit{sourceId=sourceId,part=partId,from=part.kind=="slider"?0:part.angle,requested=amount};
            if(part.kind=="slider")report.reached=physics.ClampSlide(partId,0,amount,out report.blocked);else report.reached=physics.ClampRotation(partId,part.angle,amount,out report.blocked);
            physics.EndGesture(partId);report.settled=part.kind=="slider"?physics.GetSlide(partId):physics.GetAngle(partId);report.canSolve=physics.CanSolve(partId);report.contacts=physics.GetActiveContactSummary();report.blocker=physics.LastBlockerOwner;report.blockerRole=physics.LastBlockerRole;
            string json=JsonUtility.ToJson(report,true);if(!string.IsNullOrEmpty(destination))File.WriteAllText(destination,json);return json;
        }finally{UnityEngine.Object.DestroyImmediate(root);}
    }
}
#endif

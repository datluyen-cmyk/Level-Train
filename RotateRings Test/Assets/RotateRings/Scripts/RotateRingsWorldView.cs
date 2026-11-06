using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class RotateRingsWorldView : MonoBehaviour {
    readonly Dictionary<int,PartView> parts=new Dictionary<int,PartView>();
    readonly List<BombVisual> bombs=new List<BombVisual>();
    float enteringUntil;
    public bool IsEntering=>Time.unscaledTime<enteringUntil;
    static readonly string[] Palette={"32AFDD","EC8F2E","984BE0","DD433E","6AC53E","4E77DD","23A468"};
    public static Color PartColor(int n){Color c;ColorUtility.TryParseHtmlString("#"+Palette[Mathf.Abs(n)%Palette.Length],out c);return c;}
    public void Build(RotateRingsLevel level,RotateRingsTemplateCatalog catalog){
        for(int i=transform.childCount-1;i>=0;i--){var child=transform.GetChild(i);if(Application.isPlaying)Destroy(child.gameObject);else DestroyImmediate(child.gameObject);}parts.Clear();bombs.Clear();
        var templates=new Dictionary<string,RotateRingsOriginalTemplate>();
        if(catalog==null||catalog.templates==null)throw new InvalidOperationException("Original piece catalog missing.");
        foreach(var template in catalog.templates)templates.Add(template.id,template);
        var ordered=new List<RotateRingsPart>(level.parts);ordered.Sort((a,b)=>b.y.CompareTo(a.y));
        float interval=RotateRingsOriginalMotion.Stagger(ordered.Count);
        enteringUntil=Time.unscaledTime+RotateRingsOriginalMotion.EntryDuration+Mathf.Max(0,ordered.Count-1)*interval;
        for(int i=0;i<ordered.Count;i++){
            var part=ordered[i];
            if(string.IsNullOrEmpty(part.templateId)||!templates.TryGetValue(part.templateId,out var template))throw new InvalidOperationException("Unverified piece template: "+part.id);
            var go=new GameObject("Part "+part.id+" ("+part.kind+")");go.transform.SetParent(transform,false);go.transform.localPosition=new Vector3(part.x,part.y,part.z);
            var view=go.AddComponent<PartView>();view.Initialize(part,template,i*interval);parts.Add(part.id,view);
        }
        if(level.bombs!=null)for(int i=0;i<level.bombs.Length;i++){
            var definition=level.bombs[i];var parent=definition.ownerPartId>=0?parts[definition.ownerPartId].transform:transform;
            var go=new GameObject(definition.sourceName);go.transform.SetParent(parent,false);
            var visual=go.AddComponent<BombVisual>();visual.Initialize(definition);bombs.Add(visual);
        }
    }
    public Bounds GeometryBounds(){
        Bounds bounds=new Bounds();bool first=true;
        foreach(var view in parts.Values){var local=view.MeshBounds;var p=view.Part;var pose=Matrix4x4.TRS(new Vector3(p.x,p.y,0),Quaternion.Euler(0,0,p.angle),Vector3.one);
            foreach(float x in new[]{local.min.x,local.max.x})foreach(float y in new[]{local.min.y,local.max.y}){var point=pose.MultiplyPoint3x4(new Vector3(x,y,0));if(first){bounds=new Bounds(point,Vector3.zero);first=false;}else bounds.Encapsulate(point);}}
        return bounds;
    }
    public bool TryHit(Vector2 point,out int id){float best=float.MaxValue;id=0;foreach(var pair in parts){if(!pair.Value.Active||!pair.Value.HitScore(point,out float score)||score>=best)continue;best=score;id=pair.Key;}return best<float.MaxValue;}
    public bool RingHit(Vector2 point,out int id,out Vector2 center){foreach(var pair in parts){var v=pair.Value;if(!v.Active||v.IsSlider)continue;if(v.HitScore(point,out float score)){id=pair.Key;center=v.transform.position;return true;}}id=0;center=default;return false;}
    public void ApplyState(Dictionary<int,float> angles,HashSet<int> removed,HashSet<int> keys,List<bool> activeLinks,bool wobble=false,int wobbleId=0){
        // Contact states affect game logic, never the physical piece mesh or its attachments.
        foreach(var pair in parts){var p=pair.Value.Part;pair.Value.Apply(angles.TryGetValue(p.id,out float a)?a:p.angle,removed.Contains(p.id),wobble&&wobbleId==p.id);}
    }
    public Vector2 SlideAxis(int id)=>parts.TryGetValue(id,out var p)?p.SlideAxis:Vector2.right;
    public float Radius(int id)=>parts.TryGetValue(id,out var p)?Mathf.Max(p.Part.radius,.25f):.5f;
    public void SetHint(int id){foreach(var pair in parts)pair.Value.SetSelected(pair.Key==id);}
    public void SetSliderOffset(int id,float offset){if(parts.TryGetValue(id,out var p)&&p.IsSlider)p.SetSlideOffset(offset);}
    public void BeginRelease(int id){if(parts.TryGetValue(id,out var p))p.BeginRelease();}
    public void ApplyChainState(Dictionary<int,int> counts){foreach(var pair in parts)if(counts.TryGetValue(pair.Key,out int count))pair.Value.SetChainCount(count);}
    public void SetBombState(HashSet<int> removed,HashSet<int> removedOwners){for(int i=0;i<bombs.Count;i++)bombs[i].SetVisible(!removed.Contains(i)&&!removedOwners.Contains(bombs[i].Owner));}
    sealed class BombVisual:MonoBehaviour {
        public int Owner{get;private set;}
        readonly List<Mesh> meshes=new List<Mesh>();readonly List<Material> materials=new List<Material>();
        public void Initialize(RotateRingsBomb definition){
            Owner=definition.ownerPartId;
            if(definition.catchVisual==null||definition.catchVisual.quads==null)throw new InvalidOperationException("Missing original bomb visual: "+definition.sourceName);
            foreach(var quad in definition.catchVisual.quads){var go=new GameObject("Original bomb quad");go.transform.SetParent(transform,false);OriginalCatchVisualAssets.ApplyLocalPose(go.transform,quad);var material=OriginalSourceMaterialAssets.Resolve(quad.sourceMaterial);var mesh=OriginalCatchVisualAssets.CreateCatchMesh(quad,material);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;meshes.Add(mesh);materials.Add(material);}
        }
        public void SetVisible(bool visible){gameObject.SetActive(visible);}
        void OnDestroy(){foreach(var m in meshes)if(m!=null){if(Application.isPlaying)Destroy(m);else DestroyImmediate(m);}foreach(var m in materials)if(m!=null){if(Application.isPlaying)Destroy(m);else DestroyImmediate(m);}}
    }
    sealed class PartView:MonoBehaviour{
        public RotateRingsPart Part{get;private set;}
        public bool Active{get;private set;}
        public bool IsSlider=>RotateRingsRules.IsSlider(Part);
        public float CurrentAngle{get;private set;}
        public Bounds MeshBounds=>visualBounds;
        Bounds visualBounds;
        Vector2 slideAxis;
        const float ChainBreakIntervalSeconds=.17f;
        GameObject chainRoot;float chainOuterRadius,chainGap;int chainCount=-1,chainSlotOffset,chainBreakSlot;readonly int[] chainSegmentVersions=new int[3];
        readonly Coroutine[] chainBreakRoutines=new Coroutine[3];
        readonly List<MeshRenderer> extraRenderers=new List<MeshRenderer>();
        readonly List<Mesh> extraMeshes=new List<Mesh>();
        readonly List<Material> extraMaterials=new List<Material>();
        Mesh mesh;MeshRenderer body;Material material;Vector3 home;float entryStart,wobbleStart=-10;bool selected,released;GameObject shatter;
        public void Initialize(RotateRingsPart part,RotateRingsOriginalTemplate template,float delay){
            Part=part;CurrentAngle=part.angle;home=transform.localPosition;entryStart=Time.unscaledTime+delay;
            if(IsSlider)slideAxis=RotateRingsRules.SlideAxis(part,template);
            if(IsSlider){
                mesh=OriginalPullVisualAssets.CreatePullMesh(template.pullVisual,PartColor(part.color));
                material=OriginalSourceMaterialAssets.Resolve(template.pullVisual.sourceMaterial);
                OriginalPullVisualAssets.ApplyPullMaterialSettings(material,template.pullVisual);
                gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;body=gameObject.AddComponent<MeshRenderer>();body.sharedMaterial=material;
                visualBounds=mesh.bounds;transform.localScale=Vector3.one*RotateRingsOriginalMotion.InitialWaitingScale;return;
            }
            if(part.kind=="fixed"||part.kind=="butterfly"){
                if(template.catchVisual==null||template.catchVisual.quads==null||template.catchVisual.quads.Length==0)throw new InvalidOperationException("Missing source catch visual "+part.templateId);
                bool first=true;
                foreach(var quad in template.catchVisual.quads){
                    var child=new GameObject("Original "+quad.sourceClass);child.transform.SetParent(transform,false);
                    child.transform.localPosition=quad.localPosition;child.transform.localRotation=Quaternion.Euler(quad.localEuler);child.transform.localScale=quad.localScale;
                    var mat=OriginalSourceMaterialAssets.Resolve(quad.sourceMaterial);var m=OriginalCatchVisualAssets.CreateCatchMesh(quad,mat);
                    child.AddComponent<MeshFilter>().sharedMesh=m;var renderer=child.AddComponent<MeshRenderer>();renderer.sharedMaterial=mat;
                    var local=m.bounds;var pose=Matrix4x4.TRS(quad.localPosition,Quaternion.Euler(quad.localEuler),quad.localScale);
                    foreach(float x in new[]{local.min.x,local.max.x})foreach(float y in new[]{local.min.y,local.max.y})foreach(float z in new[]{local.min.z,local.max.z}){var v=pose.MultiplyPoint3x4(new Vector3(x,y,z));if(first){visualBounds=new Bounds(v,Vector3.zero);first=false;}else visualBounds.Encapsulate(v);}
                    if(body==null){body=renderer;mesh=m;material=mat;}else{extraRenderers.Add(renderer);extraMeshes.Add(m);extraMaterials.Add(mat);}
                }
                transform.localScale=Vector3.one*RotateRingsOriginalMotion.InitialWaitingScale;return;
            }
            material=OriginalSourceMaterialAssets.Resolve(template.sourceMaterial);
            // Native controller owns cap lengths; body tiling belongs to the authored material.
            float tile=template.bodyTileLength>0?template.bodyTileLength:material.GetFloat("_BodyTileLength");
            float tail=template.tailLength>0?template.tailLength:template.visualWidth;
            float head=template.headLength>0?template.headLength:template.visualWidth;
            mesh=OriginalRingVisualAssets.CreatePieceMesh(template.visualRadius,template.visualWidth,template.visualGap,template.attachments,template.sourceVertexColor,template.gapDirectionDeg,template.capRatio,template.extraEnabled,tail,head,tile,template.styleIndex,template.arcSubdivisions,template.ringZ,template.stemZ,template.crossBarZ);
            gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;body=gameObject.AddComponent<MeshRenderer>();body.sharedMaterial=material;
            if(material.HasProperty("_HeadLength"))material.SetFloat("_HeadLength",head);
            if(material.HasProperty("_TailLength"))material.SetFloat("_TailLength",tail);
            if(material.HasProperty("_ExtraEnable"))material.SetFloat("_ExtraEnable",template.extraEnabled?1:0);
            if(material.HasProperty("_StyleIndexFromMesh"))material.SetFloat("_StyleIndexFromMesh",1);
            if(material.HasProperty("_StyleIndex"))material.SetFloat("_StyleIndex",template.styleIndex);
            visualBounds=mesh.bounds;
            chainOuterRadius=(template.radius+template.width*.5f)*24f;chainGap=template.gap;SetChainCount(part.chainLockCount);
            transform.localScale=Vector3.one*RotateRingsOriginalMotion.InitialWaitingScale;
        }
        public Vector2 SlideAxis=>slideAxis;
        public void SetChainCount(int count){
            if(Part.kind!="ring"||chainCount==count)return;
            int previous=chainCount;chainCount=count;
            if(chainRoot==null){
                if(count<=0)return;
                chainSlotOffset=count==2?1:0;
                chainRoot=OriginalChainVisualAssets.CreateOrUpdateChainVisual(transform,null,new OriginalChainVisualAssets.Geometry(chainOuterRadius,chainGap,CurrentAngle),count);
                chainRoot.SetActive(Active);
                return;
            }
            if(count<previous&&previous>0&&(previous<=3||count<=0)){
                // The source stores instances in their authored order and breaks instance[newLockCount].
                // A chain initialized at two locks contains the authored second and third directions.
                chainRoot.SetActive(true);
                for(int sourceSlot=Mathf.Min(previous,3)-1;sourceSlot>=Mathf.Max(count,0);--sourceSlot){
                    int visualSlot=sourceSlot+chainSlotOffset;
                    Transform segment=OriginalChainVisualAssets.GetChainSegment(chainRoot,visualSlot);
                    if(segment==null)continue;
                    int version=++chainSegmentVersions[visualSlot];
                    if(chainBreakRoutines[visualSlot]!=null){StopCoroutine(chainBreakRoutines[visualSlot]);chainBreakRoutines[visualSlot]=null;}
                    float delay=chainBreakSlot++*ChainBreakIntervalSeconds;
                    chainBreakRoutines[visualSlot]=StartCoroutine(PlayAndHideBrokenChainSegment(segment,visualSlot,version,delay));
                }
                return;
            }
            // Undo or a newly acquired lock restores the source set; CreateOrUpdate reuses its Spine instances.
            if(count>previous&&previous<=0)chainSlotOffset=count==2?1:0;
            if(count>=3)chainSlotOffset=0;
            bool[] wasActive=new bool[3];
            for(int slot=0;slot<3;slot++){Transform segment=OriginalChainVisualAssets.GetChainSegment(chainRoot,slot);wasActive[slot]=segment!=null&&segment.gameObject.activeSelf;}
            chainRoot=OriginalChainVisualAssets.CreateOrUpdateChainVisual(transform,chainRoot,new OriginalChainVisualAssets.Geometry(chainOuterRadius,chainGap,CurrentAngle),count);
            chainRoot.SetActive(Active);
            for(int slot=0;slot<3;slot++){
                Transform segment=OriginalChainVisualAssets.GetChainSegment(chainRoot,slot);
                if(segment!=null&&segment.gameObject.activeSelf){
                    ++chainSegmentVersions[slot];
                    if(!wasActive[slot])OriginalChainVisualAssets.PlayChainAppear(segment.gameObject);
                }
                if(chainBreakRoutines[slot]!=null){StopCoroutine(chainBreakRoutines[slot]);chainBreakRoutines[slot]=null;}
            }
        }
        System.Collections.IEnumerator PlayAndHideBrokenChainSegment(Transform segment,int slot,int version,float delay){
            if(delay>0f)yield return new WaitForSeconds(delay);
            if(segment==null||chainSegmentVersions[slot]!=version){chainBreakRoutines[slot]=null;yield break;}
            float duration=OriginalChainVisualAssets.PlayChainBreak(segment.gameObject);
            if(duration>0f)yield return new WaitForSeconds(duration);
            if(segment!=null&&chainSegmentVersions[slot]==version)segment.gameObject.SetActive(false);
            chainBreakRoutines[slot]=null;
            if(chainCount<=0&&chainRoot!=null&&NoPendingChainBreaks())chainRoot.SetActive(false);
        }
        bool NoPendingChainBreaks(){for(int i=0;i<chainBreakRoutines.Length;i++)if(chainBreakRoutines[i]!=null)return false;return true;}
        public void SetSlideOffset(float offset){transform.localPosition=home+(Vector3)slideAxis*offset;}
        public void Apply(float angle,bool removed,bool wobble){
            if(!removed&&released){released=false;if(shatter!=null)Destroy(shatter);}Active=!removed;CurrentAngle=angle;body.enabled=Active;foreach(var renderer in extraRenderers)renderer.enabled=Active;if(chainRoot!=null)chainRoot.SetActive(Active||(chainCount<=0&&!NoPendingChainBreaks()));
            OriginalRingVisualAssets.ApplySelectionOutline(material,Active&&selected);
            if(wobble&&Time.unscaledTime-wobbleStart>=RotateRingsOriginalMotion.WobbleDuration){wobbleStart=Time.unscaledTime;if(chainRoot!=null&&chainCount>0)OriginalChainVisualAssets.PlayChainShake(chainRoot);}
            ApplyPose();
        }
        public void BeginRelease(){if(released)return;released=true;body.enabled=false;foreach(var renderer in extraRenderers)renderer.enabled=false;OriginalRingVisualAssets.ApplySelectionOutline(material,false);shatter=RotateRingsOriginalMotion.PlayShatter(mesh,transform,PartColor(Part.color));}
        public void SetSelected(bool value){selected=value;OriginalRingVisualAssets.ApplySelectionOutline(material,Active&&value);}
        void Update(){ApplyPose();}
        void ApplyPose(){float entry=Mathf.Clamp01((Time.unscaledTime-entryStart)/RotateRingsOriginalMotion.EntryDuration);transform.localScale=!Application.isPlaying?Vector3.one:Vector3.one*(Time.unscaledTime<entryStart?RotateRingsOriginalMotion.InitialWaitingScale:RotateRingsOriginalMotion.EntryScale(entry));float wobble=(Time.unscaledTime-wobbleStart)/RotateRingsOriginalMotion.WobbleDuration;transform.localRotation=Quaternion.Euler(0,0,CurrentAngle+(wobble>=0&&wobble<1?RotateRingsOriginalMotion.WobbleAngle(wobble):0));}
        public bool HitScore(Vector2 point,out float score){
            score=float.MaxValue;Vector2 d=point-(Vector2)transform.position;
            float error=Mathf.Abs(d.magnitude-Part.radius);float a=Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg;
            if(Part.kind=="ring"&&error<=Mathf.Max(.22f,Part.width*.8f)&&(Part.gap<=0||Mathf.Abs(Mathf.DeltaAngle(CurrentAngle,a))>Part.gap*.5f)){score=error;return true;}
            Vector2 local=transform.InverseTransformPoint(point);var vertices=mesh.vertices;var triangles=mesh.triangles;
            for(int i=0;i<triangles.Length;i+=3)if(InTriangle(local,vertices[triangles[i]],vertices[triangles[i+1]],vertices[triangles[i+2]])){score=.18f;return true;}
            return false;
        }
        static bool InTriangle(Vector2 p,Vector2 a,Vector2 b,Vector2 c){float ab=Cross(b-a,p-a),bc=Cross(c-b,p-b),ca=Cross(a-c,p-c);return (ab>=0&&bc>=0&&ca>=0)||(ab<=0&&bc<=0&&ca<=0);}
        static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
        static void Dispose(UnityEngine.Object value){if(value==null)return;if(Application.isPlaying)Destroy(value);else DestroyImmediate(value);}
        void OnDestroy(){Dispose(shatter);Dispose(mesh);Dispose(material);foreach(var m in extraMeshes)Dispose(m);foreach(var mat in extraMaterials)Dispose(mat);}
    }
}

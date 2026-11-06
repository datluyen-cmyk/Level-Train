using System;

using System.Collections.Generic;

using UnityEngine;

using UnityEngine.EventSystems;

using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM

using UnityEngine.InputSystem;

#endif



public sealed class RotateRingsRuntime : MonoBehaviour {

    enum ScreenMode { Home, Levels, Game, Win }

    sealed class State { public Dictionary<int,float> angles=new Dictionary<int,float>(); public HashSet<int> removed=new HashSet<int>(); public HashSet<int> keys=new HashSet<int>(); public Dictionary<int,float> slides=new Dictionary<int,float>(); public Dictionary<int,int> chainLocks=new Dictionary<int,int>(); public HashSet<int> spentBombs=new HashSet<int>(); public object bindingSnapshot; public int moves; public State Copy(){var s=new State{moves=moves,chainLocks=new Dictionary<int,int>(chainLocks),spentBombs=new HashSet<int>(spentBombs),bindingSnapshot=bindingSnapshot,slides=new Dictionary<int,float>(slides),angles=new Dictionary<int,float>(angles),removed=new HashSet<int>(removed),keys=new HashSet<int>(keys)};return s;} }

    sealed class Drag { public int id; public bool slider; public bool beganLinked; public bool bombTriggered; public Vector2 startWorld,lastWorld; public float startAngle,startSlide,lastPointerAngle,delta,blockDirection; public bool valid; }

    static readonly Color Cream=new Color(1f,.99490005f,.95754719f), Ink=new Color(.30f,.20f,.14f), Brown=new Color(.56f,.34f,.21f);

    RotateRingsTemplateCatalog templates; RotateRingsCollisionWorld collision; RotateRingsBombContacts bombContacts; RotateRingsInputWorld inputWorld; RotateRingsLevelPack pack; RotateRingsLevel level; State state; RotateRingsWorldView world; Camera gameCamera;

    Canvas canvas; RectTransform uiRoot; ScreenMode mode; Stack<State> undo=new Stack<State>(); Drag drag; int levelIndex; int maxUnlocked; bool referenceQa; bool trainingPreview; bool trainingPreviewUnlockAll; bool soundOn=true; float winAt=-1f; bool hammerMode;

    Text titleText,movesText,toastText; RotateRingsOriginalHud originalHud; GameObject panel; AudioSource music, sfx; readonly Dictionary<string,Sprite> artCache=new Dictionary<string,Sprite>();



    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void AutoStart() {

        if (FindFirstObjectByType<RotateRingsRuntime>()!=null) return;

        var root=new GameObject("Rotate Rings Runtime");root.AddComponent<RotateRingsRuntime>();

    }

    void Awake(){Application.targetFrameRate=60;maxUnlocked=Mathf.Max(0,PlayerPrefs.GetInt("rr_max_level",0));CreateCamera();CreateWorld();CreateCanvas();LoadPack();if(pack!=null&&pack.levels.Count>0)StartLevel(pack.levels[Mathf.Clamp(maxUnlocked,0,pack.levels.Count-1)].id);else ShowError("LEVEL PACK UNDER REVIEW","The ring-only pack is withdrawn while original shapes and mechanics are restored.");LoadAudio();}

    void CreateCamera(){gameCamera=Camera.main;if(gameCamera==null){var go=new GameObject("Main Camera");go.tag="MainCamera";gameCamera=go.AddComponent<Camera>();}gameCamera.orthographic=true;gameCamera.orthographicSize=8;gameCamera.transform.position=new Vector3(0,0,-20);gameCamera.backgroundColor=Cream;gameCamera.clearFlags=CameraClearFlags.SolidColor;gameCamera.allowHDR=false;}

    void CreateWorld(){var go=new GameObject("Puzzle Board");world=go.AddComponent<RotateRingsWorldView>();var physical=new GameObject("Original Collision Geometry");collision=physical.AddComponent<RotateRingsCollisionWorld>();bombContacts=physical.AddComponent<RotateRingsBombContacts>();collision.MotionSample+=OnMotionSample;inputWorld=physical.AddComponent<RotateRingsInputWorld>();}

    void CreateCanvas(){var go=new GameObject("Rotate Rings UI");canvas=go.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=20;var scaler=go.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1080,2160);scaler.matchWidthOrHeight=.45f;go.AddComponent<GraphicRaycaster>();uiRoot=go.GetComponent<RectTransform>();if(EventSystem.current==null){var e=new GameObject("EventSystem");e.AddComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM

            e.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

#else

            e.AddComponent<StandaloneInputModule>();

#endif

        }}

    void LoadPack(){

        var file=Resources.Load<TextAsset>("Levels/generated_levels");var catalog=Resources.Load<TextAsset>("Levels/physics_templates");

        if(file==null||catalog==null)return;

        try{templates=JsonUtility.FromJson<RotateRingsTemplateCatalog>(catalog.text);pack=JsonUtility.FromJson<RotateRingsLevelPack>(file.text);if(pack==null||pack.schemaVersion!=3||!pack.complete||pack.sourceOriginalOnly||pack.levels==null||templates==null)pack=null;else pack.levels.Sort((a,b)=>a.id.CompareTo(b.id));}

        catch(Exception e){Debug.LogError("Could not load verified original-template pack: "+e);pack=null;}

    }

#if UNITY_EDITOR
    public void LoadOriginalReferenceForQa(int sourceId){
        string folder=System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../../Research/physics_v3"));
        templates=JsonUtility.FromJson<RotateRingsTemplateCatalog>(System.IO.File.ReadAllText(System.IO.Path.Combine(folder,"source_templates.json")));
        pack=JsonUtility.FromJson<RotateRingsLevelPack>(System.IO.File.ReadAllText(System.IO.Path.Combine(folder,"source_parity_pack.json")));
        referenceQa=true;trainingPreview=false;StartLevel(sourceId);
    }

    public void SetTrainingPreviewUnlockAllForQa(bool unlocked){trainingPreviewUnlockAll=unlocked;}
    public int TrainingPreviewLevelIdForQa => trainingPreview&&level!=null?level.id:-1;

    public void LoadTrainingPreviewForQa(string packPath,string templatesPath,int levelId){
        if(string.IsNullOrWhiteSpace(packPath)||string.IsNullOrWhiteSpace(templatesPath))throw new ArgumentException("Training preview paths are required.");
        var nextTemplates=JsonUtility.FromJson<RotateRingsTemplateCatalog>(System.IO.File.ReadAllText(System.IO.Path.GetFullPath(templatesPath)));
        var nextPack=JsonUtility.FromJson<RotateRingsLevelPack>(System.IO.File.ReadAllText(System.IO.Path.GetFullPath(packPath)));
        if(nextPack==null||nextPack.schemaVersion!=3||nextPack.sourceOriginalOnly||nextPack.levels==null||nextPack.levels.Count==0)throw new System.IO.InvalidDataException("Training preview requires a schema-3 pack with levels and sourceOriginalOnly=false.");
        if(nextTemplates==null||nextTemplates.schemaVersion!=3||nextTemplates.templates==null||nextTemplates.templates.Length==0)throw new System.IO.InvalidDataException("Training preview requires a schema-3 physics template catalog.");
        var templateIds=new HashSet<string>();foreach(var t in nextTemplates.templates)if(t!=null&&!string.IsNullOrEmpty(t.id))templateIds.Add(t.id);
        var levelIds=new HashSet<int>();
        foreach(var item in nextPack.levels){
            if(item==null||!levelIds.Add(item.id)||item.sourceOnlyParityFixture||item.parts==null||item.parts.Count==0)throw new System.IO.InvalidDataException("Every preview level needs a unique id, playable parts, and sourceOnlyParityFixture=false.");
            var partIds=new HashSet<int>();foreach(var part in item.parts)if(part==null||!partIds.Add(part.id)||string.IsNullOrEmpty(part.templateId)||!templateIds.Contains(part.templateId))throw new System.IO.InvalidDataException("Preview level "+item.id+" has a missing template or duplicate part id.");
        }
        if(!levelIds.Contains(levelId))throw new ArgumentException("Training preview level id not found: "+levelId);
        templates=nextTemplates;pack=nextPack;referenceQa=false;trainingPreview=true;
        int selectedIndex=pack.levels.FindIndex(l=>l.id==levelId);maxUnlocked=trainingPreviewUnlockAll?pack.levels.Count-1:selectedIndex;
        StartLevel(levelId);
    }
#endif

    void LoadAudio(){music=gameObject.AddComponent<AudioSource>();music.loop=true;music.volume=.24f;var clip=Resources.Load<AudioClip>("Audio/rr_bgm_level2");if(clip!=null){music.clip=clip;music.Play();}sfx=gameObject.AddComponent<AudioSource>();sfx.volume=.52f;}

    void Update(){if(mode!=ScreenMode.Game)return;if(winAt>=0f){if(Time.unscaledTime>=winAt)ShowWin();return;}if(!world.IsEntering)PollInput();}

    void ClearUI(){if(panel!=null)Destroy(panel);if(toastText!=null)Destroy(toastText.gameObject);panel=null;originalHud=null;toastText=null;titleText=null;movesText=null;}

    GameObject MakePanel(string name,Color color,RectTransform parent=null){var g=new GameObject(name,typeof(RectTransform),typeof(Image));g.transform.SetParent(parent??uiRoot,false);var i=g.GetComponent<Image>();i.color=color;i.raycastTarget=false;return g;}

    RectTransform Rect(GameObject go)=>go.GetComponent<RectTransform>();

    void Full(GameObject go){var r=Rect(go);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;}

    Text Label(string value,Transform parent,Vector2 anchor,Vector2 size,int fontSize,Color color,TextAnchor align=TextAnchor.MiddleCenter,FontStyle style=FontStyle.Bold){var go=new GameObject("Text",typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);var t=go.GetComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=value;t.fontSize=fontSize;t.color=color;t.alignment=align;t.fontStyle=style;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;t.raycastTarget=false;var r=Rect(go);r.anchorMin=r.anchorMax=anchor;r.sizeDelta=size;return t;}

    Button Btn(string text,Transform parent,Vector2 anchor,Vector2 size,Action click,Color? color=null,Sprite sprite=null){var go=new GameObject("Button - "+text,typeof(RectTransform),typeof(Image),typeof(Button));go.transform.SetParent(parent,false);var im=go.GetComponent<Image>();im.color=color??new Color(.99f,.76f,.20f);im.sprite=sprite??Art("home_entry_bg02");im.type=Image.Type.Simple;var b=go.GetComponent<Button>();b.targetGraphic=im;b.onClick.AddListener(()=>click?.Invoke());var r=Rect(go);r.anchorMin=r.anchorMax=anchor;r.sizeDelta=size;var label=Label(text,go.transform,new Vector2(.5f,.5f),size*.88f,Mathf.Clamp((int)(size.y*.32f),22,48),Ink);label.raycastTarget=false;return b;}

    Sprite Art(string prefix){if(artCache.TryGetValue(prefix,out var cached))return cached;string alias=prefix;switch(prefix){case "btn_level_yellow":alias="level_button";break;case "home_entry_bg02":alias="home_entry_button";break;case "home_entry_bg01":alias="home_entry_round_button";break;}var all=Resources.LoadAll<Texture2D>("Art");foreach(var t in all)if(t.name.IndexOf(alias,StringComparison.OrdinalIgnoreCase)>=0){var s=Sprite.Create(t,new Rect(0,0,t.width,t.height),new Vector2(.5f,.5f),100);artCache[prefix]=s;return s;}return null;}

    void SetSafeBoard(){

        if(level==null)return;

        var bounds=world.GeometryBounds();float minX=bounds.min.x,maxX=bounds.max.x,minY=bounds.min.y,maxY=bounds.max.y;

        float h=Mathf.Max(2,maxY-minY),w=Mathf.Max(2,maxX-minX);

        float aspect=(float)Screen.width/Mathf.Max(1,Screen.height);

        // Original RingRect: horizontal inset 40, bottom 420, top 1904 at 1080 x 2160.

        var rect=new Rect(40f/1080f,420f/2160f,1000f/1080f,1484f/2160f);
        if(aspect>1f)rect=new Rect(rect.x,.30f,rect.width,.52f);

        float size=Mathf.Max(h/(2*rect.height),w/(2*Mathf.Max(aspect,.2f)*rect.width))*1.04f;

        gameCamera.orthographicSize=size;

        gameCamera.transform.position=new Vector3((minX+maxX)*.5f-(rect.center.x-.5f)*2*size*aspect,(minY+maxY)*.5f-(rect.center.y-.5f)*2*size,-20);

    }



    void ShowHome(){mode=ScreenMode.Home;world.gameObject.SetActive(false);ClearUI();panel=MakePanel("Home Screen",Cream);Full(panel);Label("ROTATE RINGS",panel.transform,new Vector2(.5f,.83f),new Vector2(900,150),72,Ink);Label("Untangle the colorful rings",panel.transform,new Vector2(.5f,.76f),new Vector2(850,80),33,Brown,TextAnchor.MiddleCenter,FontStyle.Normal);

        Btn("PLAY",panel.transform,new Vector2(.5f,.59f),new Vector2(460,145),()=>StartLevel(pack.levels[Mathf.Clamp(maxUnlocked,0,pack.levels.Count-1)].id),new Color(.99f,.78f,.26f),Art("home_entry_bg02"));

        Btn("CHOOSE LEVEL",panel.transform,new Vector2(.5f,.47f),new Vector2(470,118),ShowLevels,new Color(1f,.89f,.69f),Art("home_entry_bg02"));

        Label("300 puzzles  •  progress saved",panel.transform,new Vector2(.5f,.20f),new Vector2(880,70),25,new Color(.53f,.43f,.34f),TextAnchor.MiddleCenter,FontStyle.Normal);gameCamera.backgroundColor=Cream;}

    void ShowLevels(){mode=ScreenMode.Levels;world.gameObject.SetActive(false);ClearUI();panel=MakePanel("Level Select",Cream);Full(panel);Label("CHOOSE A LEVEL",panel.transform,new Vector2(.5f,.91f),new Vector2(820,100),52,Ink);Btn("‹  HOME",panel.transform,new Vector2(.15f,.91f),new Vector2(180,82),ShowHome,new Color(1f,.89f,.69f));

        var scrollGo=new GameObject("Level Grid",typeof(RectTransform),typeof(ScrollRect),typeof(Image),typeof(RectMask2D));scrollGo.transform.SetParent(panel.transform,false);var sr=scrollGo.GetComponent<ScrollRect>();var ir=scrollGo.GetComponent<Image>();ir.color=new Color(1,1,1,.01f);var srt=Rect(scrollGo);srt.anchorMin=new Vector2(.07f,.08f);srt.anchorMax=new Vector2(.93f,.83f);srt.offsetMin=srt.offsetMax=Vector2.zero;

        var content=new GameObject("Content",typeof(RectTransform),typeof(GridLayoutGroup),typeof(ContentSizeFitter));content.transform.SetParent(scrollGo.transform,false);var cr=Rect(content);cr.anchorMin=new Vector2(0,1);cr.anchorMax=new Vector2(1,1);cr.pivot=new Vector2(.5f,1);cr.anchoredPosition=Vector2.zero;var grid=content.GetComponent<GridLayoutGroup>();grid.cellSize=new Vector2(145,125);grid.spacing=new Vector2(20,22);grid.padding=new RectOffset(8,8,10,16);grid.constraint=GridLayoutGroup.Constraint.FixedColumnCount;grid.constraintCount=5;content.GetComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;sr.content=cr;sr.horizontal=false;sr.vertical=true;

        for(int i=0;i<pack.levels.Count;i++){int ix=i;int no=i+1;var b=Btn(""+no,content.transform,new Vector2(.5f,.5f),grid.cellSize,()=>StartLevel(pack.levels[ix].id),new Color(.99f,.88f,.54f),Art("btn_level_yellow"));b.interactable=!trainingPreview||trainingPreviewUnlockAll||ix<=maxUnlocked;var br=Rect(b.gameObject);br.anchorMin=br.anchorMax=new Vector2(.5f,.5f);br.sizeDelta=grid.cellSize;}

        gameCamera.backgroundColor=Cream;}

    void StartLevel(int requested){int index=pack.levels.FindIndex(l=>l.id==requested);if(index<0)index=Mathf.Clamp(requested,0,pack.levels.Count-1);levelIndex=index;level=pack.levels[levelIndex];winAt=-1f;hammerMode=false;drag=null;state=new State();undo.Clear();foreach(var p in level.parts){state.angles[p.id]=p.angle;state.chainLocks[p.id]=p.chainLockCount;var t=Array.Find(templates.templates,v=>v.id==p.templateId);if(t==null)throw new InvalidOperationException("Unknown source template "+p.templateId);p.radius=t.radius;p.width=t.width;p.gap=t.gap;p.length=t.length;}world.gameObject.SetActive(true);world.Build(level,templates);collision.Build(level,templates);bombContacts.Build(level,collision);inputWorld.Build(level,templates,collision,bombContacts);collision.ApplyState(state.angles,state.removed,state.slides);world.SetHint(-1);SetSafeBoard();mode=ScreenMode.Game;RenderGameUI();UpdateWorld();}

    void RenderGameUI(){

        ClearUI();panel=MakePanel("Gameplay HUD",Color.clear);Full(panel);

        originalHud=RotateRingsOriginalHud.Build(Rect(panel),levelIndex+1,null,Firework,Hammer,Undo,Hint,ShowHome,Restart,ToggleSound);

        originalHud.SetScore(0);originalHud.SetMusicState(soundOn);if(trainingPreview)Label("TRAINING PREVIEW / LEVEL "+level.id,panel.transform,new Vector2(.5f,.81f),new Vector2(950,70),25,Ink);else if(referenceQa)Label("ORIGINAL REFERENCE QA / SOURCE "+level.id,panel.transform,new Vector2(.5f,.81f),new Vector2(950,70),25,Ink);

    }

    void Firework(){

        if(mode!=ScreenMode.Game||world.IsEntering||winAt>=0f)return;

        foreach(var p in level.parts)if(!state.removed.Contains(p.id)){

            drag=null;PushUndo();state.moves++;RemovePart(p.id);Cascade(p.id);UpdateWorld();CheckWin();return;

        }

    }

    void Hammer(){if(mode!=ScreenMode.Game||world.IsEntering||winAt>=0f)return;hammerMode=true;Toast("TAP A PIECE TO BREAK IT");}

    void UpdateWorld(){if(world==null||state==null)return;SyncPhysics();foreach(var p in level.parts)if(RotateRingsRules.IsSlider(p))world.SetSliderOffset(p.id,state.slides.TryGetValue(p.id,out float offset)?offset:0);var active=new List<bool>();foreach(var link in level.links)active.Add(IsActive(link));world.ApplyState(state.angles,state.removed,state.keys,active);world.SetBombState(state.spentBombs,state.removed);world.ApplyChainState(state.chainLocks);if(originalHud!=null)originalHud.SetScore(CurrentScore());if(movesText!=null)movesText.text="MOVES  "+state.moves+(level.parMoves>0?"   •   PAR "+level.parMoves:"");}

    int CurrentScore(){

        int score=0;

        foreach(var p in level.parts)if(state.removed.Contains(p.id)){

            int baseScore=RotateRingsRules.IsSlider(p)?50:100,connections=0;

            foreach(var link in level.links)if(link.source==p.id||link.target==p.id)connections++;

            score+=baseScore*(1+connections)+(p.lockKey!=0?baseScore:0);

        }

        return score;

    }

    RotateRingsPart FindPart(int id)=>level.parts.Find(p=>p.id==id);



    void PollInput(){if(drag==null){if(Down(out int pid,out Vector2 screen)){if(EventSystem.current!=null&&EventSystem.current.IsPointerOverGameObject(pid))return;Vector2 w=gameCamera.ScreenToWorldPoint(screen);if(!inputWorld.TryHit(gameCamera,screen,state.removed,out int id))return;var p=FindPart(id);if(p==null||state.removed.Contains(id)||!RotateRingsRules.IsMovable(p))return;if(hammerMode){hammerMode=false;PushUndo();state.moves++;RemovePart(id);Cascade(id);UpdateWorld();CheckWin();return;}bool slider=RotateRingsRules.IsSlider(p);if(IsLocked(p)){Wobble(id);return;}if(!slider&&HasActiveOutgoing(id)){Wobble(id);return;}world.SetHint(id);float slide=state.slides.TryGetValue(id,out float offset)?offset:0f;collision.BeginGesture(id);drag=new Drag{id=id,slider=slider,startSlide=slide,delta=slider?slide:0f,beganLinked=!slider&&HasActiveIncident(id),startWorld=w,lastWorld=w,startAngle=state.angles[id],lastPointerAngle=Mathf.Atan2(w.y-p.y,w.x-p.x)*Mathf.Rad2Deg,valid=true};PushUndo();}return;}

        if(Held(out Vector2 current)){Vector2 w=gameCamera.ScreenToWorldPoint(current);var p=FindPart(drag.id);if(p==null){drag=null;return;}if(drag.slider){Vector2 axis=world.SlideAxis(p.id);float desired=drag.startSlide+Vector2.Dot(w-drag.startWorld,axis);bool blocked;drag.delta=collision.ClampSlide(drag.id,drag.delta,desired,out blocked);state.slides[drag.id]=drag.delta;UpdateWorld();}

            else {float a=Mathf.Atan2(w.y-p.y,w.x-p.x)*Mathf.Rad2Deg;float d=Mathf.DeltaAngle(drag.lastPointerAngle,a);drag.lastPointerAngle=a;

                if(Mathf.Abs(d)>1e-6f&&d*drag.blockDirection<=0){drag.blockDirection=0;float before=state.angles[p.id];bool blocked;float accepted=collision.ClampRotation(p.id,before,d,out blocked);state.angles[p.id]=accepted;drag.delta+=accepted-before;if(blocked)drag.blockDirection=Mathf.Sign(d);UpdateWorld();}}

            if(state.removed.Contains(drag.id)){state.moves++;drag=null;world.SetHint(-1);Cascade(-1);UpdateWorld();CheckWin();return;}drag.lastWorld=w;return;}

        if(Up()){FinishDrag();}}

    bool Down(out int id,out Vector2 p){id=-1;p=default;

#if ENABLE_INPUT_SYSTEM

        if(Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame){p=Mouse.current.position.ReadValue();return true;}if(Touchscreen.current!=null){foreach(var t in Touchscreen.current.touches)if(t.press.wasPressedThisFrame){id=t.touchId.ReadValue();p=t.position.ReadValue();return true;}}

#else

        if(Input.GetMouseButtonDown(0)){p=Input.mousePosition;return true;}if(Input.touchCount>0&&Input.GetTouch(0).phase==TouchPhase.Began){id=Input.GetTouch(0).fingerId;p=Input.GetTouch(0).position;return true;}

#endif

        return false;}

    bool Held(out Vector2 p){p=default;

#if ENABLE_INPUT_SYSTEM

        if(Mouse.current!=null&&Mouse.current.leftButton.isPressed){p=Mouse.current.position.ReadValue();return true;}if(Touchscreen.current!=null){foreach(var t in Touchscreen.current.touches)if(t.press.isPressed){p=t.position.ReadValue();return true;}}

#else

        if(Input.GetMouseButton(0)){p=Input.mousePosition;return true;}if(Input.touchCount>0){p=Input.GetTouch(0).position;return true;}

#endif

        return false;}

    bool Up(){

#if ENABLE_INPUT_SYSTEM

        if(Mouse.current!=null&&Mouse.current.leftButton.wasReleasedThisFrame)return true;if(Touchscreen.current!=null){foreach(var t in Touchscreen.current.touches)if(t.press.wasReleasedThisFrame)return true;}

#else

        if(Input.GetMouseButtonUp(0)||Input.touchCount==0)return true;

#endif

        return false;}

    void FinishDrag(){
        if(drag==null)return;
        var d=drag;drag=null;world.SetHint(-1);
        collision.EndGesture(d.id,d.blockDirection);
        state.angles[d.id]=collision.GetAngle(d.id);
        if(d.slider)state.slides[d.id]=collision.GetSlide(d.id);
        float releaseDelta=d.slider?state.slides[d.id]-d.startSlide:Mathf.DeltaAngle(d.startAngle,state.angles[d.id]);
        bool moved=Mathf.Abs(releaseDelta)>.001f;
        if(moved||d.bombTriggered)state.moves++;
        bool released=TryRelease(d.id,releaseDelta,d.beganLinked);
        Cascade(d.id);UpdateWorld();CheckWin();
        if(!moved&&!released&&undo.Count>0)undo.Pop();
    }

    void OnMotionSample(int owner){
        if(state==null||level.bombs==null||level.bombs.Length==0||drag==null)return;
        state.angles[owner]=collision.GetAngle(owner);
        if(RotateRingsRules.IsSlider(FindPart(owner)))state.slides[owner]=collision.GetSlide(owner);
        // Bring rendering to the swept pose before a source remove effect is dispatched.
        UpdateWorld();
        if(bombContacts.ProbeDuringDrag(owner,state.spentBombs,state.removed,RemovePart)){
            drag.bombTriggered=true;Cascade(owner);UpdateWorld();
        }
    }

    void SyncPhysics(){if(collision!=null&&state!=null){collision.ApplyState(state.angles,state.removed,state.slides);if(bombContacts!=null)bombContacts.ApplyState(state.spentBombs,state.removed);if(inputWorld!=null)inputWorld.ApplyState(state.angles,state.slides,state.removed,state.chainLocks,state.keys);}}

    bool HasActiveOutgoing(int id)=>collision.HasOutgoingBinding(id);

    bool HasActiveIncident(int id)=>collision.HasIncidentBinding(id);

    bool IsActive(RotateRingsLink link)=>collision.IsBinding(link.source,link.target);

    bool IsLocked(RotateRingsPart p)=>(p.lockKey!=0&&!state.keys.Contains(p.lockKey))||(state.chainLocks.TryGetValue(p.id,out int count)&&count>0);

    bool TryRelease(int id,float gestureDelta,bool beganLinked){
        var p=FindPart(id);if(p==null||state.removed.Contains(id))return false;
        // Native solve eligibility is independent of CanCollect and gesture length.
        if(collision.CanSolve(id)){RemovePart(id);return true;}
        Wobble(id);return false;
    }

    void RemovePart(int id){
        var p=FindPart(id);if(p==null||state.removed.Contains(id))return;
        world.BeginRelease(id);state.removed.Add(id);
        // RingLockChainService.OnObjectAfterSolve ignores FixedObject-derived fixtures.
        if(RotateRingsRules.IsMovable(p))foreach(var other in level.parts){if(state.chainLocks.TryGetValue(other.id,out int count)&&count>0)state.chainLocks[other.id]=other.id==id?0:count-1;}
        if(level.bombs!=null)for(int i=0;i<level.bombs.Length;i++)if(level.bombs[i].ownerPartId==id)state.spentBombs.Add(i);
        SyncPhysics();if(p.key!=0)state.keys.Add(p.key);PlayBreak(p.color);
    }

    void Cascade(int touched,bool includeTouched=true){
        bool changed=true;int guard=0;
        while(changed&&guard++<level.parts.Count){
            changed=false;
            // Native service processes the registered source roots in reverse order.
            for(int i=level.parts.Count-1;i>=0;i--){var p=level.parts[i];if(state.removed.Contains(p.id)||!collision.CanSolve(p.id))continue;RemovePart(p.id);changed=true;}
        }
    }

    void PushUndo(){if(state!=null){state.bindingSnapshot=collision.CaptureBindingState();undo.Push(state.Copy());}}

    void Undo(){winAt=-1f;hammerMode=false;drag=null;world.SetHint(-1);if(undo.Count==0){Toast("NOTHING TO UNDO");return;}state=undo.Pop();SyncPhysics();collision.RestoreBindingState(state.bindingSnapshot);foreach(var p in level.parts)if(RotateRingsRules.IsSlider(p))world.SetSliderOffset(p.id,0);UpdateWorld();}

    void Restart(){StartLevel(level.id);}

    void Hint(){
        if(level.solution!=null)foreach(var gesture in level.solution){
            var part=FindPart(gesture.part);
            if(!RotateRingsRules.IsMovable(part)||state.removed.Contains(part.id)||IsLocked(part))continue;
            if(RotateRingsRules.IsSlider(part)){
                if(collision.CanSlideAndRelease(part.id,gesture.delta)){world.SetHint(part.id);Toast("Slide the highlighted piece");return;}
                continue;
            }
            if(HasActiveOutgoing(part.id))continue;
            float travel=Mathf.DeltaAngle(state.angles[part.id],gesture.angle);
            if(gesture.delta>0&&travel<0)travel+=360;
            if(gesture.delta<0&&travel>0)travel-=360;
            if(Mathf.Abs(travel)<.1f||!collision.CanTurnAndReleaseDelta(part.id,travel))continue;
            world.SetHint(part.id);Toast("Rotate the highlighted ring "+Mathf.RoundToInt(Mathf.Abs(travel))+"° "+(travel>0?"counterclockwise":"clockwise"));return;
        }
        Toast(referenceQa?"REFERENCE QA: NO VERIFIED SOLUTION YET":"NO VERIFIED HINT FROM THIS POSITION");
    }

    void ToggleSound(){soundOn=!soundOn;if(music!=null)music.mute=!soundOn;if(sfx!=null)sfx.mute=!soundOn;if(originalHud!=null)originalHud.SetMusicState(soundOn);Toast(soundOn?"SOUND ON":"SOUND OFF");}

    void Wobble(int id){if(world!=null&&state!=null){var active=new List<bool>();foreach(var l in level.links)active.Add(IsActive(l));world.ApplyState(state.angles,state.removed,state.keys,active,true,id);}Toast("THIS RING IS STILL CONNECTED");}

    void PlayBreak(int color){if(!soundOn||sfx==null)return;string n=color%3==0?"rr_sfx_wood_break_1":color%3==1?"rr_sfx_rock_break_1":"rr_sfx_jelly_break_1";var clip=Resources.Load<AudioClip>("Audio/"+n);if(clip!=null)sfx.PlayOneShot(clip);}

    void CheckWin(){foreach(var p in level.parts)if(!state.removed.Contains(p.id))return;if(!referenceQa&&!trainingPreview){PlayerPrefs.SetInt("rr_max_level",Mathf.Max(maxUnlocked,Mathf.Min(levelIndex+1,pack.levels.Count-1)));maxUnlocked=Mathf.Max(maxUnlocked,Mathf.Min(levelIndex+1,pack.levels.Count-1));PlayerPrefs.Save();}else if(trainingPreview&&!trainingPreviewUnlockAll)maxUnlocked=Mathf.Max(maxUnlocked,Mathf.Min(levelIndex+1,pack.levels.Count-1));if(winAt<0f)winAt=Time.unscaledTime+RotateRingsOriginalMotion.VictoryDelay;}

    void ShowWin(){mode=ScreenMode.Win;world.gameObject.SetActive(false);ClearUI();panel=MakePanel("Level Clear",new Color(.99f,.95f,.87f));Full(panel);Label("PUZZLE COMPLETE!",panel.transform,new Vector2(.5f,.67f),new Vector2(900,160),62,Ink);Label("Cleared in "+state.moves+" moves",panel.transform,new Vector2(.5f,.57f),new Vector2(800,90),34,Brown,TextAnchor.MiddleCenter,FontStyle.Normal);Btn("NEXT LEVEL  ›",panel.transform,new Vector2(.5f,.43f),new Vector2(490,130),()=>{if(levelIndex+1<pack.levels.Count)StartLevel(pack.levels[levelIndex+1].id);else ShowHome();},new Color(.99f,.75f,.16f));Btn("LEVELS",panel.transform,new Vector2(.5f,.32f),new Vector2(390,100),ShowLevels,new Color(1,.88f,.67f));}

    void ShowError(string header,string body){mode=ScreenMode.Home;ClearUI();panel=MakePanel("Setup Required",Cream);Full(panel);Label(header,panel.transform,new Vector2(.5f,.57f),new Vector2(900,140),52,Ink);Label(body,panel.transform,new Vector2(.5f,.47f),new Vector2(900,150),30,Brown,TextAnchor.MiddleCenter,FontStyle.Normal);}

    void Toast(string text){if(toastText==null)toastText=Label(text,uiRoot,new Vector2(.5f,.17f),new Vector2(820,72),25,Ink);else toastText.text=text;toastText.gameObject.SetActive(true);CancelInvoke(nameof(HideToast));Invoke(nameof(HideToast),1.25f);}void HideToast(){if(toastText!=null)toastText.gameObject.SetActive(false);}

}


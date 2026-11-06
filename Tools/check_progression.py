"""Exercise real Home/Play buttons, label numbering and saved-index mapping in Play Mode."""
import json
from pathlib import Path
from unity_mcp import McpClient, DEFAULT_URL

code = r'''
System.Type t=null;
foreach(var assembly in System.AppDomain.CurrentDomain.GetAssemblies()) { t=assembly.GetType("RotateRingsRuntime"); if(t!=null)break; }
var runtime=UnityEngine.Object.FindFirstObjectByType(t);
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var maxField=t.GetField("maxUnlocked",flags);
int maxBefore=(int)maxField.GetValue(runtime);
bool hadKey=UnityEngine.PlayerPrefs.HasKey("rr_max_level");
int saved=UnityEngine.PlayerPrefs.GetInt("rr_max_level",0);
System.Action<string> invoke=name=>t.GetMethod(name,flags).Invoke(runtime,null);
System.Action<int> start=id=>t.GetMethod("StartLevel",flags).Invoke(runtime,new object[]{id});
System.Func<string> label=()=> { var hud=t.GetField("originalHud",flags).GetValue(runtime); return ((UnityEngine.UI.Text)hud.GetType().GetField("levelLabel",flags).GetValue(hud)).text; };
System.Func<int> readId=()=> { var level=t.GetField("level",flags).GetValue(runtime); return (int)level.GetType().GetField("id").GetValue(level); };
System.Action clickPlay=()=> {
    var panel=(UnityEngine.GameObject)t.GetField("panel",flags).GetValue(runtime);
    foreach(var button in panel.GetComponentsInChildren<UnityEngine.UI.Button>()) if(button.name=="Button - PLAY") { button.onClick.Invoke(); return; }
    throw new System.Exception("Home PLAY button missing");
};
try {
    maxField.SetValue(runtime,0);invoke("ShowHome");clickPlay();
    bool first=readId()==1 && label()=="Level\n1";
    var level=t.GetField("level",flags).GetValue(runtime);
    var state=t.GetField("state",flags).GetValue(runtime);
    var removed=(System.Collections.Generic.HashSet<int>)state.GetType().GetField("removed").GetValue(state);
    foreach(var part in (System.Collections.IEnumerable)level.GetType().GetField("parts").GetValue(level)) removed.Add((int)part.GetType().GetField("id").GetValue(part));
    invoke("CheckWin");
    bool savedNext=UnityEngine.PlayerPrefs.GetInt("rr_max_level",-1)==1;
    invoke("ShowHome");clickPlay();bool resumed=readId()==2;
    start(300);bool last=label()=="Level\n300";
    invoke("ShowLevels");var panel=(UnityEngine.GameObject)t.GetField("panel",flags).GetValue(runtime);
    int numbered=0;bool hasFirst=false,hasLast=false,has301=false;
    foreach(var b in panel.GetComponentsInChildren<UnityEngine.UI.Button>()) { int n; if(b.name.StartsWith("Button - ")&&int.TryParse(b.name.Substring(9),out n)) { numbered++;hasFirst|=n==1;hasLast|=n==300;has301|=n==301; } }
    bool picker=numbered==300&&hasFirst&&hasLast&&!has301;
    return new {passed=first&&savedNext&&resumed&&last&&picker,firstLabelAndPlay=first,nextSavedIndex=savedNext,homePlayResumesSecond=resumed,lastLabel=last,levelPicker=picker,numberedButtons=numbered,note="Winning state was set through reflection to isolate navigation/persistence; gesture input covered separately."};
} finally {
    maxField.SetValue(runtime,maxBefore);
    if(hadKey)UnityEngine.PlayerPrefs.SetInt("rr_max_level",saved);else UnityEngine.PlayerPrefs.DeleteKey("rr_max_level");
    UnityEngine.PlayerPrefs.Save();invoke("ShowHome");
}
'''
c=McpClient(DEFAULT_URL)
r=c.command('call','execute_code '+json.dumps({'action':'execute','code':code}))
result=r.get('structuredContent',{})
if not result:
    result=json.loads(next(x['text'] for x in r['content'] if x['type']=='text'))
print(json.dumps(result,indent=2))
out=Path(__file__).resolve().parents[1]/'Research/progression_qa_report.json'
out.write_text(json.dumps(result,indent=2),encoding='utf8')
if r.get('isError') or result.get('success') is False:
    raise SystemExit(1)

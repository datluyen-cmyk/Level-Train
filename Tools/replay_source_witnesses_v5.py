"""Revalidate retained source witnesses in independent current native fixtures."""
import argparse
import hashlib
import json
from pathlib import Path
from unity_mcp import McpClient, DEFAULT_URL
from run_native_virtual_playtest_v5 import execute


def main():
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('--workspace', type=Path, required=True)
    p.add_argument('--source-report', type=Path, required=True)
    p.add_argument('--output', type=Path, required=True)
    p.add_argument('--first', type=int, default=10)
    args = p.parse_args()
    if args.output.exists():
        raise FileExistsError(args.output)
    root = args.workspace.resolve()
    source_path = root / 'data/source500_native.json'
    ids = [l['id'] for l in json.loads(source_path.read_text())['levels'][:args.first]]
    q = lambda path: json.dumps(str(Path(path).resolve()))
    code = '''
if(UnityEditor.EditorApplication.isPlaying || UnityEditor.EditorApplication.isCompiling) throw new System.InvalidOperationException("Editor must be idle outside Play Mode.");
if(SourceSolverV3.GetPlayerBatchStatusV5()!="idle") throw new System.InvalidOperationException("Native player job active.");
var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static;
var reportType=typeof(SourceSolverV3).GetNestedType("SolverReport",System.Reflection.BindingFlags.NonPublic);
var report=UnityEngine.JsonUtility.FromJson(System.IO.File.ReadAllText(SOURCE),reportType);
var rows=(System.Collections.IList)reportType.GetField("levels").GetValue(report);
var pack=UnityEngine.JsonUtility.FromJson<RotateRingsLevelPack>(System.IO.File.ReadAllText(PACK));
var templates=UnityEngine.JsonUtility.FromJson<RotateRingsTemplateCatalog>(System.IO.File.ReadAllText(TEMPLATES));
var wanted=new System.Collections.Generic.HashSet<int>(new int[]{IDS});
var replay=typeof(SourceSolverV3).GetMethod("Replay",flags);
int checkedCount=0,solved=0;
for(int i=rows.Count-1;i>=0;i--){
 var row=rows[i];var type=row.GetType();int id=(int)type.GetField("sourceId").GetValue(row);
 if(!wanted.Contains(id)){rows.RemoveAt(i);continue;}
 if((string)type.GetField("status").GetValue(row)!="solved")continue;
 var level=pack.levels.Find(l=>l.id==id);
 object[] replayArgs={level,templates,type.GetField("actions").GetValue(row),0,null};
 bool goal=(bool)replay.Invoke(null,replayArgs);checkedCount++;if(goal)solved++;
 type.GetField("replayGoal").SetValue(row,goal);type.GetField("replayedActions").SetValue(row,(int)replayArgs[3]);
 type.GetField("status").SetValue(row,goal?"solved":"unknown");
 type.GetField("proof").SetValue(row,goal?"Current independent native replay of retained historical witness; not optimality or human difficulty.":"Historical witness failed current native replay.");
 type.GetField("error").SetValue(row,replayArgs[4]);
}
reportType.GetField("rulesHash").SetValue(report,typeof(SourceSolverV3).GetMethod("HashRuleBundle",flags).Invoke(null,null));
reportType.GetField("policyId").SetValue(report,"retained-witness-current-native-replay-v5");
reportType.GetField("complete").SetValue(report,true);reportType.GetField("completedLevels").SetValue(report,rows.Count);reportType.GetField("totalLevels").SetValue(report,rows.Count);
reportType.GetField("status").SetValue(report,solved==rows.Count?"complete":"partial");
System.IO.File.WriteAllText(OUTPUT,UnityEngine.JsonUtility.ToJson(report,true));
return "replayed="+checkedCount+"; solved="+solved+"; sources="+rows.Count;
'''
    for token, value in {'SOURCE':q(args.source_report), 'PACK':q(source_path),
                         'TEMPLATES':q(root/'data/source_templates.json'), 'OUTPUT':q(args.output),
                         'IDS':','.join(map(str,ids))}.items():
        code = code.replace(token, value)
    print(execute(McpClient(DEFAULT_URL), code))
    sha = lambda path: hashlib.sha256(Path(path).read_bytes()).hexdigest()
    manifest = {'historicalReportSha256':sha(args.source_report), 'currentReplayReportSha256':sha(args.output),
                'toolSha256':sha(__file__), 'sourceIds':ids, 'meaning':'Unknown originals remain unknown; no new search or difficulty labels.'}
    args.output.with_suffix('.provenance.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')


if __name__ == '__main__':
    main()

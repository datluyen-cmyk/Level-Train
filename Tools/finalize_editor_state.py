"""Save live Editor readiness after the final Play Mode checks."""
import json
from pathlib import Path
from unity_mcp import McpClient, DEFAULT_URL
c=McpClient(DEFAULT_URL)
r=c.command('call','manage_editor '+json.dumps({'action':'stop'}))
code=r'''return new {colorSpace=UnityEditor.PlayerSettings.colorSpace.ToString(),playing=UnityEditor.EditorApplication.isPlaying,compiling=UnityEditor.EditorApplication.isCompiling,scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path,ringShaderError=UnityEditor.ShaderUtil.ShaderHasError(UnityEngine.Shader.Find("HsApp/Effect/XYStyleLine")),fragmentShaderError=UnityEditor.ShaderUtil.ShaderHasError(UnityEngine.Shader.Find("RotateRings/OriginalFragment"))};'''
def unwrap(r):
    d=r.get('structuredContent')
    if not d:d=json.loads(next(x['text'] for x in r['content'] if x['type']=='text'))
    if 'result' in d and 'success' not in d:d=d['result']
    return d
r=unwrap(c.command('call','execute_code '+json.dumps({'action':'execute','code':code})))
state=r['data']['result']
logs=unwrap(c.command('call','read_console '+json.dumps({'action':'get','types':['error'],'count':100,'format':'plain'})))
state['consoleErrorCount']=len(logs['data'])
state['consoleErrors']=logs['data']
root=Path(__file__).resolve().parents[1]
(root/'Research/physics_v2/final_editor_state.json').write_text(json.dumps(state,indent=2),encoding='utf8')
print(json.dumps(state,indent=2))
if state['playing'] or state['compiling'] or state['consoleErrorCount'] or state['ringShaderError'] or state['fragmentShaderError']:raise SystemExit(1)

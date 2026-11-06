"""Capture the live Game View via Unity MCP; requires an idle Editor or Play session."""
import argparse
import json
from pathlib import Path
import time
from unity_mcp import McpClient, DEFAULT_URL

p = argparse.ArgumentParser()
p.add_argument('--size', choices=['portrait', 'tall', 'landscape'])
p.add_argument('--level', type=int, help='one-based display level; omit for home')
p.add_argument('--label', required=True)
p.add_argument('--play', action='store_true')
a = p.parse_args()
c = McpClient(DEFAULT_URL)

def call(name, args):
    r = c.command('call', name + ' ' + json.dumps(args))
    data = r.get('structuredContent', {})
    if not data:
        for block in r.get('content', []):
            if block.get('type') == 'text':
                try:
                    data = json.loads(block['text'])
                except json.JSONDecodeError:
                    pass
    data = data.get('result', data)
    print(json.dumps(data, ensure_ascii=False))
    if r.get('isError') or data.get('success') is False:
        raise RuntimeError(str(data))
    return data

if a.size:
    width, height = {'portrait': (720,1280), 'tall': (720,1440), 'landscape': (1280,720)}[a.size]
    code = '''
var flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
var assembly = typeof(UnityEditor.Editor).Assembly;
var sizesType = assembly.GetType("UnityEditor.GameViewSizes");
var singletonType = typeof(UnityEditor.ScriptableSingleton<>).MakeGenericType(sizesType);
var sizes = singletonType.GetProperty("instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy).GetValue(null);
var groupType = assembly.GetType("UnityEditor.GameViewSizeGroupType");
var currentGroup = sizesType.GetProperty("currentGroupType", flags).GetValue(sizes);
var group = sizesType.GetMethod("GetGroup", flags).Invoke(sizes, new object[]{currentGroup});
var sizeType = assembly.GetType("UnityEditor.GameViewSize");
var modeType = assembly.GetType("UnityEditor.GameViewSizeType");
var constructor = sizeType.GetConstructor(flags, null, new System.Type[]{modeType, typeof(int), typeof(int), typeof(string)}, null);
var size = constructor.Invoke(new object[]{System.Enum.Parse(modeType, "FixedResolution"), WIDTH, HEIGHT, "Rotate Rings QA SIZE"});
group.GetType().GetMethod("AddCustomSize", flags).Invoke(group, new object[]{size});
int builtins = (int)group.GetType().GetMethod("GetBuiltinCount", flags).Invoke(group, null);
int customs = (int)group.GetType().GetMethod("GetCustomCount", flags).Invoke(group, null);
var viewType = assembly.GetType("UnityEditor.GameView");
var view = UnityEditor.EditorWindow.GetWindow(viewType);
viewType.GetProperty("selectedSizeIndex", flags).SetValue(view, builtins + customs - 1);
view.Focus();
return "Set Game View to WIDTHxHEIGHT";
'''.replace('WIDTH', str(width)).replace('HEIGHT', str(height)).replace('SIZE', a.size)
    call('execute_code', {'action': 'execute', 'code': code})
    time.sleep(1)
if a.play:
    call('manage_editor', {'action': 'play'})
    time.sleep(3)
method = 'ShowHome' if a.level is None else 'StartLevel'
parameter = 'null' if a.level is None else f'new object[]{{{a.level}}}'
code = f'''
System.Type type = null;
foreach(var assembly in System.AppDomain.CurrentDomain.GetAssemblies()) {{ type = assembly.GetType("RotateRingsRuntime"); if(type != null) break; }}
var runtime = UnityEngine.Object.FindFirstObjectByType(type);
type.GetMethod("{method}", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(runtime, {parameter});
return "Opened {a.label}";
'''
call('execute_code', {'action': 'execute', 'code': code})
time.sleep(1)
requested_at = time.time()
capture = call('manage_camera', {'action': 'screenshot', 'include_image': False, 'capture_source': 'game_view', 'screenshot_file_name': a.label + '.png', 'output_folder': 'Captures'})
target = Path(capture['data']['fullPath'])
for _ in range(20):
    if target.exists() and target.stat().st_size > 100 and target.stat().st_mtime >= requested_at:
        print('CAPTURE', target)
        break
    time.sleep(.5)
else:
    raise RuntimeError('Screenshot not written: ' + str(target))

# Unity MCP QA Notes

Project: `D:\RotateRings\RotateRings Test`  
Unity: 6000.0.78f1  
Current MCP instance: `RotateRings Test@c795d4340547c373` (read `mcpforunity://instances` again after editor reloads; hash can change).  
Endpoint/client: `http://127.0.0.1:8080/mcp`, `python Tools\unity_mcp.py`.

## Verified status

- `mcpforunity://editor/state`: `ready_for_tools=true`; `SampleScene`; Edit Mode; not compiling.
- `read_console` with `types=["error"]` returned zero entries after package setup.
- Game View screenshot capture works. Baseline captured to `Tools\Captures\screenshot-20261105-115425.png`; current template scene has no game implementation yet.

## CLI examples (PowerShell)

```powershell
python Tools\unity_mcp.py resource mcpforunity://instances
python Tools\unity_mcp.py resource mcpforunity://editor/state
python Tools\unity_mcp.py call read_console '{"action":"get","types":["error","warning"],"count":"50","format":"plain"}'
python Tools\unity_mcp.py call manage_editor '{"action":"play"}'
python Tools\unity_mcp.py call manage_editor '{"action":"stop"}'
python Tools\unity_mcp.py call manage_camera '{"action":"screenshot","include_image":false,"capture_source":"game_view"}'
```

The `execute_menu_item` schema is `{ "menu_path": string|null }`. It invokes a Unity menu command by its exact path, e.g. `Window/General/Game %2`; menu paths can be read from `mcpforunity://menu-items`. This is not a generic static-method caller. If implementation adds an Editor `[MenuItem("Rotate Rings/QA/Replay 300")]`, invoke that exact registered menu path here.

## C# execution and 300-run replay

`execute_code` is in the MCP `scripting_ext` group (it was enabled for this session). Schema:

- `action`: `execute`, `get_history`, `replay`, or `clear_history` (required)
- `code`: C# method-body source for `execute`; `return` sends data to the caller
- `compiler`: `auto` (default), `roslyn`, or `codedom`
- `safety_checks`: boolean, default true
- `index`: history entry index for `replay`
- `limit`: history length for `get_history`, 1–50, default 10

It compiles/runs C# in memory in the Unity Editor and creates no script file. For a 300-case static evaluator, either run its method through a small `execute_code` body after checking the evaluator API, or provide an Editor `[MenuItem]` and invoke it with `execute_menu_item`. `execute_code` is arbitrary Editor code, so keep it to the intended QA routine. It currently reports an empty execution history.

The separate `testing` group is disabled by default; `manage_tools` supports `{"action":"activate","group":"testing"}` for `run_tests` / `get_test_job` if formal Unity Test Framework tests exist.

## Input and screenshot smoke

- MCP can start/stop play with `manage_editor`; it does not synthesize raw gameplay keyboard/mouse input. For genuine input smoke, focus the Unity Game View and use computer-use input on the original Unity window after Play begins.
- `manage_camera` screenshot supports `capture_source="game_view"` or `"scene_view"`, `include_image`, `max_resolution`, `view_target`, and optional output folder. With `include_image=false`, Game View capture returns asynchronously to `Assets/Screenshots`; wait for the PNG and move the evidence to `Tools\Captures` if it should stay out of game assets. Scene View captures return synchronously.
- For generator GUI QA after its bundle exists: capture the visible screen, exercise cancel, confirm no generation occurred, then perform exactly one generation and capture its result. Do this only after the GUI/runtime implementation is ready.

## Later QA sequence

1. Confirm main says implementation files are stable; refresh/query editor state and check console errors.
2. Run all 300 C# replay cases and retain counts/results; use the concrete evaluator/menu method established by implementation.
3. Enter Play Mode and smoke keyboard/touch-equivalent inputs in the original Unity Game View; check console after each critical path.
4. Capture Game View screenshots and test the generator cancel path, then exactly one successful generation when that bundle is present.
5. Stop Play Mode and report the final editor state. Do not create a player build unless separately requested.

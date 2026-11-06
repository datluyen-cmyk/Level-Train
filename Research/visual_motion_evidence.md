# Original Rotate Rings visual and motion evidence

Source: recovered Hot.dll SHA256 `b83cac16e0ad6e81d81a6fecd1ee014a679f908f4f54f9deb37c654899ecb7d8`; ILSpy decompile under `Research/rules/decompiled`. These are static shipped-code facts, not an observation of the APK executing. Line numbers below are one-based decompiled C# lines. Runtime experiment selection and serialized asset values may override class defaults. No pipeline, model, level or runtime code was changed for this investigation.

## Highest-impact fidelity corrections

1. Render the recovered ring MeshRenderer geometry with its source material/shader. `RingObject` participates in MeshRenderer-based rendering, and normal release passes its actual `MeshFilter.sharedMesh` to the effect. Source geometry can itself be a flat ribbon with source UV/color channels; the original shader supplies the rounded appearance. A generic colored arc with a default shader cannot reproduce that shading or texture. Mesh and material extraction is handled separately.
2. Rotation follows the angle swept by the finger around the ring center continuously; it is not a click-to-turn action. Respect collision-clamped intermediate angles. Do not add fixed-duration rotation tweens to direct manipulation.
3. Normal release hides the ring renderers and emits **mesh-shaped shattering particles** at the same XY and ring Z-angle. A shrink/fade or fly-away ring is not the ordinary release in this path.
4. Entry is a top-to-bottom scale wave, starting at 72% and returning to exact source scale; default individual duration is 0.5 seconds.
5. A denied locked-ring gesture plays an 8-degree damped angular wobble over 0.32 seconds, returning to its original orientation. Active selection is a mint outline, not a replacement ring color.

## Continuous angular drag and tactile response

| Evidence | Exact behavior | Implementation consequence |
|---|---|---|
| `RotateRings.GamePlay.Services/DragDefaultLimitStyleHandler.cs:60-64,95-132` | Store previous world pointer. Compute signed angle between old/new vectors from ring center using dot/cross; feed the angle through feel adjustment; find last valid angle on the collision path. | Use actual pointer polar angle differences. The ring center stays fixed (`:169-174`). |
| Same file `:98-100` | Ignore world-pointer movement with squared magnitude below 0.0009 (distance0.03). | Avoid subpixel jitter. |
| Same file `:117-138` | A collision sets blocked direction; continued same-direction drag is ignored until the player reverses; threshold0.1 degree. | No elastic traversal through adjacent bodies. |
| `DragRotateObject.cs:9-23,348-423` | Path probes depend on angular span, using midpoint for intermediate motion, 0.33/0.66 for large motion, binary-search final valid pose to0.5 degree. | Geometry collision must run during a drag, not only at release. |
| `RingRotateGuardConfig.cs:3-10` | Guard-style defaults: maximum finger delta35 degrees/frame; rotation substep4 degrees. | An alternative experimental drag handler exists; do not claim the chosen experiment is known. |
| `RingRotationFeelResolver.cs:33-40,44-60` | Explicit baseline config: EnableFeel=true, damping0.75, compensation1.2. Runtime resolution copies experiment EnableFeel (not unconditionally true) and validates numeric ranges. | Support these values as an evidence-based preset; active APK experiment unknown. |
| `RingRotationFeelApplicator.cs:70-105` | When another threadable pin occupies the opening: applied angle=pointer angle*0.75, accumulate lost angle. When free and moving same direction: compensate by1.2 while repaying accumulated loss; final loss release capped12 degrees/frame. | This is conditional interaction resistance, not an easing tween over time. |
| Same file `:15-31,134-164,189-212` | Ratchet sound every15 degrees applied, at least0.12 seconds apart; ignores applied motion below0.3 degree; active-finger history0.15 seconds, travel18px/recent1.5px. | Sound follows actual applied rotation and stops when gesture ends. |
| `RingObject.cs:2607-2644,2710-2718` | Pointer down vibrates1101. Chain lock gets its own shake; relative lock plays locked animation and disables dragging until completion. Successful start enables selection outline. | Locked-feedback must not accidentally change the logical ring angle. |

`RingLockShakeService.cs:24-42` plays UiDeny, kills the prior target tween, samples `blockedShakeCurve(t)*blockedShakeStrength`, uses DOTween progress with numeric Ease1, and restores the initial local Z angle on completion. `RingLevelConfig.cs:36-42,124-152` declares duration0.32s, strength8 degrees, and default curve keyframes `(0,0),(.12,1),(.24,-.65),(.38,.38),(.52,-.18),(.68,.06),(1,0)`. This curve is a fallback; prefer the extracted ScriptableObject curve if present.

## Entry animation

The current manager registers `RotateRingLevelEnterService` (GameplayManager.cs:542). Use this service rather than the older standalone `RingLevelEnterService`, whose interval handling differs.

- `RotateRings.GamePlay.Services/RotateRingLevelEnterService.cs:122-138`: gather gameplay MonoBehaviours; sort descending world Y; disable their physics colliders before animating.
- `:186-218`: store each original scale; initialize all elements using `initialAllRingsScaleFactor`; insert individual scale-progress tween at `index*dynamicWaveInterval`; pass scale factor through lock-chain/key attachments too.
- `:188-197`: progress0..1 uses numeric Ease1 and `waveDuration`; actual scale is controlled by `RingLevelConfig.ApplyRingWaveScale`.
- `:415-430`: interval=`max(min(waveInterval,waveMaxEnterDuration/elementCount),waveMinInterval)`. Total=`(count-1)*interval+waveDuration`.
- `RingLevelConfig.cs:13-35`: class defaults waveDuration0.5s, base interval0.03s, total staggering budget1s, minimum interval0.01s, initial/start scale0.72, scale curve `AnimationCurve.EaseInOut(0,0,1,1)`.
- `RingLevelConfig.cs:163-185`: scale uses `Vector3.LerpUnclamped(originalScale*startScaleFactor,originalScale,scaleCurve.Evaluate(t))`; retain possible overshoot from a serialized curve.
- `RotateRingLevelEnterService.cs:162-167,325-332`: restarts BGM and plays RoundStart or RoundStartStyling according to banner condition.
- `RotateRings.UI.Game/UIGameView.cs:348-350`: `PlayEnterTransitionStart()` returns0. No extra generic HUD entrance delay is established by this method.

## Normal ring release: shattering, not ring translation

`RotateRings.GamePlay.Services/SolvableSolverBase.cs:182-206` marks solved, performs pre-solve cleanup/collider disable, resolves attached relatives, sends RingAfterSolveEvent, then runs break gates and remove presentation asynchronously. `:239-267` resolves the actual MeshFilter.sharedMesh from the part hierarchy and dispatches RingRemoveEffectEvent. No mesh means direct destruction; this is a fallback, not the desired appearance.

`RingSolveSuccessService.cs` is the concrete ordinary remove effect:

| Lines | Evidence |
|---|---|
|108|Loads `eff_cycle_boom` from group `UnlockRing_Effect`.|
|129-147|Finds all ParticleSystems; sets world simulation; GPU instancing=false, renderer sortingOrder10; for shapeType6 (Mesh), assigns the original part mesh.|
|149-164|Stops/clears particles; computes maximum `startDelay.constant/constantMax + startLifetime.constant/constantMax` over systems (not `main.duration + startLifetime`).|
|166-178|If mesh shape exists, disables **all** original part MeshRenderers, activates effect and plays every particle system.|
|180-184|Waits computed particle lifetime before recycling/destroying; fallback duration1s only if calculated duration<=0. It is incorrect to advertise1s as the original particle timing.|
|288-302|Disables all physics colliders immediately; ordinary release runs now, bomb release is queued.|
|306-324|Bomb queue flushes on BombBreakTickEvent.|
|356-368|Effect XY matches source, world Z offset=-5, source world Z rotation retained, effect localScale=one.|
|371-378|Once per frame, plays CeramicRingBreak plus CeramicRingRelease and vibration1102.|

Do **not** animate the actual ring moving out merely because `RingObject.cs:78-82` still serializes moveOutDirection/moveOutDuration. The current `RingObjectSolver` delegates to `SolvableSolverBase` and the particle removal path. Those fields alone do not establish a used fly-out implementation.

Exact particle speed, gravity, burst count, fragment mesh, size/color over lifetime and effect material are recovered in `Research/eff_cycle_boom_modules.json`, `Research/eff_cycle_boom_material.json`, and `Research/eff_cycle_boom_shader.json` from prefab `eff_cycle_boom` (source bundle `defaultpackage_assets_02_res_art_effect_prefab_maingameplay_41e0da58007088651c05f0c7803d1206.bundle`, prefab pathID3224471653643778443). The visible child particle lifetime is .6-.75s; the prefab also has a no-emission root particle system with startLifetime5s and startDelay0, so the service cleanup span is5s.

## Lock and special-action pacing

`RingLevelConfig` class defaults (serialized values take precedence):

| Field | Default | Source line |
|---|---:|---:|
|chainBreakIntervalSeconds|0.2s|67|
|chainBreakStartDelaySeconds|0.4s|70|
|chainRingBreakIntervalSecondsMin/Max|0.2/0.4s|73/76|
|paletteLockKeyUnlockFlyStartIntervalSeconds|0.15s|82|
|paletteLockKeyUnlockFlyTimeoutPaddingSeconds|1s padding, not flight duration|85|
|paletteLockCascadeSolveDelaySeconds|0.2s|88|

`RingSolveBreakDelayService.cs:244-250` registers ColorLockDelayHandler, ChainBreakDelayHandler, BombBreakDelayHandler in priority order; `SolvableSolverBase.cs:77-114` waits break-ready gates, emits RingBreakReadyEvent, then waits remove delay. Do not flatten every cascade into instantaneous simultaneous deletion.

Optional special-action screen shake is config-driven, not a standard effect on every release. `RotateRings.GamePlay.Services/ScreenShakeConfigAsset.cs:65-114` fallback entries: bomb delay0.55s/duration0.35s/amplitude0.15/vibrato10/randomness30; hammer and rocket delay0.1s/duration0.25s/amplitude0.12/vibrato8/randomness20. `ScreenShakePreset.cs:17-35` default Ease numeric6 and fadeOut=true. Prefer serialized ScreenShakeConfig entries when extracted.

## Visual framing and selection

- `RotateRings.GamePlay.Services/RotateCameraAdaptation.cs:19,130-189,221-228`: orthographic camera; initial size15 divided by level scale; fit checks against **UIGame.GameRect**; when needed bounds fit is `max(bounds.width*screenHeight/(2*gameRectWidth),bounds.height*screenHeight/(2*gameRectHeight))/scale`, with Y compensation to center within GameRect. Clear solid color RingTheme.Surface; near/far=-100/+100. Do not center the board under the full-screen midpoint if top/bottom HUD consumes space.
- `RingTheme.cs:5-21` supplies fallback theme colors: Canvas#FFFFFF, Surface#F4F6F8, Ink#20252B, Line#C7CED6, Selected#2563EB, Fixed#59636E, Bomb#E4572E, Released#2E9D68, MutedInk#68737F. These do not replace original level background texture/material.
- `RotateRings.GamePlay.Services/RotateRingBackgroundAdaptation.cs:13-25`: background design1080x2160 (aspect0.5), cover overscan1.05; night effect `eff_background_loop_star`, Z offset=-1. `:156-161` swaps material by theme; original background renderer/material is preserved.
- `RotateRings.GamePlay.Services/RotateRingOperateHighlight.cs:9,109-137`: selected outline color=(0.5215686,1,0.9411765,1) ~=#85FFF0; enable `_OUTLINE_ON`, width0.1/softness0.01, or experimental width0.2/softness0.005; mode2; don't use vertex color; shadow tint alpha0 while selected. Original shared material restored when unselected (`:54-65`).
- Materials vary by experiment/skin. `PartStyleGlossyMetalHandler.cs:9-13` names `mat_ring_glossy_metal`, `mat_catch_glossy_metal`; `PartStyleMatteSmoothHandler.cs:5-9` names `mat_ring_matte_smooth`, `mat_catch_matte_smooth`. A source material with its exact shader properties is preferable to a generic Standard material.
- `RotateRings.UI.Game/UIGameView.cs:64-106` exposes TopContent, CenterContent, PropContent, GameRect, RingRect, ChainLockContent, ThemeNode etc. Widget positions, fonts, sprites and animation clips must be read from the UIGame prefab; declarations alone do not establish layout values.

## Win timing and uncertainty

`GameplayManager.cs:1213-1215` schedules victory only when all unsolved objects reach0. `LevelVictorySettlementService.cs:69-110,189` waits blocking key-unlock presentations, then serialized `postSolveDelaySeconds`, then settlement continuation. The concrete delay is `GameplayManager.completeLevelDelay` (`GameplayManager.cs:353-354`), which has no class initializer. The current GameplayManager serialized scene value is now confirmed as1.0s (see serialized evidence appendix); this is stronger evidence than the older manager's default.

Not established here: active experiment, exact particle prefab modules, serialized RingLevelConfig overrides, UIGame dynamic widget font/sprite bindings, win/confetti prefab timing, and online video's correspondence to this APK version. Report these as unknown until supported by extracted asset trees or observed frames.


## Serialized scene and UIGame evidence (additional extraction)

Full type trees, source bundle SHA256 and original path IDs are retained in `Research/visual_scene_evidence.json`; animation float curves including all weighted tangents are separately in `Research/visual_ui_animation_curves.json`. Read with UnityPy directly from supplied APK bundles; no modification of source bundles.

Scene bundle: `defaultpackage_assets_02_res_art_level_scene_f88c7a9d8eb75b463217db21ad20e957.bundle`.

- Current `GameplayManager` MonoBehaviour pathID−4041243178237619870 on `GameScene/GameManager`: **completeLevelDelay=1.0 seconds**. Confirmed exact serialized value.
- Two prefab roots coexist in this bundle. `RingGameScene/Main Camera` is orthographic size12 at local(0,1,−10); `RingGameScene/Directional Light` is white directional intensity2, local(0,3,0), quaternion(.408217877,−.234569684,.109381631,.875426114). This light belongs to the older RingGameScene branch.
- Current `GameScene/GameManager/Main Camera` is local(0,0,−10), identity rotation. Its serialized perspective/size5 state is overridden by current RotateCameraAdaptation code; do not mistake prefab static projection for gameplay projection.
- No Light component belongs to current GameScene branch in this bundle. The older light's presence does not establish lighting in the current gameplay branch. Preserve original material/shader behavior before adding generic lighting.
- Current `GameScene/RIngGameBG` localZ=2.

UIGame bundle: `defaultpackage_assets_02_res_art_ui_prefab_game_uigame_94be8e9d1e5e11cc67678151538a8fd8.bundle`.

| RectTransform | Anchors min/max | Anchored position | Size delta |
|---|---|---|---|
|RingRect|(0,0)/(1,1)|(0,82)|(−80,−676)|
|TopContent|(.5,1)/(.5,1)|(0,−116)|(1080,232)|
|PropContent|(.5,0)/(.5,0)|(0,240)|(1080,200)|
|CenterContent|(0,0)/(1,1)|(0,0)|(0,0)|
|Middle|(.5,1)/(.5,1)|(0,−104)|(378,134)|
|Left|(0,1)/(0,1)|(134.748596,−95.501465)|(195.497192,191.003098)|

These are authored prefab coordinates; runtime safe-area/global canvas adapters may affect actual screen positions. The local UIGame CanvasScaler has mode0, scaleFactor as serialized, reference resolution800x600, matchWidthOrHeight0; do not infer final display sizing from that default reference resolution alone.

Original UI clips (60Hz authored sample rate):

- `anim_uigame_in`: top Y118 at0, −123.9113617 at.1666667, −116 at.4166667; prop Y−124 at0,252.687149 at.1666667,240 at.4166667; Panel alpha0 at0,1 by.1666667. Both positions overshoot and settle. Weighted tangents are retained verbatim in the curve JSON.
- `anim_uigame_out`: top Y−116 at0 to119 at.25; prop Y240 to−137 at.25; alpha stays1 through.0833333 then becomes0 at.25.
- Clip m_MuscleClip metadata stopTime=1.0 while the last visible float keys occur at.416667/.25 respectively. Report these separately; controller wait duration must not be guessed from visible keys alone.


Implementation note: base effect modules are now extracted in `Research/eff_cycle_boom_modules.json`; exact atlas `tex_sequence_005` is copied to `Assets/RotateRings/Resources/OriginalMotion/CycleFragmentAtlas.png`. The child renders billboards, with the source ring mesh used only as the emitter shape. minMaxState0 means fixed speed0.87 and burst50; inactive minScalar fields5/30 are not active ranges. Full implementation provenance is in `Research/original_motion_implementation.json`.


## Superseding serialized motion configuration (recovered later)

`Research/shaders/serialized_motion_config_overrides.json` now contains the actual source `RingLevelConfig` ScriptableObject (pathID1144418767716547317) and ScreenShakeConfig from `defaultpackage_assets_02_res_art_scriptableobject_d6a822bc5f3109e3adf81805aba37d82.bundle`. These values **supersede the class fallback presets** elsewhere in this research:

- Entry duration.5s; interval.025s; max staggeringbudget.8s; minimuminterval.001s.
- All waiting elements scale0 (invisible); when their individual tween begins, startscale.5. Authored weighted scale curve keys `(0,0),(.411148697,1.039612293),(.715143979,.984450877),(1,1)` with exact tangents/weights retained in JSON and helper. Since scale=lerp(.5,1,curve), actual scale overshoots1.019806 then settlesvia.992225.
- Denied wobble duration.5s, strength8degrees. Curvevalues same seven timing keys as classfallback, but first tangent21.0131741 and peak−.24589394 are authored overrides. Exact tangents/weights are retained.
- Chain breakinterval.17s,startdelay.48s,ring interval min.1/max.3s.
- ScreenShake Trigger0Bomb: delay.55,duration.4,amplitude.15,vibrato20,randomness0. Trigger1Hammer: delay.1,duration.4,amplitude.15,vibrato20,randomness0. Trigger2Rocket: delay.15,duration.2,amplitude.13,vibrato20,randomness25. All Ease1/fadeouttrue.

`RotateRingsOriginalMotion` has been updated to these actual serialized values. `InitialWaitingScale` exposes0 for caller handling before each individual starttime; normalized `EntryScale(0)` correctly returns.5 and must not be used for a waiting element.


### Correction: solve effect cleanup duration

The previous 10s cleanup claim was incorrect. It added the prefab root system's main.duration (5s) to its startLifetime (5s), but RingSolveSuccessEffectService.TryCreateFragmentPlayback computes only the maximum startDelay + startLifetime across all particle systems (Research/rules/decompiled/RotateRings.GamePlay.Services/RingSolveSuccessEffectService.cs, method near TryCreateFragmentPlayback). The root system has emission disabled, startDelay=0, and startLifetime=5s; the visible fragment child has startLifetime=.6-.75s. The source effect is therefore recycled after5s, not10s. The previous Play Mode report measured the old helper's 10s destroy timer, not the source's cleanup span. The QA script now expects5s, but it has not been rerun after this correction; do not present the current revision as Unity-verified.

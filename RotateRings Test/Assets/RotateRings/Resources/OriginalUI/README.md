# Original gameplay HUD extraction

`RotateRingsOriginalHud.cs` is a standalone screen-space overlay builder. It attaches beneath an existing `RectTransform` and does not modify the game runtime or world view.

```csharp
var hud = RotateRingsOriginalHud.Build(
    canvasRoot, 32,
    onSettings: OpenSettings,
    onFirework: UseFirework,
    onHammer: UseHammer,
    onUndo: Undo,
    onHint: Hint,
    onHome: ShowHome,
    onRestart: Restart,
    onMusic: ToggleMusic);
hud.SetScore(2050);
```

The parent Canvas should use portrait reference resolution 1080 x 2160. `SetLevel`, `SetScore`, and `SetMusicState` keep the visible state current. The settings button opens an overlay containing Undo, Hint, Home, Restart, Music, and Close actions. Firework and Hammer remain separate bottom booster buttons.

The HUD intro uses the recovered `TopContent` and `PropContent` final positions and animation samples from `Research/visual_ui_animation_curves.json`: top Y `118 → -123.911 @ 0.166667s → -116 @ 0.416667s`; props Y `-124 → 252.687 @ 0.166667s → 240 @ 0.416667s`; both alpha values reach 1 by 0.166667s.

`cream_background.png` is a 1 x 1 color texture made from the exact normal gameplay Main Camera clear color (`RGBA 1, 0.99490005, 0.95754719, 1`; camera path ID `9119548058752006078`). The gameplay scene has no separate full-screen cream texture. `game_bg_mask.png` is the separately named original grayscale mask texture; use it only as an optional low-opacity material overlay, not as the cream fill.

Asset source bundle names, bundle SHA-256 hashes, Unity types/path IDs, names, and dimensions are in `provenance.json`. The original font is the embedded `Roboto-ExtraBold` TTF.

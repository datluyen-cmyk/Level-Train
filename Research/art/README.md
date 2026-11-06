# Original art and audio reference extraction

Generated from the APK's Unity asset bundles under `Research/extracted/assets/yoo/DefaultPackage` with `Tools/extract_art.py` (UnityPy 1.25.4 and Pillow).

## Deliverables

- `RotateRings Test/Assets/RotateRings/Art/`: 66 PNGs (2.84 MB), selected from home UI, gameplay UI/effects, and ring texture bundles.
- `RotateRings Test/Assets/RotateRings/Audio/`: four decoded original clips: `rr_bgm_level2.wav`, `rr_sfx_wood_break_1.wav`, `rr_sfx_rock_break_1.wav`, `rr_sfx_jelly_break_1.wav`.
- `export_manifest.json` and `audio_manifest.json`: asset name, Unity type, source bundle, path ID, SHA-256 of source bundle, and dimensions or extracted member.
- `asset_inventory.csv`: names/types/path IDs for all Texture2D, Sprite, AudioClip, Font, and Material objects found in the package.
- `home_contact_sheet.png`, `game_contact_sheet.png`, `ring_contact_sheet.png`: visual browsing aids.

## Art direction and implementation notes

The home UI uses rounded, beveled, candy-like controls: warm cream faces, peach/orange rims, bright yellow accents, and soft shading. The exported home assets include `btn_level_yellow`, `home_entry_bg01/02`, `home_bg_deco01`, `icon_home_firstwin`, `icon_home_firstwin_plz`, and `icon_home_leaderboard`.

Gameplay controls and status art use saturated blue, green, yellow, and red with thick pale outlines and soft drop shadows. The atlas has useful ready-made key icons (`game_img_key_*`), settings controls, play/ad badge, move/score panels, and combo/effect textures.

The dedicated ring texture bundle exposes `ringextra`, `ringextra_n`, and `ringextra SDF`; the gameplay effect bundle includes `Tex_ring_256x256_005`. These are texture/material inputs, not complete ring or level artwork. Recreate ring geometry and colors in Unity from the learned level data, sampling these textures only if the material look matches the reference.

The backgrounds are bundled into atlases/sprites. Use home sprites for the lobby and keep a calm solid or gently textured playfield behind the rings; original bundled art does not appear to provide a separate full-screen gameplay background in these selected bundles.

## Re-running

Run `python Tools/extract_art.py` from the workspace root. This refreshes the inventory, selected assets, contact sheets, and manifests.

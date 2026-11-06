# Original fragment shader: recovered active semantics

`effect_shader_recovery_manifest.json` binds four verbatim GLES3 programs to the original APK bundle. `Tools/recover_original_effect_shader.py` reproduces extraction. `HsAppVFXAllEffect_GLES3_program4.glsl` has no keywords, exactly like `mat_cyclefragment_test`'s valid-keyword list.

The source fragment program supports polar UV, particle-custom UV offsets and animated texture offsets. The actual material has `_Main_Tex_CoordMode=0`, `_Main_Tex_Custom_ZW=0`, `_Main_Tex_U_speed=0`, `_Main_Tex_V_speed=0`. Therefore its active UV path reduces to `uv0 * _MainTex_ST.xy + _MainTex_ST.zw`, with no polar remap or animation.

The source alpha is `lerp(texture.r,texture.a,_Main_Tex_A_R) * vertexColor.a * _Main_Tex_Color.a`. Source `_Main_Tex_A_R=1`, so alpha is `texture.a * vertexColor.a * tint.a`. RGB is `texture.rgb * vertexColor.rgb * _Main_Tex_Color.rgb`. There is no active gamma transform or premultiplication; `_GammaRestore` and `_RemapGammaSRGB` are unused in this program.

The helper's `OriginalFragment.shader` directly implements this active formula with `_Tint=(1.26218963,1.26218963,1.26218963,1)`. Its parsed render state matches source Forward: RGB SrcAlpha/OneMinusSrcAlpha, alpha One/OneMinusSrcAlpha, ZWrite0, ZTest4, Cull2. No runtime shader change was necessary after recovering the concrete source.

The optional dissolve and UI-clip variants are retained as research; neither is enabled by this source material. Semantic equivalence for the active material does not establish bit-identical cross-platform raster output, Unity particle random sampling, or original device color management.

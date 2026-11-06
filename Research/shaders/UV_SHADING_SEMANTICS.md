# Original XYStyleLine shader: recovered semantics

`recovery_manifest.json` binds the original source bundle and all recovered GLES3 programs to SHA256. Program5's keyword set **exactly matches** `RingExtra_Palette_material.json`. Program7 adds `_OUTLINE_ON`. These files contain the actual shipped compiled vertex/fragment GLSL, not a guessed shader. `Tools/recover_original_ring_shader.py` reproduces the read-only recovery. UnityPy's default Shader.export omitted these bodies because the serialized pass lacked ordinary subprogram references.

Source material texture IDs resolve to `defaultpackage_share_assets_02_res_art_level_texture_ring_19f779822c64c971b9b5ff13787dc0e7.bundle`:

| Property | Texture name | Path ID | Dimensions |
|---|---|---:|---|
|_MainTex|ringextra|−1429220129537321677|1024×128|
|_NormalMap|ringextra_n|−5665834671891340632|1024×128|
|_SDFTex|ringextra SDF|−2874657492195310327|1024×128|

The material also references a MatCap, but its `_MatCapEnable=0`; active program5 contains no MatCap sampling. Preserve actual active keywords rather than enable unused properties.

## Mesh channels and caps

In program5, lines144–168 rotate local position by `_BlockedShakeAngle` and pass vertex color. `uv0.x` is distance along the entire ribbon; `uv0.y` is across its width. `uv1.x` is total visible length. `uv1.y` supplies a one-based style index when `_StyleIndexFromMesh>.5`; its negative sign marks an extra ribbon. `uv2.y` selects extra A/B/C (round and clamp1..3), effective only when `uv1.y<0`.

Lines171–205 compute these interpolated quantities:

```text
L = max(uv1.x, .0001)
capScale = L < headLength+tailLength ? L/max(headLength+tailLength,.0001) : 1
tail = tailLength*capScale
head = headLength*capScale
headStart = L-head
bodyLength = L-head-tail
tileDensity = bodyLength>.0001 && bodyTileLength>.0001
  ? max(floor(bodyLength/bodyTileLength+.5),1)/bodyLength
  : 1/max(bodyTileLength,.0001)
row = clamp((styleIndexFromMesh ? abs(uv1.y) : styleIndex)-1,0,max(styleRowCount-1,0))
rowHeight = 1/max(styleRowCount,1)
```

The material's authored head/tail length is .3, bodyTileLength2, bodyPhase0. At fragment lines383–455, distance=`uv0.x−WindowStart` and the visible window is `[0,L]`. Tail selects `distance<=tail`; head selects `distance>=headStart`; otherwise body. Tail U=`saturate(distance/tail)`, head U=`saturate((distance-headStart)/head)`, body U=`1-frac((headStart-distance+BodyPhase)*tileDensity)`. This body phase/direction is important; a naive `frac(distance/tileLength)` flips the authored pattern.

Apply selected rect xy origin/zw size with atlas-padding and derivative handling **then multiply ordinary atlas U by .5** (line453). Extra A/B/C uses the atlas's right half directly and does **not** apply this .5. With material rects Tail(0,0,.25,1), Body(.25,0,.5,1), Head(.75,0,.25,1), ordinary U occupies `[0,.5]`. Extra A(.5,0,.25,1), B(.75,0,.125,1), C(.875,0,.125,1) occupies `[.5,1]`. Atlas V includes row×rowHeight.

Program5 lines456–485 clamp derivative lengths first by `_MipClamp`, then by `max(texelSize.x,texelSize.y)*8`; multiply derivative vectors by `_GlobalMipBias.y`, then `textureGrad`. Sharp cap silhouette comes from **atlas alpha** and alphaClip. Ordinary program5 never samples `_SDFTex`; that texture is used for the selection outline variant.

## Color and the apparent round surface

Program5 lines486–509:

```text
linear(c) = c*(c*(.305306017*c+.682171106)+.0125228781)
base = lerp(atlas.rgb, 1, Blend) * linear(Color.rgb)*ColorIntensity
base = lerp(base, luminance(base), RecolorStrength)
base = (base-.5)*Contrast+.5
if UseVertexColor:
  base = ColorfulBlend ? 1-(1-base)*(1-vertexColor.rgb) : base*vertexColor.rgb
alpha = atlas.a*Color.a*(UseVertexColor ? vertexColor.a : 1)
base = lerp(luminance(base),base,Saturation)
```

Material values: Blend.5, Color white, ColorIntensity1, RecolorStrength0, Contrast1, UseVertexColor1, ColorfulBlend0, Saturation1.

Lines510–539 sample the **same mapped UV** in `_NormalMap`; normal=`sample.rgb*2−1`, XY multiplied by NormalScale1.5. Transform tangent-space normal through interpolated world tangent/bitangent/normal and normalize. Material light direction is(−.3,.566666663,.767390966), LightColor(.95,.95,.95), ShadowColor(0,0,0), LightWrap1, LightStrength1, LightIntensity1.4. It does not use a Unity scene light in this shader:

```text
ndotl = dot(normalize(worldNormal),normalize(LightDir))
wrapped = lerp(saturate(ndotl), ndotl*.5+.5, LightWrap)
light = lerp(linear(ShadowColor),linear(LightColor),wrapped)
screenMix = 1-(1-base)*(1-LightStrength*light)
rgb = base*screenMix*LightIntensity
```

This is the actual unusual screen/multiply formula; generic Standard/Lambert shading cannot reproduce it. The ribbon can remain geometrically flat, since the atlas and tangent normal produce the rounded look.

Lines540–544 discard when `windowMask*alpha < AlphaClip(.25)` and output `maskSoftness*windowMask*alpha`. `_StyleLineRectMaskRect` is in fragment screen coordinates (lines372–382), default±100000.

## SDF selection outline

Program7 lines547–581 conditionally samples `_SDFTex` **alpha**, using same mapped UV and gradients. A full derivation can follow the literal code directly. Critical facts:

- `outlineHalf=OutlineWidth*.5*vs_TEXCOORD0.w`; program7 emits w=`uv2.x` for per-vertex outline weight; non-outline program5 emits w=1.
- Softness=`max(max(length(mappedClampedDx),length(mappedClampedDy))*SDFTex_TexelSize.z*OutlineSoftness,.0001)`. These are the selected atlas-rect derivatives after both gradient-length clamps, before applying `_GlobalMipBias.y`; they are not raw path-UV derivatives. The texture lookup uses the same gradients multiplied by `_GlobalMipBias.y`.
- OutlineMode0 is inner: threshold pair(.5+2*outlineHalf,.5); mode1 centered:(.5+outlineHalf,.5−outlineHalf); mode2 outer:(.5,.5−2*outlineHalf).
- Compute two saturating linear ramps from `(sdfAlpha-threshold)/softness`; difference gives outline coverage.
- OutlineColor goes through the same source linearization polynomial. `_OutlineUseVertColor>.5` adds vertex RGB to outline RGB (not a multiply).
- The outline path preserves the source lit RGB and alpha composite; its exact arithmetic is retained verbatim in lines572–589.

No runtime files were edited for this shader research. The platform18 decompressed bytes are retained for later SPIRV analysis if needed; GLES3 already provides the concrete source behavior.

## Original project color space

`original_player_settings.json` records the APK globalgamemanagers PlayerSettings pathID1, Unity6000.3.15f1, `m_ActiveColorSpace=0` (Gamma). Byte offset7444 (object-relative260) is `00000000`. The builtin UnityPy schema reads880 of884bytes because of unknown trailing4bytes; early field alignment is independently documented with adjacent product name/screen dimensions. A Linear clone project adds color conversions absent from the original Gamma project and can wash out the same shader/palette. Preserve source Gamma before adjusting colors.

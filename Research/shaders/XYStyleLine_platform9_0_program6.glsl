Keywords { "_OUTLINE_ON" "_UNDERLAY_NONE" }
"#ifdef VERTEX
#version 300 es

#define HLSLCC_ENABLE_UNIFORM_BUFFERS 1
#if HLSLCC_ENABLE_UNIFORM_BUFFERS
#define UNITY_UNIFORM
#else
#define UNITY_UNIFORM uniform
#endif
#define UNITY_SUPPORTS_UNIFORM_LOCATION 1
#if UNITY_SUPPORTS_UNIFORM_LOCATION
#define UNITY_LOCATION(x) layout(location = x)
#define UNITY_BINDING(x) layout(binding = x, std140)
#else
#define UNITY_LOCATION(x)
#define UNITY_BINDING(x) layout(std140)
#endif
uniform 	vec4 hlslcc_mtx4x4unity_MatrixVP[4];
#if HLSLCC_ENABLE_UNIFORM_BUFFERS
UNITY_BINDING(1) uniform UnityPerDraw {
#endif
	UNITY_UNIFORM vec4                hlslcc_mtx4x4unity_ObjectToWorld[4];
	UNITY_UNIFORM vec4 Xhlslcc_UnusedXhlslcc_mtx4x4unity_WorldToObject[4];
	UNITY_UNIFORM vec4 Xhlslcc_UnusedXunity_LODFade;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedXunity_WorldTransformParams;
	UNITY_UNIFORM vec4 Xhlslcc_UnusedXunity_RenderingLayer;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedXunity_LightData;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedXunity_LightIndices[2];
	UNITY_UNIFORM vec4 Xhlslcc_UnusedXunity_ProbesOcclusion;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedXunity_SpecCube0_HDR;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedXunity_SpecCube1_HDR;
	UNITY_UNIFORM vec4 Xhlslcc_UnusedXunity_SpecCube0_BoxMax;
	UNITY_UNIFORM vec4 Xhlslcc_UnusedXunity_SpecCube0_BoxMin;
	UNITY_UNIFORM vec4 Xhlslcc_UnusedXunity_SpecCube0_ProbePosition;
	UNITY_UNIFORM vec4 Xhlslcc_UnusedXunity_SpecCube0_Rotation;
	UNITY_UNIFORM vec4 Xhlslcc_UnusedXunity_SpecCube1_BoxMax;
	UNITY_UNIFORM vec4 Xhlslcc_UnusedXunity_SpecCube1_BoxMin;
	UNITY_UNIFORM vec4 Xhlslcc_UnusedXunity_SpecCube1_ProbePosition;
	UNITY_UNIFORM vec4 Xhlslcc_UnusedXunity_SpecCube1_Rotation;
	UNITY_UNIFORM vec4 Xhlslcc_UnusedXunity_LightmapST;
	UNITY_UNIFORM vec4 Xhlslcc_UnusedXunity_DynamicLightmapST;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedXunity_SHAr;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedXunity_SHAg;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedXunity_SHAb;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedXunity_SHBr;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedXunity_SHBg;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedXunity_SHBb;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedXunity_SHC;
	UNITY_UNIFORM vec4 Xhlslcc_UnusedXunity_RendererBounds_Min;
	UNITY_UNIFORM vec4 Xhlslcc_UnusedXunity_RendererBounds_Max;
	UNITY_UNIFORM vec4 Xhlslcc_UnusedXhlslcc_mtx4x4unity_MatrixPreviousM[4];
	UNITY_UNIFORM vec4 Xhlslcc_UnusedXhlslcc_mtx4x4unity_MatrixPreviousMI[4];
	UNITY_UNIFORM vec4 Xhlslcc_UnusedXunity_MotionVectorsParams;
	UNITY_UNIFORM vec4 Xhlslcc_UnusedXunity_SpriteColor;
	UNITY_UNIFORM vec4 Xhlslcc_UnusedXunity_SpriteProps;
#if HLSLCC_ENABLE_UNIFORM_BUFFERS
};
#endif
#if HLSLCC_ENABLE_UNIFORM_BUFFERS
UNITY_BINDING(0) uniform UnityPerMaterial {
#endif
	UNITY_UNIFORM vec4                _MainTex_TexelSize;
	UNITY_UNIFORM mediump vec4                _Color;
	UNITY_UNIFORM mediump float                _ColorIntensity;
	UNITY_UNIFORM mediump float                _Blend;
	UNITY_UNIFORM mediump float                _RecolorStrength;
	UNITY_UNIFORM mediump float                _Contrast;
	UNITY_UNIFORM mediump float                _Saturation;
	UNITY_UNIFORM mediump float                _UseVertexColor;
	UNITY_UNIFORM mediump float                _ColorfulBlend;
	UNITY_UNIFORM float Xhlslcc_UnusedX_AlphaClip;
	UNITY_UNIFORM float Xhlslcc_UnusedX_VisibleLength;
	UNITY_UNIFORM float                _TailLength;
	UNITY_UNIFORM float                _HeadLength;
	UNITY_UNIFORM float                _BodyTileLength;
	UNITY_UNIFORM float                _BodyPhase;
	UNITY_UNIFORM vec4                _TailRect;
	UNITY_UNIFORM vec4                _BodyRect;
	UNITY_UNIFORM vec4                _HeadRect;
	UNITY_UNIFORM float                _StyleRowCount;
	UNITY_UNIFORM float                _StyleIndexFromMesh;
	UNITY_UNIFORM float                _StyleIndex;
	UNITY_UNIFORM float                _WindowStart;
	UNITY_UNIFORM float                _AtlasPaddingPixels;
	UNITY_UNIFORM float                _MipClamp;
	UNITY_UNIFORM float                _BlockedShakeAngle;
	UNITY_UNIFORM vec4 Xhlslcc_UnusedX_ExtraRectA;
	UNITY_UNIFORM vec4 Xhlslcc_UnusedX_ExtraRectB;
	UNITY_UNIFORM vec4 Xhlslcc_UnusedX_ExtraRectC;
	UNITY_UNIFORM float Xhlslcc_UnusedX_NormalScale;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedX_MatCapColor;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_MatCapStrength;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_MatCapLOD;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_MatCapRotation;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_MatCapBlendMode;
	UNITY_UNIFORM vec4 Xhlslcc_UnusedX_LightDir;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedX_LightColor;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_LightIntensity;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedX_ShadowColor;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_LightStrength;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_LightWrap;
	UNITY_UNIFORM vec4                _SDFTex_TexelSize;
	UNITY_UNIFORM mediump vec4                _OutlineColor;
	UNITY_UNIFORM mediump float                _OutlineWidth;
	UNITY_UNIFORM mediump float                _OutlineSoftness;
	UNITY_UNIFORM mediump float                _OutlineMode;
	UNITY_UNIFORM mediump float                _OutlineUseVertColor;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedX_UnderlayColor;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_UnderlayOffsetX;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_UnderlayOffsetY;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_UnderlayDilate;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_UnderlaySoftness;
	UNITY_UNIFORM vec4                _StyleLineRectMaskRect;
	UNITY_UNIFORM vec4                _StyleLineRectMaskSoftness;
#if HLSLCC_ENABLE_UNIFORM_BUFFERS
};
#endif
in highp vec4 in_POSITION0;
in mediump vec4 in_COLOR0;
in highp vec2 in_TEXCOORD0;
in highp vec2 in_TEXCOORD1;
in highp vec2 in_TEXCOORD2;
out mediump vec4 vs_COLOR0;
out highp vec4 vs_TEXCOORD0;
out highp vec4 vs_TEXCOORD1;
out highp vec4 vs_TEXCOORD6;
vec4 u_xlat0;
bvec3 u_xlatb0;
vec4 u_xlat1;
vec3 u_xlat2;
bool u_xlatb2;
vec2 u_xlat3;
bool u_xlatb3;
float u_xlat4;
bool u_xlatb4;
void main()
{
    u_xlat0.x = _BlockedShakeAngle * 0.0174532924;
    u_xlat1.x = cos(u_xlat0.x);
    u_xlat0.x = sin(u_xlat0.x);
    u_xlat0.xy = u_xlat0.xx * in_POSITION0.yx;
    u_xlat2.x = in_POSITION0.y * u_xlat1.x + u_xlat0.y;
    u_xlat0.x = in_POSITION0.x * u_xlat1.x + (-u_xlat0.x);
    u_xlat1 = u_xlat2.xxxx * hlslcc_mtx4x4unity_ObjectToWorld[1];
    u_xlat0 = hlslcc_mtx4x4unity_ObjectToWorld[0] * u_xlat0.xxxx + u_xlat1;
    u_xlat0 = hlslcc_mtx4x4unity_ObjectToWorld[2] * in_POSITION0.zzzz + u_xlat0;
    u_xlat0 = u_xlat0 + hlslcc_mtx4x4unity_ObjectToWorld[3];
    u_xlat1 = u_xlat0.yyyy * hlslcc_mtx4x4unity_MatrixVP[1];
    u_xlat1 = hlslcc_mtx4x4unity_MatrixVP[0] * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = hlslcc_mtx4x4unity_MatrixVP[2] * u_xlat0.zzzz + u_xlat1;
    gl_Position = hlslcc_mtx4x4unity_MatrixVP[3] * u_xlat0.wwww + u_xlat1;
    vs_COLOR0 = in_COLOR0;
    u_xlat0.x = roundEven(in_TEXCOORD2.y);
    u_xlat0.x = max(u_xlat0.x, 1.0);
    u_xlat0.x = min(u_xlat0.x, 3.0);
    u_xlatb2 = in_TEXCOORD2.y>=0.5;
    u_xlat0.x = (u_xlatb2) ? u_xlat0.x : 1.0;
    u_xlatb2 = in_TEXCOORD1.y<0.0;
    u_xlat2.x = u_xlatb2 ? 1.0 : float(0.0);
    vs_TEXCOORD0.z = u_xlat0.x * u_xlat2.x;
    vs_TEXCOORD0.xy = in_TEXCOORD0.xy;
    vs_TEXCOORD0.w = in_TEXCOORD2.x;
    u_xlat0.x = max(_BodyTileLength, 9.99999975e-05);
    u_xlat0.x = float(1.0) / u_xlat0.x;
    u_xlat2.x = _HeadLength + _TailLength;
    u_xlat4 = max(u_xlat2.x, 9.99999975e-05);
    u_xlat3.y = max(in_TEXCOORD1.x, 9.99999975e-05);
    u_xlat4 = u_xlat3.y / u_xlat4;
    u_xlatb2 = u_xlat3.y<u_xlat2.x;
    u_xlat2.x = (u_xlatb2) ? u_xlat4 : 1.0;
    u_xlat3.x = (-_HeadLength) * u_xlat2.x + u_xlat3.y;
    vs_TEXCOORD1.yz = u_xlat3.xy;
    u_xlat4 = (-_TailLength) * u_xlat2.x + u_xlat3.x;
    u_xlat2.xz = u_xlat2.xx * vec2(_TailLength, _HeadLength);
    u_xlat1.x = u_xlat4 / _BodyTileLength;
    u_xlat1.x = u_xlat1.x + 0.5;
    u_xlat1.x = floor(u_xlat1.x);
    u_xlat1.x = max(u_xlat1.x, 1.0);
    u_xlat1.x = u_xlat1.x / u_xlat4;
    u_xlatb4 = 9.99999975e-05<u_xlat4;
    u_xlatb3 = 9.99999975e-05<_BodyTileLength;
    u_xlatb4 = u_xlatb4 && u_xlatb3;
    vs_TEXCOORD1.w = (u_xlatb4) ? u_xlat1.x : u_xlat0.x;
    vs_TEXCOORD1.x = u_xlat2.x;
    u_xlatb0.xz = lessThan(vec4(9.99999975e-05, 0.0, 9.99999975e-05, 0.0), u_xlat2.xxzx).xz;
    u_xlat2.xz = vec2(1.0, 1.0) / u_xlat2.xz;
    vs_TEXCOORD6.x = u_xlatb0.x ? u_xlat2.x : 0.0;
    vs_TEXCOORD6.y = u_xlatb0.z ? u_xlat2.z : 0.0;
;
    u_xlatb0.x = 0.5<_StyleIndexFromMesh;
    u_xlat0.x = (u_xlatb0.x) ? abs(in_TEXCOORD1.y) : _StyleIndex;
    u_xlat0.x = u_xlat0.x + -1.0;
    u_xlat0.y = _StyleRowCount + -1.0;
    u_xlat0.xy = max(u_xlat0.xy, vec2(0.0, 0.0));
    vs_TEXCOORD6.w = min(u_xlat0.y, u_xlat0.x);
    u_xlat0.x = max(_StyleRowCount, 1.0);
    vs_TEXCOORD6.z = float(1.0) / u_xlat0.x;
    return;
}

#endif
#ifdef FRAGMENT
#version 300 es
#ifdef GL_EXT_shader_texture_lod
#extension GL_EXT_shader_texture_lod : enable
#endif

precision highp float;
precision highp int;
#define HLSLCC_ENABLE_UNIFORM_BUFFERS 1
#if HLSLCC_ENABLE_UNIFORM_BUFFERS
#define UNITY_UNIFORM
#else
#define UNITY_UNIFORM uniform
#endif
#define UNITY_SUPPORTS_UNIFORM_LOCATION 1
#if UNITY_SUPPORTS_UNIFORM_LOCATION
#define UNITY_LOCATION(x) layout(location = x)
#define UNITY_BINDING(x) layout(binding = x, std140)
#else
#define UNITY_LOCATION(x)
#define UNITY_BINDING(x) layout(std140)
#endif
uniform 	vec2 _GlobalMipBias;
#if HLSLCC_ENABLE_UNIFORM_BUFFERS
UNITY_BINDING(0) uniform UnityPerMaterial {
#endif
	UNITY_UNIFORM vec4                _MainTex_TexelSize;
	UNITY_UNIFORM mediump vec4                _Color;
	UNITY_UNIFORM mediump float                _ColorIntensity;
	UNITY_UNIFORM mediump float                _Blend;
	UNITY_UNIFORM mediump float                _RecolorStrength;
	UNITY_UNIFORM mediump float                _Contrast;
	UNITY_UNIFORM mediump float                _Saturation;
	UNITY_UNIFORM mediump float                _UseVertexColor;
	UNITY_UNIFORM mediump float                _ColorfulBlend;
	UNITY_UNIFORM float Xhlslcc_UnusedX_AlphaClip;
	UNITY_UNIFORM float Xhlslcc_UnusedX_VisibleLength;
	UNITY_UNIFORM float                _TailLength;
	UNITY_UNIFORM float                _HeadLength;
	UNITY_UNIFORM float                _BodyTileLength;
	UNITY_UNIFORM float                _BodyPhase;
	UNITY_UNIFORM vec4                _TailRect;
	UNITY_UNIFORM vec4                _BodyRect;
	UNITY_UNIFORM vec4                _HeadRect;
	UNITY_UNIFORM float                _StyleRowCount;
	UNITY_UNIFORM float                _StyleIndexFromMesh;
	UNITY_UNIFORM float                _StyleIndex;
	UNITY_UNIFORM float                _WindowStart;
	UNITY_UNIFORM float                _AtlasPaddingPixels;
	UNITY_UNIFORM float                _MipClamp;
	UNITY_UNIFORM float                _BlockedShakeAngle;
	UNITY_UNIFORM vec4 Xhlslcc_UnusedX_ExtraRectA;
	UNITY_UNIFORM vec4 Xhlslcc_UnusedX_ExtraRectB;
	UNITY_UNIFORM vec4 Xhlslcc_UnusedX_ExtraRectC;
	UNITY_UNIFORM float Xhlslcc_UnusedX_NormalScale;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedX_MatCapColor;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_MatCapStrength;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_MatCapLOD;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_MatCapRotation;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_MatCapBlendMode;
	UNITY_UNIFORM vec4 Xhlslcc_UnusedX_LightDir;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedX_LightColor;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_LightIntensity;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedX_ShadowColor;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_LightStrength;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_LightWrap;
	UNITY_UNIFORM vec4                _SDFTex_TexelSize;
	UNITY_UNIFORM mediump vec4                _OutlineColor;
	UNITY_UNIFORM mediump float                _OutlineWidth;
	UNITY_UNIFORM mediump float                _OutlineSoftness;
	UNITY_UNIFORM mediump float                _OutlineMode;
	UNITY_UNIFORM mediump float                _OutlineUseVertColor;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedX_UnderlayColor;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_UnderlayOffsetX;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_UnderlayOffsetY;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_UnderlayDilate;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_UnderlaySoftness;
	UNITY_UNIFORM vec4                _StyleLineRectMaskRect;
	UNITY_UNIFORM vec4                _StyleLineRectMaskSoftness;
#if HLSLCC_ENABLE_UNIFORM_BUFFERS
};
#endif
UNITY_LOCATION(0) uniform mediump sampler2D _MainTex;
UNITY_LOCATION(1) uniform mediump sampler2D _SDFTex;
in mediump  vec4 vs_COLOR0;
in highp  vec4 vs_TEXCOORD0;
in highp  vec4 vs_TEXCOORD1;
in highp  vec4 vs_TEXCOORD6;
layout(location = 0) out mediump vec4 SV_Target0;
vec2 u_xlat0;
vec4 u_xlat1;
bvec2 u_xlatb1;
vec4 u_xlat2;
bool u_xlatb2;
vec4 u_xlat3;
mediump vec4 u_xlat16_3;
vec4 u_xlat4;
bvec2 u_xlatb4;
vec4 u_xlat5;
mediump vec4 u_xlat16_6;
mediump vec3 u_xlat16_7;
mediump vec3 u_xlat16_8;
float u_xlat9;
mediump float u_xlat16_9;
bool u_xlatb9;
vec3 u_xlat10;
bool u_xlatb10;
vec2 u_xlat11;
bvec2 u_xlatb11;
mediump vec3 u_xlat16_15;
mediump vec3 u_xlat16_16;
vec2 u_xlat18;
bool u_xlatb18;
vec2 u_xlat19;
vec2 u_xlat20;
bool u_xlatb20;
vec2 u_xlat21;
float u_xlat27;
float u_xlat28;
bool u_xlatb28;
bool u_xlatb30;
mediump float u_xlat16_33;
void main()
{
vec4 hlslcc_FragCoord = vec4(gl_FragCoord.xyz, 1.0/gl_FragCoord.w);
    u_xlat0.xy = hlslcc_FragCoord.xy + (-_StyleLineRectMaskRect.xy);
    u_xlat18.xy = (-hlslcc_FragCoord.xy) + _StyleLineRectMaskRect.zw;
    u_xlat0.xy = min(u_xlat18.xy, u_xlat0.xy);
    u_xlat18.x = min(u_xlat0.y, u_xlat0.x);
    u_xlatb18 = u_xlat18.x<0.0;
    if(u_xlatb18){discard;}
    u_xlat18.xy = max(_StyleLineRectMaskSoftness.xy, vec2(9.99999975e-05, 9.99999975e-05));
    u_xlat0.xy = u_xlat0.xy / u_xlat18.xy;
    u_xlat0.xy = clamp(u_xlat0.xy, 0.0, 1.0);
    u_xlat0.x = min(u_xlat0.y, u_xlat0.x);
    u_xlat9 = vs_TEXCOORD0.x + (-_WindowStart);
    u_xlatb18 = u_xlat9>=0.0;
    u_xlatb1.xy = greaterThanEqual(vs_TEXCOORD1.zxzz, vec4(u_xlat9)).xy;
    u_xlatb18 = u_xlatb18 && u_xlatb1.x;
    u_xlat27 = u_xlatb18 ? 1.0 : float(0.0);
    u_xlat1.xz = dFdx(vs_TEXCOORD0.xy);
    u_xlat2.xy = dFdy(vs_TEXCOORD0.xy);
    u_xlat3.w = u_xlat9 * vs_TEXCOORD6.x;
    u_xlat3.w = clamp(u_xlat3.w, 0.0, 1.0);
    u_xlatb28 = u_xlat9>=vs_TEXCOORD1.y;
    u_xlat20.x = u_xlat9 + (-vs_TEXCOORD1.y);
    u_xlat3.y = u_xlat20.x * vs_TEXCOORD6.y;
    u_xlat3.y = clamp(u_xlat3.y, 0.0, 1.0);
    u_xlat20.x = (-u_xlat9) + vs_TEXCOORD1.y;
    u_xlat20.x = u_xlat20.x + _BodyPhase;
    u_xlat20.x = u_xlat20.x * vs_TEXCOORD1.w;
    u_xlat20.x = fract(u_xlat20.x);
    u_xlat4.y = (-u_xlat20.x) + 1.0;
    u_xlat5 = (bool(u_xlatb28)) ? _HeadRect : _BodyRect;
    u_xlat3.xz = vs_TEXCOORD6.yx;
    u_xlat4.x = vs_TEXCOORD1.w;
    u_xlat20.xy = (bool(u_xlatb28)) ? u_xlat3.xy : u_xlat4.xy;
    u_xlat4 = (u_xlatb1.y) ? _TailRect : u_xlat5;
    u_xlat20.xy = (u_xlatb1.y) ? u_xlat3.zw : u_xlat20.xy;
    u_xlat3.xy = _MainTex_TexelSize.xy * vec2(_AtlasPaddingPixels);
    u_xlatb28 = 0.0<_AtlasPaddingPixels;
    u_xlatb30 = vs_TEXCOORD1.x<u_xlat9;
    u_xlatb9 = u_xlat9<vs_TEXCOORD1.y;
    u_xlatb9 = u_xlatb9 && u_xlatb30;
    u_xlat3.z = 0.0;
    u_xlat21.xy = (u_xlatb1.y) ? u_xlat3.xz : u_xlat3.zx;
    u_xlat21.xy = (bool(u_xlatb9)) ? vec2(0.0, 0.0) : u_xlat21.xy;
    u_xlat10.xz = (bool(u_xlatb28)) ? u_xlat21.xy : u_xlat3.xx;
    u_xlat9 = (-u_xlat10.x) + u_xlat4.z;
    u_xlat9 = (-u_xlat10.z) + u_xlat9;
    u_xlat28 = (-u_xlat3.y) * 2.0 + u_xlat4.w;
    u_xlat10.x = u_xlat10.x + u_xlat4.x;
    u_xlat20.x = u_xlat9 * u_xlat20.x;
    u_xlat9 = u_xlat20.y * u_xlat9 + u_xlat10.x;
    u_xlat10.x = _AtlasPaddingPixels * _MainTex_TexelSize.y + u_xlat4.y;
    u_xlat10.x = vs_TEXCOORD0.y * u_xlat28 + u_xlat10.x;
    u_xlat3.xy = u_xlat4.zw + u_xlat4.xy;
    u_xlat9 = max(u_xlat4.x, u_xlat9);
    u_xlat5.x = min(u_xlat3.x, u_xlat9);
    u_xlat9 = max(u_xlat4.y, u_xlat10.x);
    u_xlat9 = min(u_xlat3.y, u_xlat9);
    u_xlat5.y = vs_TEXCOORD6.w * vs_TEXCOORD6.z + u_xlat9;
    u_xlat1.x = u_xlat1.x * u_xlat20.x;
    u_xlat1.y = u_xlat1.z * u_xlat28;
    u_xlat3.x = u_xlat2.x * u_xlat20.x;
    u_xlat3.y = u_xlat2.y * u_xlat28;
    u_xlat9 = dot(u_xlat1.xy, u_xlat1.xy);
    u_xlat9 = sqrt(u_xlat9);
    u_xlat19.x = dot(u_xlat3.xy, u_xlat3.xy);
    u_xlat19.x = sqrt(u_xlat19.x);
    u_xlatb28 = _MipClamp<u_xlat9;
    u_xlat9 = _MipClamp / u_xlat9;
    u_xlat2.xy = vec2(u_xlat9) * u_xlat1.xy;
    u_xlat1.xy = (bool(u_xlatb28)) ? u_xlat2.xy : u_xlat1.xy;
    u_xlatb9 = _MipClamp<u_xlat19.x;
    u_xlat19.x = _MipClamp / u_xlat19.x;
    u_xlat19.xy = u_xlat19.xx * u_xlat3.xy;
    u_xlat19.xy = (bool(u_xlatb9)) ? u_xlat19.xy : u_xlat3.xy;
    u_xlat9 = max(_MainTex_TexelSize.y, _MainTex_TexelSize.x);
    u_xlat9 = u_xlat9 * 8.0;
    u_xlat2.x = dot(u_xlat1.xy, u_xlat1.xy);
    u_xlat2.y = dot(u_xlat19.xy, u_xlat19.xy);
    u_xlat2.xy = sqrt(u_xlat2.xy);
    u_xlatb20 = u_xlat9<u_xlat2.x;
    u_xlat2.x = u_xlat9 / u_xlat2.x;
    u_xlat2.xw = u_xlat1.xy * u_xlat2.xx;
    u_xlat1.xy = (bool(u_xlatb20)) ? u_xlat2.xw : u_xlat1.xy;
    u_xlatb2 = u_xlat9<u_xlat2.y;
    u_xlat9 = u_xlat9 / u_xlat2.y;
    u_xlat11.xy = vec2(u_xlat9) * u_xlat19.xy;
    u_xlat1.zw = (bool(u_xlatb2)) ? u_xlat11.xy : u_xlat19.xy;
    u_xlat2 = u_xlat1 * _GlobalMipBias.yyyy;
    u_xlat16_3 = textureGrad(_MainTex, u_xlat5.xy, vec4(u_xlat2.xyxx).xy, vec4(u_xlat2.zwzz).xy);
    u_xlat16_6.xyz = (-u_xlat16_3.xyz) + vec3(1.0, 1.0, 1.0);
    u_xlat16_6.xyz = vec3(vec3(_Blend, _Blend, _Blend)) * u_xlat16_6.xyz + u_xlat16_3.xyz;
    u_xlat3.xyz = _Color.xyz * vec3(0.305306017, 0.305306017, 0.305306017) + vec3(0.682171106, 0.682171106, 0.682171106);
    u_xlat3.xyz = _Color.xyz * u_xlat3.xyz + vec3(0.0125228781, 0.0125228781, 0.0125228781);
    u_xlat3.xyz = u_xlat3.xyz * _Color.xyz;
    u_xlat16_7.xyz = u_xlat3.xyz * vec3(_ColorIntensity);
    u_xlat16_8.xyz = u_xlat16_6.xyz * u_xlat16_7.xyz;
    u_xlat16_3.w = u_xlat16_3.w * _Color.w;
    u_xlat16_33 = dot(u_xlat16_8.xyz, vec3(0.298999995, 0.587000012, 0.114));
    u_xlat16_6.xyz = (-u_xlat16_6.xyz) * u_xlat16_7.xyz + vec3(u_xlat16_33);
    u_xlat16_6.xyz = vec3(vec3(_RecolorStrength, _RecolorStrength, _RecolorStrength)) * u_xlat16_6.xyz + u_xlat16_8.xyz;
    u_xlat16_6.xyz = u_xlat16_6.xyz + vec3(-0.5, -0.5, -0.5);
    u_xlat16_3.xyz = vec3(vec3(_Contrast, _Contrast, _Contrast)) * u_xlat16_6.xyz + vec3(0.5, 0.5, 0.5);
    u_xlatb4.xy = lessThan(vec4(0.5, 0.5, 0.0, 0.0), vec4(_UseVertexColor, _ColorfulBlend, _UseVertexColor, _UseVertexColor)).xy;
    u_xlat16_6.xyz = (-u_xlat16_3.xyz) + vec3(1.0, 1.0, 1.0);
    u_xlat16_7.xyz = (-vs_COLOR0.xyz) + vec3(1.0, 1.0, 1.0);
    u_xlat16_6.xyz = (-u_xlat16_6.xyz) * u_xlat16_7.xyz + vec3(1.0, 1.0, 1.0);
    u_xlat16_7.xyz = u_xlat16_3.xyz * vs_COLOR0.xyz;
    u_xlat16_6.xyz = (u_xlatb4.y) ? u_xlat16_6.xyz : u_xlat16_7.xyz;
    u_xlat16_6.w = u_xlat16_3.w * vs_COLOR0.w;
    u_xlat16_3 = (u_xlatb4.x) ? u_xlat16_6.wxyz : u_xlat16_3.wxyz;
    u_xlat16_6.x = dot(u_xlat16_3.yzw, vec3(0.298999995, 0.587000012, 0.114));
    u_xlat16_15.xyz = u_xlat16_3.yzw + (-u_xlat16_6.xxx);
    u_xlat16_6.xyz = vec3(_Saturation) * u_xlat16_15.xyz + u_xlat16_6.xxx;
    u_xlatb9 = 0.00100000005<vs_TEXCOORD0.w;
    u_xlatb9 = u_xlatb9 && u_xlatb18;
    if(u_xlatb9){
        u_xlat16_9 = textureGrad(_SDFTex, u_xlat5.xy, vec4(u_xlat2.xyxx).xy, vec4(u_xlat2.zwzz).xy).w;
        u_xlat18.x = _OutlineWidth * 0.5;
        u_xlat2.x = u_xlat18.x * vs_TEXCOORD0.w;
        u_xlat1.x = dot(u_xlat1.xy, u_xlat1.xy);
        u_xlat1.y = dot(u_xlat1.zw, u_xlat1.zw);
        u_xlat1.xy = sqrt(u_xlat1.xy);
        u_xlat1.x = max(u_xlat1.y, u_xlat1.x);
        u_xlat1.x = u_xlat1.x * _SDFTex_TexelSize.z;
        u_xlat16_33 = u_xlat1.x * _OutlineSoftness;
        u_xlat1.x = max(u_xlat16_33, 9.99999975e-05);
        u_xlatb10 = _OutlineMode<0.5;
        u_xlat19.x = u_xlat2.x * 2.0 + 0.5;
        u_xlatb11.xy = lessThan(vec4(1.5, 0.5, 0.0, 0.0), vec4(_OutlineMode, _OutlineUseVertColor, _OutlineMode, _OutlineMode)).xy;
        u_xlat28 = (-u_xlat2.x) * 2.0 + 0.5;
        u_xlat16_33 = (-u_xlat18.x) * vs_TEXCOORD0.w + 0.5;
        u_xlat16_7.x = u_xlat18.x * vs_TEXCOORD0.w + 0.5;
        u_xlat16_33 = (u_xlatb11.x) ? u_xlat28 : u_xlat16_33;
        u_xlat16_7.x = (u_xlatb11.x) ? 0.5 : u_xlat16_7.x;
        u_xlat16_8.y = (u_xlatb10) ? 0.5 : u_xlat16_33;
        u_xlat16_8.x = (u_xlatb10) ? u_xlat19.x : u_xlat16_7.x;
        u_xlat16_7.xy = vec2(u_xlat16_9) + (-u_xlat16_8.xy);
        u_xlat16_7.xy = u_xlat16_7.xy / u_xlat1.xx;
        u_xlat16_7.xy = clamp(u_xlat16_7.xy, 0.0, 1.0);
        u_xlat1.xyz = _OutlineColor.xyz * vec3(0.305306017, 0.305306017, 0.305306017) + vec3(0.682171106, 0.682171106, 0.682171106);
        u_xlat1.xyz = _OutlineColor.xyz * u_xlat1.xyz + vec3(0.0125228781, 0.0125228781, 0.0125228781);
        u_xlat2.xyw = u_xlat1.xyz * _OutlineColor.xyz;
        u_xlat16_8.xyz = _OutlineColor.xyz * u_xlat1.xyz + vs_COLOR0.xyz;
        u_xlat16_8.xyz = (u_xlatb11.y) ? u_xlat16_8.xyz : u_xlat2.xyw;
        u_xlat16_33 = (-u_xlat16_7.x) + u_xlat16_7.y;
        u_xlat16_7.x = u_xlat16_3.x * u_xlat16_7.x;
        u_xlat16_3.x = _OutlineColor.w * u_xlat16_33 + u_xlat16_7.x;
        u_xlat16_16.xyz = u_xlat16_6.xyz + (-u_xlat16_8.xyz);
        SV_Target0.xyz = u_xlat16_7.xxx * u_xlat16_16.xyz + u_xlat16_8.xyz;
    } else {
        SV_Target0.xyz = u_xlat16_6.xyz;
    }
    u_xlat9 = u_xlat27 * u_xlat16_3.x;
    SV_Target0.w = u_xlat0.x * u_xlat9;
    return;
}

#endif
"
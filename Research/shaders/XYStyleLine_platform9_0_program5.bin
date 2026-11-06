#ifdef VERTEX
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
	UNITY_UNIFORM vec4                hlslcc_mtx4x4unity_WorldToObject[4];
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
	UNITY_UNIFORM float                _AlphaClip;
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
	UNITY_UNIFORM vec4                _ExtraRectA;
	UNITY_UNIFORM vec4                _ExtraRectB;
	UNITY_UNIFORM vec4                _ExtraRectC;
	UNITY_UNIFORM float                _NormalScale;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedX_MatCapColor;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_MatCapStrength;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_MatCapLOD;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_MatCapRotation;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_MatCapBlendMode;
	UNITY_UNIFORM vec4                _LightDir;
	UNITY_UNIFORM mediump vec4                _LightColor;
	UNITY_UNIFORM mediump float                _LightIntensity;
	UNITY_UNIFORM mediump vec4                _ShadowColor;
	UNITY_UNIFORM mediump float                _LightStrength;
	UNITY_UNIFORM mediump float                _LightWrap;
	UNITY_UNIFORM vec4 Xhlslcc_UnusedX_SDFTex_TexelSize;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedX_OutlineColor;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_OutlineWidth;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_OutlineSoftness;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_OutlineMode;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_OutlineUseVertColor;
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
in highp vec3 in_NORMAL0;
in highp vec4 in_TANGENT0;
in highp vec2 in_TEXCOORD0;
in highp vec2 in_TEXCOORD1;
in highp vec2 in_TEXCOORD2;
out mediump vec4 vs_COLOR0;
out highp vec4 vs_TEXCOORD0;
out highp vec4 vs_TEXCOORD1;
out highp vec4 vs_TEXCOORD6;
out highp vec3 vs_TEXCOORD2;
out highp vec3 vs_TEXCOORD3;
out highp vec3 vs_TEXCOORD4;
vec3 u_xlat0;
vec3 u_xlat1;
vec4 u_xlat2;
vec4 u_xlat3;
vec3 u_xlat4;
bvec2 u_xlatb4;
vec3 u_xlat5;
float u_xlat8;
bool u_xlatb8;
float u_xlat12;
bool u_xlatb12;
bool u_xlatb13;
void main()
{
    u_xlat0.x = _BlockedShakeAngle * 0.0174532924;
    u_xlat1.x = cos(u_xlat0.x);
    u_xlat0.x = sin(u_xlat0.x);
    u_xlat4.xy = u_xlat0.xx * in_POSITION0.yx;
    u_xlat8 = in_POSITION0.y * u_xlat1.x + u_xlat4.y;
    u_xlat4.x = in_POSITION0.x * u_xlat1.x + (-u_xlat4.x);
    u_xlat2 = vec4(u_xlat8) * hlslcc_mtx4x4unity_ObjectToWorld[1];
    u_xlat2 = hlslcc_mtx4x4unity_ObjectToWorld[0] * u_xlat4.xxxx + u_xlat2;
    u_xlat2 = hlslcc_mtx4x4unity_ObjectToWorld[2] * in_POSITION0.zzzz + u_xlat2;
    u_xlat2 = u_xlat2 + hlslcc_mtx4x4unity_ObjectToWorld[3];
    u_xlat3 = u_xlat2.yyyy * hlslcc_mtx4x4unity_MatrixVP[1];
    u_xlat3 = hlslcc_mtx4x4unity_MatrixVP[0] * u_xlat2.xxxx + u_xlat3;
    u_xlat3 = hlslcc_mtx4x4unity_MatrixVP[2] * u_xlat2.zzzz + u_xlat3;
    gl_Position = hlslcc_mtx4x4unity_MatrixVP[3] * u_xlat2.wwww + u_xlat3;
    vs_COLOR0 = in_COLOR0;
    u_xlat4.x = roundEven(in_TEXCOORD2.y);
    u_xlat4.x = max(u_xlat4.x, 1.0);
    u_xlat4.x = min(u_xlat4.x, 3.0);
    u_xlatb8 = in_TEXCOORD2.y>=0.5;
    u_xlat4.x = (u_xlatb8) ? u_xlat4.x : 1.0;
    u_xlatb8 = in_TEXCOORD1.y<0.0;
    u_xlat8 = u_xlatb8 ? 1.0 : float(0.0);
    vs_TEXCOORD0.z = u_xlat4.x * u_xlat8;
    vs_TEXCOORD0.xy = in_TEXCOORD0.xy;
    vs_TEXCOORD0.w = 1.0;
    u_xlat4.x = max(_BodyTileLength, 9.99999975e-05);
    u_xlat4.x = float(1.0) / u_xlat4.x;
    u_xlat8 = _HeadLength + _TailLength;
    u_xlat12 = max(u_xlat8, 9.99999975e-05);
    u_xlat5.y = max(in_TEXCOORD1.x, 9.99999975e-05);
    u_xlat12 = u_xlat5.y / u_xlat12;
    u_xlatb8 = u_xlat5.y<u_xlat8;
    u_xlat8 = (u_xlatb8) ? u_xlat12 : 1.0;
    u_xlat5.x = (-_HeadLength) * u_xlat8 + u_xlat5.y;
    vs_TEXCOORD1.yz = u_xlat5.xy;
    u_xlat12 = (-_TailLength) * u_xlat8 + u_xlat5.x;
    u_xlat5.xy = vec2(u_xlat8) * vec2(_TailLength, _HeadLength);
    u_xlat8 = u_xlat12 / _BodyTileLength;
    u_xlat8 = u_xlat8 + 0.5;
    u_xlat8 = floor(u_xlat8);
    u_xlat8 = max(u_xlat8, 1.0);
    u_xlat8 = u_xlat8 / u_xlat12;
    u_xlatb12 = 9.99999975e-05<u_xlat12;
    u_xlatb13 = 9.99999975e-05<_BodyTileLength;
    u_xlatb12 = u_xlatb12 && u_xlatb13;
    vs_TEXCOORD1.w = (u_xlatb12) ? u_xlat8 : u_xlat4.x;
    vs_TEXCOORD1.x = u_xlat5.x;
    u_xlatb4.xy = lessThan(vec4(9.99999975e-05, 9.99999975e-05, 0.0, 0.0), u_xlat5.xyxx).xy;
    u_xlat5.xy = vec2(1.0, 1.0) / u_xlat5.xy;
    vs_TEXCOORD6.x = u_xlatb4.x ? u_xlat5.x : 0.0;
    vs_TEXCOORD6.y = u_xlatb4.y ? u_xlat5.y : 0.0;
;
    u_xlatb4.x = 0.5<_StyleIndexFromMesh;
    u_xlat4.x = (u_xlatb4.x) ? abs(in_TEXCOORD1.y) : _StyleIndex;
    u_xlat4.x = u_xlat4.x + -1.0;
    u_xlat4.y = _StyleRowCount + -1.0;
    u_xlat4.xy = max(u_xlat4.xy, vec2(0.0, 0.0));
    vs_TEXCOORD6.w = min(u_xlat4.y, u_xlat4.x);
    u_xlat4.x = max(_StyleRowCount, 1.0);
    vs_TEXCOORD6.z = float(1.0) / u_xlat4.x;
    u_xlat4.xy = u_xlat0.xx * in_NORMAL0.yx;
    u_xlat2.x = in_NORMAL0.x * u_xlat1.x + (-u_xlat4.x);
    u_xlat2.y = in_NORMAL0.y * u_xlat1.x + u_xlat4.y;
    u_xlat2.z = in_NORMAL0.z;
    u_xlat3.x = dot(u_xlat2.xyz, hlslcc_mtx4x4unity_WorldToObject[0].xyz);
    u_xlat3.y = dot(u_xlat2.xyz, hlslcc_mtx4x4unity_WorldToObject[1].xyz);
    u_xlat3.z = dot(u_xlat2.xyz, hlslcc_mtx4x4unity_WorldToObject[2].xyz);
    u_xlat4.x = dot(u_xlat3.xyz, u_xlat3.xyz);
    u_xlat4.x = max(u_xlat4.x, 1.17549435e-38);
    u_xlat4.x = inversesqrt(u_xlat4.x);
    u_xlat4.xyz = u_xlat4.xxx * u_xlat3.xyz;
    vs_TEXCOORD2.xyz = u_xlat4.xyz;
    u_xlat5.xy = u_xlat0.xx * in_TANGENT0.yx;
    u_xlat0.x = in_TANGENT0.y * u_xlat1.x + u_xlat5.y;
    u_xlat1.x = in_TANGENT0.x * u_xlat1.x + (-u_xlat5.x);
    u_xlat5.xyz = u_xlat0.xxx * hlslcc_mtx4x4unity_ObjectToWorld[1].xyz;
    u_xlat1.xyz = hlslcc_mtx4x4unity_ObjectToWorld[0].xyz * u_xlat1.xxx + u_xlat5.xyz;
    u_xlat1.xyz = hlslcc_mtx4x4unity_ObjectToWorld[2].xyz * in_TANGENT0.zzz + u_xlat1.xyz;
    u_xlat0.x = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat0.x = max(u_xlat0.x, 1.17549435e-38);
    u_xlat0.x = inversesqrt(u_xlat0.x);
    u_xlat1.xyz = u_xlat0.xxx * u_xlat1.xyz;
    u_xlat0.x = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat0.x = inversesqrt(u_xlat0.x);
    u_xlat1.xyz = u_xlat0.xxx * u_xlat1.xyz;
    vs_TEXCOORD3.xyz = u_xlat1.xyz;
    u_xlat2.xyz = u_xlat4.zxy * u_xlat1.yzx;
    u_xlat0.xyz = u_xlat4.yzx * u_xlat1.zxy + (-u_xlat2.xyz);
    u_xlat0.xyz = u_xlat0.xyz * in_TANGENT0.www;
    u_xlat12 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat12 = inversesqrt(u_xlat12);
    vs_TEXCOORD4.xyz = vec3(u_xlat12) * u_xlat0.xyz;
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
	UNITY_UNIFORM float                _AlphaClip;
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
	UNITY_UNIFORM vec4                _ExtraRectA;
	UNITY_UNIFORM vec4                _ExtraRectB;
	UNITY_UNIFORM vec4                _ExtraRectC;
	UNITY_UNIFORM float                _NormalScale;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedX_MatCapColor;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_MatCapStrength;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_MatCapLOD;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_MatCapRotation;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_MatCapBlendMode;
	UNITY_UNIFORM vec4                _LightDir;
	UNITY_UNIFORM mediump vec4                _LightColor;
	UNITY_UNIFORM mediump float                _LightIntensity;
	UNITY_UNIFORM mediump vec4                _ShadowColor;
	UNITY_UNIFORM mediump float                _LightStrength;
	UNITY_UNIFORM mediump float                _LightWrap;
	UNITY_UNIFORM vec4 Xhlslcc_UnusedX_SDFTex_TexelSize;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedX_OutlineColor;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_OutlineWidth;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_OutlineSoftness;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_OutlineMode;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_OutlineUseVertColor;
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
UNITY_LOCATION(1) uniform mediump sampler2D _NormalMap;
in mediump  vec4 vs_COLOR0;
in highp  vec4 vs_TEXCOORD0;
in highp  vec4 vs_TEXCOORD1;
in highp  vec4 vs_TEXCOORD6;
in highp  vec3 vs_TEXCOORD2;
in highp  vec3 vs_TEXCOORD3;
in highp  vec3 vs_TEXCOORD4;
layout(location = 0) out mediump vec4 SV_Target0;
vec2 u_xlat0;
vec3 u_xlat1;
mediump vec3 u_xlat16_1;
vec4 u_xlat2;
mediump vec4 u_xlat16_2;
bvec2 u_xlatb2;
vec3 u_xlat3;
vec2 u_xlat4;
mediump vec4 u_xlat16_4;
vec4 u_xlat5;
vec4 u_xlat6;
mediump vec3 u_xlat16_7;
mediump vec3 u_xlat16_8;
mediump vec3 u_xlat16_9;
vec3 u_xlat10;
bool u_xlatb10;
vec3 u_xlat12;
bool u_xlatb12;
vec2 u_xlat13;
mediump vec3 u_xlat16_17;
vec2 u_xlat20;
bool u_xlatb20;
vec2 u_xlat21;
bvec2 u_xlatb21;
vec2 u_xlat22;
bool u_xlatb22;
vec2 u_xlat23;
bool u_xlatb23;
float u_xlat30;
bool u_xlatb30;
float u_xlat31;
bool u_xlatb31;
float u_xlat32;
bool u_xlatb32;
float u_xlat33;
mediump float u_xlat16_37;
void main()
{
vec4 hlslcc_FragCoord = vec4(gl_FragCoord.xyz, 1.0/gl_FragCoord.w);
    u_xlat0.xy = hlslcc_FragCoord.xy + (-_StyleLineRectMaskRect.xy);
    u_xlat20.xy = (-hlslcc_FragCoord.xy) + _StyleLineRectMaskRect.zw;
    u_xlat0.xy = min(u_xlat20.xy, u_xlat0.xy);
    u_xlat20.x = min(u_xlat0.y, u_xlat0.x);
    u_xlatb20 = u_xlat20.x<0.0;
    if(u_xlatb20){discard;}
    u_xlat20.xy = max(_StyleLineRectMaskSoftness.xy, vec2(9.99999975e-05, 9.99999975e-05));
    u_xlat0.xy = u_xlat0.xy / u_xlat20.xy;
    u_xlat0.xy = clamp(u_xlat0.xy, 0.0, 1.0);
    u_xlat0.x = min(u_xlat0.y, u_xlat0.x);
    u_xlat10.x = vs_TEXCOORD0.x + (-_WindowStart);
    u_xlatb20 = u_xlat10.x>=0.0;
    u_xlatb30 = vs_TEXCOORD1.z>=u_xlat10.x;
    u_xlatb20 = u_xlatb30 && u_xlatb20;
    u_xlat20.x = u_xlatb20 ? 1.0 : float(0.0);
    u_xlat1.xy = dFdx(vs_TEXCOORD0.xy);
    u_xlat21.xy = dFdy(vs_TEXCOORD0.xy);
    u_xlatb30 = 0.5<vs_TEXCOORD0.z;
    if(u_xlatb30){
        u_xlatb2.xy = lessThan(vs_TEXCOORD0.zzzz, vec4(1.5, 2.5, 0.0, 0.0)).xy;
        u_xlat12.xyz = (u_xlatb2.y) ? _ExtraRectB.xyz : _ExtraRectC.xyz;
        u_xlat2.xyz = (u_xlatb2.x) ? _ExtraRectA.xyz : u_xlat12.xyz;
        u_xlat30 = max(vs_TEXCOORD1.z, 9.99999975e-05);
        u_xlat32 = u_xlat10.x / u_xlat30;
        u_xlat32 = clamp(u_xlat32, 0.0, 1.0);
        u_xlat30 = float(1.0) / u_xlat30;
        u_xlat3.xy = _MainTex_TexelSize.xy * vec2(_AtlasPaddingPixels);
        u_xlat3.x = (-u_xlat3.x) * 2.0 + u_xlat2.z;
        u_xlat4.y = (-u_xlat3.y) * 2.0 + vs_TEXCOORD6.z;
        u_xlat4.x = u_xlat30 * u_xlat3.x;
        u_xlat13.xy = vec2(_AtlasPaddingPixels) * _MainTex_TexelSize.xy + u_xlat2.xy;
        u_xlat30 = u_xlat32 * u_xlat3.x + u_xlat13.x;
        u_xlat32 = vs_TEXCOORD0.y * u_xlat4.y + u_xlat13.y;
        u_xlat22.x = u_xlat2.z + u_xlat2.x;
        u_xlat30 = max(u_xlat2.x, u_xlat30);
        u_xlat3.x = min(u_xlat22.x, u_xlat30);
        u_xlat30 = u_xlat2.y + vs_TEXCOORD6.z;
        u_xlat2.x = max(u_xlat2.y, u_xlat32);
        u_xlat30 = min(u_xlat30, u_xlat2.x);
        u_xlat3.y = vs_TEXCOORD6.w * vs_TEXCOORD6.z + u_xlat30;
    } else {
        u_xlatb30 = vs_TEXCOORD1.x>=u_xlat10.x;
        u_xlat2.w = u_xlat10.x * vs_TEXCOORD6.x;
        u_xlat2.w = clamp(u_xlat2.w, 0.0, 1.0);
        u_xlatb23 = u_xlat10.x>=vs_TEXCOORD1.y;
        u_xlat33 = u_xlat10.x + (-vs_TEXCOORD1.y);
        u_xlat2.y = u_xlat33 * vs_TEXCOORD6.y;
        u_xlat2.y = clamp(u_xlat2.y, 0.0, 1.0);
        u_xlat33 = (-u_xlat10.x) + vs_TEXCOORD1.y;
        u_xlat33 = u_xlat33 + _BodyPhase;
        u_xlat33 = u_xlat33 * vs_TEXCOORD1.w;
        u_xlat33 = fract(u_xlat33);
        u_xlat5.y = (-u_xlat33) + 1.0;
        u_xlat6 = (bool(u_xlatb23)) ? _HeadRect : _BodyRect;
        u_xlat2.xz = vs_TEXCOORD6.yx;
        u_xlat5.x = vs_TEXCOORD1.w;
        u_xlat2.xy = (bool(u_xlatb23)) ? u_xlat2.xy : u_xlat5.xy;
        u_xlat5 = (bool(u_xlatb30)) ? _TailRect : u_xlat6;
        u_xlat2.xy = (bool(u_xlatb30)) ? u_xlat2.zw : u_xlat2.xy;
        u_xlat6.xy = _MainTex_TexelSize.xy * vec2(_AtlasPaddingPixels);
        u_xlatb22 = 0.0<_AtlasPaddingPixels;
        u_xlatb32 = vs_TEXCOORD1.x<u_xlat10.x;
        u_xlatb10 = u_xlat10.x<vs_TEXCOORD1.y;
        u_xlatb10 = u_xlatb10 && u_xlatb32;
        u_xlat6.z = 0.0;
        u_xlat23.xy = (bool(u_xlatb30)) ? u_xlat6.xz : u_xlat6.zx;
        u_xlat10.xz = (bool(u_xlatb10)) ? vec2(0.0, 0.0) : u_xlat23.xy;
        u_xlat10.xz = (bool(u_xlatb22)) ? u_xlat10.xz : u_xlat6.xx;
        u_xlat22.x = (-u_xlat10.x) + u_xlat5.z;
        u_xlat30 = (-u_xlat10.z) + u_xlat22.x;
        u_xlat4.y = (-u_xlat6.y) * 2.0 + u_xlat5.w;
        u_xlat10.x = u_xlat10.x + u_xlat5.x;
        u_xlat2.x = u_xlat30 * u_xlat2.x;
        u_xlat10.x = u_xlat2.y * u_xlat30 + u_xlat10.x;
        u_xlat30 = _AtlasPaddingPixels * _MainTex_TexelSize.y + u_xlat5.y;
        u_xlat10.z = vs_TEXCOORD0.y * u_xlat4.y + u_xlat30;
        u_xlat12.xy = u_xlat5.zw + u_xlat5.xy;
        u_xlat10.xz = max(u_xlat5.xy, u_xlat10.xz);
        u_xlat10.xz = min(u_xlat12.xy, u_xlat10.xz);
        u_xlat3.y = vs_TEXCOORD6.w * vs_TEXCOORD6.z + u_xlat10.z;
        u_xlat3.x = u_xlat10.x * 0.5;
        u_xlat4.x = u_xlat2.x * 0.5;
    }
    u_xlat10.xz = u_xlat1.xy * u_xlat4.xy;
    u_xlat1.xy = u_xlat21.xy * u_xlat4.xy;
    u_xlat21.x = dot(u_xlat10.xz, u_xlat10.xz);
    u_xlat21.y = dot(u_xlat1.xy, u_xlat1.xy);
    u_xlat21.xy = sqrt(u_xlat21.xy);
    u_xlatb2.x = _MipClamp<u_xlat21.x;
    u_xlat21.x = _MipClamp / u_xlat21.x;
    u_xlat12.xy = u_xlat10.xz * u_xlat21.xx;
    u_xlat10.xz = (u_xlatb2.x) ? u_xlat12.xy : u_xlat10.xz;
    u_xlatb21.x = _MipClamp<u_xlat21.y;
    u_xlat31 = _MipClamp / u_xlat21.y;
    u_xlat2.xy = vec2(u_xlat31) * u_xlat1.xy;
    u_xlat1.xy = (u_xlatb21.x) ? u_xlat2.xy : u_xlat1.xy;
    u_xlat21.x = max(_MainTex_TexelSize.y, _MainTex_TexelSize.x);
    u_xlat21.x = u_xlat21.x * 8.0;
    u_xlat31 = dot(u_xlat10.xz, u_xlat10.xz);
    u_xlat31 = sqrt(u_xlat31);
    u_xlat2.x = dot(u_xlat1.xy, u_xlat1.xy);
    u_xlat2.x = sqrt(u_xlat2.x);
    u_xlatb12 = u_xlat21.x<u_xlat31;
    u_xlat31 = u_xlat21.x / u_xlat31;
    u_xlat22.xy = u_xlat10.xz * vec2(u_xlat31);
    u_xlat10.xz = (bool(u_xlatb12)) ? u_xlat22.xy : u_xlat10.xz;
    u_xlatb31 = u_xlat21.x<u_xlat2.x;
    u_xlat21.x = u_xlat21.x / u_xlat2.x;
    u_xlat2.xy = u_xlat21.xx * u_xlat1.xy;
    u_xlat1.xy = (bool(u_xlatb31)) ? u_xlat2.xy : u_xlat1.xy;
    u_xlat10.xz = u_xlat10.xz * _GlobalMipBias.yy;
    u_xlat1.xy = u_xlat1.xy * _GlobalMipBias.yy;
    u_xlat16_2 = textureGrad(_MainTex, u_xlat3.xy, vec4(u_xlat10.xzxx).xy, vec4(u_xlat1.xyxx).xy);
    u_xlat16_7.xyz = (-u_xlat16_2.xyz) + vec3(1.0, 1.0, 1.0);
    u_xlat16_7.xyz = vec3(vec3(_Blend, _Blend, _Blend)) * u_xlat16_7.xyz + u_xlat16_2.xyz;
    u_xlat2.xyz = _Color.xyz * vec3(0.305306017, 0.305306017, 0.305306017) + vec3(0.682171106, 0.682171106, 0.682171106);
    u_xlat2.xyz = _Color.xyz * u_xlat2.xyz + vec3(0.0125228781, 0.0125228781, 0.0125228781);
    u_xlat2.xyz = u_xlat2.xyz * _Color.xyz;
    u_xlat16_8.xyz = u_xlat2.xyz * vec3(_ColorIntensity);
    u_xlat16_9.xyz = u_xlat16_7.xyz * u_xlat16_8.xyz;
    u_xlat16_2.w = u_xlat16_2.w * _Color.w;
    u_xlat16_37 = dot(u_xlat16_9.xyz, vec3(0.298999995, 0.587000012, 0.114));
    u_xlat16_7.xyz = (-u_xlat16_7.xyz) * u_xlat16_8.xyz + vec3(u_xlat16_37);
    u_xlat16_7.xyz = vec3(vec3(_RecolorStrength, _RecolorStrength, _RecolorStrength)) * u_xlat16_7.xyz + u_xlat16_9.xyz;
    u_xlat16_7.xyz = u_xlat16_7.xyz + vec3(-0.5, -0.5, -0.5);
    u_xlat16_2.xyz = vec3(vec3(_Contrast, _Contrast, _Contrast)) * u_xlat16_7.xyz + vec3(0.5, 0.5, 0.5);
    u_xlatb21.xy = lessThan(vec4(0.5, 0.5, 0.5, 0.5), vec4(_UseVertexColor, _ColorfulBlend, _UseVertexColor, _ColorfulBlend)).xy;
    u_xlat16_7.xyz = (-u_xlat16_2.xyz) + vec3(1.0, 1.0, 1.0);
    u_xlat16_8.xyz = (-vs_COLOR0.xyz) + vec3(1.0, 1.0, 1.0);
    u_xlat16_7.xyz = (-u_xlat16_7.xyz) * u_xlat16_8.xyz + vec3(1.0, 1.0, 1.0);
    u_xlat16_8.xyz = u_xlat16_2.xyz * vs_COLOR0.xyz;
    u_xlat16_4.xyz = (u_xlatb21.y) ? u_xlat16_7.xyz : u_xlat16_8.xyz;
    u_xlat16_4.w = u_xlat16_2.w * vs_COLOR0.w;
    u_xlat16_2 = (u_xlatb21.x) ? u_xlat16_4 : u_xlat16_2;
    u_xlat16_7.x = dot(u_xlat16_2.xyz, vec3(0.298999995, 0.587000012, 0.114));
    u_xlat16_17.xyz = u_xlat16_2.xyz + (-u_xlat16_7.xxx);
    u_xlat16_7.xyz = vec3(_Saturation) * u_xlat16_17.xyz + u_xlat16_7.xxx;
    u_xlat16_1.xyz = textureGrad(_NormalMap, u_xlat3.xy, vec4(u_xlat10.xzxx).xy, vec4(u_xlat1.xyxx).xy).xyz;
    u_xlat16_8.xyz = u_xlat16_1.xyz * vec3(2.0, 2.0, 2.0) + vec3(-1.0, -1.0, -1.0);
    u_xlat10.xz = u_xlat16_8.xy * vec2(_NormalScale);
    u_xlat1.xyz = u_xlat10.zzz * vs_TEXCOORD4.xyz;
    u_xlat1.xyz = u_xlat10.xxx * vs_TEXCOORD3.xyz + u_xlat1.xyz;
    u_xlat1.xyz = u_xlat16_8.zzz * vs_TEXCOORD2.xyz + u_xlat1.xyz;
    u_xlat10.x = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat10.x = inversesqrt(u_xlat10.x);
    u_xlat1.xyz = u_xlat10.xxx * u_xlat1.xyz;
    u_xlat10.x = dot(_LightDir.xyz, _LightDir.xyz);
    u_xlat10.x = inversesqrt(u_xlat10.x);
    u_xlat3.xyz = u_xlat10.xxx * _LightDir.xyz;
    u_xlat10.x = dot(u_xlat1.xyz, u_xlat3.xyz);
    u_xlat16_37 = u_xlat10.x;
    u_xlat16_37 = clamp(u_xlat16_37, 0.0, 1.0);
    u_xlat10.x = u_xlat10.x * 0.5 + 0.5;
    u_xlat10.x = (-u_xlat16_37) + u_xlat10.x;
    u_xlat10.x = _LightWrap * u_xlat10.x + u_xlat16_37;
    u_xlat1.xyz = _ShadowColor.xyz * vec3(0.305306017, 0.305306017, 0.305306017) + vec3(0.682171106, 0.682171106, 0.682171106);
    u_xlat1.xyz = _ShadowColor.xyz * u_xlat1.xyz + vec3(0.0125228781, 0.0125228781, 0.0125228781);
    u_xlat1.xyz = u_xlat1.xyz * _ShadowColor.xyz;
    u_xlat3.xyz = _LightColor.xyz * vec3(0.305306017, 0.305306017, 0.305306017) + vec3(0.682171106, 0.682171106, 0.682171106);
    u_xlat3.xyz = _LightColor.xyz * u_xlat3.xyz + vec3(0.0125228781, 0.0125228781, 0.0125228781);
    u_xlat16_8.xyz = _LightColor.xyz * u_xlat3.xyz + (-u_xlat1.xyz);
    u_xlat16_8.xyz = u_xlat10.xxx * u_xlat16_8.xyz + u_xlat1.xyz;
    u_xlat16_9.xyz = (-u_xlat16_7.xyz) + vec3(1.0, 1.0, 1.0);
    u_xlat16_8.xyz = (-u_xlat16_8.xyz) * vec3(_LightStrength) + vec3(1.0, 1.0, 1.0);
    u_xlat16_8.xyz = (-u_xlat16_9.xyz) * u_xlat16_8.xyz + vec3(1.0, 1.0, 1.0);
    u_xlat16_7.xyz = u_xlat16_7.xyz * u_xlat16_8.xyz;
    SV_Target0.xyz = u_xlat16_7.xyz * vec3(_LightIntensity);
    u_xlat10.x = u_xlat20.x * u_xlat16_2.w;
    u_xlat20.x = u_xlat16_2.w * u_xlat20.x + (-_AlphaClip);
    u_xlatb20 = u_xlat20.x<0.0;
    if(u_xlatb20){discard;}
    SV_Target0.w = u_xlat0.x * u_xlat10.x;
    return;
}

#endif

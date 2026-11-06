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
uniform 	vec4 _ScreenParams;
uniform 	vec4 hlslcc_mtx4x4glstate_matrix_projection[4];
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
	UNITY_UNIFORM mediump vec4                _MainTex_ST;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedX_Mask_Tex_ST;
	UNITY_UNIFORM mediump vec4                _Main_Tex_Color;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedX_Distortion_Tex_ST;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedX_Dissolve_Tex_ST;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedX_Dissolve_Dir_Tex_ST;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedX_Dissolve_Outline_Color;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedX_Remap_Tex_ST;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Dissolve_Tex_A_R;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Dissolve_Tex_power;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Dissolve_Tex_Custom_X;
	UNITY_UNIFORM float Xhlslcc_UnusedX_Remap_Tex_U_speed;
	UNITY_UNIFORM float Xhlslcc_UnusedX_Remap_Tex_V_speed;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_CullMode;
	UNITY_UNIFORM mediump float                _Main_Tex_A_R;
	UNITY_UNIFORM float Xhlslcc_UnusedX_Mask_Tex_U_speed;
	UNITY_UNIFORM float Xhlslcc_UnusedX_Mask_Tex_V_speed;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Remap_Tex_Followl_Main_Tex;
	UNITY_UNIFORM float Xhlslcc_UnusedX_Dissolve_Tex_V_speed;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Dissolve_Tex_smooth;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Dst;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_GammaRestore;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_RemapGammaSRGB;
	UNITY_UNIFORM float                _Main_Tex_U_speed;
	UNITY_UNIFORM float                _Main_Tex_V_speed;
	UNITY_UNIFORM mediump float                _Main_Tex_Custom_ZW;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Distortion_Tex_Power;
	UNITY_UNIFORM float Xhlslcc_UnusedX_Distortion_Tex_U_speed;
	UNITY_UNIFORM float Xhlslcc_UnusedX_Distortion_Tex_V_speed;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Distortion_Switch;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Distortion_Affect_Dissolve;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Distortion_Affect_Mask;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Distortion_Vec2_Mode;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Mask_Tex_A_R;
	UNITY_UNIFORM float Xhlslcc_UnusedX_Dissolve_Tex_U_speed;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Remap_Tex_A_R;
	UNITY_UNIFORM mediump float                _Main_Tex_CoordMode;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Remap_Tex_CoordMode;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Mask_Tex_CoordMode;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Dissolve_Tex_CoordMode;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Remap_BlendMode;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedX_Remap_Tex_Color;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Dissolve_Outline_On;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Dissolve_Outline_Width;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Dissolve_Outline_Trans;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Dissolve_Outline_BlendMode;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Dissolve_Dir_Enabled;
	UNITY_UNIFORM vec4                _ClipRect;
	UNITY_UNIFORM mediump float                _UIMaskSoftnessX;
	UNITY_UNIFORM mediump float                _UIMaskSoftnessY;
#if HLSLCC_ENABLE_UNIFORM_BUFFERS
};
#endif
in mediump vec4 in_POSITION0;
in mediump vec4 in_TEXCOORD1;
in mediump vec4 in_TEXCOORD0;
in mediump vec4 in_COLOR0;
mediump vec3 vs_TEXCOORD0;
out mediump vec4 vs_COLOR0;
out mediump vec4 vs_TEXCOORD3;
out mediump vec4 vs_TEXCOORD4;
out highp vec4 vs_TEXCOORD5;
vec4 u_xlat0;
vec4 u_xlat1;
vec2 u_xlat5;
void main()
{
    u_xlat0.xyz = in_POSITION0.yyy * hlslcc_mtx4x4unity_ObjectToWorld[1].xyz;
    u_xlat0.xyz = hlslcc_mtx4x4unity_ObjectToWorld[0].xyz * in_POSITION0.xxx + u_xlat0.xyz;
    u_xlat0.xyz = hlslcc_mtx4x4unity_ObjectToWorld[2].xyz * in_POSITION0.zzz + u_xlat0.xyz;
    u_xlat0.xyz = u_xlat0.xyz + hlslcc_mtx4x4unity_ObjectToWorld[3].xyz;
    u_xlat1 = u_xlat0.yyyy * hlslcc_mtx4x4unity_MatrixVP[1];
    u_xlat1 = hlslcc_mtx4x4unity_MatrixVP[0] * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = hlslcc_mtx4x4unity_MatrixVP[2] * u_xlat0.zzzz + u_xlat1;
    vs_TEXCOORD0.xyz = u_xlat0.xyz;
    u_xlat0 = u_xlat1 + hlslcc_mtx4x4unity_MatrixVP[3];
    gl_Position = u_xlat0;
    vs_COLOR0 = in_COLOR0;
    vs_TEXCOORD3.xy = in_TEXCOORD0.xy;
    vs_TEXCOORD3.zw = vec2(0.0, 0.0);
    vs_TEXCOORD4 = in_TEXCOORD1;
    u_xlat5.x = _ScreenParams.x * hlslcc_mtx4x4glstate_matrix_projection[0].x;
    u_xlat5.y = _ScreenParams.y * hlslcc_mtx4x4glstate_matrix_projection[1].y;
    u_xlat0.xy = u_xlat0.ww / abs(u_xlat5.xy);
    u_xlat0.xy = vec2(_UIMaskSoftnessX, _UIMaskSoftnessY) * vec2(0.25, 0.25) + abs(u_xlat0.xy);
    vs_TEXCOORD5.zw = vec2(0.25, 0.25) / u_xlat0.xy;
    u_xlat0 = max(_ClipRect, vec4(-2e+10, -2e+10, -2e+10, -2e+10));
    u_xlat0 = min(u_xlat0, vec4(2e+10, 2e+10, 2e+10, 2e+10));
    u_xlat0.xy = in_POSITION0.xy * vec2(2.0, 2.0) + (-u_xlat0.xy);
    vs_TEXCOORD5.xy = (-u_xlat0.zw) + u_xlat0.xy;
    return;
}

#endif
#ifdef FRAGMENT
#version 300 es

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
uniform 	vec4 _TimeParameters;
#if HLSLCC_ENABLE_UNIFORM_BUFFERS
UNITY_BINDING(0) uniform UnityPerMaterial {
#endif
	UNITY_UNIFORM mediump vec4                _MainTex_ST;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedX_Mask_Tex_ST;
	UNITY_UNIFORM mediump vec4                _Main_Tex_Color;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedX_Distortion_Tex_ST;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedX_Dissolve_Tex_ST;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedX_Dissolve_Dir_Tex_ST;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedX_Dissolve_Outline_Color;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedX_Remap_Tex_ST;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Dissolve_Tex_A_R;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Dissolve_Tex_power;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Dissolve_Tex_Custom_X;
	UNITY_UNIFORM float Xhlslcc_UnusedX_Remap_Tex_U_speed;
	UNITY_UNIFORM float Xhlslcc_UnusedX_Remap_Tex_V_speed;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_CullMode;
	UNITY_UNIFORM mediump float                _Main_Tex_A_R;
	UNITY_UNIFORM float Xhlslcc_UnusedX_Mask_Tex_U_speed;
	UNITY_UNIFORM float Xhlslcc_UnusedX_Mask_Tex_V_speed;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Remap_Tex_Followl_Main_Tex;
	UNITY_UNIFORM float Xhlslcc_UnusedX_Dissolve_Tex_V_speed;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Dissolve_Tex_smooth;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Dst;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_GammaRestore;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_RemapGammaSRGB;
	UNITY_UNIFORM float                _Main_Tex_U_speed;
	UNITY_UNIFORM float                _Main_Tex_V_speed;
	UNITY_UNIFORM mediump float                _Main_Tex_Custom_ZW;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Distortion_Tex_Power;
	UNITY_UNIFORM float Xhlslcc_UnusedX_Distortion_Tex_U_speed;
	UNITY_UNIFORM float Xhlslcc_UnusedX_Distortion_Tex_V_speed;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Distortion_Switch;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Distortion_Affect_Dissolve;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Distortion_Affect_Mask;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Distortion_Vec2_Mode;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Mask_Tex_A_R;
	UNITY_UNIFORM float Xhlslcc_UnusedX_Dissolve_Tex_U_speed;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Remap_Tex_A_R;
	UNITY_UNIFORM mediump float                _Main_Tex_CoordMode;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Remap_Tex_CoordMode;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Mask_Tex_CoordMode;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Dissolve_Tex_CoordMode;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Remap_BlendMode;
	UNITY_UNIFORM mediump vec4 Xhlslcc_UnusedX_Remap_Tex_Color;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Dissolve_Outline_On;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Dissolve_Outline_Width;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Dissolve_Outline_Trans;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Dissolve_Outline_BlendMode;
	UNITY_UNIFORM mediump float Xhlslcc_UnusedX_Dissolve_Dir_Enabled;
	UNITY_UNIFORM vec4                _ClipRect;
	UNITY_UNIFORM mediump float                _UIMaskSoftnessX;
	UNITY_UNIFORM mediump float                _UIMaskSoftnessY;
#if HLSLCC_ENABLE_UNIFORM_BUFFERS
};
#endif
UNITY_LOCATION(0) uniform mediump sampler2D _MainTex;
in mediump  vec4 vs_COLOR0;
in mediump  vec4 vs_TEXCOORD3;
in mediump  vec4 vs_TEXCOORD4;
in highp  vec4 vs_TEXCOORD5;
layout(location = 0) out mediump vec4 SV_Target0;
vec2 u_xlat0;
mediump vec4 u_xlat16_0;
vec2 u_xlat1;
mediump vec4 u_xlat16_1;
bool u_xlatb1;
mediump vec2 u_xlat16_2;
bool u_xlatb4;
mediump float u_xlat16_5;
vec2 u_xlat6;
float u_xlat9;
bool u_xlatb9;
void main()
{
    u_xlat0.xy = vs_TEXCOORD3.yx + vec2(-0.5, -0.5);
    u_xlat6.x = max(abs(u_xlat0.y), abs(u_xlat0.x));
    u_xlat6.x = float(1.0) / u_xlat6.x;
    u_xlat9 = min(abs(u_xlat0.y), abs(u_xlat0.x));
    u_xlat6.x = u_xlat6.x * u_xlat9;
    u_xlat9 = u_xlat6.x * u_xlat6.x;
    u_xlat1.x = u_xlat9 * 0.0208350997 + -0.0851330012;
    u_xlat1.x = u_xlat9 * u_xlat1.x + 0.180141002;
    u_xlat1.x = u_xlat9 * u_xlat1.x + -0.330299497;
    u_xlat9 = u_xlat9 * u_xlat1.x + 0.999866009;
    u_xlat1.x = u_xlat9 * u_xlat6.x;
    u_xlat1.x = u_xlat1.x * -2.0 + 1.57079637;
    u_xlatb4 = abs(u_xlat0.y)<abs(u_xlat0.x);
    u_xlat1.x = u_xlatb4 ? u_xlat1.x : float(0.0);
    u_xlat6.x = u_xlat6.x * u_xlat9 + u_xlat1.x;
    u_xlatb9 = u_xlat0.y<(-u_xlat0.y);
    u_xlat9 = u_xlatb9 ? -3.14159274 : float(0.0);
    u_xlat6.x = u_xlat9 + u_xlat6.x;
    u_xlat9 = min(u_xlat0.y, u_xlat0.x);
    u_xlatb9 = u_xlat9<(-u_xlat9);
    u_xlat1.x = max(u_xlat0.y, u_xlat0.x);
    u_xlat0.x = dot(u_xlat0.xy, u_xlat0.xy);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x + u_xlat0.x;
    u_xlatb1 = u_xlat1.x>=(-u_xlat1.x);
    u_xlatb9 = u_xlatb9 && u_xlatb1;
    u_xlat6.x = (u_xlatb9) ? (-u_xlat6.x) : u_xlat6.x;
    u_xlat0.y = u_xlat6.x * 0.159154937 + 0.5;
    u_xlat0.xy = u_xlat0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    u_xlat6.xy = _TimeParameters.xx * vec2(_Main_Tex_U_speed, _Main_Tex_V_speed);
    u_xlat6.xy = fract(u_xlat6.xy);
    u_xlat1.xy = (-u_xlat6.xy) + vs_TEXCOORD4.zw;
    u_xlat1.xy = vec2(_Main_Tex_Custom_ZW) * u_xlat1.xy + u_xlat6.xy;
    u_xlat0.xy = u_xlat0.xy + u_xlat1.xy;
    u_xlat16_2.xy = vs_TEXCOORD3.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    u_xlat6.xy = u_xlat6.xy + u_xlat16_2.xy;
    u_xlat1.xy = u_xlat16_2.xy + vs_TEXCOORD4.zw;
    u_xlat1.xy = (-u_xlat6.xy) + u_xlat1.xy;
    u_xlat6.xy = vec2(_Main_Tex_Custom_ZW) * u_xlat1.xy + u_xlat6.xy;
    u_xlatb1 = 0.5<_Main_Tex_CoordMode;
    u_xlat0.xy = (bool(u_xlatb1)) ? u_xlat0.xy : u_xlat6.xy;
    u_xlat16_0 = texture(_MainTex, u_xlat0.xy);
    u_xlat16_2.x = (-u_xlat16_0.x) + u_xlat16_0.w;
    u_xlat16_2.x = _Main_Tex_A_R * u_xlat16_2.x + u_xlat16_0.x;
    u_xlat16_1 = vs_COLOR0.wxyz * _Main_Tex_Color.wxyz;
    u_xlat16_2.x = u_xlat16_2.x * u_xlat16_1.x;
    SV_Target0.xyz = u_xlat16_0.xyz * u_xlat16_1.yzw;
    u_xlat0.xy = (-_ClipRect.xy) + _ClipRect.zw;
    u_xlat0.xy = u_xlat0.xy + -abs(vs_TEXCOORD5.xy);
    u_xlat0.xy = u_xlat0.xy * vs_TEXCOORD5.zw;
    u_xlat0.xy = clamp(u_xlat0.xy, 0.0, 1.0);
    u_xlat16_5 = u_xlat0.y * u_xlat0.x;
    SV_Target0.w = u_xlat16_5 * u_xlat16_2.x;
    return;
}

#endif

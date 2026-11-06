Shader "Sprites/BrowCatchSprite"
{
    Properties
    {
        // Retained Unity source-material settings not consumed by this shader port are cataloged explicitly.
        PixelSnap ("Source Setting", Float) = 0
        _AddPrecomputedVelocity ("Source Setting", Float) = 0
        _AlphaClip ("Source Setting", Float) = 0
        _AlphaToMask ("Source Setting", Float) = 0
        _BaseMeshHeight ("Source Setting", Float) = 0
        _Blend ("Source Setting", Float) = 0
        _BlendModePreserveSpecular ("Source Setting", Float) = 0
        _BumpScale ("Source Setting", Float) = 0
        _CapBottom ("Source Setting", Float) = 0
        _CapTop ("Source Setting", Float) = 0
        _ClearCoatMask ("Source Setting", Float) = 0
        _ClearCoatSmoothness ("Source Setting", Float) = 0
        _Cutoff ("Source Setting", Float) = 0
        _DetailAlbedoMapScale ("Source Setting", Float) = 0
        _DetailNormalMapScale ("Source Setting", Float) = 0
        _DstBlendAlpha ("Source Setting", Float) = 0
        _EmissionColor ("Source Color", Vector) = (1,1,1,1)
        _EnableExternalAlpha ("Source Setting", Float) = 0
        _EnvironmentReflections ("Source Setting", Float) = 0
        _GlossMapScale ("Source Setting", Float) = 0
        _Glossiness ("Source Setting", Float) = 0
        _GlossyReflections ("Source Setting", Float) = 0
        _Horiz9Slice ("Source Setting", Float) = 0
        _Metallic ("Source Setting", Float) = 0
        _NaturalScale ("Source Setting", Float) = 0
        _OcclusionStrength ("Source Setting", Float) = 0
        _Parallax ("Source Setting", Float) = 0
        _QueueOffset ("Source Setting", Float) = 0
        _ReceiveShadows ("Source Setting", Float) = 0
        _SliceCapLength ("Source Color", Vector) = (1,1,1,1)
        _Smoothness ("Source Setting", Float) = 0
        _SmoothnessTextureChannel ("Source Setting", Float) = 0
        _SpecColor ("Source Color", Vector) = (1,1,1,1)
        _SpecularHighlights ("Source Setting", Float) = 0
        _SrcBlendAlpha ("Source Setting", Float) = 0
        _StretchLength ("Source Setting", Float) = 0
        _Surface ("Source Setting", Float) = 0
        _WorkflowMode ("Source Setting", Float) = 0
        _XRMotionVectorsPass ("Source Setting", Float) = 0
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _BaseColor ("Base Tint", Color) = (1,1,1,1)
        _NormalScale ("Normal Scale", Range(0,2)) = 0
        _LightDir ("Fake Light Dir", Vector) = (.25,.45,1,0)
        [MaterialToggle] _ZWrite ("ZWrite", Float) = 1
        _Cull ("Cull", Float) = 0
        _SrcBlend ("Source Blend", Float) = 1
        _DstBlend ("Destination Blend", Float) = 0
        _RendererColor ("RendererColor", Color) = (1,1,1,1)
        _UVRect ("Atlas UV Rect", Vector) = (0,0,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Cull Off
        ZWrite [_ZWrite]
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            sampler2D _MainTex, _NormalMap;
            fixed4 _Color, _RendererColor, _UVRect;
            float4 _MainTex_ST;
            float _NormalScale;
            float3 _LightDir;
            struct appdata { float4 vertex:POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; float3 normal:NORMAL; float4 tangent:TANGENT; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; float3 normal:TEXCOORD1; float3 tangent:TEXCOORD2; float3 bitangent:TEXCOORD3; };
            v2f vert(appdata v)
            {
                v2f o;
                o.pos=UnityObjectToClipPos(v.vertex);
                o.uv=TRANSFORM_TEX(v.uv,_MainTex);
                o.uv=_UVRect.xy+o.uv*_UVRect.zw;
                o.color=v.color*_Color*_RendererColor;
                o.normal=UnityObjectToWorldNormal(v.normal);
                o.tangent=UnityObjectToWorldDir(v.tangent.xyz);
                o.bitangent=cross(o.normal,o.tangent)*v.tangent.w;
                return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                fixed4 c=tex2D(_MainTex,i.uv)*i.color;
                if (_NormalScale>0)
                {
                    float3 n=tex2D(_NormalMap,i.uv).xyz*2-1;
                    n.xy*=_NormalScale;
                    n=normalize(i.tangent*n.x+i.bitangent*n.y+i.normal*n.z);
                    float ndl=saturate(dot(n,normalize(_LightDir)));
                    c.rgb*=.45+.55*ndl;
                }
                return c;
            }
            ENDCG
        }
    }
    FallBack Off
}

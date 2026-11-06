Shader "RotateRings/OriginalFragment"
{
    Properties { _MainTex ("Original fragment atlas", 2D) = "white" {} _Tint ("Source material tint", Color) = (1.26218963,1.26218963,1.26218963,1) }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        // Extracted HsAppVFXAllEffect Forward pass has fixed RGB SrcAlpha,
        // destination _Dst=10; alpha One/OneMinusSrcAlpha. _SrcBlend is an
        // unused legacy material property. Zwrite=0, ZTestMode=4, CullMode=2.
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZWrite Off
        ZTest LEqual
        Cull Back
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; float4 _MainTex_ST; float4 _Tint;
            struct appdata { float4 vertex:POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; };
            struct v2f { float4 vertex:SV_POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; };
            v2f vert(appdata v) { v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.color=v.color;o.uv=TRANSFORM_TEX(v.uv,_MainTex);return o; }
            float4 frag(v2f i):SV_Target { return tex2D(_MainTex,i.uv)*i.color*_Tint; }
            ENDCG
        }
    }
}

Shader "HsApp/Effect/XYStyleLine"
{
    Properties
    {
        // Cataloged source settings outside the emitted source GLSL variants are retained as metadata.
        _DepthNormalEnable ("Source Setting", Float) = 0
        _DepthNormalStrength ("Source Setting", Float) = 0
        _ExtraCrossRect ("Source Color", Vector) = (1,1,1,1)
        _GammaCorrect ("Source Setting", Float) = 0
        _MatCapBlendMode ("Source Setting", Float) = 0
        _MatCapColor ("Source Color", Vector) = (1,1,1,1)
        _MatCapEnable ("Source Setting", Float) = 0
        _MatCapIntensity ("Source Setting", Float) = 0
        _MatCapLOD ("Source Setting", Float) = 0
        _MatCapRotation ("Source Setting", Float) = 0
        _MatCapStrength ("Source Setting", Float) = 0
        _MatCapTex ("Source Texture", 2D) = "white" {}
        _NormalWorldAlign ("Source Setting", Float) = 0
        _Outline8Tap ("Source Setting", Float) = 0
        _OutlineMethod ("Source Setting", Float) = 0
        _OutlinePixelWidth ("Source Setting", Float) = 0
        _OutlineZOffset ("Source Setting", Float) = 0
        _RenderQueue ("Source Setting", Float) = 0
        _ShadowDilate ("Source Setting", Float) = 0
        _ShadowEnable ("Source Setting", Float) = 0
        _ShadowOffsetX ("Source Setting", Float) = 0
        _ShadowOffsetY ("Source Setting", Float) = 0
        _ShadowSoftness ("Source Setting", Float) = 0
        _ShadowTintColor ("Source Color", Vector) = (1,1,1,1)
        _ShadowZOffset ("Source Setting", Float) = 0
        _ShadowZWrite ("Source Setting", Float) = 0
        _StyleLineRectMaskRect ("Source Color", Vector) = (-100000,-100000,100000,100000)
        _StyleLineRectMaskSoftness ("Source Color", Vector) = (0,0,0,0)
        _Underlay ("Source Setting", Float) = 0
        _Underlay2 ("Source Setting", Float) = 0
        _UnderlayColor ("Source Color", Vector) = (1,1,1,1)
        _UnderlayColor2 ("Source Color", Vector) = (1,1,1,1)
        _UnderlayDilate ("Source Setting", Float) = 0
        _UnderlayDilate2 ("Source Setting", Float) = 0
        _UnderlayOffsetX ("Source Setting", Float) = 0
        _UnderlayOffsetX2 ("Source Setting", Float) = 0
        _UnderlayOffsetY ("Source Setting", Float) = 0
        _UnderlayOffsetY2 ("Source Setting", Float) = 0
        _UnderlaySoftness ("Source Setting", Float) = 0
        _UnderlaySoftness2 ("Source Setting", Float) = 0
        _MainTex ("Arrow Atlas", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _ColorIntensity ("Tint Intensity", Range(0,10)) = 1
        _Blend ("Blend", Range(0,1)) = 0.5
        _RecolorStrength ("Recolor Strength", Range(0,1)) = 0
        _Contrast ("Contrast", Range(0,2)) = 1
        _Saturation ("Saturation", Range(0,2)) = 1
        _UseVertexColor ("Use Vertex Color", Float) = 1
        _ColorfulBlend ("Colorful Blend", Float) = 0
        _AlphaClipEnable ("Alpha Clip Enable", Float) = 1
        _AlphaClip ("Alpha Clip Threshold", Range(0,1)) = 0.25
        _DrivenByController ("Driven By Controller", Float) = 0
        _WindowStart ("Window Start", Float) = 0
        _VisibleLength ("Visible Length", Float) = 75
        _TailLength ("Tail Length", Float) = 0.3
        _HeadLength ("Head Length", Float) = 0.3
        _BodyTileLength ("Body Tile Length", Float) = 2
        _BodyPhase ("Body Phase", Float) = 0
        _TailRect ("Tail Rect", Vector) = (0,0,0.25,1)
        _BodyRect ("Body Rect", Vector) = (0.25,0,0.5,1)
        _HeadRect ("Head Rect", Vector) = (0.75,0,0.25,1)
        _ExtraRectA ("Extra Rect A", Vector) = (0.5,0,0.25,1)
        _ExtraRectB ("Extra Rect B", Vector) = (0.75,0,0.125,1)
        _ExtraRectC ("Extra Rect C", Vector) = (0.875,0,0.125,1)
        _ExtraEnable ("Extra Enable", Float) = 1
        _StyleRowCount ("Style Row Count", Float) = 1
        _StyleIndexFromMesh ("Style Index From Mesh", Float) = 0
        _StyleIndex ("Style Index", Float) = 4
        _AtlasPaddingPixels ("Atlas Padding Pixels", Float) = 0
        _MipClamp ("Mip Gradient Clamp", Range(0.05,1)) = 0.5
        _GlobalMipBias ("Global Mip Bias", Vector) = (0,1,0,0)
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalScale ("Normal Scale", Range(0,2)) = 1.5
        _NormalMapEnable ("Normal Map Enable", Float) = 1
        _LightDir ("Light Direction", Vector) = (-0.3,0.5667,0.7674,0)
        _LightColor ("Light Color", Color) = (0.95,0.95,0.95,1)
        _LightIntensity ("Light Intensity", Range(0,10)) = 1.4
        _ShadowColor ("Shadow Color", Color) = (0,0,0,1)
        _LightStrength ("Light Strength", Range(0,2)) = 1
        _LightWrap ("Light Wrap", Range(0,1)) = 1
        _SimpleLightEnable ("Simple Light Enable", Float) = 1
        _SDFTex ("SDF Texture", 2D) = "white" {}
        _OutlineEnable ("Outline Enable", Float) = 0
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0,1)) = 0.1
        _OutlineSoftness ("Outline Softness", Range(0,1)) = 0
        _OutlineMode ("Outline Mode", Range(0,2)) = 2
        _OutlineUseVertColor ("Outline Use Vertex Color", Float) = 0
        _BlockedShakeAngle ("Blocked Shake Angle", Float) = 0
        _Cull ("Cull", Float) = 0
        _ZWrite ("ZWrite", Float) = 1
        _ZTest ("ZTest", Float) = 4
        _SrcBlend ("Src Blend", Float) = 5
        _DstBlend ("Dst Blend", Float) = 10
    }
    SubShader
    {
        Tags { "Queue"="Transparent-9" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull [_Cull]
        ZWrite [_ZWrite]
        ZTest [_ZTest]
        Blend [_SrcBlend] [_DstBlend]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_local _ _OUTLINE_ON
            #include "UnityCG.cginc"

            sampler2D _MainTex, _NormalMap, _SDFTex;
            float4 _MainTex_TexelSize, _SDFTex_TexelSize, _GlobalMipBias;
            float4 _StyleLineRectMaskRect, _StyleLineRectMaskSoftness;
            float4 _Color, _TailRect, _BodyRect, _HeadRect, _ExtraRectA, _ExtraRectB, _ExtraRectC, _LightColor, _ShadowColor, _LightDir, _OutlineColor;
            float _ColorIntensity, _Blend, _RecolorStrength, _Contrast, _Saturation, _UseVertexColor, _ColorfulBlend, _ExtraEnable;
            float _AlphaClipEnable, _AlphaClip, _DrivenByController, _WindowStart, _VisibleLength, _TailLength, _HeadLength, _BodyTileLength, _BodyPhase;
            float _StyleRowCount, _StyleIndexFromMesh, _StyleIndex, _AtlasPaddingPixels, _MipClamp;
            float _NormalScale, _NormalMapEnable, _LightIntensity, _LightStrength, _LightWrap, _SimpleLightEnable;
            float _OutlineEnable, _OutlineWidth, _OutlineSoftness, _OutlineMode, _OutlineUseVertColor, _BlockedShakeAngle;

            struct appdata
            {
                float4 vertex:POSITION;
                float4 color:COLOR;
                float2 uv:TEXCOORD0;
                float2 ringData:TEXCOORD1;
                float2 outline:TEXCOORD2;
                float4 tangent:TANGENT;
                float3 normal:NORMAL;
            };
            struct v2f
            {
                float4 pos:SV_POSITION;
                float4 color:COLOR;
                float2 uv:TEXCOORD0;
                float2 ringData:TEXCOORD1;
                float2 outline:TEXCOORD2;
                float3 worldNormal:TEXCOORD3;
                float3 worldTangent:TEXCOORD4;
                float3 worldBinormal:TEXCOORD5;
                float style:TEXCOORD6;
            };

            v2f vert(appdata v)
            {
                v2f o;
                float a = radians(_BlockedShakeAngle);
                float s = sin(a), c = cos(a);
                v.vertex.xy = float2(c*v.vertex.x-s*v.vertex.y, s*v.vertex.x+c*v.vertex.y);
                v.tangent.xy = float2(c*v.tangent.x-s*v.tangent.y, s*v.tangent.x+c*v.tangent.y);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv = v.uv;
                o.ringData = v.ringData;
                o.outline = v.outline;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                o.worldBinormal = cross(o.worldNormal, o.worldTangent) * v.tangent.w;
                o.style = _StyleIndexFromMesh > .5 ? abs(v.ringData.y) : _StyleIndex;
                return o;
            }

            float2 atlasRect(float4 rect, float2 path, float2 pad)
            {
                float2 uv = rect.xy + path * rect.zw;
                uv = clamp(uv, rect.xy + pad, rect.xy + rect.zw - pad);
                return uv;
            }

            float2 clampGradient(float2 gradient)
            {
                float magnitude=length(gradient);
                if (magnitude>_MipClamp) gradient*=(_MipClamp/magnitude);
                float maxGradient=max(_MainTex_TexelSize.x,_MainTex_TexelSize.y)*8;
                magnitude=length(gradient);
                if (magnitude>maxGradient) gradient*=maxGradient/magnitude;
                return gradient;
            }

            fixed4 frag(v2f i):SV_Target
            {
                float distance = i.uv.x - _WindowStart;
                float total = max(i.ringData.x, 0.0001);
                if (_DrivenByController > .5 && (distance < 0 || distance > min(total, _VisibleLength))) discard;

                float capScale = total < _TailLength+_HeadLength ? total/max(_TailLength+_HeadLength,.0001) : 1;
                float tail = max(0, _TailLength*capScale);
                float head = max(0, _HeadLength*capScale);
                float headStart = max(tail, total - head);
                float2 atlasUV;
                float2 atlasDx;
                float2 atlasDy;
                float bodyLength = max(headStart-tail, 0);
                float density = bodyLength > .0001 && _BodyTileLength > .0001 ? max(floor(bodyLength/_BodyTileLength+.5),1)/bodyLength : 1/max(_BodyTileLength,.0001);
                float rowCount=max(_StyleRowCount,1);
                float styleIndex=_StyleIndexFromMesh>.5?abs(i.ringData.y):_StyleIndex;
                float row=clamp(styleIndex-1,0,max(rowCount-1,0));
                float rowHeight=1/rowCount;
                float extraIndex=clamp(round(i.outline.y),1,3);
                bool isExtra=_ExtraEnable>.5 && i.ringData.y<0;
                float padX = _AtlasPaddingPixels * _MainTex_TexelSize.x;
                float padY = _AtlasPaddingPixels * _MainTex_TexelSize.y;
                if (isExtra)
                {
                    float4 rect=extraIndex<1.5?_ExtraRectA:(extraIndex<2.5?_ExtraRectB:_ExtraRectC);
                    float2 local=float2(saturate(distance/total),i.uv.y);
                    atlasUV=atlasRect(rect,local,float2(padX,padY));
                    atlasUV.y+=row*rowHeight;
                    atlasDx=float2(rect.z/total*ddx(distance),rect.w*ddx(i.uv.y));
                    atlasDy=float2(rect.z/total*ddy(distance),rect.w*ddy(i.uv.y));
                }
                else if (distance <= tail && tail > .0001)
                {
                    float2 local = float2(saturate(distance / tail), i.uv.y);
                    atlasUV = atlasRect(_TailRect, local, float2(padX,padY));
                    atlasUV.y+=row*rowHeight;
                    atlasDx = float2(_TailRect.z/tail*ddx(distance), _TailRect.w*ddx(i.uv.y));
                    atlasDy = float2(_TailRect.z/tail*ddy(distance), _TailRect.w*ddy(i.uv.y));
                    atlasUV.x *= .5; atlasDx.x *= .5; atlasDy.x *= .5;
                }
                else if (distance >= headStart && head > .0001)
                {
                    float2 local = float2(saturate((distance-headStart)/head), i.uv.y);
                    atlasUV = atlasRect(_HeadRect, local, float2(padX,padY));
                    atlasUV.y+=row*rowHeight;
                    atlasDx = float2(_HeadRect.z/head*ddx(distance), _HeadRect.w*ddx(i.uv.y));
                    atlasDy = float2(_HeadRect.z/head*ddy(distance), _HeadRect.w*ddy(i.uv.y));
                    atlasUV.x *= .5; atlasDx.x *= .5; atlasDy.x *= .5;
                }
                else
                {
                    float bodyU = 1-frac((headStart-distance+_BodyPhase)*density);
                    float2 local = float2(bodyU, i.uv.y);
                    atlasUV = atlasRect(_BodyRect, local, float2(padX,padY));
                    atlasUV.y+=row*rowHeight;
                    atlasDx = float2(_BodyRect.z*density*ddx(distance), _BodyRect.w*ddx(i.uv.y));
                    atlasDy = float2(_BodyRect.z*density*ddy(distance), _BodyRect.w*ddy(i.uv.y));
                    atlasUV.x *= .5; atlasDx.x *= .5; atlasDy.x *= .5;
                }
                atlasDx=clampGradient(atlasDx);
                atlasDy=clampGradient(atlasDy);
                float2 sampleDx=atlasDx*_GlobalMipBias.y;
                float2 sampleDy=atlasDy*_GlobalMipBias.y;
                fixed4 tex = tex2Dgrad(_MainTex, atlasUV, sampleDx, sampleDy);
                fixed3 sampled = lerp(tex.rgb, 1, _Blend);
                fixed3 gammaTint = _Color.rgb * (_Color.rgb * (_Color.rgb*.305306017 + .682171106) + .0125228781);
                fixed3 tint = gammaTint * _ColorIntensity;
                fixed3 rgb = sampled * tint;
                fixed3 gray = dot(rgb, fixed3(.299,.587,.114));
                rgb = lerp(rgb, gray.xxx, _RecolorStrength);
                rgb = (rgb-.5)*_Contrast+.5;
                float alpha = tex.a * _Color.a;
                if (_UseVertexColor > .5)
                {
                    fixed3 multiplyColor = rgb * i.color.rgb;
                    fixed3 screenColor = 1-(1-rgb)*(1-i.color.rgb);
                    rgb = _ColorfulBlend > .5 ? screenColor : multiplyColor;
                    alpha *= i.color.a;
                }
                gray = dot(rgb, fixed3(.299,.587,.114));
                rgb = lerp(gray.xxx, rgb, _Saturation);

                if (_SimpleLightEnable > .5 && _NormalMapEnable > .5)
                {
                    float3 n = tex2Dgrad(_NormalMap, atlasUV, sampleDx, sampleDy).xyz*2-1;
                    n.xy *= _NormalScale;
                    n = normalize(i.worldTangent*n.x + i.worldBinormal*n.y + i.worldNormal*n.z);
                    float ndl = dot(n, normalize(_LightDir.xyz));
                    float diffuse = lerp(saturate(ndl), saturate(ndl*.5+.5), _LightWrap);
                    fixed3 gammaShadow = _ShadowColor.rgb * (_ShadowColor.rgb * (_ShadowColor.rgb*.305306017 + .682171106) + .0125228781);
                    fixed3 gammaLight = _LightColor.rgb * (_LightColor.rgb * (_LightColor.rgb*.305306017 + .682171106) + .0125228781);
                    fixed3 light = lerp(gammaShadow, gammaLight, diffuse);
                    fixed3 sourceLighting = 1-(1-rgb)*(1-_LightStrength*light);
                    rgb *= sourceLighting * _LightIntensity;
                }

                float outlineEnabled = _OutlineEnable;
                #if defined(_OUTLINE_ON)
                    outlineEnabled = 1;
                #endif
                if (outlineEnabled > .5 && i.outline.x > .001)
                {
                    float sdf = tex2Dgrad(_SDFTex, atlasUV, sampleDx, sampleDy).a;
                    float halfWidth = _OutlineWidth*.5*i.outline.x;
                    float sdfPixelSpan = max(length(atlasDx),length(atlasDy))*_SDFTex_TexelSize.z;
                    float softness = max(sdfPixelSpan*_OutlineSoftness,.0001);
                    float insideThreshold, outerThreshold;
                    if (_OutlineMode < .5) { insideThreshold=.5+2*halfWidth; outerThreshold=.5; }
                    else if (_OutlineMode > 1.5) { insideThreshold=.5; outerThreshold=.5-2*halfWidth; }
                    else { insideThreshold=.5+halfWidth; outerThreshold=.5-halfWidth; }
                    float innerCoverage = saturate((sdf-insideThreshold)/softness);
                    float outerCoverage = saturate((sdf-outerThreshold)/softness);
                    fixed3 gammaOutline = _OutlineColor.rgb*(_OutlineColor.rgb*(_OutlineColor.rgb*.305306017+.682171106)+.0125228781);
                    fixed3 outlineColor = gammaOutline;
                    if (_OutlineUseVertColor > .5) outlineColor += i.color.rgb;
                    float bodyAlpha = alpha*innerCoverage;
                    float outlineAlpha = _OutlineColor.a*(outerCoverage-innerCoverage);
                    rgb = outlineColor + bodyAlpha*(rgb-outlineColor);
                    alpha = outlineAlpha+bodyAlpha;
                }
                float windowMask = step(0,distance)*step(distance,total);
                alpha *= windowMask;
                float2 maskEdge = min(i.pos.xy - _StyleLineRectMaskRect.xy,
                    _StyleLineRectMaskRect.zw - i.pos.xy);
                if (min(maskEdge.x, maskEdge.y) < 0) discard;
                float2 maskSoftness = max(_StyleLineRectMaskSoftness.xy, 0.0001);
                alpha *= saturate(min(maskEdge.x / maskSoftness.x, maskEdge.y / maskSoftness.y));
                if (_AlphaClipEnable > .5 && alpha < _AlphaClip) discard;
                return fixed4(rgb, alpha);
            }
            ENDCG
        }
    }
    FallBack Off
}

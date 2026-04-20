Shader "Hidden/Acfeel/UIToolkitLiteOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        Cull Off
        ZWrite Off
        ZTest Always
        Blend Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "AcfeelUIToolkitLiteDissolve.hlsl"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTexTexelSize;
            float4 _ContentUvRect;
            float4 _OutlineColor;
            float _OutlineThickness;
            float _OutlineSampleQuality;
            float _DissolveEnabled;
            float _DissolveAmount;
            float _DissolveEdgeWidth;
            float4 _CornerRadii;
            float4 _RectSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float SDFRoundedRect(float2 p, float2 size, float4 radii)
            {
                float2 c = abs(p) - size * 0.5;
                float radius = (p.x > 0.0)
                    ? ((p.y > 0.0) ? radii.z : radii.y)
                    : ((p.y > 0.0) ? radii.w : radii.x);
                float2 q = c + radius;
                return min(max(c.x, c.y), 0.0) + length(max(q, 0.0)) - radius;
            }

            bool IsInsideContent(float2 uv)
            {
                return uv.x >= _ContentUvRect.x
                    && uv.y >= _ContentUvRect.y
                    && uv.x <= _ContentUvRect.z
                    && uv.y <= _ContentUvRect.w;
            }

            float2 RemapContentUv(float2 uv)
            {
                return float2(
                    (uv.x - _ContentUvRect.x) / max(_ContentUvRect.z - _ContentUvRect.x, 0.0001),
                    (uv.y - _ContentUvRect.y) / max(_ContentUvRect.w - _ContentUvRect.y, 0.0001));
            }

            float SampleAlpha(float2 uv)
            {
                if (!IsInsideContent(uv))
                {
                    return 0.0;
                }

                float2 sampleUv = saturate(RemapContentUv(uv));
                float2 texelInset = _MainTexTexelSize.xy * 0.5;
                sampleUv = clamp(sampleUv, texelInset, 1.0 - texelInset);
                float alpha = tex2D(_MainTex, sampleUv).a;
                return step(0.7, alpha);
            }

            float GetOutlineMask(float2 uv)
            {
                float2 texel = _MainTexTexelSize.xy * max(_OutlineThickness, 0.0001);
                float sourceAlpha = SampleAlpha(uv);
                float neighborAlpha = 0.0;
                neighborAlpha = max(neighborAlpha, SampleAlpha(uv + float2(texel.x, 0.0)));
                neighborAlpha = max(neighborAlpha, SampleAlpha(uv + float2(-texel.x, 0.0)));
                neighborAlpha = max(neighborAlpha, SampleAlpha(uv + float2(0.0, texel.y)));
                neighborAlpha = max(neighborAlpha, SampleAlpha(uv + float2(0.0, -texel.y)));

                if (_OutlineSampleQuality > 0.5)
                {
                    float2 diagonal = texel * 0.70710678;
                    neighborAlpha = max(neighborAlpha, SampleAlpha(uv + float2(diagonal.x, diagonal.y)));
                    neighborAlpha = max(neighborAlpha, SampleAlpha(uv + float2(-diagonal.x, diagonal.y)));
                    neighborAlpha = max(neighborAlpha, SampleAlpha(uv + float2(diagonal.x, -diagonal.y)));
                    neighborAlpha = max(neighborAlpha, SampleAlpha(uv + float2(-diagonal.x, -diagonal.y)));
                }

                return saturate(neighborAlpha - sourceAlpha) * LiteEffectGetDissolveMask(uv, _DissolveEnabled, _DissolveAmount, _DissolveEdgeWidth);
            }

            float4 frag(v2f i) : SV_Target
            {
                float mask = GetOutlineMask(i.uv);
                clip(mask - 0.001);

                // Apply rounded corner mask
                // Note: overlay is padded, so adjust pixel position by padding offset
                float2 pixelPos = i.uv * _MainTexTexelSize.zw;
                float2 contentSize = _RectSize.xy;
                float padding = _RectSize.z;
                float2 contentPixelPos = pixelPos - padding;
                float2 contentCenter = contentSize * 0.5;
                float2 localPos = contentPixelPos - contentCenter;
                float cornerDist = SDFRoundedRect(localPos, contentSize, _CornerRadii);
                float cornerMask = step(0.0, -cornerDist);

                return float4(_OutlineColor.rgb, mask * _OutlineColor.a * cornerMask);
            }
            ENDHLSL
        }
    }
}

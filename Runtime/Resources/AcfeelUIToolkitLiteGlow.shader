Shader "Hidden/Acfeel/UIToolkitLiteGlow"
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
            float4 _GlowColor;
            float _GlowStrength;
            float _GlowSpread;
            float _SourceAlphaMultiplier;
            float _DissolveEnabled;
            float _DissolveAmount;
            float _DissolveEdgeWidth;

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

            float SampleRawAlpha(float2 uv)
            {
                if (!IsInsideContent(uv))
                {
                    return 0.0;
                }

                float2 sampleUv = saturate(RemapContentUv(uv));
                float2 texelInset = _MainTexTexelSize.xy * 0.5;
                sampleUv = clamp(sampleUv, texelInset, 1.0 - texelInset);
                return tex2D(_MainTex, sampleUv).a * _SourceAlphaMultiplier;
            }

            float SampleContentDissolveMask(float2 uv)
            {
                if (!IsInsideContent(uv))
                {
                    return 0.0;
                }

                return LiteEffectGetDissolveMask(saturate(RemapContentUv(uv)), _DissolveEnabled, _DissolveAmount, _DissolveEdgeWidth);
            }

            float SampleGlowSourceAlpha(float2 uv)
            {
                float rawAlpha = SampleRawAlpha(uv);
                float glowAlpha = smoothstep(0.35, 0.85, rawAlpha);
                return glowAlpha * SampleContentDissolveMask(uv);
            }

            float GetGlowSpreadPixels(float normalizedSpread)
            {
                return saturate(normalizedSpread) * 4.0;
            }

            float GetOutlineMask(float2 uv, float thickness)
            {
                float2 texel = _MainTexTexelSize.xy * max(thickness, 0.0001);
                float sourceAlpha = SampleGlowSourceAlpha(uv);
                float neighborAlpha = 0.0;
                neighborAlpha = max(neighborAlpha, SampleGlowSourceAlpha(uv + float2(texel.x, 0.0)));
                neighborAlpha = max(neighborAlpha, SampleGlowSourceAlpha(uv + float2(-texel.x, 0.0)));
                neighborAlpha = max(neighborAlpha, SampleGlowSourceAlpha(uv + float2(0.0, texel.y)));
                neighborAlpha = max(neighborAlpha, SampleGlowSourceAlpha(uv + float2(0.0, -texel.y)));
                float2 diagonal = texel * 0.70710678;
                neighborAlpha = max(neighborAlpha, SampleGlowSourceAlpha(uv + float2(diagonal.x, diagonal.y)));
                neighborAlpha = max(neighborAlpha, SampleGlowSourceAlpha(uv + float2(-diagonal.x, diagonal.y)));
                neighborAlpha = max(neighborAlpha, SampleGlowSourceAlpha(uv + float2(diagonal.x, -diagonal.y)));
                neighborAlpha = max(neighborAlpha, SampleGlowSourceAlpha(uv + float2(-diagonal.x, -diagonal.y)));
                return saturate(neighborAlpha - sourceAlpha);
            }

            float4 frag(v2f i) : SV_Target
            {
                float spread = max(GetGlowSpreadPixels(_GlowSpread), 0.0001);
                float strength = saturate(_GlowStrength);
                float inner = GetOutlineMask(i.uv, spread * 0.85);
                float middle = GetOutlineMask(i.uv, spread * 1.7);
                float outer = GetOutlineMask(i.uv, spread * 2.7);
                float middlePresence = smoothstep(0.15, 0.65, strength);
                float outerPresence = smoothstep(0.45, 1.0, strength);
                float mask = saturate(
                    inner * 0.7
                    + middle * 0.45 * middlePresence
                    + outer * 0.25 * outerPresence);
                mask *= strength;
                mask *= 1.0 - SampleGlowSourceAlpha(i.uv);
                float alpha = saturate(mask * _GlowColor.a);
                return float4(_GlowColor.rgb * alpha, alpha);
            }
            ENDHLSL
        }
    }
}

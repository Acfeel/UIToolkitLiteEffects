Shader "Hidden/Acfeel/UIToolkitLiteEffects"
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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _BaseColor;
            float _Brightness;
            float _Contrast;
            float _Saturation;
            float4 _Multiply;
            float4 _Add;
            float _GradientEnabled;
            float4 _GradientFrom;
            float4 _GradientTo;
            float4 _GradientDirection;
            float _GradientMode;
            float _BlendEnabled;
            float _BlendMode;
            float _BlendStrength;
            float _OutlineEnabled;
            float4 _OutlineColor;
            float _OutlineThickness;
            float _OutlineOpacity;
            float _GlowEnabled;
            float4 _GlowColor;
            float _GlowStrength;
            float _GlowSpread;
            float _BlurEnabled;
            float _BlurRadius;
            float _BlurStrength;
            float _DissolveEnabled;
            float _DissolveAmount;
            float _DissolveEdgeWidth;
            float4 _DissolveEdgeColor;
            float _GlitchEnabled;
            float _GlitchIntensity;
            float _GlitchJitter;
            float _GlitchColorShift;
            float _GlitchScanlineStrength;
            float _LiteEffectTime;
            float4 _MainTexTexelSize;
            float4 _ContentUvRect;
            float _OutlineOnly;

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

            float3 ApplyContrast(float3 color, float contrast)
            {
                return (color - 0.5) * contrast + 0.5;
            }

            float3 ApplySaturation(float3 color, float saturation)
            {
                float luminance = dot(color, float3(0.2126, 0.7152, 0.0722));
                return lerp(luminance.xxx, color, saturation);
            }

            float4 ApplyMode(float4 source, float4 target, float mode, float strength)
            {
                if (mode < 0.5)
                {
                    return lerp(source, target, strength);
                }

                if (mode < 1.5)
                {
                    float4 multiplied = float4(source.rgb * target.rgb, source.a * target.a);
                    return lerp(source, multiplied, strength);
                }

                float4 additive = float4(source.rgb + target.rgb * strength, saturate(source.a + target.a * strength));
                return float4(saturate(additive.rgb), additive.a);
            }

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
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

            float4 SampleSource(float2 uv)
            {
                if (!IsInsideContent(uv))
                {
                    return 0.0;
                }

                float2 sampleUv = saturate(RemapContentUv(uv));
                float4 sample = tex2D(_MainTex, sampleUv) * _BaseColor;
                float fringeFix = smoothstep(0.0, 0.2, sample.a);
                sample.rgb *= fringeFix;
                return sample;
            }

            float GetNeighborAlpha(float2 uv, float2 offset)
            {
                return SampleSource(uv + offset).a;
            }

            float GetNeighborSilhouetteAlpha(float2 uv, float2 offset)
            {
                return step(0.7, GetNeighborAlpha(uv, offset));
            }

            float GetOutlineMask(float2 uv, float sourceAlpha, float thickness)
            {
                float2 texel = _MainTexTexelSize.xy * max(thickness, 0.0001);
                float neighborAlpha = 0.0;
                neighborAlpha = max(neighborAlpha, GetNeighborAlpha(uv, float2(texel.x, 0.0)));
                neighborAlpha = max(neighborAlpha, GetNeighborAlpha(uv, float2(-texel.x, 0.0)));
                neighborAlpha = max(neighborAlpha, GetNeighborAlpha(uv, float2(0.0, texel.y)));
                neighborAlpha = max(neighborAlpha, GetNeighborAlpha(uv, float2(0.0, -texel.y)));
                float2 diagonal = texel * 0.70710678;
                neighborAlpha = max(neighborAlpha, GetNeighborAlpha(uv, float2(diagonal.x, diagonal.y)));
                neighborAlpha = max(neighborAlpha, GetNeighborAlpha(uv, float2(-diagonal.x, diagonal.y)));
                neighborAlpha = max(neighborAlpha, GetNeighborAlpha(uv, float2(diagonal.x, -diagonal.y)));
                neighborAlpha = max(neighborAlpha, GetNeighborAlpha(uv, float2(-diagonal.x, -diagonal.y)));
                return saturate(neighborAlpha - sourceAlpha);
            }

            float GetGlowOutlineMask(float2 uv, float sourceSilhouetteAlpha, float thickness)
            {
                float2 texel = _MainTexTexelSize.xy * max(thickness, 0.0001);
                float neighborAlpha = 0.0;
                neighborAlpha = max(neighborAlpha, GetNeighborSilhouetteAlpha(uv, float2(texel.x, 0.0)));
                neighborAlpha = max(neighborAlpha, GetNeighborSilhouetteAlpha(uv, float2(-texel.x, 0.0)));
                neighborAlpha = max(neighborAlpha, GetNeighborSilhouetteAlpha(uv, float2(0.0, texel.y)));
                neighborAlpha = max(neighborAlpha, GetNeighborSilhouetteAlpha(uv, float2(0.0, -texel.y)));
                float2 diagonal = texel * 0.70710678;
                neighborAlpha = max(neighborAlpha, GetNeighborSilhouetteAlpha(uv, float2(diagonal.x, diagonal.y)));
                neighborAlpha = max(neighborAlpha, GetNeighborSilhouetteAlpha(uv, float2(-diagonal.x, diagonal.y)));
                neighborAlpha = max(neighborAlpha, GetNeighborSilhouetteAlpha(uv, float2(diagonal.x, -diagonal.y)));
                neighborAlpha = max(neighborAlpha, GetNeighborSilhouetteAlpha(uv, float2(-diagonal.x, -diagonal.y)));
                return saturate(neighborAlpha - sourceSilhouetteAlpha);
            }

            float GetGlowMask(float2 uv, float sourceAlpha, float spread)
            {
                float sourceSilhouetteAlpha = step(0.7, sourceAlpha);
                float innerMask = GetGlowOutlineMask(uv, sourceSilhouetteAlpha, max(spread * 0.9, 0.0001));
                float outerMask = GetGlowOutlineMask(uv, sourceSilhouetteAlpha, max(spread * 1.8, 0.0001));
                return saturate(innerMask * 0.7 + outerMask * 0.45);
            }

            float GetDissolveNoise(float2 uv)
            {
                float primary = Hash21(floor(uv * 160.0));
                float secondary = Hash21(floor(uv * 320.0) + 17.0);
                return saturate(primary * 0.7 + secondary * 0.3);
            }

            float3 SampleBlur(float2 uv, float radius)
            {
                float2 texel = _MainTexTexelSize.xy * max(radius, 0.0001);
                float4 center = SampleSource(uv);
                float4 right = SampleSource(uv + float2(texel.x, 0.0));
                float4 left = SampleSource(uv + float2(-texel.x, 0.0));
                float4 up = SampleSource(uv + float2(0.0, texel.y));
                float4 down = SampleSource(uv + float2(0.0, -texel.y));
                float2 diagonal = texel * 0.70710678;
                float4 topRight = SampleSource(uv + float2(diagonal.x, diagonal.y));
                float4 topLeft = SampleSource(uv + float2(-diagonal.x, diagonal.y));
                float4 bottomRight = SampleSource(uv + float2(diagonal.x, -diagonal.y));
                float4 bottomLeft = SampleSource(uv + float2(-diagonal.x, -diagonal.y));

                float centerWeight = 2.0;
                float crossWeight = 1.0;
                float cornerWeight = 0.75;

                float alphaWeight =
                    center.a * centerWeight +
                    (right.a + left.a + up.a + down.a) * crossWeight +
                    (topRight.a + topLeft.a + bottomRight.a + bottomLeft.a) * cornerWeight;

                float3 weightedColor =
                    center.rgb * center.a * centerWeight +
                    (right.rgb * right.a + left.rgb * left.a + up.rgb * up.a + down.rgb * down.a) * crossWeight +
                    (topRight.rgb * topRight.a + topLeft.rgb * topLeft.a + bottomRight.rgb * bottomRight.a + bottomLeft.rgb * bottomLeft.a) * cornerWeight;

                if (alphaWeight <= 0.0001)
                {
                    return center.rgb;
                }

                return weightedColor / alphaWeight;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                if (_GlitchEnabled > 0.5 && _GlitchIntensity > 0.0001)
                {
                    float lineNoise = Hash21(float2(floor(uv.y * 96.0), floor(_LiteEffectTime * 18.0)));
                    float jitter = (lineNoise - 0.5) * _GlitchIntensity * _GlitchJitter * 0.08;
                    uv.x = saturate(uv.x + jitter);
                }

                float4 source = SampleSource(uv);
                if (_OutlineOnly > 0.5)
                {
                    float outlineMask = GetOutlineMask(uv, source.a, _OutlineThickness) * _OutlineOpacity;
                    clip(outlineMask - 0.001);
                    return float4(_OutlineColor.rgb, outlineMask * _OutlineColor.a);
                }

                float4 processed = source;
                float brightness = (_Brightness - 0.5) * 2.0;
                float contrast = max(0.0, _Contrast * 2.0);
                float saturation = max(0.0, _Saturation * 2.0);

                if (_GradientEnabled > 0.5)
                {
                    float2 direction = normalize(_GradientDirection.xy);
                    float t = saturate(dot(uv - 0.5, direction) + 0.5);
                    float4 gradient = lerp(_GradientFrom, _GradientTo, t);
                    processed = ApplyMode(processed, gradient, _GradientMode, 1.0);
                }

                if (_OutlineEnabled > 0.5 && _OutlineThickness > 0.0001 && _OutlineOpacity > 0.0001)
                {
                    float outlineMask = GetOutlineMask(uv, source.a, _OutlineThickness) * _OutlineOpacity;
                    processed.rgb = lerp(processed.rgb, _OutlineColor.rgb, outlineMask * _OutlineColor.a);
                    processed.a = saturate(processed.a + outlineMask * _OutlineColor.a);
                }

                if (_BlendEnabled > 0.5)
                {
                    processed = ApplyMode(source, processed, _BlendMode, _BlendStrength);
                }

                if (_BlurEnabled > 0.5 && _BlurRadius > 0.0001 && _BlurStrength > 0.0001)
                {
                    float3 blurred = SampleBlur(uv, _BlurRadius);
                    processed.rgb = lerp(processed.rgb, blurred, _BlurStrength);
                }

                if (_GlowEnabled > 0.5 && _GlowStrength > 0.0001 && _GlowSpread > 0.0001)
                {
                    float glowMask = GetGlowMask(uv, source.a, _GlowSpread) * _GlowStrength;
                    processed.rgb = saturate(processed.rgb + _GlowColor.rgb * glowMask * _GlowColor.a);
                    processed.a = saturate(processed.a + glowMask * _GlowColor.a * 0.5);
                }

                if (_DissolveEnabled > 0.5 && _DissolveAmount > 0.0001)
                {
                    float microNoise = Hash21(floor(uv * 640.0) + 37.0);
                    float noise = saturate(GetDissolveNoise(uv) * 0.75 + microNoise * 0.25);
                    float dissolveAmount = saturate(_DissolveAmount);
                    float edgeWidth = max(_DissolveEdgeWidth, 0.0001);
                    float dissolveMask = dissolveAmount >= 0.9999
                        ? 0.0
                        : saturate((noise - dissolveAmount) / edgeWidth) * (1.0 - dissolveAmount);
                    processed.rgb *= dissolveMask;
                    processed.a *= dissolveMask;

                    if (_DissolveEdgeColor.a > 0.0001)
                    {
                        float edge = 1.0 - dissolveMask;
                        processed.rgb = lerp(processed.rgb, _DissolveEdgeColor.rgb, edge * _DissolveEdgeColor.a * 0.25);
                    }
                }

                if (_GlitchEnabled > 0.5 && _GlitchIntensity > 0.0001)
                {
                    float shift = _MainTexTexelSize.x * _GlitchColorShift * _GlitchIntensity * 8.0;
                    float r = SampleSource(uv + float2(shift, 0.0)).r;
                    float b = SampleSource(uv - float2(shift, 0.0)).b;
                    processed.r = lerp(processed.r, r, _GlitchIntensity);
                    processed.b = lerp(processed.b, b, _GlitchIntensity);
                    float scan = sin((uv.y + _LiteEffectTime * 2.7) * 180.0) * 0.5 + 0.5;
                    processed.rgb *= 1.0 - scan * _GlitchScanlineStrength * _GlitchIntensity * 0.25;
                }

                processed.rgb += brightness;
                processed.rgb = ApplyContrast(processed.rgb, contrast);
                processed.rgb = ApplySaturation(processed.rgb, saturation);
                processed *= _Multiply;
                processed += _Add;

                processed.rgb = saturate(processed.rgb);
                processed.a = saturate(processed.a);
                return processed;
            }
            ENDHLSL
        }
    }
}

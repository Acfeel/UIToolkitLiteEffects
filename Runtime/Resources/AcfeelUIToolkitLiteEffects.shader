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

            float4 frag(v2f i) : SV_Target
            {
                float4 source = tex2D(_MainTex, i.uv) * _BaseColor;
                float4 processed = source;
                float brightness = (_Brightness - 0.5) * 2.0;
                float contrast = max(0.0, _Contrast * 2.0);
                float saturation = max(0.0, _Saturation * 2.0);

                if (_GradientEnabled > 0.5)
                {
                    float2 direction = normalize(_GradientDirection.xy);
                    float t = saturate(dot(i.uv - 0.5, direction) + 0.5);
                    float4 gradient = lerp(_GradientFrom, _GradientTo, t);
                    processed = ApplyMode(processed, gradient, _GradientMode, 1.0);
                }

                if (_BlendEnabled > 0.5)
                {
                    processed = ApplyMode(source, processed, _BlendMode, _BlendStrength);
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

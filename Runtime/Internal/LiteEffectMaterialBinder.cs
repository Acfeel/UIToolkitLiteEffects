using UnityEngine;

namespace Acfeel.UIToolkitLiteEffects
{
    internal static class LiteEffectMaterialBinder
    {
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int BrightnessId = Shader.PropertyToID("_Brightness");
        private static readonly int ContrastId = Shader.PropertyToID("_Contrast");
        private static readonly int SaturationId = Shader.PropertyToID("_Saturation");
        private static readonly int HueId = Shader.PropertyToID("_Hue");
        private static readonly int MultiplyId = Shader.PropertyToID("_Multiply");
        private static readonly int AddId = Shader.PropertyToID("_Add");
        private static readonly int GradientEnabledId = Shader.PropertyToID("_GradientEnabled");
        private static readonly int GradientFromId = Shader.PropertyToID("_GradientFrom");
        private static readonly int GradientToId = Shader.PropertyToID("_GradientTo");
        private static readonly int GradientDirectionId = Shader.PropertyToID("_GradientDirection");
        private static readonly int GradientModeId = Shader.PropertyToID("_GradientMode");
        private static readonly int GradientStrengthId = Shader.PropertyToID("_GradientStrength");
        private static readonly int OutlineEnabledId = Shader.PropertyToID("_OutlineEnabled");
        private static readonly int GlowEnabledId = Shader.PropertyToID("_GlowEnabled");
        private static readonly int GlowColorId = Shader.PropertyToID("_GlowColor");
        private static readonly int GlowStrengthId = Shader.PropertyToID("_GlowStrength");
        private static readonly int GlowSpreadId = Shader.PropertyToID("_GlowSpread");
        private static readonly int BlurEnabledId = Shader.PropertyToID("_BlurEnabled");
        private static readonly int BlurRadiusId = Shader.PropertyToID("_BlurRadius");
        private static readonly int BlurStrengthId = Shader.PropertyToID("_BlurStrength");
        private static readonly int DissolveEnabledId = Shader.PropertyToID("_DissolveEnabled");
        private static readonly int DissolveAmountId = Shader.PropertyToID("_DissolveAmount");
        private static readonly int DissolveEdgeWidthId = Shader.PropertyToID("_DissolveEdgeWidth");
        private static readonly int DissolveEdgeColorId = Shader.PropertyToID("_DissolveEdgeColor");
        private static readonly int GlitchEnabledId = Shader.PropertyToID("_GlitchEnabled");
        private static readonly int GlitchIntensityId = Shader.PropertyToID("_GlitchIntensity");
        private static readonly int GlitchJitterId = Shader.PropertyToID("_GlitchJitter");
        private static readonly int GlitchColorShiftId = Shader.PropertyToID("_GlitchColorShift");
        private static readonly int GlitchScanlineStrengthId = Shader.PropertyToID("_GlitchScanlineStrength");
        private static readonly int ColorizeEnabledId = Shader.PropertyToID("_ColorizeEnabled");
        private static readonly int ColorizeColorId = Shader.PropertyToID("_ColorizeColor");
        private static readonly int ColorizeStrengthId = Shader.PropertyToID("_ColorizeStrength");
        private static readonly int TimeId = Shader.PropertyToID("_LiteEffectTime");
        private static readonly int TexelSizeId = Shader.PropertyToID("_MainTexTexelSize");
        private static readonly int ContentUvRectId = Shader.PropertyToID("_ContentUvRect");

        public static void Bind(Material material, Texture inputTexture, Color backgroundColor, ResolvedLiteEffectSettings resolvedSettings, RenderTexture processedTexture)
        {
            var gradientRadians = resolvedSettings.Gradient.Angle * Mathf.Deg2Rad;

            material.SetTexture(MainTexId, inputTexture);
            material.SetVector(BaseColorId, (Vector4)backgroundColor.linear);
            material.SetFloat(BrightnessId, resolvedSettings.ColorAdjust.Brightness);
            material.SetFloat(ContrastId, resolvedSettings.ColorAdjust.Contrast);
            material.SetFloat(SaturationId, resolvedSettings.ColorAdjust.Saturation);
            material.SetFloat(HueId, resolvedSettings.ColorAdjust.Hue);
            material.SetColor(MultiplyId, resolvedSettings.ColorAdjust.Multiply);
            material.SetColor(AddId, resolvedSettings.ColorAdjust.Add);
            material.SetFloat(GradientEnabledId, resolvedSettings.Gradient.Enabled ? 1f : 0f);
            material.SetColor(GradientFromId, resolvedSettings.Gradient.From);
            material.SetColor(GradientToId, resolvedSettings.Gradient.To);
            material.SetVector(GradientDirectionId, new Vector4(Mathf.Cos(gradientRadians), Mathf.Sin(gradientRadians), 0f, 0f));
            material.SetFloat(GradientModeId, (float)resolvedSettings.Gradient.Mode);
            material.SetFloat(GradientStrengthId, resolvedSettings.Gradient.Strength);
            material.SetFloat(GlowEnabledId, resolvedSettings.Glow.Enabled ? 1f : 0f);
            material.SetColor(GlowColorId, resolvedSettings.Glow.Color);
            material.SetFloat(GlowStrengthId, resolvedSettings.Glow.Strength);
            material.SetFloat(GlowSpreadId, resolvedSettings.Glow.Spread);
            material.SetFloat(BlurEnabledId, resolvedSettings.Blur.Enabled ? 1f : 0f);
            material.SetFloat(BlurRadiusId, resolvedSettings.Blur.Radius);
            material.SetFloat(BlurStrengthId, resolvedSettings.Blur.Strength);
            material.SetFloat(DissolveEnabledId, resolvedSettings.Dissolve.Enabled ? 1f : 0f);
            material.SetFloat(DissolveAmountId, resolvedSettings.Dissolve.Amount);
            material.SetFloat(DissolveEdgeWidthId, resolvedSettings.Dissolve.EdgeWidth);
            material.SetColor(DissolveEdgeColorId, resolvedSettings.Dissolve.EdgeColor);
            material.SetFloat(GlitchEnabledId, resolvedSettings.Glitch.Enabled ? 1f : 0f);
            material.SetFloat(GlitchIntensityId, resolvedSettings.Glitch.Intensity);
            material.SetFloat(GlitchJitterId, resolvedSettings.Glitch.Jitter);
            material.SetFloat(GlitchColorShiftId, resolvedSettings.Glitch.ColorShift);
            material.SetFloat(GlitchScanlineStrengthId, resolvedSettings.Glitch.ScanlineStrength);
            material.SetFloat(ColorizeEnabledId, resolvedSettings.Colorize.Enabled ? 1f : 0f);
            material.SetColor(ColorizeColorId, resolvedSettings.Colorize.Color);
            material.SetFloat(ColorizeStrengthId, resolvedSettings.Colorize.Strength);
            material.SetFloat(TimeId, Time.unscaledTime);
            material.SetFloat(OutlineEnabledId, 0f); // Outline is rendered via overlay element, not the main shader pass
            material.SetVector(TexelSizeId, new Vector4(1f / processedTexture.width, 1f / processedTexture.height, processedTexture.width, processedTexture.height));
            material.SetVector(ContentUvRectId, new Vector4(0f, 0f, 1f, 1f));
        }
    }
}

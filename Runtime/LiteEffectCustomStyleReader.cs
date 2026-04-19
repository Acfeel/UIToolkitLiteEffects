using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Acfeel.UIToolkitLiteEffects
{
    internal static class LiteEffectCustomStyleReader
    {
        private static readonly CustomStyleProperty<float> BrightnessProperty = new("--uitoolkitlitefx-brightness");
        private static readonly CustomStyleProperty<float> ContrastProperty = new("--uitoolkitlitefx-contrast");
        private static readonly CustomStyleProperty<float> SaturationProperty = new("--uitoolkitlitefx-saturation");
        private static readonly CustomStyleProperty<float> HueProperty = new("--uitoolkitlitefx-hue");
        private static readonly CustomStyleProperty<Color> MultiplyProperty = new("--uitoolkitlitefx-multiply");
        private static readonly CustomStyleProperty<Color> AddProperty = new("--uitoolkitlitefx-add");
        private static readonly CustomStyleProperty<Color> GradientFromProperty = new("--uitoolkitlitefx-gradient-from");
        private static readonly CustomStyleProperty<Color> GradientToProperty = new("--uitoolkitlitefx-gradient-to");
        private static readonly CustomStyleProperty<float> GradientAngleProperty = new("--uitoolkitlitefx-gradient-angle");
        private static readonly CustomStyleProperty<string> GradientModeProperty = new("--uitoolkitlitefx-gradient-mode");
        private static readonly CustomStyleProperty<float> GradientStrengthProperty = new("--uitoolkitlitefx-gradient-strength");
        private static readonly CustomStyleProperty<Color> OutlineColorProperty = new("--uitoolkitlitefx-outline-color");
        private static readonly CustomStyleProperty<float> OutlineThicknessProperty = new("--uitoolkitlitefx-outline-thickness");
        private static readonly CustomStyleProperty<string> OutlineQualityProperty = new("--uitoolkitlitefx-outline-quality");
        private static readonly CustomStyleProperty<Color> GlowColorProperty = new("--uitoolkitlitefx-glow-color");
        private static readonly CustomStyleProperty<float> GlowStrengthProperty = new("--uitoolkitlitefx-glow-strength");
        private static readonly CustomStyleProperty<float> GlowSpreadProperty = new("--uitoolkitlitefx-glow-spread");
        private static readonly CustomStyleProperty<float> BlurRadiusProperty = new("--uitoolkitlitefx-blur-radius");
        private static readonly CustomStyleProperty<float> BlurStrengthProperty = new("--uitoolkitlitefx-blur-strength");
        private static readonly CustomStyleProperty<float> DissolveAmountProperty = new("--uitoolkitlitefx-dissolve-amount");
        private static readonly CustomStyleProperty<float> DissolveEdgeWidthProperty = new("--uitoolkitlitefx-dissolve-edge-width");
        private static readonly CustomStyleProperty<Color> DissolveEdgeColorProperty = new("--uitoolkitlitefx-dissolve-edge-color");
        private static readonly CustomStyleProperty<float> GlitchIntensityProperty = new("--uitoolkitlitefx-glitch-intensity");
        private static readonly CustomStyleProperty<float> GlitchJitterProperty = new("--uitoolkitlitefx-glitch-jitter");
        private static readonly CustomStyleProperty<float> GlitchColorShiftProperty = new("--uitoolkitlitefx-glitch-color-shift");
        private static readonly CustomStyleProperty<float> GlitchScanlineStrengthProperty = new("--uitoolkitlitefx-glitch-scanline-strength");
        private static readonly CustomStyleProperty<Color> ColorizeColorProperty = new("--uitoolkitlitefx-colorize-color");
        private static readonly CustomStyleProperty<float> ColorizeStrengthProperty = new("--uitoolkitlitefx-colorize-strength");

        public static LiteEffectSettings Read(ICustomStyle customStyle)
        {
            var settings = new LiteEffectSettings();

            if (customStyle.TryGetValue(BrightnessProperty, out var brightness)
                || customStyle.TryGetValue(ContrastProperty, out var contrast)
                || customStyle.TryGetValue(SaturationProperty, out var saturation)
                || customStyle.TryGetValue(HueProperty, out var hue)
                || customStyle.TryGetValue(MultiplyProperty, out var multiply)
                || customStyle.TryGetValue(AddProperty, out var add))
            {
                settings.ColorAdjust = new ColorAdjustSettings();

                if (customStyle.TryGetValue(BrightnessProperty, out brightness))
                {
                    settings.ColorAdjust.Brightness = brightness;
                }

                if (customStyle.TryGetValue(ContrastProperty, out contrast))
                {
                    settings.ColorAdjust.Contrast = contrast;
                }

                if (customStyle.TryGetValue(SaturationProperty, out saturation))
                {
                    settings.ColorAdjust.Saturation = saturation;
                }

                if (customStyle.TryGetValue(HueProperty, out hue))
                {
                    settings.ColorAdjust.Hue = hue;
                }

                if (customStyle.TryGetValue(MultiplyProperty, out multiply))
                {
                    settings.ColorAdjust.Multiply = multiply;
                }

                if (customStyle.TryGetValue(AddProperty, out add))
                {
                    settings.ColorAdjust.Add = add;
                }
            }

            if (customStyle.TryGetValue(GradientFromProperty, out var from)
                || customStyle.TryGetValue(GradientToProperty, out var to)
                || customStyle.TryGetValue(GradientAngleProperty, out var angle)
                || customStyle.TryGetValue(GradientModeProperty, out var gradientModeText)
                || customStyle.TryGetValue(GradientStrengthProperty, out var gradientStrength))
            {
                settings.Gradient = new GradientSettings();

                if (customStyle.TryGetValue(GradientFromProperty, out from))
                {
                    settings.Gradient.From = from;
                }

                if (customStyle.TryGetValue(GradientToProperty, out to))
                {
                    settings.Gradient.To = to;
                }

                if (customStyle.TryGetValue(GradientAngleProperty, out angle))
                {
                    settings.Gradient.Angle = angle;
                }

                if (customStyle.TryGetValue(GradientModeProperty, out gradientModeText)
                    && Enum.TryParse<LiteEffectBlendMode>(gradientModeText, true, out var mode))
                {
                    settings.Gradient.Mode = mode;
                }

                if (customStyle.TryGetValue(GradientStrengthProperty, out gradientStrength))
                {
                    settings.Gradient.Strength = gradientStrength;
                }
            }

            if (customStyle.TryGetValue(OutlineColorProperty, out var outlineColor)
                || customStyle.TryGetValue(OutlineThicknessProperty, out var outlineThickness)
                || customStyle.TryGetValue(OutlineQualityProperty, out var outlineQualityText))
            {
                settings.Outline = new OutlineSettings();

                if (customStyle.TryGetValue(OutlineColorProperty, out outlineColor))
                {
                    settings.Outline.Color = outlineColor;
                }

                if (customStyle.TryGetValue(OutlineThicknessProperty, out outlineThickness))
                {
                    settings.Outline.Thickness = outlineThickness;
                }

                if (customStyle.TryGetValue(OutlineQualityProperty, out outlineQualityText)
                    && Enum.TryParse<LiteEffectOutlineQuality>(outlineQualityText, true, out var outlineQuality))
                {
                    settings.Outline.Quality = outlineQuality;
                }
            }

            if (customStyle.TryGetValue(GlowColorProperty, out var glowColor)
                || customStyle.TryGetValue(GlowStrengthProperty, out var glowStrength)
                || customStyle.TryGetValue(GlowSpreadProperty, out var glowSpread))
            {
                settings.Glow = new GlowSettings();

                if (customStyle.TryGetValue(GlowColorProperty, out glowColor))
                {
                    settings.Glow.Color = glowColor;
                }

                if (customStyle.TryGetValue(GlowStrengthProperty, out glowStrength))
                {
                    settings.Glow.Strength = glowStrength;
                }

                if (customStyle.TryGetValue(GlowSpreadProperty, out glowSpread))
                {
                    settings.Glow.Spread = glowSpread;
                }
            }

            if (customStyle.TryGetValue(BlurRadiusProperty, out var blurRadius)
                || customStyle.TryGetValue(BlurStrengthProperty, out var blurStrength))
            {
                settings.Blur = new BlurSettings();

                if (customStyle.TryGetValue(BlurRadiusProperty, out blurRadius))
                {
                    settings.Blur.Radius = blurRadius;
                }

                if (customStyle.TryGetValue(BlurStrengthProperty, out blurStrength))
                {
                    settings.Blur.Strength = blurStrength;
                }
            }

            if (customStyle.TryGetValue(DissolveAmountProperty, out var dissolveAmount)
                || customStyle.TryGetValue(DissolveEdgeWidthProperty, out var dissolveEdgeWidth)
                || customStyle.TryGetValue(DissolveEdgeColorProperty, out var dissolveEdgeColor))
            {
                settings.Dissolve = new DissolveSettings();

                if (customStyle.TryGetValue(DissolveAmountProperty, out dissolveAmount))
                {
                    settings.Dissolve.Amount = dissolveAmount;
                }

                if (customStyle.TryGetValue(DissolveEdgeWidthProperty, out dissolveEdgeWidth))
                {
                    settings.Dissolve.EdgeWidth = dissolveEdgeWidth;
                }

                if (customStyle.TryGetValue(DissolveEdgeColorProperty, out dissolveEdgeColor))
                {
                    settings.Dissolve.EdgeColor = dissolveEdgeColor;
                }
            }

            if (customStyle.TryGetValue(GlitchIntensityProperty, out var glitchIntensity)
                || customStyle.TryGetValue(GlitchJitterProperty, out var glitchJitter)
                || customStyle.TryGetValue(GlitchColorShiftProperty, out var glitchColorShift)
                || customStyle.TryGetValue(GlitchScanlineStrengthProperty, out var glitchScanlineStrength))
            {
                settings.Glitch = new GlitchSettings();

                if (customStyle.TryGetValue(GlitchIntensityProperty, out glitchIntensity))
                {
                    settings.Glitch.Intensity = glitchIntensity;
                }

                if (customStyle.TryGetValue(GlitchJitterProperty, out glitchJitter))
                {
                    settings.Glitch.Jitter = glitchJitter;
                }

                if (customStyle.TryGetValue(GlitchColorShiftProperty, out glitchColorShift))
                {
                    settings.Glitch.ColorShift = glitchColorShift;
                }

                if (customStyle.TryGetValue(GlitchScanlineStrengthProperty, out glitchScanlineStrength))
                {
                    settings.Glitch.ScanlineStrength = glitchScanlineStrength;
                }
            }

            if (customStyle.TryGetValue(ColorizeColorProperty, out var colorizeColor)
                || customStyle.TryGetValue(ColorizeStrengthProperty, out var colorizeStrength))
            {
                settings.Colorize = new ColorizeSettings();

                if (customStyle.TryGetValue(ColorizeColorProperty, out colorizeColor))
                {
                    settings.Colorize.Color = new Color(colorizeColor.r, colorizeColor.g, colorizeColor.b, 1f);
                }

                if (customStyle.TryGetValue(ColorizeStrengthProperty, out colorizeStrength))
                {
                    settings.Colorize.Strength = colorizeStrength;
                }
            }

            return settings;
        }
    }
}

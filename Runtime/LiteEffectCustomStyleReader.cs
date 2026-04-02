using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Acfeel.UIToolkitLiteEffects
{
    internal static class LiteEffectCustomStyleReader
    {
        private static readonly CustomStyleProperty<float> BrightnessProperty = new("--ac-litefx-brightness");
        private static readonly CustomStyleProperty<float> ContrastProperty = new("--ac-litefx-contrast");
        private static readonly CustomStyleProperty<float> SaturationProperty = new("--ac-litefx-saturation");
        private static readonly CustomStyleProperty<Color> GradientFromProperty = new("--ac-litefx-gradient-from");
        private static readonly CustomStyleProperty<Color> GradientToProperty = new("--ac-litefx-gradient-to");
        private static readonly CustomStyleProperty<float> GradientAngleProperty = new("--ac-litefx-gradient-angle");
        private static readonly CustomStyleProperty<string> BlendModeProperty = new("--ac-litefx-blend-mode");
        private static readonly CustomStyleProperty<float> BlendStrengthProperty = new("--ac-litefx-blend-strength");

        public static LiteEffectSettings Read(ICustomStyle customStyle)
        {
            var settings = new LiteEffectSettings();

            if (customStyle.TryGetValue(BrightnessProperty, out var brightness)
                || customStyle.TryGetValue(ContrastProperty, out var contrast)
                || customStyle.TryGetValue(SaturationProperty, out var saturation))
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
            }

            if (customStyle.TryGetValue(GradientFromProperty, out var from)
                || customStyle.TryGetValue(GradientToProperty, out var to)
                || customStyle.TryGetValue(GradientAngleProperty, out var angle))
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
            }

            if (customStyle.TryGetValue(BlendModeProperty, out var modeText)
                || customStyle.TryGetValue(BlendStrengthProperty, out var blendStrength))
            {
                settings.Blend = new BlendSettings();

                if (customStyle.TryGetValue(BlendModeProperty, out modeText)
                    && Enum.TryParse<LiteEffectBlendMode>(modeText, true, out var mode))
                {
                    settings.Blend.Mode = mode;
                }

                if (customStyle.TryGetValue(BlendStrengthProperty, out blendStrength))
                {
                    settings.Blend.Strength = blendStrength;
                }
            }

            return settings;
        }
    }
}

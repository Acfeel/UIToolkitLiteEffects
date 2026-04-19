using UnityEngine;

namespace Acfeel.UIToolkitLiteEffects
{
    internal static class LiteEffectTweenSettingsUtility
    {
        public static LiteEffectSettings Clone(LiteEffectSettings settings)
        {
            if (settings == null)
            {
                return new LiteEffectSettings();
            }

            return new LiteEffectSettings
            {
                ColorAdjust = Clone(settings.ColorAdjust),
                Gradient = Clone(settings.Gradient),
                Outline = Clone(settings.Outline),
                Glow = Clone(settings.Glow),
                Blur = Clone(settings.Blur),
                Dissolve = Clone(settings.Dissolve),
                Glitch = Clone(settings.Glitch),
                Colorize = Clone(settings.Colorize)
            };
        }

        public static ColorAdjustSettings Clone(ColorAdjustSettings settings)
        {
            if (settings == null)
            {
                return null;
            }

            return new ColorAdjustSettings
            {
                Enabled = settings.Enabled,
                Brightness = settings.Brightness,
                Contrast = settings.Contrast,
                Saturation = settings.Saturation,
                Hue = settings.Hue,
                Multiply = settings.Multiply,
                Add = settings.Add
            };
        }

        public static GradientSettings Clone(GradientSettings settings)
        {
            if (settings == null)
            {
                return null;
            }

            return new GradientSettings
            {
                Enabled = settings.Enabled,
                From = settings.From,
                To = settings.To,
                Angle = settings.Angle,
                Mode = settings.Mode,
                Strength = settings.Strength
            };
        }

        public static OutlineSettings Clone(OutlineSettings settings)
        {
            if (settings == null)
            {
                return null;
            }

            return new OutlineSettings
            {
                Enabled = settings.Enabled,
                Color = settings.Color,
                Thickness = settings.Thickness,
                Opacity = settings.Opacity,
                Quality = settings.Quality
            };
        }

        public static GlowSettings Clone(GlowSettings settings)
        {
            if (settings == null)
            {
                return null;
            }

            return new GlowSettings
            {
                Enabled = settings.Enabled,
                Color = settings.Color,
                Strength = settings.Strength,
                Spread = settings.Spread
            };
        }

        public static BlurSettings Clone(BlurSettings settings)
        {
            if (settings == null)
            {
                return null;
            }

            return new BlurSettings
            {
                Enabled = settings.Enabled,
                Radius = settings.Radius,
                Strength = settings.Strength
            };
        }

        public static DissolveSettings Clone(DissolveSettings settings)
        {
            if (settings == null)
            {
                return null;
            }

            return new DissolveSettings
            {
                Enabled = settings.Enabled,
                Amount = settings.Amount,
                EdgeWidth = settings.EdgeWidth,
                EdgeColor = settings.EdgeColor
            };
        }

        public static GlitchSettings Clone(GlitchSettings settings)
        {
            if (settings == null)
            {
                return null;
            }

            return new GlitchSettings
            {
                Enabled = settings.Enabled,
                Intensity = settings.Intensity,
                Jitter = settings.Jitter,
                ColorShift = settings.ColorShift,
                ScanlineStrength = settings.ScanlineStrength
            };
        }

        public static ColorizeSettings Clone(ColorizeSettings settings)
        {
            if (settings == null)
            {
                return null;
            }

            return new ColorizeSettings
            {
                Enabled = settings.Enabled,
                Color = settings.Color,
                Strength = settings.Strength
            };
        }

        public static LiteEffectSettings FromResolved(ResolvedLiteEffectSettings resolved)
        {
            return new LiteEffectSettings
            {
                ColorAdjust = new ColorAdjustSettings
                {
                    Enabled = resolved.ColorAdjust.Enabled,
                    Brightness = resolved.ColorAdjust.Brightness,
                    Contrast = resolved.ColorAdjust.Contrast,
                    Saturation = resolved.ColorAdjust.Saturation,
                    Hue = resolved.ColorAdjust.Hue,
                    Multiply = resolved.ColorAdjust.Multiply,
                    Add = resolved.ColorAdjust.Add
                },
                Gradient = new GradientSettings
                {
                    Enabled = resolved.Gradient.Enabled,
                    From = resolved.Gradient.From,
                    To = resolved.Gradient.To,
                    Angle = resolved.Gradient.Angle,
                    Mode = resolved.Gradient.Mode,
                    Strength = resolved.Gradient.Strength
                },
                Outline = new OutlineSettings
                {
                    Enabled = resolved.Outline.Enabled,
                    Color = resolved.Outline.Color,
                    Thickness = resolved.Outline.Thickness,
                    Opacity = resolved.Outline.Opacity,
                    Quality = resolved.Outline.Quality
                },
                Glow = new GlowSettings
                {
                    Enabled = resolved.Glow.Enabled,
                    Color = resolved.Glow.Color,
                    Strength = resolved.Glow.Strength,
                    Spread = resolved.Glow.Spread
                },
                Blur = new BlurSettings
                {
                    Enabled = resolved.Blur.Enabled,
                    Radius = resolved.Blur.Radius,
                    Strength = resolved.Blur.Strength
                },
                Dissolve = new DissolveSettings
                {
                    Enabled = resolved.Dissolve.Enabled,
                    Amount = resolved.Dissolve.Amount,
                    EdgeWidth = resolved.Dissolve.EdgeWidth,
                    EdgeColor = resolved.Dissolve.EdgeColor
                },
                Glitch = new GlitchSettings
                {
                    Enabled = resolved.Glitch.Enabled,
                    Intensity = resolved.Glitch.Intensity,
                    Jitter = resolved.Glitch.Jitter,
                    ColorShift = resolved.Glitch.ColorShift,
                    ScanlineStrength = resolved.Glitch.ScanlineStrength
                },
                Colorize = new ColorizeSettings
                {
                    Enabled = resolved.Colorize.Enabled,
                    Color = resolved.Colorize.Color,
                    Strength = resolved.Colorize.Strength
                }
            };
        }

        public static LiteEffectSettings Merge(LiteEffectSettings baseSettings, LiteEffectSettings overlay)
        {
            var merged = Clone(baseSettings);
            ApplyColorAdjust(merged, overlay?.ColorAdjust);
            ApplyGradient(merged, overlay?.Gradient);
            ApplyOutline(merged, overlay?.Outline);
            ApplyGlow(merged, overlay?.Glow);
            ApplyBlur(merged, overlay?.Blur);
            ApplyDissolve(merged, overlay?.Dissolve);
            ApplyGlitch(merged, overlay?.Glitch);
            ApplyColorize(merged, overlay?.Colorize);
            return merged;
        }

        public static LiteEffectSettings ExtractMasked(LiteEffectSettings source, LiteEffectSettings mask)
        {
            return new LiteEffectSettings
            {
                ColorAdjust = ExtractMasked(source?.ColorAdjust, mask?.ColorAdjust),
                Gradient = ExtractMasked(source?.Gradient, mask?.Gradient),
                Outline = ExtractMasked(source?.Outline, mask?.Outline),
                Glow = ExtractMasked(source?.Glow, mask?.Glow),
                Blur = ExtractMasked(source?.Blur, mask?.Blur),
                Dissolve = ExtractMasked(source?.Dissolve, mask?.Dissolve),
                Glitch = ExtractMasked(source?.Glitch, mask?.Glitch),
                Colorize = ExtractMasked(source?.Colorize, mask?.Colorize)
            };
        }

        public static LiteEffectSettings LerpPartial(LiteEffectSettings from, LiteEffectSettings to, float t)
        {
            return new LiteEffectSettings
            {
                ColorAdjust = LerpPartial(from?.ColorAdjust, to?.ColorAdjust, t),
                Gradient = LerpPartial(from?.Gradient, to?.Gradient, t),
                Outline = LerpPartial(from?.Outline, to?.Outline, t),
                Glow = LerpPartial(from?.Glow, to?.Glow, t),
                Blur = LerpPartial(from?.Blur, to?.Blur, t),
                Dissolve = LerpPartial(from?.Dissolve, to?.Dissolve, t),
                Glitch = LerpPartial(from?.Glitch, to?.Glitch, t),
                Colorize = LerpPartial(from?.Colorize, to?.Colorize, t)
            };
        }

        public static bool HasAnyAssignedField(LiteEffectSettings settings)
        {
            return HasAnyAssignedField(settings?.ColorAdjust)
                || HasAnyAssignedField(settings?.Gradient)
                || HasAnyAssignedField(settings?.Outline)
                || HasAnyAssignedField(settings?.Glow)
                || HasAnyAssignedField(settings?.Blur)
                || HasAnyAssignedField(settings?.Dissolve)
                || HasAnyAssignedField(settings?.Glitch)
                || HasAnyAssignedField(settings?.Colorize);
        }

        private static void ApplyColorize(LiteEffectSettings destination, ColorizeSettings overlay)
        {
            if (overlay == null)
            {
                return;
            }

            destination.Colorize ??= new ColorizeSettings();

            if (overlay.Enabled.HasValue)
            {
                destination.Colorize.Enabled = overlay.Enabled;
            }

            if (overlay.Color.HasValue)
            {
                destination.Colorize.Color = overlay.Color;
            }

            if (overlay.Strength.HasValue)
            {
                destination.Colorize.Strength = overlay.Strength;
            }
        }

        private static void ApplyColorAdjust(LiteEffectSettings destination, ColorAdjustSettings overlay)
        {
            if (overlay == null)
            {
                return;
            }

            destination.ColorAdjust ??= new ColorAdjustSettings();

            if (overlay.Enabled.HasValue)
            {
                destination.ColorAdjust.Enabled = overlay.Enabled;
            }

            if (overlay.Brightness.HasValue)
            {
                destination.ColorAdjust.Brightness = overlay.Brightness;
            }

            if (overlay.Contrast.HasValue)
            {
                destination.ColorAdjust.Contrast = overlay.Contrast;
            }

            if (overlay.Saturation.HasValue)
            {
                destination.ColorAdjust.Saturation = overlay.Saturation;
            }

            if (overlay.Hue.HasValue)
            {
                destination.ColorAdjust.Hue = overlay.Hue;
            }

            if (overlay.Multiply.HasValue)
            {
                destination.ColorAdjust.Multiply = overlay.Multiply;
            }

            if (overlay.Add.HasValue)
            {
                destination.ColorAdjust.Add = overlay.Add;
            }
        }

        private static void ApplyGradient(LiteEffectSettings destination, GradientSettings overlay)
        {
            if (overlay == null)
            {
                return;
            }

            destination.Gradient ??= new GradientSettings();

            if (overlay.Enabled.HasValue)
            {
                destination.Gradient.Enabled = overlay.Enabled;
            }

            if (overlay.From.HasValue)
            {
                destination.Gradient.From = overlay.From;
            }

            if (overlay.To.HasValue)
            {
                destination.Gradient.To = overlay.To;
            }

            if (overlay.Angle.HasValue)
            {
                destination.Gradient.Angle = overlay.Angle;
            }

            if (overlay.Mode.HasValue)
            {
                destination.Gradient.Mode = overlay.Mode;
            }

            if (overlay.Strength.HasValue)
            {
                destination.Gradient.Strength = overlay.Strength;
            }
        }

        private static void ApplyOutline(LiteEffectSettings destination, OutlineSettings overlay)
        {
            if (overlay == null)
            {
                return;
            }

            destination.Outline ??= new OutlineSettings();

            if (overlay.Enabled.HasValue)
            {
                destination.Outline.Enabled = overlay.Enabled;
            }

            if (overlay.Color.HasValue)
            {
                destination.Outline.Color = overlay.Color;
            }

            if (overlay.Thickness.HasValue)
            {
                destination.Outline.Thickness = overlay.Thickness;
            }

            if (overlay.Opacity.HasValue)
            {
                destination.Outline.Opacity = overlay.Opacity;
            }

            if (overlay.Quality.HasValue)
            {
                destination.Outline.Quality = overlay.Quality;
            }
        }

        private static void ApplyGlow(LiteEffectSettings destination, GlowSettings overlay)
        {
            if (overlay == null)
            {
                return;
            }

            destination.Glow ??= new GlowSettings();

            if (overlay.Enabled.HasValue)
            {
                destination.Glow.Enabled = overlay.Enabled;
            }

            if (overlay.Color.HasValue)
            {
                destination.Glow.Color = overlay.Color;
            }

            if (overlay.Strength.HasValue)
            {
                destination.Glow.Strength = overlay.Strength;
            }

            if (overlay.Spread.HasValue)
            {
                destination.Glow.Spread = overlay.Spread;
            }
        }

        private static void ApplyBlur(LiteEffectSettings destination, BlurSettings overlay)
        {
            if (overlay == null)
            {
                return;
            }

            destination.Blur ??= new BlurSettings();

            if (overlay.Enabled.HasValue)
            {
                destination.Blur.Enabled = overlay.Enabled;
            }

            if (overlay.Radius.HasValue)
            {
                destination.Blur.Radius = overlay.Radius;
            }

            if (overlay.Strength.HasValue)
            {
                destination.Blur.Strength = overlay.Strength;
            }
        }

        private static void ApplyDissolve(LiteEffectSettings destination, DissolveSettings overlay)
        {
            if (overlay == null)
            {
                return;
            }

            destination.Dissolve ??= new DissolveSettings();

            if (overlay.Enabled.HasValue)
            {
                destination.Dissolve.Enabled = overlay.Enabled;
            }

            if (overlay.Amount.HasValue)
            {
                destination.Dissolve.Amount = overlay.Amount;
            }

            if (overlay.EdgeWidth.HasValue)
            {
                destination.Dissolve.EdgeWidth = overlay.EdgeWidth;
            }

            if (overlay.EdgeColor.HasValue)
            {
                destination.Dissolve.EdgeColor = overlay.EdgeColor;
            }
        }

        private static void ApplyGlitch(LiteEffectSettings destination, GlitchSettings overlay)
        {
            if (overlay == null)
            {
                return;
            }

            destination.Glitch ??= new GlitchSettings();

            if (overlay.Enabled.HasValue)
            {
                destination.Glitch.Enabled = overlay.Enabled;
            }

            if (overlay.Intensity.HasValue)
            {
                destination.Glitch.Intensity = overlay.Intensity;
            }

            if (overlay.Jitter.HasValue)
            {
                destination.Glitch.Jitter = overlay.Jitter;
            }

            if (overlay.ColorShift.HasValue)
            {
                destination.Glitch.ColorShift = overlay.ColorShift;
            }

            if (overlay.ScanlineStrength.HasValue)
            {
                destination.Glitch.ScanlineStrength = overlay.ScanlineStrength;
            }
        }

        private static ColorAdjustSettings ExtractMasked(ColorAdjustSettings source, ColorAdjustSettings mask)
        {
            if (mask == null)
            {
                return null;
            }

            return new ColorAdjustSettings
            {
                Enabled = mask.Enabled.HasValue ? source?.Enabled : null,
                Brightness = mask.Brightness.HasValue ? source?.Brightness : null,
                Contrast = mask.Contrast.HasValue ? source?.Contrast : null,
                Saturation = mask.Saturation.HasValue ? source?.Saturation : null,
                Hue = mask.Hue.HasValue ? source?.Hue : null,
                Multiply = mask.Multiply.HasValue ? source?.Multiply : null,
                Add = mask.Add.HasValue ? source?.Add : null
            };
        }

        private static GradientSettings ExtractMasked(GradientSettings source, GradientSettings mask)
        {
            if (mask == null)
            {
                return null;
            }

            return new GradientSettings
            {
                Enabled = mask.Enabled.HasValue ? source?.Enabled : null,
                From = mask.From.HasValue ? source?.From : null,
                To = mask.To.HasValue ? source?.To : null,
                Angle = mask.Angle.HasValue ? source?.Angle : null,
                Mode = mask.Mode.HasValue ? source?.Mode : null,
                Strength = mask.Strength.HasValue ? source?.Strength : null
            };
        }

        private static OutlineSettings ExtractMasked(OutlineSettings source, OutlineSettings mask)
        {
            if (mask == null)
            {
                return null;
            }

            return new OutlineSettings
            {
                Enabled = mask.Enabled.HasValue ? source?.Enabled : null,
                Color = mask.Color.HasValue ? source?.Color : null,
                Thickness = mask.Thickness.HasValue ? source?.Thickness : null,
                Opacity = mask.Opacity.HasValue ? source?.Opacity : null,
                Quality = mask.Quality.HasValue ? source?.Quality : null
            };
        }

        private static GlowSettings ExtractMasked(GlowSettings source, GlowSettings mask)
        {
            if (mask == null)
            {
                return null;
            }

            return new GlowSettings
            {
                Enabled = mask.Enabled.HasValue ? source?.Enabled : null,
                Color = mask.Color.HasValue ? source?.Color : null,
                Strength = mask.Strength.HasValue ? source?.Strength : null,
                Spread = mask.Spread.HasValue ? source?.Spread : null
            };
        }

        private static BlurSettings ExtractMasked(BlurSettings source, BlurSettings mask)
        {
            if (mask == null)
            {
                return null;
            }

            return new BlurSettings
            {
                Enabled = mask.Enabled.HasValue ? source?.Enabled : null,
                Radius = mask.Radius.HasValue ? source?.Radius : null,
                Strength = mask.Strength.HasValue ? source?.Strength : null
            };
        }

        private static DissolveSettings ExtractMasked(DissolveSettings source, DissolveSettings mask)
        {
            if (mask == null)
            {
                return null;
            }

            return new DissolveSettings
            {
                Enabled = mask.Enabled.HasValue ? source?.Enabled : null,
                Amount = mask.Amount.HasValue ? source?.Amount : null,
                EdgeWidth = mask.EdgeWidth.HasValue ? source?.EdgeWidth : null,
                EdgeColor = mask.EdgeColor.HasValue ? source?.EdgeColor : null
            };
        }

        private static GlitchSettings ExtractMasked(GlitchSettings source, GlitchSettings mask)
        {
            if (mask == null)
            {
                return null;
            }

            return new GlitchSettings
            {
                Enabled = mask.Enabled.HasValue ? source?.Enabled : null,
                Intensity = mask.Intensity.HasValue ? source?.Intensity : null,
                Jitter = mask.Jitter.HasValue ? source?.Jitter : null,
                ColorShift = mask.ColorShift.HasValue ? source?.ColorShift : null,
                ScanlineStrength = mask.ScanlineStrength.HasValue ? source?.ScanlineStrength : null
            };
        }

        private static ColorizeSettings ExtractMasked(ColorizeSettings source, ColorizeSettings mask)
        {
            if (mask == null)
            {
                return null;
            }

            return new ColorizeSettings
            {
                Enabled = mask.Enabled.HasValue ? source?.Enabled : null,
                Color = mask.Color.HasValue ? source?.Color : null,
                Strength = mask.Strength.HasValue ? source?.Strength : null
            };
        }

        private static ColorAdjustSettings LerpPartial(ColorAdjustSettings from, ColorAdjustSettings to, float t)
        {
            if (from == null && to == null)
            {
                return null;
            }

            return new ColorAdjustSettings
            {
                Enabled = LerpBool(from?.Enabled, to?.Enabled, t),
                Brightness = LerpFloat(from?.Brightness, to?.Brightness, t),
                Contrast = LerpFloat(from?.Contrast, to?.Contrast, t),
                Saturation = LerpFloat(from?.Saturation, to?.Saturation, t),
                Hue = LerpFloat(from?.Hue, to?.Hue, t),
                Multiply = LerpColor(from?.Multiply, to?.Multiply, t),
                Add = LerpColor(from?.Add, to?.Add, t)
            };
        }

        private static GradientSettings LerpPartial(GradientSettings from, GradientSettings to, float t)
        {
            if (from == null && to == null)
            {
                return null;
            }

            return new GradientSettings
            {
                Enabled = LerpBool(from?.Enabled, to?.Enabled, t),
                From = LerpColor(from?.From, to?.From, t),
                To = LerpColor(from?.To, to?.To, t),
                Angle = LerpFloat(from?.Angle, to?.Angle, t),
                Mode = LerpEnum(from?.Mode, to?.Mode, t),
                Strength = LerpFloat(from?.Strength, to?.Strength, t)
            };
        }

        private static OutlineSettings LerpPartial(OutlineSettings from, OutlineSettings to, float t)
        {
            if (from == null && to == null)
            {
                return null;
            }

            return new OutlineSettings
            {
                Enabled = LerpBool(from?.Enabled, to?.Enabled, t),
                Color = LerpColor(from?.Color, to?.Color, t),
                Thickness = LerpFloat(from?.Thickness, to?.Thickness, t),
                Opacity = LerpFloat(from?.Opacity, to?.Opacity, t),
                Quality = LerpEnum(from?.Quality, to?.Quality, t)
            };
        }

        private static GlowSettings LerpPartial(GlowSettings from, GlowSettings to, float t)
        {
            if (from == null && to == null)
            {
                return null;
            }

            return new GlowSettings
            {
                Enabled = LerpBool(from?.Enabled, to?.Enabled, t),
                Color = LerpColor(from?.Color, to?.Color, t),
                Strength = LerpFloat(from?.Strength, to?.Strength, t),
                Spread = LerpFloat(from?.Spread, to?.Spread, t)
            };
        }

        private static BlurSettings LerpPartial(BlurSettings from, BlurSettings to, float t)
        {
            if (from == null && to == null)
            {
                return null;
            }

            return new BlurSettings
            {
                Enabled = LerpBool(from?.Enabled, to?.Enabled, t),
                Radius = LerpFloat(from?.Radius, to?.Radius, t),
                Strength = LerpFloat(from?.Strength, to?.Strength, t)
            };
        }

        private static DissolveSettings LerpPartial(DissolveSettings from, DissolveSettings to, float t)
        {
            if (from == null && to == null)
            {
                return null;
            }

            return new DissolveSettings
            {
                Enabled = LerpBool(from?.Enabled, to?.Enabled, t),
                Amount = LerpFloat(from?.Amount, to?.Amount, t),
                EdgeWidth = LerpFloat(from?.EdgeWidth, to?.EdgeWidth, t),
                EdgeColor = LerpColor(from?.EdgeColor, to?.EdgeColor, t)
            };
        }

        private static GlitchSettings LerpPartial(GlitchSettings from, GlitchSettings to, float t)
        {
            if (from == null && to == null)
            {
                return null;
            }

            return new GlitchSettings
            {
                Enabled = LerpBool(from?.Enabled, to?.Enabled, t),
                Intensity = LerpFloat(from?.Intensity, to?.Intensity, t),
                Jitter = LerpFloat(from?.Jitter, to?.Jitter, t),
                ColorShift = LerpFloat(from?.ColorShift, to?.ColorShift, t),
                ScanlineStrength = LerpFloat(from?.ScanlineStrength, to?.ScanlineStrength, t)
            };
        }

        private static ColorizeSettings LerpPartial(ColorizeSettings from, ColorizeSettings to, float t)
        {
            if (from == null && to == null)
            {
                return null;
            }

            return new ColorizeSettings
            {
                Enabled = LerpBool(from?.Enabled, to?.Enabled, t),
                Color = LerpColor(from?.Color, to?.Color, t),
                Strength = LerpFloat(from?.Strength, to?.Strength, t)
            };
        }

        private static bool HasAnyAssignedField(ColorAdjustSettings settings)
        {
            return settings != null
                && (settings.Enabled.HasValue
                || settings.Brightness.HasValue
                || settings.Contrast.HasValue
                || settings.Saturation.HasValue
                || settings.Hue.HasValue
                || settings.Multiply.HasValue
                || settings.Add.HasValue);
        }

        private static bool HasAnyAssignedField(GradientSettings settings)
        {
            return settings != null
                && (settings.Enabled.HasValue
                || settings.From.HasValue
                || settings.To.HasValue
                || settings.Angle.HasValue
                || settings.Mode.HasValue
                || settings.Strength.HasValue);
        }

        private static bool HasAnyAssignedField(OutlineSettings settings)
        {
            return settings != null
                && (settings.Enabled.HasValue
                || settings.Color.HasValue
                || settings.Thickness.HasValue
                || settings.Opacity.HasValue
                || settings.Quality.HasValue);
        }

        private static bool HasAnyAssignedField(GlowSettings settings)
        {
            return settings != null
                && (settings.Enabled.HasValue
                || settings.Color.HasValue
                || settings.Strength.HasValue
                || settings.Spread.HasValue);
        }

        private static bool HasAnyAssignedField(BlurSettings settings)
        {
            return settings != null
                && (settings.Enabled.HasValue
                || settings.Radius.HasValue
                || settings.Strength.HasValue);
        }

        private static bool HasAnyAssignedField(DissolveSettings settings)
        {
            return settings != null
                && (settings.Enabled.HasValue
                || settings.Amount.HasValue
                || settings.EdgeWidth.HasValue
                || settings.EdgeColor.HasValue);
        }

        private static bool HasAnyAssignedField(GlitchSettings settings)
        {
            return settings != null
                && (settings.Enabled.HasValue
                || settings.Intensity.HasValue
                || settings.Jitter.HasValue
                || settings.ColorShift.HasValue
                || settings.ScanlineStrength.HasValue);
        }

        private static bool HasAnyAssignedField(ColorizeSettings settings)
        {
            return settings != null
                && (settings.Enabled.HasValue
                || settings.Color.HasValue
                || settings.Strength.HasValue);
        }

        private static float? LerpFloat(float? from, float? to, float t)
        {
            if (!from.HasValue || !to.HasValue)
            {
                return null;
            }

            return Mathf.LerpUnclamped(from.Value, to.Value, t);
        }

        private static Color? LerpColor(Color? from, Color? to, float t)
        {
            if (!from.HasValue || !to.HasValue)
            {
                return null;
            }

            return Color.LerpUnclamped(from.Value, to.Value, t);
        }

        private static bool? LerpBool(bool? from, bool? to, float t)
        {
            if (!from.HasValue || !to.HasValue)
            {
                return null;
            }

            return t >= 1f ? to.Value : from.Value;
        }

        private static LiteEffectBlendMode? LerpEnum(LiteEffectBlendMode? from, LiteEffectBlendMode? to, float t)
        {
            if (!from.HasValue || !to.HasValue)
            {
                return null;
            }

            return t >= 1f ? to.Value : from.Value;
        }

        private static LiteEffectOutlineQuality? LerpEnum(LiteEffectOutlineQuality? from, LiteEffectOutlineQuality? to, float t)
        {
            if (!from.HasValue || !to.HasValue)
            {
                return null;
            }

            return t >= 1f ? to.Value : from.Value;
        }
    }
}

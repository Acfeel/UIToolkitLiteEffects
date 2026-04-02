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
                Blend = Clone(settings.Blend)
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
                Mode = settings.Mode
            };
        }

        public static BlendSettings Clone(BlendSettings settings)
        {
            if (settings == null)
            {
                return null;
            }

            return new BlendSettings
            {
                Enabled = settings.Enabled,
                Mode = settings.Mode,
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
                    Multiply = resolved.ColorAdjust.Multiply,
                    Add = resolved.ColorAdjust.Add
                },
                Gradient = new GradientSettings
                {
                    Enabled = resolved.Gradient.Enabled,
                    From = resolved.Gradient.From,
                    To = resolved.Gradient.To,
                    Angle = resolved.Gradient.Angle,
                    Mode = resolved.Gradient.Mode
                },
                Blend = new BlendSettings
                {
                    Enabled = resolved.Blend.Enabled,
                    Mode = resolved.Blend.Mode,
                    Strength = resolved.Blend.Strength
                }
            };
        }

        public static LiteEffectSettings Merge(LiteEffectSettings baseSettings, LiteEffectSettings overlay)
        {
            var merged = Clone(baseSettings);
            ApplyColorAdjust(merged, overlay?.ColorAdjust);
            ApplyGradient(merged, overlay?.Gradient);
            ApplyBlend(merged, overlay?.Blend);
            return merged;
        }

        public static LiteEffectSettings ExtractMasked(LiteEffectSettings source, LiteEffectSettings mask)
        {
            return new LiteEffectSettings
            {
                ColorAdjust = ExtractMasked(source?.ColorAdjust, mask?.ColorAdjust),
                Gradient = ExtractMasked(source?.Gradient, mask?.Gradient),
                Blend = ExtractMasked(source?.Blend, mask?.Blend)
            };
        }

        public static LiteEffectSettings LerpPartial(LiteEffectSettings from, LiteEffectSettings to, float t)
        {
            return new LiteEffectSettings
            {
                ColorAdjust = LerpPartial(from?.ColorAdjust, to?.ColorAdjust, t),
                Gradient = LerpPartial(from?.Gradient, to?.Gradient, t),
                Blend = LerpPartial(from?.Blend, to?.Blend, t)
            };
        }

        public static bool HasAnyAssignedField(LiteEffectSettings settings)
        {
            return HasAnyAssignedField(settings?.ColorAdjust)
                || HasAnyAssignedField(settings?.Gradient)
                || HasAnyAssignedField(settings?.Blend);
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
        }

        private static void ApplyBlend(LiteEffectSettings destination, BlendSettings overlay)
        {
            if (overlay == null)
            {
                return;
            }

            destination.Blend ??= new BlendSettings();

            if (overlay.Enabled.HasValue)
            {
                destination.Blend.Enabled = overlay.Enabled;
            }

            if (overlay.Mode.HasValue)
            {
                destination.Blend.Mode = overlay.Mode;
            }

            if (overlay.Strength.HasValue)
            {
                destination.Blend.Strength = overlay.Strength;
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
                Mode = mask.Mode.HasValue ? source?.Mode : null
            };
        }

        private static BlendSettings ExtractMasked(BlendSettings source, BlendSettings mask)
        {
            if (mask == null)
            {
                return null;
            }

            return new BlendSettings
            {
                Enabled = mask.Enabled.HasValue ? source?.Enabled : null,
                Mode = mask.Mode.HasValue ? source?.Mode : null,
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
                Mode = LerpEnum(from?.Mode, to?.Mode, t)
            };
        }

        private static BlendSettings LerpPartial(BlendSettings from, BlendSettings to, float t)
        {
            if (from == null && to == null)
            {
                return null;
            }

            return new BlendSettings
            {
                Enabled = LerpBool(from?.Enabled, to?.Enabled, t),
                Mode = LerpEnum(from?.Mode, to?.Mode, t),
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
                || settings.Mode.HasValue);
        }

        private static bool HasAnyAssignedField(BlendSettings settings)
        {
            return settings != null
                && (settings.Enabled.HasValue
                || settings.Mode.HasValue
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
    }
}

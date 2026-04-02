using System;
using UnityEngine;

namespace Acfeel.UIToolkitLiteEffects
{
    [Serializable]
    public sealed class LiteEffectSettings
    {
        public ColorAdjustSettings ColorAdjust;
        public GradientSettings Gradient;
        public BlendSettings Blend;
    }

    [Serializable]
    public sealed class ColorAdjustSettings
    {
        public bool? Enabled;
        public float? Brightness;
        public float? Contrast;
        public float? Saturation;
        public Color? Multiply;
        public Color? Add;
    }

    [Serializable]
    public sealed class GradientSettings
    {
        public bool? Enabled;
        public Color? From;
        public Color? To;
        public float? Angle;
        public LiteEffectBlendMode? Mode;
    }

    [Serializable]
    public sealed class BlendSettings
    {
        public bool? Enabled;
        public LiteEffectBlendMode? Mode;
        public float? Strength;
    }

    public enum LiteEffectBlendMode
    {
        Mix = 0,
        Multiply = 1,
        Additive = 2
    }

    internal readonly struct ResolvedLiteEffectSettings
    {
        public ResolvedLiteEffectSettings(
            ResolvedColorAdjustSettings colorAdjust,
            ResolvedGradientSettings gradient,
            ResolvedBlendSettings blend)
        {
            ColorAdjust = colorAdjust;
            Gradient = gradient;
            Blend = blend;
        }

        public ResolvedColorAdjustSettings ColorAdjust { get; }

        public ResolvedGradientSettings Gradient { get; }

        public ResolvedBlendSettings Blend { get; }

        public bool HasAnyEffect => ColorAdjust.Enabled || Gradient.Enabled || Blend.Enabled;
    }

    internal readonly struct ResolvedColorAdjustSettings
    {
        public ResolvedColorAdjustSettings(
            bool enabled,
            float brightness,
            float contrast,
            float saturation,
            Color multiply,
            Color add)
        {
            Enabled = enabled;
            Brightness = brightness;
            Contrast = contrast;
            Saturation = saturation;
            Multiply = multiply;
            Add = add;
        }

        public bool Enabled { get; }

        public float Brightness { get; }

        public float Contrast { get; }

        public float Saturation { get; }

        public Color Multiply { get; }

        public Color Add { get; }
    }

    internal readonly struct ResolvedGradientSettings
    {
        public ResolvedGradientSettings(
            bool enabled,
            Color from,
            Color to,
            float angle,
            LiteEffectBlendMode mode)
        {
            Enabled = enabled;
            From = from;
            To = to;
            Angle = angle;
            Mode = mode;
        }

        public bool Enabled { get; }

        public Color From { get; }

        public Color To { get; }

        public float Angle { get; }

        public LiteEffectBlendMode Mode { get; }
    }

    internal readonly struct ResolvedBlendSettings
    {
        public ResolvedBlendSettings(bool enabled, LiteEffectBlendMode mode, float strength)
        {
            Enabled = enabled;
            Mode = mode;
            Strength = strength;
        }

        public bool Enabled { get; }

        public LiteEffectBlendMode Mode { get; }

        public float Strength { get; }
    }

    internal static class LiteEffectSettingsResolver
    {
        public static ResolvedLiteEffectSettings Resolve(LiteEffectSettings explicitSettings, LiteEffectSettings ussSettings)
        {
            var colorAdjust = ResolveColorAdjust(explicitSettings?.ColorAdjust, ussSettings?.ColorAdjust);
            var gradient = ResolveGradient(explicitSettings?.Gradient, ussSettings?.Gradient);
            var blend = ResolveBlend(explicitSettings?.Blend, ussSettings?.Blend);
            return new ResolvedLiteEffectSettings(colorAdjust, gradient, blend);
        }

        private static ResolvedColorAdjustSettings ResolveColorAdjust(ColorAdjustSettings explicitSettings, ColorAdjustSettings ussSettings)
        {
            var brightness = Mathf.Clamp01(explicitSettings?.Brightness ?? ussSettings?.Brightness ?? 0.5f);
            var contrast = Mathf.Clamp01(explicitSettings?.Contrast ?? ussSettings?.Contrast ?? 0.5f);
            var saturation = Mathf.Clamp01(explicitSettings?.Saturation ?? ussSettings?.Saturation ?? 0.5f);
            var multiply = explicitSettings?.Multiply ?? ussSettings?.Multiply ?? Color.white;
            var add = explicitSettings?.Add ?? ussSettings?.Add ?? Color.clear;
            var enabled = explicitSettings?.Enabled
                ?? ussSettings?.Enabled
                ?? (!Mathf.Approximately(brightness, 0.5f)
                || !Mathf.Approximately(contrast, 0.5f)
                || !Mathf.Approximately(saturation, 0.5f)
                || multiply != Color.white
                || add != Color.clear);

            return new ResolvedColorAdjustSettings(enabled, brightness, contrast, saturation, multiply, add);
        }

        private static ResolvedGradientSettings ResolveGradient(GradientSettings explicitSettings, GradientSettings ussSettings)
        {
            var from = explicitSettings?.From ?? ussSettings?.From ?? Color.clear;
            var to = explicitSettings?.To ?? ussSettings?.To ?? Color.clear;
            var angle = explicitSettings?.Angle ?? ussSettings?.Angle ?? 0f;
            var mode = explicitSettings?.Mode ?? ussSettings?.Mode ?? LiteEffectBlendMode.Mix;
            var hasAssignedFields = HasAnyAssignedField(explicitSettings) || HasAnyAssignedField(ussSettings);
            var enabled = explicitSettings?.Enabled
                ?? ussSettings?.Enabled
                ?? hasAssignedFields;

            return new ResolvedGradientSettings(enabled, from, to, angle, mode);
        }

        private static ResolvedBlendSettings ResolveBlend(BlendSettings explicitSettings, BlendSettings ussSettings)
        {
            var mode = explicitSettings?.Mode ?? ussSettings?.Mode ?? LiteEffectBlendMode.Mix;
            var strength = Mathf.Clamp01(explicitSettings?.Strength ?? ussSettings?.Strength ?? 1f);
            var hasAssignedFields = HasAnyAssignedField(explicitSettings) || HasAnyAssignedField(ussSettings);
            var enabled = explicitSettings?.Enabled
                ?? ussSettings?.Enabled
                ?? hasAssignedFields;

            return new ResolvedBlendSettings(enabled, mode, strength);
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
    }
}

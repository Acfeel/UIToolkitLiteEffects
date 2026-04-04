using System;
using UnityEngine;

namespace Acfeel.UIToolkitLiteEffects
{
    [Serializable]
    public sealed class LiteEffectSettings
    {
        public ColorAdjustSettings ColorAdjust;
        public GradientSettings Gradient;
        public OutlineSettings Outline;
        public GlowSettings Glow;
        public BlurSettings Blur;
        public DissolveSettings Dissolve;
        public GlitchSettings Glitch;
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
        public float? Strength;
    }

    [Serializable]
    public sealed class OutlineSettings
    {
        public bool? Enabled;
        public Color? Color;
        public float? Thickness;
        public float? Opacity;
        public LiteEffectOutlineQuality? Quality;
    }

    [Serializable]
    public sealed class GlowSettings
    {
        public bool? Enabled;
        public Color? Color;
        public float? Strength;
        public float? Spread;
    }

    [Serializable]
    public sealed class BlurSettings
    {
        public bool? Enabled;
        public float? Radius;
        public float? Strength;
    }

    [Serializable]
    public sealed class DissolveSettings
    {
        public bool? Enabled;
        public float? Amount;
        public float? EdgeWidth;
        public Color? EdgeColor;
    }

    [Serializable]
    public sealed class GlitchSettings
    {
        public bool? Enabled;
        public float? Intensity;
        public float? Jitter;
        public float? ColorShift;
        public float? ScanlineStrength;
    }

    public enum LiteEffectBlendMode
    {
        Mix = 0,
        Multiply = 1,
        Additive = 2
    }

    public enum LiteEffectOutlineQuality
    {
        Low = 0,
        Normal = 1
    }

    internal readonly struct ResolvedLiteEffectSettings
    {
        public ResolvedLiteEffectSettings(
            ResolvedColorAdjustSettings colorAdjust,
            ResolvedGradientSettings gradient,
            ResolvedOutlineSettings outline,
            ResolvedGlowSettings glow,
            ResolvedBlurSettings blur,
            ResolvedDissolveSettings dissolve,
            ResolvedGlitchSettings glitch)
        {
            ColorAdjust = colorAdjust;
            Gradient = gradient;
            Outline = outline;
            Glow = glow;
            Blur = blur;
            Dissolve = dissolve;
            Glitch = glitch;
        }

        public ResolvedColorAdjustSettings ColorAdjust { get; }

        public ResolvedGradientSettings Gradient { get; }

        public ResolvedOutlineSettings Outline { get; }

        public ResolvedGlowSettings Glow { get; }

        public ResolvedBlurSettings Blur { get; }

        public ResolvedDissolveSettings Dissolve { get; }

        public ResolvedGlitchSettings Glitch { get; }

        public bool HasAnyEffect =>
            ColorAdjust.Enabled
            || Gradient.Enabled
            || Outline.Enabled
            || Glow.Enabled
            || Blur.Enabled
            || Dissolve.Enabled
            || Glitch.Enabled;

        public bool RequiresRealtimeRefresh => Glitch.Enabled && Glitch.Intensity > 0.0001f;
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
            LiteEffectBlendMode mode,
            float strength)
        {
            Enabled = enabled;
            From = from;
            To = to;
            Angle = angle;
            Mode = mode;
            Strength = strength;
        }

        public bool Enabled { get; }
        public Color From { get; }
        public Color To { get; }
        public float Angle { get; }
        public LiteEffectBlendMode Mode { get; }
        public float Strength { get; }
    }

    internal readonly struct ResolvedOutlineSettings
    {
        public ResolvedOutlineSettings(bool enabled, Color color, float thickness, float opacity, LiteEffectOutlineQuality quality)
        {
            Enabled = enabled;
            Color = color;
            Thickness = thickness;
            Opacity = opacity;
            Quality = quality;
        }

        public bool Enabled { get; }
        public Color Color { get; }
        public float Thickness { get; }
        public float Opacity { get; }
        public LiteEffectOutlineQuality Quality { get; }
    }

    internal readonly struct ResolvedGlowSettings
    {
        public ResolvedGlowSettings(bool enabled, Color color, float strength, float spread)
        {
            Enabled = enabled;
            Color = color;
            Strength = strength;
            Spread = spread;
        }

        public bool Enabled { get; }
        public Color Color { get; }
        public float Strength { get; }
        public float Spread { get; }
    }

    internal readonly struct ResolvedBlurSettings
    {
        public ResolvedBlurSettings(bool enabled, float radius, float strength)
        {
            Enabled = enabled;
            Radius = radius;
            Strength = strength;
        }

        public bool Enabled { get; }
        public float Radius { get; }
        public float Strength { get; }
    }

    internal readonly struct ResolvedDissolveSettings
    {
        public ResolvedDissolveSettings(bool enabled, float amount, float edgeWidth, Color edgeColor)
        {
            Enabled = enabled;
            Amount = amount;
            EdgeWidth = edgeWidth;
            EdgeColor = edgeColor;
        }

        public bool Enabled { get; }
        public float Amount { get; }
        public float EdgeWidth { get; }
        public Color EdgeColor { get; }
    }

    internal readonly struct ResolvedGlitchSettings
    {
        public ResolvedGlitchSettings(bool enabled, float intensity, float jitter, float colorShift, float scanlineStrength)
        {
            Enabled = enabled;
            Intensity = intensity;
            Jitter = jitter;
            ColorShift = colorShift;
            ScanlineStrength = scanlineStrength;
        }

        public bool Enabled { get; }
        public float Intensity { get; }
        public float Jitter { get; }
        public float ColorShift { get; }
        public float ScanlineStrength { get; }
    }

    internal static class LiteEffectSettingsResolver
    {
        public static ResolvedLiteEffectSettings Resolve(LiteEffectSettings explicitSettings, LiteEffectSettings ussSettings)
        {
            var colorAdjust = ResolveColorAdjust(explicitSettings?.ColorAdjust, ussSettings?.ColorAdjust);
            var gradient = ResolveGradient(explicitSettings?.Gradient, ussSettings?.Gradient);
            var outline = ResolveOutline(explicitSettings?.Outline, ussSettings?.Outline);
            var glow = ResolveGlow(explicitSettings?.Glow, ussSettings?.Glow);
            var blur = ResolveBlur(explicitSettings?.Blur, ussSettings?.Blur);
            var dissolve = ResolveDissolve(explicitSettings?.Dissolve, ussSettings?.Dissolve);
            var glitch = ResolveGlitch(explicitSettings?.Glitch, ussSettings?.Glitch);
            return new ResolvedLiteEffectSettings(colorAdjust, gradient, outline, glow, blur, dissolve, glitch);
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
            var strength = Mathf.Clamp01(explicitSettings?.Strength ?? ussSettings?.Strength ?? 1f);
            var hasAssignedFields = HasAnyAssignedField(explicitSettings) || HasAnyAssignedField(ussSettings);
            var hasGradientColors =
                (explicitSettings?.From).HasValue
                || (explicitSettings?.To).HasValue
                || (ussSettings?.From).HasValue
                || (ussSettings?.To).HasValue;
            var enabled = explicitSettings?.Enabled
                ?? ussSettings?.Enabled
                ?? (hasAssignedFields && hasGradientColors);

            return new ResolvedGradientSettings(enabled, from, to, angle, mode, strength);
        }

        private static ResolvedOutlineSettings ResolveOutline(OutlineSettings explicitSettings, OutlineSettings ussSettings)
        {
            var color = explicitSettings?.Color ?? ussSettings?.Color ?? Color.black;
            var thickness = Mathf.Clamp01(explicitSettings?.Thickness ?? ussSettings?.Thickness ?? 0f);
            var opacity = Mathf.Clamp01(explicitSettings?.Opacity ?? ussSettings?.Opacity ?? 0f);
            var quality = explicitSettings?.Quality ?? ussSettings?.Quality ?? LiteEffectOutlineQuality.Normal;
            var enabled = explicitSettings?.Enabled
                ?? ussSettings?.Enabled
                ?? (thickness > 0.0001f && opacity > 0.0001f);

            return new ResolvedOutlineSettings(enabled, color, thickness, opacity, quality);
        }

        private static ResolvedGlowSettings ResolveGlow(GlowSettings explicitSettings, GlowSettings ussSettings)
        {
            var color = explicitSettings?.Color ?? ussSettings?.Color ?? Color.white;
            var strength = Mathf.Clamp01(explicitSettings?.Strength ?? ussSettings?.Strength ?? 0f);
            var spread = Mathf.Clamp01(explicitSettings?.Spread ?? ussSettings?.Spread ?? 0f);
            var enabled = explicitSettings?.Enabled
                ?? ussSettings?.Enabled
                ?? (strength > 0.0001f && spread > 0.0001f);

            return new ResolvedGlowSettings(enabled, color, strength, spread);
        }

        private static ResolvedBlurSettings ResolveBlur(BlurSettings explicitSettings, BlurSettings ussSettings)
        {
            var radius = Mathf.Clamp01(explicitSettings?.Radius ?? ussSettings?.Radius ?? 0f);
            var strength = Mathf.Clamp01(explicitSettings?.Strength ?? ussSettings?.Strength ?? 0f);
            var enabled = explicitSettings?.Enabled
                ?? ussSettings?.Enabled
                ?? (radius > 0.0001f && strength > 0.0001f);

            return new ResolvedBlurSettings(enabled, radius, strength);
        }

        private static ResolvedDissolveSettings ResolveDissolve(DissolveSettings explicitSettings, DissolveSettings ussSettings)
        {
            var amount = Mathf.Clamp01(explicitSettings?.Amount ?? ussSettings?.Amount ?? 0f);
            var edgeWidth = Mathf.Clamp01(explicitSettings?.EdgeWidth ?? ussSettings?.EdgeWidth ?? 0.08f);
            var edgeColor = explicitSettings?.EdgeColor ?? ussSettings?.EdgeColor ?? Color.clear;
            var enabled = explicitSettings?.Enabled
                ?? ussSettings?.Enabled
                ?? amount > 0.0001f;

            return new ResolvedDissolveSettings(enabled, amount, edgeWidth, edgeColor);
        }

        private static ResolvedGlitchSettings ResolveGlitch(GlitchSettings explicitSettings, GlitchSettings ussSettings)
        {
            var intensity = Mathf.Clamp01(explicitSettings?.Intensity ?? ussSettings?.Intensity ?? 0f);
            var jitter = Mathf.Clamp01(explicitSettings?.Jitter ?? ussSettings?.Jitter ?? 0.5f);
            var colorShift = Mathf.Clamp01(explicitSettings?.ColorShift ?? ussSettings?.ColorShift ?? 0.35f);
            var scanlineStrength = Mathf.Clamp01(explicitSettings?.ScanlineStrength ?? ussSettings?.ScanlineStrength ?? 0.25f);
            var enabled = explicitSettings?.Enabled
                ?? ussSettings?.Enabled
                ?? intensity > 0.0001f;

            return new ResolvedGlitchSettings(enabled, intensity, jitter, colorShift, scanlineStrength);
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
    }
}

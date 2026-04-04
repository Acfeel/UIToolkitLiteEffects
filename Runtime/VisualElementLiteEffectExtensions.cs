using UnityEngine.UIElements;

namespace Acfeel.UIToolkitLiteEffects
{
    public static class VisualElementLiteEffectExtensions
    {
        public static LiteEffectHandle EnableLiteEffectFromUss(this VisualElement element)
        {
            LiteEffectControllerRegistry.GetOrCreate(element).EnableFromUssOnly();
            return new LiteEffectHandle(element);
        }

        public static LiteEffectHandle SetLiteEffect(this VisualElement element, LiteEffectSettings settings)
        {
            var controller = LiteEffectControllerRegistry.GetOrCreate(element);
            controller.Apply(settings);
            return new LiteEffectHandle(element);
        }

        public static LiteEffectHandle SetColorAdjust(this VisualElement element, ColorAdjustSettings settings)
        {
            var controller = LiteEffectControllerRegistry.GetOrCreate(element);
            controller.SetColorAdjust(settings);
            return new LiteEffectHandle(element);
        }

        public static LiteEffectHandle SetGradient(this VisualElement element, GradientSettings settings)
        {
            var controller = LiteEffectControllerRegistry.GetOrCreate(element);
            controller.SetGradient(settings);
            return new LiteEffectHandle(element);
        }

        public static LiteEffectHandle SetOutline(this VisualElement element, OutlineSettings settings)
        {
            var controller = LiteEffectControllerRegistry.GetOrCreate(element);
            controller.SetOutline(settings);
            return new LiteEffectHandle(element);
        }

        public static LiteEffectHandle SetGlow(this VisualElement element, GlowSettings settings)
        {
            var controller = LiteEffectControllerRegistry.GetOrCreate(element);
            controller.SetGlow(settings);
            return new LiteEffectHandle(element);
        }

        public static LiteEffectHandle SetBlur(this VisualElement element, BlurSettings settings)
        {
            var controller = LiteEffectControllerRegistry.GetOrCreate(element);
            controller.SetBlur(settings);
            return new LiteEffectHandle(element);
        }

        public static LiteEffectHandle SetDissolve(this VisualElement element, DissolveSettings settings)
        {
            var controller = LiteEffectControllerRegistry.GetOrCreate(element);
            controller.SetDissolve(settings);
            return new LiteEffectHandle(element);
        }

        public static LiteEffectHandle SetGlitch(this VisualElement element, GlitchSettings settings)
        {
            var controller = LiteEffectControllerRegistry.GetOrCreate(element);
            controller.SetGlitch(settings);
            return new LiteEffectHandle(element);
        }

        public static void ClearLiteEffect(this VisualElement element)
        {
            LiteEffectControllerRegistry.GetOrCreate(element).ClearExplicit();
        }

        public static LiteEffectTween AnimateColorAdjust(this VisualElement element, ColorAdjustSettings to, float duration)
        {
            return CreateTween(element, new LiteEffectSettings
            {
                ColorAdjust = to
            }, duration);
        }

        public static LiteEffectTween AnimateGradient(this VisualElement element, GradientSettings to, float duration)
        {
            return CreateTween(element, new LiteEffectSettings
            {
                Gradient = to
            }, duration);
        }

        public static LiteEffectTween AnimateLiteEffect(this VisualElement element, LiteEffectSettings to, float duration)
        {
            return CreateTween(element, to, duration);
        }

        private static LiteEffectTween CreateTween(this VisualElement element, LiteEffectSettings targetSettings, float duration)
        {
            var sequence = new LiteEffectTweenSequenceDefinition();
            var definition = new LiteEffectTweenDefinition
            {
                TargetSettings = LiteEffectTweenSettingsUtility.Clone(targetSettings),
                Duration = duration < 0f ? 0f : duration
            };
            sequence.Append(definition);
            var playback = new LiteEffectTweenPlayback(element, sequence);
            var tween = new LiteEffectTween(playback, definition);
            playback.ScheduleAutoPlay();
            return tween;
        }
    }
}

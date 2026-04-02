using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Acfeel.UIToolkitLiteEffects
{
    public enum LiteEffectEase
    {
        Linear = 0,
        InSine = 1,
        OutSine = 2,
        InOutSine = 3,
        InQuad = 4,
        OutQuad = 5,
        InOutQuad = 6
    }

    public sealed class LiteEffectTween
    {
        private readonly LiteEffectTweenPlayback playback;
        private readonly LiteEffectTweenDefinition definition;

        internal LiteEffectTween(LiteEffectTweenPlayback playback, LiteEffectTweenDefinition definition)
        {
            this.playback = playback;
            this.definition = definition;
        }

        internal VisualElement Element => playback.Element;

        internal LiteEffectTweenDefinition Definition => definition;

        public LiteEffectTween SetEase(LiteEffectEase ease)
        {
            definition.Ease = ease;
            playback.NotifySequenceChanged();
            return this;
        }

        public LiteEffectTween SetDelay(float seconds)
        {
            definition.Delay = Mathf.Max(0f, seconds);
            playback.NotifySequenceChanged();
            return this;
        }

        public LiteEffectTween OnComplete(Action callback)
        {
            playback.Sequence.OnComplete += callback;
            playback.NotifySequenceChanged();
            return this;
        }

        public LiteEffectSequence Append(LiteEffectTween tween)
        {
            playback.Sequence.Append(CloneTweenDefinition(tween));
            playback.NotifySequenceChanged();
            return new LiteEffectSequence(playback);
        }

        public LiteEffectSequence Join(LiteEffectTween tween)
        {
            playback.Sequence.Join(CloneTweenDefinition(tween));
            playback.NotifySequenceChanged();
            return new LiteEffectSequence(playback);
        }

        public void Kill()
        {
            playback.Kill();
        }

        private LiteEffectTweenDefinition CloneTweenDefinition(LiteEffectTween tween)
        {
            if (tween == null)
            {
                throw new ArgumentNullException(nameof(tween));
            }

            if (tween.Element != playback.Element)
            {
                throw new InvalidOperationException("LiteEffectTween can only append tweens that target the same VisualElement.");
            }

            return tween.Definition.Clone();
        }
    }

    public sealed class LiteEffectSequence
    {
        private readonly LiteEffectTweenPlayback playback;

        internal LiteEffectSequence(LiteEffectTweenPlayback playback)
        {
            this.playback = playback;
        }

        public LiteEffectSequence OnComplete(Action callback)
        {
            playback.Sequence.OnComplete += callback;
            playback.NotifySequenceChanged();
            return this;
        }

        public LiteEffectSequence Append(LiteEffectTween tween)
        {
            playback.Sequence.Append(CloneTweenDefinition(tween));
            playback.NotifySequenceChanged();
            return this;
        }

        public LiteEffectSequence Join(LiteEffectTween tween)
        {
            playback.Sequence.Join(CloneTweenDefinition(tween));
            playback.NotifySequenceChanged();
            return this;
        }

        public void Kill()
        {
            playback.Kill();
        }

        private LiteEffectTweenDefinition CloneTweenDefinition(LiteEffectTween tween)
        {
            if (tween == null)
            {
                throw new ArgumentNullException(nameof(tween));
            }

            if (tween.Element != playback.Element)
            {
                throw new InvalidOperationException("LiteEffectSequence can only append tweens that target the same VisualElement.");
            }

            return tween.Definition.Clone();
        }
    }

    internal sealed class LiteEffectTweenSequenceDefinition
    {
        private readonly List<LiteEffectTweenGroupDefinition> groups = new();

        public Action OnComplete;

        public IReadOnlyList<LiteEffectTweenGroupDefinition> Groups => groups;

        public bool HasTweens => groups.Count > 0;

        public void Append(LiteEffectTweenDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            var group = new LiteEffectTweenGroupDefinition();
            group.Items.Add(definition);
            groups.Add(group);
        }

        public void Join(LiteEffectTweenDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (groups.Count == 0)
            {
                Append(definition);
                return;
            }

            groups[groups.Count - 1].Items.Add(definition);
        }

        public LiteEffectTweenSequenceDefinition Clone()
        {
            var clone = new LiteEffectTweenSequenceDefinition
            {
                OnComplete = OnComplete
            };

            foreach (var group in groups)
            {
                var groupClone = new LiteEffectTweenGroupDefinition();
                foreach (var item in group.Items)
                {
                    groupClone.Items.Add(item.Clone());
                }

                clone.groups.Add(groupClone);
            }

            return clone;
        }
    }

    internal sealed class LiteEffectTweenGroupDefinition
    {
        public List<LiteEffectTweenDefinition> Items { get; } = new();
    }

    internal sealed class LiteEffectTweenDefinition
    {
        public LiteEffectSettings TargetSettings;
        public float Duration;
        public float Delay;
        public LiteEffectEase Ease = LiteEffectEase.Linear;

        public LiteEffectTweenDefinition Clone()
        {
            return new LiteEffectTweenDefinition
            {
                TargetSettings = LiteEffectTweenSettingsUtility.Clone(TargetSettings),
                Duration = Duration,
                Delay = Delay,
                Ease = Ease
            };
        }
    }

    internal sealed class LiteEffectTweenPlayback
    {
        private readonly VisualElement element;
        private bool autoPlayScheduled;
        private bool started;
        private bool killed;
        private IVisualElementScheduledItem autoPlayItem;

        public LiteEffectTweenPlayback(VisualElement element, LiteEffectTweenSequenceDefinition sequence)
        {
            this.element = element;
            Sequence = sequence;
        }

        public VisualElement Element => element;

        public LiteEffectTweenSequenceDefinition Sequence { get; }

        public void ScheduleAutoPlay()
        {
            if (autoPlayScheduled || started || killed)
            {
                return;
            }

            autoPlayScheduled = true;
            autoPlayItem = element.schedule.Execute(AutoPlay).StartingIn(0);
        }

        public void NotifySequenceChanged()
        {
            if (killed)
            {
                return;
            }

            if (started)
            {
                LiteEffectControllerRegistry.GetOrCreate(element).PlayTweenSequence(Sequence.Clone());
            }
            else
            {
                ScheduleAutoPlay();
            }
        }

        public void Kill()
        {
            killed = true;
            autoPlayScheduled = false;

            if (autoPlayItem != null)
            {
                autoPlayItem.Pause();
                autoPlayItem = null;
            }

            LiteEffectControllerRegistry.GetOrCreate(element).KillActiveTween(false);
        }

        private void AutoPlay()
        {
            autoPlayScheduled = false;
            autoPlayItem = null;

            if (killed || element.panel == null || !Sequence.HasTweens)
            {
                return;
            }

            started = true;
            LiteEffectControllerRegistry.GetOrCreate(element).PlayTweenSequence(Sequence.Clone());
        }
    }

    internal sealed class LiteEffectTweenRuntimeSequence
    {
        private readonly List<LiteEffectTweenRuntimeGroup> groups;

        public LiteEffectTweenRuntimeSequence(List<LiteEffectTweenRuntimeGroup> groups, Action onComplete)
        {
            this.groups = groups;
            OnComplete = onComplete;
        }

        public Action OnComplete { get; }

        public bool TryEvaluate(float elapsed, out LiteEffectSettings settings, out bool completed)
        {
            settings = null;
            completed = false;

            if (groups.Count == 0)
            {
                return false;
            }

            var localTime = elapsed;
            LiteEffectSettings lastState = null;

            foreach (var group in groups)
            {
                lastState = LiteEffectTweenSettingsUtility.Clone(group.EndState);

                if (localTime >= group.TotalDuration)
                {
                    localTime -= group.TotalDuration;
                    continue;
                }

                var current = LiteEffectTweenSettingsUtility.Clone(group.BaseState);
                foreach (var item in group.Items)
                {
                    if (localTime < item.Delay)
                    {
                        continue;
                    }

                    var itemElapsed = item.Duration <= 0f
                        ? 1f
                        : Mathf.Clamp01((localTime - item.Delay) / item.Duration);
                    var eased = LiteEffectTweenEaseUtility.Evaluate(item.Ease, itemElapsed);
                    var currentPartial = LiteEffectTweenSettingsUtility.LerpPartial(item.FromValues, item.ToValues, eased);
                    current = LiteEffectTweenSettingsUtility.Merge(current, currentPartial);
                }

                settings = current;
                return true;
            }

            settings = lastState;
            completed = true;
            return true;
        }
    }

    internal sealed class LiteEffectTweenRuntimeGroup
    {
        public LiteEffectSettings BaseState;
        public LiteEffectSettings EndState;
        public float TotalDuration;
        public List<LiteEffectTweenRuntimeItem> Items { get; } = new();
    }

    internal sealed class LiteEffectTweenRuntimeItem
    {
        public LiteEffectSettings FromValues;
        public LiteEffectSettings ToValues;
        public float Delay;
        public float Duration;
        public LiteEffectEase Ease;
    }

    internal static class LiteEffectTweenEaseUtility
    {
        public static float Evaluate(LiteEffectEase ease, float t)
        {
            t = Mathf.Clamp01(t);

            switch (ease)
            {
                case LiteEffectEase.InSine:
                    return 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
                case LiteEffectEase.OutSine:
                    return Mathf.Sin(t * Mathf.PI * 0.5f);
                case LiteEffectEase.InOutSine:
                    return -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;
                case LiteEffectEase.InQuad:
                    return t * t;
                case LiteEffectEase.OutQuad:
                    return 1f - ((1f - t) * (1f - t));
                case LiteEffectEase.InOutQuad:
                    return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) * 0.5f;
                default:
                    return t;
            }
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Acfeel.UIToolkitLiteEffects
{
    internal sealed class LiteEffectTweenController : IDisposable
    {
        private readonly VisualElement element;
        private readonly Action refreshAction;
        private IVisualElementScheduledItem tweenScheduledItem;
        private readonly System.Collections.Generic.List<LiteEffectActiveTweenSequence> activeTweenSequences = new();

        public LiteEffectTweenController(VisualElement element, Action refreshAction)
        {
            this.element = element;
            this.refreshAction = refreshAction;
            TweenSettings = new LiteEffectSettings();
        }

        public bool HasTweenSettings { get; private set; }

        public LiteEffectSettings TweenSettings { get; private set; }

        public void PlaySequence(LiteEffectTweenSequenceDefinition sequence, LiteEffectSettings startState, object owner)
        {
            if (sequence == null || !sequence.HasTweens)
            {
                return;
            }

            var compiled = CompileTweenSequence(sequence, startState);
            if (compiled == null)
            {
                return;
            }

            RemoveSequences(owner);
            activeTweenSequences.Add(new LiteEffectActiveTweenSequence(owner, compiled, Time.realtimeSinceStartupAsDouble));
            EnsureScheduler();
            UpdateTween();
        }

        public void Kill(object owner, bool keepCurrentValue, Action<LiteEffectSettings> promoteExplicit)
        {
            if (keepCurrentValue && TryBuildCurrentFrame(out var currentFrame))
            {
                promoteExplicit?.Invoke(currentFrame);
            }

            RemoveSequences(owner);
            RecalculateCurrentTweenState();

            if (activeTweenSequences.Count == 0 && tweenScheduledItem != null)
            {
                tweenScheduledItem.Pause();
            }
        }

        public void Dispose()
        {
            activeTweenSequences.Clear();
            HasTweenSettings = false;
            TweenSettings = new LiteEffectSettings();

            if (tweenScheduledItem != null)
            {
                tweenScheduledItem.Pause();
            }
        }

        private LiteEffectTweenRuntimeSequence CompileTweenSequence(LiteEffectTweenSequenceDefinition sequence, LiteEffectSettings startState)
        {
            var currentState = LiteEffectTweenSettingsUtility.Clone(startState);
            var runtimeGroups = new System.Collections.Generic.List<LiteEffectTweenRuntimeGroup>();

            foreach (var group in sequence.Groups)
            {
                var runtimeGroup = new LiteEffectTweenRuntimeGroup
                {
                    BaseState = LiteEffectTweenSettingsUtility.Clone(currentState),
                    EndState = LiteEffectTweenSettingsUtility.Clone(currentState)
                };

                var longestDuration = 0f;
                foreach (var item in group.Items)
                {
                    if (item?.TargetSettings == null || !LiteEffectTweenSettingsUtility.HasAnyAssignedField(item.TargetSettings))
                    {
                        continue;
                    }

                    var startPartial = LiteEffectTweenSettingsUtility.ExtractMasked(currentState, item.TargetSettings);
                    var targetState = LiteEffectTweenSettingsUtility.Merge(currentState, item.TargetSettings);
                    var endPartial = LiteEffectTweenSettingsUtility.ExtractMasked(targetState, item.TargetSettings);

                    runtimeGroup.Items.Add(new LiteEffectTweenRuntimeItem
                    {
                        FromValues = startPartial,
                        ToValues = endPartial,
                        Delay = Mathf.Max(0f, item.Delay),
                        Duration = Mathf.Max(0f, item.Duration),
                        Ease = item.Ease
                    });

                    runtimeGroup.EndState = LiteEffectTweenSettingsUtility.Merge(runtimeGroup.EndState, item.TargetSettings);
                    longestDuration = Mathf.Max(longestDuration, Mathf.Max(0f, item.Delay) + Mathf.Max(0f, item.Duration));
                }

                if (runtimeGroup.Items.Count == 0)
                {
                    continue;
                }

                runtimeGroup.TotalDuration = longestDuration;
                runtimeGroups.Add(runtimeGroup);
                currentState = LiteEffectTweenSettingsUtility.Clone(runtimeGroup.EndState);
            }

            return runtimeGroups.Count == 0 ? null : new LiteEffectTweenRuntimeSequence(runtimeGroups, sequence.OnComplete);
        }

        private void EnsureScheduler()
        {
            tweenScheduledItem ??= element.schedule.Execute(UpdateTween).Every(16);
            tweenScheduledItem.Resume();
        }

        private void UpdateTween()
        {
            if (activeTweenSequences.Count == 0)
            {
                HasTweenSettings = false;
                TweenSettings = new LiteEffectSettings();

                if (tweenScheduledItem != null)
                {
                    tweenScheduledItem.Pause();
                }
                return;
            }

            if (element.panel == null)
            {
                return;
            }

            var now = Time.realtimeSinceStartupAsDouble;
            var mergedFrame = new LiteEffectSettings();
            var hasFrame = false;
            System.Collections.Generic.List<Action> completedCallbacks = null;

            for (var i = 0; i < activeTweenSequences.Count; i++)
            {
                var activeSequence = activeTweenSequences[i];
                if (!activeSequence.Sequence.TryEvaluate((float)(now - activeSequence.StartTime), out var frame, out var completed)
                    || frame == null)
                {
                    activeTweenSequences.RemoveAt(i);
                    i--;
                    continue;
                }

                mergedFrame = LiteEffectTweenSettingsUtility.Merge(mergedFrame, frame);
                hasFrame = true;

                if (!completed)
                {
                    continue;
                }

                completedCallbacks ??= new System.Collections.Generic.List<Action>();
                if (activeSequence.Sequence.OnComplete != null)
                {
                    completedCallbacks.Add(activeSequence.Sequence.OnComplete);
                }

                activeTweenSequences.RemoveAt(i);
                i--;
            }

            if (!hasFrame)
            {
                HasTweenSettings = false;
                TweenSettings = new LiteEffectSettings();

                if (tweenScheduledItem != null)
                {
                    tweenScheduledItem.Pause();
                }
                return;
            }

            HasTweenSettings = true;
            TweenSettings = LiteEffectTweenSettingsUtility.Clone(mergedFrame);
            refreshAction?.Invoke();

            if (activeTweenSequences.Count > 0)
            {
                if (completedCallbacks != null)
                {
                    foreach (var callback in completedCallbacks)
                    {
                        callback?.Invoke();
                    }
                }

                return;
            }

            HasTweenSettings = false;
            TweenSettings = new LiteEffectSettings();

            if (tweenScheduledItem != null)
            {
                tweenScheduledItem.Pause();
            }

            if (completedCallbacks == null)
            {
                return;
            }

            foreach (var callback in completedCallbacks)
            {
                callback?.Invoke();
            }
        }

        private void RemoveSequences(object owner)
        {
            if (owner == null)
            {
                activeTweenSequences.Clear();
                return;
            }

            for (var i = activeTweenSequences.Count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(activeTweenSequences[i].Owner, owner))
                {
                    activeTweenSequences.RemoveAt(i);
                }
            }
        }

        private void RecalculateCurrentTweenState()
        {
            if (!TryBuildCurrentFrame(out var currentFrame))
            {
                HasTweenSettings = false;
                TweenSettings = new LiteEffectSettings();
                refreshAction?.Invoke();
                return;
            }

            HasTweenSettings = true;
            TweenSettings = currentFrame;
            refreshAction?.Invoke();
        }

        private bool TryBuildCurrentFrame(out LiteEffectSettings currentFrame)
        {
            currentFrame = null;

            if (activeTweenSequences.Count == 0 || element.panel == null)
            {
                return false;
            }

            var now = Time.realtimeSinceStartupAsDouble;
            var mergedFrame = new LiteEffectSettings();
            var hasFrame = false;

            for (var i = activeTweenSequences.Count - 1; i >= 0; i--)
            {
                var activeSequence = activeTweenSequences[i];
                if (!activeSequence.Sequence.TryEvaluate((float)(now - activeSequence.StartTime), out var frame, out _)
                    || frame == null)
                {
                    activeTweenSequences.RemoveAt(i);
                    continue;
                }

                mergedFrame = LiteEffectTweenSettingsUtility.Merge(mergedFrame, frame);
                hasFrame = true;
            }

            if (!hasFrame)
            {
                return false;
            }

            currentFrame = LiteEffectTweenSettingsUtility.Clone(mergedFrame);
            return true;
        }
    }

    internal readonly struct LiteEffectActiveTweenSequence
    {
        public LiteEffectActiveTweenSequence(object owner, LiteEffectTweenRuntimeSequence sequence, double startTime)
        {
            Owner = owner;
            Sequence = sequence;
            StartTime = startTime;
        }

        public object Owner { get; }

        public LiteEffectTweenRuntimeSequence Sequence { get; }

        public double StartTime { get; }
    }
}

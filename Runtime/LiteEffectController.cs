using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace Acfeel.UIToolkitLiteEffects
{
    internal static class LiteEffectControllerRegistry
    {
        private static readonly ConditionalWeakTable<VisualElement, LiteEffectController> Controllers = new();

        public static LiteEffectController GetOrCreate(VisualElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return Controllers.GetValue(element, static key => new LiteEffectController(key));
        }
    }

    internal sealed class LiteEffectController : IDisposable
    {
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int BrightnessId = Shader.PropertyToID("_Brightness");
        private static readonly int ContrastId = Shader.PropertyToID("_Contrast");
        private static readonly int SaturationId = Shader.PropertyToID("_Saturation");
        private static readonly int MultiplyId = Shader.PropertyToID("_Multiply");
        private static readonly int AddId = Shader.PropertyToID("_Add");
        private static readonly int GradientEnabledId = Shader.PropertyToID("_GradientEnabled");
        private static readonly int GradientFromId = Shader.PropertyToID("_GradientFrom");
        private static readonly int GradientToId = Shader.PropertyToID("_GradientTo");
        private static readonly int GradientDirectionId = Shader.PropertyToID("_GradientDirection");
        private static readonly int GradientModeId = Shader.PropertyToID("_GradientMode");
        private static readonly int BlendEnabledId = Shader.PropertyToID("_BlendEnabled");
        private static readonly int BlendModeId = Shader.PropertyToID("_BlendMode");
        private static readonly int BlendStrengthId = Shader.PropertyToID("_BlendStrength");

        private readonly VisualElement element;
        private LiteEffectSettings explicitSettings = new();
        private LiteEffectSettings ussSettings = new();
        private LiteEffectSettings tweenSettings = new();
        private IVisualElementScheduledItem tweenScheduledItem;
        private LiteEffectTweenRuntimeSequence activeTweenSequence;
        private RenderTexture processedTexture;
        private Material effectMaterial;
        private StyleColor originalInlineTint;
        private Vector2Int processedTextureSize;
        private double tweenStartTime;
        private bool hasExplicitSettings;
        private bool hasTweenSettings;
        private bool dirty = true;
        private bool tintCaptured;
        private bool tintSuppressed;
        private bool disposed;

        public LiteEffectController(VisualElement element)
        {
            this.element = element;
            element.generateVisualContent += OnGenerateVisualContent;
            element.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            element.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            element.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            element.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        public void Apply(LiteEffectSettings settings)
        {
            KillActiveTween(false);
            explicitSettings = settings ?? new LiteEffectSettings();
            hasExplicitSettings = true;
            Refresh();
        }

        public void SetColorAdjust(ColorAdjustSettings settings)
        {
            KillActiveTween(false);
            explicitSettings ??= new LiteEffectSettings();
            explicitSettings.ColorAdjust = settings;
            hasExplicitSettings = true;
            Refresh();
        }

        public void SetGradient(GradientSettings settings)
        {
            KillActiveTween(false);
            explicitSettings ??= new LiteEffectSettings();
            explicitSettings.Gradient = settings;
            hasExplicitSettings = true;
            Refresh();
        }

        public void SetBlend(BlendSettings settings)
        {
            KillActiveTween(false);
            explicitSettings ??= new LiteEffectSettings();
            explicitSettings.Blend = settings;
            hasExplicitSettings = true;
            Refresh();
        }

        public void ClearExplicit()
        {
            KillActiveTween(false);
            explicitSettings = new LiteEffectSettings();
            hasExplicitSettings = false;
            Refresh();
        }

        internal void PlayTweenSequence(LiteEffectTweenSequenceDefinition sequence)
        {
            if (sequence == null || !sequence.HasTweens)
            {
                return;
            }

            var compiled = CompileTweenSequence(sequence);
            if (compiled == null)
            {
                return;
            }

            KillActiveTween(false);
            activeTweenSequence = compiled;
            tweenStartTime = Time.realtimeSinceStartupAsDouble;
            EnsureTweenScheduler();
            UpdateTween();
        }

        internal void KillActiveTween(bool keepCurrentValue)
        {
            if (hasTweenSettings && keepCurrentValue)
            {
                explicitSettings = LiteEffectTweenSettingsUtility.Clone(tweenSettings);
                hasExplicitSettings = true;
            }

            hasTweenSettings = false;
            tweenSettings = new LiteEffectSettings();
            activeTweenSequence = null;

            if (tweenScheduledItem != null)
            {
                tweenScheduledItem.Pause();
            }
        }

        public void Refresh()
        {
            if (disposed)
            {
                return;
            }

            dirty = true;
            UpdateVisualState();
            element.MarkDirtyRepaint();
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            KillActiveTween(false);
            element.generateVisualContent -= OnGenerateVisualContent;
            element.UnregisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            element.UnregisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            element.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            element.UnregisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
            RestoreBackgroundImageTint();
            ReleaseProcessedTexture();

            if (effectMaterial != null)
            {
                UnityEngine.Object.DestroyImmediate(effectMaterial);
                effectMaterial = null;
            }
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            Refresh();
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            KillActiveTween(true);
            ReleaseProcessedTexture();
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (!evt.newRect.size.Equals(evt.oldRect.size))
            {
                Refresh();
            }
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            ussSettings = LiteEffectCustomStyleReader.Read(evt.customStyle);
            Refresh();
        }

        private void OnGenerateVisualContent(MeshGenerationContext context)
        {
            if (disposed)
            {
                return;
            }

            var resolvedSettings = GetResolvedSettings();
            if (!resolvedSettings.HasAnyEffect)
            {
                RestoreBackgroundImageTint();
                return;
            }

            if (dirty)
            {
                UpdateVisualState();
            }

            if (processedTexture == null)
            {
                return;
            }

            var rect = element.contentRect;
            if (rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            var mesh = context.Allocate(4, 6, processedTexture);
            var vertices = new Vertex[4];
            vertices[0] = CreateVertex(new Vector2(rect.xMin, rect.yMin), new Vector2(0f, 1f));
            vertices[1] = CreateVertex(new Vector2(rect.xMax, rect.yMin), new Vector2(1f, 1f));
            vertices[2] = CreateVertex(new Vector2(rect.xMax, rect.yMax), new Vector2(1f, 0f));
            vertices[3] = CreateVertex(new Vector2(rect.xMin, rect.yMax), new Vector2(0f, 0f));

            mesh.SetAllVertices(vertices);
            mesh.SetAllIndices(new ushort[] { 0, 1, 2, 2, 3, 0 });
        }

        private static Vertex CreateVertex(Vector2 position, Vector2 uv)
        {
            return new Vertex
            {
                position = new Vector3(position.x, position.y, Vertex.nearZ),
                uv = uv,
                tint = Color.white
            };
        }

        private ResolvedLiteEffectSettings GetResolvedSettings()
        {
            var activeSettings = hasTweenSettings
                ? tweenSettings
                : hasExplicitSettings ? explicitSettings : null;
            return LiteEffectSettingsResolver.Resolve(activeSettings, ussSettings);
        }

        private LiteEffectSettings CaptureTweenStartSettings()
        {
            if (hasTweenSettings)
            {
                return LiteEffectTweenSettingsUtility.Clone(tweenSettings);
            }

            if (hasExplicitSettings)
            {
                return LiteEffectTweenSettingsUtility.Clone(explicitSettings);
            }

            return LiteEffectTweenSettingsUtility.FromResolved(GetResolvedSettings());
        }

        private void UpdateVisualState()
        {
            dirty = false;

            var resolvedSettings = GetResolvedSettings();
            if (!resolvedSettings.HasAnyEffect)
            {
                RestoreBackgroundImageTint();
                ReleaseProcessedTexture();
                return;
            }

            EnsureMaterial();

            var sourceTexture = ExtractTexture(element.resolvedStyle.backgroundImage);
            if (sourceTexture != null)
            {
                SuppressBackgroundImageTint();
            }
            else
            {
                RestoreBackgroundImageTint();
            }

            var targetSize = GetTargetTextureSize(sourceTexture);
            if (targetSize.x <= 0 || targetSize.y <= 0)
            {
                ReleaseProcessedTexture();
                return;
            }

            EnsureProcessedTexture(targetSize);
            RenderEffectTexture(sourceTexture, resolvedSettings);
        }

        private void EnsureMaterial()
        {
            if (effectMaterial != null)
            {
                return;
            }

            var shader = Resources.Load<Shader>("AcfeelUIToolkitLiteEffects");
            if (shader == null)
            {
                shader = Shader.Find("Hidden/Acfeel/UIToolkitLiteEffects");
            }

            if (shader == null)
            {
                throw new InvalidOperationException("UIToolkitLiteEffects shader was not found.");
            }

            effectMaterial = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        private void EnsureProcessedTexture(Vector2Int targetSize)
        {
            if (processedTexture != null && processedTextureSize == targetSize)
            {
                return;
            }

            ReleaseProcessedTexture();

            processedTexture = new RenderTexture(targetSize.x, targetSize.y, 0, RenderTextureFormat.ARGB32)
            {
                name = "UIToolkitLiteEffects_RT",
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            processedTexture.Create();
            processedTextureSize = targetSize;
        }

        private void ReleaseProcessedTexture()
        {
            processedTextureSize = default;

            if (processedTexture == null)
            {
                return;
            }

            processedTexture.Release();
            UnityEngine.Object.DestroyImmediate(processedTexture);
            processedTexture = null;
        }

        private void RenderEffectTexture(Texture sourceTexture, ResolvedLiteEffectSettings resolvedSettings)
        {
            var inputTexture = sourceTexture != null ? sourceTexture : Texture2D.whiteTexture;
            var backgroundColor = sourceTexture != null ? Color.white : element.resolvedStyle.backgroundColor;
            var gradientRadians = resolvedSettings.Gradient.Angle * Mathf.Deg2Rad;

            effectMaterial.SetTexture(MainTexId, inputTexture);
            effectMaterial.SetColor(BaseColorId, backgroundColor);
            effectMaterial.SetFloat(BrightnessId, resolvedSettings.ColorAdjust.Brightness);
            effectMaterial.SetFloat(ContrastId, resolvedSettings.ColorAdjust.Contrast);
            effectMaterial.SetFloat(SaturationId, resolvedSettings.ColorAdjust.Saturation);
            effectMaterial.SetColor(MultiplyId, resolvedSettings.ColorAdjust.Multiply);
            effectMaterial.SetColor(AddId, resolvedSettings.ColorAdjust.Add);
            effectMaterial.SetFloat(GradientEnabledId, resolvedSettings.Gradient.Enabled ? 1f : 0f);
            effectMaterial.SetColor(GradientFromId, resolvedSettings.Gradient.From);
            effectMaterial.SetColor(GradientToId, resolvedSettings.Gradient.To);
            effectMaterial.SetVector(GradientDirectionId, new Vector4(Mathf.Cos(gradientRadians), Mathf.Sin(gradientRadians), 0f, 0f));
            effectMaterial.SetFloat(GradientModeId, (float)resolvedSettings.Gradient.Mode);
            effectMaterial.SetFloat(BlendEnabledId, resolvedSettings.Blend.Enabled ? 1f : 0f);
            effectMaterial.SetFloat(BlendModeId, (float)resolvedSettings.Blend.Mode);
            effectMaterial.SetFloat(BlendStrengthId, resolvedSettings.Blend.Strength);

            Graphics.Blit(inputTexture, processedTexture, effectMaterial);
        }

        private Vector2Int GetTargetTextureSize(Texture sourceTexture)
        {
            if (sourceTexture != null)
            {
                return new Vector2Int(
                    Mathf.Clamp(sourceTexture.width, 1, 2048),
                    Mathf.Clamp(sourceTexture.height, 1, 2048));
            }

            var rect = element.contentRect;
            return new Vector2Int(
                Mathf.Clamp(Mathf.CeilToInt(rect.width), 1, 2048),
                Mathf.Clamp(Mathf.CeilToInt(rect.height), 1, 2048));
        }

        private void SuppressBackgroundImageTint()
        {
            if (!tintCaptured)
            {
                originalInlineTint = element.style.unityBackgroundImageTintColor;
                tintCaptured = true;
            }

            if (tintSuppressed)
            {
                return;
            }

            tintSuppressed = true;
            element.style.unityBackgroundImageTintColor = new StyleColor(new Color(1f, 1f, 1f, 0f));
        }

        private void RestoreBackgroundImageTint()
        {
            if (!tintSuppressed)
            {
                return;
            }

            tintSuppressed = false;
            element.style.unityBackgroundImageTintColor = originalInlineTint;
        }

        private static Texture ExtractTexture(Background background)
        {
            if (background.renderTexture != null)
            {
                return background.renderTexture;
            }

            if (background.texture != null)
            {
                return background.texture;
            }

            if (background.sprite != null)
            {
                return background.sprite.texture;
            }

            return null;
        }

        private LiteEffectTweenRuntimeSequence CompileTweenSequence(LiteEffectTweenSequenceDefinition sequence)
        {
            var currentState = CaptureTweenStartSettings();
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

            if (runtimeGroups.Count == 0)
            {
                return null;
            }

            return new LiteEffectTweenRuntimeSequence(runtimeGroups, sequence.OnComplete);
        }

        private void EnsureTweenScheduler()
        {
            tweenScheduledItem ??= element.schedule.Execute(UpdateTween).Every(16);
            tweenScheduledItem.Resume();
        }

        private void UpdateTween()
        {
            if (disposed || activeTweenSequence == null)
            {
                KillActiveTween(false);
                return;
            }

            if (element.panel == null)
            {
                KillActiveTween(true);
                return;
            }

            if (!activeTweenSequence.TryEvaluate((float)(Time.realtimeSinceStartupAsDouble - tweenStartTime), out var frame, out var completed)
                || frame == null)
            {
                KillActiveTween(false);
                return;
            }

            hasTweenSettings = true;
            tweenSettings = LiteEffectTweenSettingsUtility.Clone(frame);
            Refresh();

            if (!completed)
            {
                return;
            }

            explicitSettings = LiteEffectTweenSettingsUtility.Clone(frame);
            hasExplicitSettings = true;
            var callback = activeTweenSequence.OnComplete;
            KillActiveTween(false);
            callback?.Invoke();
        }
    }
}

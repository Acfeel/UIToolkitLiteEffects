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

    internal static class LiteEffectMeshUtility
    {
        public static Vertex CreateVertex(Vector2 position, Vector2 uv)
        {
            return new Vertex
            {
                position = new Vector3(position.x, position.y, Vertex.nearZ),
                uv = uv,
                tint = Color.white
            };
        }

        public static Vertex CreateTintedVertex(Vector2 position, Vector2 uv, Color tint)
        {
            return new Vertex
            {
                position = new Vector3(position.x, position.y, Vertex.nearZ),
                uv = uv,
                tint = tint
            };
        }
    }

    internal sealed class LiteEffectTweenController : IDisposable
    {
        private readonly VisualElement element;
        private readonly Action refreshAction;
        private IVisualElementScheduledItem tweenScheduledItem;
        private LiteEffectTweenRuntimeSequence activeTweenSequence;
        private double tweenStartTime;

        public LiteEffectTweenController(VisualElement element, Action refreshAction)
        {
            this.element = element;
            this.refreshAction = refreshAction;
            TweenSettings = new LiteEffectSettings();
        }

        public bool HasTweenSettings { get; private set; }

        public LiteEffectSettings TweenSettings { get; private set; }

        public void PlaySequence(LiteEffectTweenSequenceDefinition sequence, LiteEffectSettings startState)
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

            Kill(false, null);
            activeTweenSequence = compiled;
            tweenStartTime = Time.realtimeSinceStartupAsDouble;
            EnsureScheduler();
            UpdateTween();
        }

        public void Kill(bool keepCurrentValue, Action<LiteEffectSettings> promoteExplicit)
        {
            if (HasTweenSettings && keepCurrentValue)
            {
                promoteExplicit?.Invoke(LiteEffectTweenSettingsUtility.Clone(TweenSettings));
            }

            HasTweenSettings = false;
            TweenSettings = new LiteEffectSettings();
            activeTweenSequence = null;

            if (tweenScheduledItem != null)
            {
                tweenScheduledItem.Pause();
            }
        }

        public void Dispose()
        {
            Kill(false, null);
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
            if (activeTweenSequence == null)
            {
                Kill(false, null);
                return;
            }

            if (element.panel == null)
            {
                return;
            }

            if (!activeTweenSequence.TryEvaluate((float)(Time.realtimeSinceStartupAsDouble - tweenStartTime), out var frame, out var completed)
                || frame == null)
            {
                Kill(false, null);
                return;
            }

            HasTweenSettings = true;
            TweenSettings = LiteEffectTweenSettingsUtility.Clone(frame);
            refreshAction?.Invoke();

            if (!completed)
            {
                return;
            }

            var callback = activeTweenSequence.OnComplete;
            Kill(false, null);
            callback?.Invoke();
        }
    }

    internal sealed class LiteEffectOutlineOverlayController : IDisposable
    {
        private readonly VisualElement element;
        private readonly Shader outlineShader;
        private VisualElement outlineOverlayElement;
        private VisualElement outlineOverlayHost;
        private RenderTexture outlineTexture;
        private Material outlineMaterial;
        private StyleEnum<Overflow> originalInlineOverflow;
        private Vector2Int outlineTextureSize;
        private Color outlineOverlayColor = Color.clear;
        private float outlineOverlayThickness;
        private bool overflowCaptured;
        private bool overflowExpanded;
        private IOutlineRenderer activeOutlineRenderer;

        public LiteEffectOutlineOverlayController(VisualElement element)
        {
            this.element = element;
            outlineShader = ResolveShader("AcfeelUIToolkitLiteOutline", "Hidden/Acfeel/UIToolkitLiteOutline");
        }

        public void Update(Texture sourceTexture, Rect contentRect, ResolvedOutlineSettings outline, float opacity, Visibility visibility, DisplayStyle display)
        {
            if (!outline.Enabled || outline.Opacity <= 0.0001f || outline.Thickness <= 0.0001f || contentRect.width <= 0f || contentRect.height <= 0f)
            {
                Hide();
                return;
            }

            if (!EnsureOverlayHost())
            {
                Hide();
                return;
            }

            activeOutlineRenderer = sourceTexture != null ? TransparentImageOutlineRenderer.Instance : ElementOutlineRenderer.Instance;
            var padding = activeOutlineRenderer.GetPadding(outline);
            var targetSize = new Vector2Int(
                Mathf.Clamp(Mathf.CeilToInt(contentRect.width) + padding * 2, 1, 2048),
                Mathf.Clamp(Mathf.CeilToInt(contentRect.height) + padding * 2, 1, 2048));

            var hostWorldRect = outlineOverlayHost.worldBound;
            var contentWorldRect = new Rect(
                element.worldBound.xMin + contentRect.xMin,
                element.worldBound.yMin + contentRect.yMin,
                contentRect.width,
                contentRect.height);

            outlineOverlayElement.style.left = contentWorldRect.xMin - hostWorldRect.xMin - padding;
            outlineOverlayElement.style.top = contentWorldRect.yMin - hostWorldRect.yMin - padding;
            outlineOverlayElement.style.width = targetSize.x;
            outlineOverlayElement.style.height = targetSize.y;
            outlineOverlayElement.style.opacity = opacity;
            outlineOverlayElement.style.visibility = visibility;
            outlineOverlayElement.style.display = display == DisplayStyle.None ? DisplayStyle.None : DisplayStyle.Flex;
            outlineOverlayElement.style.backgroundImage = StyleKeyword.Null;
            outlineOverlayElement.style.backgroundColor = StyleKeyword.Null;
            UpdateOverflowState(true);

            outlineOverlayColor = new Color(outline.Color.r, outline.Color.g, outline.Color.b, outline.Color.a * outline.Opacity);
            outlineOverlayThickness = Mathf.Max(1f, outline.Thickness);

            if (activeOutlineRenderer.RequiresTexture)
            {
                if (sourceTexture == null)
                {
                    Hide();
                    return;
                }

                EnsureOutlineMaterial();
                EnsureOutlineTexture(targetSize);
                activeOutlineRenderer.PrepareTexture(outlineMaterial, outlineTexture, sourceTexture, contentRect.size, targetSize, padding, outline);
            }
            else
            {
                ReleaseOutlineTexture();
            }

            outlineOverlayElement.MarkDirtyRepaint();
        }

        public void Hide()
        {
            if (outlineOverlayElement != null)
            {
                outlineOverlayElement.style.display = DisplayStyle.None;
                outlineOverlayElement.style.backgroundImage = StyleKeyword.Null;
                outlineOverlayElement.style.backgroundColor = StyleKeyword.Null;
            }

            activeOutlineRenderer = null;
            outlineOverlayColor = Color.clear;
            outlineOverlayThickness = 0f;
            ReleaseOutlineTexture();
            RestoreOverflow();
        }

        public void Detach()
        {
            Hide();
            if (outlineOverlayElement != null)
            {
                outlineOverlayElement.RemoveFromHierarchy();
            }

            outlineOverlayHost = null;
        }

        public void Dispose()
        {
            Hide();

            if (outlineMaterial != null)
            {
                UnityEngine.Object.DestroyImmediate(outlineMaterial);
                outlineMaterial = null;
            }

            if (outlineOverlayElement != null)
            {
                outlineOverlayElement.generateVisualContent -= OnGenerateVisualContent;
                outlineOverlayElement.RemoveFromHierarchy();
                outlineOverlayElement = null;
            }

            outlineOverlayHost = null;
        }

        private void OnGenerateVisualContent(MeshGenerationContext context)
        {
            if (activeOutlineRenderer == null || outlineOverlayElement == null)
            {
                return;
            }

            activeOutlineRenderer.Generate(context, outlineOverlayElement.contentRect, outlineTexture, outlineOverlayColor, outlineOverlayThickness);
        }

        private void EnsureOverlayElement()
        {
            if (outlineOverlayElement != null)
            {
                return;
            }

            outlineOverlayElement = new VisualElement
            {
                pickingMode = PickingMode.Ignore
            };
            outlineOverlayElement.style.position = Position.Absolute;
            outlineOverlayElement.style.display = DisplayStyle.None;
            outlineOverlayElement.generateVisualContent += OnGenerateVisualContent;
        }

        private bool EnsureOverlayHost()
        {
            var parent = element.parent;
            if (parent == null)
            {
                return false;
            }

            EnsureOverlayElement();
            if (outlineOverlayElement.parent != parent)
            {
                outlineOverlayElement.RemoveFromHierarchy();
                parent.Insert(parent.IndexOf(element), outlineOverlayElement);
            }
            else
            {
                var elementIndex = parent.IndexOf(element);
                var overlayIndex = parent.IndexOf(outlineOverlayElement);
                if (overlayIndex >= elementIndex)
                {
                    outlineOverlayElement.RemoveFromHierarchy();
                    parent.Insert(elementIndex, outlineOverlayElement);
                }
            }

            outlineOverlayHost = parent;
            return true;
        }

        private void EnsureOutlineMaterial()
        {
            if (outlineMaterial != null)
            {
                return;
            }

            outlineMaterial = new Material(outlineShader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        private void EnsureOutlineTexture(Vector2Int targetSize)
        {
            if (outlineTexture != null && outlineTextureSize == targetSize)
            {
                return;
            }

            ReleaseOutlineTexture();

            outlineTexture = new RenderTexture(targetSize.x, targetSize.y, 0, RenderTextureFormat.ARGB32)
            {
                name = "UIToolkitLiteEffects_OutlineRT",
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            outlineTexture.Create();
            outlineTextureSize = targetSize;
        }

        private void ReleaseOutlineTexture()
        {
            outlineTextureSize = default;

            if (outlineTexture == null)
            {
                return;
            }

            outlineTexture.Release();
            UnityEngine.Object.DestroyImmediate(outlineTexture);
            outlineTexture = null;
        }

        private void UpdateOverflowState(bool expanded)
        {
            if (outlineOverlayHost == null)
            {
                return;
            }

            if (!overflowCaptured)
            {
                originalInlineOverflow = outlineOverlayHost.style.overflow;
                overflowCaptured = true;
            }

            if (expanded)
            {
                if (!overflowExpanded)
                {
                    overflowExpanded = true;
                    outlineOverlayHost.style.overflow = Overflow.Visible;
                }

                return;
            }

            RestoreOverflow();
        }

        private void RestoreOverflow()
        {
            if (!overflowExpanded)
            {
                return;
            }

            overflowExpanded = false;
            if (outlineOverlayHost != null)
            {
                outlineOverlayHost.style.overflow = originalInlineOverflow;
            }
        }

        private static Shader ResolveShader(string resourceName, string shaderName)
        {
            var shader = Resources.Load<Shader>(resourceName);
            if (shader == null)
            {
                shader = Shader.Find(shaderName);
            }

            if (shader == null)
            {
                throw new InvalidOperationException($"{resourceName} shader was not found.");
            }

            return shader;
        }
    }

    internal sealed class LiteEffectController : IDisposable
    {
        private readonly VisualElement element;
        private readonly LiteEffectRenderTextureController renderTextureController;
        private readonly LiteEffectOutlineOverlayController outlineOverlayController;
        private readonly LiteEffectTweenController tweenController;
        private LiteEffectSettings explicitSettings = new();
        private LiteEffectSettings ussSettings = new();
        private StyleColor originalInlineTint;
        private bool hasExplicitSettings;
        private bool dirty = true;
        private bool tintCaptured;
        private bool tintSuppressed;
        private bool disposed;

        public LiteEffectController(VisualElement element)
        {
            this.element = element;
            renderTextureController = new LiteEffectRenderTextureController();
            outlineOverlayController = new LiteEffectOutlineOverlayController(element);
            tweenController = new LiteEffectTweenController(element, Refresh);
            element.generateVisualContent += OnGenerateVisualContent;
            element.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            element.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            element.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            element.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        public void Apply(LiteEffectSettings settings)
        {
            tweenController.Kill(false, PromoteExplicitSettings);
            explicitSettings = settings ?? new LiteEffectSettings();
            hasExplicitSettings = true;
            Refresh();
        }

        public void SetColorAdjust(ColorAdjustSettings settings)
        {
            tweenController.Kill(false, PromoteExplicitSettings);
            explicitSettings ??= new LiteEffectSettings();
            explicitSettings.ColorAdjust = settings;
            hasExplicitSettings = true;
            Refresh();
        }

        public void SetGradient(GradientSettings settings)
        {
            tweenController.Kill(false, PromoteExplicitSettings);
            explicitSettings ??= new LiteEffectSettings();
            explicitSettings.Gradient = settings;
            hasExplicitSettings = true;
            Refresh();
        }

        public void SetBlend(BlendSettings settings)
        {
            tweenController.Kill(false, PromoteExplicitSettings);
            explicitSettings ??= new LiteEffectSettings();
            explicitSettings.Blend = settings;
            hasExplicitSettings = true;
            Refresh();
        }

        public void SetOutline(OutlineSettings settings)
        {
            tweenController.Kill(false, PromoteExplicitSettings);
            explicitSettings ??= new LiteEffectSettings();
            explicitSettings.Outline = settings;
            hasExplicitSettings = true;
            Refresh();
        }

        public void SetGlow(GlowSettings settings)
        {
            tweenController.Kill(false, PromoteExplicitSettings);
            explicitSettings ??= new LiteEffectSettings();
            explicitSettings.Glow = settings;
            hasExplicitSettings = true;
            Refresh();
        }

        public void SetBlur(BlurSettings settings)
        {
            tweenController.Kill(false, PromoteExplicitSettings);
            explicitSettings ??= new LiteEffectSettings();
            explicitSettings.Blur = settings;
            hasExplicitSettings = true;
            Refresh();
        }

        public void SetDissolve(DissolveSettings settings)
        {
            tweenController.Kill(false, PromoteExplicitSettings);
            explicitSettings ??= new LiteEffectSettings();
            explicitSettings.Dissolve = settings;
            hasExplicitSettings = true;
            Refresh();
        }

        public void SetGlitch(GlitchSettings settings)
        {
            tweenController.Kill(false, PromoteExplicitSettings);
            explicitSettings ??= new LiteEffectSettings();
            explicitSettings.Glitch = settings;
            hasExplicitSettings = true;
            Refresh();
        }

        public void ClearExplicit()
        {
            tweenController.Kill(false, PromoteExplicitSettings);
            explicitSettings = new LiteEffectSettings();
            hasExplicitSettings = false;
            Refresh();
        }

        internal void PlayTweenSequence(LiteEffectTweenSequenceDefinition sequence)
        {
            tweenController.PlaySequence(sequence, CaptureTweenStartSettings());
        }

        internal void KillActiveTween(bool keepCurrentValue)
        {
            tweenController.Kill(keepCurrentValue, PromoteExplicitSettings);
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
            tweenController.Dispose();
            element.generateVisualContent -= OnGenerateVisualContent;
            element.UnregisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            element.UnregisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            element.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            element.UnregisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
            RestoreBackgroundImageTint();
            outlineOverlayController.Dispose();
            renderTextureController.Dispose();
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            Refresh();
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            tweenController.Kill(true, PromoteExplicitSettings);
            outlineOverlayController.Detach();
            renderTextureController.Release();
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

            var processedTexture = renderTextureController.ProcessedTexture;
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
            vertices[0] = LiteEffectMeshUtility.CreateVertex(new Vector2(rect.xMin, rect.yMin), new Vector2(0f, 1f));
            vertices[1] = LiteEffectMeshUtility.CreateVertex(new Vector2(rect.xMax, rect.yMin), new Vector2(1f, 1f));
            vertices[2] = LiteEffectMeshUtility.CreateVertex(new Vector2(rect.xMax, rect.yMax), new Vector2(1f, 0f));
            vertices[3] = LiteEffectMeshUtility.CreateVertex(new Vector2(rect.xMin, rect.yMax), new Vector2(0f, 0f));

            mesh.SetAllVertices(vertices);
            mesh.SetAllIndices(new ushort[] { 0, 1, 2, 2, 3, 0 });

            if (resolvedSettings.RequiresRealtimeRefresh)
            {
                element.MarkDirtyRepaint();
            }
        }

        private ResolvedLiteEffectSettings GetResolvedSettings()
        {
            var activeSettings = tweenController.HasTweenSettings
                ? tweenController.TweenSettings
                : hasExplicitSettings ? explicitSettings : null;
            return LiteEffectSettingsResolver.Resolve(activeSettings, ussSettings);
        }

        private LiteEffectSettings CaptureTweenStartSettings()
        {
            if (tweenController.HasTweenSettings)
            {
                return LiteEffectTweenSettingsUtility.Clone(tweenController.TweenSettings);
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
                outlineOverlayController.Hide();
                renderTextureController.Release();
                return;
            }

            var sourceTexture = ExtractTexture(element.resolvedStyle.backgroundImage);
            var backgroundColor = sourceTexture != null ? Color.white : element.resolvedStyle.backgroundColor;
            if (sourceTexture != null)
            {
                SuppressBackgroundImageTint();
            }
            else
            {
                RestoreBackgroundImageTint();
            }

            if (!renderTextureController.Update(element.contentRect, sourceTexture, backgroundColor, resolvedSettings))
            {
                outlineOverlayController.Hide();
                return;
            }

            outlineOverlayController.Update(
                sourceTexture,
                element.contentRect,
                resolvedSettings.Outline,
                element.resolvedStyle.opacity,
                element.resolvedStyle.visibility,
                element.resolvedStyle.display);
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

        private void PromoteExplicitSettings(LiteEffectSettings settings)
        {
            explicitSettings = settings ?? new LiteEffectSettings();
            hasExplicitSettings = true;
        }
    }
}

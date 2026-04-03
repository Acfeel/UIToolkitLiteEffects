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

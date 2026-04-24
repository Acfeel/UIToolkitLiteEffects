using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Acfeel.UIToolkitLiteEffects
{
    internal sealed class LiteEffectOutlineOverlayController : LiteEffectOverlayControllerBase
    {
        private static readonly int DissolveEnabledId = Shader.PropertyToID("_DissolveEnabled");
        private static readonly int DissolveAmountId = Shader.PropertyToID("_DissolveAmount");
        private static readonly int DissolveEdgeWidthId = Shader.PropertyToID("_DissolveEdgeWidth");
        private Color outlineOverlayColor = Color.clear;
        private float outlineOverlayThickness;
        private IOutlineRenderer activeOutlineRenderer;
        private Vector4 cornerRadii = Vector4.zero;
        private int outlineOverlayPadding;

        public LiteEffectOutlineOverlayController(VisualElement element)
            : base(element, "AcfeelUIToolkitLiteOutline", "Hidden/Acfeel/UIToolkitLiteOutline")
        {
        }

        public void Update(Texture sourceTexture, Rect contentRect, ResolvedOutlineSettings outline, ResolvedDissolveSettings dissolve, float opacity, Visibility visibility, DisplayStyle display, Vector4 radii = default)
        {
            if (!outline.Enabled || outline.Opacity <= 0.0001f || outline.Thickness <= 0.0001f || contentRect.width <= 0f || contentRect.height <= 0f)
            {
                Hide();
                return;
            }

            if (LiteEffectDissolveUtility.IsComplete(dissolve))
            {
                Hide();
                return;
            }

            if (!EnsureOverlayHost())
            {
                Hide();
                return;
            }

            cornerRadii = radii;
            activeOutlineRenderer = sourceTexture != null ? TransparentImageOutlineRenderer.Instance : ElementOutlineRenderer.Instance;
            var dissolveFade = LiteEffectDissolveUtility.GetFlatFade(dissolve);
            var padding = activeOutlineRenderer.GetPadding(outline);
            outlineOverlayPadding = padding;
            var targetSize = new Vector2Int(
                Mathf.Clamp(Mathf.CeilToInt(contentRect.width) + padding * 2, 1, 2048),
                Mathf.Clamp(Mathf.CeilToInt(contentRect.height) + padding * 2, 1, 2048));

            var hostWorldRect = overlayHost.worldBound;
            var contentWorldRect = new Rect(
                element.worldBound.xMin + contentRect.xMin,
                element.worldBound.yMin + contentRect.yMin,
                contentRect.width,
                contentRect.height);

            overlayElement.style.left = contentWorldRect.xMin - hostWorldRect.xMin - padding;
            overlayElement.style.top = contentWorldRect.yMin - hostWorldRect.yMin - padding;
            overlayElement.style.width = targetSize.x;
            overlayElement.style.height = targetSize.y;
            overlayElement.style.opacity = opacity;
            overlayElement.style.visibility = visibility;
            overlayElement.style.display = display == DisplayStyle.None ? DisplayStyle.None : DisplayStyle.Flex;
            overlayElement.style.backgroundImage = StyleKeyword.Null;
            overlayElement.style.backgroundColor = StyleKeyword.Null;

            var outlineThicknessPixels = LiteEffectNormalizedRange.ToOutlineThicknessPixels(outline.Thickness);
            outlineOverlayColor = new Color(
                outline.Color.r,
                outline.Color.g,
                outline.Color.b,
                outline.Color.a * outline.Opacity * (activeOutlineRenderer.RequiresTexture ? 1f : dissolveFade));
            outlineOverlayThickness = outlineThicknessPixels;
            IsVisible = display != DisplayStyle.None;

            if (activeOutlineRenderer.RequiresTexture)
            {
                if (sourceTexture == null)
                {
                    Hide();
                    return;
                }

                EnsureOverlayMaterial();
                EnsureOverlayTexture(targetSize);
                overlayMaterial.SetFloat(DissolveEnabledId, dissolve.Enabled ? 1f : 0f);
                overlayMaterial.SetFloat(DissolveAmountId, dissolve.Amount);
                overlayMaterial.SetFloat(DissolveEdgeWidthId, dissolve.EdgeWidth);
                activeOutlineRenderer.PrepareTexture(overlayMaterial, overlayTexture, sourceTexture, contentRect.size, targetSize, padding, outline, cornerRadii);
            }
            else
            {
                ReleaseOverlayTexture();
            }

            overlayElement.MarkDirtyRepaint();
        }

        public override void Hide()
        {
            base.Hide();
            activeOutlineRenderer = null;
            outlineOverlayColor = Color.clear;
            outlineOverlayThickness = 0f;
        }

        public override void Detach()
        {
            if (overlayElement != null)
            {
                overlayElement.generateVisualContent -= OnGenerateVisualContent;
            }
            base.Detach();
        }

        public override void Dispose()
        {
            if (overlayElement != null)
            {
                overlayElement.generateVisualContent -= OnGenerateVisualContent;
            }
            base.Dispose();
        }

        private void OnGenerateVisualContent(MeshGenerationContext context)
        {
            if (activeOutlineRenderer == null || overlayElement == null)
            {
                return;
            }

            activeOutlineRenderer.Generate(context, overlayElement.contentRect, overlayTexture, outlineOverlayColor, outlineOverlayThickness, cornerRadii, outlineOverlayPadding);
        }

        protected override void EnsureOverlayElement()
        {
            base.EnsureOverlayElement();
            if (overlayElement != null)
            {
                overlayElement.generateVisualContent += OnGenerateVisualContent;
            }
        }
    }
}

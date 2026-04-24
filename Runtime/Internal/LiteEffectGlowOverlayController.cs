using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Acfeel.UIToolkitLiteEffects
{
    internal sealed class LiteEffectGlowOverlayController : LiteEffectOverlayControllerBase
    {
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int GlowColorId = Shader.PropertyToID("_GlowColor");
        private static readonly int GlowStrengthId = Shader.PropertyToID("_GlowStrength");
        private static readonly int GlowSpreadId = Shader.PropertyToID("_GlowSpread");
        private static readonly int SourceAlphaMultiplierId = Shader.PropertyToID("_SourceAlphaMultiplier");
        private static readonly int DissolveEnabledId = Shader.PropertyToID("_DissolveEnabled");
        private static readonly int DissolveAmountId = Shader.PropertyToID("_DissolveAmount");
        private static readonly int DissolveEdgeWidthId = Shader.PropertyToID("_DissolveEdgeWidth");
        private static readonly int TexelSizeId = Shader.PropertyToID("_MainTexTexelSize");
        private static readonly int ContentUvRectId = Shader.PropertyToID("_ContentUvRect");
        private static readonly int CornerRadiiId = Shader.PropertyToID("_CornerRadii");
        private static readonly int RectSizeId = Shader.PropertyToID("_RectSize");

        private Vector4 cornerRadii = Vector4.zero;

        public LiteEffectGlowOverlayController(VisualElement element)
            : base(element, "AcfeelUIToolkitLiteGlow", "Hidden/Acfeel/UIToolkitLiteGlow")
        {
        }

        public void Update(
            Texture sourceTexture,
            Rect contentRect,
            Color backgroundColor,
            ResolvedDissolveSettings dissolve,
            ResolvedGlowSettings glow,
            float opacity,
            Visibility visibility,
            DisplayStyle display,
            Vector4 radii = default)
        {
            if (!glow.Enabled || glow.Strength <= 0.0001f || glow.Spread <= 0.0001f || contentRect.width <= 0f || contentRect.height <= 0f)
            {
                Hide();
                return;
            }

            if (LiteEffectDissolveUtility.IsComplete(dissolve))
            {
                Hide();
                return;
            }

            var sourceAlphaMultiplier = sourceTexture != null ? 1f : backgroundColor.a;
            if (sourceAlphaMultiplier <= 0.0001f)
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
            var glowSpreadPixels = LiteEffectNormalizedRange.ToGlowSpreadPixels(glow.Spread);
            var padding = Mathf.CeilToInt(Mathf.Max(2f, glowSpreadPixels * 3f));
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

            EnsureOverlayMaterial();
            EnsureOverlayTexture(targetSize);

            var contentWidth = Mathf.Clamp(Mathf.CeilToInt(contentRect.width), 1, targetSize.x - padding * 2);
            var contentHeight = Mathf.Clamp(Mathf.CeilToInt(contentRect.height), 1, targetSize.y - padding * 2);
            var contentUvRect = new Vector4(
                padding / (float)targetSize.x,
                padding / (float)targetSize.y,
                (padding + contentWidth) / (float)targetSize.x,
                (padding + contentHeight) / (float)targetSize.y);

            var glowSourceTexture = sourceTexture != null ? sourceTexture : Texture2D.whiteTexture;
            overlayMaterial.SetTexture(MainTexId, glowSourceTexture);
            overlayMaterial.SetColor(GlowColorId, glow.Color);
            overlayMaterial.SetFloat(GlowStrengthId, glow.Strength);
            overlayMaterial.SetFloat(GlowSpreadId, glow.Spread);
            overlayMaterial.SetFloat(SourceAlphaMultiplierId, sourceAlphaMultiplier);
            overlayMaterial.SetFloat(DissolveEnabledId, dissolve.Enabled ? 1f : 0f);
            overlayMaterial.SetFloat(DissolveAmountId, dissolve.Amount);
            overlayMaterial.SetFloat(DissolveEdgeWidthId, dissolve.EdgeWidth);
            overlayMaterial.SetVector(TexelSizeId, new Vector4(1f / targetSize.x, 1f / targetSize.y, targetSize.x, targetSize.y));
            overlayMaterial.SetVector(ContentUvRectId, contentUvRect);
            overlayMaterial.SetVector(CornerRadiiId, cornerRadii);
            overlayMaterial.SetVector(RectSizeId, new Vector4(contentRect.width, contentRect.height, padding, 0f));
            Graphics.Blit(glowSourceTexture, overlayTexture, overlayMaterial);

            overlayElement.style.backgroundImage = Background.FromRenderTexture(overlayTexture);
            overlayElement.style.backgroundColor = StyleKeyword.Null;
            IsVisible = display != DisplayStyle.None;
        }

        public override void Hide()
        {
            if (overlayElement != null)
            {
                overlayElement.style.display = DisplayStyle.None;
                overlayElement.style.backgroundImage = StyleKeyword.Null;
                overlayElement.style.backgroundColor = StyleKeyword.Null;
            }

            IsVisible = false;
            ReleaseOverlayTexture();
        }
    }
}

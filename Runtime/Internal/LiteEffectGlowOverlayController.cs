using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Acfeel.UIToolkitLiteEffects
{
    internal sealed class LiteEffectGlowOverlayController : IDisposable
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

        private readonly VisualElement element;
        private readonly Shader glowShader;
        private VisualElement glowOverlayElement;
        private VisualElement glowOverlayHost;
        private RenderTexture glowTexture;
        private Material glowMaterial;
        private Vector2Int glowTextureSize;
        private Vector4 cornerRadii = Vector4.zero;

        public LiteEffectGlowOverlayController(VisualElement element)
        {
            this.element = element;
            glowShader = LiteEffectShaderResolver.Resolve("AcfeelUIToolkitLiteGlow", "Hidden/Acfeel/UIToolkitLiteGlow");
        }

        public bool IsVisible { get; private set; }

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

            var hostWorldRect = glowOverlayHost.worldBound;
            var contentWorldRect = new Rect(
                element.worldBound.xMin + contentRect.xMin,
                element.worldBound.yMin + contentRect.yMin,
                contentRect.width,
                contentRect.height);

            glowOverlayElement.style.left = contentWorldRect.xMin - hostWorldRect.xMin - padding;
            glowOverlayElement.style.top = contentWorldRect.yMin - hostWorldRect.yMin - padding;
            glowOverlayElement.style.width = targetSize.x;
            glowOverlayElement.style.height = targetSize.y;
            glowOverlayElement.style.opacity = opacity;
            glowOverlayElement.style.visibility = visibility;
            glowOverlayElement.style.display = display == DisplayStyle.None ? DisplayStyle.None : DisplayStyle.Flex;

            EnsureGlowMaterial();
            EnsureGlowTexture(targetSize);

            var contentWidth = Mathf.Clamp(Mathf.CeilToInt(contentRect.width), 1, targetSize.x - padding * 2);
            var contentHeight = Mathf.Clamp(Mathf.CeilToInt(contentRect.height), 1, targetSize.y - padding * 2);
            var contentUvRect = new Vector4(
                padding / (float)targetSize.x,
                padding / (float)targetSize.y,
                (padding + contentWidth) / (float)targetSize.x,
                (padding + contentHeight) / (float)targetSize.y);

            var glowSourceTexture = sourceTexture != null ? sourceTexture : Texture2D.whiteTexture;
            glowMaterial.SetTexture(MainTexId, glowSourceTexture);
            glowMaterial.SetColor(GlowColorId, glow.Color);
            glowMaterial.SetFloat(GlowStrengthId, glow.Strength);
            glowMaterial.SetFloat(GlowSpreadId, glow.Spread);
            glowMaterial.SetFloat(SourceAlphaMultiplierId, sourceAlphaMultiplier);
            glowMaterial.SetFloat(DissolveEnabledId, dissolve.Enabled ? 1f : 0f);
            glowMaterial.SetFloat(DissolveAmountId, dissolve.Amount);
            glowMaterial.SetFloat(DissolveEdgeWidthId, dissolve.EdgeWidth);
            glowMaterial.SetVector(TexelSizeId, new Vector4(1f / targetSize.x, 1f / targetSize.y, targetSize.x, targetSize.y));
            glowMaterial.SetVector(ContentUvRectId, contentUvRect);
            glowMaterial.SetVector(CornerRadiiId, cornerRadii);
            glowMaterial.SetVector(RectSizeId, new Vector4(contentRect.width, contentRect.height, padding, 0f));
            Graphics.Blit(glowSourceTexture, glowTexture, glowMaterial);

            glowOverlayElement.style.backgroundImage = Background.FromRenderTexture(glowTexture);
            glowOverlayElement.style.backgroundColor = StyleKeyword.Null;
            IsVisible = display != DisplayStyle.None;
        }

        public void Hide()
        {
            if (glowOverlayElement != null)
            {
                glowOverlayElement.style.display = DisplayStyle.None;
                glowOverlayElement.style.backgroundImage = StyleKeyword.Null;
                glowOverlayElement.style.backgroundColor = StyleKeyword.Null;
            }

            IsVisible = false;
            ReleaseGlowTexture();
        }

        public void Detach()
        {
            Hide();
            if (glowOverlayElement != null)
            {
                glowOverlayElement.RemoveFromHierarchy();
            }

            glowOverlayHost = null;
        }

        public void Dispose()
        {
            Hide();

            if (glowMaterial != null)
            {
                UnityEngine.Object.DestroyImmediate(glowMaterial);
                glowMaterial = null;
            }

            if (glowOverlayElement != null)
            {
                glowOverlayElement.RemoveFromHierarchy();
                glowOverlayElement = null;
            }

            glowOverlayHost = null;
        }

        private void EnsureGlowElement()
        {
            if (glowOverlayElement != null)
            {
                return;
            }

            glowOverlayElement = new VisualElement
            {
                pickingMode = PickingMode.Ignore
            };
            glowOverlayElement.style.position = Position.Absolute;
            glowOverlayElement.style.display = DisplayStyle.None;
        }

        private bool EnsureOverlayHost()
        {
            var parent = element.parent;
            if (parent == null)
            {
                return false;
            }

            EnsureGlowElement();
            if (glowOverlayElement.parent != parent)
            {
                glowOverlayElement.RemoveFromHierarchy();
                parent.Insert(parent.IndexOf(element), glowOverlayElement);
            }
            else
            {
                var elementIndex = parent.IndexOf(element);
                var overlayIndex = parent.IndexOf(glowOverlayElement);
                if (overlayIndex >= elementIndex)
                {
                    glowOverlayElement.RemoveFromHierarchy();
                    parent.Insert(elementIndex, glowOverlayElement);
                }
            }

            glowOverlayHost = parent;
            return true;
        }

        private void EnsureGlowMaterial()
        {
            if (glowMaterial != null)
            {
                return;
            }

            glowMaterial = new Material(glowShader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        private void EnsureGlowTexture(Vector2Int targetSize)
        {
            if (glowTexture != null && glowTextureSize == targetSize)
            {
                return;
            }

            ReleaseGlowTexture();

            glowTexture = new RenderTexture(targetSize.x, targetSize.y, 0, RenderTextureFormat.ARGB32)
            {
                name = "UIToolkitLiteEffects_GlowRT",
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            glowTexture.Create();
            glowTextureSize = targetSize;
        }

        private void ReleaseGlowTexture()
        {
            glowTextureSize = default;

            if (glowTexture == null)
            {
                return;
            }

            glowTexture.Release();
            UnityEngine.Object.DestroyImmediate(glowTexture);
            glowTexture = null;
        }

    }
}

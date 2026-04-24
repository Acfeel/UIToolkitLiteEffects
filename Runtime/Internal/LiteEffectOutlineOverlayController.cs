using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Acfeel.UIToolkitLiteEffects
{
    internal sealed class LiteEffectOutlineOverlayController : IDisposable
    {
        private static readonly int DissolveEnabledId = Shader.PropertyToID("_DissolveEnabled");
        private static readonly int DissolveAmountId = Shader.PropertyToID("_DissolveAmount");
        private static readonly int DissolveEdgeWidthId = Shader.PropertyToID("_DissolveEdgeWidth");
        private readonly VisualElement element;
        private readonly Shader outlineShader;
        private VisualElement outlineOverlayElement;
        private VisualElement outlineOverlayHost;
        private RenderTexture outlineTexture;
        private Material outlineMaterial;
        private Vector2Int outlineTextureSize;
        private Color outlineOverlayColor = Color.clear;
        private float outlineOverlayThickness;
        private IOutlineRenderer activeOutlineRenderer;
        private Vector4 cornerRadii = Vector4.zero;
        private int outlineOverlayPadding;

        public LiteEffectOutlineOverlayController(VisualElement element)
        {
            this.element = element;
            outlineShader = LiteEffectShaderResolver.Resolve("AcfeelUIToolkitLiteOutline", "Hidden/Acfeel/UIToolkitLiteOutline");
        }

        public bool IsVisible { get; private set; }

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

                EnsureOutlineMaterial();
                EnsureOutlineTexture(targetSize);
                outlineMaterial.SetFloat(DissolveEnabledId, dissolve.Enabled ? 1f : 0f);
                outlineMaterial.SetFloat(DissolveAmountId, dissolve.Amount);
                outlineMaterial.SetFloat(DissolveEdgeWidthId, dissolve.EdgeWidth);
                activeOutlineRenderer.PrepareTexture(outlineMaterial, outlineTexture, sourceTexture, contentRect.size, targetSize, padding, outline, cornerRadii);
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
            IsVisible = false;
            ReleaseOutlineTexture();
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

            activeOutlineRenderer.Generate(context, outlineOverlayElement.contentRect, outlineTexture, outlineOverlayColor, outlineOverlayThickness, cornerRadii, outlineOverlayPadding);
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

    }
}

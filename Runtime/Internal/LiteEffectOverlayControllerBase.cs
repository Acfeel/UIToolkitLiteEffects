using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Acfeel.UIToolkitLiteEffects
{
    internal abstract class LiteEffectOverlayControllerBase : IDisposable
    {
        protected readonly VisualElement element;
        protected readonly Shader overlayShader;
        protected VisualElement overlayElement;
        protected VisualElement overlayHost;
        protected RenderTexture overlayTexture;
        protected Material overlayMaterial;
        protected Vector2Int overlayTextureSize;

        protected LiteEffectOverlayControllerBase(VisualElement element, string shaderResourceName, string shaderName)
        {
            this.element = element;
            overlayShader = LiteEffectShaderResolver.Resolve(shaderResourceName, shaderName);
        }

        public bool IsVisible { get; protected set; }

        public virtual void Hide()
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

        public virtual void Detach()
        {
            Hide();
            if (overlayElement != null)
            {
                overlayElement.RemoveFromHierarchy();
            }

            overlayHost = null;
        }

        public virtual void Dispose()
        {
            Hide();

            if (overlayMaterial != null)
            {
                UnityEngine.Object.DestroyImmediate(overlayMaterial);
                overlayMaterial = null;
            }

            if (overlayElement != null)
            {
                overlayElement.RemoveFromHierarchy();
                overlayElement = null;
            }

            overlayHost = null;
        }

        protected virtual void EnsureOverlayElement()
        {
            if (overlayElement != null)
            {
                return;
            }

            overlayElement = new VisualElement
            {
                pickingMode = PickingMode.Ignore
            };
            overlayElement.style.position = Position.Absolute;
            overlayElement.style.display = DisplayStyle.None;
        }

        protected virtual bool EnsureOverlayHost()
        {
            var parent = element.parent;
            if (parent == null)
            {
                return false;
            }

            EnsureOverlayElement();
            if (overlayElement.parent != parent)
            {
                overlayElement.RemoveFromHierarchy();
                parent.Insert(parent.IndexOf(element), overlayElement);
            }
            else
            {
                var elementIndex = parent.IndexOf(element);
                var overlayIndex = parent.IndexOf(overlayElement);
                if (overlayIndex >= elementIndex)
                {
                    overlayElement.RemoveFromHierarchy();
                    parent.Insert(elementIndex, overlayElement);
                }
            }

            overlayHost = parent;
            return true;
        }

        protected virtual void EnsureOverlayMaterial()
        {
            if (overlayMaterial != null)
            {
                return;
            }

            overlayMaterial = new Material(overlayShader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        protected virtual void EnsureOverlayTexture(Vector2Int targetSize)
        {
            if (overlayTexture != null && overlayTextureSize == targetSize)
            {
                return;
            }

            ReleaseOverlayTexture();

            overlayTexture = new RenderTexture(targetSize.x, targetSize.y, 0, RenderTextureFormat.ARGB32)
            {
                name = "UIToolkitLiteEffects_OverlayRT",
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            overlayTexture.Create();
            overlayTextureSize = targetSize;
        }

        protected virtual void ReleaseOverlayTexture()
        {
            overlayTextureSize = default;

            if (overlayTexture == null)
            {
                return;
            }

            overlayTexture.Release();
            UnityEngine.Object.DestroyImmediate(overlayTexture);
            overlayTexture = null;
        }
    }
}

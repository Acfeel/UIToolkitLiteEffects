using System;
using UnityEngine;

namespace Acfeel.UIToolkitLiteEffects
{
    internal sealed class LiteEffectRenderTextureController : IDisposable
    {
        private readonly Shader effectShader;
        private RenderTexture processedTexture;
        private Material effectMaterial;
        private Vector2Int processedTextureSize;

        public LiteEffectRenderTextureController()
        {
            effectShader = LiteEffectShaderResolver.Resolve("AcfeelUIToolkitLiteEffects", "Hidden/Acfeel/UIToolkitLiteEffects");
        }

        public RenderTexture ProcessedTexture => processedTexture;

        public bool Update(Rect contentRect, Texture sourceTexture, Color backgroundColor, ResolvedLiteEffectSettings resolvedSettings)
        {
            var targetSize = new Vector2Int(
                Mathf.Clamp(Mathf.CeilToInt(contentRect.width), 1, 2048),
                Mathf.Clamp(Mathf.CeilToInt(contentRect.height), 1, 2048));

            if (targetSize.x <= 0 || targetSize.y <= 0)
            {
                Release();
                return false;
            }

            EnsureMaterial();
            EnsureProcessedTexture(targetSize);
            var inputTexture = sourceTexture != null ? sourceTexture : Texture2D.whiteTexture;
            LiteEffectMaterialBinder.Bind(effectMaterial, inputTexture, backgroundColor, resolvedSettings, processedTexture);
            Graphics.Blit(inputTexture, processedTexture, effectMaterial);
            return true;
        }

        public void Release()
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

        public void Dispose()
        {
            Release();

            if (effectMaterial != null)
            {
                UnityEngine.Object.DestroyImmediate(effectMaterial);
                effectMaterial = null;
            }
        }

        private void EnsureMaterial()
        {
            if (effectMaterial != null)
            {
                return;
            }

            effectMaterial = new Material(effectShader)
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

            Release();

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

    }
}

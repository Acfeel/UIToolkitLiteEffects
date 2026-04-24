using UnityEngine;
using UnityEngine.UIElements;

namespace Acfeel.UIToolkitLiteEffects
{
    internal interface IOutlineRenderer
    {
        bool RequiresTexture { get; }

        int GetPadding(ResolvedOutlineSettings outline);

        void PrepareTexture(
            Material outlineMaterial,
            RenderTexture outlineTexture,
            Texture sourceTexture,
            Vector2 contentSize,
            Vector2Int targetSize,
            int padding,
            ResolvedOutlineSettings outline,
            Vector4 cornerRadii);

        void Generate(MeshGenerationContext context, Rect rect, RenderTexture outlineTexture, Color outlineColor, float thickness, Vector4 cornerRadii, int padding);
    }

    internal sealed class ElementOutlineRenderer : IOutlineRenderer
    {
        public static readonly ElementOutlineRenderer Instance = new();

        public bool RequiresTexture => false;

        public int GetPadding(ResolvedOutlineSettings outline)
        {
            return Mathf.CeilToInt(Mathf.Max(1f, LiteEffectNormalizedRange.ToOutlineThicknessPixels(outline.Thickness)));
        }

        public void PrepareTexture(
            Material outlineMaterial,
            RenderTexture outlineTexture,
            Texture sourceTexture,
            Vector2 contentSize,
            Vector2Int targetSize,
            int padding,
            ResolvedOutlineSettings outline,
            Vector4 cornerRadii)
        {
        }

        public void Generate(MeshGenerationContext context, Rect rect, RenderTexture outlineTexture, Color outlineColor, float thickness, Vector4 cornerRadii, int padding)
        {
            if (outlineColor.a <= 0.0001f || thickness <= 0.0001f || rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            var t = thickness;
            var inner = new Rect(t + padding, t + padding, rect.width - t * 2f - padding * 2f, rect.height - t * 2f - padding * 2f);
            if (inner.width <= 0f || inner.height <= 0f)
            {
                return;
            }

            if (cornerRadii == Vector4.zero)
            {
                var mesh = context.Allocate(16, 24, Texture2D.whiteTexture);
                var vertices = new Vertex[16];
                vertices[0] = LiteEffectMeshUtility.CreateTintedVertex(new Vector2(rect.xMin, rect.yMin), Vector2.zero, outlineColor);
                vertices[1] = LiteEffectMeshUtility.CreateTintedVertex(new Vector2(rect.xMax, rect.yMin), Vector2.zero, outlineColor);
                vertices[2] = LiteEffectMeshUtility.CreateTintedVertex(new Vector2(rect.xMax, inner.yMin), Vector2.zero, outlineColor);
                vertices[3] = LiteEffectMeshUtility.CreateTintedVertex(new Vector2(rect.xMin, inner.yMin), Vector2.zero, outlineColor);
                vertices[4] = LiteEffectMeshUtility.CreateTintedVertex(new Vector2(rect.xMin, inner.yMax), Vector2.zero, outlineColor);
                vertices[5] = LiteEffectMeshUtility.CreateTintedVertex(new Vector2(rect.xMax, inner.yMax), Vector2.zero, outlineColor);
                vertices[6] = LiteEffectMeshUtility.CreateTintedVertex(new Vector2(rect.xMax, rect.yMax), Vector2.zero, outlineColor);
                vertices[7] = LiteEffectMeshUtility.CreateTintedVertex(new Vector2(rect.xMin, rect.yMax), Vector2.zero, outlineColor);
                vertices[8] = LiteEffectMeshUtility.CreateTintedVertex(new Vector2(rect.xMin, inner.yMin), Vector2.zero, outlineColor);
                vertices[9] = LiteEffectMeshUtility.CreateTintedVertex(new Vector2(inner.xMin, inner.yMin), Vector2.zero, outlineColor);
                vertices[10] = LiteEffectMeshUtility.CreateTintedVertex(new Vector2(inner.xMin, inner.yMax), Vector2.zero, outlineColor);
                vertices[11] = LiteEffectMeshUtility.CreateTintedVertex(new Vector2(rect.xMin, inner.yMax), Vector2.zero, outlineColor);
                vertices[12] = LiteEffectMeshUtility.CreateTintedVertex(new Vector2(inner.xMax, inner.yMin), Vector2.zero, outlineColor);
                vertices[13] = LiteEffectMeshUtility.CreateTintedVertex(new Vector2(rect.xMax, inner.yMin), Vector2.zero, outlineColor);
                vertices[14] = LiteEffectMeshUtility.CreateTintedVertex(new Vector2(rect.xMax, inner.yMax), Vector2.zero, outlineColor);
                vertices[15] = LiteEffectMeshUtility.CreateTintedVertex(new Vector2(inner.xMax, inner.yMax), Vector2.zero, outlineColor);
                mesh.SetAllVertices(vertices);
                mesh.SetAllIndices(new ushort[]
                {
                    0, 1, 2, 2, 3, 0,
                    4, 5, 6, 6, 7, 4,
                    8, 9, 10, 10, 11, 8,
                    12, 13, 14, 14, 15, 12
                });
            }
            else
            {
                var verts = new System.Collections.Generic.List<Vertex>();
                var indices = new System.Collections.Generic.List<ushort>();
                LiteEffectMeshUtility.GenerateRoundedRingMesh(rect, cornerRadii, thickness, 8, verts, indices, outlineColor);

                if (verts.Count > 0 && indices.Count > 0)
                {
                    var mesh = context.Allocate(verts.Count, indices.Count, Texture2D.whiteTexture);
                    mesh.SetAllVertices(verts.ToArray());
                    mesh.SetAllIndices(indices.ToArray());
                }
            }
        }
    }

    internal sealed class TransparentImageOutlineRenderer : IOutlineRenderer
    {
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineThicknessId = Shader.PropertyToID("_OutlineThickness");
        private static readonly int OutlineOpacityId = Shader.PropertyToID("_OutlineOpacity");
        private static readonly int OutlineSampleQualityId = Shader.PropertyToID("_OutlineSampleQuality");
        private static readonly int TexelSizeId = Shader.PropertyToID("_MainTexTexelSize");
        private static readonly int ContentUvRectId = Shader.PropertyToID("_ContentUvRect");
        private static readonly int CornerRadiiId = Shader.PropertyToID("_CornerRadii");
        private static readonly int RectSizeId = Shader.PropertyToID("_RectSize");

        public static readonly TransparentImageOutlineRenderer Instance = new();

        public bool RequiresTexture => true;

        public int GetPadding(ResolvedOutlineSettings outline)
        {
            return Mathf.CeilToInt(Mathf.Max(1f, LiteEffectNormalizedRange.ToOutlineThicknessPixels(outline.Thickness)));
        }

        public void PrepareTexture(
            Material outlineMaterial,
            RenderTexture outlineTexture,
            Texture sourceTexture,
            Vector2 contentSize,
            Vector2Int targetSize,
            int padding,
            ResolvedOutlineSettings outline,
            Vector4 cornerRadii)
        {
            if (outlineMaterial == null || outlineTexture == null || sourceTexture == null)
            {
                return;
            }

            var contentWidth = Mathf.Clamp(Mathf.CeilToInt(contentSize.x), 1, targetSize.x - padding * 2);
            var contentHeight = Mathf.Clamp(Mathf.CeilToInt(contentSize.y), 1, targetSize.y - padding * 2);
            var contentUvRect = new Vector4(
                padding / (float)targetSize.x,
                padding / (float)targetSize.y,
                (padding + contentWidth) / (float)targetSize.x,
                (padding + contentHeight) / (float)targetSize.y);

            outlineMaterial.SetTexture(MainTexId, sourceTexture);
            outlineMaterial.SetColor(OutlineColorId, outline.Color);
            outlineMaterial.SetFloat(OutlineThicknessId, LiteEffectNormalizedRange.ToOutlineThicknessPixels(outline.Thickness));
            outlineMaterial.SetFloat(OutlineOpacityId, outline.Opacity);
            outlineMaterial.SetFloat(OutlineSampleQualityId, outline.Quality == LiteEffectOutlineQuality.Low ? 0f : 1f);
            outlineMaterial.SetVector(TexelSizeId, new Vector4(1f / targetSize.x, 1f / targetSize.y, targetSize.x, targetSize.y));
            outlineMaterial.SetVector(ContentUvRectId, contentUvRect);
            outlineMaterial.SetVector(CornerRadiiId, cornerRadii);
            outlineMaterial.SetVector(RectSizeId, new Vector4(contentSize.x, contentSize.y, padding, 0f));
            Graphics.Blit(sourceTexture, outlineTexture, outlineMaterial);
        }

        public void Generate(MeshGenerationContext context, Rect rect, RenderTexture outlineTexture, Color outlineColor, float thickness, Vector4 cornerRadii, int padding)
        {
            if (outlineTexture == null || rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            if (cornerRadii == Vector4.zero)
            {
                var mesh = context.Allocate(4, 6, outlineTexture);
                var vertices = new Vertex[4];
                vertices[0] = LiteEffectMeshUtility.CreateVertex(new Vector2(rect.xMin, rect.yMin), new Vector2(0f, 1f));
                vertices[1] = LiteEffectMeshUtility.CreateVertex(new Vector2(rect.xMax, rect.yMin), new Vector2(1f, 1f));
                vertices[2] = LiteEffectMeshUtility.CreateVertex(new Vector2(rect.xMax, rect.yMax), new Vector2(1f, 0f));
                vertices[3] = LiteEffectMeshUtility.CreateVertex(new Vector2(rect.xMin, rect.yMax), new Vector2(0f, 0f));
                mesh.SetAllVertices(vertices);
                mesh.SetAllIndices(new ushort[] { 0, 1, 2, 2, 3, 0 });
            }
            else
            {
                var verts = new System.Collections.Generic.List<Vertex>();
                var indices = new System.Collections.Generic.List<ushort>();
                LiteEffectMeshUtility.GenerateRoundedRectMesh(rect, cornerRadii, 8, verts, indices, Color.white);

                if (verts.Count > 0 && indices.Count > 0)
                {
                    var mesh = context.Allocate(verts.Count, indices.Count, outlineTexture);
                    mesh.SetAllVertices(verts.ToArray());
                    mesh.SetAllIndices(indices.ToArray());
                }
            }
        }
    }
}

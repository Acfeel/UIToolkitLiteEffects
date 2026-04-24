using UnityEngine;
using UnityEngine.UIElements;

namespace Acfeel.UIToolkitLiteEffects
{
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

        public static Vector4 ReadBorderRadii(VisualElement element)
        {
            if (element == null)
                return Vector4.zero;

            var rect = element.contentRect;
            if (float.IsNaN(rect.width) || float.IsNaN(rect.height) || rect.width <= 0f || rect.height <= 0f)
                return Vector4.zero;

            // Try resolvedStyle first, fallback to inline style if not resolved
            var tl = element.resolvedStyle.borderTopLeftRadius;
            var tr = element.resolvedStyle.borderTopRightRadius;
            var br = element.resolvedStyle.borderBottomRightRadius;
            var bl = element.resolvedStyle.borderBottomLeftRadius;

            // If resolvedStyle is zero, check inline style
            if (tl <= 0.01f && element.style.borderTopLeftRadius != StyleKeyword.Null && element.style.borderTopLeftRadius != StyleKeyword.Undefined)
                tl = element.style.borderTopLeftRadius.value.value;
            if (tr <= 0.01f && element.style.borderTopRightRadius != StyleKeyword.Null && element.style.borderTopRightRadius != StyleKeyword.Undefined)
                tr = element.style.borderTopRightRadius.value.value;
            if (br <= 0.01f && element.style.borderBottomRightRadius != StyleKeyword.Null && element.style.borderBottomRightRadius != StyleKeyword.Undefined)
                br = element.style.borderBottomRightRadius.value.value;
            if (bl <= 0.01f && element.style.borderBottomLeftRadius != StyleKeyword.Null && element.style.borderBottomLeftRadius != StyleKeyword.Undefined)
                bl = element.style.borderBottomLeftRadius.value.value;

            // Clamp each corner radius to half of its corresponding dimension to prevent arc center inversion
            // Each radius must be ≤ width/2 (for horizontal pairs) and ≤ height/2 (for vertical pairs)
            float maxHorizontal = rect.width * 0.5f;
            float maxVertical = rect.height * 0.5f;

            tl = Mathf.Min(tl, maxHorizontal, maxVertical);
            tr = Mathf.Min(tr, maxHorizontal, maxVertical);
            br = Mathf.Min(br, maxHorizontal, maxVertical);
            bl = Mathf.Min(bl, maxHorizontal, maxVertical);

            return new Vector4(tl, tr, br, bl);
        }

        public static int CountRoundedRectVertices(Vector4 radii, int segmentsPerCorner)
        {
            if (radii == Vector4.zero)
                return 0;

            int count = 1; // center vertex
            count += (radii.x > 0 ? segmentsPerCorner + 1 : 1) + (radii.y > 0 ? segmentsPerCorner + 1 : 1) +
                     (radii.z > 0 ? segmentsPerCorner + 1 : 1) + (radii.w > 0 ? segmentsPerCorner + 1 : 1);
            return count;
        }

        public static void GenerateRoundedRectMesh(Rect rect, Vector4 radii, int segmentsPerCorner,
            System.Collections.Generic.List<Vertex> vertsOut, System.Collections.Generic.List<ushort> indicesOut, Color tint)
        {
            vertsOut.Clear();
            indicesOut.Clear();

            var centerX = rect.xMin + rect.width * 0.5f;
            var centerY = rect.yMin + rect.height * 0.5f;
            var centerVert = CreateTintedVertex(new Vector2(centerX, centerY), Vector2.one * 0.5f, tint);
            vertsOut.Add(centerVert);

            var boundaryPoints = new System.Collections.Generic.List<Vector2>();
            var cornerSegments = new System.Collections.Generic.List<Vector2>();

            // Build boundary vertices going clockwise: top → right → bottom → left → back to top
            // Top edge
            boundaryPoints.Add(new Vector2(rect.xMin + radii.x, rect.yMin));
            boundaryPoints.Add(new Vector2(rect.xMax - radii.y, rect.yMin));

            // Top-right corner
            GenerateCornerArc(rect, radii.y, 1, segmentsPerCorner, cornerSegments);
            for (int i = 1; i < cornerSegments.Count; i++) // skip first point (already added as edge end)
                boundaryPoints.Add(cornerSegments[i]);

            // Right edge
            boundaryPoints.Add(new Vector2(rect.xMax, rect.yMin + radii.y));
            boundaryPoints.Add(new Vector2(rect.xMax, rect.yMax - radii.z));

            // Bottom-right corner
            cornerSegments.Clear();
            GenerateCornerArc(rect, radii.z, 2, segmentsPerCorner, cornerSegments);
            for (int i = 1; i < cornerSegments.Count; i++)
                boundaryPoints.Add(cornerSegments[i]);

            // Bottom edge
            boundaryPoints.Add(new Vector2(rect.xMax - radii.z, rect.yMax));
            boundaryPoints.Add(new Vector2(rect.xMin + radii.w, rect.yMax));

            // Bottom-left corner
            cornerSegments.Clear();
            GenerateCornerArc(rect, radii.w, 3, segmentsPerCorner, cornerSegments);
            for (int i = 1; i < cornerSegments.Count; i++)
                boundaryPoints.Add(cornerSegments[i]);

            // Left edge
            boundaryPoints.Add(new Vector2(rect.xMin, rect.yMax - radii.w));
            boundaryPoints.Add(new Vector2(rect.xMin, rect.yMin + radii.x));

            // Top-left corner
            cornerSegments.Clear();
            GenerateCornerArc(rect, radii.x, 0, segmentsPerCorner, cornerSegments);
            for (int i = 1; i < cornerSegments.Count; i++)
                boundaryPoints.Add(cornerSegments[i]);

            // Convert boundary points to vertices and create fan triangulation
            var centerIdx = (ushort)0;
            for (int i = 0; i < boundaryPoints.Count; i++)
            {
                var point = boundaryPoints[i];
                var uv = new Vector2((point.x - rect.xMin) / rect.width, (point.y - rect.yMin) / rect.height);
                vertsOut.Add(CreateTintedVertex(point, uv, tint));
            }

            // Create triangles from center to boundary
            for (int i = 0; i < boundaryPoints.Count; i++)
            {
                var next = (i + 1) % boundaryPoints.Count;
                indicesOut.Add(centerIdx);
                indicesOut.Add((ushort)(i + 1));
                indicesOut.Add((ushort)(next + 1));
            }
        }

        private static void GenerateCornerArc(Rect rect, float radius, int cornerIndex, int segments,
            System.Collections.Generic.List<Vector2> points)
        {
            points.Clear();
            if (radius <= 0.0001f)
            {
                var corner = GetCornerPoint(rect, cornerIndex);
                points.Add(corner);
                return;
            }

            // Generate arc points for each corner with explicit angle ranges
            // cornerIndex: 0=TL, 1=TR, 2=BR, 3=BL
            Vector2 cornerCenter = Vector2.zero;
            float startAngle = 0f;
            float endAngle = 0f;

            if (cornerIndex == 0) // TL: top-left (last in boundary, goes from left edge back to top edge)
            {
                cornerCenter = new Vector2(rect.xMin + radius, rect.yMin + radius);
                startAngle = Mathf.PI; // 180° (pointing left)
                endAngle = Mathf.PI * 1.5f; // 270° (pointing up)
            }
            else if (cornerIndex == 1) // TR: top-right (goes from top edge to right edge)
            {
                cornerCenter = new Vector2(rect.xMax - radius, rect.yMin + radius);
                startAngle = Mathf.PI * 1.5f; // 270° (pointing up)
                endAngle = 0f; // 0° (pointing right)
            }
            else if (cornerIndex == 2) // BR: bottom-right (goes from right edge to bottom edge)
            {
                cornerCenter = new Vector2(rect.xMax - radius, rect.yMax - radius);
                startAngle = 0f; // 0° (pointing right)
                endAngle = Mathf.PI * 0.5f; // 90° (pointing down)
            }
            else // BL: bottom-left (goes from bottom edge to left edge)
            {
                cornerCenter = new Vector2(rect.xMin + radius, rect.yMax - radius);
                startAngle = Mathf.PI * 0.5f; // 90° (pointing down)
                endAngle = Mathf.PI; // 180° (pointing left)
            }

            // Generate arc by interpolating the angle
            // Handle angle wrapping to take the short path
            float adjustedEndAngle = endAngle;
            if (adjustedEndAngle < startAngle)
                adjustedEndAngle += Mathf.PI * 2f;

            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                float angle = Mathf.Lerp(startAngle, adjustedEndAngle, t);
                float x = cornerCenter.x + radius * Mathf.Cos(angle);
                float y = cornerCenter.y + radius * Mathf.Sin(angle);
                points.Add(new Vector2(x, y));
            }
        }

        private static Vector2 GetCornerPoint(Rect rect, int cornerIndex)
        {
            if (cornerIndex == 0) return new Vector2(rect.xMin, rect.yMin);
            if (cornerIndex == 1) return new Vector2(rect.xMax, rect.yMin);
            if (cornerIndex == 2) return new Vector2(rect.xMax, rect.yMax);
            return new Vector2(rect.xMin, rect.yMax);
        }

        private static void AppendCornerVertices(System.Collections.Generic.List<Vector2> cornerPoints, Rect rect,
            System.Collections.Generic.List<Vertex> vertsOut, System.Collections.Generic.List<ushort> indicesOut, Color tint)
        {
            var centerIdx = (ushort)0;
            var baseIdx = (ushort)vertsOut.Count;

            foreach (var point in cornerPoints)
            {
                var uv = new Vector2((point.x - rect.xMin) / rect.width, (point.y - rect.yMin) / rect.height);
                vertsOut.Add(CreateTintedVertex(point, uv, tint));
            }

            for (int i = 0; i < cornerPoints.Count - 1; i++)
            {
                indicesOut.Add(centerIdx);
                indicesOut.Add((ushort)(baseIdx + i));
                indicesOut.Add((ushort)(baseIdx + i + 1));
            }
        }

        public static void GenerateRoundedRingMesh(Rect outer, Vector4 outerRadii, float thickness, int segmentsPerCorner,
            System.Collections.Generic.List<Vertex> vertsOut, System.Collections.Generic.List<ushort> indicesOut, Color tint)
        {
            vertsOut.Clear();
            indicesOut.Clear();

            var t = thickness;
            var inner = new Rect(outer.x + t, outer.y + t, outer.width - t * 2f, outer.height - t * 2f);
            if (inner.width <= 0f || inner.height <= 0f)
                return;

            var innerRadii = new Vector4(
                Mathf.Max(0, outerRadii.x - thickness),
                Mathf.Max(0, outerRadii.y - thickness),
                Mathf.Max(0, outerRadii.z - thickness),
                Mathf.Max(0, outerRadii.w - thickness)
            );

            var outerVerts = new System.Collections.Generic.List<Vertex>();
            var innerVerts = new System.Collections.Generic.List<Vertex>();
            var tempIndices = new System.Collections.Generic.List<ushort>();

            GenerateRoundedRectMesh(outer, outerRadii, segmentsPerCorner, outerVerts, tempIndices, tint);
            GenerateRoundedRectMesh(inner, innerRadii, segmentsPerCorner, innerVerts, tempIndices, tint);

            // Merge rings: outer (skip center) + inner (skip center)
            var baseOuter = (ushort)vertsOut.Count;
            for (int i = 1; i < outerVerts.Count; i++)
                vertsOut.Add(outerVerts[i]);

            var baseInner = (ushort)vertsOut.Count;
            for (int i = 1; i < innerVerts.Count; i++)
                vertsOut.Add(innerVerts[i]);

            // Tri-strip between outer and inner loops
            int outerCount = outerVerts.Count - 1;
            int innerCount = innerVerts.Count - 1;
            for (int i = 0; i < outerCount; i++)
            {
                var next = (i + 1) % outerCount;
                indicesOut.Add((ushort)(baseOuter + i));
                indicesOut.Add((ushort)(baseOuter + next));
                indicesOut.Add((ushort)(baseInner + i));
                indicesOut.Add((ushort)(baseOuter + next));
                indicesOut.Add((ushort)(baseInner + next));
                indicesOut.Add((ushort)(baseInner + i));
            }
        }
    }
}

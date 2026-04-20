using System;
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

            var maxRadius = Mathf.Min(rect.width, rect.height) * 0.5f;

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

            tl = Mathf.Min(tl, maxRadius);
            tr = Mathf.Min(tr, maxRadius);
            br = Mathf.Min(br, maxRadius);
            bl = Mathf.Min(bl, maxRadius);

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
            var inner = new Rect(t, t, outer.width - t * 2f, outer.height - t * 2f);
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

    internal static class LiteEffectMaterialBinder
    {
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int BrightnessId = Shader.PropertyToID("_Brightness");
        private static readonly int ContrastId = Shader.PropertyToID("_Contrast");
        private static readonly int SaturationId = Shader.PropertyToID("_Saturation");
        private static readonly int HueId = Shader.PropertyToID("_Hue");
        private static readonly int MultiplyId = Shader.PropertyToID("_Multiply");
        private static readonly int AddId = Shader.PropertyToID("_Add");
        private static readonly int GradientEnabledId = Shader.PropertyToID("_GradientEnabled");
        private static readonly int GradientFromId = Shader.PropertyToID("_GradientFrom");
        private static readonly int GradientToId = Shader.PropertyToID("_GradientTo");
        private static readonly int GradientDirectionId = Shader.PropertyToID("_GradientDirection");
        private static readonly int GradientModeId = Shader.PropertyToID("_GradientMode");
        private static readonly int GradientStrengthId = Shader.PropertyToID("_GradientStrength");
        private static readonly int OutlineEnabledId = Shader.PropertyToID("_OutlineEnabled");
        private static readonly int GlowEnabledId = Shader.PropertyToID("_GlowEnabled");
        private static readonly int GlowColorId = Shader.PropertyToID("_GlowColor");
        private static readonly int GlowStrengthId = Shader.PropertyToID("_GlowStrength");
        private static readonly int GlowSpreadId = Shader.PropertyToID("_GlowSpread");
        private static readonly int BlurEnabledId = Shader.PropertyToID("_BlurEnabled");
        private static readonly int BlurRadiusId = Shader.PropertyToID("_BlurRadius");
        private static readonly int BlurStrengthId = Shader.PropertyToID("_BlurStrength");
        private static readonly int DissolveEnabledId = Shader.PropertyToID("_DissolveEnabled");
        private static readonly int DissolveAmountId = Shader.PropertyToID("_DissolveAmount");
        private static readonly int DissolveEdgeWidthId = Shader.PropertyToID("_DissolveEdgeWidth");
        private static readonly int DissolveEdgeColorId = Shader.PropertyToID("_DissolveEdgeColor");
        private static readonly int GlitchEnabledId = Shader.PropertyToID("_GlitchEnabled");
        private static readonly int GlitchIntensityId = Shader.PropertyToID("_GlitchIntensity");
        private static readonly int GlitchJitterId = Shader.PropertyToID("_GlitchJitter");
        private static readonly int GlitchColorShiftId = Shader.PropertyToID("_GlitchColorShift");
        private static readonly int GlitchScanlineStrengthId = Shader.PropertyToID("_GlitchScanlineStrength");
        private static readonly int ColorizeEnabledId = Shader.PropertyToID("_ColorizeEnabled");
        private static readonly int ColorizeColorId = Shader.PropertyToID("_ColorizeColor");
        private static readonly int ColorizeStrengthId = Shader.PropertyToID("_ColorizeStrength");
        private static readonly int TimeId = Shader.PropertyToID("_LiteEffectTime");
        private static readonly int TexelSizeId = Shader.PropertyToID("_MainTexTexelSize");
        private static readonly int ContentUvRectId = Shader.PropertyToID("_ContentUvRect");

        public static void Bind(Material material, Texture inputTexture, Color backgroundColor, ResolvedLiteEffectSettings resolvedSettings, RenderTexture processedTexture)
        {
            var gradientRadians = resolvedSettings.Gradient.Angle * Mathf.Deg2Rad;

            material.SetTexture(MainTexId, inputTexture);
            material.SetVector(BaseColorId, (Vector4)backgroundColor.linear);
            material.SetFloat(BrightnessId, resolvedSettings.ColorAdjust.Brightness);
            material.SetFloat(ContrastId, resolvedSettings.ColorAdjust.Contrast);
            material.SetFloat(SaturationId, resolvedSettings.ColorAdjust.Saturation);
            material.SetFloat(HueId, resolvedSettings.ColorAdjust.Hue);
            material.SetColor(MultiplyId, resolvedSettings.ColorAdjust.Multiply);
            material.SetColor(AddId, resolvedSettings.ColorAdjust.Add);
            material.SetFloat(GradientEnabledId, resolvedSettings.Gradient.Enabled ? 1f : 0f);
            material.SetColor(GradientFromId, resolvedSettings.Gradient.From);
            material.SetColor(GradientToId, resolvedSettings.Gradient.To);
            material.SetVector(GradientDirectionId, new Vector4(Mathf.Cos(gradientRadians), Mathf.Sin(gradientRadians), 0f, 0f));
            material.SetFloat(GradientModeId, (float)resolvedSettings.Gradient.Mode);
            material.SetFloat(GradientStrengthId, resolvedSettings.Gradient.Strength);
            material.SetFloat(GlowEnabledId, resolvedSettings.Glow.Enabled ? 1f : 0f);
            material.SetColor(GlowColorId, resolvedSettings.Glow.Color);
            material.SetFloat(GlowStrengthId, resolvedSettings.Glow.Strength);
            material.SetFloat(GlowSpreadId, resolvedSettings.Glow.Spread);
            material.SetFloat(BlurEnabledId, resolvedSettings.Blur.Enabled ? 1f : 0f);
            material.SetFloat(BlurRadiusId, resolvedSettings.Blur.Radius);
            material.SetFloat(BlurStrengthId, resolvedSettings.Blur.Strength);
            material.SetFloat(DissolveEnabledId, resolvedSettings.Dissolve.Enabled ? 1f : 0f);
            material.SetFloat(DissolveAmountId, resolvedSettings.Dissolve.Amount);
            material.SetFloat(DissolveEdgeWidthId, resolvedSettings.Dissolve.EdgeWidth);
            material.SetColor(DissolveEdgeColorId, resolvedSettings.Dissolve.EdgeColor);
            material.SetFloat(GlitchEnabledId, resolvedSettings.Glitch.Enabled ? 1f : 0f);
            material.SetFloat(GlitchIntensityId, resolvedSettings.Glitch.Intensity);
            material.SetFloat(GlitchJitterId, resolvedSettings.Glitch.Jitter);
            material.SetFloat(GlitchColorShiftId, resolvedSettings.Glitch.ColorShift);
            material.SetFloat(GlitchScanlineStrengthId, resolvedSettings.Glitch.ScanlineStrength);
            material.SetFloat(ColorizeEnabledId, resolvedSettings.Colorize.Enabled ? 1f : 0f);
            material.SetColor(ColorizeColorId, resolvedSettings.Colorize.Color);
            material.SetFloat(ColorizeStrengthId, resolvedSettings.Colorize.Strength);
            material.SetFloat(TimeId, Time.unscaledTime);
            material.SetFloat(OutlineEnabledId, 0f); // Outline is rendered via overlay element, not the main shader pass
            material.SetVector(TexelSizeId, new Vector4(1f / processedTexture.width, 1f / processedTexture.height, processedTexture.width, processedTexture.height));
            material.SetVector(ContentUvRectId, new Vector4(0f, 0f, 1f, 1f));
        }
    }

    internal static class LiteEffectDissolveUtility
    {
        public const float CompleteThreshold = 0.9995f;

        public static bool IsComplete(ResolvedDissolveSettings dissolve)
        {
            return dissolve.Enabled && dissolve.Amount >= CompleteThreshold;
        }

        public static float GetFlatFade(ResolvedDissolveSettings dissolve)
        {
            return dissolve.Enabled ? Mathf.SmoothStep(1f, 0f, Mathf.Clamp01(dissolve.Amount)) : 1f;
        }
    }

    internal static class LiteEffectNormalizedRange
    {
        // Maximum pixel sizes for normalized [0..1] input values in USS and the C# API.
        public const float OutlineThicknessMaxPixels = 4f;
        public const float GlowSpreadMaxPixels = 4f;

        public static float ToOutlineThicknessPixels(float normalizedThickness)
        {
            return Mathf.Clamp01(normalizedThickness) * OutlineThicknessMaxPixels;
        }

        public static float ToGlowSpreadPixels(float normalizedSpread)
        {
            return Mathf.Clamp01(normalizedSpread) * GlowSpreadMaxPixels;
        }
    }

    internal sealed class LiteEffectRenderTextureController : IDisposable
    {
        private readonly Shader effectShader;
        private RenderTexture processedTexture;
        private Material effectMaterial;
        private Vector2Int processedTextureSize;

        public LiteEffectRenderTextureController()
        {
            effectShader = ResolveShader("AcfeelUIToolkitLiteEffects", "Hidden/Acfeel/UIToolkitLiteEffects");
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

    internal sealed class LiteEffectTweenController : IDisposable
    {
        private readonly VisualElement element;
        private readonly Action refreshAction;
        private IVisualElementScheduledItem tweenScheduledItem;
        private readonly System.Collections.Generic.List<LiteEffectActiveTweenSequence> activeTweenSequences = new();

        public LiteEffectTweenController(VisualElement element, Action refreshAction)
        {
            this.element = element;
            this.refreshAction = refreshAction;
            TweenSettings = new LiteEffectSettings();
        }

        public bool HasTweenSettings { get; private set; }

        public LiteEffectSettings TweenSettings { get; private set; }

        public void PlaySequence(LiteEffectTweenSequenceDefinition sequence, LiteEffectSettings startState, object owner)
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

            RemoveSequences(owner);
            activeTweenSequences.Add(new LiteEffectActiveTweenSequence(owner, compiled, Time.realtimeSinceStartupAsDouble));
            EnsureScheduler();
            UpdateTween();
        }

        public void Kill(object owner, bool keepCurrentValue, Action<LiteEffectSettings> promoteExplicit)
        {
            if (keepCurrentValue && TryBuildCurrentFrame(out var currentFrame))
            {
                promoteExplicit?.Invoke(currentFrame);
            }

            RemoveSequences(owner);
            RecalculateCurrentTweenState();

            if (activeTweenSequences.Count == 0 && tweenScheduledItem != null)
            {
                tweenScheduledItem.Pause();
            }
        }

        public void Dispose()
        {
            activeTweenSequences.Clear();
            HasTweenSettings = false;
            TweenSettings = new LiteEffectSettings();

            if (tweenScheduledItem != null)
            {
                tweenScheduledItem.Pause();
            }
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
            if (activeTweenSequences.Count == 0)
            {
                HasTweenSettings = false;
                TweenSettings = new LiteEffectSettings();

                if (tweenScheduledItem != null)
                {
                    tweenScheduledItem.Pause();
                }
                return;
            }

            if (element.panel == null)
            {
                return;
            }

            var now = Time.realtimeSinceStartupAsDouble;
            var mergedFrame = new LiteEffectSettings();
            var hasFrame = false;
            System.Collections.Generic.List<Action> completedCallbacks = null;

            for (var i = 0; i < activeTweenSequences.Count; i++)
            {
                var activeSequence = activeTweenSequences[i];
                if (!activeSequence.Sequence.TryEvaluate((float)(now - activeSequence.StartTime), out var frame, out var completed)
                    || frame == null)
                {
                    activeTweenSequences.RemoveAt(i);
                    i--;
                    continue;
                }

                mergedFrame = LiteEffectTweenSettingsUtility.Merge(mergedFrame, frame);
                hasFrame = true;

                if (!completed)
                {
                    continue;
                }

                completedCallbacks ??= new System.Collections.Generic.List<Action>();
                if (activeSequence.Sequence.OnComplete != null)
                {
                    completedCallbacks.Add(activeSequence.Sequence.OnComplete);
                }

                activeTweenSequences.RemoveAt(i);
                i--;
            }

            if (!hasFrame)
            {
                HasTweenSettings = false;
                TweenSettings = new LiteEffectSettings();

                if (tweenScheduledItem != null)
                {
                    tweenScheduledItem.Pause();
                }
                return;
            }

            HasTweenSettings = true;
            TweenSettings = LiteEffectTweenSettingsUtility.Clone(mergedFrame);
            refreshAction?.Invoke();

            if (activeTweenSequences.Count > 0)
            {
                if (completedCallbacks != null)
                {
                    foreach (var callback in completedCallbacks)
                    {
                        callback?.Invoke();
                    }
                }

                return;
            }

            HasTweenSettings = false;
            TweenSettings = new LiteEffectSettings();

            if (tweenScheduledItem != null)
            {
                tweenScheduledItem.Pause();
            }

            if (completedCallbacks == null)
            {
                return;
            }

            foreach (var callback in completedCallbacks)
            {
                callback?.Invoke();
            }
        }

        private void RemoveSequences(object owner)
        {
            if (owner == null)
            {
                activeTweenSequences.Clear();
                return;
            }

            for (var i = activeTweenSequences.Count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(activeTweenSequences[i].Owner, owner))
                {
                    activeTweenSequences.RemoveAt(i);
                }
            }
        }

        private void RecalculateCurrentTweenState()
        {
            if (!TryBuildCurrentFrame(out var currentFrame))
            {
                HasTweenSettings = false;
                TweenSettings = new LiteEffectSettings();
                refreshAction?.Invoke();
                return;
            }

            HasTweenSettings = true;
            TweenSettings = currentFrame;
            refreshAction?.Invoke();
        }

        private bool TryBuildCurrentFrame(out LiteEffectSettings currentFrame)
        {
            currentFrame = null;

            if (activeTweenSequences.Count == 0 || element.panel == null)
            {
                return false;
            }

            var now = Time.realtimeSinceStartupAsDouble;
            var mergedFrame = new LiteEffectSettings();
            var hasFrame = false;

            for (var i = activeTweenSequences.Count - 1; i >= 0; i--)
            {
                var activeSequence = activeTweenSequences[i];
                if (!activeSequence.Sequence.TryEvaluate((float)(now - activeSequence.StartTime), out var frame, out _)
                    || frame == null)
                {
                    activeTweenSequences.RemoveAt(i);
                    continue;
                }

                mergedFrame = LiteEffectTweenSettingsUtility.Merge(mergedFrame, frame);
                hasFrame = true;
            }

            if (!hasFrame)
            {
                return false;
            }

            currentFrame = LiteEffectTweenSettingsUtility.Clone(mergedFrame);
            return true;
        }
    }

    internal readonly struct LiteEffectActiveTweenSequence
    {
        public LiteEffectActiveTweenSequence(object owner, LiteEffectTweenRuntimeSequence sequence, double startTime)
        {
            Owner = owner;
            Sequence = sequence;
            StartTime = startTime;
        }

        public object Owner { get; }

        public LiteEffectTweenRuntimeSequence Sequence { get; }

        public double StartTime { get; }
    }

    internal sealed class LiteEffectOverflowController : IDisposable
    {
        private readonly VisualElement element;
        private VisualElement host;
        private StyleEnum<Overflow> originalInlineOverflow;
        private bool overflowCaptured;
        private bool expanded;

        public LiteEffectOverflowController(VisualElement element)
        {
            this.element = element;
        }

        public void SetExpanded(bool shouldExpand)
        {
            var parent = element.parent;
            if (parent == null)
            {
                RestoreOverflow();
                return;
            }

            if (host != null && host != parent)
            {
                RestoreOverflow();
            }

            host = parent;
            if (!overflowCaptured)
            {
                originalInlineOverflow = host.style.overflow;
                overflowCaptured = true;
            }

            if (shouldExpand)
            {
                if (!expanded)
                {
                    expanded = true;
                    host.style.overflow = Overflow.Visible;
                }

                return;
            }

            RestoreOverflow();
        }

        public void Dispose()
        {
            RestoreOverflow();
        }

        private void RestoreOverflow()
        {
            if (!expanded)
            {
                host = element.parent;
                return;
            }

            expanded = false;
            if (host != null)
            {
                host.style.overflow = originalInlineOverflow;
            }

            host = element.parent;
        }
    }

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

        public LiteEffectOutlineOverlayController(VisualElement element)
        {
            this.element = element;
            outlineShader = ResolveShader("AcfeelUIToolkitLiteOutline", "Hidden/Acfeel/UIToolkitLiteOutline");
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

            activeOutlineRenderer.Generate(context, outlineOverlayElement.contentRect, outlineTexture, outlineOverlayColor, outlineOverlayThickness, cornerRadii);
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
            glowShader = ResolveShader("AcfeelUIToolkitLiteGlow", "Hidden/Acfeel/UIToolkitLiteGlow");
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

        void Generate(MeshGenerationContext context, Rect rect, RenderTexture outlineTexture, Color outlineColor, float thickness, Vector4 cornerRadii);
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

        public void Generate(MeshGenerationContext context, Rect rect, RenderTexture outlineTexture, Color outlineColor, float thickness, Vector4 cornerRadii)
        {
            if (outlineColor.a <= 0.0001f || thickness <= 0.0001f || rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            var t = thickness;
            var inner = new Rect(t, t, rect.width - t * 2f, rect.height - t * 2f);
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

        public void Generate(MeshGenerationContext context, Rect rect, RenderTexture outlineTexture, Color outlineColor, float thickness, Vector4 cornerRadii)
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
                UnityEngine.Debug.Log($"[OutlineRenderer] Rounded mesh: {verts.Count} verts, cornerRadii={cornerRadii}");

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

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
    }

    internal static class LiteEffectMaterialBinder
    {
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int BrightnessId = Shader.PropertyToID("_Brightness");
        private static readonly int ContrastId = Shader.PropertyToID("_Contrast");
        private static readonly int SaturationId = Shader.PropertyToID("_Saturation");
        private static readonly int MultiplyId = Shader.PropertyToID("_Multiply");
        private static readonly int AddId = Shader.PropertyToID("_Add");
        private static readonly int GradientEnabledId = Shader.PropertyToID("_GradientEnabled");
        private static readonly int GradientFromId = Shader.PropertyToID("_GradientFrom");
        private static readonly int GradientToId = Shader.PropertyToID("_GradientTo");
        private static readonly int GradientDirectionId = Shader.PropertyToID("_GradientDirection");
        private static readonly int GradientModeId = Shader.PropertyToID("_GradientMode");
        private static readonly int BlendEnabledId = Shader.PropertyToID("_BlendEnabled");
        private static readonly int BlendModeId = Shader.PropertyToID("_BlendMode");
        private static readonly int BlendStrengthId = Shader.PropertyToID("_BlendStrength");
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
        private static readonly int TimeId = Shader.PropertyToID("_LiteEffectTime");
        private static readonly int TexelSizeId = Shader.PropertyToID("_MainTexTexelSize");
        private static readonly int ContentUvRectId = Shader.PropertyToID("_ContentUvRect");

        public static void Bind(Material material, Texture inputTexture, Color backgroundColor, ResolvedLiteEffectSettings resolvedSettings, RenderTexture processedTexture)
        {
            var gradientRadians = resolvedSettings.Gradient.Angle * Mathf.Deg2Rad;

            material.SetTexture(MainTexId, inputTexture);
            material.SetColor(BaseColorId, backgroundColor);
            material.SetFloat(BrightnessId, resolvedSettings.ColorAdjust.Brightness);
            material.SetFloat(ContrastId, resolvedSettings.ColorAdjust.Contrast);
            material.SetFloat(SaturationId, resolvedSettings.ColorAdjust.Saturation);
            material.SetColor(MultiplyId, resolvedSettings.ColorAdjust.Multiply);
            material.SetColor(AddId, resolvedSettings.ColorAdjust.Add);
            material.SetFloat(GradientEnabledId, resolvedSettings.Gradient.Enabled ? 1f : 0f);
            material.SetColor(GradientFromId, resolvedSettings.Gradient.From);
            material.SetColor(GradientToId, resolvedSettings.Gradient.To);
            material.SetVector(GradientDirectionId, new Vector4(Mathf.Cos(gradientRadians), Mathf.Sin(gradientRadians), 0f, 0f));
            material.SetFloat(GradientModeId, (float)resolvedSettings.Gradient.Mode);
            material.SetFloat(BlendEnabledId, resolvedSettings.Blend.Enabled ? 1f : 0f);
            material.SetFloat(BlendModeId, (float)resolvedSettings.Blend.Mode);
            material.SetFloat(BlendStrengthId, resolvedSettings.Blend.Strength);
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
            material.SetFloat(TimeId, Time.unscaledTime);
            material.SetFloat(OutlineEnabledId, 0f);
            material.SetVector(TexelSizeId, new Vector4(1f / processedTexture.width, 1f / processedTexture.height, processedTexture.width, processedTexture.height));
            material.SetVector(ContentUvRectId, new Vector4(0f, 0f, 1f, 1f));
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
        private LiteEffectTweenRuntimeSequence activeTweenSequence;
        private double tweenStartTime;

        public LiteEffectTweenController(VisualElement element, Action refreshAction)
        {
            this.element = element;
            this.refreshAction = refreshAction;
            TweenSettings = new LiteEffectSettings();
        }

        public bool HasTweenSettings { get; private set; }

        public LiteEffectSettings TweenSettings { get; private set; }

        public void PlaySequence(LiteEffectTweenSequenceDefinition sequence, LiteEffectSettings startState)
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

            Kill(false, null);
            activeTweenSequence = compiled;
            tweenStartTime = Time.realtimeSinceStartupAsDouble;
            EnsureScheduler();
            UpdateTween();
        }

        public void Kill(bool keepCurrentValue, Action<LiteEffectSettings> promoteExplicit)
        {
            if (HasTweenSettings && keepCurrentValue)
            {
                promoteExplicit?.Invoke(LiteEffectTweenSettingsUtility.Clone(TweenSettings));
            }

            HasTweenSettings = false;
            TweenSettings = new LiteEffectSettings();
            activeTweenSequence = null;

            if (tweenScheduledItem != null)
            {
                tweenScheduledItem.Pause();
            }
        }

        public void Dispose()
        {
            Kill(false, null);
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
            if (activeTweenSequence == null)
            {
                Kill(false, null);
                return;
            }

            if (element.panel == null)
            {
                return;
            }

            if (!activeTweenSequence.TryEvaluate((float)(Time.realtimeSinceStartupAsDouble - tweenStartTime), out var frame, out var completed)
                || frame == null)
            {
                Kill(false, null);
                return;
            }

            HasTweenSettings = true;
            TweenSettings = LiteEffectTweenSettingsUtility.Clone(frame);
            refreshAction?.Invoke();

            if (!completed)
            {
                return;
            }

            var callback = activeTweenSequence.OnComplete;
            Kill(false, null);
            callback?.Invoke();
        }
    }

    internal sealed class LiteEffectOutlineOverlayController : IDisposable
    {
        private readonly VisualElement element;
        private readonly Shader outlineShader;
        private VisualElement outlineOverlayElement;
        private VisualElement outlineOverlayHost;
        private RenderTexture outlineTexture;
        private Material outlineMaterial;
        private StyleEnum<Overflow> originalInlineOverflow;
        private Vector2Int outlineTextureSize;
        private Color outlineOverlayColor = Color.clear;
        private float outlineOverlayThickness;
        private bool overflowCaptured;
        private bool overflowExpanded;
        private IOutlineRenderer activeOutlineRenderer;

        public LiteEffectOutlineOverlayController(VisualElement element)
        {
            this.element = element;
            outlineShader = ResolveShader("AcfeelUIToolkitLiteOutline", "Hidden/Acfeel/UIToolkitLiteOutline");
        }

        public void Update(Texture sourceTexture, Rect contentRect, ResolvedOutlineSettings outline, float opacity, Visibility visibility, DisplayStyle display)
        {
            if (!outline.Enabled || outline.Opacity <= 0.0001f || outline.Thickness <= 0.0001f || contentRect.width <= 0f || contentRect.height <= 0f)
            {
                Hide();
                return;
            }

            if (!EnsureOverlayHost())
            {
                Hide();
                return;
            }

            activeOutlineRenderer = sourceTexture != null ? TransparentImageOutlineRenderer.Instance : ElementOutlineRenderer.Instance;
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
            UpdateOverflowState(true);

            outlineOverlayColor = new Color(outline.Color.r, outline.Color.g, outline.Color.b, outline.Color.a * outline.Opacity);
            outlineOverlayThickness = Mathf.Max(1f, outline.Thickness);

            if (activeOutlineRenderer.RequiresTexture)
            {
                if (sourceTexture == null)
                {
                    Hide();
                    return;
                }

                EnsureOutlineMaterial();
                EnsureOutlineTexture(targetSize);
                activeOutlineRenderer.PrepareTexture(outlineMaterial, outlineTexture, sourceTexture, contentRect.size, targetSize, padding, outline);
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
            ReleaseOutlineTexture();
            RestoreOverflow();
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

            activeOutlineRenderer.Generate(context, outlineOverlayElement.contentRect, outlineTexture, outlineOverlayColor, outlineOverlayThickness);
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

        private void UpdateOverflowState(bool expanded)
        {
            if (outlineOverlayHost == null)
            {
                return;
            }

            if (!overflowCaptured)
            {
                originalInlineOverflow = outlineOverlayHost.style.overflow;
                overflowCaptured = true;
            }

            if (expanded)
            {
                if (!overflowExpanded)
                {
                    overflowExpanded = true;
                    outlineOverlayHost.style.overflow = Overflow.Visible;
                }

                return;
            }

            RestoreOverflow();
        }

        private void RestoreOverflow()
        {
            if (!overflowExpanded)
            {
                return;
            }

            overflowExpanded = false;
            if (outlineOverlayHost != null)
            {
                outlineOverlayHost.style.overflow = originalInlineOverflow;
            }
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
            ResolvedOutlineSettings outline);

        void Generate(MeshGenerationContext context, Rect rect, RenderTexture outlineTexture, Color outlineColor, float thickness);
    }

    internal sealed class ElementOutlineRenderer : IOutlineRenderer
    {
        public static readonly ElementOutlineRenderer Instance = new();

        public bool RequiresTexture => false;

        public int GetPadding(ResolvedOutlineSettings outline)
        {
            return Mathf.CeilToInt(Mathf.Max(1f, outline.Thickness));
        }

        public void PrepareTexture(
            Material outlineMaterial,
            RenderTexture outlineTexture,
            Texture sourceTexture,
            Vector2 contentSize,
            Vector2Int targetSize,
            int padding,
            ResolvedOutlineSettings outline)
        {
        }

        public void Generate(MeshGenerationContext context, Rect rect, RenderTexture outlineTexture, Color outlineColor, float thickness)
        {
            if (outlineColor.a <= 0.0001f || thickness <= 0.0001f || rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            var t = Mathf.Max(1f, thickness);
            var inner = new Rect(t, t, rect.width - t * 2f, rect.height - t * 2f);
            if (inner.width <= 0f || inner.height <= 0f)
            {
                return;
            }

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

        public static readonly TransparentImageOutlineRenderer Instance = new();

        public bool RequiresTexture => true;

        public int GetPadding(ResolvedOutlineSettings outline)
        {
            return Mathf.CeilToInt(Mathf.Max(1f, outline.Thickness));
        }

        public void PrepareTexture(
            Material outlineMaterial,
            RenderTexture outlineTexture,
            Texture sourceTexture,
            Vector2 contentSize,
            Vector2Int targetSize,
            int padding,
            ResolvedOutlineSettings outline)
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
            outlineMaterial.SetFloat(OutlineThicknessId, outline.Thickness);
            outlineMaterial.SetFloat(OutlineOpacityId, outline.Opacity);
            outlineMaterial.SetFloat(OutlineSampleQualityId, outline.Quality == LiteEffectOutlineQuality.Low ? 0f : 1f);
            outlineMaterial.SetVector(TexelSizeId, new Vector4(1f / targetSize.x, 1f / targetSize.y, targetSize.x, targetSize.y));
            outlineMaterial.SetVector(ContentUvRectId, contentUvRect);
            Graphics.Blit(sourceTexture, outlineTexture, outlineMaterial);
        }

        public void Generate(MeshGenerationContext context, Rect rect, RenderTexture outlineTexture, Color outlineColor, float thickness)
        {
            if (outlineTexture == null || rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            var mesh = context.Allocate(4, 6, outlineTexture);
            var vertices = new Vertex[4];
            vertices[0] = LiteEffectMeshUtility.CreateVertex(new Vector2(rect.xMin, rect.yMin), new Vector2(0f, 1f));
            vertices[1] = LiteEffectMeshUtility.CreateVertex(new Vector2(rect.xMax, rect.yMin), new Vector2(1f, 1f));
            vertices[2] = LiteEffectMeshUtility.CreateVertex(new Vector2(rect.xMax, rect.yMax), new Vector2(1f, 0f));
            vertices[3] = LiteEffectMeshUtility.CreateVertex(new Vector2(rect.xMin, rect.yMax), new Vector2(0f, 0f));
            mesh.SetAllVertices(vertices);
            mesh.SetAllIndices(new ushort[] { 0, 1, 2, 2, 3, 0 });
        }
    }
}

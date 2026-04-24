using UnityEngine;

namespace Acfeel.UIToolkitLiteEffects
{
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
}

using UnityEngine;

namespace Acfeel.UIToolkitLiteEffects
{
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
}

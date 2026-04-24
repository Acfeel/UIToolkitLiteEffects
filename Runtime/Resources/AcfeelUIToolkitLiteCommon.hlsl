#ifndef ACFEEL_UITOOLKITLITE_COMMON_INCLUDED
#define ACFEEL_UITOOLKITLITE_COMMON_INCLUDED

// Common helpers shared across Acfeel UIToolkitLiteEffects shaders.
// Requires the including shader to declare the following uniforms:
//   float4 _MainTexTexelSize;   // (1/w, 1/h, w, h)
//   float4 _ContentUvRect;      // (uMin, vMin, uMax, vMax)

// Converts a normalized [0..1] input into pixel distance used by Outline.
// The maximum pixel range (4.0) must match LiteEffectNormalizedRange.OutlineThicknessMaxPixels in C#.
float GetOutlineThicknessPixels(float normalizedThickness)
{
    return saturate(normalizedThickness) * 4.0;
}

// Converts a normalized [0..1] input into pixel distance used by Glow.
// The maximum pixel range (4.0) must match LiteEffectNormalizedRange.GlowSpreadMaxPixels in C#.
float GetGlowSpreadPixels(float normalizedSpread)
{
    return saturate(normalizedSpread) * 4.0;
}

// Converts a normalized [0..1] input into pixel distance used by Blur.
float GetBlurRadiusPixels(float normalizedRadius)
{
    return saturate(normalizedRadius) * 3.0;
}

// Signed distance field of an axis-aligned rounded rectangle with per-corner radii.
// p is the point in local space (rect centered at origin). size is the rect size.
// radii components: x=TL, y=TR, z=BR, w=BL (matches Vector4 packing used by the C# side).
float SDFRoundedRect(float2 p, float2 size, float4 radii)
{
    float2 c = abs(p) - size * 0.5;
    float radius = (p.x > 0.0)
        ? ((p.y > 0.0) ? radii.z : radii.y)
        : ((p.y > 0.0) ? radii.w : radii.x);
    float2 q = c + radius;
    return min(max(c.x, c.y), 0.0) + length(max(q, 0.0)) - radius;
}

// True when uv lies inside the content sub-rect of the padded overlay texture.
bool IsInsideContent(float2 uv)
{
    return uv.x >= _ContentUvRect.x
        && uv.y >= _ContentUvRect.y
        && uv.x <= _ContentUvRect.z
        && uv.y <= _ContentUvRect.w;
}

// Remaps a uv inside the content sub-rect back to [0..1] within that sub-rect.
float2 RemapContentUv(float2 uv)
{
    return float2(
        (uv.x - _ContentUvRect.x) / max(_ContentUvRect.z - _ContentUvRect.x, 0.0001),
        (uv.y - _ContentUvRect.y) / max(_ContentUvRect.w - _ContentUvRect.y, 0.0001));
}

#endif // ACFEEL_UITOOLKITLITE_COMMON_INCLUDED

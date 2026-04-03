static const float LiteEffectDissolveCompleteThreshold = 0.9999;

float LiteEffectHash21(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 78.233);
    return frac(p.x * p.y);
}

float LiteEffectGetDissolveNoise(float2 uv)
{
    float coarse = LiteEffectHash21(floor(uv * 1024.0) + 11.0);
    float fine = LiteEffectHash21(floor(uv * 2048.0) + 29.0);
    float micro = LiteEffectHash21(floor(uv * 4096.0) + 37.0);
    return saturate(coarse * 0.5 + fine * 0.3 + micro * 0.2);
}

float LiteEffectGetDissolveMask(float2 uv, float enabled, float amount, float edgeWidth)
{
    if (enabled <= 0.5)
    {
        return 1.0;
    }

    if (amount >= LiteEffectDissolveCompleteThreshold)
    {
        return 0.0;
    }

    float microNoise = LiteEffectHash21(floor(uv * 640.0) + 37.0);
    float noise = saturate(LiteEffectGetDissolveNoise(uv) * 0.75 + microNoise * 0.25);
    amount = saturate(amount);
    edgeWidth = max(edgeWidth, 0.0001);

    return smoothstep(amount - edgeWidth, amount + edgeWidth, noise);
}

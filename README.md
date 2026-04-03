# UIToolkitLiteEffects

Unity UI Toolkit 向けの軽量エフェクトパッケージです。`VisualElement` や `backgroundImage` を持つ要素に対して、色調補正、グラデーション、簡易ブレンドに加えて、疑似アウトライン、グロー風、疑似ブラー、ディゾルブ、ノイズ/グリッチをコード主導で適用できます。

## 特徴

- `VisualElement` 拡張メソッドで即座に適用できる
- UI Toolkit の `backgroundImage` にも対応
- USS カスタムプロパティを併用できる
- 内部ではカスタムシェーダーを使ってエフェクト済みテクスチャを生成する
- AI から扱いやすいフラットな API
- `AnimateColorAdjust` `AnimateGradient` `AnimateLiteEffect` のチェーン式 Tween に対応
- 追加エフェクトも 1 パスの近似表現を優先し、軽さを重視している

## 導入方法

1. Unity の `Window > Package Manager` を開く
2. `Install package from git URL...` を選ぶ
3. このリポジトリ URL を指定する

## 使い方

```csharp
using Acfeel.UIToolkitLiteEffects;
using UnityEngine;
using UnityEngine.UIElements;

var icon = root.Q<VisualElement>("Icon");

icon.SetLiteEffect(new LiteEffectSettings
{
    ColorAdjust = new ColorAdjustSettings
    {
        Brightness = 0.55f,
        Contrast = 0.6f,
        Saturation = 0.62f
    },
    Gradient = new GradientSettings
    {
        From = new Color(1f, 0.4f, 0.2f, 0.55f),
        To = new Color(1f, 0.9f, 0.2f, 0.15f),
        Angle = 35f
    },
    Blend = new BlendSettings
    {
        Mode = LiteEffectBlendMode.Multiply,
        Strength = 0.35f
    },
    Outline = new OutlineSettings
    {
        Color = Color.white,
        Thickness = 1.0f,
        Opacity = 0.5f
    },
    Glow = new GlowSettings
    {
        Color = new Color(0.35f, 0.8f, 1f, 1f),
        Strength = 0.2f,
        Spread = 1.1f
    },
    Blur = new BlurSettings
    {
        Radius = 1.0f,
        Strength = 0.25f
    },
    Dissolve = new DissolveSettings
    {
        Amount = 0.15f,
        EdgeWidth = 0.08f,
        EdgeColor = new Color(1f, 0.55f, 0.2f, 1f)
    },
    Glitch = new GlitchSettings
    {
        Intensity = 0.2f,
        Jitter = 0.45f,
        ColorShift = 0.35f,
        ScanlineStrength = 0.25f
    }
});
```

部分更新もできます。

```csharp
icon.SetColorAdjust(new ColorAdjustSettings
{
    Multiply = new Color(0.8f, 1.0f, 1.2f, 1f)
});

icon.SetGradient(new GradientSettings
{
    From = Color.cyan,
    To = Color.blue,
    Angle = 90f
});

icon.SetOutline(new OutlineSettings
{
    Thickness = 1.5f,
    Opacity = 0.75f
});

icon.SetGlow(new GlowSettings
{
    Strength = 0.35f,
    Spread = 1.2f
});

icon.SetBlur(new BlurSettings
{
    Radius = 1.25f,
    Strength = 0.3f
});

icon.SetDissolve(new DissolveSettings
{
    Amount = 0.4f
});

icon.SetGlitch(new GlitchSettings
{
    Intensity = 0.3f
});

icon.ClearLiteEffect();
```

チェーン式 Tween も利用できます。更新は `VisualElement.schedule` ベースで、`Coroutine` を使いません。作成した時点で自動再生されるので、`Play` は不要です。

```csharp
icon
    .AnimateColorAdjust(new ColorAdjustSettings
    {
        Multiply = new Color(1.25f, 0.8f, 0.8f, 1f)
    }, 0.2f)
    .SetEase(LiteEffectEase.OutQuad)
    .Append(icon.AnimateGradient(new GradientSettings
    {
        From = new Color(1f, 0.4f, 0.2f, 0.6f),
        To = new Color(0.2f, 0.8f, 1f, 0.2f),
        Angle = 120f
    }, 0.45f).SetEase(LiteEffectEase.InOutSine))
    .OnComplete(() => Debug.Log("LiteEffect tween completed."))
;
```

USS カスタムプロパティだけを使いたい場合は、空の設定でコントローラだけ有効化してください。

```csharp
icon.SetLiteEffect(new LiteEffectSettings());
```

```css
#Icon {
    --ac-litefx-brightness: 0.54;
    --ac-litefx-contrast: 0.58;
    --ac-litefx-saturation: 0.62;
    --ac-litefx-gradient-from: rgba(255, 128, 64, 0.55);
    --ac-litefx-gradient-to: rgba(255, 255, 64, 0.1);
    --ac-litefx-gradient-angle: 45;
    --ac-litefx-blend-mode: multiply;
    --ac-litefx-blend-strength: 0.3;
    --ac-litefx-outline-color: rgba(255, 255, 255, 1);
    --ac-litefx-outline-thickness: 1.0;
    --ac-litefx-outline-opacity: 0.6;
    --ac-litefx-glow-color: rgba(90, 200, 255, 1);
    --ac-litefx-glow-strength: 0.22;
    --ac-litefx-glow-spread: 1.2;
    --ac-litefx-blur-radius: 1.0;
    --ac-litefx-blur-strength: 0.25;
    --ac-litefx-dissolve-amount: 0.15;
    --ac-litefx-dissolve-edge-width: 0.08;
    --ac-litefx-dissolve-edge-color: rgba(255, 160, 64, 1);
    --ac-litefx-glitch-intensity: 0.2;
    --ac-litefx-glitch-jitter: 0.45;
    --ac-litefx-glitch-color-shift: 0.35;
    --ac-litefx-glitch-scanline-strength: 0.25;
}
```

## 値の目安

このパッケージは、数値を見ただけで挙動を予測しやすいように、公開レンジを `0..1` に揃えています。

- `Brightness`: `0..1`。`0.5` がニュートラルで、`0` で暗く、`1` で明るくなります
- `Contrast`: `0..1`。`0.5` がニュートラルで、`0` で弱く、`1` で強くなります
- `Saturation`: `0..1`。`0.5` がニュートラルで、`0` で無彩色、`1` で彩度強めになります
- `Multiply`: `Color.white` がニュートラルです
- `Add`: `Color.clear` がニュートラルです
- `Blend Strength`: `0..1`。`0` で無効、`1` で最大です
- `Outline Thickness`: `0..4`。小さな値で使う前提の疑似輪郭です
- `Glow Spread`: `0..4`。広げ過ぎず、輪郭共有の軽量発光です
- `Blur Radius`: `0..3`。少数サンプルの疑似ブラーなので大きい値には向きません
- `Dissolve Amount`: `0..1`。`0` で表示、`1` に近いほど消えます
- `Glitch Intensity`: `0..1`。低い値で短い揺れを足す用途を想定しています

サンプルも、この値の意味がそのまま見えるようにしています。`0.5` を中心に動かすと、効果の強弱を直感的に追えます。

## 制約

- v1 は矩形領域を前提に描画します
- 角丸や複雑なマスクには未対応です
- テキストグリフそのものの加工ではなく、要素の矩形描画レイヤーに対する効果です
- `backgroundImage` の元画像を動的に差し替えた場合は `LiteEffectHandle.Refresh()` または再設定を呼ぶと確実です
- v1 の Tween API は `SetEase` `SetDelay` `OnComplete` `Append` `Join` `Kill` に限定しています
- `Glow` は bloom ではなく、要素外周へはみ出す軽量な発光オーバーレイです
- `Blur` は背景を本当にぼかすガラス表現ではなく、要素内容をやわらかくする近似です
- `Glitch` は強度がある間だけ再描画を継続するため、常用は低めの値を推奨します

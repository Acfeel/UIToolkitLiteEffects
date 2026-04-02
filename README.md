# UIToolkitLiteEffects

Unity UI Toolkit 向けの軽量エフェクトパッケージです。`VisualElement` や `backgroundImage` を持つ要素に対して、色調補正、グラデーション、簡易ブレンドをコード主導で適用できます。

## 特徴

- `VisualElement` 拡張メソッドで即座に適用できる
- UI Toolkit の `backgroundImage` にも対応
- USS カスタムプロパティを併用できる
- 内部ではカスタムシェーダーを使ってエフェクト済みテクスチャを生成する
- AI から扱いやすいフラットな API
- `AnimateColorAdjust` `AnimateGradient` `AnimateLiteEffect` のチェーン式 Tween に対応

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

サンプルも、この値の意味がそのまま見えるようにしています。`0.5` を中心に動かすと、効果の強弱を直感的に追えます。

## 制約

- v1 は矩形領域を前提に描画します
- 角丸や複雑なマスクには未対応です
- テキストグリフそのものの加工ではなく、要素の矩形描画レイヤーに対する効果です
- `backgroundImage` の元画像を動的に差し替えた場合は `LiteEffectHandle.Refresh()` または再設定を呼ぶと確実です
- v1 の Tween API は `SetEase` `SetDelay` `OnComplete` `Append` `Join` `Kill` に限定しています

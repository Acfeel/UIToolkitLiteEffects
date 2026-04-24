# UIToolkitLiteEffects

Unity UI Toolkit 向けの軽量ビジュアルエフェクトパッケージです。`VisualElement` や `backgroundImage` を持つ要素に対して、色調補正、着色、グラデーション、簡易ブレンド、疑似アウトライン、グロー、疑似ブラー、ディゾルブ、グリッチをコードまたは USS カスタムプロパティから適用できます。

## 注意

`UnityEngine.UIElements.Image` 要素には対応していません。

正式なサポート対象は背景描画ベースの要素のみです。画像にエフェクトを適用したい場合は、`backgroundImage` を持つ `VisualElement` を使用してください。

このパッケージは開発中です。今後の更新で API や挙動に破壊的な変更が入る可能性があります。

## 特徴

- `VisualElement` 拡張メソッドでそのまま使えます
- 各エフェクトを単体で適用できます
- 必要になったら `LiteEffectSettings` で一括指定に移行できます
- USS カスタムプロパティだけで制御する使い方にも対応しています
- Tween API で軽いアニメーションをチェーンできます
- 値の公開レンジは基本 `0..1` に寄せてあり、数値の意味を把握しやすい設計です

## 導入方法

1. Unity の `Window > Package Manager` を開きます
2. `Install package from git URL...` を選びます
3. 次の URL を貼り付けます

```text
https://github.com/Acfeel/UIToolkitLiteEffects.git
```

## 基本的な使い方

```csharp
using Acfeel.UIToolkitLiteEffects;
using UnityEngine.UIElements;

var icon = root.Q<VisualElement>("Icon");

icon.SetGlow(new GlowSettings
{
    Strength = 0.25f,
    Spread = 0.85f
});
```

## エフェクト単体で使う

`SetColorAdjust` `SetGradient` `SetOutline` `SetGlow` `SetBlur` `SetDissolve` `SetGlitch` を使うと、必要なエフェクトだけを個別に指定できます。

### ColorAdjust

```csharp
icon.SetColorAdjust(new ColorAdjustSettings
{
    Brightness = 0.55f,
    Contrast = 0.6f,
    Saturation = 0.62f
});
```

### Gradient

`Mode` を省略した場合は `LiteEffectBlendMode.Replace` が使われ、グラデーション色でそのまま置き換えます。

```csharp
icon.SetGradient(new GradientSettings
{
    From = Color.cyan,
    To = Color.blue,
    Strength = 1f,
    Angle = 90f
});
```

### Outline

```csharp
icon.SetOutline(new OutlineSettings
{
    Color = Color.white,
    Thickness = 1.0f,
    Opacity = 0.6f
});
```

### Glow

```csharp
icon.SetGlow(new GlowSettings
{
    Color = new Color(0.35f, 0.8f, 1f, 1f),
    Strength = 0.2f,
    Spread = 0.9f
});
```

### Blur

```csharp
icon.SetBlur(new BlurSettings
{
    Radius = 0.8f,
    Strength = 0.25f
});
```

### Dissolve

```csharp
icon.SetDissolve(new DissolveSettings
{
    Amount = 0.4f,
    EdgeWidth = 0.08f,
    EdgeColor = Color.clear
});
```

### Glitch

```csharp
icon.SetGlitch(new GlitchSettings
{
    Intensity = 0.3f,
    Jitter = 0.45f,
    ColorShift = 0.35f,
    ScanlineStrength = 0.25f
});
```

### Colorize

元の画像のアルファを保持しつつ、RGB を指定した一色に置き換えます。Strength=1 で完全に一色になり、0 に近づくほど元の色に戻ります。

```csharp
icon.SetColorize(new ColorizeSettings
{
    Color = Color.red,
    Strength = 1f
});
```

### 明示的に無効化したい場合

各エフェクト設定には `Enabled` があります。値が入ると自動で有効化される設計ですが、明示的に切りたい場合は `Enabled = false` を指定できます。

```csharp
icon.SetGlow(new GlowSettings
{
    Enabled = false
});
```

### クリア

```csharp
icon.ClearLiteEffect();
```

## 一括指定で使う

複数エフェクトをまとめて設定したい場合は `SetLiteEffect` を使います。

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
        Strength = 1f,
        Angle = 35f
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
        Spread = 0.9f
    },
    Blur = new BlurSettings
    {
        Radius = 0.8f,
        Strength = 0.25f
    },
    Dissolve = new DissolveSettings
    {
        Amount = 0.15f,
        EdgeWidth = 0.08f,
        EdgeColor = Color.clear
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

使い分けの目安は次の通りです。

- まずは 1 つの効果だけ付けたい: `SetGlow` などの単体 API
- 複数の効果をまとめて管理したい: `SetLiteEffect`
- すでに適用済みの要素を再描画したい: `LiteEffectHandle.Refresh()`

## USS カスタムプロパティだけで使う

USS カスタムプロパティだけで制御したい場合は、まず `EnableLiteEffectFromUss()` を呼びます。

```csharp
icon.EnableLiteEffectFromUss();
```

そのうえで `--uitoolkitlitefx-*` の接頭辞を使って指定します。

```css
#Icon {
    --uitoolkitlitefx-brightness: 0.54;
    --uitoolkitlitefx-contrast: 0.58;
    --uitoolkitlitefx-saturation: 0.62;
    --uitoolkitlitefx-gradient-from: rgba(255, 128, 64, 0.55);
    --uitoolkitlitefx-gradient-to: rgba(255, 255, 64, 0.1);
    --uitoolkitlitefx-gradient-angle: 45;
    --uitoolkitlitefx-gradient-mode: replace;
    --uitoolkitlitefx-gradient-strength: 1;
    --uitoolkitlitefx-outline-color: rgba(255, 255, 255, 1);
    --uitoolkitlitefx-outline-thickness: 1.0;
    --uitoolkitlitefx-outline-opacity: 0.6;
    --uitoolkitlitefx-glow-color: rgba(90, 200, 255, 1);
    --uitoolkitlitefx-glow-strength: 0.22;
    --uitoolkitlitefx-glow-spread: 0.9;
    --uitoolkitlitefx-blur-radius: 1.0;
    --uitoolkitlitefx-blur-strength: 0.25;
    --uitoolkitlitefx-dissolve-amount: 0.15;
    --uitoolkitlitefx-dissolve-edge-width: 0.08;
    --uitoolkitlitefx-dissolve-edge-color: rgba(255, 160, 64, 1);
    --uitoolkitlitefx-glitch-intensity: 0.2;
    --uitoolkitlitefx-glitch-jitter: 0.45;
    --uitoolkitlitefx-glitch-color-shift: 0.35;
    --uitoolkitlitefx-glitch-scanline-strength: 0.25;
    --uitoolkitlitefx-colorize-color: rgba(255, 0, 0, 1);
    --uitoolkitlitefx-colorize-strength: 1.0;
}
```

`SetLiteEffect(new LiteEffectSettings())` でも同様に動作しますが、USS だけで制御する場合は `EnableLiteEffectFromUss()` のほうが用途に合っています。

## Tween

更新は `VisualElement.schedule` ベースで、`Coroutine` は使いません。作成した時点で自動再生されるので `Play` は不要です。

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
    .OnComplete(() => Debug.Log("LiteEffect tween completed."));
```

v1 の Tween API は `SetEase` `SetDelay` `OnComplete` `Append` `Join` `Kill` に限定しています。

### Tween の補足挙動

- `Append` / `Join` に渡した Tween は、その時点で単独再生キューから外れ、連結先 sequence の一部として再生されます
- Tween を panel 未接続の `VisualElement` に対して作成した場合でも、要素が panel にアタッチされたあと自動で再生されます
- `LiteEffectTween.Kill()` と `LiteEffectSequence.Kill()` は、そのハンドルに対応する Tween だけを停止します。他の並行 Tween は停止しません

## 値の目安

このパッケージは、数値を見ただけで挙動を予測しやすいように、基本の公開レンジを `0..1` に揃えています。一部の角度や補助値は別レンジです。

- `Brightness`: `0..1`。`0.5` がニュートラルで、`0` で暗く、`1` で明るくなります
- `Contrast`: `0..1`。`0.5` がニュートラルで、`0` で弱く、`1` で強くなります
- `Saturation`: `0..1`。`0.5` がニュートラルで、`0` で無彩色、`1` で彩度強めになります
- `Multiply`: `Color.white` がニュートラルです
- `Add`: `Color.clear` がニュートラルです
- `Gradient Mode`: 省略時は `replace` で、グラデーション色に置き換えます
- `Gradient Strength`: `0..1`。`0` で無効、`1` で最大です
- `Outline Thickness`: `0..1`。`0` で無効、`1` で最も太くなります
- `Glow Spread`: `0..1`。`0` で無効、`1` で最も広がります
- `Blur Radius`: `0..1`。`0` で無効、`1` で最も強く広がります
- `Dissolve Amount`: `0..1`。`0` で表示、`1` に近いほど消えます
- `Dissolve Edge Width`: `0..1`。消え際のなめらかさを調整します
- `Dissolve EdgeColor`: `Color.clear` なら縁色を出さず、そのまま透明に抜けます
- `Glitch Intensity`: `0..1`。低い値で短い揺れを足す用途を想定しています

見た目確認用の目安としては次のくらいから始めると調整しやすいです。

- `Outline Thickness`: `0.25`, `0.5`, `0.75`, `1.0`
- `Glow Spread`: `0.25`, `0.5`, `0.75`, `1.0`
- `Blur Radius`: `0.2`, `0.5`, `0.8`, `1.0`

## 使い分けのヒント

- まずは 1 つの効果だけ試したい場合は、`SetGlow` などの単体 API がわかりやすく扱えます
- 複数効果をまとめて管理したい場合は、`SetLiteEffect` にまとめると見通しがよくなります
- 各エフェクトは `Enabled` を省略した場合でも、値が入ると自動で有効化されます
- USS だけで制御したい場合は、`EnableLiteEffectFromUss()` を呼んでからカスタムプロパティを指定します

## 副作用と既知の挙動

- エフェクト適用中は、内部的に `unityBackgroundImageTintColor` や `backgroundColor` を退避して描画用に上書きすることがあります
- 見た目のトラブルを調べるときは、LiteEffect が背景色や背景画像 tint を一時的に制御している前提で確認すると原因を追いやすくなります
- `Glow` や `Outline` が有効な間は、見切れを防ぐために周辺描画領域の扱いも内部で調整します
- `backgroundImage` の元画像を動的に差し替えた場合は `LiteEffectHandle.Refresh()` または再設定を呼ぶと確実です
- `Glitch` は強度がある間だけ再描画を継続するため、通常利用では低めの値を推奨します

## 制約

- 角丸には正式対応していません
- `Glow` は bloom ではなく、要素外周へはみ出す軽量な発光オーバーレイです
- `Blur` は背景を本当にぼかすガラス表現ではなく、要素内容をやわらかくする近似です

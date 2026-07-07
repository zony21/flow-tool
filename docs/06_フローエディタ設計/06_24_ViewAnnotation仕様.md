# 06_24_ViewAnnotation仕様

## 1. 目的

フロー図上に配置する画像・吹き出しなどの表示補助要素を定義する。

本機能は人間向けの説明性向上を目的とし、AI Flow Designerの構造解析対象には含めない。

## 2. 基本方針

AI Flow DesignerのSSOTは以下である。

- Flow
- Lane
- Stage
- Node
- Link
- Comment
- Metadata

画像・吹き出しは設計情報ではなくView Annotationとして扱う。

つまり、以下には利用しない。

- 処理順判断
- 責務判断
- AI DSL生成
- Mermaid生成
- API設計生成
- DB影響解析

## 3. View Annotation分類

### Image Annotation

用途:

- 設備写真
- 画面イメージ
- 補足画像
- レイアウト説明

保持情報例:

```ts
interface FlowImageAnnotation {
  imageId: string
  flowId: string
  fileName: string
  filePath: string
  x: number
  y: number
  width: number
  height: number
  opacity: number
  zIndex: number
}
```

## 4. Balloon Annotation

用途:

- 補足説明
- 注意事項
- 人間向けコメント

保持情報例:

```ts
interface FlowBalloonAnnotation {
  balloonId: string
  flowId: string
  text: string
  x: number
  y: number
  width: number
  height: number
  direction: string
  targetNodeId?: string | null
  zIndex: number
}
```

Nodeとの紐付けは任意。

## 5. Canvas表示

表示レイヤー順:

```text
工程表背景
↓
画像Annotation
↓
Node / Link
↓
吹き出しAnnotation
↓
選択UI
```

画像・吹き出しは自由配置可能。

Lane / Stage所属は不要。

## 6. 編集機能

画像:

- 追加
- 移動
- サイズ変更
- 削除
- 表示順変更

吹き出し:

- 追加
- テキスト編集
- 移動
- サイズ変更
- 向き変更
- 削除

## 7. Export仕様

| 出力 | Image | Balloon |
| --- | --- | --- |
| Mermaid | 対象外 | 対象外 |
| AI DSL | 対象外 | 対象外 |
| AI解析JSON | 対象外 | 対象外 |
| PDF | オプション指定 | オプション指定 |

## 8. PDF出力

PDF出力時に確認する。

```text
PDF出力設定

[ ] 画像を含める
[ ] 吹き出しを含める
```

設定ONの場合のみPDF描画へ含める。

## 9. 禁止事項

- Annotationから処理意味を推測しない
- Mermaidへコメント変換しない
- AI DSLへ混入しない
- Node/Link生成に利用しない

## 10. 完了条件

- フロー構造と表示補助が分離されている
- PDFのみ任意反映できる
- AI解析精度へ影響しない

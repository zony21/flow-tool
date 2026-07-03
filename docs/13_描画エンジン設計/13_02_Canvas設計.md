# 13_02_Canvas設計

## 1. 目的

本書は、AI Flow DesignerのCanvas設計を定義する。

Canvasは、Lane / Stage / Node / Link / Commentを表示・編集する中央領域である。Canvasは編集体験を提供するが、保存の正はSSOTである。

## 2. 基本方針

- Canvasは無限平面に近い編集領域として扱う。
- Viewportで表示範囲を管理する。
- Zoom / Panをサポートする。
- Grid / Snapを描画補助として扱う。
- Canvas座標とScreen座標を分離する。
- Canvas状態をSSOTへ混ぜない。

## 3. 座標系

Canvasでは以下の座標系を扱う。

| 座標 | 内容 |
| --- | --- |
| Screen Coordinate | ブラウザ画面上の座標 |
| Canvas Coordinate | Flow内の論理座標 |
| Node Local Coordinate | Node内部の相対座標 |

NodeのpositionはCanvas Coordinateで保持する。

## 4. Viewport

Viewportは現在の表示範囲を表す。

```ts
interface CanvasViewport {
  x: number;
  y: number;
  zoom: number;
}
```

ViewportはeditorStoreで保持する。
SSOTへ保存しない。

## 5. Zoom

Zoom範囲:

- min: 0.25
- max: 2.0
- default: 1.0

操作:

- Toolbar Zoom In / Out
- Mouse Wheel + modifier
- Fit View
- Reset Zoom

## 6. Pan

PanはCanvas表示位置を移動する。

操作:

- Space + Drag
- Middle Mouse Drag
- Vue Flow標準Pan

Pan状態はViewportに反映するが、保存対象ではない。

## 7. Grid

Gridは配置補助である。

初期値:

- gridVisible: true
- gridSize: 16

Gridは表示補助であり、SSOTの業務意味ではない。

## 8. Canvas Layer

Canvasはレイヤ構造で考える。

```text
Background Layer
Grid Layer
Lane / Stage Layer
Link Layer
Node Layer
Comment Layer
Selection Layer
Guide Layer
Overlay Layer
```

## 9. Fit View

Fit Viewは表示対象全体をViewport内へ収める。

対象:

- Node
- Comment
- Lane / Stage範囲

大量データでも重くならないよう、bounding box計算は必要時のみ行う。

## 10. Canvas状態

保存しない状態:

- zoom
- pan
- selected
- hovered
- dragging
- connecting

保存する状態:

- Node position
- Node size
- Comment position
- Lane / Stage order

## 11. テスト観点

- Screen座標をCanvas座標へ変換できる。
- Zoom変更でNode論理座標が変わらない。
- Pan変更が保存Requestへ混入しない。
- Fit Viewで全Nodeが表示範囲へ入る。
- Grid表示切替がSSOTに影響しない。

## 12. 完了条件

- Canvas座標、Viewport、Zoom、Panの方針が定義されている。
- Canvas状態とSSOTの境界が明確である。
- 実装者がCanvas基盤を実装できる。

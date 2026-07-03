# 13_16_MiniMap設計

## 1. 目的

本書は、AI Flow DesignerのMiniMap設計を定義する。

MiniMapは、大きなFlow全体を俯瞰し、現在のViewport位置を把握しやすくする補助UIである。

## 2. 基本方針

- MiniMapは表示補助でありSSOTではない。
- Node / Link / Lane / Stageの簡易形状を表示する。
- 現在Viewportを枠で表示する。
- MiniMap操作でCanvas Viewportを移動できる。
- 大規模Flowでは簡略表示する。

## 3. 表示対象

- Flow全体Bounds
- Node簡易矩形
- Lane / Stage境界
- Current Viewport
- 選択中Nodeの位置

## 4. 表示位置

初期方針:

- Canvas右下にFloating表示
- 折りたたみ可能
- Toolbarから表示切替可能

## 5. Viewport同期

CanvasのViewport変更時にMiniMapのViewport枠を更新する。
MiniMap上のDragでCanvas Viewportを移動できる。

## 6. 性能方針

MiniMapは詳細描画しない。

- Nodeは矩形のみ
- Linkは省略または簡略線
- Commentは省略可
- 画像サムネイルは表示しない

## 7. テスト観点

- Canvas移動でMiniMap Viewportが更新される。
- MiniMap操作でCanvas Viewportが移動する。
- 大規模FlowでもMiniMap描画が重くならない。

## 8. 完了条件

- MiniMapの表示対象、位置、Viewport同期、性能方針が定義されている。
- 実装者がMiniMapを実装できる。

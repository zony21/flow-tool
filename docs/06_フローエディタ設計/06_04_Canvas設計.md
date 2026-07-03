# 06_04_Canvas設計

## 1. 本書の目的

Canvasの設計仕様を定義する。
CanvasはNode、Link、Commentを視覚的に配置・編集する中心領域である。

## 2. 基本方針

CanvasはVue Flowで実装する。
表示は構造化データの投影結果であり、保存上の正はFlowVersion配下のデータである。

## 3. 対応操作

- Node表示
- Node移動
- Node複数選択
- Link作成
- Link選択
- Comment配置
- Zoom
- Pan
- Grid表示
- Snap

## 4. 座標管理

Nodeはx、y、width、heightを保持する。
Lane/Stageとの関係は座標だけで判定せず、laneId、stageIdを明示的に保持する。

## 5. Link表示

初期接続線はBezierとする。
将来Straight、Step、SmoothStepへ切替可能とする。

## 6. ループ

ループ接続を許可する。
自己参照や戻り線は構造として保存できる。

## 7. エラー処理

不正なLink、存在しないNode参照、座標不正は保存前Validationで検出する。

## 8. テスト観点

- Nodeを配置できること
- Linkを作成できること
- ループ接続できること
- 再読込後に座標が復元されること

## 9. 完了条件

CanvasでFlowVersionの構造を視覚的に編集できること。

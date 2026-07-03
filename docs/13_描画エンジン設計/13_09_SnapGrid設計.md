# 13_09_SnapGrid設計

## 1. 目的

本書は、AI Flow DesignerのSnap / Grid設計を定義する。

Snap / Gridは、Canvas上でNodeやCommentを整列しやすくするための編集補助機能である。SSOTの業務意味ではないが、見やすい設計図を作成するために重要である。

## 2. 基本方針

- Gridは表示補助とする。
- Snapは配置補助とする。
- Grid表示状態はSSOTへ保存しない。
- Snap有効状態はユーザー設定またはeditorStoreで管理する。
- Node positionはSnap後のCanvas座標として保存する。

## 3. Grid設定

初期値:

```text
gridVisible = true
gridSize = 16
```

将来設定:

- gridSize変更
- Grid色変更
- SubGrid表示
- Dot Grid / Line Grid切替

## 4. Snap設定

初期値:

```text
snapEnabled = true
snapSize = 16
```

Snap対象:

- Node position
- Comment position
- Node resize
- Stage width
- Lane height

## 5. Snap計算

基本式:

```ts
snapped = Math.round(value / snapSize) * snapSize;
```

Node移動時はx / yをSnapする。
Node resize時はwidth / heightをSnapする。

## 6. Guide Line

将来、他Nodeとの整列補助線を表示する。

候補:

- 左端揃え
- 右端揃え
- 中央揃え
- 上端揃え
- 下端揃え
- 同幅
- 同高

## 7. Snap無効化

Altキー押下中は一時的にSnapを無効化できるようにする。

ToolbarからSnapのON / OFFも切り替え可能にする。

## 8. Lane / Stageとの関係

SnapはLane / Stage境界にも反応できるようにする。

例:

- NodeをStage左端へ吸着
- NodeをLane上端へ吸着
- Stage幅をGridへ吸着

## 9. 禁止事項

- Snap状態をFlow構造へ保存する。
- Drag中に毎回保存Requestを生成する。
- SnapによりNodeのLane / Stage所属を変更不能にする。
- Gridを業務上の工程境界として扱う。

## 10. テスト観点

- Snap有効時にNode座標がGridへ丸められる。
- Snap無効時に自由配置できる。
- Altキーで一時Snap無効化できる。
- Grid表示切替がSSOTへ影響しない。
- Resize時にwidth / heightがSnapされる。

## 11. 完了条件

- GridとSnapの責務が定義されている。
- Snap計算、対象、無効化条件が定義されている。
- 実装者がSnap / Grid機能を実装できる。

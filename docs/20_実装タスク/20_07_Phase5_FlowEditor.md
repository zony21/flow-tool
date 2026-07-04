# 20_07_Phase5_FlowEditor

## 1. 目的

Vue Flowを利用して、SSOT構造化データを編集するFlow Editorを実装する。

## 2. Task一覧

| Task ID | Task Name | 優先度 |
| --- | --- | --- |
| P5-001 | FlowCanvas基本表示 | A |
| P5-002 | Node Palette実装 | A |
| P5-003 | Node追加実装 | A |
| P5-004 | Link追加実装 | A |
| P5-005 | Property Panel実装 | A |
| P5-006 | Lane / Stage表示 | B |
| P5-007 | Viewer読み取り専用 | A |
| P5-008 | 保存連携 | A |

## 3. P5-001 FlowCanvas基本表示

目的:

Vue FlowでCanvasを表示し、Flow StoreのNode/Linkを描画する。

実装内容:

- FlowCanvas.vue作成
- Vue Flow導入
- nodes / edges反映
- zoom / pan有効化

関連設計:

- 06_フローエディタ設計
- 13_描画エンジン設計

テスト観点:

- Flow取得後にNode/Linkが表示される

完了条件:

- BasicFlowがCanvasに表示される

## 4. P5-002 Node Palette実装

目的:

初期Node種別を追加できるPaletteを作成する。

実装内容:

- Start
- End
- Process
- Decision
- Hexagon
- Image
- Comment

関連設計:

- 06_フローエディタ設計

テスト観点:

- Editorには表示、Viewerには追加不可

完了条件:

- PaletteからNode種別を選択できる

## 5. P5-003 Node追加実装

目的:

PaletteからCanvasへNodeを追加する。

実装内容:

- Node ID採番
- type設定
- label初期値
- position設定
- Flow Store更新

関連設計:

- 06_フローエディタ設計
- 09_AI構造化データ設計

テスト観点:

- Node追加でStore更新
- Viewerは追加不可

完了条件:

- Node追加後、保存対象データに含まれる

## 6. P5-004 Link追加実装

目的:

Node間を接続し、Link構造化データを作成する。

実装内容:

- sourceNodeId
- targetNodeId
- linkType=bezier
- Decision条件入力準備
- Loop接続許可

関連設計:

- 06_フローエディタ設計
- 13_描画エンジン設計

テスト観点:

- Link追加でStore更新
- Loop Linkが作成できる

完了条件:

- Node間接続が保存対象データに含まれる

## 7. P5-005 Property Panel実装

目的:

選択Node / Link / Commentの詳細を編集する。

実装内容:

- Node label編集
- Node description編集
- Link condition編集
- Comment text編集
- readonly制御

関連設計:

- 03_画面設計
- 06_フローエディタ設計

テスト観点:

- Editorは編集可
- Viewerはreadonly

完了条件:

- Property変更がFlow Storeに反映される

## 8. P5-006 Lane / Stage表示

目的:

LaneとStageをCanvas上に表示する。

実装内容:

- Lane Header
- Stage Header
- Node所属Lane/Stage
- 表示ON/OFF設定反映

関連設計:

- 06_フローエディタ設計
- 18_設定設計

テスト観点:

- Lane/StageとNodeの対応が分かる

完了条件:

- Lane/StageがCanvasに表示される

## 9. P5-007 Viewer読み取り専用

目的:

ViewerがFlowを閲覧できるが編集できない状態を実装する。

実装内容:

- Node移動不可
- Node追加不可
- Link追加不可
- Save非活性
- Property readonly

関連設計:

- 17_画面制御設計
- 19_権限テスト仕様

テスト観点:

- Viewerは編集できない
- ViewerはExportできる

完了条件:

- Viewerで編集操作が不可になる

## 10. P5-008 保存連携

目的:

Flow Storeの構造化データをBackendへ保存する。

実装内容:

- Save Button
- saveFlow API呼び出し
- 保存成功Toast
- Validation Error表示
- 403時Permission再取得

関連設計:

- 05_API設計
- 17_画面制御設計

テスト観点:

- 保存後再読込で同じFlowが表示される

完了条件:

- Editorで編集したFlowを保存できる

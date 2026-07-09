# 05_06_Template_Export_API設計

## 1. 目的

Template APIとExport APIの詳細仕様を定義する。
Templateは構造化データの雛形、ExportはFlowVersionから成果物を生成する処理である。

Transport Flowでは、搬送表出力をExportの一種として扱う。

重要:

Single Source of TruthはFlow構造であり、生成された搬送表を直接編集して正としない。

## 2. Template一覧取得

GET /api/templates

Template一覧を返す。
カテゴリ、説明、含まれるLane数、Stage数、Node数を含める。

TemplateがTransport用の場合はflowType=TRANSPORTを含める。

## 3. Template詳細取得

GET /api/templates/{templateId}

Templateに含まれるLane、Stage、Node、Link、Commentを返す。

Transport Templateの場合はNodeのTransport属性も返す。

## 4. TemplateからFlow作成

POST /api/projects/{projectId}/flows/from-template

TemplateをもとにFlowと初期FlowVersionを作成する。
ID再採番と参照張替えを行う。

Transport Templateの場合はTransport Flowとして作成する。

## 5. Mermaid出力

POST /api/flow-versions/{flowVersionId}/export/mermaid

FlowVersionの構造化データからMermaid flowchartまたはsequenceDiagramを生成する。

## 6. PDF出力

POST /api/flow-versions/{flowVersionId}/export/pdf

設計書として読みやすいPDFを生成する。

## 7. JSON出力

POST /api/flow-versions/{flowVersionId}/export/json

Project、Flow、FlowVersion、Lane、Stage、Node、Link、Commentを含むJSONを出力する。

Transport Flowの場合はNodeのTransport属性も含める。

## 8. 搬送表出力

POST /api/flow-versions/{flowVersionId}/export/transport-table

Transport Flowのみ利用可能とする。

## 8.1 搬送パターン単位

搬送表はFlow全体で1表ではなく、Lane単位で生成する。

定義:

- Lane = 搬送パターン
- Node = 搬送パターン内の処理手順
- Link = 処理順序
- Stage = 工程・設備・担当などの分類

例:

Lane:
A1 → A2 実空PL搬送

生成:

# A1 → A2 実空PL搬送

|No|処理|動作|ロケ|設備|R/W|処理区分|
|-|-|-|-|-|-|-|
|1|A1占有ON|-|-|-|-|DB更新|
|2|SH1開指令|-|-|SH1|Write|PLC書込|
|3|A1荷上げ|Loading|A1|-|-|荷上げ|

## 8.2 生成仕様

1. FlowTypeがTRANSPORTであることを確認する
2. Flow内のLaneを取得する
3. Laneごとに搬送パターンを生成する
4. Lane配下Nodeを取得する
5. LinkをたどってLane内Node実行順を決定する
6. Lane単位でNoを1から採番する
7. Node名を処理欄へ出力する
8. command_idから動作欄を出力する
9. location_idからロケ欄を出力する
10. equipment_idから設備欄を出力する
11. rw_typeからR/W欄を出力する
12. commandまたはrw_typeから処理区分を判定する

## 8.3 出力項目

| 項目 | 内容 |
| --- | --- |
| 搬送パターン名 | Lane名 |
| No | Lane内実行順 |
| 処理 | Node表示名 |
| 動作 | Transport Command |
| ロケ | Location |
| 設備 | Equipment |
| R/W | ReadまたはWrite |
| 処理区分 | CommandまたはR/Wから判定 |

初期対応はMarkdownとする。
将来的にExcel、PDFへ拡張する。

## 9. 出力履歴

GET /api/flow-versions/{flowVersionId}/exports

EXPORT_HISTORYを取得する。

## 10. エラー

- 404: TemplateまたはFlowVersionが存在しない
- 400: 出力条件が不正
- 422: Transport Flowではないため搬送表を出力できない
- 500: 生成失敗

## 11. テスト観点

- Template適用時にIDが再採番されること
- Linkのfrom/toが新Node IDへ張り替わること
- Mermaid、PDF、JSONが生成できること
- Transport FlowでLane単位の搬送表を生成できること
- Normal Flowで搬送表出力が拒否されること
- 出力成功時に履歴が残ること

## 12. 完了条件

TemplateからFlowを生成でき、FlowVersionから成果物を生成できること。

Transport Flowの場合はLane単位で搬送パターン表を生成できること。

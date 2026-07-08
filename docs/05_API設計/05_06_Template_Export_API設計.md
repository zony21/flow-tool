# 05_06_Template_Export_API設計

## 1. 目的

Template APIとExport APIの詳細仕様を定義する。
Templateは構造化データの雛形、ExportはFlowVersionから成果物を生成する処理である。

Transport Flowでは、搬送表出力をExportの一種として扱う。

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

### 生成仕様

1. FlowTypeがTRANSPORTであることを確認する
2. LinkをたどってNode実行順を決定する
3. 実行順にNoを採番する
4. Node名を処理欄へ出力する
5. command_idから動作欄を出力する
6. location_idからロケ欄を出力する
7. equipment_idから設備欄を出力する
8. rw_typeからR/W欄を出力する
9. commandまたはrw_typeから処理区分を判定する

### 出力項目

| 項目 | 内容 |
| --- | --- |
| No | 実行順 |
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
- Transport Flowで搬送表を生成できること
- Normal Flowで搬送表出力が拒否されること
- 出力成功時に履歴が残ること

## 12. 完了条件

TemplateからFlowを生成でき、FlowVersionから成果物を生成できること。

Transport Flowの場合は搬送表を生成できること。

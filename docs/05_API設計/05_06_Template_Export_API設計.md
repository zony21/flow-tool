# 05_06_Template_Export_API設計

## 1. 目的

Template APIとExport APIの詳細仕様を定義する。
Templateは構造化データの雛形、ExportはFlowVersionから成果物を生成する処理である。

## 2. Template一覧取得

GET /api/templates

Template一覧を返す。
カテゴリ、説明、含まれるLane数、Stage数、Node数を含める。

## 3. Template詳細取得

GET /api/templates/{templateId}

Templateに含まれるLane、Stage、Node、Link、Commentを返す。

## 4. TemplateからFlow作成

POST /api/projects/{projectId}/flows/from-template

TemplateをもとにFlowと初期FlowVersionを作成する。
ID再採番と参照張替えを行う。

## 5. Mermaid出力

POST /api/flow-versions/{flowVersionId}/export/mermaid

FlowVersionの構造化データからMermaid flowchartまたはsequenceDiagramを生成する。

## 6. PDF出力

POST /api/flow-versions/{flowVersionId}/export/pdf

設計書として読みやすいPDFを生成する。

## 7. JSON出力

POST /api/flow-versions/{flowVersionId}/export/json

Project、Flow、FlowVersion、Lane、Stage、Node、Link、Commentを含むJSONを出力する。

## 8. 出力履歴

GET /api/flow-versions/{flowVersionId}/exports

EXPORT_HISTORYを取得する。

## 9. エラー

- 404: TemplateまたはFlowVersionが存在しない
- 400: 出力条件が不正
- 500: 生成失敗

## 10. テスト観点

- Template適用時にIDが再採番されること
- Linkのfrom/toが新Node IDへ張り替わること
- Mermaid、PDF、JSONが生成できること
- 出力成功時に履歴が残ること

## 11. 完了条件

TemplateからFlowを生成でき、FlowVersionから成果物を生成できること。

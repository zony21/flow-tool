# 20_10_Phase8_Export_AI_DSL

## 1. 目的

Mermaid、PDF、JSON、AI DSLのExport機能を実装する。

ExportはSSOTである構造化データから生成する。

## 2. Task一覧

| Task ID | Task Name | 優先度 |
| --- | --- | --- |
| P8-001 | JSON Export実装 | A |
| P8-002 | Mermaid Export実装 | A |
| P8-003 | AI DSL Export実装 | A |
| P8-004 | PDF Export実装 | B |
| P8-005 | Export API実装 | A |
| P8-006 | Export Dialog実装 | B |

## 3. P8-001 JSON Export実装

目的:

構造化データをJSON形式で出力する。

実装内容:

- Project/Flow/Node/Link/Lane/Stage/Comment/Image/Metadata出力
- prettyPrint設定反映
- metadata含有設定反映

関連設計:

- 09_AI構造化データ設計
- 18_設定設計

テスト観点:

- 19_07 JSON Export

完了条件:

- JSONにSSOT構造が含まれる

## 4. P8-002 Mermaid Export実装

目的:

FlowをMermaid flowchartへ変換する。

実装内容:

- flowchart LR/TB
- Process/Decision/Start/End表現
- Link条件出力
- Loop出力

関連設計:

- 07_Mermaid出力設計

テスト観点:

- 19_07 Mermaid Export

完了条件:

- BasicFlowとDecisionFlowをMermaid出力できる

## 5. P8-003 AI DSL Export実装

目的:

AIが曖昧なく解析できる独自DSLを出力する。

実装内容:

- dslVersion
- Project
- Flow
- Lane/Stage
- Node/Link
- Decision condition
- Loop
- Responsibility
- DB/API/PLC情報
- Metadata

関連設計:

- 09_AI構造化データ設計
- 18_AI Setting
- 19_08_AI_DSLテスト仕様

テスト観点:

- BasicFlow/Decision/Loop/Responsibility

完了条件:

- AI DSL Testの優先度Aを満たす

## 6. P8-004 PDF Export実装

目的:

FlowをPDFとして出力する。

実装内容:

- PageSize
- Orientation
- Diagram出力
- Comment/Image/Metadata含有設定

関連設計:

- 08_PDF出力設計
- 18_Export Setting

テスト観点:

- 19_07 PDF Export

完了条件:

- PDFが生成され、主要情報が含まれる

## 7. P8-005 Export API実装

目的:

FrontendからExportを実行できるAPIを実装する。

実装内容:

- POST /api/projects/{projectId}/exports
- exportType指定
- Export Setting反映
- Viewer Export制御
- AuditLog記録

関連設計:

- 05_API設計
- 17_権限管理設計
- 18_設定設計

テスト観点:

- Owner/Editor/Viewer Export
- Viewer禁止時403

完了条件:

- APIから各Exportを実行できる

## 8. P8-006 Export Dialog実装

目的:

画面からExport形式を選択して実行できるようにする。

実装内容:

- Export Dialog
- Export形式選択
- Export Setting表示
- DownloadまたはPreview
- Error表示

関連設計:

- 03_画面設計
- 18_設定設計

テスト観点:

- ExportボタンからAPIが呼ばれる

完了条件:

- 画面からExportを実行できる

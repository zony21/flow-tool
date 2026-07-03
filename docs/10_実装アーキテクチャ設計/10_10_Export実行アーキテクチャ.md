# 10_10_Export実行アーキテクチャ

## 1. 目的

本書は、AI Flow DesignerにおけるExport実行アーキテクチャを定義する。

Exportは、画面表示の印刷ではない。
保存済みFlowまたはVersion Snapshotを入力とし、Mermaid、PDF、JSON、AI DSLなどの成果物を生成する処理である。

## 2. 基本方針

- Export処理はBackend側に集約する。
- 入力は保存済みFlowまたはVersion Snapshotとする。
- FrontendのCanvas DOMやVue Flow内部状態を直接入力にしない。
- Export形式ごとに専用Serviceを持つ。
- 共通前処理としてExport Input Modelを生成する。

## 3. 全体構成

```text
Frontend Export Button
  ↓
ExportRequest DTO
  ↓
ExportController
  ↓
ExportApplicationService
  ↓
Flow / Snapshot Load
  ↓
ExportInputBuilder
  ↓
Format-specific Export Service
  ↓
ExportResult
```

## 4. Export対象

| 出力形式 | 用途 |
| --- | --- |
| Mermaid | Markdownや設計書貼り付け用 |
| PDF | 人間向け正式出力 |
| JSON | SSOT外部共有 / Import候補 |
| AI DSL | AI読解用 |

## 5. Export Request

Export Requestは以下を含む。

- projectId
- flowId
- snapshotId
- exportType
- includeComments
- includeAiNotes
- includeMetadata
- layoutMode
- pageSize
- timezone

snapshotIdが指定された場合はSnapshotを優先する。
snapshotIdが未指定の場合は現在保存済みFlowを対象とする。

## 6. Export Input Model

各Export Serviceへ直接DB Entityを渡さない。
共通のExport Input Modelを作成する。

Export Input Modelに含めるもの:

- Project情報
- Flow情報
- Lane一覧
- Stage一覧
- Node一覧
- Link一覧
- Comment一覧
- Version情報
- 出力オプション

## 7. 同期・非同期方針

初期実装では同期Exportでよい。

ただし、PDFや大量Flow出力が重くなる可能性があるため、Application Serviceの境界は将来非同期ジョブへ移行しやすい形にする。

将来構成:

```text
Export Request
  ↓
Export Job Create
  ↓
Queue
  ↓
Worker
  ↓
File Storage
  ↓
Download URL
```

## 8. 権限方針

Export実行時は以下を確認する。

- ログイン済みである。
- 対象Projectへの閲覧権限がある。
- 対象FlowがProject配下である。
- 対象SnapshotがFlow配下である。

## 9. エラー方針

| エラー | 内容 |
| --- | --- |
| ExportTargetNotFound | FlowまたはSnapshotが存在しない |
| ExportPermissionDenied | 権限がない |
| ExportValidationFailed | 出力前検証に失敗 |
| ExportRendererFailed | PDF等の描画処理に失敗 |
| ExportUnsupportedType | 未対応形式 |

## 10. テスト観点

- 保存済みFlowからMermaidを出力できる。
- SnapshotからPDFを出力できる。
- Viewer権限でExportできる。
- 権限なしProjectのExportが拒否される。
- Vue Flow表示状態を変更しても未保存ならExportへ反映されない。
- includeAiNotes=falseの場合、AI専用メモが出力されない。

## 11. 完了条件

- ExportがBackend側で実行される設計である。
- Export入力が保存済みFlowまたはSnapshotに限定されている。
- 形式ごとのExport Serviceが分離されている。
- 将来非同期ジョブ化できる境界になっている。

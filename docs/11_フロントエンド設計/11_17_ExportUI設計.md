# 11_17_ExportUI設計

## 1. 目的

本書は、AI Flow DesignerのExport UI設計を定義する。

Export UIは、保存済みFlowまたはVersion Snapshotから、Mermaid、PDF、JSON、AI DSLなどの成果物を出力するための画面機能である。
Exportは画面DOMの印刷ではなく、BackendがSSOTを入力として生成する正式出力である。

## 2. 基本方針

- Export対象は保存済みFlowまたはSnapshotとする。
- 未保存変更がある場合は警告する。
- Export形式ごとに必要なOptionを切り替える。
- Export実行中は進捗またはLoadingを表示する。
- Export失敗時はtraceIdと再実行導線を表示する。
- 将来の非同期Export Jobに対応できるUI構成にする。

## 3. UI構成

```text
EditorHeader
  └─ Export Button

ExportDialog
  ├─ ExportTypeSelector
  ├─ ExportTargetSelector
  ├─ ExportOptionForm
  ├─ ExportPreviewArea
  ├─ ExportProgressArea
  └─ ExportResultArea
```

## 4. ExportDialog

### 4.1 表示タイミング

- HeaderのExportボタン押下時
- Flow保存後の導線
- Snapshot一覧から出力選択時

### 4.2 入力項目

- Export形式
- 対象Flow
- 対象Snapshot
- コメントを含めるか
- AI専用メモを含めるか
- Metadataを含めるか
- レイアウトモード
- PDFページサイズ

## 5. Export形式

| 形式 | 用途 |
| --- | --- |
| Mermaid | Markdown貼り付け、設計書補助 |
| PDF | 人間向け正式出力 |
| JSON | SSOT共有、Import候補 |
| AI DSL | AI読解・AIレビュー連携 |

## 6. ExportTypeSelector

Export形式を選択する。

表示:

- アイコン
- 形式名
- 説明
- 推奨用途

選択変更時、ExportOptionFormを切り替える。

## 7. ExportTargetSelector

Export対象を選択する。

対象:

- 現在保存済みFlow
- Version Snapshot

未保存変更がある場合:

- 「現在の未保存変更はExportに含まれません」と表示する。
- 保存してからExportする導線を表示する。

## 8. ExportOptionForm

### 8.1 共通Option

- includeComments
- includeAiNotes
- includeMetadata
- timezone

### 8.2 Mermaid Option

- direction
- showLane
- showStage
- showCondition

### 8.3 PDF Option

- pageSize
- orientation
- includeCover
- includeTableOfContents
- includeDetailSection

### 8.4 JSON Option

- prettyPrint
- includeSchemaVersion
- includeSnapshots

### 8.5 AI DSL Option

- includeResponsibility
- includeCommunication
- includeDbUpdate
- includeAiNotes

## 9. ExportPreviewArea

初期実装ではPreviewは必須ではない。

優先順:

1. Mermaid Text Preview
2. AI DSL Text Preview
3. JSON Preview
4. PDF Preview

PDF Previewは将来機能として扱う。

## 10. Export実行フロー

```text
Export Button Click
  ↓
ExportDialog Open
  ↓
Select Type / Options
  ↓
executeExport
  ↓
exportApi
  ↓
Backend Export Service
  ↓
Export Result
  ↓
Download / Copy / Preview
```

## 11. Export Result

形式別の結果:

| 形式 | 結果 |
| --- | --- |
| Mermaid | Text表示、Copy、Download |
| PDF | Download |
| JSON | Text表示、Download |
| AI DSL | Text表示、Copy、Download |

## 12. Error表示

Export失敗時:

- Dialogを閉じない。
- Error Messageを表示する。
- traceIdを表示する。
- 再実行ボタンを表示する。

## 13. 非同期Job対応

将来、Exportが重くなる場合は非同期Jobへ移行する。

UIは以下に対応できるようにする。

- Job作成
- 進捗表示
- 完了通知
- 失敗通知
- ダウンロードURL表示

## 14. 禁止事項

- Canvas DOMを直接PDF化する。
- 未保存Frontend状態を正式Export対象にする。
- Export形式ごとのOptionを混在させる。
- Export失敗時にDialogを強制的に閉じる。

## 15. テスト観点

- ExportDialogが開く。
- Export形式変更でOptionが切り替わる。
- 未保存変更時に警告が表示される。
- Mermaid出力結果をCopyできる。
- PDF出力結果をDownloadできる。
- Export失敗時にtraceIdと再実行導線が表示される。

## 16. 完了条件

- Export UIの構成が定義されている。
- 形式別Optionと結果表示が定義されている。
- 保存済みFlow / Snapshotを対象にする方針が明確である。
- 将来非同期Exportへ拡張できるUI境界がある。

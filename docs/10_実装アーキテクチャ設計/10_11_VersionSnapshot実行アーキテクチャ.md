# 10_11_VersionSnapshot実行アーキテクチャ

## 1. 目的

本書は、AI Flow DesignerにおけるVersion Snapshot実行アーキテクチャを定義する。

Version Snapshotは、Undo / Redoとは異なる。
Undo / RedoはFrontendの一時操作履歴であり、Version Snapshotは保存済みFlowの正式な履歴である。

## 2. 基本方針

- SnapshotはBackendで作成する。
- Snapshotの入力は保存済みFlowとする。
- 未保存のFrontend一時状態から直接Snapshotを作成しない。
- Snapshot作成後は、その時点のFlowを再現できるようにする。
- SnapshotはExportやAIレビューの入力に利用できる。

## 3. 実行フロー

```text
Frontend Create Snapshot
  ↓
CreateSnapshotRequest
  ↓
SnapshotController
  ↓
SnapshotApplicationService
  ↓
Load Saved Flow
  ↓
Validate Flow
  ↓
Serialize Snapshot Body
  ↓
Save Snapshot Header / Body
  ↓
Return Snapshot Response
```

## 4. Snapshot対象

Snapshotには以下を含める。

- Project情報
- Flow情報
- Lane一覧
- Stage一覧
- Node一覧
- Link一覧
- Comment一覧
- Flow Metadata
- Export関連Metadata
- Schema Version

## 5. Snapshot Header

一覧表示や検索に必要な情報はHeaderとして保持する。

項目例:

- snapshotId
- projectId
- flowId
- versionNo
- title
- description
- createdBy
- createdAt
- schemaVersion

## 6. Snapshot Body

Flow再現に必要な構造はBodyとして保持する。

初期方針ではJSON文字列として保持する。

理由:

- 当時の構造をそのまま保持しやすい。
- 将来テーブル構造が変わっても履歴再現できる。
- ExportやAIレビューの再実行に利用しやすい。

## 7. Version番号方針

Version番号はFlow単位で採番する。

例:

- v1
- v2
- v3

内部的には連番を保持し、表示上は `v{versionNo}` とする。

## 8. 保存前提

Snapshot作成前にFlow保存を要求する。

未保存変更がある場合の画面挙動:

- 「未保存の変更があります」警告を表示する。
- 保存後にSnapshot作成する導線を提供する。
- 未保存状態のままSnapshot作成しない。

## 9. 復元方針

Snapshot復元は将来機能として扱う。

ただし、設計上は以下のどちらにも対応できるようにする。

- Snapshotを現在Flowへ上書き復元する。
- Snapshotから新しいFlowを複製作成する。

復元時は、ID再採番の要否を明示する。

## 10. Export連携

Export RequestにsnapshotIdが指定された場合、現在FlowではなくSnapshotを入力とする。

これにより、過去版のPDF、Mermaid、AI DSLを再出力できる。

## 11. テスト観点

- Snapshot作成時に保存済みFlowが読み込まれる。
- 未保存Frontend状態はSnapshotへ入らない。
- Snapshot作成後に現在Flowを変更してもSnapshot内容は変わらない。
- SnapshotからPDF Exportできる。
- Flow単位でVersion番号が連番になる。

## 12. 完了条件

- SnapshotとUndo / Redoの責務が分離されている。
- Snapshot作成がBackendで実行される。
- Snapshotから過去版Exportが可能である。
- Snapshotが将来復元に耐える構造で保存される。

# 09_14_VersionSnapshot

## 1. 目的

Version Snapshotは、ある時点のFlow構造を完全に保存し、後からMermaid、PDF、JSON、AI DSLを同じ内容で再生成できるようにする仕組みである。
Undo/Redoとは責務を分離する。

## 2. Snapshot対象

- Flow基本情報
- Lane
- Stage
- Node
- Link
- Comment
- Export関連メタ情報
- schemaVersion

## 3. 保存タイミング

- ユーザーがVersion保存を実行した時
- 重要なExport前に必要に応じて保存した時
- 将来の承認フローで承認対象にした時

## 4. Undo/Redoとの違い

| 項目 | Version Snapshot | Undo/Redo |
| --- | --- | --- |
| 目的 | 履歴・再現・承認 | 編集操作の取り消し |
| 保存期間 | 永続 | セッション中心 |
| 単位 | Flow全体 | 操作Command |
| Export入力 | 使用する | 使用しない |

## 5. JSON例

```json
{
  "versionId": "VER-0001",
  "flowId": "FLOW-0001",
  "versionNo": 1,
  "schemaVersion": "1.0.0",
  "snapshotJson": {},
  "createdBy": "USER-001",
  "createdAt": "2026-07-03T00:00:00+09:00",
  "changeSummary": "初版作成"
}
```

## 6. 完了条件

保存済みVersion Snapshotから、現在DBの状態に依存せずFlowを完全復元できる。

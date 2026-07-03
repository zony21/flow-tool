# 14_15_Snapshot責務分離設計

## 1. 目的

本書は、Undo / RedoとVersion Snapshotの責務分離を定義する。

Undo / RedoはFrontend編集中の一時操作履歴であり、Version SnapshotはBackendで永続化される正式履歴である。両者を混同すると、保存、復元、Export、履歴管理の責務が破綻する。

## 2. 基本方針

- Undo / RedoはFrontend内で完結する。
- Version SnapshotはBackendで作成・保存する。
- Undo StackをSnapshotとして保存しない。
- SnapshotをUndo Stackとして使わない。
- Export対象はSnapshotまたは保存済みFlowであり、Undo履歴ではない。

## 3. 比較

| 項目 | Undo / Redo | Version Snapshot |
| --- | --- | --- |
| 管理場所 | Frontend | Backend |
| 永続化 | なし | あり |
| 粒度 | 操作単位 | Flow全体 |
| 用途 | 編集取り消し | 正式履歴 |
| Export対象 | ならない | なる |
| 復元対象 | 編集中状態 | 保存済み構造 |

## 4. Snapshot作成時

Snapshot作成はUndo Stackへ積まない。

ただし、Snapshot作成前に未保存変更がある場合は、保存確認Dialogを表示する。

## 5. Snapshot復元時

Snapshot復元はBackend ResponseによりFlow構造を再同期するため、Undo / Redo Stackはクリアする。

理由:

- 既存CommandのtargetIdが無効になる可能性がある。
- Flow構造全体が置き換わる。

## 6. 保存との関係

保存成功時は原則Undo Stackを維持する。
ただし、Backend ResponseでID再採番や構造補正が発生した場合は履歴をクリアする。

## 7. 禁止事項

- Undo StackをBackendへ送信する。
- SnapshotJsonからUndo Stackを復元する。
- Snapshot作成をUndo対象にする。
- Snapshot復元後に古いRedoを実行する。

## 8. テスト観点

- Snapshot作成でUndo Stackが増えない。
- Snapshot復元でUndo / Redo Stackがクリアされる。
- 保存成功ではUndo Stackが維持される。
- ExportがUndo履歴を参照しない。

## 9. 完了条件

- Undo / RedoとSnapshotの責務分離が明確である。
- Snapshot作成・復元時のStack扱いが定義されている。

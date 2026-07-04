# 20_08_Phase6_Version_UndoRedo

## 1. 目的

Version SnapshotとUndo/Redoを実装する。

Versionは設計履歴、Undo/Redoは操作履歴であり、責務を分離する。

## 2. Task一覧

| Task ID | Task Name | 優先度 |
| --- | --- | --- |
| P6-001 | UndoRedo Store実装 | A |
| P6-002 | Node操作Undo/Redo | A |
| P6-003 | Link操作Undo/Redo | A |
| P6-004 | Version Snapshot API | A |
| P6-005 | Version一覧UI | B |
| P6-006 | Version復元 | B |

## 3. P6-001 UndoRedo Store実装

目的:

Frontend操作履歴を管理するStoreを作成する。

実装内容:

- undoStack
- redoStack
- pushAction
- undo
- redo
- clear

関連設計:

- 14_UndoRedo設計

テスト観点:

- 操作追加でundoStackに入る
- undo後redoStackに入る

完了条件:

- 操作履歴をStoreで管理できる

## 4. P6-002 Node操作Undo/Redo

目的:

Node追加、更新、削除、移動をUndo/Redoできるようにする。

実装内容:

- AddNodeAction
- UpdateNodeAction
- DeleteNodeAction
- MoveNodeAction

関連設計:

- 14_UndoRedo設計
- 06_フローエディタ設計

テスト観点:

- Node追加後Undoで消える
- Undo後Redoで戻る

完了条件:

- Node操作がUndo/Redo可能

## 5. P6-003 Link操作Undo/Redo

目的:

Link追加、更新、削除をUndo/Redoできるようにする。

実装内容:

- AddLinkAction
- UpdateLinkAction
- DeleteLinkAction

関連設計:

- 14_UndoRedo設計

テスト観点:

- Link追加後Undoで消える

完了条件:

- Link操作がUndo/Redo可能

## 6. P6-004 Version Snapshot API

目的:

Flowの設計履歴としてSnapshotを保存する。

実装内容:

- POST /versions
- GET /versions
- Snapshot JSON保存
- 作成者、作成日時保存

関連設計:

- 14_UndoRedo設計
- 12_バックエンド設計

テスト観点:

- EditorはVersion作成可能
- ViewerはVersion作成不可

完了条件:

- Version Snapshotを作成できる

## 7. P6-005 Version一覧UI

目的:

Version一覧を画面から確認できるようにする。

実装内容:

- VersionDialog
- Version一覧表示
- 作成日時、作成者表示

関連設計:

- 03_画面設計
- 14_UndoRedo設計

テスト観点:

- 作成済みVersionが一覧に表示される

完了条件:

- Version一覧を確認できる

## 8. P6-006 Version復元

目的:

指定VersionのSnapshotからFlowを復元する。

実装内容:

- Restore API
- 復元確認Dialog
- Flow Store更新
- 復元後保存

関連設計:

- 14_UndoRedo設計

テスト観点:

- Viewerは復元不可
- 復元後Flow構造がSnapshotと一致

完了条件:

- VersionからFlowを復元できる

# 14_01_UndoRedo概要

## 1. 目的

本書は、AI Flow DesignerのUndo / Redo全体方針を定義する。

Undo / Redoは、Frontend編集中の一時操作履歴を戻す・やり直すための機能である。Backendで永続化するVersion Snapshotとは責務が異なる。

## 2. 基本方針

- Command Patternを採用する。
- 編集操作はCommandとして記録する。
- Undo StackとRedo Stackを分ける。
- 新しいCommand実行時はRedo Stackをクリアする。
- Drag中の細かい移動は履歴化しない。
- Drag完了時に1つのCommandとして記録する。
- 保存操作はUndo対象にしない。
- Version SnapshotはUndo / Redo対象にしない。

## 3. 対象操作

Undo / Redo対象:

- Node追加
- Node削除
- Node移動
- Nodeリサイズ
- Node Property更新
- Link追加
- Link削除
- Link Property更新
- Lane / Stage追加・更新・削除
- Comment追加・更新・削除・移動
- Clipboard Paste / Duplicate
- Template適用
- Layout適用

対象外:

- 保存
- Export
- AIレビュー実行
- Viewport変更
- Zoom / Pan
- Selection変更
- Grid表示切替
- Version Snapshot作成

## 4. 全体構成

```text
User Operation
  ↓
Command生成
  ↓
Command.execute()
  ↓
flowStore更新
  ↓
undoStack.push(command)
  ↓
redoStack.clear()
```

Undo:

```text
undoStack.pop()
  ↓
command.undo()
  ↓
redoStack.push(command)
```

Redo:

```text
redoStack.pop()
  ↓
command.redo()
  ↓
undoStack.push(command)
```

## 5. Store構成

undoRedoStoreで管理する。

```ts
interface UndoRedoState {
  undoStack: FlowCommand[];
  redoStack: FlowCommand[];
  isApplyingHistory: boolean;
  maxHistoryCount: number;
}
```

## 6. Snapshotとの違い

| 項目 | Undo / Redo | Version Snapshot |
| --- | --- | --- |
| 管理場所 | Frontend | Backend |
| 永続化 | しない | する |
| 用途 | 編集中の取り消し | 正式履歴 |
| 粒度 | 操作単位 | Flow全体 |
| Export対象 | ならない | なる |

## 7. 禁止事項

- Undo履歴をBackendへ保存する。
- Version SnapshotをUndo Stackとして扱う。
- Drag中のmousemoveをすべてCommand化する。
- Selection変更をUndo対象にする。
- 保存成功をUndo対象にする。

## 8. テスト観点

- Command実行後にundoStackへ追加される。
- Undoで状態が前に戻る。
- Redoで状態が再適用される。
- 新規Command実行でredoStackがクリアされる。
- 保存してもundoStackが消えない。
- Snapshot作成がundoStackへ入らない。

## 9. 完了条件

- Undo / Redoの責務が定義されている。
- Command Pattern採用方針が定義されている。
- Snapshotとの責務分離が明確である。
- 実装者がUndo / Redo基盤を実装できる。

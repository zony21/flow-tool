# 14_05_RedoStack設計

## 1. 目的

本書は、AI Flow DesignerのRedo Stack設計を定義する。

Redo Stackは、UndoされたCommandを保持し、再実行するためのFrontend一時履歴である。

## 2. 基本方針

- UndoされたCommandをredoStackへ積む。
- Redo実行時はredoStack末尾をpopする。
- Redo後のCommandはundoStackへ戻す。
- 新しいCommandが実行されたらredoStackをクリアする。
- Redo StackはBackendへ保存しない。

## 3. Redo処理

```text
if redoStack empty return
command = redoStack.pop()
isApplyingHistory = true
command.redo(context)
undoStack.push(command)
isApplyingHistory = false
```

## 4. Clear条件

redoStackをクリアする条件:

- 新規Command実行
- Flow切替
- Project切替
- Flow再読込
- Snapshot復元
- Template適用後の新規操作

UndoだけではRedo Stackをクリアしない。

## 5. 保存との関係

保存成功ではRedo Stackを無条件に削除しない。
ただし、Backend ResponseでFlowが再同期された場合は、Redo Stackの整合性が失われるためクリアする。

## 6. 禁止事項

- Redo StackをBackendへ保存する。
- 新規Command実行後もRedo Stackを残す。
- Flow再読込後に古いRedoを実行する。
- Redo実行時にCommandを新規作成し直す。

## 7. テスト観点

- Undo後にredoStackへ積まれる。
- RedoでredoStackからundoStackへ戻る。
- 新規Command実行でredoStackがクリアされる。
- Flow再読込でredoStackがクリアされる。

## 8. 完了条件

- Redo Stackのpush、pop、clear条件が定義されている。
- Undo Stackとの関係が明確である。
- 実装者がRedo Stackを実装できる。

# 14_04_UndoStack設計

## 1. 目的

本書は、AI Flow DesignerのUndo Stack設計を定義する。

Undo Stackは、実行済みCommandを保持し、直前の編集操作から順に取り消すためのFrontend一時履歴である。

## 2. 基本方針

- Command実行後にundoStackへ積む。
- Undo実行時は末尾Commandをpopする。
- undo後のCommandはredoStackへ移動する。
- StackはFrontend一時状態とし、Backendへ保存しない。
- maxHistoryCountを超えたら古い履歴から破棄する。

## 3. State

```ts
interface UndoRedoState {
  undoStack: FlowCommand[];
  redoStack: FlowCommand[];
  isApplyingHistory: boolean;
  maxHistoryCount: number;
}
```

## 4. push処理

```text
execute command
  ↓
undoStack.push(command)
  ↓
redoStack.clear()
  ↓
isDirty = true
```

## 5. undo処理

```text
if undoStack empty return
command = undoStack.pop()
isApplyingHistory = true
command.undo(context)
redoStack.push(command)
isApplyingHistory = false
```

## 6. Stack容量

初期値:

```text
maxHistoryCount = 100
```

超過時は先頭から削除する。

## 7. 保存との関係

保存成功時にundoStackを消すかは設定方針が必要である。

初期方針:

- 保存してもundoStackは維持する。
- ただし保存済み位置をlastSavedCommandIdとして記録できるようにする。

## 8. 禁止事項

- undoStackをBackendへ保存する。
- 保存成功で無条件にundoStackを破棄する。
- Selection変更をundoStackへ積む。
- Drag中の中間移動をすべて積む。

## 9. テスト観点

- Command実行後にundoStackへ積まれる。
- Undoで末尾Commandが取り消される。
- Undo後にredoStackへ移動する。
- maxHistoryCount超過時に古い履歴が破棄される。

## 10. 完了条件

- Undo Stackのpush、pop、容量、保存との関係が定義されている。
- 実装者がUndo Stackを実装できる。

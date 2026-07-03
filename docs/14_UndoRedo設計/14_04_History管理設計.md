# 14_04_History管理設計

## 1. 本書の目的

本書は、AI Flow Designer のUndo / Redo履歴管理設計を定義する。

履歴管理はCommandをUndo Stack / Redo Stackで管理し、ユーザー操作の取消・再実行を安定して行うための基盤である。

## 2. 基本方針

- 履歴はFlow単位で保持する
- Undo StackとRedo Stackを分ける
- 最大履歴件数は1000件
- 新規Command実行時にRedo Stackを破棄する
- 履歴はDB保存しない
- Flow切替時は対象Flowの履歴に切り替える
- ブラウザリロード時に履歴は失われてよい

## 3. Store構造

```ts
type CommandHistoryStoreState = {
  historiesByFlowId: Record<string, FlowCommandHistory>
  activeFlowId?: string
}

type FlowCommandHistory = {
  flowId: string
  undoStack: EditorCommand[]
  redoStack: EditorCommand[]
  maxHistoryCount: number
  isExecuting: boolean
  lastExecutedAt?: string
}
```

## 4. activeFlowId

現在編集中のFlowIdをactiveFlowIdとして保持する。

Undo / RedoはactiveFlowIdの履歴に対して実行する。

## 5. 履歴初期化

Flow読込時:

1. activeFlowIdを設定
2. historiesByFlowIdに履歴がなければ作成
3. undoStack / redoStackを空で開始

初期実装ではFlow再読込時に履歴をクリアしてよい。

## 6. Command実行

executeCommand手順:

1. isExecuting確認
2. Command.execute実行
3. undoStackへpush
4. redoStackをclear
5. maxHistoryCountを超えた場合、古い履歴を削除
6. dirty更新
7. validation更新

## 7. Undo実行

undo手順:

1. activeFlowId確認
2. undoStackが空なら終了
3. isExecuting=true
4. undoStackからpop
5. command.undo実行
6. redoStackへpush
7. dirty更新
8. isExecuting=false

## 8. Redo実行

redo手順:

1. activeFlowId確認
2. redoStackが空なら終了
3. isExecuting=true
4. redoStackからpop
5. command.redo実行
6. undoStackへpush
7. dirty更新
8. isExecuting=false

## 9. isExecuting

Undo / Redo 実行中の二重実行を防ぐ。

二重実行禁止対象:

- executeCommand
- undo
- redo
- Flow切替
- AutoSave確定処理

## 10. 履歴上限

maxHistoryCountを超えた場合、最も古いCommandを削除する。

```text
if undoStack.length > maxHistoryCount:
  undoStack.shift()
```

## 11. Redo Stack破棄条件

Redo Stackは以下で破棄する。

- 新規Command実行
- Flow再読込
- Version復元
- Template大量適用後に履歴リセットを選択した場合
- 他ユーザー変更を反映した場合

## 12. Undo / Redo 可否

```ts
type HistoryStatus = {
  canUndo: boolean
  canRedo: boolean
  undoLabel?: string
  redoLabel?: string
}
```

ツールバーやショートカットはこの状態を参照する。

## 13. Label表示

Undo / RedoメニューにはCommand labelを表示する。

例:

- 元に戻す: Node移動
- やり直す: Link追加

## 14. Flow切替

Flow切替時の初期方針:

- 未保存変更があれば確認
- 切替後、対象Flowの履歴を新規作成
- 前Flowの履歴はメモリに残してもよい
- メモリ圧迫時は破棄してよい

## 15. Version復元時

Version復元はFlow全体を大きく変更する。

初期方針:

- 復元後にUndo / Redo履歴をクリアする
- 復元前状態はFlow Versionとして保存する

理由:

- 履歴整合性が複雑になる
- 永続的な復元はVersion機能で担保する

## 16. AutoSave後の履歴

AutoSave成功後もUndo Stackは保持する。

理由:

- AutoSaveは保存処理であり、編集操作ではない
- ユーザーは保存後でも直前操作を取り消したい

## 17. 手動保存後の履歴

手動保存後もUndo Stackは保持する。

ただし保存済み位置を示す savedHistoryPointer を持つことを検討する。

初期実装ではdirty状態のみ管理する。

## 18. Memory管理

履歴にはNode/LinkのSnapshotを含むためメモリ消費が大きい。

対策:

- maxHistoryCountを設定
- 大量CommandはCompositeでまとめる
- Snapshotは必要最小限にする
- 画像本体は保持しない
- Template適用時は差分だけ保持する

## 19. 破棄タイミング

履歴破棄対象:

- ユーザーログアウト
- Project切替
- Flow明示クローズ
- メモリ逼迫
- Version復元
- 他ユーザー変更取り込み

## 20. テスト観点

- Command実行でUndo Stackへ追加される
- UndoでRedo Stackへ移動する
- RedoでUndo Stackへ戻る
- 新規Command実行でRedo Stackが消える
- maxHistoryCountを超えると古い履歴が消える
- Flowごとに履歴が分離される
- AutoSave後も履歴が残る
- Version復元で履歴がクリアされる

## 21. 完了条件

- Flow単位の履歴管理ができる
- Undo / Redo可否をUIへ提供できる
- 履歴上限と破棄条件が実装されている
- AutoSave / Versionとの関係が破綻しない

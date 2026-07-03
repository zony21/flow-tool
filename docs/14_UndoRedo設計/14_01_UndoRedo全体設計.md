# 14_01_UndoRedo全体設計

## 1. 本書の目的

本書は、AI Flow Designer における Undo / Redo 全体設計を定義する。

AI Flow Designer は、図形エディタではなく構造化データを正とするフロー設計ツールである。そのため Undo / Redo は見た目の復元ではなく、Project / Flow / Lane / Stage / Node / Link / Comment の構造化データを正しく戻すための仕組みとして設計する。

## 2. 基本方針

Undo / Redo は Command Pattern で実装する。

- すべての編集操作を Command 化する
- Command は do / undo / redo を持つ
- Vue Flow の内部履歴には依存しない
- 構造化データの差分を保持する
- 選択状態は原則 Undo / Redo 対象にしない
- AutoSaveとは独立して管理する
- Flow Versionとは別機能として管理する
- 1 Flow 単位で履歴を保持する

## 3. Undo / Redo の対象

対象とする操作:

- Node追加
- Node移動
- Nodeリサイズ
- Node削除
- Nodeプロパティ更新
- Link追加
- Link再接続
- Link削除
- Linkプロパティ更新
- Lane追加
- Lane更新
- Lane削除
- Stage追加
- Stage更新
- Stage削除
- Comment追加
- Comment移動
- Comment更新
- Comment削除
- Template適用
- 貼付
- 複製

## 4. Undo / Redo の対象外

対象外:

- 選択状態
- Hover状態
- Zoom
- Pan
- Tooltip表示
- プロパティパネル開閉
- Validation表示状態
- API通信中状態
- 編集ロック状態

ただし、Command実行後に選択状態を補助的に更新することは許可する。

## 5. 履歴単位

履歴は Flow 単位で保持する。

理由:

- Flowごとに編集内容が独立する
- 別Flowへ移動した時に履歴が混在しない
- AutoSaveやVersionの対象もFlow単位である

## 6. Undo Stack / Redo Stack

各Flowに対して以下を持つ。

```ts
type CommandHistoryState = {
  flowId: string
  undoStack: EditorCommand[]
  redoStack: EditorCommand[]
  maxHistoryCount: number
  isExecuting: boolean
}
```

## 7. 履歴上限

初期上限:

```text
maxHistoryCount = 1000
```

性能試験ではUndo 1000回を対象にする。

## 8. Redoクリア

新しいCommandを実行した場合、Redo Stackはクリアする。

例:

1. Node移動
2. Undo
3. 別Node追加
4. Redo Stackは破棄

## 9. Command実行フロー

```text
User Operation
  ↓
Create Command
  ↓
Command.execute()
  ↓
Update Editor State
  ↓
Push Undo Stack
  ↓
Clear Redo Stack
  ↓
Mark Dirty
```

## 10. Undo実行フロー

```text
Undo Request
  ↓
Pop Undo Stack
  ↓
Command.undo()
  ↓
Update Editor State
  ↓
Push Redo Stack
  ↓
Mark Dirty
```

## 11. Redo実行フロー

```text
Redo Request
  ↓
Pop Redo Stack
  ↓
Command.redo()
  ↓
Update Editor State
  ↓
Push Undo Stack
  ↓
Mark Dirty
```

## 12. AutoSaveとの関係

Undo / Redo実行後もAutoSave対象になる。

Undoした状態もユーザーが意図した編集状態であるため、dirtyとして扱う。

## 13. Flow Versionとの関係

Undo / Redo は一時的な編集履歴である。

Flow Version は永続的なSnapshotである。

Undo履歴はDB保存しない。

Version作成時にUndo Stackを含めない。

## 14. 編集ロックとの関係

編集ロック未取得またはViewerの場合、Undo / Redoは実行不可とする。

理由:

- Undo / Redo は編集操作である
- 他ユーザーの編集状態を破壊しないため

## 15. Commandの粒度

原則として、ユーザーが1操作と認識する単位を1 Commandとする。

例:

- Nodeドラッグ中のmousemoveはCommandにしない
- ドロップ時に1つのMoveNodeCommandを作成する
- 複数Node移動はMoveNodesCommand 1件にまとめる
- Template適用はApplyTemplateCommand 1件にまとめる

## 16. 複合Command

複数の操作を1つの操作として扱う場合はCompositeCommandを使用する。

例:

- Node削除 + 関連Link削除
- Lane削除 + Node移動
- Stage削除 + Node/Link削除
- Template適用
- 貼付

## 17. Dirty管理

Command実行後はFlowをdirtyにする。

対象:

- execute
- undo
- redo

ただし、Command生成に失敗した場合はdirtyにしない。

## 18. Validationとの関係

Command実行後、必要なValidationを再実行する。

軽量Validation:

- Node必須項目
- Link接続先
- Lane/Stage参照

重いValidationは保存前または明示実行時に行う。

## 19. 禁止事項

- Vue Flow内部履歴に依存する
- Undo履歴をDBへ保存する
- 選択状態をUndo対象にする
- mousemoveごとにCommandを作成する
- Command内でAPI保存する
- Command内で直接DOM操作する

## 20. 完了条件

- すべての編集操作がCommand化される
- Undo / Redo が構造化データを復元する
- Redo Stackの破棄条件が定義されている
- AutoSave / Version / 編集ロックとの関係が明確である

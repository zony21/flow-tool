# 14_02_CommandPattern設計

## 1. 本書の目的

本書は、AI Flow Designer における Command Pattern の実装設計を定義する。

Command Pattern は、ユーザーの編集操作をオブジェクトとして表現し、実行、取消、再実行を統一的に扱うための設計である。

## 2. 基本方針

- 編集操作はすべてCommandとして表現する
- Commandは構造化データを更新する
- CommandはDOMを直接操作しない
- CommandはAPI通信を行わない
- CommandはUndo/Redoに必要なbefore/afterを保持する
- Commandはシリアライズ可能な単純データを中心に持つ
- Command実行後にEditor Storeを更新する

## 3. Command Interface

基本Interface例:

```ts
export interface EditorCommand {
  commandId: string
  commandType: EditorCommandType
  label: string
  execute(context: CommandContext): void
  undo(context: CommandContext): void
  redo(context: CommandContext): void
  canMerge?(next: EditorCommand): boolean
  merge?(next: EditorCommand): EditorCommand
}
```

## 4. CommandContext

CommandはStoreへ直接依存させず、CommandContext経由で状態更新する。

```ts
export type CommandContext = {
  flowId: string
  nodes: NodeRepositoryLike
  links: LinkRepositoryLike
  lanes: LaneRepositoryLike
  stages: StageRepositoryLike
  comments: CommentRepositoryLike
  markDirty: (target: DirtyTarget) => void
  validateLight: (targets: ValidationTarget[]) => void
}
```

## 5. CommandType

```ts
export type EditorCommandType =
  | 'AddNode'
  | 'MoveNode'
  | 'MoveNodes'
  | 'ResizeNode'
  | 'UpdateNode'
  | 'DeleteNode'
  | 'AddLink'
  | 'ReconnectLink'
  | 'UpdateLink'
  | 'DeleteLink'
  | 'AddLane'
  | 'UpdateLane'
  | 'DeleteLane'
  | 'AddStage'
  | 'UpdateStage'
  | 'DeleteStage'
  | 'AddComment'
  | 'MoveComment'
  | 'UpdateComment'
  | 'DeleteComment'
  | 'PasteSelection'
  | 'ApplyTemplate'
  | 'Composite'
```

## 6. before / after

Commandは復元に必要なbefore/afterを保持する。

例: Node移動

```ts
export type MoveNodeCommandPayload = {
  nodeId: string
  before: PositionAndMembership
  after: PositionAndMembership
}
```

## 7. execute / undo / redo

- execute: 初回実行
- undo: 取り消し
- redo: 再実行

redoは原則executeと同じ結果になるが、実装上はredoメソッドを明示する。

## 8. Command生成タイミング

Commandはユーザー操作確定時に生成する。

例:

- Nodeドラッグ開始時ではなく、ドラッグ終了時
- リサイズ中ではなく、リサイズ終了時
- プロパティ入力中ではなく、入力確定またはdebounce後

## 9. Command Merge

連続する小さな操作を1つにまとめる場合、canMerge / mergeを使用する。

対象例:

- テキスト入力
- プロパティ変更
- 連続した微小移動

初期実装では必須ではないが、Interface上は拡張可能にする。

## 10. CompositeCommand

複数Commandを1つのCommandとして扱う。

利用例:

- Node削除 + Link削除
- Lane削除 + Node移動
- Stage削除 + Node削除 + Link削除
- Template適用
- 貼付

## 11. CompositeCommand実行順

execute:

```text
command1.execute
command2.execute
command3.execute
```

undo:

```text
command3.undo
command2.undo
command1.undo
```

redo:

```text
command1.redo
command2.redo
command3.redo
```

## 12. Command内の禁止処理

Command内では以下を禁止する。

- API通信
- DB保存
- DOM操作
- Toast表示
- Dialog表示
- Router遷移
- GitHub連携
- 非同期処理の乱用

## 13. 非同期Command

初期実装ではCommandは同期処理とする。

理由:

- Undo/Redoの順序保証が容易
- API保存はAutoSave/手動保存側で扱う
- UI操作の再現性が高い

将来的に画像アップロードなど非同期が必要な場合は、Command作成前にアップロードを完了させ、CommandにはimageFileIdのみ持たせる。

## 14. CommandとValidation

Command実行後、対象に応じて軽量Validationを呼び出す。

例:

- Node更新後: Node必須項目Validation
- Link再接続後: Link endpoint Validation
- Lane削除後: Node所属Validation

## 15. CommandとDirty

Command実行後、dirty対象を更新する。

例:

- AddNodeCommand: dirtyNodesに追加
- DeleteLinkCommand: deletedLinksに追加
- UpdateLaneCommand: dirtyLanesに追加

## 16. CommandとSelection

Commandは原則Selectionを変更しない。

ただし、Command実行後のUI補助としてCommandHandlerがSelectionを更新してよい。

例:

- AddNode後に追加Nodeを選択
- Paste後に貼付対象を選択
- Delete後に選択解除

## 17. CommandFactory

ユーザー操作からCommandを生成する専用Factoryを用意する。

```ts
export class EditorCommandFactory {
  createMoveNodeCommand(...): MoveNodeCommand
  createDeleteNodeCommand(...): CompositeCommand
  createPasteSelectionCommand(...): PasteSelectionCommand
}
```

## 18. CommandHandler

CommandHandlerはCommandの実行と履歴登録を担当する。

責務:

- executeCommand
- undo
- redo
- undoStack管理
- redoStack管理
- dirty更新
- selection補助更新

## 19. エラー処理

Command実行中にエラーが発生した場合:

- Stackへ積まない
- Editor状態を可能な限り変更前に戻す
- エラーをvalidationStoreまたは通知へ渡す

Commandは事前Validation済みであることを前提にする。

## 20. 完了条件

- Command Interfaceが定義されている
- CompositeCommandが定義されている
- CommandContextでStore更新できる
- Command内の責務外処理が排除されている
- CommandHandlerで履歴管理できる

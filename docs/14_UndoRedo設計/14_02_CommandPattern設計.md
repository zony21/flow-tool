# 14_02_CommandPattern設計

## 1. 目的

本書は、AI Flow DesignerにおけるCommand Pattern設計を定義する。

Command Patternは、ユーザーの編集操作をCommandとして表現し、実行、取消、再実行を統一的に扱うための設計である。

## 2. 基本方針

- 編集操作はCommandとして表現する。
- CommandはSSOTの編集状態を更新する。
- CommandはDOMを直接操作しない。
- CommandはAPI通信を行わない。
- CommandはUndo / Redoに必要なbefore / afterを保持する。
- CommandはStore経由で状態を変更する。
- Commandは可能な限り純粋なデータで構成する。

## 3. Commandの責務

Commandが担当すること:

- 実行前後の差分保持
- execute
- undo
- redo
- 表示名保持
- 対象要素ID保持

Commandが担当しないこと:

- Backend保存
- API通信
- Dialog表示
- Toast表示
- 権限判定の最終決定

## 4. Command Interface

```ts
export interface FlowCommand {
  commandId: string;
  commandType: string;
  label: string;
  targetIds: string[];
  executedAt?: string;
  execute(context: CommandContext): void;
  undo(context: CommandContext): void;
  redo(context: CommandContext): void;
}
```

## 5. CommandContext

```ts
export interface CommandContext {
  flowStore: FlowStore;
  selectionStore: SelectionStore;
  editorStore: EditorStore;
}
```

CommandはContext経由でStoreへアクセスする。

## 6. Command分類

- NodeCommand
- LinkCommand
- LaneStageCommand
- CommentCommand
- ClipboardCommand
- TemplateCommand
- LayoutCommand
- CompositeCommand

## 7. before / after保持

更新系Commandは変更前後を保持する。

例:

```ts
interface MoveNodeCommandPayload {
  nodeId: string;
  before: { x: number; y: number; laneId?: string; stageId?: string };
  after: { x: number; y: number; laneId?: string; stageId?: string };
}
```

## 8. execute / undo / redo

- execute: afterを適用する。
- undo: beforeへ戻す。
- redo: afterを再適用する。

Add系:

- execute: 追加
- undo: 削除
- redo: 再追加

Delete系:

- execute: 削除
- undo: 復元
- redo: 再削除

## 9. 禁止事項

- Command内でfetch / axiosを呼ぶ。
- Command内でDOMを直接操作する。
- Command内でDialogを開く。
- Command内でBackend保存する。
- CommandがSelection変更だけを履歴化する。

## 10. テスト観点

- Command.executeで状態が更新される。
- Command.undoでbeforeに戻る。
- Command.redoでafterに戻る。
- CommandがAPI通信をしない。
- CommandがDOM操作をしない。

## 11. 完了条件

- Command Patternの責務が定義されている。
- Command InterfaceとContextが定義されている。
- before / after保持方針が明確である。
- 実装者がCommand基盤を実装できる。

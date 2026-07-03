# 14_03_Commandインターフェース設計

## 1. 目的

本書は、AI Flow DesignerのCommandインターフェース設計を定義する。

Undo / Redo対象となる編集操作を統一的に扱うため、すべてのCommandが共通インターフェースを実装する。

## 2. 基本方針

- すべてのCommandはFlowCommandを実装する。
- Commandはexecute / undo / redoを持つ。
- Commandは対象要素IDを保持する。
- Commandは表示名を持つ。
- CommandはAPI通信を行わない。
- CommandはDOM操作を行わない。

## 3. FlowCommand

```ts
export interface FlowCommand {
  commandId: string;
  commandType: FlowCommandType;
  label: string;
  targetIds: string[];
  createdAt: string;
  execute(context: CommandContext): void;
  undo(context: CommandContext): void;
  redo(context: CommandContext): void;
}
```

## 4. FlowCommandType

```ts
export type FlowCommandType =
  | 'addNode'
  | 'deleteNode'
  | 'moveNode'
  | 'resizeNode'
  | 'updateNode'
  | 'addLink'
  | 'deleteLink'
  | 'updateLink'
  | 'addLane'
  | 'updateLane'
  | 'deleteLane'
  | 'addStage'
  | 'updateStage'
  | 'deleteStage'
  | 'addComment'
  | 'updateComment'
  | 'deleteComment'
  | 'moveComment'
  | 'pasteElements'
  | 'applyTemplate'
  | 'applyLayout'
  | 'composite';
```

## 5. CommandContext

```ts
export interface CommandContext {
  flowStore: FlowStore;
  selectionStore: SelectionStore;
  editorStore: EditorStore;
}
```

CommandはContextからStoreを参照する。

## 6. CommandResult

初期実装ではCommandはvoidを返す。

将来、失敗理由やWarningを返すため、以下を検討する。

```ts
interface CommandResult {
  success: boolean;
  warnings?: string[];
}
```

## 7. ID生成

Command自体にはcommandIdを付与する。

- UUIDを利用する。
- Stack内で識別できればよい。
- Backend保存対象ではない。

## 8. 禁止事項

- CommandがBackend APIを呼ぶ。
- CommandがVue Componentへ直接依存する。
- CommandがDOM要素を保持する。
- CommandがFileやBlobなど重いオブジェクトを保持する。

## 9. テスト観点

- 全CommandがFlowCommandを実装する。
- execute / undo / redoが存在する。
- commandTypeで分類できる。
- targetIdsを保持できる。

## 10. 完了条件

- FlowCommand、FlowCommandType、CommandContextが定義されている。
- Command実装者が共通仕様に従える。

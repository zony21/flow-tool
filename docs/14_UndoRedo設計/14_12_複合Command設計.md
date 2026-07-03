# 14_12_複合Command設計

## 1. 目的

本書は、AI Flow Designerの複合Command設計を定義する。

複合Commandは、複数のCommandを1つのUndo / Redo履歴として扱うための仕組みである。Template適用、Clipboard Paste、Lane削除、Layout適用などで利用する。

## 2. 基本方針

- 複数Commandを1履歴として扱う。
- executeは順方向に実行する。
- undoは逆順で実行する。
- redoは順方向に再実行する。
- 一部失敗時に不整合が起きないよう、事前検証を行う。

## 3. CompositeCommand

```ts
export class CompositeCommand implements FlowCommand {
  commandId: string;
  commandType = 'composite';
  label: string;
  targetIds: string[];
  commands: FlowCommand[];

  execute(context: CommandContext): void;
  undo(context: CommandContext): void;
  redo(context: CommandContext): void;
}
```

## 4. 実行順序

execute / redo:

```text
command[0]
command[1]
command[2]
```

undo:

```text
command[2]
command[1]
command[0]
```

## 5. 利用例

- Template適用
- Paste複数要素
- Lane削除 DeleteWithNodes
- Stage削除 DeleteWithNodes
- Layout適用
- 複数Node移動

## 6. 禁止事項

- 複合操作を個別履歴として大量に積む。
- undoを順方向で実行する。
- 失敗可能なCommandを事前検証なしで混在させる。

## 7. テスト観点

- 複数Commandを1回のUndoで戻せる。
- Undoは逆順で実行される。
- Redoは順方向で実行される。
- Template適用が1履歴になる。

## 8. 完了条件

- CompositeCommandの責務と実行順序が定義されている。
- 複合操作を1履歴として扱える。

# 14_05_CompositeCommand設計

## 1. 本書の目的

本書は、AI Flow Designer における CompositeCommand 設計を定義する。

CompositeCommand は、複数のCommandを1つのユーザー操作として扱うための仕組みである。Node削除、Lane削除、Stage削除、Template適用、貼付など、複数の構造要素を同時に変更する操作で使用する。

## 2. 基本方針

- ユーザーが1操作と認識する単位を1つのCompositeCommandにする
- executeは登録順に実行する
- undoは逆順に実行する
- redoは登録順に実行する
- 子Commandの一部だけ履歴に積まない
- CompositeCommand全体をUndo Stackへ1件として積む

## 3. Interface

```ts
export class CompositeCommand implements EditorCommand {
  commandId: string
  commandType: 'Composite'
  label: string
  commands: EditorCommand[]

  execute(context: CommandContext): void
  undo(context: CommandContext): void
  redo(context: CommandContext): void
}
```

## 4. execute順序

executeはcommands配列の順に実行する。

```text
command1.execute
command2.execute
command3.execute
```

例: Node削除

1. 関連Link削除
2. Node紐付けComment削除
3. Node削除

## 5. undo順序

undoは逆順で実行する。

```text
command3.undo
command2.undo
command1.undo
```

理由:

- 依存関係を壊さず復元するため
- Node復元後にLinkを復元する必要がある場合などに対応するため

## 6. redo順序

redoはexecuteと同じ順序で実行する。

## 7. 代表的な利用箇所

CompositeCommandを利用する操作:

- DeleteNodeWithRelationsCommand
- DeleteLaneMoveNodesCommand
- DeleteLaneWithNodesCommand
- DeleteStageMoveNodesCommand
- DeleteStageWithNodesCommand
- PasteSelectionCommand
- DuplicateSelectionCommand
- ApplyTemplateCommand
- BulkUpdateNodePropertyCommand

## 8. Node削除Composite

Node削除時は関連Linkも削除対象になる。

構成例:

```text
CompositeCommand: Node削除
  ├─ DeleteLinkCommand(link1)
  ├─ DeleteLinkCommand(link2)
  ├─ DeleteCommentCommand(comment1)
  └─ DeleteNodeCommand(node)
```

## 9. Lane削除Composite

Lane削除は選択式である。

選択肢:

- Nodeを別Laneへ移動
- Nodeを同時削除

Node移動の場合:

```text
CompositeCommand: Lane削除(Node移動)
  ├─ MoveNodeCommand(node1 laneA -> laneB)
  ├─ MoveNodeCommand(node2 laneA -> laneB)
  └─ DeleteLaneCommand(laneA)
```

Node同時削除の場合:

```text
CompositeCommand: Lane削除(Node同時削除)
  ├─ DeleteLinkCommand(...)
  ├─ DeleteNodeCommand(...)
  └─ DeleteLaneCommand(laneA)
```

## 10. Stage削除Composite

Stage削除もLane削除と同じ構造とする。

- Node移動
- Node同時削除

Stageの場合はstageIdを移動先Stageへ変更する。

## 11. 貼付Composite

貼付時は複数要素をまとめて追加する。

```text
CompositeCommand: 貼付
  ├─ AddNodeCommand(node1)
  ├─ AddNodeCommand(node2)
  ├─ AddLinkCommand(link1)
  └─ AddCommentCommand(comment1)
```

LinkはNode追加後に追加する。

## 12. Template適用Composite

Template適用ではLane / Stage / Node / Link / Commentを追加する。

実行順:

1. Lane追加
2. Stage追加
3. Node追加
4. Link追加
5. Comment追加

undoは逆順で行う。

## 13. 失敗時の扱い

CompositeCommand実行中に途中失敗した場合:

- すでに実行した子Commandを逆順でundoする
- CompositeCommandはUndo Stackへ積まない
- エラーを通知する

## 14. 子Commandの制約

CompositeCommandに含めるCommandは以下を満たすこと。

- 同期的に実行できる
- undo可能である
- redo可能である
- API通信を行わない
- DOM操作を行わない

## 15. Dirty管理

CompositeCommand実行後は、子Commandのdirty対象を統合する。

例:

- dirtyNodes
- dirtyLinks
- dirtyLanes
- dirtyStages
- dirtyComments
- deletedIds

## 16. Validation

CompositeCommand実行後、関連するValidationをまとめて実行する。

例:

- Node削除後: Link整合性
- Lane削除後: Node所属
- Template適用後: Flow構造全体の軽量Validation

## 17. Label

CompositeCommandのlabelはユーザー操作単位で表現する。

例:

- Node削除
- Lane削除
- Stage削除
- 貼付
- Template適用

子CommandのlabelをUIへ直接表示しない。

## 18. 禁止事項

- 子Commandを個別にUndo Stackへ積む
- executeとundoで順序を同じにする
- Template適用を大量の個別履歴として積む
- 一部だけUndo可能なCompositeを作る
- API保存をCompositeCommand内で行う

## 19. テスト観点

- executeが順番通り実行される
- undoが逆順で実行される
- redoが順番通り実行される
- 途中失敗時に実行済みCommandが戻る
- Node削除で関連Linkも戻る
- Lane削除でNode移動をUndoできる
- Template適用を1回でUndoできる

## 20. 完了条件

- 複合操作を1履歴として扱える
- 依存関係を壊さずUndo / Redoできる
- Lane / Stage削除の選択式挙動に対応できる
- Template適用や貼付を安全に扱える

# 14_07_UndoRedo性能設計

## 1. 本書の目的

本書は、AI Flow Designer における Undo / Redo の性能設計を定義する。

本システムでは、Undo 1000回を性能試験対象とする。大量Node / Linkを扱うFlowでも、履歴管理によって操作不能にならないようにする。

## 2. 性能目標

|対象|目標|
|---|---:|
|Undo 1回|500ms以内|
|Redo 1回|500ms以内|
|Undo Stack 1000件保持|操作可能|
|MoveNodeCommand 1000件|保持可能|
|Template適用Undo|2秒以内|
|Paste 100Node Undo|2秒以内|

## 3. 基本方針

- Flow全体Snapshotを毎Commandで保持しない
- 差分Snapshotを保持する
- 画像本体を履歴に保持しない
- 大量操作はCompositeCommandで1履歴にまとめる
- mousemoveごとにCommandを作成しない
- Undo / Redo時に全ViewModelを再生成しない設計を目指す

## 4. 差分保持

Commandは復元に必要な最小情報だけ保持する。

例: MoveNodeCommand

保持するもの:

- nodeId
- beforeX/Y
- afterX/Y
- beforeLaneId / beforeStageId
- afterLaneId / afterStageId

保持しないもの:

- Flow全体
- 全Node一覧
- 全Link一覧

## 5. Snapshotが必要なCommand

削除系Commandでは復元にSnapshotが必要である。

対象:

- DeleteNodeCommand
- DeleteLinkCommand
- DeleteLaneWithNodesCommand
- DeleteStageWithNodesCommand
- DeleteCommentCommand

Snapshotは対象Entityのみ保持する。

## 6. 大量削除時の注意

Lane削除やStage削除では大量Node / Linkを保持する可能性がある。

対策:

- CompositeCommandで1件として扱う
- SnapshotはJSON互換の軽量構造にする
- 画像本体は保持しない
- 不要な派生データは保持しない

## 7. Command Merge

連続した小さい変更は必要に応じてmergeする。

対象候補:

- テキスト入力
- プロパティ変更
- 微小移動

初期実装ではドラッグ完了時に1Command化することで、mousemoveによる履歴肥大化を防ぐ。

## 8. ViewModel更新最適化

Undo / Redo後は変更対象だけ再描画する方針とする。

初期実装で全再生成する場合も、将来差分更新へ移行できるよう、Commandが変更対象IDを返せる設計にする。

```ts
type CommandResult = {
  changedNodeIds: string[]
  changedLinkIds: string[]
  changedLaneIds: string[]
  changedStageIds: string[]
  changedCommentIds: string[]
}
```

## 9. Dirty更新最適化

Command結果をもとにdirty対象を更新する。

Flow全体をdirtyにするのではなく、対象別dirtyを管理する。

- dirtyNodes
- dirtyLinks
- dirtyLanes
- dirtyStages
- dirtyComments
- deletedIds

## 10. Validation最適化

Undo / Redo後に全Validationを毎回実行しない。

軽量Validation対象:

- 変更Node
- 変更Link
- 変更Lane / Stage配下Node
- 削除/復元された要素

保存前には全体Validationを行う。

## 11. メモリ見積

Command履歴はメモリを消費する。

特に重いもの:

- DeleteLaneWithNodesCommand
- DeleteStageWithNodesCommand
- ApplyTemplateCommand
- PasteSelectionCommand

上限1000件は通常Command前提であり、大量Compositeが多い場合はメモリ監視が必要。

## 12. 履歴圧縮

将来検討:

- 古いMoveCommandの圧縮
- 連続UpdatePropertyCommandの統合
- 大量Snapshotの圧縮
- Flow単位の履歴破棄確認

初期実装では履歴上限で制御する。

## 13. 画像Node

画像NodeのUndo / RedoではimageFileIdのみ保持する。

画像バイナリやObject URLは履歴に含めない。

## 14. Template適用

Template適用は大量要素追加になる可能性がある。

性能方針:

- 1つのCompositeCommandとして扱う
- 追加EntityのSnapshotを保持
- undoは追加Entityの論理削除または追加前状態へ戻す
- redoは同じIDで再復元する

## 15. 貼付

貼付も大量要素を扱う可能性がある。

貼付時に生成した新IDをCommandに保持し、redo時に同じIDで再適用する。

Redoで再採番してはならない。

## 16. Undo連打対策

Undo / Redoボタン連打対策として isExecuting を利用する。

実行中は次のUndo / Redoを受け付けない。

## 17. 非同期処理禁止

Command自体は同期実行を基本とする。

非同期API保存はAutoSaveまたは手動保存で行う。

これによりUndo / Redoの順序と一貫性を保つ。

## 18. 性能テストケース

- MoveNodeCommand 1000件作成
- Undo 1000回連続実行
- Redo 1000回連続実行
- 100Node貼付Undo
- 1000Node FlowでNode削除Undo
- 10000Link FlowでLink削除Undo
- Lane削除WithNodes Undo
- Stage削除MoveNodes Undo

## 19. 計測項目

- Command作成時間
- execute時間
- undo時間
- redo時間
- ViewModel反映時間
- Validation時間
- メモリ使用量

## 20. 禁止事項

- 毎CommandでFlow全体をdeep copyする
- mousemoveごとに履歴を積む
- Undo実行時にAPI保存する
- 画像本体を履歴に持つ
- Redo時に新しいGUIDを採番する
- 大量Compositeを子Commandごとに履歴へ積む

## 21. 完了条件

- Undo 1000回の性能試験方針が定義されている
- 履歴メモリ肥大化を抑制できる
- 大量Node / Link操作でも破綻しない
- 差分更新へ拡張可能な設計になっている

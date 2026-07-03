# 14_03_Command一覧設計

## 1. 本書の目的

本書は、AI Flow Designer で実装するCommand一覧を定義する。

すべての編集操作はCommandとして扱い、Undo / Redo、AutoSave、Validation、Dirty管理と連携する。

## 2. Node系Command

Node系Command:

- AddNodeCommand
- MoveNodeCommand
- MoveNodesCommand
- ResizeNodeCommand
- UpdateNodePropertyCommand
- DeleteNodeCommand
- DuplicateNodeCommand

## 3. AddNodeCommand

目的:

Nodeを追加する。

保持情報:

- node
- insertIndex
- selectedAfterExecute

execute:

- nodesByIdへ追加
- nodeOrderへ追加
- dirtyNodesへ追加

undo:

- nodeを削除扱いに戻す、または追加前状態へ戻す
- dirty状態を更新

redo:

- nodeを再追加

## 4. MoveNodeCommand

目的:

単一Nodeを移動する。

保持情報:

- nodeId
- beforeX
- beforeY
- afterX
- afterY
- beforeLaneId
- beforeStageId
- afterLaneId
- afterStageId

execute/redo:

- after状態を適用

undo:

- before状態へ戻す

## 5. MoveNodesCommand

目的:

複数Nodeをまとめて移動する。

保持情報:

- nodeMoves[]

1回のドラッグ操作を1Commandとする。

## 6. ResizeNodeCommand

目的:

Nodeサイズを変更する。

保持情報:

- nodeId
- beforeWidth
- beforeHeight
- afterWidth
- afterHeight
- beforeX
- beforeY
- afterX
- afterY

## 7. UpdateNodePropertyCommand

目的:

Nodeのプロパティを変更する。

対象:

- displayName
- description
- nodeTypeId
- laneId
- stageId
- extensionJson
- shape

保持情報:

- nodeId
- propertyName
- beforeValue
- afterValue

複数項目更新時はCompositeCommandまたはUpdateNodePropertiesCommandを使用する。

## 8. DeleteNodeCommand

目的:

Nodeを論理削除する。

Node削除時は関連Linkも削除するため、実際にはCompositeCommandで扱うことが多い。

保持情報:

- nodeSnapshot
- relatedLinkSnapshots
- relatedCommentSnapshots

## 9. Link系Command

Link系Command:

- AddLinkCommand
- ReconnectLinkCommand
- UpdateLinkPropertyCommand
- MoveLinkControlPointCommand
- DeleteLinkCommand

## 10. AddLinkCommand

目的:

Linkを追加する。

保持情報:

- link

execute:

- linksByIdへ追加
- dirtyLinksへ追加

undo:

- Linkを追加前状態へ戻す

## 11. ReconnectLinkCommand

目的:

Linkのfrom/toを変更する。

保持情報:

- linkId
- beforeFromNodeId
- beforeToNodeId
- beforeFromAnchor
- beforeToAnchor
- afterFromNodeId
- afterToNodeId
- afterFromAnchor
- afterToAnchor

## 12. UpdateLinkPropertyCommand

対象:

- dataName
- communicationType
- condition
- description
- linkType
- controlPoints

保持情報:

- linkId
- propertyName
- beforeValue
- afterValue

## 13. MoveLinkControlPointCommand

目的:

Bezier制御点を移動する。

保持情報:

- linkId
- controlPointIndex
- beforePoint
- afterPoint

初期実装で制御点UIを実装しない場合でも、将来用にCommandを定義する。

## 14. DeleteLinkCommand

目的:

Linkを論理削除する。

保持情報:

- linkSnapshot

undoで元のLinkを復元する。

## 15. Lane系Command

Lane系Command:

- AddLaneCommand
- UpdateLaneCommand
- ReorderLaneCommand
- DeleteLaneMoveNodesCommand
- DeleteLaneWithNodesCommand

## 16. DeleteLaneMoveNodesCommand

目的:

Lane削除時に配下Nodeを別Laneへ移動する。

保持情報:

- laneSnapshot
- moveTargetLaneId
- nodeBeforeAfter[]

execute:

- 対象NodeのlaneIdを移動先へ変更
- Laneを論理削除

undo:

- NodeのlaneIdを戻す
- Laneを復元

## 17. DeleteLaneWithNodesCommand

目的:

Lane削除時に配下Nodeも同時削除する。

保持情報:

- laneSnapshot
- nodeSnapshots[]
- linkSnapshots[]
- commentSnapshots[]

## 18. Stage系Command

Stage系Command:

- AddStageCommand
- UpdateStageCommand
- ReorderStageCommand
- DeleteStageMoveNodesCommand
- DeleteStageWithNodesCommand

Stage削除の考え方はLane削除に準ずる。

## 19. Comment系Command

Comment系Command:

- AddCommentCommand
- MoveCommentCommand
- ResizeCommentCommand
- UpdateCommentCommand
- DeleteCommentCommand
- AttachCommentToNodeCommand
- DetachCommentFromNodeCommand

## 20. Clipboard系Command

Clipboard系Command:

- CopySelectionCommand
- PasteSelectionCommand
- DuplicateSelectionCommand

CopySelectionCommandは履歴対象外とする。

PasteSelectionCommandとDuplicateSelectionCommandは履歴対象とする。

## 21. Template系Command

Template系Command:

- ApplyTemplateCommand

Template適用は複数Node/Link/Lane/Stage/Commentを追加するためCompositeCommandとして扱う。

## 22. Flow系Command

Flow系Command:

- UpdateFlowPropertyCommand

対象:

- Flow名
- 説明
- 表示設定
- Export設定

## 23. Command粒度一覧

|操作|Command粒度|
|---|---|
|Nodeドラッグ|ドロップ時に1件|
|複数Node移動|まとめて1件|
|テキスト入力|確定またはdebounce後に1件|
|Lane削除|選択結果を含めて1件|
|Stage削除|選択結果を含めて1件|
|Template適用|Compositeで1件|
|貼付|Compositeで1件|

## 24. 完了条件

- 主要編集操作に対応するCommandが定義されている
- Lane/Stage削除の選択式挙動がCommand化されている
- Template適用や貼付がCompositeCommandで表現できる
- 将来のBezier制御点編集にも対応できる

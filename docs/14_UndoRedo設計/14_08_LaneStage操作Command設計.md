# 14_08_LaneStage操作Command設計

## 1. 目的

本書は、AI Flow DesignerのLane / Stage操作Command設計を定義する。

Laneは責務、Stageは工程区切りを表す重要な構造要素である。削除や並び替えでは内包Nodeへの影響があるため、Undo / Redoで安全に復元できる必要がある。

## 2. 対象Command

- AddLaneCommand
- UpdateLaneCommand
- DeleteLaneCommand
- ReorderLaneCommand
- AddStageCommand
- UpdateStageCommand
- DeleteStageCommand
- ReorderStageCommand

## 3. AddLaneCommand

保持情報:

- lane
- insertIndex
- previousSelection

undoでは追加Laneを削除し、redoで再追加する。

## 4. DeleteLaneCommand

保持情報:

- deletedLane
- affectedNodes
- affectedLinks
- affectedComments
- deleteMode
- moveToLaneId

DeleteMode:

- MoveNodes
- DeleteWithNodes

MoveNodesの場合、UndoではNodeの元laneIdを復元する。
DeleteWithNodesの場合、Node / Link / Commentを復元する。

## 5. ReorderLaneCommand

保持情報:

- beforeOrder
- afterOrder

LaneのorderNoを戻せるようにする。

## 6. Stage Command

StageもLaneと同様に扱う。

DeleteStageCommandでは以下を保持する。

- deletedStage
- affectedNodes
- affectedLinks
- affectedComments
- deleteMode
- moveToStageId

## 7. 禁止事項

- Lane削除時に内包Nodeを無確認で削除する。
- Stage削除時に関連Linkを失う。
- orderNo変更をUndoできない形で反映する。

## 8. テスト観点

- AddLaneをUndoできる。
- DeleteLane MoveNodesをUndoできる。
- DeleteLane DeleteWithNodesをUndoできる。
- ReorderStageをUndoできる。

## 9. 完了条件

- Lane / Stage操作Commandが定義されている。
- 内包Node / Link / Commentへの影響をUndo / Redoできる。

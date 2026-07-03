# 14_06_Node操作Command設計

## 1. 目的

本書は、AI Flow DesignerのNode操作Command設計を定義する。

Node操作は、追加、削除、移動、リサイズ、Property更新など、Canvas編集の中心である。

## 2. 対象Command

- AddNodeCommand
- DeleteNodeCommand
- MoveNodeCommand
- ResizeNodeCommand
- UpdateNodeCommand

## 3. AddNodeCommand

保持情報:

- node
- insertIndex
- previousSelection

execute:

- Nodeを追加する。
- 追加Nodeを選択する。

undo:

- Nodeを削除する。
- previousSelectionを復元する。

redo:

- Nodeを再追加する。

## 4. DeleteNodeCommand

保持情報:

- deletedNode
- relatedLinks
- relatedComments
- previousSelection

execute:

- Nodeを削除状態にする。
- 関連Linkを削除状態にする。
- 必要に応じてCommentを削除または解除する。

undo:

- Nodeを復元する。
- 関連Linkを復元する。
- Commentを復元する。

## 5. MoveNodeCommand

保持情報:

- nodeId
- before x / y / laneId / stageId
- after x / y / laneId / stageId

Drag中は生成しない。
Drag完了時に1つだけ生成する。

## 6. ResizeNodeCommand

保持情報:

- nodeId
- before width / height
- after width / height

画像Nodeや判定Nodeなど、NodeType別の最小サイズを守る。

## 7. UpdateNodeCommand

保持情報:

- nodeId
- before properties
- after properties

対象:

- title
- description
- nodeType別property
- aiNotes
- laneId
- stageId

## 8. 禁止事項

- Node移動中のmousemoveをすべてCommand化する。
- Node削除時に関連Linkを失う。
- UpdateNodeCommandでFlow全体を保持する。
- selected状態をCommandに含めすぎる。

## 9. テスト観点

- AddNodeをUndoで消せる。
- DeleteNodeをUndoで復元できる。
- MoveNodeをUndoで元位置へ戻せる。
- ResizeNodeをRedoできる。
- UpdateNodeでPropertyが戻る。

## 10. 完了条件

- Node操作Commandのexecute / undo / redoが定義されている。
- 関連Link / Commentの扱いが明確である。
- 実装者がNode Commandを実装できる。

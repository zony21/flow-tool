# 13_08_DragDrop設計

## 1. 目的

本書は、AI Flow DesignerのDrag & Drop設計を定義する。

Drag & Dropは、Node追加、Node移動、Comment移動、Lane / Stage間移動、Template適用など、Canvas編集の中心操作である。

## 2. 基本方針

- Drag中状態と確定状態を分ける。
- Drag完了時にCommandを生成する。
- Drag中の一時座標をSSOTへ保存しない。
- Node PaletteからCanvasへNodeを追加できる。
- Node移動時にLane / Stage候補を判定する。

## 3. DragState

```ts
interface DragState {
  isDragging: boolean;
  dragType: 'node' | 'comment' | 'paletteNode' | 'selection' | 'none';
  sourceId?: string;
  currentX?: number;
  currentY?: number;
  targetLaneId?: string;
  targetStageId?: string;
}
```

## 4. Node Palette Drag

処理:

1. Palette Item drag start。
2. NodeTypeをDragPayloadへ設定。
3. Canvas上でPreview表示。
4. Drop位置をCanvas座標へ変換。
5. Lane / Stage候補を判定。
6. AddNodeCommand生成。
7. Node追加。
8. 追加Nodeを選択。

## 5. Node移動

Drag中:

- 表示のみ更新。
- 保存Requestは作らない。

Drag完了:

- Canvas座標へ変換。
- MoveNodeCommand生成。
- laneId / stageId候補を更新。
- undoRedoStoreへ登録。

## 6. 複数Node移動

複数選択中にDragした場合、選択中Nodeをまとめて移動する。

Command:

```text
MoveSelectionCommand
```

各Nodeの移動前後座標を保持する。

## 7. Comment移動

独立CommentはCanvas上で移動できる。
Node紐付けCommentは、初期ではNode移動に追従する。

## 8. Drop禁止条件

- Canvas外へのDrop
- 存在しないLane / Stageへの所属確定
- Viewer権限でのDrop
- 編集ロック未取得時のDrop

## 9. Drag Preview

Previewは軽量に表示する。

- Node追加Preview
- 移動中Border
- Drop可能領域Highlight
- Drop不可Cursor

## 10. テスト観点

- PaletteからNodeを追加できる。
- Drop位置がCanvas座標へ変換される。
- Node移動がMoveNodeCommandになる。
- 複数Node移動がMoveSelectionCommandになる。
- Viewer権限ではDropできない。

## 11. 完了条件

- Drag中状態と確定状態が分離されている。
- Node追加、Node移動、Comment移動、複数移動が定義されている。
- 実装者がDrag & Dropを実装できる。

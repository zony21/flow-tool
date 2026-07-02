# Undo/Redo設計

## 基本方針
コマンドパターンを採用し、すべての編集操作をCommandとして記録する。

## Commandインターフェース
- execute()
- undo()
- redo()

## 主なコマンド
- AddNodeCommand
- DeleteNodeCommand
- MoveNodeCommand
- ResizeNodeCommand
- UpdateNodeCommand
- AddLinkCommand
- DeleteLinkCommand
- UpdatePropertyCommand
- MoveLaneCommand
- MoveStageCommand

## 履歴管理
- UndoStack
- RedoStack
- 最大保持件数設定
- 保存後も履歴保持

## グループ化
ドラッグ中の連続移動は1コマンドとして記録。
複数ノード移動も1履歴にまとめる。

## 対象操作
- ノード
- 接続
- レーン
- 工程
- コメント
- プロパティ変更

## 将来拡張
- 永続化履歴
- タイムライン表示
- 操作差分比較

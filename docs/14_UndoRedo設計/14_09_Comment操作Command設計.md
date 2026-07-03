# 14_09_Comment操作Command設計

## 1. 目的

本書は、AI Flow DesignerのComment操作Command設計を定義する。

Commentは、通常補足、レビュー指摘、AI専用メモを表す。独立CommentとNode紐付けCommentの両方をUndo / Redoできる必要がある。

## 2. 対象Command

- AddCommentCommand
- UpdateCommentCommand
- DeleteCommentCommand
- MoveCommentCommand
- ChangeCommentTargetCommand

## 3. AddCommentCommand

保持情報:

- comment
- previousSelection

executeでCommentを追加し、undoで削除する。

## 4. UpdateCommentCommand

保持情報:

- commentId
- before
- after

対象:

- body
- commentType
- aiVisible
- targetType
- targetId

## 5. DeleteCommentCommand

保持情報:

- deletedComment
- previousSelection

undoで元のCommentを復元する。

## 6. MoveCommentCommand

保持情報:

- commentId
- before x / y
- after x / y

Drag中はCommandを生成せず、Drag完了時に1つ生成する。

## 7. ChangeCommentTargetCommand

Node紐付けCommentの対象を変更する場合に使用する。

保持情報:

- commentId
- before targetType / targetId
- after targetType / targetId

## 8. 禁止事項

- Comment本文だけを保存し、target情報を失う。
- AI専用メモのaiVisibleをUndo対象外にする。
- Comment移動中のmousemoveをすべてCommand化する。

## 9. テスト観点

- AddCommentをUndoできる。
- UpdateCommentで本文を戻せる。
- MoveCommentをUndoできる。
- ChangeCommentTargetをUndoできる。

## 10. 完了条件

- Comment追加・更新・削除・移動・紐付け変更Commandが定義されている。
- AI専用メモを含むComment情報を復元できる。

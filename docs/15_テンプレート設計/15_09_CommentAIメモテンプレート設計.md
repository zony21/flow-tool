# 15_09_CommentAIメモテンプレート設計

## 1. 目的

本書は、TemplateにおけるCommentとAIメモの扱いを定義する。

Commentは補足、レビュー、AI専用メモを保持し、Template適用後のAI理解精度を高めるために利用する。

## 2. 基本方針

- CommentはTemplate保存対象にできる。
- 独立Commentと対象紐付けCommentを保持する。
- AI専用メモを保持できる。
- aiVisibleを保持する。
- 適用時は参照先IDを新しいIDへ置き換える。

## 3. 保存項目

- commentId
- targetType
- targetId
- commentType
- body
- positionX
- positionY
- aiVisible

## 4. CommentType

- normal
- warning
- aiMemo
- reviewNote

## 5. 適用時の扱い

対象紐付けCommentは、対象要素の新しいIDへ紐付け直す。
対象がTemplate内に存在しない場合は、独立Commentとして扱うか、適用前Validationで警告する。

## 6. AIメモ

AIメモは設計者向けの内部補足として扱う。
Export時に含めるかはExport Optionで制御する。

## 7. テスト観点

- Comment本文を保存できる。
- aiVisibleを保持できる。
- Node紐付けCommentを適用後のNodeへ紐付け直せる。

## 8. 完了条件

- CommentとAIメモの保存・適用方針が定義されている。

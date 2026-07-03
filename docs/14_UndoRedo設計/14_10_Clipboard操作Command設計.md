# 14_10_Clipboard操作Command設計

## 1. 目的

本書は、AI Flow DesignerのClipboard操作Command設計を定義する。

Clipboard操作では、Copy / Cut / Paste / Duplicateにより複数Node、Link、Commentをまとめて扱う。Paste時はID再採番とLink参照更新が必須である。

## 2. 対象Command

- PasteElementsCommand
- CutElementsCommand
- DuplicateElementsCommand

Copyは状態保存のみで編集操作ではないため、Undo対象外とする。

## 3. PasteElementsCommand

保持情報:

- pastedNodes
- pastedLinks
- pastedComments
- idMapping
- pasteOffset
- previousSelection

execute:

- 再採番済み要素を追加する。
- 貼り付け要素を選択する。

undo:

- 貼り付け要素を削除する。
- previousSelectionを復元する。

redo:

- 同じIDで再追加する。

## 4. CutElementsCommand

CutはCopy + Deleteとして扱う。

保持情報:

- copiedData
- deletedElements
- previousSelection

undo:

- 削除した要素を復元する。

## 5. DuplicateElementsCommand

DuplicateはCopy + Pasteを1つのCommandとして扱う。

Shortcut:

```text
Ctrl + D
```

## 6. ID再採番

Paste前に以下を再採番する。

- nodeId
- linkId
- commentId

LinkのsourceNodeId / targetNodeIdは新NodeIdへ変換する。

## 7. 禁止事項

- コピー元IDをそのままPasteする。
- Link参照を更新せずPasteする。
- Copy操作だけをUndo Stackへ積む。
- Paste時にSelectionを失う。

## 8. テスト観点

- PasteをUndoで取り消せる。
- Redoで同じIDの要素を再追加できる。
- Link参照が新NodeIdへ変換される。
- Duplicateが1つの履歴として扱われる。

## 9. 完了条件

- Clipboard操作Commandが定義されている。
- ID再採番とLink参照更新が定義されている。
- 実装者がClipboard操作のUndo / Redoを実装できる。

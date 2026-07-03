# 14_11_Template適用Command設計

## 1. 目的

本書は、AI Flow DesignerのTemplate適用Command設計を定義する。

Template適用は、Lane / Stage / Node / Link / Commentをまとめて追加または置換する複合操作である。Undo / Redoでは適用前の状態へ安全に戻せる必要がある。

## 2. 基本方針

- Template適用はCompositeCommandとして扱う。
- 適用時にIDを再採番する。
- 追加モードと置換モードを区別する。
- Undoで適用前状態へ戻す。
- Template適用自体はFrontend編集履歴であり、Backend Snapshotではない。

## 3. 適用モード

- Append: 現在Flowへ追加
- Replace: 現在Flowを置き換え

## 4. ApplyTemplateCommand

保持情報:

- templateId
- applyMode
- beforeFlowElements
- appliedElements
- idMapping
- previousSelection

## 5. Append時

execute:

- ID再採番済みTemplate要素を追加する。
- 追加要素を選択する。

undo:

- 追加要素を削除する。
- previousSelectionを復元する。

redo:

- 同じIDで再追加する。

## 6. Replace時

execute:

- 現在Flow要素を退避する。
- Template要素へ置換する。

undo:

- 置換前Flow要素を復元する。

redo:

- Template要素へ再置換する。

## 7. 禁止事項

- Template適用をUndo不可にする。
- ID再採番せずに適用する。
- Replace時に置換前状態を保持しない。
- Template適用をVersion Snapshotと混同する。

## 8. テスト観点

- Append適用をUndoできる。
- Replace適用をUndoできる。
- Redoで再適用できる。
- ID再採番される。

## 9. 完了条件

- Template適用CommandのAppend / Replaceが定義されている。
- Undo / Redoで適用前後を復元できる。

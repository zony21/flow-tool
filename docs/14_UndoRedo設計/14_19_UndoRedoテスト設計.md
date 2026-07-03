# 14_19_UndoRedoテスト設計

## 1. 目的

本書は、AI Flow DesignerのUndo / Redoテスト設計を定義する。

Undo / Redoは編集体験の信頼性に直結する。Command、Stack、Drag粒度、Clipboard、Template、Snapshot分離、例外時の安全性をテストする。

## 2. テスト分類

- Command Unit Test
- Stack Unit Test
- Store Test
- Interaction Test
- E2E Test
- 競合 Test

## 3. Command Unit Test

対象:

- AddNodeCommand
- DeleteNodeCommand
- MoveNodeCommand
- ResizeNodeCommand
- AddLinkCommand
- UpdateLinkCommand
- DeleteLaneCommand
- PasteElementsCommand
- ApplyTemplateCommand
- CompositeCommand

観点:

- executeでafter適用。
- undoでbefore復元。
- redoでafter再適用。
- 関連要素も復元される。

## 4. Stack Test

観点:

- Command実行後にundoStackへpushされる。
- UndoでredoStackへ移動する。
- RedoでundoStackへ戻る。
- 新規Command実行でredoStackがclearされる。
- maxHistoryCount超過時に古い履歴が消える。

## 5. Drag Test

観点:

- Drag中にCommandが生成されない。
- Drag完了で1Command生成される。
- 移動なしDragではCommand生成されない。
- 複数Node移動が1履歴になる。

## 6. Clipboard Test

観点:

- PasteをUndoできる。
- Paste時にID再採番される。
- Link参照が新NodeIdへ更新される。
- Duplicateが1履歴になる。

## 7. Snapshot分離 Test

観点:

- Snapshot作成がUndo Stackへ入らない。
- Snapshot復元でUndo / Redo Stackがclearされる。
- ExportがUndo履歴を参照しない。

## 8. E2E Test

代表シナリオ:

1. Nodeを追加する。
2. UndoでNodeが消える。
3. RedoでNodeが戻る。
4. Nodeを移動する。
5. Undoで元位置に戻る。
6. Linkを追加する。
7. UndoでLinkが消える。
8. 保存する。
9. Undo履歴が維持される。

## 9. 競合 Test

- Flow再読込でStackがclearされる。
- Snapshot復元でStackがclearされる。
- 対象ID不存在Commandを安全に失敗させる。

## 10. 完了条件

- Command、Stack、Drag、Clipboard、Snapshot分離、競合のテスト観点が定義されている。
- 実装者がUndo / Redoテストを実装できる。

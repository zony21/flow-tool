# 14_20_UndoRedo設計まとめ

## 1. 目的

本書は、14_UndoRedo設計の総まとめである。

Undo / Redoは、Frontend編集中の操作履歴を戻す・やり直すための機能である。BackendのVersion Snapshotとは責務が異なる。

## 2. Undo / Redoの責務

- 編集操作をCommandとして記録する。
- Undo Stackで取り消し可能操作を保持する。
- Redo Stackで再実行可能操作を保持する。
- Drag操作を適切な粒度で履歴化する。
- 複合操作を1履歴として扱う。
- Snapshot復元やFlow再読込時に履歴を安全に破棄する。

## 3. 対象外

- Backend保存
- Export
- AIレビュー実行
- Version Snapshot作成
- Zoom / Pan
- Selection変更
- Grid表示切替

## 4. 実装順序

1. FlowCommand Interface
2. CommandContext
3. undoRedoStore
4. Undo Stack
5. Redo Stack
6. Node Command
7. Link Command
8. Lane / Stage Command
9. Comment Command
10. Clipboard Command
11. Composite Command
12. Shortcut連携
13. テスト

## 5. 実装チェックリスト

- [ ] CommandがAPI通信しない。
- [ ] CommandがDOM操作しない。
- [ ] Drag中の中間操作を履歴化しない。
- [ ] 新規Command実行でRedo Stackをclearする。
- [ ] Snapshot作成を履歴化しない。
- [ ] Snapshot復元でStackをclearする。
- [ ] Paste時にID再採番する。
- [ ] Template適用をCompositeCommandにする。
- [ ] 保存成功で無条件に履歴破棄しない。

## 6. 完了条件

14_UndoRedo設計は、以下を満たすことで完了とする。

- Command Patternが定義されている。
- Undo / Redo Stackが定義されている。
- Node / Link / Lane / Stage / Comment操作Commandが定義されている。
- Clipboard、Template、複合Commandが定義されている。
- Drag粒度、履歴容量、Snapshot分離、Shortcut、例外、テストが定義されている。
- 実装者が本章を読んでUndo / Redo機能を実装できる。

## 7. 次工程

次工程は、15_テンプレート設計である。

15章では、Template定義、保存、適用、ID再採番、標準Template、Project Template、Template複製を詳細化する。

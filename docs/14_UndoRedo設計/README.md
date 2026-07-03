# 14_UndoRedo設計

## 1. 本書の位置付け

本フォルダは、AI Flow DesignerのUndo / Redo設計を管理する。

Undo / Redoは、Frontend編集中の一時操作履歴である。Backendで保存するVersion Snapshotとは責務が異なる。

## 2. 設計思想

- Command Patternを採用する。
- Undo StackとRedo Stackを分離する。
- 編集操作はCommandとして記録する。
- Drag中の細かい移動は履歴化しない。
- Drag完了時に1つのCommandとして記録する。
- Template適用やClipboard操作は複合Commandとして扱う。
- Version Snapshotとは責務を分離する。

## 3. 正式設計書一覧

| ファイル | 内容 | 状態 |
| --- | --- | --- |
| `14_01_UndoRedo概要.md` | Undo / Redo全体方針 | 詳細化済み |
| `14_02_CommandPattern設計.md` | Command Pattern | 詳細化済み |
| `14_03_Commandインターフェース設計.md` | Command Interface | 詳細化済み |
| `14_04_UndoStack設計.md` | Undo Stack | 詳細化済み |
| `14_05_RedoStack設計.md` | Redo Stack | 詳細化済み |
| `14_06_Node操作Command設計.md` | Node操作 | 詳細化済み |
| `14_07_Link操作Command設計.md` | Link操作 | 詳細化済み |
| `14_08_LaneStage操作Command設計.md` | Lane / Stage操作 | 詳細化済み |
| `14_09_Comment操作Command設計.md` | Comment操作 | 詳細化済み |
| `14_10_Clipboard操作Command設計.md` | Clipboard操作 | 詳細化済み |
| `14_11_Template適用Command設計.md` | Template適用 | 詳細化済み |
| `14_12_複合Command設計.md` | Composite Command | 詳細化済み |
| `14_13_Drag操作履歴設計.md` | Drag履歴粒度 | 詳細化済み |
| `14_14_履歴容量管理設計.md` | Stack容量管理 | 詳細化済み |
| `14_15_Snapshot責務分離設計.md` | Snapshotとの分離 | 詳細化済み |
| `14_16_キーボードショートカット設計.md` | Shortcut | 詳細化済み |
| `14_17_履歴表示設計.md` | History UI | 詳細化済み |
| `14_18_例外競合設計.md` | 例外・競合 | 詳細化済み |
| `14_19_UndoRedoテスト設計.md` | Test | 詳細化済み |
| `14_20_UndoRedo設計まとめ.md` | 総まとめ | 詳細化済み |

## 4. 管理方針

`14_UndoRedo設計` 配下には正式設計書のみを残す。

作業メモ、draft、temp、old、更新概要、数行だけの旧設計書は正式設計書へ統合後に削除する。

## 5. 現在の状態

14章は `14_01`〜`14_20` まで詳細化済み。

14_UndoRedo設計は詳細化済みとする。
次工程は `15_テンプレート設計` の詳細化を想定する。

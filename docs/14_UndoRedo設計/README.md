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
| `14_01_UndoRedo概要.md` | Undo / Redo全体方針 | 詳細化対象 |
| `14_02_CommandPattern設計.md` | Command Pattern | 詳細化対象 |
| `14_03_Commandインターフェース設計.md` | Command Interface | 追加予定 |
| `14_04_UndoStack設計.md` | Undo Stack | 追加予定 |
| `14_05_RedoStack設計.md` | Redo Stack | 追加予定 |
| `14_06_Node操作Command設計.md` | Node操作 | 追加予定 |
| `14_07_Link操作Command設計.md` | Link操作 | 追加予定 |
| `14_08_LaneStage操作Command設計.md` | Lane / Stage操作 | 追加予定 |
| `14_09_Comment操作Command設計.md` | Comment操作 | 追加予定 |
| `14_10_Clipboard操作Command設計.md` | Clipboard操作 | 追加予定 |
| `14_11_Template適用Command設計.md` | Template適用 | 追加予定 |
| `14_12_複合Command設計.md` | Composite Command | 追加予定 |
| `14_13_Drag操作履歴設計.md` | Drag履歴粒度 | 追加予定 |
| `14_14_履歴容量管理設計.md` | Stack容量管理 | 追加予定 |
| `14_15_Snapshot責務分離設計.md` | Snapshotとの分離 | 追加予定 |
| `14_16_キーボードショートカット設計.md` | Shortcut | 追加予定 |
| `14_17_履歴表示設計.md` | History UI | 追加予定 |
| `14_18_例外競合設計.md` | 例外・競合 | 追加予定 |
| `14_19_UndoRedoテスト設計.md` | Test | 追加予定 |
| `14_20_UndoRedo設計まとめ.md` | 総まとめ | 追加予定 |

## 4. 管理方針

`14_UndoRedo設計` 配下には正式設計書のみを残す。

作業メモ、draft、temp、old、更新概要、数行だけの旧設計書は正式設計書へ統合後に削除する。

## 5. 現在の状態

14章は既存ファイルを回収しつつ、`14_01`〜`14_20` へ再整理する。

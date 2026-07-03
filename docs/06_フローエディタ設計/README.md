# 06_フローエディタ設計

## 1. 本書の位置付け

本ファイルは、`06_フローエディタ設計` 配下の正式設計書の索引である。
短文メモ、更新メモ、前版維持のみの内容は正式設計書として扱わない。

## 2. フローエディタ設計の目的

フローエディタは、AI Flow Designerの中核機能である。
利用者が見やすく編集できるだけでなく、AIが理解できる構造化データを正しく作成・保存・出力できることを目的とする。

## 3. 正式設計書一覧

| ファイル | 内容 |
| --- | --- |
| `06_01_フローエディタ設計方針.md` | エディタ共通方針 |
| `06_02_全体レイアウト設計.md` | 全体レイアウト |
| `06_03_Toolbar設計.md` | Toolbar |
| `06_04_Canvas設計.md` | Canvas |
| `06_05_NodePalette設計.md` | NodePalette |
| `06_06_PropertyPanel設計.md` | PropertyPanel |
| `06_07_Lane設計.md` | Lane |
| `06_08_Stage設計.md` | Stage |
| `06_09_Node設計.md` | Node |
| `06_10_Link設計.md` | Link |
| `06_11_Comment設計.md` | Comment |
| `06_12_ContextMenu設計.md` | ContextMenu |
| `06_13_Shortcut設計.md` | Shortcut |
| `06_14_Grid_Snap設計.md` | Grid/Snap |
| `06_15_AutoSave設計.md` | AutoSave |
| `06_16_Version連携設計.md` | Version連携 |
| `06_17_Template連携設計.md` | Template連携 |
| `06_18_Export連携設計.md` | Export連携 |
| `06_19_AIレビュー連携設計.md` | AIレビュー連携 |
| `06_20_フローエディタ設計まとめ.md` | 本まとめ |

## 4. 不要ファイル削除ルール

`06_フローエディタ設計` 配下には正式設計書のみを残す。
更新メモ、更新概要、作業メモ、draft、temp、old、数行だけの旧設計書は正式設計書へ統合済みであれば削除する。

## 5. 今後の詳細化対象

- Vue Flowコンポーネント構成
- Pinia Store詳細
- Command Pattern連携
- API Request/Response完全対応
- UIワイヤーフレーム
- AIレビュー結果パネル

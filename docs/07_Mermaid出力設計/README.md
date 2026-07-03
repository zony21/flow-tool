# 07_Mermaid出力設計

## 1. 本書の位置付け

本ファイルは、`07_Mermaid出力設計` 配下の正式設計書の索引である。
短文メモ、更新メモ、前版維持のみの内容は正式設計書として扱わない。

## 2. Mermaid出力設計の目的

Mermaid出力は、FlowVersionの構造化データから生成されるテキスト成果物である。
設計書、GitHub、AI入力に利用し、人とAIの両方がフロー構造を理解できることを目的とする。

## 3. 正式設計書一覧

| ファイル | 内容 |
| --- | --- |
| `07_01_Mermaid出力方針.md` | Mermaid出力共通方針 |
| `07_02_flowchart生成仕様.md` | flowchart生成 |
| `07_03_sequenceDiagram生成仕様.md` | sequenceDiagram生成 |
| `07_04_Node変換仕様.md` | Node変換 |
| `07_05_Link変換仕様.md` | Link変換 |
| `07_06_Lane変換仕様.md` | Lane変換 |
| `07_07_Stage変換仕様.md` | Stage変換 |
| `07_08_Comment変換仕様.md` | Comment変換 |
| `07_09_スタイル設計.md` | Mermaidスタイル |
| `07_10_レイアウト設計.md` | 出力順・レイアウト |
| `07_11_エラー処理.md` | 生成エラー処理 |
| `07_12_特殊ケース設計.md` | 分岐・合流・孤立Node |
| `07_13_ループ表現設計.md` | ループ表現 |
| `07_14_Subgraph設計.md` | subgraph表現 |
| `07_15_識別子設計.md` | Mermaid識別子 |
| `07_16_出力オプション.md` | 出力オプション |
| `07_17_性能設計.md` | 性能設計 |
| `07_18_テスト仕様.md` | テスト仕様 |
| `07_19_サンプル出力.md` | サンプル |
| `07_20_Mermaid出力設計まとめ.md` | 本まとめ |

## 4. 不要ファイル削除ルール

`07_Mermaid出力設計` 配下には正式設計書のみを残す。
更新メモ、更新概要、作業メモ、draft、temp、old、数行だけの旧設計書は正式設計書へ統合済みであれば削除する。

## 5. 今後の詳細化対象

- Mermaid生成Service設計
- Export APIのRequest/Response完全定義
- Mermaid出力ユニットテスト
- Mermaid Parser互換検証
- 大規模Flowの出力最適化

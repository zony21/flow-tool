# 07_20_Mermaid出力設計まとめ

## 1. 本書の目的

本書は、07_Mermaid出力設計の内容を総括する。
Mermaid出力は、FlowVersionの構造化データから生成されるテキスト成果物であり、設計書、GitHub、AI入力に利用する。

## 2. 正式設計書一覧

| ファイル | 内容 |
| --- | --- |
| 07_01_Mermaid出力方針.md | Mermaid出力共通方針 |
| 07_02_flowchart生成仕様.md | flowchart生成 |
| 07_03_sequenceDiagram生成仕様.md | sequenceDiagram生成 |
| 07_04_Node変換仕様.md | Node変換 |
| 07_05_Link変換仕様.md | Link変換 |
| 07_06_Lane変換仕様.md | Lane変換 |
| 07_07_Stage変換仕様.md | Stage変換 |
| 07_08_Comment変換仕様.md | Comment変換 |
| 07_09_スタイル設計.md | Mermaidスタイル |
| 07_10_レイアウト設計.md | 出力順・レイアウト |
| 07_11_エラー処理.md | 生成エラー処理 |
| 07_12_特殊ケース設計.md | 分岐・合流・孤立Node |
| 07_13_ループ表現設計.md | ループ表現 |
| 07_14_Subgraph設計.md | subgraph表現 |
| 07_15_識別子設計.md | Mermaid識別子 |
| 07_16_出力オプション.md | 出力オプション |
| 07_17_性能設計.md | 性能設計 |
| 07_18_テスト仕様.md | テスト仕様 |
| 07_19_サンプル出力.md | サンプル |
| 07_20_Mermaid出力設計まとめ.md | 本まとめ |

## 3. 中核方針

MermaidはCanvasの見た目からではなく、FlowVersion配下のLane、Stage、Node、Link、Commentから生成する。
Node IDやLane IDを元に安定した識別子を生成し、表示名変更で参照が壊れないようにする。

## 4. 今後の詳細化対象

- Mermaid生成Service設計
- Export APIのRequest/Response完全定義
- Mermaid出力ユニットテスト
- Mermaid Parser互換検証
- 大規模Flowの出力最適化

## 5. 完了条件

Mermaid出力設計は、実装者がFlowVersionからflowchartとsequenceDiagramを生成でき、AIが再解析しやすい成果物を作れる状態になった場合に完了とする。

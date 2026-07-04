# 23_Performance設計

## 1. 目的

AI Flow Designerの性能設計を定義する。

大規模Flowでも編集・保存・Exportが破綻しないよう、初期実装から性能上の前提を明確にする。

## 2. 想定規模

| 規模 | Node数 | Link数 | 用途 |
| --- | --- | --- | --- |
| Small | 〜100 | 〜200 | 通常Flow |
| Medium | 〜500 | 〜1000 | 大規模業務Flow |
| Large | 〜1000 | 〜3000 | 複雑システム設計 |

初期実装ではSmall〜Mediumを快適に扱うことを目標にする。

Largeは設計上の上限目安とし、最適化対象とする。

## 3. 基本方針

- Canvas表示とSSOT保存を分離する
- Flow保存は差分保存を将来対応可能にする
- Exportは非同期化を将来対応可能にする
- 大量Node時は描画負荷を抑える
- 検索・絞り込みで編集対象を減らせるようにする

## 4. Frontend性能方針

| 対象 | 方針 |
| --- | --- |
| Canvas | Vue Flowの再描画を最小化 |
| Node更新 | 変更対象Nodeのみ更新 |
| Store | 大量配列の全再生成を避ける |
| Search | Node/Label/Metadata検索を用意 |
| Property Panel | 選択対象のみ描画 |
| MiniMap | Largeでは非表示選択可能 |

## 5. Backend性能方針

| 対象 | 方針 |
| --- | --- |
| Flow取得 | Include過多を避ける |
| Flow保存 | 初期は全体保存、将来差分保存 |
| Export | Large Flowでは非同期化候補 |
| Index | PROJECT_ID、FLOW_ID、NODE_IDへIndex |
| Version | Snapshot肥大化に注意 |

## 6. Export性能方針

- JSON Exportは同期処理で開始
- Mermaid Exportは同期処理で開始
- PDF Exportは時間がかかる場合、将来非同期化
- AI DSL ExportはValidationとセットで最適化する

## 7. 性能テスト観点

| Test | 条件 | 期待結果 |
| --- | --- | --- |
| MediumFlow_Load | Node 500 / Link 1000 | 読込可能 |
| MediumFlow_Save | Node 500 / Link 1000 | 保存可能 |
| LargeFlow_ExportJson | Node 1000 / Link 3000 | Export可能 |
| Search_NodeLabel | Node 1000 | 検索可能 |

## 8. 完了条件

- 想定規模が定義されている
- Frontend/Backend/Exportの性能方針が定義されている
- 将来の差分保存・非同期Export方針が定義されている
- 性能テスト観点が定義されている

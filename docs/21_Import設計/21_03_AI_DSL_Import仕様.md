# 21_03_AI_DSL_Import仕様

## 1. 目的

AI DSLからFlow構造化データを生成するImport仕様を定義する。

AI DSL Importは、AIが生成した設計案をAI Flow Designer上のFlowとして復元するための入口である。

## 2. 基本方針

- AI DSLは保存形式ではなく理解形式である
- Import時にSSOT構造へ変換する
- DSLに不足がある場合はwarningとしてPreview表示する
- 重大な構造不正はImport不可とする

## 3. 対象DSL Version

初期対応Version:

```text
v1
```

未対応Versionはerrorとする。

## 4. DSLからSSOTへの対応

| AI DSL | SSOT |
| --- | --- |
| PROJECT | ProjectまたはImport先Project |
| FLOW | Flow |
| LANES | Lane |
| STAGES | Stage |
| NODES | Node |
| LINKS | Link |
| RESPONSIBILITIES | Node.laneId / metadata |
| DATA_EFFECTS | Node.properties / metadata |
| COMMUNICATIONS | Node.properties / metadata |
| VALIDATION | Import warning/error |
| METADATA | Flow metadata |

## 5. Node変換

| DSL Node Type | SSOT Node Type |
| --- | --- |
| Start | start |
| End | end |
| Process | process |
| Decision | decision |
| Hexagon | hexagon |
| Image | image |
| Comment | comment |

不明なNode Typeはhexagonまたはprocessへ変換候補を出し、Previewで確認させる。

## 6. Decision変換

DecisionはconditionとbranchesをNode propertiesへ格納する。

branchesのtargetNodeIdはImport時ID Mappingで再解決する。

conditionがない場合はwarningを出す。

## 7. Loop変換

DSLのisLoop=true Linkは、そのままLoop Linkとして取り込む。

AI Flow DesignerはLoopを許可するため、Loop自体はerrorにしない。

## 8. Validation

| 状態 | 扱い |
| --- | --- |
| DSL_VERSIONなし | error |
| 未対応Version | error |
| NODESなし | error |
| LINK source/target不正 | error |
| Decision conditionなし | warning |
| LaneなしNode | warning |
| 不明Node Type | warning |

## 9. Preview

Import確定前に以下を表示する。

- 作成予定Flow名
- Node数
- Link数
- Lane数
- Stage数
- warning一覧
- error一覧

errorがある場合は確定不可。

## 10. 完了条件

- AI DSLからSSOTへの変換対応が定義されている
- Decision、Loop、Responsibility、通信情報の扱いが定義されている
- ValidationとPreview方針が定義されている

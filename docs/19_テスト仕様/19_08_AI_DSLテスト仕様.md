# 19_08_AI_DSLテスト仕様

## 1. 目的

AI DSL出力の品質を検証するためのテスト仕様を定義する。

AI DSLは、AIがFlow構造を曖昧なく理解するための形式であり、AI Flow Designerの価値の中心である。

## 2. 基本方針

- JSON保存形式とは別にAI理解形式として検証する
- Node / Link / Lane / Stage / Comment / Image / Metadataの欠落を確認する
- Decision、Loop、外部通信、DB影響などAI解析に重要な情報を確認する
- AIが処理順序と責務を判断できる粒度を確認する

## 3. 共通確認項目

| 項目 | 確認内容 |
| --- | --- |
| dslVersion | DSL形式Versionがある |
| project | Project情報がある |
| flow | Flow情報がある |
| lanes | Lane情報がある |
| stages | Stage情報がある |
| nodes | Node情報がある |
| links | Link情報がある |
| comments | Comment情報がある |
| images | Image情報がある |
| metadata | Metadata情報がある |
| responsibilities | 責務情報がある |
| dependencies | 依存関係がある |

## 4. Basic Flow

| Test | 条件 | 期待結果 |
| --- | --- | --- |
| AiDsl_BasicFlow_IncludesStartEnd | Start/Endあり | Start/End出力 |
| AiDsl_BasicFlow_IncludesProcessOrder | Processあり | 処理順序出力 |
| AiDsl_BasicFlow_IncludesLinks | Linkあり | 接続関係出力 |

## 5. Decision Flow

| Test | 条件 | 期待結果 |
| --- | --- | --- |
| AiDsl_Decision_IncludesCondition | Decisionあり | condition出力 |
| AiDsl_Decision_IncludesYesNoBranches | Yes/No分岐 | 分岐条件出力 |
| AiDsl_Decision_MissingCondition_ReturnsValidationWarning | 条件なし | Warning |

## 6. Loop Flow

| Test | 条件 | 期待結果 |
| --- | --- | --- |
| AiDsl_Loop_IncludesLoopReference | 循環Linkあり | loop情報出力 |
| AiDsl_Loop_DoesNotCrash | Loopあり | 出力成功 |
| AiDsl_Loop_IncludesExitConditionIfExists | Exit条件あり | exitCondition出力 |

## 7. Responsibility

| Test | 条件 | 期待結果 |
| --- | --- | --- |
| AiDsl_Lane_IncludesResponsibleArea | Laneあり | 担当領域出力 |
| AiDsl_Node_IncludesOwnerLane | Nodeあり | 所属Lane出力 |
| AiDsl_NodeWithoutLane_ReturnsWarning | LaneなしNode | Warning |

## 8. DB / API / Communication

| Test | 条件 | 期待結果 |
| --- | --- | --- |
| AiDsl_DbOperation_IncludesTableAndAction | DB操作Node | table/action出力 |
| AiDsl_ApiCall_IncludesEndpointAndMethod | API通信Node | endpoint/method出力 |
| AiDsl_PlcCommunication_IncludesSignal | PLC通信 | signal出力 |
| AiDsl_MissingExternalTarget_ReturnsWarning | 通信先不明 | Warning |

## 9. Image / Comment

| Test | 条件 | 期待結果 |
| --- | --- | --- |
| AiDsl_ImageNode_IncludesImageMetadata | Image Node | fileKey等出力 |
| AiDsl_Comment_IncludesLinkedNode | Node紐付Comment | linkedNodeId出力 |
| AiDsl_IndependentComment_IncludesPosition | 独立Comment | position出力 |

## 10. Detail Level

| Test | 条件 | 期待結果 |
| --- | --- | --- |
| AiDsl_Simple_ExcludesOptionalDetails | simple | 最小情報 |
| AiDsl_Standard_IncludesNormalDetails | standard | 標準情報 |
| AiDsl_Detailed_IncludesValidationHints | detailed | Validation Hintあり |

## 11. AI解析可能性チェック

AI DSL出力後、以下を機械的に確認する。

- Node IDが一意である
- Linkのsource/targetが存在する
- Decision Nodeに分岐情報がある
- Loopが無限に展開されない
- Lane/StageとNodeの対応が分かる
- Export設定に応じてMetadataが含まれる
- DSL Versionが明示される

## 12. 完了条件

- 基本Flow、Decision、Loopが検証されている
- 責務、通信、DB/API影響が検証されている
- Image/Commentが検証されている
- detailLevel別の出力差分が検証されている
- AIが構造を曖昧なく解析できる最低条件が定義されている

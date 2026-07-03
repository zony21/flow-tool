# 09_05_Stage構造

## 1. 目的

Stageは、Flow内の工程・フェーズ・時間的区切りを表す分類単位である。
Laneが責務の横軸であるのに対し、Stageは工程の縦軸または時系列区分として利用する。

## 2. 必須項目

| 項目 | 型 | 必須 | 内容 |
| --- | --- | --- | --- |
| stageId | string | yes | Stage識別子 |
| flowId | string | yes | 所属Flow |
| name | string | yes | Stage名 |
| description | string | no | 工程説明 |
| orderNo | number | yes | 表示順 |
| stageType | enum | yes | phase / process / timing / custom |
| style | object | no | 背景色、境界線等 |

## 3. JSON例

```json
{
  "stageId": "STAGE-READ",
  "flowId": "FLOW-0001",
  "name": "読取工程",
  "description": "RFIDを読み取り搬送条件を判定する工程",
  "orderNo": 10,
  "stageType": "process"
}
```

## 4. 設計ルール

- StageはNodeの工程分類に利用する。
- NodeはstageId未設定を許可するが、AIレビューでは工程未分類として警告できる。
- Stage削除時はNode移動または同時削除をユーザーが選択する。
- Stageは時系列を保証するものではなく、Link順序と併用して工程順を判断する。

## 5. AI向け意味

AIはStageを工程境界として扱う。
工程跨ぎのLink、工程未分類Node、工程順序の矛盾検知に利用する。

## 6. 完了条件

Stage情報により、AIがFlow内の工程区分と処理の所属工程を理解できる。

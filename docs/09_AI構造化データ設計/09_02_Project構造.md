# 09_02_Project構造

## 1. 目的

Projectは、複数のFlowを束ねる最上位単位である。
AI構造化データ上では、システム、業務、設備、機能単位の設計対象を表す。

## 2. 必須項目

| 項目 | 型 | 必須 | 内容 |
| --- | --- | --- | --- |
| projectId | string | yes | Project識別子 |
| name | string | yes | Project名 |
| description | string | no | 説明 |
| ownerUserId | string | yes | 所有者User ID |
| visibility | enum | yes | private / shared / public |
| flows | Flow[] | yes | Flow一覧 |
| createdAt | datetime | yes | 作成日時 |
| updatedAt | datetime | yes | 更新日時 |

## 3. JSON例

```json
{
  "projectId": "PRJ-0001",
  "name": "倉庫制御システム",
  "description": "WCSとAGF搬送を管理する設計Project",
  "visibility": "private",
  "flows": []
}
```

## 4. 参照関係

ProjectはFlowの親であり、Flowは必ずProjectに属する。
Project削除時は、Flow、Version、Export履歴への影響を事前に検証する。

## 5. AI向け意味

AIはProjectを設計対象全体の境界として扱う。
異なるProject間のFlowは、明示的な外部連携がない限り同一システムとして推論しない。

## 6. 完了条件

Project情報から、AIが設計対象の範囲とFlow群の所属を判断できる。

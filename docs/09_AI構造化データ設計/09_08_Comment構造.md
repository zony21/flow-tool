# 09_08_Comment構造

## 1. 目的

Commentは、Flowに対する補足、注意、設計意図、レビュー指摘、AI専用メモを保持する要素である。
独立配置とNode紐付けの両方を扱う。

## 2. 必須項目

| 項目 | 型 | 必須 | 内容 |
| --- | --- | --- | --- |
| commentId | string | yes | Comment識別子 |
| flowId | string | yes | 所属Flow |
| targetNodeId | string | no | 紐付けNode |
| commentType | enum | yes | normal / warning / review / aiMemo / note |
| body | string | yes | 本文 |
| position | object | no | 独立コメント座標 |
| createdBy | string | yes | 作成者 |
| updatedBy | string | no | 更新者 |
| createdAt | datetime | yes | 作成日時 |
| updatedAt | datetime | no | 更新日時 |

## 3. JSON例

```json
{
  "commentId": "COMMENT-001",
  "flowId": "FLOW-0001",
  "targetNodeId": "NODE-002",
  "commentType": "aiMemo",
  "body": "この判定は通信抜け検知の対象にする",
  "createdBy": "USER-001"
}
```

## 4. 設計ルール

- targetNodeIdがある場合はNodeコメントとして扱う。
- targetNodeIdがない場合は独立コメントとして扱う。
- 独立コメントはpositionを持つ。
- AI専用メモは人向け説明と分離し、必要に応じてPDFやAI DSLへ出力する。

## 5. AI向け意味

AIはcommentTypeがaiMemoのCommentを、通常コメントより強い補足情報として扱う。
ただしAIメモは構造化データの事実を上書きしない。

## 6. 完了条件

Commentから設計意図、注意点、AI解析補足を復元でき、Node紐付け有無を判別できる。

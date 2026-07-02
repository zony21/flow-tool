# 05_05_Editor保存API詳細設計

## 概要

フローエディタの編集結果を一括保存するAPIである。

Lane、Stage、Node、Link、Commentをまとめて保存し、構造整合性を担保する。

## API

| メソッド | URL | 用途 |
|---|---|---|
| PUT | /api/flows/{flowId}/structure | Flow構造一括保存 |

## Request

```json
{
  "flowId": "guid",
  "clientRevision": 12,
  "lanes": [],
  "stages": [],
  "nodes": [],
  "links": [],
  "comments": [],
  "createVersion": false,
  "changeSummary": "RFID送信処理を追加"
}
```

## 処理順序

1. 認証確認
2. 権限確認
3. 編集ロック確認
4. clientRevision確認
5. Lane検証
6. Stage検証
7. Node検証
8. Link検証
9. Comment検証
10. トランザクション開始
11. 既存構造との差分反映
12. 必要に応じてFLOW_VERSION作成
13. FLOW更新
14. コミット
15. 最新構造を返却

## バリデーション

### Lane

- LANE_IDがGUID形式
- LANE_NAME必須
- DISPLAY_ORDER重複不可

### Stage

- STAGE_IDがGUID形式
- STAGE_NAME必須
- DISPLAY_ORDER重複不可

### Node

- NODE_IDがGUID形式
- LANE_IDが同一Flow内に存在
- STAGE_IDが同一Flow内に存在
- WIDTH/HEIGHTが1以上
- POSITION_X/POSITION_Yが数値

### Link

- FROM_NODE_IDが存在
- TO_NODE_IDが存在
- FROM/TOが同一Flow
- 削除済みNodeへの接続不可
- ループは許可

### Comment

- NODE_IDがnullの場合は独立コメント
- NODE_ID指定時は同一Flow内Nodeのみ許可

## レスポンス

```json
{
  "flowId": "guid",
  "serverRevision": 13,
  "updatedAt": "2026-07-03T10:00:00Z"
}
```

## 競合

clientRevisionがサーバーと一致しない場合は409を返す。

## トランザクション

構造保存は必ず1トランザクションで行う。
途中失敗時は全ロールバックする。

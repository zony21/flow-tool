# 05_06_Node Link API詳細設計

## 概要

基本はFlow構造一括保存を推奨する。

ただし、将来のリアルタイム編集や差分保存のため、Node/Link単位APIも定義する。

## Node API

| メソッド | URL | 用途 |
|---|---|---|
| POST | /api/flows/{flowId}/nodes | Node作成 |
| PUT | /api/nodes/{nodeId} | Node更新 |
| DELETE | /api/nodes/{nodeId} | Node削除 |

## POST Node Request

```json
{
  "laneId": "guid",
  "stageId": "guid",
  "nodeTypeId": "guid",
  "displayName": "RFID番号送信",
  "processType": "Send",
  "description": "RFID番号をWCSへ送信する",
  "positionX": 120,
  "positionY": 240,
  "width": 160,
  "height": 80,
  "shapeType": "Process"
}
```

## Node削除

Node削除時は接続しているLinkも論理削除する。

対象:

- FROM_NODE_ID = nodeId
- TO_NODE_ID = nodeId

## Link API

| メソッド | URL | 用途 |
|---|---|---|
| POST | /api/flows/{flowId}/links | Link作成 |
| PUT | /api/links/{linkId} | Link更新 |
| DELETE | /api/links/{linkId} | Link削除 |

## POST Link Request

```json
{
  "fromNodeId": "guid",
  "toNodeId": "guid",
  "dataName": "RFID番号",
  "communicationType": "TCP/IP",
  "conditionText": "読取成功時",
  "description": "包装PLCからWCSへ送信",
  "lineType": "Bezier",
  "controlPointsJson": "{}"
}
```

## Link検証

- FROM_NODE_ID必須
- TO_NODE_ID必須
- 同一Flow内Node
- ループ許可
- 複数出力許可
- 複数入力許可
- 同一Node間複数Link許可

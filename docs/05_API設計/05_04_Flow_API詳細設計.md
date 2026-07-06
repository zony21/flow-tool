# 05_04_Flow API詳細設計

## API一覧

| メソッド | URL | 用途 |
|---|---|---|
| GET | /api/projects/{projectId}/flows | Flow一覧 |
| GET | /api/projects/{projectId}/flows/{flowId} | Flow詳細 |
| POST | /api/projects/{projectId}/flows | Flow作成 |
| PUT | /api/projects/{projectId}/flows/{flowId} | Flow更新 |
| DELETE | /api/projects/{projectId}/flows/{flowId} | Flow削除 |
| POST | /api/projects/{projectId}/flows/{flowId}/lock | 編集ロック取得 |
| DELETE | /api/projects/{projectId}/flows/{flowId}/lock | 編集ロック解除 |

## GET /api/projects/{projectId}/flows/{flowId}

### 用途

Flowエディタで利用する全構造データを返す。

### Response

```json
{
  "flowId": "guid",
  "projectId": "guid",
  "flowName": "RFID読取り",
  "description": "RFID読取り処理",
  "currentVersionNo": 3,
  "lanes": [],
  "stages": [],
  "nodes": [],
  "links": [],
  "comments": []
}
```

## POST /api/projects/{projectId}/flows

### Request

```json
{
  "flowName": "搬送要求",
  "description": "WCSからRCSへ搬送要求する"
}
```

### 初期作成

Flow作成時、テンプレート指定がなければ以下を初期作成する。

- デフォルトLaneなし
- デフォルトStageなし
- 空キャンバス

## PUT /api/projects/{projectId}/flows/{flowId}

### 用途

Flow基本情報を更新する。

構成要素の一括保存は `05_05_Editor保存API` で扱う。

## 編集ロック

Flow編集前にロック取得を行う。

### POST /api/projects/{projectId}/flows/{flowId}/lock

```json
{
  "lockedUntil": "2026-07-03T11:00:00Z"
}
```

### 競合時

409を返す。

```json
{
  "success": false,
  "errors": [
    {
      "code": "FLOW_LOCKED",
      "message": "別ユーザーが編集中です。"
    }
  ]
}
```

## ループ対応

Flow取得・保存APIではループをエラーにしない。

ただしMermaid sequenceDiagram出力時は表現順序を別途計算する。

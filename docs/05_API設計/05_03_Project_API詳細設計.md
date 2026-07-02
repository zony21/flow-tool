# 05_03_Project API詳細設計

## API一覧

| メソッド | URL | 用途 |
|---|---|---|
| GET | /api/projects | プロジェクト一覧 |
| GET | /api/projects/{projectId} | プロジェクト詳細 |
| POST | /api/projects | 作成 |
| PUT | /api/projects/{projectId} | 更新 |
| DELETE | /api/projects/{projectId} | 削除 |
| POST | /api/projects/{projectId}/duplicate | 複写 |

## GET /api/projects

### 用途

ログインユーザーが参照可能なプロジェクト一覧を返す。

### Response

```json
[
  {
    "projectId": "guid",
    "projectName": "包装ライン制御",
    "description": "包装ラインの制御フロー",
    "projectColor": "#1976D2",
    "flowCount": 5,
    "updatedAt": "2026-07-03T10:00:00Z"
  }
]
```

## POST /api/projects

### Request

```json
{
  "projectName": "包装ライン制御",
  "description": "説明",
  "projectColor": "#1976D2",
  "iconImageId": null
}
```

### バリデーション

| 項目 | 条件 |
|---|---|
| projectName | 必須、200文字以内 |
| description | 任意 |
| projectColor | HEX形式 |
| iconImageId | 存在する画像ID |

### 処理

1. 認証確認
2. 同名重複チェック
3. PROJECT作成
4. 権限初期データ作成
5. 201を返却

## PUT /api/projects/{projectId}

### 用途

プロジェクト情報を更新する。

### 権限

ProjectAdmin以上。

### 注意

PROJECT_NAME変更時は、同一ユーザー所有プロジェクトで重複不可。

## DELETE /api/projects/{projectId}

### 用途

プロジェクトを論理削除する。

### 処理

1. PROJECTを論理削除
2. 配下FLOWを論理削除
3. 配下構成要素も論理削除
4. 監査ログ出力

## POST /api/projects/{projectId}/duplicate

### 用途

過去プロジェクトを複写し、新規プロジェクトを作成する。

### 処理

- PROJECT再作成
- FLOW再作成
- LANE/STAGE/NODE/LINK/COMMENT再作成
- すべてのIDを再採番
- 参照IDをマッピング置換

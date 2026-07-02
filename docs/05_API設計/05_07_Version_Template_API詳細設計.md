# 05_07_Version Template API詳細設計

## Version API

| メソッド | URL | 用途 |
|---|---|---|
| GET | /api/flows/{flowId}/versions | バージョン一覧 |
| POST | /api/flows/{flowId}/versions | バージョン作成 |
| GET | /api/flow-versions/{versionId} | バージョン詳細 |
| POST | /api/flow-versions/{versionId}/restore | 復元 |

## POST /api/flows/{flowId}/versions

### Request

```json
{
  "versionName": "レビュー提出版",
  "changeSummary": "AGF搬送完了処理を追加"
}
```

### 処理

1. 現在のFlow構造を取得
2. Snapshot JSON作成
3. VERSION_NO採番
4. FLOW_VERSION作成
5. FLOW.CURRENT_VERSION_NO更新

## Restore

復元時は現在状態を退避し、選択バージョンを展開する。

## Template API

| メソッド | URL | 用途 |
|---|---|---|
| GET | /api/templates | テンプレート一覧 |
| GET | /api/templates/{templateId} | テンプレート詳細 |
| POST | /api/templates | 作成 |
| PUT | /api/templates/{templateId} | 更新 |
| DELETE | /api/templates/{templateId} | 削除 |
| POST | /api/templates/{templateId}/apply | 適用 |
| POST | /api/projects/{projectId}/templates | プロジェクトから作成 |

## テンプレート適用

テンプレート適用時はIDを再採番する。

- Lane
- Stage
- Node
- Link
- Comment

LinkのFROM/TOは旧ID→新IDのマッピングで置換する。

## テンプレート種別

- SYSTEM
- USER
- PROJECT_COPY

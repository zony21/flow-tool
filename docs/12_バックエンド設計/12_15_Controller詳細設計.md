# 12_15_Controller詳細設計

## 1. 目的

本書は、AI Flow Designer BackendのController層の詳細設計を定義する。

ControllerはHTTPリクエストを受け取り、認証・基本入力・Service呼び出し・HTTPレスポンス返却を行う層である。業務ロジックは持たない。

## 2. 基本方針

- Controllerは薄くする。
- 業務ロジックを書かない。
- Repositoryを直接呼ばない。
- DbContextを直接呼ばない。
- Entityを返さない。
- DTOを返す。
- 共通レスポンス形式を利用する。
- 例外は共通Middlewareへ委譲する。
- 認証必須APIにはAuthorizeを付与する。
- Project単位認可はServiceへ委譲する。

## 3. Controller一覧

```text
AuthController
ProjectController
FlowController
EditorController
TemplateController
VersionController
ExportController
ImageController
SettingController
```

## 4. 共通レスポンス

成功時:

```json
{
  "success": true,
  "data": {}
}
```

失敗時は例外Middlewareが返却する。

```json
{
  "success": false,
  "errorCode": "FLOW_NOT_FOUND",
  "message": "Flow was not found.",
  "details": [],
  "traceId": "00-..."
}
```

## 5. AuthController

責務:

- GitHubログイン開始
- GitHub Callback
- ログアウト
- 自分のユーザー情報取得

API:

```text
GET  /api/auth/github/login
GET  /api/auth/github/callback
POST /api/auth/logout
GET  /api/auth/me
```

方針:

- GitHub OAuthの詳細処理はAuthServiceへ委譲する。
- Callbackで受け取ったcode検証はServiceへ委譲する。
- JWT発行もControllerでは行わない。

## 6. ProjectController

API:

```text
GET    /api/projects
POST   /api/projects
GET    /api/projects/{projectId}
PUT    /api/projects/{projectId}
DELETE /api/projects/{projectId}
GET    /api/projects/{projectId}/members
POST   /api/projects/{projectId}/members
PUT    /api/projects/{projectId}/members/{userId}
DELETE /api/projects/{projectId}/members/{userId}
```

ProjectControllerはProjectServiceを呼ぶ。

注意点:

- Member管理はProjectAdmin以上が必要。
- Project削除は論理削除とする。
- Project一覧ではFlow詳細を取得しない。

## 7. FlowController

API:

```text
GET    /api/projects/{projectId}/flows
POST   /api/projects/{projectId}/flows
GET    /api/flows/{flowId}
PUT    /api/flows/{flowId}
DELETE /api/flows/{flowId}
POST   /api/flows/{flowId}/save
POST   /api/flows/{flowId}/autosave
```

Flow構造保存は `/save` を使用する。

方針:

- Flow基本情報更新とFlow構造保存を分ける。
- Flow構造保存はFlowSaveServiceへ委譲する。
- Autosaveも通常保存と同じValidationを通す。

## 8. EditorController

編集ロック専用Controllerとして分離する。

API:

```text
POST   /api/flows/{flowId}/lock
PUT    /api/flows/{flowId}/lock/extend
DELETE /api/flows/{flowId}/lock
DELETE /api/flows/{flowId}/lock/admin-release
```

方針:

- Lock取得・延長・解除をEditorLockServiceへ委譲する。
- 管理者解除はProjectAdmin以上に限定する。
- Viewerはロック取得不可。

## 9. VersionController

API:

```text
GET  /api/flows/{flowId}/versions
POST /api/flows/{flowId}/versions
GET  /api/flows/{flowId}/versions/{versionId}
POST /api/flows/{flowId}/versions/{versionId}/restore
GET  /api/flows/{flowId}/versions/compare
```

Version比較では `fromVersionId` と `toVersionId` をQueryで受け取る。

方針:

- Snapshot作成は保存済みFlowを対象とする。
- 未保存Frontend状態は受け取らない。
- 復元はFlowVersionServiceへ委譲する。

## 10. TemplateController

API:

```text
GET    /api/templates/standard
GET    /api/projects/{projectId}/templates
POST   /api/projects/{projectId}/templates
GET    /api/templates/{templateId}
PUT    /api/templates/{templateId}
DELETE /api/templates/{templateId}
POST   /api/templates/{templateId}/apply
POST   /api/projects/{projectId}/duplicate
```

方針:

- Template適用時はID再採番をServiceで行う。
- Template削除は論理削除とする。
- Project複製はTemplateServiceまたはProjectServiceへ委譲する。

## 11. ExportController

API:

```text
POST /api/flows/{flowId}/exports/mermaid-flowchart
POST /api/flows/{flowId}/exports/mermaid-sequence
POST /api/flows/{flowId}/exports/pdf
POST /api/flows/{flowId}/exports/json
POST /api/flows/{flowId}/exports/ai-dsl
```

将来:

```text
POST /api/flows/{flowId}/exports/api-spec
POST /api/flows/{flowId}/exports/db-update-list
POST /api/flows/{flowId}/exports/plc-list
POST /api/flows/{flowId}/exports/communication-list
POST /api/flows/{flowId}/exports/design-draft
```

方針:

- Exportは保存済みFlowまたはSnapshotを対象とする。
- Canvas DOMやFrontend一時状態は受け取らない。
- ExportTypeごとの処理はExportServiceへ委譲する。

## 12. ImageController

API:

```text
POST   /api/projects/{projectId}/images
GET    /api/images/{imageFileId}
DELETE /api/images/{imageFileId}
```

方針:

- Uploadはmultipart/form-dataで受け取る。
- 画像検証はImageFileServiceへ委譲する。
- 物理パスは返さない。
- 権限確認なしで画像を返さない。

## 13. SettingController

API:

```text
GET /api/settings
PUT /api/settings
GET /api/projects/{projectId}/settings
PUT /api/projects/{projectId}/settings
```

方針:

- 初期は最低限の設定のみ扱う。
- Project別設定とUser別設定は分ける。
- 将来Node拡張設定へ対応する。

## 14. DTO方針

ControllerはRequest DTOとResponse DTOのみ扱う。

例:

- CreateProjectRequest
- ProjectDto
- SaveFlowRequest
- SaveFlowResponse
- ExportRequest
- ExportResultDto
- UploadImageResponse

Entityを直接返してはならない。

## 15. 認証・認可属性

認証必須APIには `[Authorize]` を付与する。

Project単位のRole判定はServiceへ委譲する。

理由:

- ProjectIdがRoute / Body / FlowId経由など複数経路になるため。
- Service層で再確認することで認可漏れを防ぐため。

## 16. 例外処理

Controllerでは原則try-catchしない。

例外は共通ExceptionMiddlewareで処理する。

Controllerでcatchする例外は、ファイルアップロードのRequest形式不正など、HTTP境界特有の例外に限定する。

## 17. テスト観点

- ControllerがServiceを呼ぶだけになっている。
- ControllerがRepositoryを直接呼ばない。
- EntityをResponseに返さない。
- 未認証APIが401になる。
- Viewerが編集APIを呼ぶと403になる。
- Flow保存APIがFlowSaveServiceを呼ぶ。
- Export APIがFrontend一時状態を受け取らない。
- Image取得APIで権限確認が行われる。

## 18. 完了条件

- Controller一覧とAPIが定義されている。
- Controllerの責務がHTTP境界に限定されている。
- Service委譲方針が明確である。
- DTO方針、認証、例外処理方針が定義されている。
- AIが本書を読んでController実装に着手できる。

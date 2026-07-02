# 12_15_Controller詳細設計

## 1. 本書の目的

本書は、AI Flow Designer のController層の詳細設計を定義する。

ControllerはHTTPリクエストを受け取り、認証・基本入力・Service呼び出し・HTTPレスポンス返却を行う層である。業務ロジックは持たない。

## 2. 基本方針

- Controllerは薄くする
- 業務ロジックを書かない
- Repositoryを直接呼ばない
- Entityを返さない
- DTOを返す
- 共通レスポンス形式を利用する
- 例外は共通Middlewareへ委譲する
- 認証必須APIにはAuthorizeを付与する
- Project単位認可はServiceへ委譲する

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

## 8. EditorController

API:

```text
POST   /api/flows/{flowId}/lock
PUT    /api/flows/{flowId}/lock/extend
DELETE /api/flows/{flowId}/lock
DELETE /api/flows/{flowId}/lock/admin-release
```

編集ロック専用Controllerとして分離する。

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

## 12. ImageController

API:

```text
POST   /api/projects/{projectId}/images
GET    /api/images/{imageFileId}
GET    /api/projects/{projectId}/images
DELETE /api/images/{imageFileId}
```

画像取得も認証必須とする。

## 13. SettingController

API:

```text
GET /api/settings/me
PUT /api/settings/me
GET /api/projects/{projectId}/settings
PUT /api/projects/{projectId}/settings
```

個人設定とProject設定を分ける。

## 14. Controller実装例

```csharp
[ApiController]
[Route("api/flows")]
[Authorize]
public sealed class FlowController : ControllerBase
{
    private readonly IFlowService _flowService;
    private readonly IFlowSaveService _flowSaveService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public FlowController(
        IFlowService flowService,
        IFlowSaveService flowSaveService,
        ICurrentUserProvider currentUserProvider)
    {
        _flowService = flowService;
        _flowSaveService = flowSaveService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpPost("{flowId:guid}/save")]
    public async Task<ActionResult<ApiResponse<SaveFlowResultDto>>> Save(
        Guid flowId,
        SaveFlowRequest request,
        CancellationToken cancellationToken)
    {
        var user = _currentUserProvider.GetCurrentUser();

        var result = await _flowSaveService.SaveAsync(
            flowId,
            request,
            user,
            cancellationToken);

        return Ok(ApiResponse.Success(result));
    }
}
```

## 15. CurrentUserProvider

ControllerはJWT Claimを直接解析しない。

`ICurrentUserProvider` を利用する。

```csharp
public interface ICurrentUserProvider
{
    CurrentUser GetCurrentUser();
}
```

CurrentUser:

```csharp
public sealed class CurrentUser
{
    public Guid UserId { get; init; }
    public string LoginName { get; init; }
    public bool IsSystemAdmin { get; init; }
}
```

## 16. 入力DTO

ControllerはRequest DTOを受け取る。

DTOにはEntityを利用しない。

例:

```csharp
public sealed class SaveFlowRequest
{
    public long Revision { get; init; }
    public bool IsAutoSave { get; init; }
    public IReadOnlyList<SaveLaneRequest> Lanes { get; init; }
    public IReadOnlyList<SaveStageRequest> Stages { get; init; }
    public IReadOnlyList<SaveNodeRequest> Nodes { get; init; }
    public IReadOnlyList<SaveLinkRequest> Links { get; init; }
    public IReadOnlyList<SaveCommentRequest> Comments { get; init; }
}
```

## 17. CancellationToken

Controller Actionでは必ずCancellationTokenを受け取り、Serviceへ渡す。

```csharp
public async Task<ActionResult<ApiResponse<ProjectDto>>> Get(
    Guid projectId,
    CancellationToken cancellationToken)
```

## 18. ModelState

ASP.NET CoreのModelState不正は共通エラー形式へ変換する。

自動400レスポンスの形式が共通形式とズレる場合、InvalidModelStateResponseFactoryを設定する。

## 19. Route制約

GUIDパラメータには `{id:guid}` を付ける。

例:

```text
/api/flows/{flowId:guid}
```

## 20. ファイルアップロードController

ImageControllerではIFormFileを受け取る。

```csharp
[HttpPost("/api/projects/{projectId:guid}/images")]
public async Task<ActionResult<ApiResponse<ImageFileDto>>> Upload(
    Guid projectId,
    IFormFile file,
    [FromForm] Guid? flowId,
    CancellationToken cancellationToken)
```

ファイル検証はControllerではなくServiceで行う。

## 21. 禁止事項

- ControllerでDbContextを注入する
- ControllerでRepositoryを注入する
- Controllerで業務ルールを書く
- ControllerでTransactionを開始する
- Controllerでtry-catchを多用する
- ControllerでEntityを返す
- Controllerで物理ファイルパスを返す

## 22. テスト観点

Controller単体:

- 正しいServiceを呼ぶ
- CurrentUserを渡す
- CancellationTokenを渡す
- ApiResponseで返す

結合:

- 未認証で401
- 不正GUIDで404または400
- Validation不正で400
- Service例外が共通形式へ変換される

## 23. 完了条件

- Controllerが薄く保たれている
- 全APIがDTOを返す
- Entity返却がない
- 認証必須APIにAuthorizeがある
- Project権限はServiceで確認される
- 共通レスポンス形式で統一されている

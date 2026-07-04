# 18_06_Backend設計

## 1. 目的

設定機能のBackend設計を定義する。

ASP.NET Core .NET 8、Entity Framework Core、SQLiteを前提に、Controller、Service、Validation、Default値、権限制御を整理する。

## 2. 基本方針

- ControllerはHTTP入出力に集中する
- 設定取得・保存はSettingServiceに集約する
- 初期値はSettingDefaultServiceで一元管理する
- 保存前ValidationはSettingValidationServiceで行う
- Project単位設定はPermissionServiceで認可する
- 設定レコードが存在しない場合でもDefault値を返す

## 3. Backend構成

| 部品 | 役割 |
| --- | --- |
| UserSettingController | User Setting API |
| ProjectSettingController | Project / Editor / AI / Export Setting API |
| SettingService | 設定取得・保存の中心Service |
| SettingDefaultService | 初期値生成 |
| SettingValidationService | 設定値Validation |
| PermissionService | 17章の権限判定 |
| CurrentUserService | ログインUser取得 |

## 4. Controller設計

### 4.1 UserSettingController

責務:

- User Setting取得
- User Setting更新

API:

```text
GET /api/settings/user
PUT /api/settings/user
```

### 4.2 ProjectSettingController

責務:

- Project Setting取得・更新
- Editor Setting取得・更新
- AI Setting取得・更新
- Export Setting取得・更新

API:

```text
GET /api/projects/{projectId}/settings/project
PUT /api/projects/{projectId}/settings/project
GET /api/projects/{projectId}/settings/editor
PUT /api/projects/{projectId}/settings/editor
GET /api/projects/{projectId}/settings/ai
PUT /api/projects/{projectId}/settings/ai
GET /api/projects/{projectId}/settings/export
PUT /api/projects/{projectId}/settings/export
```

## 5. Service設計

### 5.1 ISettingService

```csharp
public interface ISettingService
{
    Task<UserSettingDto> GetUserSettingAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserSettingDto> UpdateUserSettingAsync(Guid userId, UpdateUserSettingRequest request, CancellationToken cancellationToken = default);

    Task<ProjectSettingDto> GetProjectSettingAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<ProjectSettingDto> UpdateProjectSettingAsync(Guid userId, Guid projectId, UpdateProjectSettingRequest request, CancellationToken cancellationToken = default);

    Task<EditorSettingDto> GetEditorSettingAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default);
    Task<EditorSettingDto> UpdateEditorSettingAsync(Guid userId, Guid projectId, UpdateEditorSettingRequest request, CancellationToken cancellationToken = default);

    Task<AiSettingDto> GetAiSettingAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<AiSettingDto> UpdateAiSettingAsync(Guid userId, Guid projectId, UpdateAiSettingRequest request, CancellationToken cancellationToken = default);

    Task<ExportSettingDto> GetExportSettingAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<ExportSettingDto> UpdateExportSettingAsync(Guid userId, Guid projectId, UpdateExportSettingRequest request, CancellationToken cancellationToken = default);
}
```

## 6. Default値方針

SettingDefaultServiceで分類ごとのDefault値を返す。

```csharp
public interface ISettingDefaultService
{
    UserSettingDto CreateDefaultUserSetting();
    ProjectSettingDto CreateDefaultProjectSetting();
    EditorSettingDto CreateDefaultEditorSetting();
    AiSettingDto CreateDefaultAiSetting();
    ExportSettingDto CreateDefaultExportSetting();
}
```

設定レコードが存在しない場合:

1. Default値を返す
2. 必要に応じてDBへ作成する
3. GETだけではDB作成しない設計も可

初期実装では、GET時に未登録ならDefault値を返し、PUT時にUpsertする。

## 7. Validation設計

SettingValidationServiceで保存前に検証する。

検証内容:

- 必須
- 選択肢
- 数値範囲
- JSON形式
- Project存在確認
- User存在確認

例:

| 項目 | Validation |
| --- | --- |
| theme | light / dark / system |
| defaultZoom | 0.2〜3.0 |
| gridSize | 4〜64 |
| defaultLinkType | bezier / straight / step / smoothstep |
| reviewStrictness | low / normal / high |
| pdfPageSize | A4 / A3 |

## 8. 認可設計

17章のPermissionServiceを使用する。

| 処理 | Permission |
| --- | --- |
| User Setting更新 | Setting.UserUpdate |
| Project Setting更新 | Setting.ProjectUpdate |
| Editor Setting更新 | Setting.UserUpdate |
| AI Setting更新 | Setting.AiUpdate |
| Export Setting更新 | Setting.ExportUpdate |

User SettingとEditor Settingは個人設定のため、ログインUser本人のみ更新できる。

Project Setting / AI Setting / Export SettingはOwnerのみ更新可能とする。

## 9. Upsert方針

設定更新時、対象レコードが存在しない場合は作成する。

処理:

1. 対象設定を検索
2. 存在しない場合はDefault値からEntity作成
3. Request値を反映
4. Validation
5. 保存
6. DTO返却

## 10. AuditLog方針

以下の更新はAuditLogへ記録する。

- Project Setting更新
- AI Setting更新
- Export Setting更新

User SettingとEditor Settingは個人設定のため、初期実装ではAuditLog対象外とする。

## 11. Error方針

| Error | HTTP Status |
| --- | --- |
| 未ログイン | 401 |
| 権限不足 | 403 |
| Projectなし | 404 |
| Validation Error | 400 |
| 保存失敗 | 500 |

## 12. 完了条件

- Controller構成が定義されている
- Service Interfaceが定義されている
- Default値方針が定義されている
- Validation方針が定義されている
- PermissionServiceとの連携が明確である
- EF Core実装に展開できる

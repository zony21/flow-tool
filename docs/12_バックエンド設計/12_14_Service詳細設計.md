# 12_14_Service詳細設計

## 1. 目的

本書は、AI Flow Designer BackendのService層の詳細設計を定義する。

Service層は、Controllerから呼び出され、業務ルール、Transaction、Repository呼び出し、DTO変換、例外送出を制御する中核層である。

## 2. 基本方針

- Controllerを薄くする。
- 業務ロジックはServiceへ集約する。
- Repositoryを直接Controllerから呼ばない。
- ServiceでTransaction境界を管理する。
- Serviceで認可確認を行う。
- ServiceでRevision競合を確認する。
- Serviceで編集ロックを確認する。
- ServiceはDTOまたはApplication Modelを返す。
- EntityをAPIレスポンスとして返さない。
- RepositoryでSaveChangesさせない。

## 3. Service一覧

```text
AuthService
ProjectService
ProjectAuthorizationService
FlowService
FlowQueryService
FlowSaveService
EditorLockService
TemplateService
FlowVersionService
ExportService
ImageFileService
SettingService
```

## 4. ProjectService

責務:

- Project作成
- Project更新
- Project削除
- Project一覧取得
- メンバー管理
- Project権限連携

主なメソッド:

```csharp
Task<ProjectDto> CreateAsync(CreateProjectRequest request, CurrentUser user, CancellationToken ct);
Task<ProjectDto> UpdateAsync(Guid projectId, UpdateProjectRequest request, CurrentUser user, CancellationToken ct);
Task DeleteAsync(Guid projectId, CurrentUser user, CancellationToken ct);
Task<PagedResult<ProjectListDto>> SearchAsync(ProjectSearchRequest request, CurrentUser user, CancellationToken ct);
```

Project作成時はPROJECTとPROJECT_MEMBERを同一Transactionで作成し、作成者をProjectAdminにする。

## 5. ProjectAuthorizationService

責務:

- Project権限確認
- Role判定
- Admin判定
- Viewer / Editor / ProjectAdmin判定

主なメソッド:

```csharp
Task EnsureCanViewAsync(Guid projectId, CurrentUser user, CancellationToken ct);
Task EnsureCanEditAsync(Guid projectId, CurrentUser user, CancellationToken ct);
Task EnsureCanAdminAsync(Guid projectId, CurrentUser user, CancellationToken ct);
Task<ProjectRole> GetRoleAsync(Guid projectId, CurrentUser user, CancellationToken ct);
```

重要操作ではControllerのAuthorizeに加えてService層で再確認する。

## 6. FlowService

責務:

- Flow作成
- Flow更新
- Flow削除
- Flow一覧取得
- Flow基本情報管理

Flow構造全体の保存はFlowSaveServiceへ委譲する。

主なメソッド:

```csharp
Task<FlowDto> CreateAsync(Guid projectId, CreateFlowRequest request, CurrentUser user, CancellationToken ct);
Task<FlowDto> UpdateAsync(Guid flowId, UpdateFlowRequest request, CurrentUser user, CancellationToken ct);
Task DeleteAsync(Guid flowId, CurrentUser user, CancellationToken ct);
Task<IReadOnlyList<FlowListDto>> FindByProjectAsync(Guid projectId, CurrentUser user, CancellationToken ct);
```

## 7. FlowQueryService

責務:

- Flow詳細取得
- Export用Flow構造取得
- Version用Snapshot元データ取得
- 参照専用ReadModel作成

FlowQueryServiceは原則AsNoTrackingで取得する。

主なメソッド:

```csharp
Task<FlowDetailDto> GetDetailAsync(Guid flowId, CurrentUser user, CancellationToken ct);
Task<FlowStructureModel> GetStructureAsync(Guid flowId, CancellationToken ct);
Task<ExportInputModel> GetExportInputAsync(Guid flowId, Guid? snapshotId, ExportOptions options, CancellationToken ct);
```

## 8. FlowSaveService

FlowSaveServiceはBackendで最も重要なServiceである。

責務:

- Flow一括保存
- 自動保存
- Lane / Stage / Node / Link / Comment差分反映
- Revision確認
- 編集ロック確認
- 構造整合性検証
- Transaction管理
- Snapshot作成連携

主なメソッド:

```csharp
Task<SaveFlowResponse> SaveAsync(Guid flowId, SaveFlowRequest request, CurrentUser user, CancellationToken ct);
Task<SaveFlowResponse> AutoSaveAsync(Guid flowId, SaveFlowRequest request, CurrentUser user, CancellationToken ct);
```

## 9. FlowSaveService処理

1. ユーザー権限確認。
2. Flow存在確認。
3. 編集ロック確認。
4. Revision確認。
5. DTO Validation。
6. 構造整合性Validation。
7. Transaction開始。
8. 差分分類。
9. Lane反映。
10. Stage反映。
11. Node反映。
12. Link反映。
13. Comment反映。
14. Revision更新。
15. SaveChanges。
16. Commit。
17. 結果DTO返却。

## 10. 差分分類

保存DTOの各要素は以下に分類する。

- Added
- Updated
- Deleted
- Unchanged

削除は物理削除ではなく論理削除とする。
全削除・全再作成は原則禁止する。

## 11. Lane削除Service処理

Lane削除時はユーザー選択に従う。

選択肢:

- MoveNodes
- DeleteWithNodes

MoveNodes:

1. 移動先Lane確認。
2. 対象NodeのLaneId更新。
3. Lane論理削除。

DeleteWithNodes:

1. Lane配下Node論理削除。
2. 関連Link論理削除。
3. Node紐付けComment処理。
4. Lane論理削除。

## 12. Stage削除Service処理

Stage削除もLane削除と同様。

選択肢:

- MoveNodes
- DeleteWithNodes

Stageの場合は移動先StageへNodeを移動する。

## 13. EditorLockService

責務:

- Flow単位ロック取得
- ロック延長
- ロック解除
- 管理者解除
- タイムアウト判定

主なメソッド:

```csharp
Task<EditorLockDto> AcquireAsync(Guid flowId, CurrentUser user, CancellationToken ct);
Task<EditorLockDto> ExtendAsync(Guid flowId, CurrentUser user, CancellationToken ct);
Task ReleaseAsync(Guid flowId, CurrentUser user, CancellationToken ct);
Task AdminReleaseAsync(Guid flowId, CurrentUser user, CancellationToken ct);
```

ロックはFlow保存前に必ず確認する。

## 14. FlowVersionService

責務:

- Version作成
- Version一覧
- Version比較
- Version復元
- Snapshot生成
- Snapshot読込

主なメソッド:

```csharp
Task<FlowVersionDto> CreateAsync(Guid flowId, CreateVersionRequest request, CurrentUser user, CancellationToken ct);
Task<IReadOnlyList<FlowVersionListDto>> FindByFlowAsync(Guid flowId, CurrentUser user, CancellationToken ct);
Task RestoreAsync(Guid flowId, Guid versionId, CurrentUser user, CancellationToken ct);
```

Version復元時は、現在状態を復元前Versionとして保存してから復元する。

## 15. TemplateService

責務:

- 標準Template取得
- Project Template作成
- Template更新
- Template削除
- Template適用
- Project複製

Template適用時はIDを再採番する。
Template適用はTransaction必須とする。

## 16. ExportService

責務:

- Export要求受付
- Flow / Snapshot取得
- ExportInputModel作成
- 形式別Renderer呼び出し
- Export結果返却

対象形式:

- Mermaid
- PDF
- JSON
- AI DSL

Exportは保存済みFlowまたはSnapshotを入力とする。
Frontend Canvas DOMは利用しない。

## 17. ImageFileService

責務:

- 画像アップロード
- ファイル検証
- Hash計算
- StorageKey生成
- IMAGE_FILE登録
- 画像取得
- 画像削除

画像実体とDBメタデータの整合に注意する。

## 18. AuthService

責務:

- GitHub OAuth Callback処理
- GitHub User情報取得
- USER_ACCOUNT作成・更新
- JWT発行
- ログアウト処理
- /me情報返却

OAuth tokenやJWTをログに出さない。

## 19. 例外送出方針

Service層は意味のある例外を送出する。

例:

- ValidationException
- BusinessRuleException
- NotFoundException
- ForbiddenException
- ConflictException
- ExportException
- FileStorageException

Controllerで個別try-catchせず、Middlewareで標準レスポンス化する。

## 20. テスト観点

- FlowSaveServiceがTransactionを開始する。
- FlowSaveServiceでRevision競合を検出する。
- Lane削除MoveNodesが正しく動作する。
- Template適用時にID再採番される。
- Version復元時に現在状態が事前保存される。
- ExportServiceがCanvas DOMに依存しない。
- ImageFileServiceが不正MIMEを拒否する。

## 21. 完了条件

- 主要Serviceの責務が明確である。
- FlowSaveService、Version、Template、Export、画像、認証の処理方針が定義されている。
- Transactionと例外送出の責務がServiceにある。
- AIが本書を読んでService層実装に着手できる。

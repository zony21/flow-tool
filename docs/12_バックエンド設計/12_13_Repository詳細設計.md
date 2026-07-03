# 12_13_Repository詳細設計

## 1. 目的

本書は、AI Flow Designer BackendのRepository層の詳細設計を定義する。

RepositoryはEF Coreを利用した永続化処理を担当する。ただし、業務判断、認可判定、Transaction開始、SaveChanges、API DTO組立の責務を持たない。

## 2. 基本方針

- RepositoryはInterfaceを定義する。
- ServiceはInterface経由でRepositoryを利用する。
- RepositoryはDbContextを利用する。
- RepositoryでSaveChangesを呼ばない。
- RepositoryでTransactionを開始しない。
- Repositoryで業務例外を作らない。
- RepositoryはEntityまたはReadModelを返す。
- API DTOへの変換はServiceまたはMapperで行う。
- 検索用Repositoryと更新用Repositoryを分けてもよい。

## 3. 責務

Repositoryの責務:

- Entity取得
- Entity追加
- Entity更新準備
- Entity論理削除準備
- Exists判定
- Count取得
- 一覧取得
- DB固有の取得最適化

Repositoryの責務外:

- 認可
- 業務ルール判定
- Transaction制御
- SaveChanges
- APIレスポンス作成
- エラーレスポンス作成
- ログインユーザー判定

## 4. 配置

```text
FlowDesigner.Application/Interfaces/Repositories
FlowDesigner.Infrastructure/Repositories
```

Interface例:

```text
IProjectRepository
IFlowRepository
ILaneRepository
IStageRepository
IFlowNodeRepository
IFlowLinkRepository
IFlowCommentRepository
IFlowVersionRepository
ITemplateRepository
IImageFileRepository
IProjectMemberRepository
```

実装例:

```text
ProjectRepository
FlowRepository
LaneRepository
StageRepository
FlowNodeRepository
FlowLinkRepository
FlowCommentRepository
FlowVersionRepository
TemplateRepository
ImageFileRepository
ProjectMemberRepository
```

## 5. 命名規則

Interface:

```text
I{EntityName}Repository
```

実装:

```text
{EntityName}Repository
```

## 6. DbContext注入

RepositoryはConstructor InjectionでDbContextを受け取る。

```csharp
public sealed class FlowRepository : IFlowRepository
{
    private readonly AppDbContext _dbContext;

    public FlowRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
}
```

## 7. SaveChanges禁止

Repository内で以下は禁止する。

```csharp
await _dbContext.SaveChangesAsync();
```

理由:

- Transaction境界が不明になる。
- 複数Repository更新の整合性が崩れる。
- Flow一括保存で部分保存が発生する。
- テストが難しくなる。

SaveChangesはServiceまたはUnitOfWorkで行う。

## 8. Transaction禁止

Repository内で以下は禁止する。

```csharp
await _dbContext.Database.BeginTransactionAsync();
```

TransactionはService層で開始する。

## 9. ProjectRepository

責務:

- Project取得
- Project一覧取得
- Project作成
- Project名重複確認
- 論理削除対象取得

主要メソッド:

```csharp
Task<Project?> FindByIdAsync(Guid projectId, CancellationToken ct);
Task<Project?> FindByIdForUpdateAsync(Guid projectId, CancellationToken ct);
Task<bool> ExistsByNameAsync(string name, Guid userId, CancellationToken ct);
Task<IReadOnlyList<Project>> SearchAsync(ProjectSearchCondition condition, CancellationToken ct);
void Add(Project project);
void Update(Project project);
```

## 10. ProjectMemberRepository

責務:

- Project権限確認
- ProjectMember取得
- Member追加・更新・論理削除

主要メソッド:

```csharp
Task<ProjectMember?> FindAsync(Guid projectId, Guid userId, CancellationToken ct);
Task<bool> HasRoleAsync(Guid projectId, Guid userId, ProjectRole requiredRole, CancellationToken ct);
Task<IReadOnlyList<ProjectMember>> FindByProjectIdAsync(Guid projectId, CancellationToken ct);
void Add(ProjectMember member);
void Update(ProjectMember member);
```

## 11. FlowRepository

責務:

- Flow取得
- Project配下Flow一覧
- Revision確認
- Flow作成
- Flow更新
- Flow論理削除

主要メソッド:

```csharp
Task<Flow?> FindByIdAsync(Guid flowId, CancellationToken ct);
Task<Flow?> FindForUpdateAsync(Guid flowId, CancellationToken ct);
Task<IReadOnlyList<Flow>> FindByProjectIdAsync(Guid projectId, CancellationToken ct);
Task<bool> ExistsByNameAsync(Guid projectId, string flowName, CancellationToken ct);
void Add(Flow flow);
void Update(Flow flow);
```

## 12. LaneRepository / StageRepository

Lane / StageはFlowId単位で取得する。

```csharp
Task<IReadOnlyList<Lane>> FindByFlowIdAsync(Guid flowId, CancellationToken ct);
Task<Dictionary<Guid, Lane>> FindMapByFlowIdAsync(Guid flowId, CancellationToken ct);
void AddRange(IEnumerable<Lane> lanes);
void UpdateRange(IEnumerable<Lane> lanes);
```

Stageも同様とする。

## 13. FlowNodeRepository

責務:

- FlowId単位Node取得
- Node一括追加
- Node一括更新
- Node論理削除

```csharp
Task<IReadOnlyList<FlowNode>> FindByFlowIdAsync(Guid flowId, CancellationToken ct);
Task<Dictionary<Guid, FlowNode>> FindMapByFlowIdAsync(Guid flowId, CancellationToken ct);
void AddRange(IEnumerable<FlowNode> nodes);
void UpdateRange(IEnumerable<FlowNode> nodes);
```

## 14. FlowLinkRepository

責務:

- FlowId単位Link取得
- Link一括追加
- Link一括更新
- Link論理削除

```csharp
Task<IReadOnlyList<FlowLink>> FindByFlowIdAsync(Guid flowId, CancellationToken ct);
Task<Dictionary<Guid, FlowLink>> FindMapByFlowIdAsync(Guid flowId, CancellationToken ct);
void AddRange(IEnumerable<FlowLink> links);
void UpdateRange(IEnumerable<FlowLink> links);
```

NodeごとにLinkを取得してはならない。

## 15. FlowCommentRepository

責務:

- FlowId単位Comment取得
- Target別Comment取得
- Comment一括追加・更新

```csharp
Task<IReadOnlyList<FlowComment>> FindByFlowIdAsync(Guid flowId, CancellationToken ct);
Task<IReadOnlyList<FlowComment>> FindByTargetAsync(string targetType, Guid targetId, CancellationToken ct);
void AddRange(IEnumerable<FlowComment> comments);
void UpdateRange(IEnumerable<FlowComment> comments);
```

## 16. FlowVersionRepository

責務:

- Version一覧取得
- 最新VersionNo取得
- Snapshot取得
- Snapshot追加

```csharp
Task<int> GetLatestVersionNoAsync(Guid flowId, CancellationToken ct);
Task<IReadOnlyList<FlowVersionSnapshot>> FindByFlowIdAsync(Guid flowId, CancellationToken ct);
Task<FlowVersionSnapshot?> FindByIdAsync(Guid versionId, CancellationToken ct);
void Add(FlowVersionSnapshot snapshot);
```

## 17. TemplateRepository

責務:

- 標準Template取得
- Project別Template取得
- Template作成・更新・削除

```csharp
Task<IReadOnlyList<Template>> FindStandardAsync(CancellationToken ct);
Task<IReadOnlyList<Template>> FindByProjectIdAsync(Guid projectId, CancellationToken ct);
Task<Template?> FindByIdAsync(Guid templateId, CancellationToken ct);
void Add(Template template);
void Update(Template template);
```

## 18. ImageFileRepository

責務:

- ImageFile取得
- Project / Flow単位画像一覧
- Hash重複確認
- メタデータ登録
- 論理削除

```csharp
Task<ImageFile?> FindByIdAsync(Guid imageFileId, CancellationToken ct);
Task<IReadOnlyList<ImageFile>> FindByProjectIdAsync(Guid projectId, CancellationToken ct);
Task<bool> ExistsByHashAsync(Guid projectId, string hashSha256, CancellationToken ct);
void Add(ImageFile imageFile);
void Update(ImageFile imageFile);
```

## 19. Query最適化

- ReadOnly取得ではAsNoTrackingを使う。
- 一覧APIではProjectionを使う。
- Flow詳細ではFlowId単位で一括取得する。
- N+1を禁止する。
- IS_DELETED条件を必ず考慮する。

## 20. テスト観点

- RepositoryでSaveChangesを呼ばない。
- RepositoryでTransactionを開始しない。
- FlowId単位でNode / Link / Commentを一括取得できる。
- 論理削除済データが通常検索に出ない。
- ProjectMember権限確認ができる。
- ImageFileをHashで検索できる。

## 21. 完了条件

- Repository Interfaceと実装責務が明確である。
- 主要EntityごとのRepository方針が定義されている。
- Transaction / SaveChangesがRepository外にある。
- AIが本書を読んでRepository実装に着手できる。

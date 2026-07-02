# 12_13_Repository詳細設計

## 1. 本書の目的

本書は、AI Flow Designer のRepository層の詳細設計を定義する。

RepositoryはEF Coreを利用した永続化処理を担当する。ただし、業務判断、認可判定、Transaction開始、DTO組立の責務を持たない。

## 2. 基本方針

- RepositoryはInterfaceを定義する
- ServiceはInterface経由でRepositoryを利用する
- RepositoryはDbContextを利用する
- RepositoryでSaveChangesを呼ばない
- RepositoryでTransactionを開始しない
- Repositoryで業務例外を作らない
- RepositoryはEntityを返す
- API DTOへの変換はServiceまたはMapperで行う
- 検索用Repositoryと更新用Repositoryを分けてもよい

## 3. 責務

Repositoryの責務:

- Entity取得
- Entity追加
- Entity更新準備
- Entity削除準備
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

## 4. 命名規則

Interface:

```text
I{EntityName}Repository
```

実装:

```text
{EntityName}Repository
```

例:

```text
IProjectRepository
ProjectRepository
IFlowRepository
FlowRepository
IFlowNodeRepository
FlowNodeRepository
```

## 5. 配置

```text
Backend/
  Domain/
    Entities/
  Application/
    Interfaces/
      Repositories/
  Infrastructure/
    Persistence/
      Repositories/
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

Repository内で以下は禁止。

```csharp
await _dbContext.SaveChangesAsync();
```

理由:

- Transaction境界が不明になる
- 複数Repository更新の整合性が崩れる
- Flow一括保存で部分保存が発生する
- テストが難しくなる

SaveChangesはServiceまたはUnitOfWorkで行う。

## 8. Transaction禁止

Repository内で以下は禁止。

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
Task<bool> ExistsByNameAsync(string name, Guid userId, CancellationToken ct);
Task<IReadOnlyList<Project>> SearchAsync(ProjectSearchCondition condition, CancellationToken ct);
void Add(Project project);
void Update(Project project);
```

## 10. FlowRepository

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
void Add(Flow flow);
void Update(Flow flow);
```

## 11. LaneRepository

主要メソッド:

```csharp
Task<IReadOnlyList<Lane>> FindByFlowIdAsync(Guid flowId, CancellationToken ct);
Task<Dictionary<Guid, Lane>> FindMapByFlowIdAsync(Guid flowId, CancellationToken ct);
void AddRange(IEnumerable<Lane> lanes);
void UpdateRange(IEnumerable<Lane> lanes);
```

## 12. StageRepository

LaneRepositoryと同様にFlowId単位で取得する。

```csharp
Task<IReadOnlyList<Stage>> FindByFlowIdAsync(Guid flowId, CancellationToken ct);
Task<Dictionary<Guid, Stage>> FindMapByFlowIdAsync(Guid flowId, CancellationToken ct);
void AddRange(IEnumerable<Stage> stages);
void UpdateRange(IEnumerable<Stage> stages);
```

## 13. FlowNodeRepository

1000ノード規模を想定し、一括処理を基本とする。

```csharp
Task<IReadOnlyList<FlowNode>> FindByFlowIdAsync(Guid flowId, CancellationToken ct);
Task<Dictionary<Guid, FlowNode>> FindMapByFlowIdAsync(Guid flowId, CancellationToken ct);
void AddRange(IEnumerable<FlowNode> nodes);
void UpdateRange(IEnumerable<FlowNode> nodes);
```

Node単位取得をループで呼ばない。

## 14. FlowLinkRepository

10000リンク規模を想定する。

```csharp
Task<IReadOnlyList<FlowLink>> FindByFlowIdAsync(Guid flowId, CancellationToken ct);
Task<IReadOnlyList<FlowLink>> FindByNodeIdAsync(Guid nodeId, CancellationToken ct);
void AddRange(IEnumerable<FlowLink> links);
void UpdateRange(IEnumerable<FlowLink> links);
```

## 15. FlowCommentRepository

```csharp
Task<IReadOnlyList<FlowComment>> FindByFlowIdAsync(Guid flowId, CancellationToken ct);
void AddRange(IEnumerable<FlowComment> comments);
void UpdateRange(IEnumerable<FlowComment> comments);
```

Commentは独立コメントとノード紐付けコメントの両方を扱う。

## 16. TemplateRepository

```csharp
Task<Template?> FindByIdAsync(Guid templateId, CancellationToken ct);
Task<bool> ExistsByNameAsync(Guid? projectId, string name, CancellationToken ct);
Task<IReadOnlyList<Template>> FindStandardTemplatesAsync(CancellationToken ct);
Task<IReadOnlyList<Template>> FindUserTemplatesAsync(Guid projectId, CancellationToken ct);
void Add(Template template);
void Update(Template template);
```

## 17. FlowVersionRepository

Version一覧ではSnapshot本文を取得しない。

```csharp
Task<IReadOnlyList<FlowVersionSummary>> FindSummariesByFlowIdAsync(Guid flowId, CancellationToken ct);
Task<FlowVersion?> FindByIdWithSnapshotAsync(Guid versionId, CancellationToken ct);
void Add(FlowVersion version);
```

## 18. ProjectMemberRepository

```csharp
Task<ProjectMember?> FindAsync(Guid projectId, Guid userId, CancellationToken ct);
Task<IReadOnlyList<ProjectMember>> FindByProjectIdAsync(Guid projectId, CancellationToken ct);
void Add(ProjectMember member);
void Update(ProjectMember member);
```

権限確認で頻繁に使うため、ProjectId/UserIdにインデックスを設定する。

## 19. Query Repository

参照専用で複数テーブルを組み立てる場合はQueryRepositoryを許可する。

例:

```text
IFlowQueryRepository
```

責務:

- Flow詳細取得
- Export用構造取得
- 一覧DTO用Projection

ただしAPI DTOを返すか、Application層専用のReadModelを返す。

## 20. ReadModel

ReadModel例:

```csharp
public sealed class FlowStructureReadModel
{
    public Flow Flow { get; init; }
    public IReadOnlyList<Lane> Lanes { get; init; }
    public IReadOnlyList<Stage> Stages { get; init; }
    public IReadOnlyList<FlowNode> Nodes { get; init; }
    public IReadOnlyList<FlowLink> Links { get; init; }
    public IReadOnlyList<FlowComment> Comments { get; init; }
}
```

## 21. AsNoTrackingルール

Repositoryメソッド名で意図を明確にする。

- FindByIdAsync: 更新可能Entity
- FindByIdAsNoTrackingAsync: 参照専用
- SearchAsync: 原則AsNoTracking
- FindForUpdateAsync: 更新前提

## 22. 論理削除

Repositoryは原則 `IsDeleted == false` を条件に含める。

削除済も含める場合はメソッド名に明示する。

```csharp
FindByIdIncludingDeletedAsync
```

## 23. CancellationToken

すべての非同期RepositoryメソッドはCancellationTokenを受け取る。

理由:

- HTTPリクエスト中断時の処理停止
- Export中断
- 大量Flow取得のキャンセル

## 24. テスト方針

Repositoryテストでは以下を確認する。

- 論理削除済が取得されない
- FlowId単位でNode/Linkが取得される
- AsNoTrackingが参照系で利用される
- Version一覧でSnapshot本文を取得しない
- ProjectMember検索が正しい

SQLite InMemoryではなく、可能であればテスト用SQLiteファイルを利用する。

## 25. 完了条件

- Repository内でSaveChangesしない
- Repository内でTransaction開始しない
- ServiceがInterface経由で利用する
- 参照系でN+1が発生しない
- 大量Node/Link取得に耐えられる
- 論理削除条件が統一されている

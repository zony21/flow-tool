# 12_16_ExportService設計

## 1. 目的

本書は、AI Flow Designer BackendにおけるExportService設計を定義する。

ExportServiceは、保存済みFlowまたはVersion Snapshotを入力として、Mermaid、PDF、JSON、AI DSLなどの成果物を生成する。ExportはFrontendのCanvas DOMを印刷する処理ではなく、SSOTから正式成果物を生成するBackend機能である。

## 2. 基本方針

- ExportはBackendで実行する。
- 入力は保存済みFlowまたはSnapshotとする。
- Frontend一時状態やCanvas DOMを入力にしない。
- ExportServiceは形式別Rendererへ処理を委譲する。
- ExportInputModelを共通入力とする。
- Export中にDB Transactionを保持しない。
- PDFなど重い処理は将来非同期Job化できる構成にする。

## 3. 対象Export形式

初期対象:

- Mermaid Flowchart
- Mermaid Sequence
- PDF
- JSON
- AI DSL

将来対象:

- API仕様
- DB更新一覧
- PLC一覧
- 通信一覧
- 設計書ドラフト
- コード生成補助

## 4. Service構成

```text
ExportController
 -> ExportService
 -> ProjectAuthorizationService
 -> FlowQueryService / FlowVersionService
 -> ExportInputBuilder
 -> Format Renderer
```

Format Renderer:

```text
IMermaidExportRenderer
IPdfExportRenderer
IJsonExportRenderer
IAiDslExportRenderer
```

## 5. ExportService責務

- ExportRequest検証
- Project権限確認
- Export対象確認
- FlowまたはSnapshot読込
- ExportInputModel作成
- 形式別Renderer呼び出し
- ExportResult作成
- Exportログ出力
- Export例外の意味付け

## 6. ExportInputModel

Rendererへ渡す共通Modelを定義する。

```csharp
public sealed class ExportInputModel
{
    public ProjectExportModel Project { get; init; }
    public FlowExportModel Flow { get; init; }
    public IReadOnlyList<LaneExportModel> Lanes { get; init; }
    public IReadOnlyList<StageExportModel> Stages { get; init; }
    public IReadOnlyList<NodeExportModel> Nodes { get; init; }
    public IReadOnlyList<LinkExportModel> Links { get; init; }
    public IReadOnlyList<CommentExportModel> Comments { get; init; }
    public ExportOptions Options { get; init; }
    public SnapshotInfo? Snapshot { get; init; }
}
```

## 7. ExportRequest

```csharp
public sealed class ExportRequest
{
    public Guid FlowId { get; init; }
    public Guid? SnapshotId { get; init; }
    public ExportType ExportType { get; init; }
    public bool IncludeComments { get; init; }
    public bool IncludeAiNotes { get; init; }
    public bool IncludeMetadata { get; init; }
    public string? LayoutMode { get; init; }
    public string? PageSize { get; init; }
}
```

## 8. ExportResult

```csharp
public sealed class ExportResult
{
    public ExportType ExportType { get; init; }
    public string FileName { get; init; }
    public string ContentType { get; init; }
    public byte[]? BinaryContent { get; init; }
    public string? TextContent { get; init; }
}
```

## 9. Mermaid Export

Mermaid ExportはFlow構造からMermaid文字列を生成する。

方針:

- NodeIdをMermaid安全IDへ変換する。
- Node Titleを表示名に使う。
- Link Label / ConditionをEdge Labelへ反映する。
- Lane / Stageは必要に応じてsubgraph化する。
- Loopを許可する。

## 10. PDF Export

PDF Exportは人間向け正式成果物を生成する。

構成:

- 表紙
- Flow概要
- 図面ページ
- Node一覧
- Link一覧
- Comment一覧
- AI Notes任意出力

方針:

- Canvas DOMを直接PDF化しない。
- ExportInputModelからPDF Document Modelを生成する。
- PDF RendererはInfrastructure層に配置する。

## 11. JSON Export

JSON ExportはSSOT共有用の構造化出力である。

方針:

- SchemaVersionを含める。
- Project / Flow / Lane / Stage / Node / Link / Commentを含める。
- IncludeMetadataで監査情報を含めるか制御する。
- Import候補として利用できる形式にする。

## 12. AI DSL Export

AI DSLはAIが読みやすい独自仕様である。

方針:

- JSONとは別形式とする。
- Lane責務、Stage目的、Node処理、Link条件を明確に出す。
- AI専用メモはIncludeAiNotesで制御する。
- 通信、DB更新、PLC、API候補を抽出しやすい形式にする。

## 13. Snapshot指定時の処理

SnapshotIdが指定された場合、現在FlowではなくSnapshotを入力とする。

処理:

1. Snapshot取得。
2. SnapshotJsonをExportInputModelへ変換。
3. Renderer呼び出し。

現在Flowの未保存変更は含めない。

## 14. 例外処理

Export失敗時はExportExceptionを送出する。

errorCode例:

- EXPORT_TARGET_NOT_FOUND
- EXPORT_UNSUPPORTED_TYPE
- EXPORT_MERMAID_FAILED
- EXPORT_PDF_FAILED
- EXPORT_JSON_FAILED
- EXPORT_AI_DSL_FAILED

## 15. 非同期Job化方針

初期は同期実行とする。

将来、PDFや大量Exportが重い場合は以下へ移行する。

```text
ExportRequest
 -> ExportJob作成
 -> Queue
 -> Worker
 -> Storage保存
 -> DownloadUrl返却
```

Service Interfaceは非同期化を妨げない形にする。

## 16. テスト観点

- 保存済みFlowからMermaidを生成できる。
- SnapshotからPDFを生成できる。
- JSONにSchemaVersionが含まれる。
- AI DSLにLane責務とLink条件が含まれる。
- 未保存Frontend状態がExportへ混入しない。
- 未対応ExportTypeで例外になる。

## 17. 完了条件

- ExportServiceとRendererの責務が分離されている。
- ExportInputModelが定義されている。
- Mermaid / PDF / JSON / AI DSLの方針が定義されている。
- Snapshot入力と現在Flow入力の扱いが明確である。
- AIが本書を読んでExportServiceを実装できる。

# 12_14_Service詳細設計

## 1. 本書の目的

本書は、AI Flow Designer のService層の詳細設計を定義する。

Service層は、Controllerから呼び出され、業務ルール、Transaction、Repository呼び出し、DTO変換、例外送出を制御する中核層である。

## 2. 基本方針

- Controllerを薄くする
- 業務ロジックはServiceへ集約する
- Repositoryを直接Controllerから呼ばない
- ServiceでTransaction境界を管理する
- Serviceで認可確認を行う
- ServiceでRevision競合を確認する
- Serviceで編集ロックを確認する
- ServiceはDTOまたはApplication Modelを返す
- EntityをAPIレスポンスとして返さない

## 3. Service分類

本システムでは以下のServiceを定義する。

```text
AuthService
ProjectService
FlowService
FlowSaveService
FlowQueryService
EditorLockService
TemplateService
FlowVersionService
ExportService
ImageFileService
SettingService
ProjectAuthorizationService
```

## 4. ProjectService

責務:

- Project作成
- Project更新
- Project削除
- Project一覧取得
- メンバー管理
- Project権限と連携

主なメソッド:

```csharp
Task<ProjectDto> CreateAsync(CreateProjectRequest request, CurrentUser user, CancellationToken ct);
Task<ProjectDto> UpdateAsync(Guid projectId, UpdateProjectRequest request, CurrentUser user, CancellationToken ct);
Task DeleteAsync(Guid projectId, CurrentUser user, CancellationToken ct);
Task<PagedResult<ProjectListDto>> SearchAsync(ProjectSearchRequest request, CurrentUser user, CancellationToken ct);
```

## 5. FlowService

責務:

- Flow作成
- Flow更新
- Flow削除
- Flow一覧取得
- Flow基本情報管理

Flow構造全体の保存はFlowSaveServiceへ委譲する。

## 6. FlowQueryService

責務:

- Flow詳細取得
- Export用Flow構造取得
- Version用Snapshot元データ取得
- 参照専用ReadModel作成

FlowQueryServiceは原則AsNoTrackingで取得する。

## 7. FlowSaveService

最重要Service。

責務:

- Flow一括保存
- 自動保存
- Lane/Stage/Node/Link/Comment差分反映
- Revision確認
- 編集ロック確認
- 構造整合性検証
- Transaction管理
- Snapshot作成連携

## 8. FlowSaveService 処理

1. ユーザー権限確認
2. Flow存在確認
3. 編集ロック確認
4. Revision確認
5. DTO Validation
6. 構造整合性Validation
7. Transaction開始
8. 差分分類
9. Entity反映
10. Revision更新
11. SaveChanges
12. Commit
13. 結果DTO返却

## 9. 差分分類

保存DTOの各要素は以下に分類する。

- Added
- Updated
- Deleted
- Unchanged

削除は物理削除ではなく論理削除。

## 10. Lane削除Service処理

Lane削除時はユーザー選択に従う。

- MoveNodes
- DeleteWithNodes

MoveNodes:

1. 移動先Lane確認
2. 対象NodeのLaneId更新
3. Lane論理削除

DeleteWithNodes:

1. Lane配下Node論理削除
2. 関連Link論理削除
3. Node紐付けComment処理
4. Lane論理削除

## 11. Stage削除Service処理

Stage削除もLane削除と同様。

- MoveNodes
- DeleteWithNodes

Stageの場合は移動先StageへNodeを移動する。

## 12. EditorLockService

責務:

- Flow単位ロック取得
- ロック延長
- ロック解除
- 管理者解除
- タイムアウト判定

ロックはFlow保存前に必ず確認する。

## 13. FlowVersionService

責務:

- Version作成
- Version一覧
- Version比較
- Version復元
- Snapshot生成
- Snapshot読込

Version復元時は、現在状態を復元前Versionとして保存してから復元する。

## 14. TemplateService

責務:

- 標準テンプレート一覧
- ユーザーテンプレート一覧
- Template作成
- Template適用
- Template削除
- 過去Project複写

TemplateにはLane/Stage/Node/Link/Comment/Layoutを含める。

## 15. ExportService

Exportは種類ごとにServiceを分ける。

```text
MermaidFlowchartExportService
MermaidSequenceExportService
PdfExportService
JsonExportService
AiDslExportService
```

共通Interface:

```csharp
Task<ExportResultDto> ExportAsync(Guid flowId, ExportRequest request, CurrentUser user, CancellationToken ct);
```

## 16. ImageFileService

責務:

- 画像アップロード
- 画像取得
- 画像削除
- 画像メタデータ管理
- SVG検証
- 画像Node参照確認

物理ファイル処理はFileStorageServiceへ委譲する。

## 17. SettingService

責務:

- 個人設定取得
- 個人設定更新
- Project設定取得
- Project設定更新
- ショートカット設定
- グリッド設定
- スナップ設定
- 出力設定

## 18. ProjectAuthorizationService

責務:

- ProjectRole取得
- RequiredRole判定
- Admin判定
- ProjectAdmin判定
- Editor判定
- Viewer判定
- 権限例外送出

## 19. Serviceの例外方針

Serviceは業務上意味のある例外を送出する。

- NotFoundAppException
- ValidationAppException
- BusinessRuleException
- ConflictAppException
- ForbiddenAppException

Repository由来のDB例外は必要に応じてDatabaseExceptionへ変換する。

## 20. Serviceのログ方針

Serviceは主要業務処理の開始・終了・失敗をログ出力する。

特に以下:

- Flow保存
- Version復元
- Template適用
- Export
- 画像アップロード
- 編集ロック競合

## 21. DTO変換

ServiceはEntityをDTOへ変換して返す。

変換方法:

- 手動Mapper
- static factory
- 専用Mapperクラス

AutoMapperは初期実装では必須としない。

理由:

- 構造化フローの変換が複雑
- 明示的な変換の方がAI実装しやすい
- 変換ルールが追跡しやすい

## 22. Service単体テスト

ServiceはMock Repositoryで単体テストする。

確認例:

- Viewerが保存できない
- Revision不一致でConflict
- Lock競合でConflict
- NodeなしLinkでValidation
- Lane削除MoveNodesが正しく動く
- Stage削除DeleteWithNodesでLinkも削除される
- Template適用時にIDが再採番される

## 23. Service結合テスト

実DBを使うテスト:

- Flow一括保存
- Version作成
- Version復元
- Template適用
- 画像アップロード
- Transaction Rollback

## 24. 禁止事項

- Controllerに業務ロジックを書く
- ControllerからRepositoryを直接呼ぶ
- ServiceからHTTP Contextへ直接依存する
- ServiceでJWTを解析する
- Repositoryで業務例外を作る
- Serviceで物理ファイルパスをAPIへ返す
- Entityをそのまま返す

## 25. 完了条件

- 主要ServiceのInterfaceが定義されている
- FlowSaveServiceが一括保存を担う
- Service層でTransaction境界が明確
- Service層で業務例外を送出する
- Controllerが薄く保たれている
- Unit Testで業務ルールを検証できる

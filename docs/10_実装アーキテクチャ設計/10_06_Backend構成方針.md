# 10_06_Backend構成方針

## 1. 目的

本書は、AI Flow DesignerのBackend構成方針を定義する。

Backendは、SSOTの正当性を守る中心であり、単なるCRUD APIではない。
保存、検証、Version Snapshot、Export、Import、認証、権限、将来のAIレビュー連携を担当する。

## 2. 採用技術

- ASP.NET Core .NET 8
- Entity Framework Core
- SQLite
- 将来SQL Server
- GitHub OAuth

## 3. 推奨プロジェクト構成

```text
backend/
 ├─ FlowDesigner.Api/
 │   ├─ Controllers/
 │   ├─ Filters/
 │   ├─ Middlewares/
 │   └─ Program.cs
 │
 ├─ FlowDesigner.Application/
 │   ├─ Services/
 │   ├─ UseCases/
 │   ├─ DTOs/
 │   └─ Validators/
 │
 ├─ FlowDesigner.Domain/
 │   ├─ Models/
 │   ├─ ValueObjects/
 │   ├─ Enums/
 │   └─ Rules/
 │
 ├─ FlowDesigner.Infrastructure/
 │   ├─ Data/
 │   ├─ Repositories/
 │   ├─ Auth/
 │   ├─ Export/
 │   └─ Storage/
 │
 └─ FlowDesigner.Tests/
     ├─ Unit/
     ├─ Integration/
     └─ Export/
```

## 4. API Layer

API LayerはHTTPの入口である。

責務:

- Request DTO受信
- 認証状態確認
- 入力形式の最低限チェック
- Application Service呼び出し
- Response DTO返却
- HTTPステータス変換

禁止事項:

- Controllerに業務ロジックを書く。
- Controllerから直接DbContextを呼ぶ。
- ControllerでExport変換を実装する。
- ControllerでSSOT構造検証を完結させる。

## 5. Application Layer

Application Layerはユースケースを担当する。

例:

- CreateProjectUseCase
- SaveFlowUseCase
- GetFlowUseCase
- CreateSnapshotUseCase
- ExportMermaidUseCase
- ExportPdfUseCase
- ExportJsonUseCase
- ExportAiDslUseCase
- ImportFlowUseCase
- ApplyTemplateUseCase

Application Layerは、Domain Validator、Repository、Export Serviceを組み合わせる。

## 6. Domain Layer

Domain LayerはSSOTの意味とルールを保持する。

主なModel:

- Project
- Flow
- Lane
- Stage
- Node
- Link
- Comment
- FlowVersionSnapshot
- Template

主なRule:

- ID一意性
- 参照整合性
- Node Type別必須Property
- Lane / Stage参照存在
- Linkのsource / target存在
- Loop許可
- Comment紐付け対象存在

Domain LayerはEF Coreに依存しない。

## 7. Infrastructure Layer

Infrastructure Layerは外部技術依存を閉じ込める。

対象:

- DbContext
- Repository実装
- SQLite接続
- SQL Server接続
- GitHub OAuth連携
- PDF生成ライブラリ
- ファイル保存
- ログ出力

Application LayerからはInterface経由で利用する。

## 8. Repository方針

Repositoryは、Domain Modelの永続化を担当する。

原則:

- Controllerから直接Repositoryを呼ばない。
- RepositoryはDTOを返さず、Domain Modelまたは永続化用Modelを扱う。
- DB固有の最適化はInfrastructure内に閉じる。
- SQLiteからSQL Serverへ移行してもApplication Layerの変更を最小化する。

## 9. Export Service方針

Export ServiceはBackend側に配置する。

理由:

- 保存済みSSOTから安定生成するため。
- Version Snapshotから再出力できるようにするため。
- ブラウザDOM依存を排除するため。
- 将来サーバーサイド一括出力に対応するため。

Export Service例:

- MermaidExportService
- PdfExportService
- JsonExportService
- AiDslExportService

## 10. エラー処理方針

エラーは以下に分類する。

| 区分 | 例 | HTTP |
| --- | --- | --- |
| Validation | Node参照不正 | 400 |
| Auth | 未ログイン | 401 |
| Permission | 権限なし | 403 |
| NotFound | Flowなし | 404 |
| Conflict | Version競合 | 409 |
| System | DB障害 | 500 |

Validation Errorは、FrontendでProperty PanelやStatus Barに表示できる形式で返す。

## 11. 完了条件

- Controllerに業務ロジックが集中しない。
- Domain LayerがEF Coreに依存しない。
- Export処理がBackend側で完結する。
- SQLite / SQL Server差分がInfrastructureに閉じている。
- AIだけが本書を読んでもBackendの実装配置を判断できる。

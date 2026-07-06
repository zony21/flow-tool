# Backend

AI Flow Designer Backend。

## 技術

- ASP.NET Core .NET 8
- Entity Framework Core
- SQLite

## ソリューション構成

```text
apps/backend/
  FlowDesigner.sln
  FlowDesigner.Api/
  FlowDesigner.Application/
  FlowDesigner.Domain/
  FlowDesigner.Infrastructure/
  FlowDesigner.Tests/
```

## セットアップ

```bash
cd apps/backend
dotnet restore FlowDesigner.sln
dotnet build FlowDesigner.sln
dotnet run --project FlowDesigner.Api/FlowDesigner.Api.csproj
```

## API確認

- GET /api/health

## Migration例

```bash
cd apps/backend
dotnet ef migrations add InitialCreate --project FlowDesigner.Infrastructure/FlowDesigner.Infrastructure.csproj --startup-project FlowDesigner.Api/FlowDesigner.Api.csproj --output-dir Persistence/Migrations
dotnet ef database update --project FlowDesigner.Infrastructure/FlowDesigner.Infrastructure.csproj --startup-project FlowDesigner.Api/FlowDesigner.Api.csproj
```

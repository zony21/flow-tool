# 05_09_OpenAPI設計

## 1. 目的

OpenAPI定義の作成方針を定義する。
API仕様を人間とAIの両方が理解できる形式で管理し、Frontend実装、Backend実装、テストに利用する。

## 2. 基本方針

ASP.NET CoreのSwagger/OpenAPI出力を利用する。
Controller、Request DTO、Response DTO、ErrorResponseに説明を付与する。

## 3. 記載対象

- API概要
- HTTP Method
- Path
- Request Body
- Response Body
- Status Code
- ErrorResponse
- 認証要否
- 権限要否

## 4. Tag分類

OpenAPIのTagは以下とする。

- Auth
- Projects
- Flows
- FlowVersions
- Templates
- Exports
- Users
- Notifications

## 5. DTO説明

DTOの各プロパティには用途、必須、最大文字数、制約を記載する。

## 6. ErrorResponse

全APIで共通ErrorResponseを参照する。
400、401、403、404、409、500を明記する。

## 7. 活用方法

- Frontend API client生成
- Backend実装確認
- テストケース作成
- AI実装支援

## 8. テスト観点

- Swagger UIでAPI一覧が確認できること
- DTOスキーマが表示されること
- ErrorResponseが共通定義になっていること
- 認証が必要なAPIが区別できること

## 9. 完了条件

OpenAPI定義からFrontend、Backend、テストの作業へ展開できること。

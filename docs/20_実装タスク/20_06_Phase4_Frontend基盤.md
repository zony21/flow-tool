# 20_06_Phase4_Frontend基盤

## 1. 目的

Frontendの基盤画面、Router、Store、API Client、Layoutを実装する。

## 2. Task一覧

| Task ID | Task Name | 優先度 |
| --- | --- | --- |
| P4-001 | Frontendディレクトリ構成作成 | A |
| P4-002 | Router実装 | A |
| P4-003 | API Client実装 | A |
| P4-004 | Layout実装 | A |
| P4-005 | Project Store実装 | A |
| P4-006 | Flow Store実装 | A |
| P4-007 | 共通Error表示実装 | B |

## 3. P4-001 Frontendディレクトリ構成作成

目的:

Vue実装の標準構成を作成する。

実装内容:

- components
- pages
- stores
- api
- types
- constants
- router
- utils

関連設計:

- 11_フロントエンド設計

テスト観点:

- npm run build成功

完了条件:

- 実装用ディレクトリが整理されている

## 4. P4-002 Router実装

目的:

主要画面のRouteを作成する。

実装内容:

- Login
- ProjectList
- ProjectDetail
- FlowEditor
- Settings
- Forbidden

関連設計:

- 03_画面設計
- 17_画面制御設計

テスト観点:

- Route Guardが動作する

完了条件:

- 主要画面へ遷移できる

## 5. P4-003 API Client実装

目的:

Backend API呼び出しを共通化する。

実装内容:

- axiosまたはfetch wrapper
- 401/403/400共通処理
- Project API Client
- Flow API Client
- Setting API Client

関連設計:

- 05_API設計
- 18_API設計

テスト観点:

- 403時にPermission再取得処理へ接続できる

完了条件:

- API呼び出しが共通Client経由になる

## 6. P4-004 Layout実装

目的:

Editor全体のLayoutを実装する。

実装内容:

- Header
- Toolbar
- Left Panel
- Center Canvas Area
- Right Property Panel
- Bottom Status

関連設計:

- 03_画面設計
- 06_フローエディタ設計

テスト観点:

- 画面領域が表示される

完了条件:

- Editor基本Layoutが表示される

## 7. P4-005 Project Store実装

目的:

Project一覧・詳細をFrontendで管理する。

実装内容:

- fetchProjects
- fetchProject
- createProject
- updateProject
- deleteProject

関連設計:

- 11_フロントエンド設計

テスト観点:

- Store更新が期待通り

完了条件:

- Project画面からStoreを利用できる

## 8. P4-006 Flow Store実装

目的:

Flow構造化データをFrontendで管理する。

実装内容:

- currentFlow
- nodes
- links
- lanes
- stages
- comments
- fetchFlow
- saveFlow

関連設計:

- 06_フローエディタ設計
- 09_AI構造化データ設計

テスト観点:

- Node/Link追加時にStoreが更新される

完了条件:

- EditorがFlow Storeを参照できる

## 9. P4-007 共通Error表示実装

目的:

API Errorを画面で分かりやすく表示する。

実装内容:

- Toast
- Dialog
- Validation Error表示
- 401/403/404/500表示

関連設計:

- 03_画面設計
- 19_テスト仕様

テスト観点:

- 403時に権限不足メッセージが表示される

完了条件:

- 主要Errorが画面表示される

# AI Flow Designer

## 1. 概要

AI Flow Designerは、構造化データを中心にフロー設計を行うWebアプリである。
人が見やすく、AIが正確に解析でき、設計書として利用できる成果物を生成する。

## 2. 設計思想

Single Source of Truth（SSOT）を採用する。
図そのものではなく、Project / Flow / Lane / Stage / Node / Link / Comment / Image / Version / Metadata の構造化データを正とする。

図形は結果であり、保存・解析・出力の正は構造化データである。

Node座標（X/Y）は設計意味を持たないView Hintとして扱う。
処理順・責務・AI DSL解析は構造化データから判断する。

## 3. 技術スタック

| 区分 | 技術 |
| --- | --- |
| Frontend | Vue3 / TypeScript / PrimeVue / Vue Flow / Pinia / Vite |
| Backend | ASP.NET Core .NET 8 / Entity Framework Core |
| Database | SQLite、将来SQL Server |
| Auth | GitHub OAuth |

## 4. docs正式索引

| フォルダ | 内容 | 状態 |
| --- | --- | --- |
| `00_全体方針` | 全体方針 | 詳細化済み |
| `01_全体構想` | 全体構想 | 詳細化済み |
| `02_要件定義` | 要件定義 | 詳細化済み |
| `03_画面設計` | 画面設計 | 詳細化済み |
| `04_DB設計` | DB設計 | 詳細化済み |
| `05_API設計` | API設計 | 詳細化済み |
| `06_フローエディタ設計` | フローエディタ（Node座標View Hint仕様含む） | 詳細化済み |
| `07_Mermaid出力設計` | Mermaid出力 | 詳細化済み |
| `08_PDF出力設計` | PDF出力 | 詳細化済み |
| `09_AI構造化データ設計` | AI構造化データ | 詳細化済み |
| `10_実装アーキテクチャ設計` | 実装アーキテクチャ | 詳細化済み |
| `11_フロントエンド設計` | Frontend設計 | 詳細化済み |
| `12_バックエンド設計` | Backend設計 | 詳細化済み |
| `13_描画エンジン設計` | 描画エンジン | 詳細化済み |
| `14_UndoRedo設計` | Undo/Redo | 詳細化済み |
| `15_テンプレート設計` | Template | 詳細化済み |
| `16_画像管理設計` | 画像管理 | 主要設計詳細化済み |
| `17_権限管理設計` | 権限管理 | 再詳細化済み |
| `18_設定設計` | 設定 | 詳細化済み |
| `19_テスト仕様` | テスト仕様 | 詳細化済み |
| `20_実装タスク` | 実装タスク | 詳細化済み |
| `21_Import設計` | Import | 追加詳細化済み |
| `22_AIレビュー設計` | AI Review | 追加詳細化済み |
| `23_Performance設計` | Performance | 追加詳細化済み |

## 5. 運用ルール

- 開発対象ブランチは、Project Ownerの明示的な指示によって決定する。
- docs配下には正式設計書のみ残す。
- READMEは正式索引として管理する。
- feature / chatgpt / cleanup 等の過去ブランチは正としない。

## 6. 現在の詳細化状況

全体監査で見つかった不足候補を追加補強済み。

追加補強内容:

- AI DSL Schema仕様
- Node拡張仕様
- Node座標View Hint仕様
- Import設計
- AI Review設計
- ErrorResponse共通仕様
- Performance設計

次工程は実装フェーズ。

## 7. 実装構成

- apps/frontend
	- Vue3 + TypeScript + Vite
	- Pinia / Vue Router / PrimeVue / Vue Flow
- apps/backend
	- ASP.NET Core .NET 8
	- Api / Application / Domain / Infrastructure / Tests
	- SQLite接続

## 8. 起動手順

Frontend:

```bash
cd apps/frontend
npm install
npm run dev
```

Backend:

```bash
cd apps/backend
dotnet restore FlowDesigner.sln
dotnet run --project FlowDesigner.Api/FlowDesigner.Api.csproj
```

## 9. 補足

- PowerShell環境でnpmが実行ポリシーにより失敗する場合は `npm.cmd` を利用する。
- SQLite接続文字列は `apps/backend/FlowDesigner.Api/appsettings*.json` で管理する。

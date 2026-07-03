# 12_バックエンド設計

## 1. 本書の位置付け

本フォルダは、AI Flow DesignerのBackend実装設計を管理する。

Backendは、SSOTであるProject / Flow / Lane / Stage / Node / Link / Commentを永続化し、検証し、Version Snapshot、Export、認証・認可、画像管理、Template適用を支える中核である。

## 2. 採用技術

- ASP.NET Core .NET 8
- Entity Framework Core
- SQLite
- 将来SQL Server
- GitHub OAuth
- JWT

## 3. 設計思想

- Controllerは薄くする。
- Serviceが業務処理とTransaction境界を管理する。
- Repositoryは永続化処理のみ担当する。
- DomainはSSOTの意味と整合性を表す。
- EntityをAPIへ直接返さない。
- SQLite固有処理をApplication層へ漏らさない。
- Exportは保存済みFlowまたはSnapshotから生成する。

## 4. 正式設計書一覧

| ファイル | 内容 | 状態 |
| --- | --- | --- |
| `12_01_バックエンド概要.md` | Backend全体方針 | 詳細化済み |
| `12_02_プロジェクト構成設計.md` | Solution / Project構成 | 詳細化済み |
| `12_03_レイヤ責務設計.md` | Api / Application / Domain / Infrastructure責務 | 詳細化済み |
| `12_04_DomainEntity設計.md` | Domain Entity / SSOT構造 | 詳細化済み |
| `12_05_依存性注入(DI)設計.md` | DI方針 | 詳細化済み |
| `12_06_トランザクション設計.md` | Transaction方針 | 詳細化済み |
| `12_07_例外処理設計.md` | 例外処理 | 詳細化済み |
| `12_08_ログ設計.md` | ログ設計 | 詳細化済み |
| `12_09_認証・認可設計.md` | GitHub OAuth / JWT / 権限 | 詳細化済み |
| `12_10_ファイル・画像管理設計.md` | ファイル・画像管理 | 詳細化済み |
| `12_11_バックエンド処理シーケンス.md` | 処理シーケンス | 詳細化済み |
| `12_12_性能・キャッシュ設計.md` | 性能・キャッシュ | 詳細化済み |
| `12_13_Repository詳細設計.md` | Repository詳細 | 詳細化済み |
| `12_14_Service詳細設計.md` | Service詳細 | 詳細化済み |
| `12_15_Controller詳細設計.md` | Controller詳細 | 詳細化済み |

## 5. 今後追加・詳細化する設計書

- `12_16_ExportService設計.md`
- `12_17_VersionSnapshot設計.md`
- `12_18_Validation設計.md`
- `12_19_バックエンドテスト設計.md`
- `12_20_バックエンド設計まとめ.md`

## 6. 管理方針

`12_バックエンド設計` 配下には正式設計書のみを残す。

作業メモ、draft、temp、old、更新概要、数行だけの旧設計書は正式設計書へ統合後に削除する。

## 7. 現在の状態

12章は、旧重複ファイルを削除し、`12_01`〜`12_15` を実装レベルで詳細化済み。

次工程では `12_16`〜`12_20` を追加し、12章全体を完了させる。

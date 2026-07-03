# 04_19_Migration運用

## 1. 本書の目的

本書は、EF Core Migration の運用方針を定義する。
SQLite初期実装からSQL Server移行まで、DB変更履歴を安全に管理することを目的とする。

## 2. 基本方針

DB変更はEntity変更とMigrationを一致させる。
手作業SQLだけでDBを変更しない。

## 3. 採用理由

複数環境でDB構造を再現し、Visual Studio + EF Coreで実装者が迷わず更新できるようにするため。

## 4. 利用機能

DB構築、開発環境更新、テスト環境更新、SQL Server移行。

## 5. 関連画面

直接関連画面はないが、全画面のDB構造に影響する。

## 6. 関連API

全API。

## 7. 命名規則

Migration名は英語で目的を表す。

例:

- InitialCreate
- AddFlowVersionTables
- AddTemplateTables
- AddEditorLock
- AddExportHistory

## 8. Version管理

MigrationファイルはGit管理する。
DB状態とコード状態を一致させる。

## 9. Rollback

Rollbackが必要なMigrationでは、Downメソッドで復元可能な範囲を明確にする。
データ破壊を伴う場合は事前退避SQLを用意する。

## 10. SQLite注意点

SQLiteではALTER TABLE制約が弱いため、カラム削除や制約変更は慎重に扱う。

## 11. SQL Server注意点

SQL Server移行時は型、制約、Index、日時精度の差異を確認する。

## 12. 更新タイミング

Entity追加、カラム追加、制約変更、Index追加、テーブル追加時にMigrationを作成する。

## 13. テスト観点

- 新規DBにMigrationを適用できること
- 既存DBを更新できること
- Rollbackできること
- SQLiteとSQL Serverの差異が吸収されていること

## 14. 完了条件

Migration命名、適用、Rollback、移行方針が明確であること。

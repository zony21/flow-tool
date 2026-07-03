# 04_23_SQLServer移行設計

## 1. 本書の目的

本書は、初期SQLiteから将来SQL Serverへ移行する際の設計方針を定義する。

## 2. 基本方針

EF Coreを使用し、EntityとMigrationを通じてSQLiteとSQL Serverの差異を吸収する。
SQLite固有SQLへ依存しすぎない。

## 3. SQLiteとの差異

| 観点 | SQLite | SQL Server | 方針 |
| --- | --- | --- | --- |
| GUID | TEXT | uniqueidentifier | EntityでGuid利用を検討 |
| Boolean | INTEGER | bit | EF Coreでbool変換 |
| DateTime | TEXT | datetime2 | UTCまたはJST方針を統一 |
| FK | 設定依存 | 強い制約 | Migrationで明示 |
| CHECK | 制限あり | 利用可 | SQL Server移行時に強化 |
| Index | 基本Index | INCLUDE/Filtered可 | 移行後に最適化 |

## 4. Identity

主キーはGUID方針のためIdentity依存を避ける。
将来SQL Serverでも一意性と分散作成に強い設計にする。

## 5. Datetime

日時はアプリ層で統一する。
SQL Serverではdatetime2を使用する方針とする。

## 6. FK制約

SQLite初期でもFKを意識した設計にする。
SQL Server移行時はRestrict/Cascadeを明示する。

## 7. Index

移行後は実行計画を確認し、必要に応じてINCLUDE列やFiltered Indexを追加する。

## 8. 移行手順

1. EntityのDBプロバイダ切替
2. SQL Server用Migration確認
3. 既存SQLiteデータExport
4. SQL ServerへImport
5. FK整合性確認
6. 画面・API動作確認

## 9. テスト観点

- SQL ServerでMigration適用できること
- 主要テーブルのデータ移行ができること
- GUID、日時、boolが正しく変換されること
- FK制約エラーが発生しないこと

## 10. 完了条件

SQLite初期実装からSQL Serverへ移行する際の差異、手順、検証観点が明確であること。

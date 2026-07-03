# 04_03_PROJECT詳細設計

## 1. 本書の目的

本書は、PROJECTテーブルの詳細設計を定義する。
PROJECTはAI Flow Designerにおける最上位の管理単位であり、複数のFlowを束ねる。

## 2. テーブル概要

PROJECTは、包装ライン制御、物流センター、WCS開発などの設計対象プロジェクトを保持する。

## 3. 採用理由

Flowを単独管理すると、関連する設計フロー、テンプレート、メンバー、出力履歴を体系化できない。
PROJECTを最上位に置くことで、設計対象ごとに情報を整理できる。

## 4. 利用機能

- プロジェクト一覧
- プロジェクト詳細
- フロー一覧
- 権限制御
- 出力管理

## 5. 関連画面

- 03_03_プロジェクト一覧画面
- 03_06_プロジェクト詳細画面
- 03_04_フロー一覧画面

## 6. 関連API

- GET /api/projects
- POST /api/projects
- GET /api/projects/{projectId}
- PUT /api/projects/{projectId}
- DELETE /api/projects/{projectId}

## 7. ER上の位置

PROJECTはFLOWの親であり、PROJECT_MEMBERを介してUSERと関連する。

## 8. テーブル定義

| 論理名 | 物理名 |
| --- | --- |
| プロジェクト | PROJECT |

## 9. カラム一覧

| カラム | 型 | NULL | 説明 |
| --- | --- | --- | --- |
| project_id | TEXT | NO | PK、GUID |
| project_name | TEXT | NO | プロジェクト名 |
| description | TEXT | YES | 説明 |
| display_order | INTEGER | NO | 表示順 |
| is_deleted | INTEGER | NO | 論理削除 |
| created_at | TEXT | NO | 作成日時 |
| created_by | TEXT | NO | 作成者 |
| updated_at | TEXT | NO | 更新日時 |
| updated_by | TEXT | NO | 更新者 |
| deleted_at | TEXT | YES | 削除日時 |
| deleted_by | TEXT | YES | 削除者 |

## 10. PK

project_idをPKとする。

## 11. FK

created_by、updated_by、deleted_byはUSER.user_idを参照する方針とする。
初期実装ではアプリ側整合でもよい。

## 12. Unique

同一ユーザーが同一名称のプロジェクトを作成できるかは運用で決める。
初期実装ではUniqueなし、将来(project_name, is_deleted)の制約を検討する。

## 13. Index

- idx_project_deleted_order: is_deleted, display_order
- idx_project_updated_at: updated_at

## 14. Default値

- display_order: 0
- is_deleted: 0

## 15. NULL可否

project_name、監査カラムはNOT NULLとする。
description、deleted_at、deleted_byはNULL可とする。

## 16. CHECK制約

is_deletedは0または1とする。
display_orderは0以上とする。

## 17. 論理削除

削除時はis_deleted=1、deleted_at、deleted_byを更新する。
配下FLOWは自動削除しない。

## 18. 監査カラム

共通監査カラムを保持する。

## 19. 更新タイミング

作成、名称変更、説明変更、表示順変更、削除時に更新される。

## 20. 削除ルール

配下FLOWが存在する場合は削除確認を行い、初期実装ではPROJECTのみ論理削除する。

## 21. Versionとの関係

PROJECT自体はVersionを持たない。
VersionはFLOW配下で管理する。

## 22. Templateとの関係

PROJECT配下にTemplateからFlowを作成できる。
Template自体はPROJECT共通またはユーザー別に管理する。

## 23. サンプルデータ

```sql
insert into PROJECT(project_id, project_name, description, display_order, is_deleted, created_at, created_by, updated_at, updated_by)
values('p-001', '包装ライン制御', '包装ラインのPLC/WCS/RCS連携設計', 1, 0, '2026-07-03T00:00:00', 'u-001', '2026-07-03T00:00:00', 'u-001');
```

## 24. SQL例

```sql
select * from PROJECT where is_deleted = 0 order by display_order, updated_at desc;
```

## 25. パフォーマンス

一覧ではis_deletedとdisplay_orderを使用するため複合Indexを利用する。

## 26. 将来拡張

プロジェクト単位権限、タグ、カテゴリ、アーカイブ状態を追加できる。

## 27. テスト観点

- PROJECTを作成できること
- 論理削除できること
- 削除済みが一覧に出ないこと
- Flowとの親子関係が維持されること

## 28. 完了条件

PROJECTの作成、更新、削除、一覧表示に必要なカラム、制約、Index、API対応が明確になった場合に完了とする。

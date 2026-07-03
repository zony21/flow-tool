# 04_13_PROJECT_MEMBER詳細設計

## 1. 本書の目的

PROJECT_MEMBERテーブルは、PROJECTとUSERの参加・権限関係を管理する。

## 2. テーブル概要

プロジェクト単位で、利用者が参照・編集・管理できるかを保持する。

## 3. 採用理由

社内複数人利用を前提とし、将来的な権限管理を実現するため。

## 4. 利用機能

プロジェクト一覧、権限制御、共同編集、監査。

## 5. 関連画面

プロジェクト一覧、プロジェクト詳細、権限制御画面、フローエディタ。

## 6. 関連API

GET /api/projects/{projectId}/members、PUT /api/projects/{projectId}/members、GET /api/projects/{projectId}/permissions。

## 7. ER上の位置

PROJECT N:M USERをPROJECT_MEMBERで表現する。

## 8. テーブル定義

| 論理名 | 物理名 |
| --- | --- |
| プロジェクトメンバー | PROJECT_MEMBER |

## 9. カラム一覧

| カラム | 型 | NULL | 説明 |
| --- | --- | --- | --- |
| project_member_id | TEXT | NO | PK |
| project_id | TEXT | NO | PROJECT FK |
| user_id | TEXT | NO | USER FK |
| role | TEXT | NO | Owner/Editor/Viewer |
| can_export | INTEGER | NO | 出力可否 |
| is_deleted | INTEGER | NO | 論理削除 |
| created_at | TEXT | NO | 作成日時 |
| created_by | TEXT | NO | 作成者 |
| updated_at | TEXT | NO | 更新日時 |
| updated_by | TEXT | NO | 更新者 |

## 10. PK
project_member_idをPKとする。

## 11. FK
project_idはPROJECT、user_idはUSERを参照する。

## 12. Unique
project_id + user_idをUniqueとする。

## 13. Index
idx_project_member_project、idx_project_member_user。

## 14. Default値
role='Viewer'、can_export=0、is_deleted=0。

## 15. NULL可否
project_id、user_id、roleはNOT NULL。

## 16. CHECK制約
roleはOwner/Editor/Viewer、can_exportとis_deletedは0/1。

## 17. 論理削除
メンバー解除はis_deleted=1とする。

## 18. 監査カラム
共通監査カラムを保持する。

## 19. 更新タイミング
招待、権限変更、メンバー解除時。

## 20. 削除ルール
監査目的で物理削除しない。

## 21. Versionとの関係
Version作成権限判定に利用する。

## 22. Templateとの関係
Template作成・適用権限判定に利用可能。

## 23. サンプルデータ
```sql
insert into PROJECT_MEMBER(project_member_id, project_id, user_id, role, can_export, is_deleted, created_at, created_by, updated_at, updated_by)
values('pm-001','p-001','u-001','Owner',1,0,'2026-07-03T00:00:00','u-001','2026-07-03T00:00:00','u-001');
```

## 24. SQL例
```sql
select * from PROJECT_MEMBER where project_id='p-001' and user_id='u-001' and is_deleted=0;
```

## 25. パフォーマンス
ログイン後のプロジェクト一覧でuser_id検索を行うためIndexを使用する。

## 26. 将来拡張
Flow単位権限、チーム権限、承認者権限を追加可能。

## 27. テスト観点
権限取得、Viewer制御、Editor保存可、Owner管理可を確認する。

## 28. 完了条件
Project単位の利用者権限がDB上で判定できること。

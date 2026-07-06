# 04_10_VERSION詳細設計

## 1. 本書の目的

本書は、FLOW_VERSIONテーブルの詳細設計を定義する。VersionはFlowの構造データSnapshotを保持する単位であり、Undo/Redoとは責務を分離する。

## 2. テーブル概要

FLOW_VERSIONは、ある時点のLane、Stage、Node、Link、Commentを束ねる。最新版だけを上書きせず、必要に応じて版を作成する。

## 3. 採用理由

設計変更の履歴を保持し、AIや実装者が参照しているフローの版を明確にするため。

## 4. 利用機能

バージョン管理、フローエディタ、出力、テンプレート適用、設計履歴管理。

## 5. 関連画面

フローエディタ画面、バージョン管理画面、出力画面。

## 6. 関連API

GET /api/projects/{projectId}/flows/{flowId}/versions、POST /api/projects/{projectId}/flows/{flowId}/versions、GET /api/flow-versions/{id}。

## 7. ER上の位置

FLOW 1:N FLOW_VERSION、FLOW_VERSION 1:N LANE/STAGE/NODE/LINK/COMMENT。

## 8. テーブル定義

| 論理名 | 物理名 |
| --- | --- |
| フローバージョン | FLOW_VERSION |

## 9. カラム一覧

| カラム | 型 | NULL | 説明 |
| --- | --- | --- | --- |
| flow_version_id | TEXT | NO | PK |
| flow_id | TEXT | NO | FLOW FK |
| version_no | TEXT | NO | 1.0等 |
| is_latest | INTEGER | NO | 最新版フラグ |
| change_summary | TEXT | YES | 変更概要 |
| change_reason | TEXT | YES | 変更理由 |
| is_deleted | INTEGER | NO | 論理削除 |
| created_at | TEXT | NO | 作成日時 |
| created_by | TEXT | NO | 作成者 |
| updated_at | TEXT | NO | 更新日時 |
| updated_by | TEXT | NO | 更新者 |

## 10. PK
flow_version_idをPKとする。

## 11. FK
flow_idはFLOWを参照する。

## 12. Unique
flow_id + version_noをUniqueとする。

## 13. Index
idx_flow_version_latest: flow_id, is_latest, is_deleted。

## 14. Default値
is_latest=0、is_deleted=0。

## 15. NULL可否
flow_id、version_no、is_latestはNOT NULL。

## 16. CHECK制約
is_latest、is_deletedは0/1。

## 17. 論理削除
Versionは履歴性が重要なため論理削除を基本とする。

## 18. 監査カラム
共通監査カラムを保持する。

## 19. 更新タイミング
Version作成、概要変更、最新版切替、削除時。

## 20. 削除ルール
最新版削除時は別Versionを最新版にするか削除不可とする。

## 21. Versionとの関係
本テーブル自体がVersion管理の中心である。Undo/Redoとは別管理とする。

## 22. Templateとの関係
TemplateからFlow作成時に初期FLOW_VERSIONを生成する。

## 23. サンプルデータ
```sql
insert into FLOW_VERSION(flow_version_id, flow_id, version_no, is_latest, change_summary, is_deleted, created_at, created_by, updated_at, updated_by)
values('fv-001','f-001','1.0',1,'初版',0,'2026-07-03T00:00:00','u-001','2026-07-03T00:00:00','u-001');
```

## 24. SQL例
```sql
select * from FLOW_VERSION where flow_id='f-001' and is_latest=1 and is_deleted=0;
```

## 25. パフォーマンス
最新版取得が頻繁なため、flow_id + is_latest + is_deleted Indexを使用する。

## 26. 将来拡張
承認状態、レビュー状態、差分情報、タグを追加可能。

## 27. テスト観点
Version作成、最新版切替、Snapshot複製、重複version_no制御を確認する。

## 28. 完了条件
VersionがSnapshotとして保存され、Flow構造と履歴を正しく分離できること。

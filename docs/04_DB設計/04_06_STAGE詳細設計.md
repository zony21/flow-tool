# 04_06_STAGE詳細設計

## 1. 本書の目的

STAGEテーブルは、FlowVersion内の工程レーンを保持する。工程はRFID読取り、搬送要求、ラベル印刷、搬送完了など、処理の段階を表す。

## 2. テーブル概要

STAGEはFLOW_VERSIONに属するSnapshot要素であり、Nodeがどの工程に属するかを構造化する。

## 3. 採用理由

AIが処理順序や工程責務を推測せず理解できるよう、工程を明示的なテーブルとして保持する。

## 4. 利用機能

フローエディタ、工程編集、Node配置、PDF出力、AI DSL出力で利用する。

## 5. 関連画面

フローエディタ画面、プロパティパネル、共通ダイアログ。

## 6. 関連API

GET/PUT /api/flow-versions/{flowVersionId}。

## 7. ER上の位置

FLOW_VERSION 1:N STAGE、STAGE 1:N NODE。

## 8. テーブル定義

| 論理名 | 物理名 |
| --- | --- |
| 工程 | STAGE |

## 9. カラム一覧

| カラム | 型 | NULL | 説明 |
| --- | --- | --- | --- |
| stage_id | TEXT | NO | PK |
| flow_version_id | TEXT | NO | FLOW_VERSION FK |
| stage_name | TEXT | NO | 工程名 |
| description | TEXT | YES | 説明 |
| color | TEXT | YES | 表示色 |
| display_order | INTEGER | NO | 表示順 |
| is_deleted | INTEGER | NO | 論理削除 |
| created_at | TEXT | NO | 作成日時 |
| created_by | TEXT | NO | 作成者 |
| updated_at | TEXT | NO | 更新日時 |
| updated_by | TEXT | NO | 更新者 |

## 10. PK

stage_idをPKとする。

## 11. FK

flow_version_idはFLOW_VERSIONを参照する。

## 12. Unique

同一flow_version_id内のstage_name重複は原則禁止を推奨する。

## 13. Index

idx_stage_version_order: flow_version_id, is_deleted, display_order。

## 14. Default値

is_deleted=0、display_order=0。

## 15. NULL可否

stage_name、flow_version_idはNOT NULL。

## 16. CHECK制約

is_deletedは0/1。display_orderは0以上。

## 17. 論理削除

Stage削除時にNodeが存在する場合、削除・移動・キャンセルを選択する。

## 18. 監査カラム

共通監査カラムを保持する。

## 19. 更新タイミング

工程追加、名称変更、順序変更、削除時。

## 20. 削除ルール

Node存在時の自動削除は禁止し、アプリ側で明示選択させる。

## 21. Versionとの関係

StageはFlowVersion Snapshotの一部である。

## 22. Templateとの関係

Template適用時はStage IDを再採番する。

## 23. サンプルデータ

```sql
insert into STAGE(stage_id, flow_version_id, stage_name, display_order, is_deleted, created_at, created_by, updated_at, updated_by)
values('s-001','fv-001','RFID読取り',1,0,'2026-07-03T00:00:00','u-001','2026-07-03T00:00:00','u-001');
```

## 24. SQL例

```sql
select * from STAGE where flow_version_id='fv-001' and is_deleted=0 order by display_order;
```

## 25. パフォーマンス

FlowVersion単位で一括取得するためflow_version_id先頭Indexを使用する。

## 26. 将来拡張

工程カテゴリ、工程グループ、AI推奨工程を追加可能。

## 27. テスト観点

Stage追加、Node紐付き削除制御、Version複製、Template適用を確認する。

## 28. 完了条件

工程情報がNodeと結び付き、AIが工程を誤解しない構造になっていること。

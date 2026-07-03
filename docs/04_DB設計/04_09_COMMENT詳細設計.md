# 04_09_COMMENT詳細設計

## 1. 本書の目的

COMMENTテーブルは、FlowVersion内のコメントを保持する。コメントは独立配置とNode紐付けの両方に対応する。

## 2. テーブル概要

COMMENTはレビュー、注意事項、AI向け補足を保存する。通常コメントとAIメモを区別できる設計とする。

## 3. 採用理由

設計意図や注意点を構造化して保持することで、AIレビューや設計書生成時に補足情報を利用できる。

## 4. 利用機能

フローエディタ、レビュー、PDF出力、JSON出力、AI DSL出力。

## 5. 関連画面

フローエディタ画面、プロパティパネル、出力画面。

## 6. 関連API

GET/PUT /api/flow-versions/{flowVersionId}。

## 7. ER上の位置

FLOW_VERSION 1:N COMMENT、NODE 1:N COMMENT(optional)。

## 8. テーブル定義

| 論理名 | 物理名 |
| --- | --- |
| コメント | COMMENT |

## 9. カラム一覧

| カラム | 型 | NULL | 説明 |
| --- | --- | --- | --- |
| comment_id | TEXT | NO | PK |
| flow_version_id | TEXT | NO | FLOW_VERSION FK |
| node_id | TEXT | YES | 紐付けNODE |
| comment_text | TEXT | NO | コメント本文 |
| is_ai_only | INTEGER | NO | AI専用フラグ |
| x | REAL | YES | 独立コメントX座標 |
| y | REAL | YES | 独立コメントY座標 |
| is_deleted | INTEGER | NO | 論理削除 |
| created_at | TEXT | NO | 作成日時 |
| created_by | TEXT | NO | 作成者 |
| updated_at | TEXT | NO | 更新日時 |
| updated_by | TEXT | NO | 更新者 |

## 10. PK
comment_idをPKとする。

## 11. FK
flow_version_idはFLOW_VERSION、node_idはNODEを参照する。

## 12. Unique
Unique制約なし。同一Nodeに複数コメントを許可する。

## 13. Index
idx_comment_version: flow_version_id, is_deleted。idx_comment_node: node_id。

## 14. Default値
is_ai_only=0、is_deleted=0。

## 15. NULL可否
node_idは独立コメントのためNULL可。comment_textはNOT NULL。

## 16. CHECK制約
is_ai_only、is_deletedは0/1。

## 17. 論理削除
コメント削除時はis_deleted=1。

## 18. 監査カラム
共通監査カラムを保持する。

## 19. 更新タイミング
コメント追加、本文変更、Node紐付け変更、削除時。

## 20. 削除ルール
Node削除時、紐付くコメントを独立コメント化するか削除するかは画面で選択可能とする。

## 21. Versionとの関係
COMMENTはFlowVersion Snapshotの一部である。

## 22. Templateとの関係
Template適用時、node_idがあるコメントは新Node IDへ張り替える。

## 23. サンプルデータ
```sql
insert into COMMENT(comment_id, flow_version_id, node_id, comment_text, is_ai_only, is_deleted, created_at, created_by, updated_at, updated_by)
values('c-001','fv-001','n-001','RFIDリーダ機種依存あり',0,0,'2026-07-03T00:00:00','u-001','2026-07-03T00:00:00','u-001');
```

## 24. SQL例
```sql
select * from COMMENT where flow_version_id='fv-001' and is_deleted=0;
```

## 25. パフォーマンス
FlowVersion単位で一括取得するため、flow_version_id Indexを使用する。

## 26. 将来拡張
レビュー状態、解決済み、メンション、添付ファイル連携を追加可能。

## 27. テスト観点
独立コメント、Nodeコメント、AI専用コメント、Version複製、Template適用時張替えを確認する。

## 28. 完了条件
コメントが通常情報とAI補足情報を表現でき、Node紐付けと独立配置の両方に対応すること。

# 04_14_EDITOR_LOCK詳細設計

## 1. 本書の目的

EDITOR_LOCKテーブルは、FlowVersion編集中の簡易ロック状態を管理する。

## 2. テーブル概要

同一FlowVersionを複数ユーザーが同時編集する場合の競合を抑止する。初期実装では厳密なリアルタイム共同編集ではなく、編集開始中ユーザーの通知と保存競合防止を目的とする。

## 3. 採用理由

設計データは構造整合性が重要であり、同時保存によるNode/Link不整合を避けるため。

## 4. 利用機能

フローエディタ、保存処理、未保存警告、セッション切れ処理。

## 5. 関連画面

フローエディタ画面、エラーメッセージ仕様、画面状態遷移。

## 6. 関連API

POST /api/flow-versions/{flowVersionId}/lock、DELETE /api/flow-versions/{flowVersionId}/lock、GET /api/flow-versions/{flowVersionId}/lock。

## 7. ER上の位置

FLOW_VERSION 1:0..1 EDITOR_LOCK、USER 1:N EDITOR_LOCK。

## 8. テーブル定義

| 論理名 | 物理名 |
| --- | --- |
| 編集ロック | EDITOR_LOCK |

## 9. カラム一覧

| カラム | 型 | NULL | 説明 |
| --- | --- | --- | --- |
| editor_lock_id | TEXT | NO | PK |
| flow_version_id | TEXT | NO | 対象Version |
| user_id | TEXT | NO | ロック取得者 |
| locked_at | TEXT | NO | ロック日時 |
| expires_at | TEXT | NO | 有効期限 |
| client_id | TEXT | YES | クライアント識別 |

## 10. PK
editor_lock_idをPKとする。

## 11. FK
flow_version_idはFLOW_VERSION、user_idはUSERを参照する。

## 12. Unique
flow_version_idにUniqueを設け、同一Versionの同時ロックを制限する。

## 13. Index
idx_editor_lock_expires: expires_at。

## 14. Default値
locked_atは現在日時、expires_atは一定時間後。

## 15. NULL可否
flow_version_id、user_id、locked_at、expires_atはNOT NULL。

## 16. CHECK制約
expires_atはlocked_atより後であることをアプリ側で検証する。

## 17. 論理削除
一時データのため物理削除でよい。

## 18. 監査カラム
簡易ロックのため監査カラムは必須としない。

## 19. 更新タイミング
エディタ入室、ハートビート、エディタ離脱、期限切れ時。

## 20. 削除ルール
期限切れロックは定期処理または取得時に削除する。

## 21. Versionとの関係
FlowVersion単位でロックする。

## 22. Templateとの関係
Templateにはロックを適用しない。

## 23. サンプルデータ
```sql
insert into EDITOR_LOCK(editor_lock_id, flow_version_id, user_id, locked_at, expires_at)
values('el-001','fv-001','u-001','2026-07-03T00:00:00','2026-07-03T00:30:00');
```

## 24. SQL例
```sql
select * from EDITOR_LOCK where flow_version_id='fv-001' and expires_at > '2026-07-03T00:10:00';
```

## 25. パフォーマンス
期限切れ削除のためexpires_at Indexを使用する。

## 26. 将来拡張
リアルタイム共同編集、編集カーソル共有、差分同期に拡張可能。

## 27. テスト観点
ロック取得、重複ロック拒否、期限切れ解除、保存競合制御を確認する。

## 28. 完了条件
FlowVersion編集中の競合を最低限検知・制御できること。

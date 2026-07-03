# 04_17_NOTIFICATION詳細設計

## 1. 本書の目的

NOTIFICATIONテーブルは、ユーザーへの通知情報を保持する。

## 2. テーブル概要

レビュー依頼、編集競合、出力完了、将来AIレビュー結果などを通知として管理する。

## 3. 採用理由

複数人利用時に、設計変更やレビュー依頼を画面上で伝達するため。

## 4. 利用機能

通知表示、レビュー依頼、AIレビュー結果通知、編集競合通知。

## 5. 関連画面

共通UI、Header、通知一覧、フローエディタ。

## 6. 関連API

GET /api/notifications、PUT /api/notifications/{id}/read。

## 7. ER上の位置

USER 1:N NOTIFICATION。

## 8. テーブル定義

| 論理名 | 物理名 |
| --- | --- |
| 通知 | NOTIFICATION |

## 9. カラム一覧

| カラム | 型 | NULL | 説明 |
| --- | --- | --- | --- |
| notification_id | TEXT | NO | PK |
| user_id | TEXT | NO | 通知先USER |
| notification_type | TEXT | NO | 通知種別 |
| title | TEXT | NO | タイトル |
| message | TEXT | YES | 本文 |
| link_url | TEXT | YES | 遷移先 |
| is_read | INTEGER | NO | 既読 |
| created_at | TEXT | NO | 作成日時 |
| read_at | TEXT | YES | 既読日時 |

## 10. PK
notification_idをPKとする。

## 11. FK
user_idはUSERを参照する。

## 12. Unique
Uniqueなし。

## 13. Index
idx_notification_user_read: user_id, is_read, created_at。

## 14. Default値
is_read=0。

## 15. NULL可否
user_id、notification_type、title、created_atはNOT NULL。

## 16. CHECK制約
is_readは0/1。

## 17. 論理削除
通知は一定期間後に物理削除してよい。

## 18. 監査カラム
created_at、read_atを保持する。

## 19. 更新タイミング
通知作成、既読化時。

## 20. 削除ルール
保持期間を過ぎた通知は削除対象とする。

## 21. Versionとの関係
Versionレビュー依頼通知でflow_version_idを将来追加可能。

## 22. Templateとの関係
Template公開通知などで利用可能。

## 23. サンプルデータ
```sql
insert into NOTIFICATION(notification_id, user_id, notification_type, title, is_read, created_at)
values('nt-001','u-001','review','レビュー依頼',0,'2026-07-03T00:00:00');
```

## 24. SQL例
```sql
select * from NOTIFICATION where user_id='u-001' and is_read=0 order by created_at desc;
```

## 25. パフォーマンス
未読件数取得が多いためuser_id、is_readのIndexを使用する。

## 26. 将来拡張
重要度、通知チャンネル、メール連携、Slack連携を追加可能。

## 27. テスト観点
通知作成、未読取得、既読化、保持期間削除を確認する。

## 28. 完了条件
ユーザー単位で通知を管理し、画面表示できること。

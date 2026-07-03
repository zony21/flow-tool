# 04_16_USER_SETTING詳細設計

## 1. 本書の目的

USER_SETTINGテーブルは、ユーザーごとの表示設定、エディタ設定、出力設定を保持する。

## 2. テーブル概要

テーマ、Grid表示、Snap、初期ズーム、既定出力形式などをユーザー単位で保存する。

## 3. 採用理由

利用者ごとの編集体験を維持し、フローエディタの操作性を高めるため。

## 4. 利用機能

ユーザー設定画面、フローエディタ、出力画面。

## 5. 関連画面

ユーザー設定画面、フローエディタ画面。

## 6. 関連API

GET /api/users/me/settings、PUT /api/users/me/settings、POST /api/users/me/settings/reset。

## 7. ER上の位置

USER 1:1 USER_SETTING。

## 8. テーブル定義

| 論理名 | 物理名 |
| --- | --- |
| ユーザー設定 | USER_SETTING |

## 9. カラム一覧

| カラム | 型 | NULL | 説明 |
| --- | --- | --- | --- |
| user_setting_id | TEXT | NO | PK |
| user_id | TEXT | NO | USER FK |
| theme | TEXT | NO | light/dark/system |
| grid_enabled | INTEGER | NO | Grid表示 |
| snap_enabled | INTEGER | NO | Snap有効 |
| default_zoom | INTEGER | NO | 初期ズーム |
| default_export_type | TEXT | NO | 既定出力形式 |
| updated_at | TEXT | NO | 更新日時 |

## 10. PK
user_setting_idをPKとする。

## 11. FK
user_idはUSERを参照する。

## 12. Unique
user_idをUniqueとし、1ユーザー1設定とする。

## 13. Index
idx_user_setting_user: user_id。

## 14. Default値
theme='system'、grid_enabled=1、snap_enabled=1、default_zoom=100、default_export_type='JSON'。

## 15. NULL可否
全主要項目はNOT NULL。

## 16. CHECK制約
default_zoomは25〜200、各フラグは0/1。

## 17. 論理削除
ユーザー設定は物理削除せず、ユーザー無効化に従う。

## 18. 監査カラム
updated_atを保持する。必要に応じてupdated_byを追加する。

## 19. 更新タイミング
設定保存、初期化時。

## 20. 削除ルール
通常削除しない。

## 21. Versionとの関係
Versionデータとは独立する。

## 22. Templateとの関係
Template選択の既定条件へ将来利用可能。

## 23. サンプルデータ
```sql
insert into USER_SETTING(user_setting_id, user_id, theme, grid_enabled, snap_enabled, default_zoom, default_export_type, updated_at)
values('us-001','u-001','system',1,1,100,'JSON','2026-07-03T00:00:00');
```

## 24. SQL例
```sql
select * from USER_SETTING where user_id='u-001';
```

## 25. パフォーマンス
ログイン後にuser_idで1件取得するためUnique Indexを利用する。

## 26. 将来拡張
ショートカット設定、AIレビュー設定、通知設定を追加可能。

## 27. テスト観点
設定取得、保存、初期化、フローエディタ反映を確認する。

## 28. 完了条件
ユーザー別設定を保存し、画面に反映できること。

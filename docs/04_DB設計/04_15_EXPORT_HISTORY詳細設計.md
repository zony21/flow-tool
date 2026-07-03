# 04_15_EXPORT_HISTORY詳細設計

## 1. 本書の目的

EXPORT_HISTORYテーブルは、FlowVersionから生成した成果物の出力履歴を管理する。

## 2. テーブル概要

Mermaid、PDF、JSON、将来AI DSLなどの出力日時、出力者、出力形式、条件を保持する。

## 3. 採用理由

どのVersionからどの成果物を生成したかを追跡し、設計書やAI入力の根拠を明確にするため。

## 4. 利用機能

出力画面、履歴確認、監査、将来再ダウンロード。

## 5. 関連画面

出力画面、バージョン管理画面。

## 6. 関連API

POST /api/flow-versions/{id}/export/{type}、GET /api/flow-versions/{id}/exports。

## 7. ER上の位置

FLOW_VERSION 1:N EXPORT_HISTORY、USER 1:N EXPORT_HISTORY。

## 8. テーブル定義

| 論理名 | 物理名 |
| --- | --- |
| 出力履歴 | EXPORT_HISTORY |

## 9. カラム一覧

| カラム | 型 | NULL | 説明 |
| --- | --- | --- | --- |
| export_history_id | TEXT | NO | PK |
| flow_version_id | TEXT | NO | 対象Version |
| export_type | TEXT | NO | Mermaid/PDF/JSON等 |
| file_name | TEXT | YES | 出力ファイル名 |
| option_json | TEXT | YES | 出力条件JSON |
| exported_at | TEXT | NO | 出力日時 |
| exported_by | TEXT | NO | 出力者 |

## 10. PK
export_history_idをPKとする。

## 11. FK
flow_version_idはFLOW_VERSION、exported_byはUSERを参照する。

## 12. Unique
Uniqueなし。複数回出力を許可する。

## 13. Index
idx_export_version、idx_export_user、idx_export_type。

## 14. Default値
exported_atは現在日時。

## 15. NULL可否
flow_version_id、export_type、exported_at、exported_byはNOT NULL。

## 16. CHECK制約
export_typeは定義済み形式のみ許可する。

## 17. 論理削除
履歴は原則削除しない。保存容量対策が必要な場合は別途保持期間を設計する。

## 18. 監査カラム
exported_at/exported_byが監査情報として機能する。

## 19. 更新タイミング
成果物生成成功時に追加する。

## 20. 削除ルール
通常削除しない。

## 21. Versionとの関係
必ずFlowVersionに紐付ける。

## 22. Templateとの関係
Template出力を行う場合は将来template_idを追加する。

## 23. サンプルデータ
```sql
insert into EXPORT_HISTORY(export_history_id, flow_version_id, export_type, file_name, exported_at, exported_by)
values('ex-001','fv-001','JSON','flow_fv-001.json','2026-07-03T00:00:00','u-001');
```

## 24. SQL例
```sql
select * from EXPORT_HISTORY where flow_version_id='fv-001' order by exported_at desc;
```

## 25. パフォーマンス
Version単位の履歴表示に備えflow_version_id Indexを使用する。

## 26. 将来拡張
ファイル保存キー、再ダウンロード、署名、出力差分を追加可能。

## 27. テスト観点
出力成功時に履歴が残ること、出力形式別に検索できることを確認する。

## 28. 完了条件
成果物の生成根拠をVersion単位で追跡できること。

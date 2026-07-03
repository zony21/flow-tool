# 04_11_TEMPLATE詳細設計

## 1. 本書の目的

本書は、TEMPLATE関連テーブルの詳細設計を定義する。TemplateはFlow作成時の雛形であり、Lane、Stage、Node、Link、Commentを含める。

## 2. テーブル概要

Templateは単なる名前付き雛形ではなく、構造化データの初期セットである。適用時にIDを再採番し、参照関係を張り替える。

## 3. 採用理由

包装ライン、WCS、AGF搬送などの典型構成を再利用し、設計品質と作成速度を向上させるため。

## 4. 利用機能

テンプレート選択、Flow作成、既存Flow複製、将来テンプレート管理。

## 5. 関連画面

テンプレート選択画面、フロー一覧画面、フローエディタ画面。

## 6. 関連API

GET /api/templates、GET /api/templates/{id}、POST /api/projects/{projectId}/flows/from-template。

## 7. ER上の位置

TEMPLATE 1:N TEMPLATE_LANE/STAGE/NODE/LINK/COMMENT。適用時にFLOW_VERSION配下へ変換する。

## 8. テーブル定義

| 論理名 | 物理名 |
| --- | --- |
| テンプレート | TEMPLATE |
| テンプレートレーン | TEMPLATE_LANE |
| テンプレート工程 | TEMPLATE_STAGE |
| テンプレートノード | TEMPLATE_NODE |
| テンプレート接続 | TEMPLATE_LINK |
| テンプレートコメント | TEMPLATE_COMMENT |

## 9. カラム一覧

TEMPLATEはtemplate_id、template_name、category、description、is_deleted、監査カラムを持つ。
子テーブルは通常テーブルと同等の構造を持つが、flow_version_idではなくtemplate_idを保持する。

## 10. PK
各テーブルは個別IDをPKとする。

## 11. FK
子テーブルはTEMPLATE.template_idを参照する。

## 12. Unique
template_nameの重複はカテゴリ単位で制限を検討する。

## 13. Index
idx_template_category、idx_template_deleted、子テーブルのtemplate_id Indexを設ける。

## 14. Default値
is_deleted=0、display_order=0。

## 15. NULL可否
template_name、template_idはNOT NULL。

## 16. CHECK制約
is_deletedは0/1。

## 17. 論理削除
Templateは論理削除を基本とする。

## 18. 監査カラム
共通監査カラムを保持する。

## 19. 更新タイミング
Template作成、編集、削除、構成変更時。

## 20. 削除ルール
利用済みTemplateを削除しても作成済みFlowには影響しない。

## 21. Versionとの関係
Template適用時にFLOW_VERSIONを新規作成する。

## 22. Templateとの関係
Template内のNode、Link、Comment参照は適用時に再採番・張替えする。

## 23. サンプルデータ
```sql
insert into TEMPLATE(template_id, template_name, category, is_deleted, created_at, created_by, updated_at, updated_by)
values('t-001','包装ライン基本','包装',0,'2026-07-03T00:00:00','u-001','2026-07-03T00:00:00','u-001');
```

## 24. SQL例
```sql
select * from TEMPLATE where is_deleted=0 order by category, template_name;
```

## 25. パフォーマンス
テンプレート選択画面ではcategory、is_deletedで検索するためIndexを利用する。

## 26. 将来拡張
テンプレート公開範囲、バージョン、タグ、AI生成テンプレートを追加可能。

## 27. テスト観点
Template作成、適用、ID再採番、Link参照張替え、削除影響なしを確認する。

## 28. 完了条件
Templateから整合したFlowVersion構造を生成できる設計であること。

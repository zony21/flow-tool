# 04_26_TRANSPORT詳細設計

## 1. 本書の目的

本書は、AGF/AGV搬送フローで利用するTransport関連テーブルの詳細設計を定義する。

Transport関連テーブルは、メーカー、コマンド、ロケーション、設備をProject単位で管理し、Transport FlowのNode属性および搬送表出力に利用する。

## 2. テーブル概要

Transport関連テーブルは以下で構成する。

- TRANSPORT_MANUFACTURER
- TRANSPORT_COMMAND
- LOCATION
- EQUIPMENT

## 3. 採用理由

AGF/AGV搬送ではメーカーごとに動作コマンドが異なる。

また、搬送表にはロケーション、設備、R/W、処理区分が必要であり、これらを自由文字列だけで管理すると表記揺れが発生する。

そのためProject単位のマスタとして構造化する。

## 4. TRANSPORT_MANUFACTURER

### 4.1 概要

AGF/AGVメーカーを管理する。

### 4.2 カラム一覧

| カラム | 型 | NULL | 説明 |
| --- | --- | --- | --- |
| manufacturer_id | TEXT | NO | PK、GUID |
| project_id | TEXT | NO | PROJECT FK |
| manufacturer_name | TEXT | NO | メーカー名 |
| vehicle_type | TEXT | NO | AGF、AGV、AMRなど |
| description | TEXT | YES | 説明 |
| display_order | INTEGER | NO | 表示順 |
| is_deleted | INTEGER | NO | 論理削除 |
| created_at | TEXT | NO | 作成日時 |
| created_by | TEXT | NO | 作成者 |
| updated_at | TEXT | NO | 更新日時 |
| updated_by | TEXT | NO | 更新者 |

## 5. TRANSPORT_COMMAND

### 5.1 概要

メーカー別の動作コマンドを管理する。

### 5.2 カラム一覧

| カラム | 型 | NULL | 説明 |
| --- | --- | --- | --- |
| command_id | TEXT | NO | PK、GUID |
| manufacturer_id | TEXT | NO | TRANSPORT_MANUFACTURER FK |
| command_name | TEXT | NO | TravelToPosture、Loadingなど |
| process_category | TEXT | NO | 移動、荷上げ、荷下ろしなど |
| description | TEXT | YES | 説明 |
| display_order | INTEGER | NO | 表示順 |
| is_deleted | INTEGER | NO | 論理削除 |
| created_at | TEXT | NO | 作成日時 |
| created_by | TEXT | NO | 作成者 |
| updated_at | TEXT | NO | 更新日時 |
| updated_by | TEXT | NO | 更新者 |

## 6. LOCATION

### 6.1 概要

搬送位置をProject単位で管理する。

### 6.2 カラム一覧

| カラム | 型 | NULL | 説明 |
| --- | --- | --- | --- |
| location_id | TEXT | NO | PK、GUID |
| project_id | TEXT | NO | PROJECT FK |
| location_code | TEXT | NO | P1、A1、ST1など |
| location_name | TEXT | YES | 表示名 |
| location_type | TEXT | NO | 経由点、荷役場所、充電位置、待機場所など |
| description | TEXT | YES | 説明 |
| display_order | INTEGER | NO | 表示順 |
| is_deleted | INTEGER | NO | 論理削除 |
| created_at | TEXT | NO | 作成日時 |
| created_by | TEXT | NO | 作成者 |
| updated_at | TEXT | NO | 更新日時 |
| updated_by | TEXT | NO | 更新者 |

## 7. EQUIPMENT

### 7.1 概要

PLC、RCS、WCS、コンベア、シャッターなどの実設備をProject単位で管理する。

### 7.2 カラム一覧

| カラム | 型 | NULL | 説明 |
| --- | --- | --- | --- |
| equipment_id | TEXT | NO | PK、GUID |
| project_id | TEXT | NO | PROJECT FK |
| equipment_name | TEXT | NO | 設備名 |
| equipment_category | TEXT | NO | PLC、RCS、WCS、コンベアなど |
| description | TEXT | YES | 説明 |
| display_order | INTEGER | NO | 表示順 |
| is_deleted | INTEGER | NO | 論理削除 |
| created_at | TEXT | NO | 作成日時 |
| created_by | TEXT | NO | 作成者 |
| updated_at | TEXT | NO | 更新日時 |
| updated_by | TEXT | NO | 更新者 |

## 8. Index

- idx_transport_manufacturer_project: project_id, is_deleted, display_order
- idx_transport_command_manufacturer: manufacturer_id, is_deleted, display_order
- idx_location_project: project_id, is_deleted, display_order
- idx_equipment_project: project_id, is_deleted, display_order

## 9. 削除ルール

各マスタは論理削除とする。

Nodeで参照中のデータは削除不可とする。

## 10. 利用画面

- AGF/AGVメーカー管理画面
- AGF/AGVコマンド管理画面
- 設備管理画面
- フローエディタ画面
- ロケーション管理ダイアログ

## 11. 利用API

- Transport Manufacturer API
- Transport Command API
- Location API
- Equipment API

## 12. テスト観点

- Project単位でメーカーを管理できること
- メーカー単位でコマンドを管理できること
- ロケーションを追加し、Transport Nodeで選択できること
- 設備を追加し、Transport Nodeで選択できること
- Nodeで使用中のマスタを削除できないこと

## 13. 完了条件

Transport Flowで利用するメーカー、コマンド、ロケーション、設備をProject単位で管理でき、Node属性および搬送表生成に利用できること。

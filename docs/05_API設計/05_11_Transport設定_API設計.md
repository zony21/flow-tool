# 05_11_Transport設定_API設計

## 1. 目的

本書は、AGF/AGV搬送フローで利用するTransport設定APIの詳細仕様を定義する。

Transport設定APIは、メーカー、メーカー別コマンド、ロケーション、設備をProject単位で管理する。

## 2. Manufacturer API

### 2.1 一覧取得

GET /api/projects/{projectId}/transport/manufacturers

Project配下のAGF/AGVメーカー一覧を返す。

### 2.2 作成

POST /api/projects/{projectId}/transport/manufacturers

| 項目 | 必須 | 説明 |
| --- | --- | --- |
| manufacturerName | YES | メーカー名 |
| vehicleType | YES | AGF、AGV、AMRなど |
| description | NO | 説明 |
| displayOrder | NO | 表示順 |

### 2.3 更新

PUT /api/projects/{projectId}/transport/manufacturers/{manufacturerId}

メーカー名、種別、説明、表示順を更新する。

### 2.4 削除

DELETE /api/projects/{projectId}/transport/manufacturers/{manufacturerId}

未使用メーカーのみ削除可能とする。

## 3. Command API

### 3.1 一覧取得

GET /api/projects/{projectId}/transport/manufacturers/{manufacturerId}/commands

メーカー配下のコマンド一覧を返す。

### 3.2 作成

POST /api/projects/{projectId}/transport/manufacturers/{manufacturerId}/commands

| 項目 | 必須 | 説明 |
| --- | --- | --- |
| commandName | YES | コマンド名 |
| processCategory | YES | 処理区分 |
| description | NO | 説明 |
| displayOrder | NO | 表示順 |

### 3.3 更新

PUT /api/projects/{projectId}/transport/commands/{commandId}

コマンド名、処理区分、説明、表示順を更新する。

### 3.4 削除

DELETE /api/projects/{projectId}/transport/commands/{commandId}

Nodeで未使用のコマンドのみ削除可能とする。

## 4. Location API

### 4.1 一覧取得

GET /api/projects/{projectId}/locations

Project配下のロケーション一覧を返す。

### 4.2 作成

POST /api/projects/{projectId}/locations

| 項目 | 必須 | 説明 |
| --- | --- | --- |
| locationCode | YES | P1、A1など |
| locationName | NO | 表示名 |
| locationType | YES | 経由点、荷役場所、充電位置など |
| description | NO | 説明 |
| displayOrder | NO | 表示順 |

### 4.3 更新

PUT /api/projects/{projectId}/locations/{locationId}

ロケーション情報を更新する。

### 4.4 削除

DELETE /api/projects/{projectId}/locations/{locationId}

Nodeで未使用のロケーションのみ削除可能とする。

## 5. Equipment API

### 5.1 一覧取得

GET /api/projects/{projectId}/equipments

Project配下の設備一覧を返す。

### 5.2 作成

POST /api/projects/{projectId}/equipments

| 項目 | 必須 | 説明 |
| --- | --- | --- |
| equipmentName | YES | 設備名 |
| equipmentCategory | YES | PLC、RCS、WCS、コンベア等 |
| description | NO | 説明 |
| displayOrder | NO | 表示順 |

### 5.3 更新

PUT /api/projects/{projectId}/equipments/{equipmentId}

設備情報を更新する。

### 5.4 削除

DELETE /api/projects/{projectId}/equipments/{equipmentId}

Nodeで未使用の設備のみ削除可能とする。

## 6. 共通エラー

- 400: 入力値不正
- 404: Projectまたは対象データが存在しない
- 409: 使用中データの削除不可
- 422: 種別値不正
- 500: 処理失敗

## 7. 権限

参照はProject参照権限で可能とする。
作成、更新、削除はProject編集権限またはTransport設定管理権限を必要とする。

## 8. テスト観点

- Project単位で各マスタを取得できること
- メーカー配下にコマンドを登録できること
- ロケーションを登録し、フローエディタで参照できること
- 設備を登録し、フローエディタで参照できること
- Nodeで使用中のデータを削除できないこと

## 9. 完了条件

Transport Flowで必要なメーカー、コマンド、ロケーション、設備をAPI経由で管理でき、フローエディタと搬送表出力へ利用できること。

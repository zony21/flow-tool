# 03_14_AGF_AGVメーカー管理画面

## 1. 本書の目的

本書は、AGF/AGVメーカー管理画面の仕様を定義する。

AGF/AGV搬送フローではメーカーごとに利用可能な動作コマンドが異なるため、メーカー情報をProject単位で管理できるようにする。

## 2. 画面の役割

本画面の役割は以下である。

- AGF/AGVメーカーの一覧表示
- メーカーの追加
- メーカーの編集
- メーカーの削除
- コマンド管理画面への導線

## 3. 表示項目

| 項目 | 説明 |
| --- | --- |
| メーカー名 | Mujin、Toyota、Nichiyuなど |
| 種別 | AGF、AGV、AMRなど |
| 説明 | 任意補足 |
| コマンド数 | 登録済みコマンド数 |
| 更新日時 | 最終更新日時 |

## 4. 操作

### 4.1 追加

メーカー名と種別を入力して登録する。

メーカー名は必須とする。

### 4.2 編集

既存メーカーの名称、種別、説明を変更する。

既にTransport Flowで利用中のメーカーは削除不可とするが、名称・説明の編集は可能とする。

### 4.3 削除

未使用メーカーのみ削除可能とする。

利用中の場合は削除不可とし、利用中FlowまたはCommandが存在する旨を表示する。

## 5. 画面遷移

```text
プロジェクト詳細
  ↓
AGF/AGVメーカー管理
  ├ メーカー追加
  ├ メーカー編集
  └ コマンド管理
```

## 6. 権限

Project編集権限またはTransport設定管理権限を持つユーザーのみ更新可能とする。

参照権限のみの場合は一覧表示のみ可能とする。

## 7. API

利用APIはTransport Manufacturer APIとする。

- GET /api/projects/{projectId}/transport/manufacturers
- POST /api/projects/{projectId}/transport/manufacturers
- PUT /api/projects/{projectId}/transport/manufacturers/{manufacturerId}
- DELETE /api/projects/{projectId}/transport/manufacturers/{manufacturerId}

## 8. DB

主にTRANSPORT_MANUFACTURERを利用する。

## 9. テスト観点

- メーカーを追加できること
- メーカーを編集できること
- 未使用メーカーを削除できること
- 使用中メーカーを削除できないこと
- コマンド管理画面へ遷移できること

## 10. 完了条件

AGF/AGV搬送フローで利用するメーカーをProject単位で管理でき、コマンド管理へ展開できる状態であること。

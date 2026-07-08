# 03_15_AGF_AGVコマンド管理画面

## 1. 本書の目的

本書は、AGF/AGVコマンド管理画面の仕様を定義する。

Transport Flowでは、メーカー固有コマンドをNodeの動作として選択し、搬送表の動作欄および処理区分判定に利用する。

## 2. 画面の役割

本画面の役割は以下である。

- メーカー別コマンド一覧表示
- コマンド追加
- コマンド編集
- コマンド削除
- 処理区分の紐付け

## 3. 表示項目

| 項目 | 説明 |
| --- | --- |
| メーカー | 対象メーカー |
| コマンド名 | TravelToPosture、Loading、Unloadingなど |
| 処理区分 | 移動、荷上げ、荷下ろしなど |
| 説明 | 任意補足 |
| 更新日時 | 最終更新日時 |

## 4. 処理区分

初期候補は以下とする。

- 移動
- 荷上げ
- 荷下ろし
- 待機
- 充電
- PLC書込み
- PLC読込み
- テーブル更新
- テーブル確認
- その他

処理区分はユーザー追加可能とする。

## 5. 操作

### 5.1 追加

メーカーを選択し、コマンド名と処理区分を登録する。

コマンド名、処理区分は必須とする。

### 5.2 編集

既存コマンドの名称、処理区分、説明を変更する。

既にNodeで利用中のコマンドは削除不可とするが、説明や処理区分の編集は可能とする。

### 5.3 削除

未使用コマンドのみ削除可能とする。

## 6. 画面遷移

```text
AGF/AGVメーカー管理
  ↓
AGF/AGVコマンド管理
```

## 7. API

利用APIはTransport Command APIとする。

- GET /api/projects/{projectId}/transport/manufacturers/{manufacturerId}/commands
- POST /api/projects/{projectId}/transport/manufacturers/{manufacturerId}/commands
- PUT /api/projects/{projectId}/transport/commands/{commandId}
- DELETE /api/projects/{projectId}/transport/commands/{commandId}

## 8. DB

主にTRANSPORT_COMMANDを利用する。

## 9. テスト観点

- メーカー別にコマンドを登録できること
- コマンドに処理区分を紐付けできること
- Transport Nodeで利用中のコマンドを削除できないこと
- 処理区分変更後、搬送表出力の処理区分に反映されること

## 10. 完了条件

AGF/AGVメーカー固有コマンドを構造化して管理し、Transport FlowのNode詳細および搬送表生成に利用できる状態であること。

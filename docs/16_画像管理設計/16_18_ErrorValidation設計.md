# 16_18_ErrorValidation設計

## 1. 目的

画像管理におけるErrorとValidation方針を定義する。

## 2. Validation対象

- ファイル形式
- ファイルサイズ
- MIME type
- 画像読み取り可否
- Project権限
- 参照中画像の削除可否

## 3. Error分類

- IMAGE_NOT_FOUND
- IMAGE_PERMISSION_DENIED
- IMAGE_UPLOAD_FAILED
- IMAGE_INVALID_FORMAT
- IMAGE_SIZE_EXCEEDED
- IMAGE_IN_USE
- IMAGE_STORAGE_ERROR

## 4. 表示方針

FrontendではDialog内ErrorとToastを使い分ける。
詳細調査用にtraceIdを表示できるようにする。

## 5. テスト観点

- 不正形式を拒否できる。
- サイズ超過を拒否できる。
- 使用中画像削除を拒否できる。
- 権限なし操作を拒否できる。

## 6. 完了条件

画像管理のErrorとValidationが定義されている。

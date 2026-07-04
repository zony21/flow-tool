# 16_17_画像Frontend設計

## 1. 目的

Frontendにおける画像管理UIと状態管理方針を定義する。

## 2. 基本方針

- 画像本体をStoreへ保持しない。
- MetadataをStoreで管理する。
- 表示はImage APIのURLを利用する。
- Image Nodeから画像選択Dialogを開ける。

## 3. UI要素

- 画像Upload Dialog
- 画像一覧Dialog
- 画像Preview Dialog
- Image Node Property Panel

## 4. Store

保持情報:

- imageId
- originalFileName
- mimeType
- width
- height
- updatedAt

## 5. 操作

- Upload
- Select
- Preview
- Replace
- Delete

## 6. テスト観点

- 画像一覧を表示できる。
- Image Nodeへ画像を設定できる。
- Previewを開ける。
- 権限なし操作を非活性にできる。

## 7. 完了条件

Frontend画像管理UIとStore方針が定義されている。

# 16_04_Storage設計

## 1. 目的

画像保存領域(Storage)の設計を定義する。

## 2. 基本方針

- DBと画像実体を分離する。
- Storage Provider方式にする。
- 初期はLocal Storageを利用する。
- 将来Cloud Storageへ差替可能にする。

## 3. 保存構造

project単位で保存する。

例:

/images/{projectId}/{imageId}.png

## 4. Storage Service責務

- 保存
- 取得
- 削除
- URL生成
- 存在確認

## 5. 禁止事項

- DB Blob保存
- Frontendへの物理パス公開

## 6. 完了条件

Storage抽象化方針が定義されている。

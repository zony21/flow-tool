# 16_07_画像表示Preview設計

## 1. 目的

画像表示およびPreview機能の設計を定義する。

## 2. 基本方針

- Canvasでは用途に応じたサイズを取得する。
- 一覧はThumbnailを利用する。
- 詳細Previewは元画像を利用する。
- 権限確認後に表示する。

## 3. 表示場所

- Image Node
- Property Panel
- Preview Dialog
- Export Preview

## 4. Image取得

FrontendはStorageパスを直接参照しない。

Image API経由で取得する。

## 5. Cache

ブラウザCacheを利用可能にする。
更新時はVersion値を変更する。

## 6. Error表示

- 画像なし
- 権限なし
- 読込失敗

## 7. 完了条件

安全な画像表示方式が定義されている。

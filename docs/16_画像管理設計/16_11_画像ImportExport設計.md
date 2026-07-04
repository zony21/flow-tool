# 16_11_画像ImportExport設計

## 1. 目的

画像を含むFlow Export / Import仕様を定義する。

## 2. 基本方針

- 構造データと画像ファイルを分離する。
- Export時は画像同梱方式を利用可能にする。
- Import時は新しい画像情報として登録する。

## 3. Export対象

- 画像Metadata
- 画像ファイル
- Image Node関連情報

## 4. Import処理

- ファイル検証
- 画像登録
- Node関連付け
- 構造確認

## 5. 完了条件

画像付きFlowの移行方針が定義されている。

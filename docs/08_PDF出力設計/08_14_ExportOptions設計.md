# 08_14_ExportOptions設計

## 1. 目的
PDF出力時の表示内容、用紙、余白、縮尺、分割を制御するオプションを定義する。

## 2. 設定項目
| 項目 | 内容 | 初期値 |
| --- | --- | --- |
| paperSize | A4 / A3 | A4 |
| orientation | portrait / landscape | landscape |
| marginMm | 上下左右余白 | 15 |
| scaleMode | fit / split / actual | fit |
| includeComments | Comment一覧 | true |
| includeAiMemo | AI専用メモ | true |
| includeTables | 一覧表 | true |
| includeHeaderFooter | ヘッダー・フッター | true |

## 3. バリデーション
未対応の用紙、負数余白、未対応scaleModeは400エラーとする。

## 4. 完了条件
同じFlowVersionと同じExportOptionsで同一内容のPDFを生成できる。

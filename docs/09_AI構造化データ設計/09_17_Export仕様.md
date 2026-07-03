# 09_17_Export仕様

## 1. 目的

AI構造化データを外部へ出力し、バックアップ、別環境移行、AI解析、設計書生成、将来のコード生成へ利用できるようにする。

## 2. Export形式

| 形式 | 用途 |
| --- | --- |
| JSON | 完全な構造保存、Import用 |
| AI DSL | AI読解用 |
| Mermaid | Markdown・GitHub表示用 |
| PDF | レビュー・承認用 |

## 3. JSON Export

JSON ExportはFlowVersion Snapshotを基準にする。
現在編集中の未保存状態はExportしない。

## 4. 出力範囲

- Project単位
- Flow単位
- Version単位

初期実装ではFlowVersion単位を優先する。

## 5. セキュリティ

Export実行にはProject閲覧権限が必要である。
private ProjectのExport結果には、出力者、出力日時、Project識別情報を含める。

## 6. 完了条件

ExportされたJSONから同一Flowを再Importでき、AI DSL、Mermaid、PDFの生成元として利用できる。

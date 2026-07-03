# 08_16_Backendサービス設計

## 1. 目的
PDF生成処理をBackend Serviceとして分離し、Controller、Repository、Rendererの責務を明確にする。

## 2. 構成
| クラス | 責務 |
| --- | --- |
| PdfExportController | API受付、権限確認、レスポンス返却 |
| PdfExportService | PDF生成ユースケース制御 |
| FlowSnapshotRepository | FlowVersion Snapshot取得 |
| PdfViewModelBuilder | PDF用ViewModel生成 |
| PdfLayoutCalculator | 座標変換、縮尺、分割計算 |
| PdfRenderer | PDF描画 |

## 3. 処理責務
ControllerはPDF描画を行わない。ServiceはPDFライブラリの詳細に依存せず、Rendererへ委譲する。

## 4. 完了条件
各責務が分離され、単体テストでViewModel生成、レイアウト計算、描画呼び出しを検証できる。

# 06_18_Export連携設計

## 1. 本書の目的

フローエディタとExport機能の連携仕様を定義する。
Exportは現在開いているFlowVersionの構造化データをもとに成果物を生成する。

## 2. 初期出力

- Mermaid flowchart
- Mermaid sequenceDiagram
- PDF
- JSON

## 3. 将来出力

- AI DSL
- API仕様
- DB更新一覧
- 通信一覧
- PLC一覧
- 設計書ドラフト
- コード生成用定義

## 4. 実行方針

Export実行前に未保存変更がある場合は保存を促す。
保存済みFlowVersionを基準にExport APIを呼び出す。

## 5. 出力履歴

出力成功後はEXPORT_HISTORYへ記録する。

## 6. テスト観点

- 保存済みVersionから出力できること
- 未保存時に確認されること
- Mermaid、PDF、JSONが生成できること
- 出力履歴が残ること

## 7. 完了条件

フローエディタから構造化データを基準に成果物を生成できること。

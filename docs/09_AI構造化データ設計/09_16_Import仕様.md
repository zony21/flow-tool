# 09_16_Import仕様

## 1. 目的

外部JSON、過去Export、将来のAI DSL変換結果からFlowを取り込むためのImport仕様を定義する。
Importでは、既存Projectへ追加する場合と新規Projectとして作成する場合を扱う。

## 2. Import対象

- Project JSON
- Flow JSON
- FlowVersion Snapshot JSON
- 将来のAI DSL

## 3. 処理フロー

1. ファイルを受け取る。
2. MIMEと拡張子を検証する。
3. JSON Schemaを検証する。
4. アプリケーションバリデーションを行う。
5. Import方式を選択する。
6. ID再採番を行う。
7. DBへ保存する。
8. Import結果を返す。

## 4. ID再採番

既存ProjectへImportする場合、Project内のID衝突を避けるためFlow、Lane、Stage、Node、Link、CommentのIDを再採番する。
LinkのfromNodeId、toNodeId、CommentのtargetNodeIdも対応表で置換する。

## 5. エラー処理

- JSON形式不正は400。
- Schema不一致は400。
- 参照不整合は409。
- 権限不足は403。

## 6. 完了条件

ExportしたFlowを再Importしても、Node、Link、Commentの関係が維持される。

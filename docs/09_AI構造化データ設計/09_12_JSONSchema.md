# 09_12_JSONSchema

## 1. 目的

AI構造化データのJSON形式を機械的に検証するため、JSON Schemaの設計方針を定義する。
Import、Export、API受信、Version Snapshot検証で利用する。

## 2. Schema管理方針

- schemaVersionを必須とする。
- Major Versionが異なるSchemaは互換性を保証しない。
- Minor Version差分はマイグレーションで吸収する。
- SchemaはBackendとテストで利用する。

## 3. 必須検証

| 対象 | 検証内容 |
| --- | --- |
| Project | projectId、name必須 |
| Flow | flowId、projectId、name必須 |
| Lane | laneId、name、orderNo必須 |
| Stage | stageId、name、orderNo必須 |
| Node | nodeId、nodeType、title、position、size必須 |
| Link | linkId、fromNodeId、toNodeId必須 |
| Comment | commentId、body、commentType必須 |

## 4. 参照整合性

JSON Schema単体ではNode存在チェックなどの参照整合性を完全に表現しない。
参照整合性は `09_13_バリデーション.md` のアプリケーションバリデーションで検証する。

## 5. 完了条件

Import前に構文・必須項目・型不正を検出でき、アプリケーション層で参照整合性検証へ進める。

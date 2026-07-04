# 15_14_ImportExport設計

## 1. 目的

本書は、AI Flow DesignerのTemplate Import / Export設計を定義する。

Templateを外部ファイルとして共有・保管できるようにし、別Projectや別環境へ取り込めるようにする。

## 2. 基本方針

- Export形式はJSONとする。
- SchemaVersionを必ず含める。
- Import時はSchemaVersionを検証する。
- Import後はProject Templateとして保存する。
- Import時に構造Validationを行う。

## 3. Export内容

- templateName
- description
- category
- tags
- templateJson
- schemaVersion
- exportedAt
- exportedBy

## 4. Importフロー

```text
JSON選択
  ↓
形式検証
  ↓
SchemaVersion確認
  ↓
Template構造Validation
  ↓
Project Templateとして保存
```

## 5. セキュリティ

Import JSONを信用しない。

検証対象:

- JSON形式
- 最大サイズ
- schemaVersion
- NodeType
- Link参照
- 画像参照

## 6. Export権限

Template参照権限があればExport可能とする。

ImportはProjectのEditor以上が可能とする。

## 7. テスト観点

- TemplateをJSON Exportできる。
- Export JSONをImportできる。
- 不正JSONを拒否できる。
- 未対応SchemaVersionを拒否できる。

## 8. 完了条件

- Template Import / Export形式、検証、権限が定義されている。

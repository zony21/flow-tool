# 21_02_JSON_Import仕様

## 1. 目的

AI Flow Designer互換JSONからFlowをImportする仕様を定義する。

## 2. 対象

JSON Importは、AI Flow DesignerがExportしたJSON、または同等SchemaのJSONを対象とする。

## 3. 必須構造

```json
{
  "project": {},
  "flow": {},
  "lanes": [],
  "stages": [],
  "nodes": [],
  "links": [],
  "comments": [],
  "images": [],
  "metadata": {}
}
```

## 4. Validation

| 項目 | 内容 |
| --- | --- |
| flow | 必須 |
| nodes | 配列必須 |
| links | 配列必須 |
| node.id | 必須、一意 |
| node.type | 必須、Registryに存在 |
| link.sourceNodeId | 必須、nodesに存在 |
| link.targetNodeId | 必須、nodesに存在 |

## 5. 変換方針

Import時は外部JSONのIDを内部IDへ再採番する。

ID Mappingを作成し、Link、Comment、Image参照を再解決する。

```text
externalNodeId -> internalNodeId
externalLinkId -> internalLinkId
```

## 6. Import結果

Import結果として以下を返す。

```json
{
  "success": true,
  "warnings": [],
  "errors": [],
  "createdFlowId": "..."
}
```

## 7. 完了条件

- JSON必須構造が定義されている
- Validation項目が定義されている
- ID Mapping方針が定義されている
- Import結果形式が定義されている

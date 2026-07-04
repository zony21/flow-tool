# 21_04_ImportValidation仕様

## 1. 目的

Import時のValidation仕様を定義する。

Import Validationは、外部データをSSOT構造へ取り込む前に不正や不足を検出し、Flow破損を防ぐために行う。

## 2. Validation分類

| 分類 | 内容 | Import可否 |
| --- | --- | --- |
| error | 構造として成立しない | 不可 |
| warning | 補完可能または注意が必要 | 可能 |
| info | 補足情報 | 可能 |

## 3. Error条件

| code | 内容 |
| --- | --- |
| IMPORT_FORMAT_INVALID | 形式不正 |
| IMPORT_VERSION_UNSUPPORTED | Version未対応 |
| FLOW_REQUIRED | Flow情報なし |
| NODE_ID_DUPLICATED | Node ID重複 |
| LINK_SOURCE_NOT_FOUND | Link sourceなし |
| LINK_TARGET_NOT_FOUND | Link targetなし |
| NODE_TYPE_REQUIRED | Node Typeなし |

## 4. Warning条件

| code | 内容 |
| --- | --- |
| DECISION_CONDITION_MISSING | Decision条件なし |
| NODE_WITHOUT_LANE | Lane未所属Node |
| NODE_WITHOUT_STAGE | Stage未所属Node |
| UNKNOWN_NODE_TYPE | 不明Node Type |
| COMMENT_TARGET_MISSING | Comment紐付先なし |
| IMAGE_METADATA_MISSING | Image metadata不足 |

## 5. Validation結果形式

```json
{
  "level": "warning",
  "code": "DECISION_CONDITION_MISSING",
  "message": "Decision条件が未設定です。",
  "targetType": "node",
  "targetId": "decision-001"
}
```

## 6. Preview制御

- errorが1件以上ある場合、Import確定不可
- warningのみの場合、確認後にImport可能
- infoのみの場合、そのままImport可能

## 7. 完了条件

- error/warning/infoの扱いが定義されている
- Import不可条件が明確である
- Previewで表示するValidation結果形式が定義されている

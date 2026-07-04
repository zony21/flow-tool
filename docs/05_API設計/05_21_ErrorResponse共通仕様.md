# 05_21_ErrorResponse共通仕様

## 1. 目的

Backend APIの共通Error Response仕様を定義する。

APIごとにError形式が揺れると、Frontend実装とAI実装の精度が下がるため、全APIで共通形式を使用する。

## 2. 基本方針

- Error Responseは全APIで統一する
- HTTP StatusとerrorCodeを分離する
- Validation Errorはdetailsで項目単位に返す
- FrontendはerrorCodeで表示制御する
- 予期しない例外はINTERNAL_ERRORへ変換する

## 3. 共通形式

```json
{
  "errorCode": "VALIDATION_ERROR",
  "message": "入力内容に誤りがあります。",
  "details": [
    {
      "field": "flowName",
      "message": "Flow名は必須です。"
    }
  ],
  "traceId": "string"
}
```

## 4. HTTP Status

| Status | 用途 |
| --- | --- |
| 400 | Validation Error、Request不正 |
| 401 | 未ログイン、認証情報不正 |
| 403 | 権限不足 |
| 404 | 対象なし |
| 409 | 競合、Version不一致 |
| 500 | 予期しないError |

## 5. errorCode一覧

| errorCode | Status | 内容 |
| --- | --- | --- |
| VALIDATION_ERROR | 400 | Validation Error |
| AUTH_REQUIRED | 401 | 未ログイン |
| PERMISSION_DENIED | 403 | 権限不足 |
| PROJECT_NOT_FOUND | 404 | Projectなし |
| FLOW_NOT_FOUND | 404 | Flowなし |
| VERSION_CONFLICT | 409 | Version競合 |
| IMPORT_VALIDATION_ERROR | 400 | Import不正 |
| EXPORT_FAILED | 500 | Export失敗 |
| INTERNAL_ERROR | 500 | 予期しないError |

## 6. Frontend表示方針

| errorCode | 表示 |
| --- | --- |
| VALIDATION_ERROR | 画面項目下に表示 |
| AUTH_REQUIRED | Loginへ遷移 |
| PERMISSION_DENIED | Toast + 権限再取得 |
| PROJECT_NOT_FOUND | NotFound画面 |
| VERSION_CONFLICT | 再読込確認Dialog |
| INTERNAL_ERROR | 共通Error Dialog |

## 7. 完了条件

- 共通Error Response形式が定義されている
- HTTP Statusの使い分けが定義されている
- errorCode一覧が定義されている
- Frontend表示方針が定義されている

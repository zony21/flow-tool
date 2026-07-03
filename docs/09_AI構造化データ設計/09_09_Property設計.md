# 09_09_Property設計

## 1. 目的

Propertyは、Project / Flow / Lane / Stage / Node / Link / Comment の標準項目では表現しきれない拡張情報を保持する仕組みである。
Node種別拡張、通信情報、設備情報、API情報、DB更新情報などを柔軟に表現する。

## 2. 基本方針

- 標準項目は明示的なカラムまたはフィールドとして定義する。
- 拡張情報のみpropertiesへ格納する。
- propertiesはJSON objectとする。
- AIが解釈しやすいようキー名は英語のcamelCaseを基本とする。
- 意味が重要なPropertyは将来標準項目へ昇格できるようにする。

## 3. Property例

```json
{
  "equipmentId": "AGF-01",
  "apiEndpoint": "/api/tasks/start",
  "dbTable": "T_TRANSPORT_TASK",
  "plcAddress": "D1000",
  "retryCount": 3
}
```

## 4. 型ルール

Property値はstring、number、boolean、object、arrayを許可する。
関数、式、循環参照は許可しない。

## 5. AI向け意味

AIはpropertiesを補助情報として扱う。
ただし、標準項目とpropertiesが矛盾する場合は標準項目を優先する。

## 6. 完了条件

拡張Nodeや将来機能の情報を、標準構造を壊さず保持できる。

# 05_08_Export API詳細設計

## 概要

構造化データから各種成果物を生成する。

初期対応:

- Mermaid flowchart
- Mermaid sequenceDiagram
- PDF
- JSON

将来対応:

- AI DSL
- API仕様
- DB更新一覧
- 通信一覧
- PLC一覧
- 設計書ドラフト

## API一覧

| メソッド | URL | 用途 |
|---|---|---|
| POST | /api/flows/{flowId}/export/mermaid | Mermaid出力 |
| POST | /api/flows/{flowId}/export/pdf | PDF出力 |
| POST | /api/flows/{flowId}/export/json | JSON出力 |
| POST | /api/flows/{flowId}/export/ai-dsl | AI DSL出力 |

## Mermaid Request

```json
{
  "type": "flowchart",
  "direction": "TD",
  "includeComments": true
}
```

## Mermaid Response

```json
{
  "fileName": "RFID読取り_flowchart.mmd",
  "content": "flowchart TD\n..."
}
```

## PDF Request

```json
{
  "pageSize": "A4",
  "orientation": "landscape",
  "scaleMode": "fit",
  "includeNodeList": true,
  "includeLinkList": true,
  "includeCommentList": true
}
```

## PDF Response

バイナリファイルを返す。

Content-Type:

```text
application/pdf
```

## JSON Export

Flow構造を正規化したJSONで返す。

## AI DSL Export

独自仕様として固定する。

```text
FLOW RFID読取り
LANE 包装PLC
STAGE RFID読取り
NODE RFID番号送信 TYPE Send
LINK RFID番号送信 -> WCS受信 DATA RFID番号 PROTOCOL TCP/IP
```

## 権限

閲覧権限以上で出力可能。

ただし、管理者設定によりExport禁止にできる。

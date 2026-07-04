# 15_08_NodeLinkテンプレート設計

## 1. 目的

本書は、TemplateにおけるNode / Linkの扱いを定義する。

NodeとLinkはFlowの処理内容と接続関係を表す中核であり、Template適用時に参照整合性を維持する必要がある。

## 2. 基本方針

- NodeとLinkはTemplate保存対象にする。
- NodeType固有Propertyを保持する。
- LinkのLabel、Condition、DataName、CommunicationTypeを保持する。
- 適用時はNodeとLinkのIDを新規発行する。
- Linkの接続関係は新しいNode IDへ更新する。

## 3. Node保存項目

- nodeId
- nodeType
- title
- description
- laneId
- stageId
- propertyJson
- positionX
- positionY
- width
- height
- aiNotes

## 4. Link保存項目

- linkId
- sourceNodeId
- targetNodeId
- label
- condition
- dataName
- communicationType
- linkType

## 5. 選択範囲Template化

選択NodeのみをTemplate化する場合、選択Node同士を結ぶLinkのみ保存する。

選択外Nodeへ接続するLinkは保存しない。

## 6. Validation

- NodeType存在確認
- Node名最大文字数
- Link接続先Node存在確認
- decision NodeのCondition不足Warning
- image Nodeの画像参照確認

## 7. テスト観点

- NodeType固有Propertyを保持できる。
- Link条件を保持できる。
- 選択外NodeへのLinkを除外できる。
- 適用後もLink接続が維持される。

## 8. 完了条件

- Node / Linkの保存項目と参照整合性方針が定義されている。

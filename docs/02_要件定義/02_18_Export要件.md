# 02_18_Export要件

## 1. 本書の目的

本書は、AI Flow Designer のExport要件を定義する。

Export機能は、構造化Flowデータから人間向け・AI向け・開発者向けの成果物を生成する機能である。

## 2. 基本方針

- Exportは構造化Flowデータから生成する
- 図面表示状態を正としない
- Export前にValidationを行う
- Viewer以上でExport可能とする
- Export結果はFlowを変更しない
- 将来的にExport形式を追加可能な構造とする

## 3. 初期Export対象

初期対応:

- Mermaid Flowchart
- Mermaid Sequence
- PDF
- JSON

## 4. 将来Export対象

将来対応:

- AI DSL
- API仕様
- DB更新一覧
- PLC一覧
- 通信一覧
- 設計書ドラフト
- CSV
- SVG

## 5. Mermaid Flowchart要件

目的:

- Flowの処理順序をMermaid Flowchartとして出力する

要件:

- NodeをMermaid Nodeへ変換する
- LinkをMermaid Edgeへ変換する
- Node種別に応じて形状を変える
- LinkラベルにdataNameやconditionを含める
- Lane / Stage情報をコメントまたはサブグラフで表現する

## 6. Mermaid Sequence要件

目的:

- Lane間のやり取りをSequence図として出力する

要件:

- Laneをparticipantとして出力する
- Linkをmessageとして出力する
- communicationTypeをmessageへ含める
- conditionをalt/opt表現へ変換できるようにする

## 7. PDF要件

目的:

- 人間がレビュー・共有しやすい資料として出力する

要件:

- Flow図をPDF化する
- ヘッダーにProject名、Flow名を表示する
- フッターに出力日時を表示する
- 大きなFlowではページ分割を考慮する
- Lane / Stage背景を出力できる

## 8. JSON要件

目的:

- 機械可読な構造データとして出力する

要件:

- schemaVersionを含む
- Flow構造を欠落なく含む
- ID参照を保持する
- 監査項目を含めるか選択できる
- 整形あり/なしを選択できる

## 9. AI DSL要件

AI DSLは将来Export対象だが、仕様は固定とする。

要件:

- AIが解析しやすい
- 人間も読める
- Node / Link / Lane / Stage / Commentを欠落なく表現する

## 10. API仕様Export要件

将来、LinkやNode情報からAPI仕様ドラフトを生成する。

対象:

- REST通信Link
- request / response情報
- dataName
- condition

## 11. DB更新一覧Export要件

将来、NodeやLinkの拡張情報からDB更新一覧を生成する。

対象:

- 更新対象テーブル
- 更新条件
- 更新タイミング
- 更新元Node

## 12. PLC一覧Export要件

将来、PLC連携要素を一覧出力する。

対象:

- PLC Lane
- PLC関連Node
- PLC通信Link
- 信号名
- 条件

## 13. 通信一覧Export要件

将来、Flow内通信を一覧化する。

対象:

- fromLane
- toLane
- dataName
- communicationType
- condition
- description

## 14. Export前Validation

Export前に以下を検証する。

- Flowが存在する
- Nodeが存在する
- Link接続が不正でない
- Lane / Stage参照が不正でない
- Export形式固有の必須情報が不足していない

## 15. Export権限

Viewer以上でExport可能とする。

ただし、将来的にProject設定でExport制限を設けられるようにする。

## 16. Export履歴

初期実装ではExport履歴保存は必須としない。

将来対応:

- Export日時
- Exportユーザー
- Export形式
- 対象Flow
- 成功 / 失敗

## 17. ファイル名要件

ファイル名例:

```text
{projectName}_{flowName}_{exportType}_{yyyyMMddHHmmss}.{ext}
```

ファイル名に利用できない文字は置換する。

## 18. 異常系

- Flowが存在しない
- 権限不足
- Validationエラー
- PDF生成失敗
- Mermaid生成失敗
- JSONシリアライズ失敗

## 19. テスト観点

- Mermaid Flowchartを生成できる
- Mermaid Sequenceを生成できる
- PDFを生成できる
- JSONを生成できる
- Export前Validationが動作する
- ViewerでExportできる
- Export失敗時にFlowが変更されない

## 20. 完了条件

- 初期Export形式が定義されている
- 将来Export形式の方針が定義されている
- Export前Validationが定義されている
- 権限と異常系が明確である

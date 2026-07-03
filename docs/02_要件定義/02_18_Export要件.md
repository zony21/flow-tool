# 02_18_Export要件

## 1. 本書の目的

本書は、AI Flow Designer のExport要件を定義する。

Exportは、Single Source of Truthである構造化Flowデータから、利用目的に応じた成果物を生成する機能である。

## 2. 基本方針

- Exportの正は構造化Flowデータである
- 図面表示状態をExportの正としない
- Export前にValidationを行う
- Viewer以上でExport可能とする
- Export結果はFlowを変更しない
- Export形式は将来追加可能な設計とする

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

要件:

- NodeをMermaid Nodeとして出力する
- LinkをMermaid Edgeとして出力する
- Link条件をラベルへ含める
- NodeTypeに応じた形状表現を検討する
- Lane / Stage情報はコメントまたはサブグラフで表現する

## 6. Mermaid Sequence要件

要件:

- Laneをparticipantとして出力する
- Linkをメッセージとして出力する
- communicationTypeを表現する
- conditionをalt / opt相当で表現する
- 処理順序はFlow構造とStage順をもとに決定する

## 7. PDF要件

PDFでは人間がレビューしやすいフロー図を出力する。

要件:

- Flow図をPDF化する
- 用紙サイズを選択できる
- 縦横向きを選択できる
- ヘッダーにProject名 / Flow名を表示する
- フッターに出力日時を表示する
- 大きなFlowはページ分割または縮小する

## 8. JSON要件

JSONでは構造化データを機械可読形式で出力する。

要件:

- schemaVersionを含める
- Flow構造を欠落なく含める
- Lane / Stage / Node / Link / Commentを含める
- 監査項目を含めるか選択できる
- 整形有無を選択できる

## 9. AI DSL要件

AI DSLは将来対応だが、要件上はExport対象として定義する。

要件:

- 独自仕様として固定する
- AIが解析しやすい構文にする
- Flow、Lane、Stage、Node、Linkの意味情報を保持する
- JSONと矛盾しない

## 10. API仕様Export要件

将来、FlowからAPI仕様ドラフトを生成する。

対象:

- API呼び出しNode
- 通信Link
- Request / Response相当情報
- エラー条件

## 11. DB更新一覧Export要件

将来、FlowからDB更新一覧を生成する。

対象:

- DB更新Node
- 登録 / 更新 / 削除処理
- 対象テーブル
- 更新タイミング

## 12. PLC一覧Export要件

将来、PLC連携を含むFlowからPLC一覧を生成する。

対象:

- PLC Lane
- PLC通信Link
- 信号名
- 条件
- ON / OFFタイミング

## 13. 通信一覧Export要件

将来、Link情報から通信一覧を生成する。

対象:

- from Lane
- to Lane
- dataName
- communicationType
- condition
- description

## 14. Exportオプション要件

共通オプション:

- 出力形式
- ファイル名
- 監査項目含有
- コメント含有
- 画像含有
- 内部ID含有

形式別オプションは各Export設計で定義する。

## 15. Export前Validation

検証対象:

- Node名不足
- Link接続先不足
- Lane / Stage不足
- 画像Node画像なし
- Export形式固有必須項目不足

エラーがある場合は原則Export不可とする。

警告のみの場合は警告付きExportを許可できる。

## 16. ファイル名要件

命名例:

```text
{projectName}_{flowName}_{exportType}_{yyyyMMddHHmmss}.{ext}
```

ファイル名に利用できない文字は置換する。

## 17. 権限要件

ExportはViewer以上で可能とする。

ただし、Project設定でExport制限を導入する場合はその設定を優先する。

## 18. 異常系

- Flowが存在しない
- 権限なし
- Validationエラー
- PDF生成失敗
- JSON生成失敗
- ファイル書き出し失敗

## 19. テスト観点

- Mermaid Flowchartを出力できる
- Mermaid Sequenceを出力できる
- PDFを出力できる
- JSONを出力できる
- Validationエラー時に出力不可になる
- ViewerがExportできる
- ExportしてもFlowが変更されない

## 20. 完了条件

- 初期Export対象が定義されている
- 将来Export対象が定義されている
- Export前Validationが定義されている
- 権限とファイル名ルールが明確である

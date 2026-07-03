# 07_01_Mermaid出力方針

## 1. 本書の目的

本書は、AI Flow Designer の構造化データからMermaidを生成するための共通方針を定義する。
Mermaid出力は、人が確認しやすく、AIが再解析しやすいテキスト形式の成果物として扱う。

## 2. 基本方針

Mermaidは画面描画結果からではなく、FlowVersion配下のLane、Stage、Node、Link、Commentから生成する。
正となるのは構造化データであり、Mermaidは出力結果である。

## 3. 出力対象

初期対応は以下とする。

- flowchart
- sequenceDiagram

## 4. 採用理由

MermaidはMarkdownに埋め込みやすく、設計書、GitHub、AI入力に利用しやすい。
また、テキストで差分管理できるためGit管理と相性がよい。

## 5. 変換単位

Mermaid生成はFlowVersion単位で行う。
Project全体や複数Flow統合出力は将来拡張とする。

## 6. 出力原則

- Node IDを安定識別子として使用する
- 表示名はラベルとして使用する
- Link方向はFROMからTOを正とする
- 条件、データ名、通信種別をLinkラベルへ含める
- Lane、Stageは可能な範囲でsubgraphへ変換する

## 7. 関連画面

- フローエディタ画面
- 出力画面
- バージョン管理画面

## 8. 関連API

- POST /api/flow-versions/{flowVersionId}/export/mermaid

## 9. 関連DB

- FLOW_VERSION
- LANE
- STAGE
- NODE
- LINK
- COMMENT

## 10. テスト観点

- FlowVersionからMermaidを生成できること
- Node IDが安定していること
- Link方向が正しく出力されること
- ループを表現できること

## 11. 完了条件

Mermaid出力がSSOTの構造化データを基準に生成されること。

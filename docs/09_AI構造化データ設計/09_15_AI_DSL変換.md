# 09_15_AI_DSL変換

## 1. 目的

AI DSLは、JSONとは別に定義する独自仕様であり、AIがFlowの意味を読み取りやすい形式へ変換するための出力である。
JSONは機械保存向け、AI DSLはAI読解向けとして役割を分離する。

## 2. 変換元

AI DSLの変換元はFlowVersion SnapshotのAI構造化データとする。
画面上の図形や一時状態から直接生成しない。

## 3. 出力単位

- PROJECT
- FLOW
- LANE
- STAGE
- NODE
- LINK
- COMMENT
- AI_MEMO

## 4. DSL例

```text
FLOW 搬送開始フロー
LANE WCS responsibility="搬送指示と状態管理"
STAGE 読取工程
NODE NODE-001 type=process lane=WCS stage=読取工程 title="RFID読取"
LINK NODE-001 -> NODE-002 condition="読取成功" data="rfidNo"
COMMENT target=NODE-001 type=aiMemo "読取失敗時は再試行分岐へ進む"
```

## 5. 変換ルール

- IDは保持する。
- Lane / Stage名称を併記し、AIの責務理解を補助する。
- Link条件、データ名、通信方式は省略しない。
- CommentとAI専用メモは区別して出力する。
- Loopは削除せず、そのまま出力する。

## 6. 完了条件

AI DSLだけをAIへ渡しても、Flowの工程、責務、接続、条件、補足を理解できる。

# 06_22_Stage列表示仕様

## 1. 目的

Flow EditorにおけるStage（列）表示仕様を定義する。

AI Flow Designerでは、誰が担当するか（Lane）だけでなく、いつ・どの工程で実行されるか（Stage）を明確化する。

これによりAI解析時に、処理順序・責務・工程位置を正確に判断できるようにする。

## 2. 基本思想

従来の横方向のみのフローでは以下が曖昧になる。

- 処理タイミング
- 工程区分
- 責任境界
- 前後関係

そのためCanvasは以下の2軸構造を基本とする。

```text
              Stage（工程）

          受付     処理     確認     完了
        +--------+--------+--------+--------+
営業    | Node   |        |        |        |
        +--------+--------+--------+--------+
WCS     |        | Node   | Node   |        |
        +--------+--------+--------+--------+
PLC     |        |        | Node   |        |
        +--------+--------+--------+--------+

Lane（責務）
```

## 3. 用語定義

| 用語 | 意味 |
| --- | --- |
| Lane | 担当・システム・責任範囲 |
| Stage | 工程・時系列・処理フェーズ |
| Cell | Lane × Stage の交点 |
| Node | Cell内に配置される処理 |

## 4. Stage基本仕様

StageはFlow配下に存在する。

```text
Flow
 ├ Lane
 ├ Stage
 └ Node
```

Nodeは以下を保持する。

```text
node {
 laneId
 stageId
 position
}
```

## 5. Stage操作

対応操作:

- Stage追加
- Stage名変更
- Stage削除
- Stage順序変更
- Stage幅変更
- Stage表示/非表示

## 6. Stage削除時

削除時はユーザー選択とする。

選択肢:

1. 内部Nodeも削除
2. Nodeを未分類Stageへ移動

勝手な削除は禁止。

## 7. Node移動仕様

Node移動時:

- 横移動 → stageId更新
- 縦移動 → laneId更新

位置情報だけではなく、必ず構造情報を更新する。

## 8. AI DSL出力

AI DSLではStage情報を必ず出力する。

例:

```text
NODE {
 id: node-001
 label: 在庫確認
 lane: WCS
 stage: 出荷準備
}
```

AI判断:

「出荷準備工程でWCSが在庫確認する」

まで理解可能にする。

## 9. Validation

チェック項目:

| 条件 | Level |
| --- | --- |
| NodeにLaneなし | warning |
| NodeにStageなし | warning |
| StageなしFlow | info |
| LinkがStage順序を逆行 | info |

※Loopを許可しているため逆方向Linkは禁止しない。

## 10. UI表示

初期表示:

- Lane表示 ON
- Stage表示 ON
- Grid ON

設定により非表示可能。

## 11. 完了条件

- Lane + Stage二軸表示できる
- Node所属工程を管理できる
- AI DSLへ工程情報を渡せる
- AIが担当者と処理タイミングを判断できる

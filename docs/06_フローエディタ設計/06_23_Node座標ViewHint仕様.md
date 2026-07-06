# 06_23_Node座標ViewHint仕様

## 1. 目的

Flow Nodeが保持する座標情報の意味を定義する。

AI Flow Designerでは、図形の見た目ではなく構造化データを正とする。

そのため、NodeのX/Y座標は設計意味を表すSSOT情報ではなく、Canvas表示のためのView Hintとして扱う。

## 2. 基本方針

NodeのX/Y座標は、ユーザーがCanvas上で見やすく編集するための表示補助情報である。

設計上の意味は以下で判断する。

- Flow
- Lane
- Stage
- Node Type
- Node Properties
- Link
- Comment
- Metadata

座標だけで処理順、責務、正当性を判断してはならない。

## 3. 禁止用途

NodeのX/Y座標を以下に使ってはならない。

| 禁止用途 | 理由 |
| --- | --- |
| AI DSLの意味解析 | AI理解は構造化データを正とするため |
| 処理順推定 | 処理順はLinkとStageで判断するため |
| 責務推定 | 責務はLaneで判断するため |
| 正当性判定 | ValidationはNode/Link/Lane/Stage/Metadataで行うため |
| API/DB影響判断 | 影響情報はNode PropertiesやMetadataで保持するため |

## 4. 許可用途

NodeのX/Y座標は以下にのみ利用できる。

| 許可用途 | 内容 |
| --- | --- |
| Canvas描画 | ユーザーが編集しやすい位置にNodeを表示する |
| PDFレイアウト補助 | 人間向け出力時の配置補助に使う |
| ユーザー視認性向上 | Node重なり回避や見やすさの調整に使う |
| 再表示補助 | 前回編集時の見た目を再現する |

## 5. Lane / Stageとの関係

Nodeがどの担当・工程に属するかは、X/Y座標ではなく以下で判断する。

```text
Node.LaneId
Node.StageId
```

Canvas上でNodeを移動した場合、表示座標だけでなくLaneIdとStageIdも更新する。

## 6. Exportでの扱い

| Export | X/Y利用 |
| --- | --- |
| JSON | View Hintとして出力可 |
| Mermaid | 原則利用しない |
| PDF | レイアウト補助として利用可 |
| AI DSL | 意味解析には利用しない。必要な場合もView Hintとして明示する |

## 7. AI DSLでの扱い

AI DSLでは、X/Yを処理意味として扱わない。

必要に応じて以下のようにView Hintとして分離する。

```text
VIEW_HINT {
  nodeId
  x
  y
}
```

AIへのInstructionでは、View Hintを意味解析に利用しないことを明記する。

## 8. 将来方針

初期実装ではFlowNodeにX/Yを保持する。

将来、複数ユーザー別・Project別・表示レイアウト別の需要が出た場合は、Editor Layout専用テーブルへの分離を検討する。

候補:

```text
NODE_VIEW_LAYOUT
- USER_ID
- PROJECT_ID
- FLOW_ID
- NODE_ID
- X
- Y
```

ただし初期実装では過剰分離を避け、X/YをView Hintとして扱う。

## 9. 完了条件

- X/Yが設計意味ではなくView Hintであることが明確である
- 禁止用途と許可用途が定義されている
- Lane/Stageが責務・工程判断の正であることが明確である
- AI DSLで意味解析に使わないことが明確である

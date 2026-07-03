# 13_04_Link・Bezier描画設計

## 1. 本書の目的

本書は、AI Flow Designer のLinkおよびBezier描画設計を定義する。

Linkは単なる線ではなく、Node間の関係、データ移動、通信、条件、制御、処理順序を表す構造化要素である。

初期実装ではBezierを採用し、将来的に直線・折れ線・Bezierを切替可能にする。

## 2. Linkの基本構造

Linkは以下を保持する。

- linkId
- flowId
- fromNodeId
- toNodeId
- fromAnchor
- toAnchor
- dataName
- communicationType
- condition
- description
- linkType
- controlPoints
- order
- isDeleted

## 3. LinkType

初期値:

```text
Bezier
```

将来:

- Straight
- Orthogonal
- Bezier

DBにはlinkTypeを保持する。

## 4. Bezier採用理由

Bezierを初期採用する理由:

- 視認性が高い
- 複雑なフローでも線が重なりにくい
- Vue Flow標準と相性が良い
- 将来の制御点編集へ拡張しやすい

## 5. Link描画レイヤ

LinkはNodeより背面に表示する。

ただし、選択中LinkやHover Linkは強調表示する。

レイヤ順:

```text
Lane/Stage背景
Link
Node
Comment
Selection
Handle
```

## 6. 接続点

LinkはNodeのAnchorに接続する。

保持:

- fromAnchor
- toAnchor

未指定時は自動判定する。

自動判定:

- FROM NodeとTO Nodeの中心位置を比較
- 方向に応じてRight/Left/Top/Bottomを選択

## 7. Bezier制御点

Bezierは以下で構成する。

- startPoint
- controlPoint1
- controlPoint2
- endPoint

初期は自動計算する。

ユーザーが制御点を編集した場合、controlPointsへ保存する。

## 8. 自動Bezier計算

基本式:

```text
dx = abs(end.x - start.x)
offset = max(dx * 0.5, 80)

control1 = start + directionFromAnchor * offset
control2 = end + directionToAnchor * offset
```

## 9. Anchor方向

```text
Right  = (1, 0)
Left   = (-1, 0)
Top    = (0, -1)
Bottom = (0, 1)
```

## 10. ループLink

ループは許可する。

対象:

- A -> A
- A -> B -> A
- 循環フロー

自己Linkの場合は専用のBezier形状で描画する。

自己Link表示例:

- Node右側から出る
- 上方向へ回る
- Node上部または右側へ戻る

## 11. Linkラベル

Linkにはラベルを表示できる。

表示対象:

- dataName
- condition
- communicationType

初期表示優先:

```text
condition > dataName > communicationType
```

複数表示は将来設定可能。

## 12. ラベル位置

ラベルはBezier曲線の中点付近に表示する。

ユーザーが移動した場合、labelOffsetを保持する設計も将来可能とする。

初期は自動位置。

## 13. 条件表示

判定Nodeから出るLinkではconditionを重視する。

例:

- Yes
- No
- OK
- NG
- 在庫あり
- 異常あり

conditionはAI DSL、Mermaid、設計書出力でも使用する。

## 14. 通信種別

communicationType例:

- HTTP
- REST
- TCP
- UDP
- PLC
- FTP
- File
- Manual
- Internal
- DB

Link描画上はアイコンまたはラベルで表示可能とする。

## 15. Link作成

Link作成手順:

1. fromNodeのHandleをドラッグ
2. toNodeのHandleへドロップ
3. 接続可否判定
4. AddLinkCommand発行
5. Link作成
6. プロパティパネルで詳細編集

## 16. 接続可否判定

初期ルール:

- fromNodeId必須
- toNodeId必須
- 削除済Node不可
- 読み取り専用不可
- ループ許可
- 同一Node自己接続許可

将来的にNodeType別制約を追加可能。

## 17. Link再接続

Linkの始点・終点を変更できる。

操作:

- Link端点をドラッグ
- 別Node Handleへ接続
- ReconnectLinkCommand発行

保持:

- beforeFromNodeId
- beforeToNodeId
- afterFromNodeId
- afterToNodeId
- beforeAnchor
- afterAnchor

## 18. Link削除

Link削除は論理削除とする。

Deleteキーまたはメニューで削除する。

Node削除時は関連Linkも削除対象となる。

## 19. Edge ViewModel

Vue Flow Edge ViewModel例:

```ts
type FlowLinkViewModel = {
  id: string
  source: string
  target: string
  sourceHandle?: string
  targetHandle?: string
  type: 'bezier' | 'straight' | 'orthogonal'
  label?: string
  data: {
    linkId: string
    dataName?: string
    condition?: string
    communicationType?: string
    validationErrors: string[]
    readonly: boolean
  }
}
```

## 20. Validation

Link Validation例:

- fromNodeIdなし
- toNodeIdなし
- fromNodeが存在しない
- toNodeが存在しない
- 削除済Nodeへ接続
- communicationTypeが不正
- conditionが長すぎる

表示:

- 線をエラー表示
- ラベル横にエラーアイコン
- プロパティパネルで詳細表示

## 21. Mermaid出力との関係

Mermaid FlowchartではLinkは矢印として出力する。

conditionまたはdataNameをラベルとして利用する。

例:

```text
A -->|OK| B
```

## 22. Sequence出力との関係

Mermaid SequenceではLinkのfrom/toの担当レーンを利用する。

LinkのcommunicationType、dataName、conditionからメッセージを生成する。

## 23. AI DSLとの関係

AI DSLではLinkは関係情報として出力する。

必要情報:

- fromNode
- toNode
- dataName
- communicationType
- condition
- description
- isLoop

## 24. 性能注意点

10000Linkを想定する。

避けること:

- Linkごとの重いDOM
- 常時Tooltip生成
- Linkごとのwatch
- 毎フレーム全Link再計算
- Nodeドラッグ中に全Linkを過剰再描画

対策:

- Vue FlowのEdge描画を活用
- 表示範囲外Linkの最適化を将来検討
- 選択中/関連Linkのみ詳細表示
- ラベル簡略表示

## 25. 禁止事項

- 線の見た目だけで意味を管理する
- conditionをNode名へ埋め込む
- communicationTypeを自由文字列だけで無制御にする
- Link再接続時に別Linkを新規作成して旧Linkを残す
- Node削除後に孤立Linkを残す

## 26. テスト観点

- Link作成でfrom/toが保存される
- Bezierが正しく描画される
- Node移動でLinkが追従する
- Link再接続ができる
- 自己Linkが描画できる
- conditionがラベル表示される
- Link削除で論理削除される
- 10000Linkで表示できる
- Mermaid出力とLink情報が一致する

## 27. 完了条件

- Bezier Linkが描画できる
- Linkに意味情報を保持できる
- 再接続・削除・選択が可能
- ループを許可できる
- Exportと整合する

# 13_05_LaneStage背景描画設計

## 1. 本書の目的

本書は、AI Flow Designer のLaneおよびStage背景描画設計を定義する。

Laneは担当、Stageは工程を表す。これらはAIがシステムフローを高精度に理解するための重要な構造要素である。

Nodeがどの担当・どの工程に属するかを明確にするため、Lane/Stageは視覚的にも構造的にも明確に扱う。

## 2. Laneの意味

Laneは担当を表す。

例:

- 包装PLC
- 統括PLC
- WCS
- RCS
- AGF
- MES
- ERP
- 作業者

保持情報:

- laneId
- flowId
- name
- category
- icon
- color
- description
- order
- size

## 3. Stageの意味

Stageは工程を表す。

例:

- RFID読取り
- 搬送要求
- ラベル印刷
- 搬送完了
- 異常停止

保持情報:

- stageId
- flowId
- name
- color
- description
- order
- size

## 4. 基本表示

画面構成:

```text
上: Laneヘッダ
左: Stageヘッダ
中央: Node配置キャンバス
```

LaneとStageの交差領域がNode配置領域となる。

## 5. 背景レイヤ

Lane/Stage背景はNode/Linkより背面に描画する。

```text
Grid
Stage Background
Lane Background
Link
Node
Comment
Selection
```

## 6. Laneヘッダ

Laneヘッダに表示するもの:

- 担当名
- カテゴリ
- アイコン
- 色
- 説明Tooltip
- 表示順

Laneヘッダは横方向に並ぶ。

## 7. Stageヘッダ

Stageヘッダに表示するもの:

- 工程名
- 色
- 説明Tooltip
- 表示順

Stageヘッダは縦方向に並ぶ。

## 8. 交差領域

LaneとStageの交差領域には薄い背景色または区切り線を表示する。

目的:

- Nodeの所属を視覚的に理解しやすくする
- 担当と工程の関係を明確化する
- AI向け構造化情報と見た目を一致させる

## 9. サイズ

Lane幅、Stage高さは設定可能とする。

初期値:

```text
laneWidth = 240
stageHeight = 160
laneHeaderHeight = 64
stageHeaderWidth = 160
```

将来、個別Lane/Stageごとのサイズ変更を可能にする。

## 10. Node所属

NodeはlaneIdとstageIdを保持する。

座標だけで所属を判断しない。

Lane/Stage背景は、Nodeの所属を補助表示する。

## 11. 所属領域計算

Lane範囲:

```text
laneX = stageHeaderWidth + lane.order * laneWidth
laneY = 0
laneWidth = lane.size
```

Stage範囲:

```text
stageX = 0
stageY = laneHeaderHeight + stage.order * stageHeight
stageHeight = stage.size
```

交差領域:

```text
x = laneX
y = stageY
width = laneWidth
height = stageHeight
```

## 12. Node移動時のハイライト

Node移動中、移動先候補のLane/Stage交差領域をハイライトする。

表示:

- 背景色強調
- 境界線強調
- Lane名/Stage名の強調

## 13. Lane追加

Lane追加時:

1. AddLaneCommand発行
2. laneId採番
3. order設定
4. 背景再計算
5. Node配置領域拡張

## 14. Stage追加

Stage追加時:

1. AddStageCommand発行
2. stageId採番
3. order設定
4. 背景再計算
5. Node配置領域拡張

## 15. Lane削除

Lane削除は選択式。

- ノード移動
- 同時削除

UIでは削除前にダイアログを出す。

ノード移動の場合:

- 移動先Laneを選択
- 対象NodeのlaneIdを更新

同時削除の場合:

- Lane配下Nodeを論理削除
- 関連Linkを論理削除

## 16. Stage削除

Stage削除も選択式。

- ノード移動
- 同時削除

Stage削除時は移動先Stageを選択する。

## 17. Lane並び替え

Laneは表示順を変更できる。

変更時:

- ReorderLaneCommand発行
- lane.order更新
- 背景再計算
- Node座標は原則変更しないか、所属Laneの移動に追従するかを選択する

初期方針:

- 視覚的整合のためNode座標もLane移動分だけ追従する

## 18. Stage並び替え

Stageも表示順を変更できる。

初期方針:

- Node座標もStage移動分だけ追従する

## 19. 色

Lane/Stageには色を設定できる。

色は背景に薄く反映する。

濃すぎる色はNode/Linkの視認性を下げるため、透明度を調整する。

## 20. アイコン

Laneにはアイコンを設定できる。

例:

- PLC
- Server
- Robot
- Human
- Database
- ExternalSystem

アイコンはヘッダに表示する。

## 21. Validation

Lane/Stage Validation:

- name未入力
- order重複
- 削除対象にNodeがあるが削除方針未指定
- Nodeが存在しないLane/Stageを参照

表示:

- ヘッダにエラーアイコン
- プロパティパネルで詳細表示

## 22. Exportとの関係

Mermaid SequenceではLaneがparticipantになる。

PDFではLane/Stage背景を出力する。

JSON/AI DSLではNodeのlaneId/stageIdを明示する。

## 23. 性能注意点

Lane/Stageは数十件程度を想定する。

ただし、背景の交差領域をすべてDOM化すると数が増える可能性がある。

対策:

- SVGまたはCanvas的に背景を描画
- 必要最小限のDOM
- 表示範囲外の交差背景は簡略化

## 24. 禁止事項

- Lane/Stageを単なる背景画像として扱う
- Node所属を座標だけに依存する
- Lane削除で無確認にNodeを削除する
- Stage削除で無確認にNodeを削除する
- Lane/Stage名をNode名へ埋め込む

## 25. テスト観点

- Laneが表示順どおり表示される
- Stageが表示順どおり表示される
- Node移動で所属候補がハイライトされる
- Lane削除で移動/同時削除を選択できる
- Stage削除で移動/同時削除を選択できる
- Lane並び替えで背景が更新される
- Stage並び替えで背景が更新される
- Export時に担当・工程が保持される

## 26. 完了条件

- Lane/Stageが構造要素として描画される
- Node所属が視覚的に分かる
- 削除時の選択式挙動が実装可能
- Export/AI DSLと整合する

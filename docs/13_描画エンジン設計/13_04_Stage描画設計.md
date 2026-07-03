# 13_04_Stage描画設計

## 1. 目的

本書は、AI Flow DesignerのStage描画設計を定義する。

Stageは工程区切りを表す。Laneが責務・担当を表すのに対し、Stageは処理の時間的・工程的なまとまりを表す。

## 2. 基本方針

- StageはFlow配下の構造要素として扱う。
- StageはCanvas上で縦方向の列として描画する。
- NodeはStageIdを保持できる。
- Stageは工程目的を持つ。
- Stage幅はユーザー調整可能とする。
- Stage削除時は内包Nodeの扱いを確認する。

## 3. Stage描画Model

```ts
interface DrawingStage {
  stageId: string;
  name: string;
  purpose: string;
  orderNo: number;
  x: number;
  y: number;
  width: number;
  height: number;
  style?: StageStyle;
  hasValidationError: boolean;
}
```

## 4. 表示項目

- Stage名
- 工程目的
- Header背景
- 境界線
- Resize Handle
- Warning Badge

## 5. Stage配置

初期方針ではStageは横方向に並べる。

```text
| Stage 1 | Stage 2 | Stage 3 |
```

LaneとStageを組み合わせることで、責務 × 工程のMatrixとして視覚化する。

## 6. Stage幅

Stage widthは以下で決定する。

- 最小幅
- 内包Nodeの最大X座標
- ユーザーリサイズ値

初期最小幅:

```text
240px
```

## 7. Node所属判定

Node移動終了時に、Node中心点が含まれるStageを候補にする。

処理:

1. Node drag stop。
2. Node centerを計算。
3. Stage boundsと比較。
4. stageIdを更新候補にする。
5. MoveNodeCommandへ反映。

## 8. Stage並び替え

StageはorderNoで並び順を持つ。

並び替え時:

- orderNoを再計算する。
- Stage boundsを再計算する。
- NodeのstageIdは維持する。
- 必要に応じてNode座標を補正する。

## 9. Stage削除

Stage削除時はDialogで選択する。

選択肢:

- MoveNodes: 内包Nodeを別Stageへ移動
- DeleteWithNodes: 内包Nodeと関連Linkを削除
- Cancel

## 10. Laneとの関係

NodeはLaneIdとStageIdの両方を持てる。

描画上は以下の交差領域で配置を判断する。

```text
Lane bounds ∩ Stage bounds
```

NodeがLaneとStageの両方に所属する場合、その交差セル内に配置する。

## 11. Validation表示

Stage名未入力、工程目的未入力はWarning表示する。

Stage目的はAI DSLや設計書出力で利用するため、未入力は警告扱いとする。

## 12. 禁止事項

- Stageを単なる罫線として扱う。
- Stage削除時に内包Nodeを無確認で削除する。
- Stage目的を出力対象から除外する。
- NodeのStage所属を座標だけで変更不能にする。

## 13. テスト観点

- StageがorderNo順に描画される。
- Stage幅変更が描画Modelへ反映される。
- Node移動でStage候補を判定できる。
- Stage削除時に内包Nodeが検出される。
- Lane × Stageの交差領域を計算できる。

## 14. 完了条件

- Stage描画Modelが定義されている。
- Stageの配置、幅、削除、Laneとの関係が明確である。
- 実装者がStage描画を実装できる。

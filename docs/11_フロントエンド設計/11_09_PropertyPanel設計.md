# 11_09_PropertyPanel設計

## 1. 目的

本書は、AI Flow DesignerのProperty Panel設計を定義する。

Property Panelは、選択中のLane / Stage / Node / Link / Commentなどの構造化情報を編集する領域である。
AI Flow Designerでは、見た目だけでなく、AIが工程・責務・条件・通信・補足を理解できる情報を入力する必要がある。

そのため、Property Panelは単なる図形プロパティ編集ではなく、SSOTの品質を高めるための中心UIとして設計する。

## 2. 基本方針

- selectionStoreの選択状態に応じて表示内容を切り替える。
- Node / Link / Comment / Lane / StageごとにForm Componentを分割する。
- Form入力はflowStoreのeditingFlowへ反映する。
- 保存APIはProperty Panelから直接呼ばない。
- Validation Errorは項目単位で表示する。
- AI専用メモを編集できる。

## 3. 表示切替

```text
selectionStore.selectedType
  ├─ none     → EmptyPropertyPanel
  ├─ flow     → FlowPropertyForm
  ├─ lane     → LanePropertyForm
  ├─ stage    → StagePropertyForm
  ├─ node     → NodePropertyForm
  ├─ link     → LinkPropertyForm
  └─ comment  → CommentPropertyForm
```

## 4. Component構成

```text
features/editor/components/PropertyPanel.vue
features/flow/components/FlowPropertyForm.vue
features/editor/components/LanePropertyForm.vue
features/editor/components/StagePropertyForm.vue
features/node/components/NodePropertyForm.vue
features/link/components/LinkPropertyForm.vue
features/comment/components/CommentPropertyForm.vue
features/comment/components/AiMemoEditor.vue
```

## 5. FlowPropertyForm

編集項目:

- Flow名
- 目的
- 説明
- Version表示
- Export対象設定
- AIレビュー対象設定

注意点:

- Flow名は必須。
- Flowの目的はAI DSLやPDF設計書出力へ利用する。

## 6. LanePropertyForm

編集項目:

- Lane名
- 責務
- 表示順
- 色
- 補足説明

重要:

Laneは「どこがその工程を担うか」をAIが理解するための主要情報である。
単なる背景帯として扱わない。

Validation:

- Lane名必須
- 同一Flow内のLane名重複警告
- 責務未入力警告

## 7. StagePropertyForm

編集項目:

- Stage名
- 工程目的
- 表示順
- 補足説明

Validation:

- Stage名必須
- 同一Flow内のStage名重複警告

## 8. NodePropertyForm

共通編集項目:

- Node名
- Node種別
- Lane
- Stage
- 説明
- 詳細Property
- AI専用メモ

Node種別別項目:

| Node種別 | 追加項目 |
| --- | --- |
| start | 開始条件 |
| end | 終了条件 |
| process | 処理内容、入力、出力 |
| decision | 判定条件、分岐基準 |
| hexagon | 外部処理・特殊処理内容 |
| balloon | 表示文言・補足 |
| image | 画像ID、代替テキスト |

Validation:

- Node名必須
- Lane参照必須
- Stage参照推奨
- decision Nodeは分岐条件必須
- image Nodeは画像Metadata必須

## 9. LinkPropertyForm

編集項目:

- Link名またはラベル
- 条件
- データ名
- 通信方式
- 補足説明
- 表示スタイル

重要:

Linkは単なる線ではない。
処理間の接続、条件、データ受け渡し、通信内容を表す。

Validation:

- sourceNode存在
- targetNode存在
- decision NodeからのLinkはcondition必須
- communicationTypeがある場合はdataName推奨

## 10. CommentPropertyForm

編集項目:

- コメント本文
- コメント種別
- 紐付け対象
- AIへ出力するか
- 表示位置

コメント種別:

- normal
- warning
- aiMemo
- reviewNote

## 11. AiMemoEditor

AI専用メモは、AIレビュー、AI DSL、設計書ドラフトに利用する。

編集対象:

- Node
- Link
- Flow
- Comment

方針:

- 通常コメントとAI専用メモを区別する。
- AI専用メモは画面上で目立ちすぎないが、Property Panelから編集できる。
- Export Optionで出力有無を制御できる。

## 12. 更新タイミング

初期方針:

- 入力変更時にeditingFlowへ即時反映する。
- 保存はHeaderの保存ボタンで行う。
- 一部重い入力はblur時反映でもよい。

```text
Input Change
  ↓
Property Form emits update
  ↓
flowStore.updateElementProperty
  ↓
flowStore.markDirty
  ↓
undoRedoStore.pushCommand
```

## 13. Error表示

Property Panelでは以下を表示する。

- 項目単位エラー
- 対象要素全体のエラー
- 保存失敗エラー
- Backend Validation Error

Backend Validation Errorはfieldパスを元に対象Formへ紐付ける。

## 14. 禁止事項

- Property Panelから直接API保存を呼ぶ。
- 全Node種別の項目を1つの巨大Componentへ直書きする。
- Linkをラベルだけの編集にする。
- Laneの責務入力を省略扱いにする。
- AI専用メモと通常コメントを混同する。

## 15. テスト観点

- 選択対象に応じてFormが切り替わる。
- Node名変更でeditingFlowが更新される。
- Link条件変更でeditingFlowが更新される。
- Validation Errorが項目単位で表示される。
- AI専用メモを編集できる。
- 保存APIがProperty Panel内に直接存在しない。

## 16. 完了条件

- 選択対象別Formが定義されている。
- Node / Link / Comment / Lane / Stageの編集項目が明確である。
- AI理解に必要な情報を入力できる。
- Store更新と保存APIの責務が分離されている。
- AIが本書を読んでProperty Panel実装に着手できる。

# 15_07_LaneStageテンプレート設計

## 1. 目的

本書は、TemplateにおけるLane / Stageの扱いを定義する。

Laneは責務、Stageは工程区切りを表すため、Template化することで業務フローの標準形を再利用できる。

## 2. 基本方針

- LaneとStageはTemplate保存対象にできる。
- Lane責務とStage目的を保持する。
- 並び順を保持する。
- Style情報を保持できる。
- 適用時は新しいIDを発行する。

## 3. Lane保存項目

- laneId
- laneName
- responsibility
- orderNo
- style

## 4. Stage保存項目

- stageId
- stageName
- purpose
- orderNo
- style

## 5. 適用時の扱い

Append時:

- 既存Lane / Stageへ追加する。
- orderNoを再計算する。

Replace時:

- 既存Lane / Stageを置き換える。
- Node所属もTemplate構造に従う。

## 6. Validation

- Lane名必須
- Stage名必須
- orderNo重複確認
- Node所属先のLane / Stage存在確認

## 7. テスト観点

- Lane責務をTemplate保存できる。
- Stage目的をTemplate保存できる。
- 適用時に並び順が維持される。
- Node所属先が正しく置き換わる。

## 8. 完了条件

- Lane / Stageの保存項目、適用方針、Validationが定義されている。

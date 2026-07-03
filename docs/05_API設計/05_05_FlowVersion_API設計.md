# 05_05_FlowVersion_API設計

## 1. 目的

FlowVersion APIの詳細仕様を定義する。
FlowVersionはLane、Stage、Node、Link、Commentを含むSnapshotである。

## 2. Version一覧取得

GET /api/flows/{flowId}/versions

対象FlowのVersion一覧を返す。
最新版フラグ、変更概要、作成者、作成日時を含める。

## 3. 最新Version取得

GET /api/flows/{flowId}/versions/latest

対象Flowの最新版FlowVersionを取得する。
フローエディタ初期表示で使用する。

## 4. Version詳細取得

GET /api/flow-versions/{flowVersionId}

FlowVersionDetailResponseを返す。
Lane、Stage、Node、Link、Commentを一括取得する。

## 5. Version作成

POST /api/flows/{flowId}/versions

既存最新版からSnapshotを複製し、新しいVersionを作成する。
versionNo、changeSummary、changeReasonを受け取る。

## 6. Version保存

PUT /api/flow-versions/{flowVersionId}

Lane、Stage、Node、Link、Commentを一括保存する。
保存時はトランザクションを使用する。

## 7. 保存時の整合性

- Nodeは存在するLaneとStageを参照する
- Linkは存在するfromNodeIdとtoNodeIdを参照する
- CommentのnodeIdはNULLまたは存在するNodeを参照する
- 同一FlowVersion内のID参照であることを確認する

## 8. Version複製

POST /api/flow-versions/{flowVersionId}/duplicate

指定Versionを複製し、別Versionとして保存する。
Lane、Stage、Node、Link、CommentのIDは再採番する。

## 9. エラー

- 400: 構造不正
- 404: FlowVersionが存在しない
- 409: Version番号重複
- 500: 保存失敗

## 10. テスト観点

- Version詳細から画面を復元できること
- 保存時にLink参照整合性を検証すること
- Version複製時にIDが再採番されること
- 保存失敗時に中途半端なデータが残らないこと

## 11. 完了条件

FlowVersion APIでフローエディタの構造データを取得・保存・複製できること。

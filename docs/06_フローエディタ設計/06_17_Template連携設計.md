# 06_17_Template連携設計

## 1. 本書の目的

フローエディタとTemplateの連携仕様を定義する。
TemplateはLane、Stage、Node、Link、Commentを含む構造化データの雛形である。

## 2. 適用方針

Template適用時は新しいFlowと初期FlowVersionを作成する。
既存FlowVersionへ直接上書き適用する処理は初期対象外とする。

## 3. ID再採番

Template内のLane、Stage、Node、Link、Commentは適用時に新規IDを採番する。
LinkのfromNodeId、toNodeId、CommentのnodeIdは新Node IDへ張り替える。

## 4. エディタ表示

Templateから作成後、生成されたFlowVersionをフローエディタで開く。

## 5. テスト観点

- TemplateからFlowを作成できること
- IDが再採番されること
- Link参照が張り替わること
- 生成後にエディタで開けること

## 6. 完了条件

Templateから整合したFlowVersionを作成し、エディタで編集開始できること。

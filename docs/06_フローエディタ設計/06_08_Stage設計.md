# 06_08_Stage設計

## 1. 本書の目的

Stageの編集仕様を定義する。
Stageは処理工程を表し、Nodeがどの工程で実行されるかを構造化する。

## 2. 保持情報

- stageId
- 工程名
- 説明
- 色
- 表示順

## 3. 代表例

- RFID読取り
- 搬送要求
- ラベル印刷
- 搬送完了
- 異常停止

## 4. 操作

- Stage追加
- Stage名称変更
- Stage並べ替え
- Stage削除
- 色変更

## 5. 削除仕様

StageにNodeが存在する場合、自動削除は禁止する。
以下から選択させる。

- Nodeも削除
- 別Stageへ移動
- キャンセル

## 6. Versionとの関係

StageはFlowVersion Snapshotの一部であり、Version間で共有しない。

## 7. Templateとの関係

Template適用時はStage IDを再採番する。

## 8. テスト観点

- Stageを追加できること
- Node存在時の削除選択が表示されること
- Version複製時にStageが複製されること

## 9. 完了条件

Stageが工程情報として構造化データに保存されること。

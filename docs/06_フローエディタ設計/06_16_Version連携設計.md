# 06_16_Version連携設計

## 1. 本書の目的

フローエディタとFlowVersionの連携仕様を定義する。
VersionはSnapshotであり、Undo/Redoとは責務を分離する。

## 2. 基本方針

エディタは1つのFlowVersionを編集対象として開く。
Version作成時は現在の構造を複製し、新しいSnapshotとして保存する。

## 3. 取得

エディタ起動時にFlowVersion APIからLane、Stage、Node、Link、Commentを一括取得する。

## 4. 保存

保存時は現在のFlowVersionへ構造全体を保存する。
Version番号や変更概要はVersion管理画面で扱う。

## 5. Version切替

未保存状態でVersionを切り替える場合は確認ダイアログを表示する。

## 6. テスト観点

- 最新Versionを開けること
- Version作成時にSnapshotが複製されること
- 未保存で切替時に確認されること

## 7. 完了条件

エディタがFlowVersion Snapshotと正しく連携すること。

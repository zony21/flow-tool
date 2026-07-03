# 06_06_PropertyPanel設計

## 1. 本書の目的

PropertyPanelの設計仕様を定義する。
PropertyPanelは選択中のLane、Stage、Node、Link、Commentの意味情報を編集する右側パネルである。

## 2. 表示切替

選択対象に応じて表示項目を切り替える。
未選択時はFlow概要または操作ガイドを表示する。

## 3. Node選択時

- 表示名
- NodeType
- Lane
- Stage
- 処理種別
- 説明
- AIメモ
- 座標
- サイズ

## 4. Link選択時

- FROM
- TO
- データ名
- 通信種別
- 条件
- 説明
- 接続線種別

## 5. Lane / Stage / Comment

Lane、Stage、Commentも同一パネルで編集する。
参照のみ権限ではreadonlyにする。

## 6. 未保存管理

入力変更後はEditor Storeをdirty状態にする。
保存失敗時もdirty状態を維持する。

## 7. テスト観点

- 選択対象ごとに項目が切り替わること
- 編集後に未保存状態になること
- 参照権限では編集できないこと

## 8. 完了条件

PropertyPanelで構造化データの意味情報を編集できること。

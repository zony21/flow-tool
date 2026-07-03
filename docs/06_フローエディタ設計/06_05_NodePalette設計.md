# 06_05_NodePalette設計

## 1. 本書の目的

NodePaletteの設計仕様を定義する。
NodePaletteはNodeTypeを選択し、CanvasへNodeを追加するための左側パネルである。

## 2. 初期NodeType

- 開始
- 終了
- 処理
- 判定
- 六角形
- 吹き出し
- 画像

## 3. 将来拡張

NodeTypeは固定ではなく拡張可能とする。
将来、API、PLC、DB、MQ、AI、通知、FTPなどを追加可能にする。

## 4. 操作

- 検索
- カテゴリ絞り込み
- Drag and Drop
- ダブルクリック追加

## 5. 権限制御

参照のみ権限ではNode追加を不可にする。

## 6. 状態管理

NodeType一覧、検索条件、選択中NodeTypeをPiniaで管理する。

## 7. テスト観点

- NodeTypeが表示されること
- Drag and DropでNodeを追加できること
- 追加後に未保存状態になること

## 8. 完了条件

NodePaletteから意味を持つNodeを追加できること。

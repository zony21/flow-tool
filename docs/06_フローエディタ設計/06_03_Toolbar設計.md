# 06_03_Toolbar設計

## 1. 本書の目的

Toolbarの操作仕様を定義する。
Toolbarはフローエディタの主要操作を集約する上部領域である。

## 2. 表示ボタン

- 戻る
- 保存
- Version作成
- Undo
- Redo
- Zoom In
- Zoom Out
- 表示リセット
- Mermaid出力
- PDF出力
- JSON出力
- AIレビュー

## 3. 保存状態表示

未保存変更がある場合、保存ボタンを強調表示する。
保存中はボタンを非活性にする。
保存失敗時は未保存状態を維持する。

## 4. 権限制御

参照のみ権限では保存、Version作成、Undo、Redo、編集系操作を非活性にする。
出力権限がない場合はExport系ボタンを非活性にする。

## 5. イベント

Toolbar操作はPinia storeのActionを呼び出す。
保存はFlowVersion API、出力はExport APIへ接続する。

## 6. エラー処理

保存失敗、出力失敗、権限不足、Session切れをToastまたはErrorDialogで表示する。

## 7. テスト観点

- 保存ボタンが未保存時に強調されること
- 保存中に二重実行できないこと
- Undo/Redoの活性状態が履歴と一致すること
- 権限不足時に操作できないこと

## 8. 完了条件

Toolbarから主要操作を一貫して実行できること。

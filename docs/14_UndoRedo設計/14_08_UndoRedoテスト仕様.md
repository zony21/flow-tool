# 14_08_UndoRedoテスト仕様

## 1. 本書の目的

本書は、AI Flow Designer における Undo / Redo のテスト仕様を定義する。

Undo / Redo は構造化データの復元機能であり、見た目だけでなく Node / Link / Lane / Stage / Comment の関係が正しく戻ることを検証する。

## 2. テスト分類

- Command単体テスト
- CompositeCommand単体テスト
- History管理テスト
- Store結合テスト
- 描画連携テスト
- AutoSave連携テスト
- E2Eテスト
- 性能テスト
- 回帰テスト

## 3. Command単体テスト

対象:

- AddNodeCommand
- MoveNodeCommand
- MoveNodesCommand
- ResizeNodeCommand
- UpdateNodePropertyCommand
- DeleteNodeCommand
- AddLinkCommand
- ReconnectLinkCommand
- UpdateLinkPropertyCommand
- DeleteLinkCommand
- AddCommentCommand
- UpdateCommentCommand
- DeleteCommentCommand

確認:

- executeで期待状態になる
- undoでbefore状態へ戻る
- redoでafter状態へ戻る
- dirty対象が更新される
- Validation対象が返る

## 4. Node系テスト

確認項目:

- Node追加をUndoできる
- Node追加をRedoできる
- Node移動をUndoできる
- 複数Node移動をUndoできる
- NodeリサイズをUndoできる
- Node名変更をUndoできる
- Node削除をUndoできる
- Node削除時に関連Linkも戻る

## 5. Link系テスト

確認項目:

- Link追加をUndoできる
- Link削除をUndoできる
- Link再接続をUndoできる
- Link条件変更をUndoできる
- Link通信種別変更をUndoできる
- 自己LinkをUndo/Redoできる
- 循環LinkをUndo/Redoできる

## 6. Lane系テスト

確認項目:

- Lane追加をUndoできる
- Lane名変更をUndoできる
- Lane並び替えをUndoできる
- Lane削除(Node移動)をUndoできる
- Lane削除(Node同時削除)をUndoできる
- Undo後にNodeのlaneIdが戻る
- Undo後に関連Linkが戻る

## 7. Stage系テスト

確認項目:

- Stage追加をUndoできる
- Stage名変更をUndoできる
- Stage並び替えをUndoできる
- Stage削除(Node移動)をUndoできる
- Stage削除(Node同時削除)をUndoできる
- Undo後にNodeのstageIdが戻る

## 8. Comment系テスト

確認項目:

- 独立Comment追加をUndoできる
- Node紐付けComment追加をUndoできる
- Comment移動をUndoできる
- Comment本文変更をUndoできる
- Comment削除をUndoできる
- Node削除時の紐付けComment復元が正しい

## 9. CompositeCommandテスト

確認項目:

- executeが順番通り実行される
- undoが逆順で実行される
- redoが順番通り実行される
- 途中失敗時に実行済みCommandがrollbackされる
- Composite全体が1履歴になる
- 子Commandが個別履歴にならない

## 10. Clipboardテスト

確認項目:

- コピーはUndo履歴に積まれない
- 貼付はUndo履歴に積まれる
- 貼付Undoで追加Node/Link/Commentが消える
- 貼付Redoで同じIDが復元される
- 貼付時に元IDを流用しない
- 選択範囲外Linkを貼付しない

## 11. History管理テスト

確認項目:

- Command実行でUndo Stackへ追加される
- UndoでRedo Stackへ移動する
- RedoでUndo Stackへ戻る
- 新規Command実行でRedo Stackが消える
- maxHistoryCount超過で古い履歴が削除される
- Flowごとに履歴が分離される

## 12. AutoSave連携テスト

確認項目:

- Command実行後にdirtyになる
- Undo後もdirtyになる
- Redo後もdirtyになる
- AutoSave成功後もUndo Stackは残る
- AutoSave中にUndoした場合の状態が破綻しない

## 13. Version連携テスト

確認項目:

- Version作成時にUndo履歴は含まれない
- Version復元後にUndo履歴がクリアされる
- 復元前状態はVersionとして保存される
- Version復元後にRedoできない

## 14. 編集ロック連携テスト

確認項目:

- ViewerではUndoできない
- ViewerではRedoできない
- 編集ロック未取得ではUndoできない
- 編集ロック解除後はUndo/Redo不可になる
- 自分がロック保持者の場合のみUndo/Redo可能

## 15. 描画連携テスト

確認項目:

- Undo後にNode表示位置が戻る
- Redo後にNode表示位置が進む
- Link再接続Undoで線が戻る
- Lane削除Undoで背景が戻る
- Stage削除Undoで背景が戻る
- Validation表示が更新される

## 16. E2Eテスト

シナリオ:

1. Node追加
2. Node移動
3. Link追加
4. Comment追加
5. Undoを4回実行
6. Redoを4回実行
7. 保存
8. 再読込
9. 構造が一致すること

## 17. 性能テスト

性能ケース:

- MoveNodeCommand 1000件
- Undo 1000回
- Redo 1000回
- 100Node貼付Undo
- 10000Link存在下でLink削除Undo
- Lane削除WithNodes Undo
- Template適用Undo

## 18. 回帰テスト

重点回帰:

- Undo後に孤立Linkが残らない
- Redo時にGUIDが変わらない
- Node削除UndoでCommentが戻る
- Lane削除UndoでNode所属が戻る
- Stage削除UndoでNode所属が戻る
- Undo後のAutoSaveで正しい状態が保存される

## 19. 禁止確認

以下が起きないことを確認する。

- mousemoveごとに履歴が増える
- コピーで履歴が増える
- UndoでAPI保存が走る
- Undo履歴がDB保存される
- Redoで新GUIDが採番される
- ViewerがUndoできる

## 20. 完了条件

- 主要Commandの単体テストがある
- CompositeCommandの順序テストがある
- History管理テストがある
- AutoSave / Version / 編集ロックとの連携が検証されている
- Undo 1000回性能テストが定義されている

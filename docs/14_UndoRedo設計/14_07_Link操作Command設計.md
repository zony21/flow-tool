# 14_07_Link操作Command設計

## 1. 目的

本書は、AI Flow DesignerのLink操作Command設計を定義する。

Link操作は、Node間の接続、条件、データ名、通信種別などFlowの意味に直結するため、Undo / Redoで正確に復元できる必要がある。

## 2. 対象Command

- AddLinkCommand
- DeleteLinkCommand
- UpdateLinkCommand
- ReconnectLinkCommand

## 3. AddLinkCommand

保持情報:

- link
- previousSelection

execute:

- Linkを追加する。
- 追加Linkを選択する。

undo:

- Linkを削除する。
- previousSelectionを復元する。

redo:

- Linkを再追加する。

## 4. DeleteLinkCommand

保持情報:

- deletedLink
- previousSelection

execute:

- Linkを削除状態にする。

undo:

- Linkを復元する。

redo:

- Linkを再削除する。

## 5. UpdateLinkCommand

保持情報:

- linkId
- before
- after

対象:

- label
- condition
- dataName
- communicationType
- linkType

## 6. ReconnectLinkCommand

Linkの接続先を変更する場合に使用する。

保持情報:

- linkId
- before sourceNodeId / targetNodeId
- after sourceNodeId / targetNodeId

接続先Nodeが存在することを事前に検証する。

## 7. Validation連携

Command実行後、対象LinkのValidation状態を再計算する。

例:

- decision NodeからのLinkにconditionがない。
- source / targetが存在しない。

## 8. 禁止事項

- Vue Flow EdgeをそのままCommandに保存する。
- source / targetの整合性を無視してRedoする。
- Link削除時に関連Commentを見失う。

## 9. テスト観点

- AddLinkをUndoで削除できる。
- DeleteLinkをUndoで復元できる。
- UpdateLinkでconditionを戻せる。
- ReconnectLinkで接続先をUndoできる。

## 10. 完了条件

- Link追加・削除・更新・再接続Commandが定義されている。
- FlowLinkの意味情報を保持してUndo / Redoできる。

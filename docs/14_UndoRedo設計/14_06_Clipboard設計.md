# 14_06_Clipboard設計

## 1. 本書の目的

本書は、AI Flow Designer におけるClipboard設計を定義する。

Clipboardは、Node / Link / Comment のコピー、貼付、複製、Template化の前段処理として利用する。貼付時には新しいGUIDを採番し、構造化データとして破綻しないように変換する。

## 2. 基本方針

- コピーはUndo/Redo対象外
- 貼付はUndo/Redo対象
- 複製はUndo/Redo対象
- Clipboardには構造化データのSnapshotを保持する
- 貼付時に必ずIDを再採番する
- 選択範囲外のLinkはコピーしない
- 画像ファイル本体はClipboardに保持しない
- imageFileIdは参照として保持する

## 3. Clipboard対象

対象:

- Node
- Link
- Comment

初期では対象外:

- Lane
- Stage
- Flow全体

ただし、TemplateではLane / Stageも対象にできる。

## 4. Clipboard形式

```ts
type EditorClipboardData = {
  sourceFlowId: string
  copiedAt: string
  nodes: ClipboardNode[]
  links: ClipboardLink[]
  comments: ClipboardComment[]
  bounds: ClipboardBounds
}
```

## 5. ClipboardNode

```ts
type ClipboardNode = {
  originalNodeId: string
  laneId: string
  stageId: string
  nodeTypeId: string
  displayName: string
  description?: string
  x: number
  y: number
  width: number
  height: number
  shape?: string
  extensionJson?: unknown
}
```

## 6. ClipboardLink

```ts
type ClipboardLink = {
  originalLinkId: string
  originalFromNodeId: string
  originalToNodeId: string
  fromAnchor?: string
  toAnchor?: string
  dataName?: string
  communicationType?: string
  condition?: string
  description?: string
  linkType: string
  controlPoints?: LinkControlPoint[]
}
```

## 7. ClipboardComment

```ts
type ClipboardComment = {
  originalCommentId: string
  originalNodeId?: string
  text: string
  x: number
  y: number
  width?: number
  height?: number
}
```

## 8. コピー対象Link

Linkは、fromNodeとtoNodeの両方がコピー対象Nodeに含まれる場合のみコピーする。

理由:

- 片側だけコピーすると貼付後に孤立Linkになる
- 別Flow貼付時に参照先が存在しない

## 9. コピー対象Comment

Commentは以下をコピーする。

- 選択された独立Comment
- コピー対象Nodeに紐付くComment

Node紐付けCommentは、貼付後に新Nodeへ紐付ける。

## 10. コピー処理

処理手順:

1. selectionStoreから選択対象取得
2. Node Snapshot作成
3. 対象Node同士のLink抽出
4. Comment抽出
5. bounds計算
6. clipboardStoreへ保存

## 11. bounds計算

貼付時の座標調整に利用する。

```ts
type ClipboardBounds = {
  minX: number
  minY: number
  maxX: number
  maxY: number
  width: number
  height: number
}
```

## 12. 貼付処理

貼付時手順:

1. Clipboard有無確認
2. 新GUID採番Map作成
3. Nodeを新IDで生成
4. Linkのfrom/toを新Node IDへ変換
5. CommentのnodeIdを新Node IDへ変換
6. 座標を貼付位置へ調整
7. PasteSelectionCommandを生成
8. Command実行
9. 貼付対象を選択

## 13. ID再採番

ID Map例:

```ts
type PasteIdMap = {
  nodeIdMap: Record<string, string>
  linkIdMap: Record<string, string>
  commentIdMap: Record<string, string>
}
```

originalIdを新IDへ変換する。

## 14. 座標調整

初期貼付位置は以下とする。

- マウス位置がある場合: マウス位置基準
- ない場合: 元座標 + 40px

複数貼付時は前回貼付位置からさらにずらす。

## 15. Lane / Stage扱い

初期ClipboardではLane / Stageはコピーしない。

貼付先Flowに同じlaneId / stageIdが存在しない場合:

- 現在選択中Lane / Stageへ割り当てる
- ない場合はデフォルトLane / Stageへ割り当てる
- それもない場合は貼付不可

## 16. 別Flow貼付

同一Project内の別Flow貼付は将来対応可能とする。

初期実装では同一Flow内貼付を優先する。

別Flow貼付時に必要な処理:

- laneId変換
- stageId変換
- nodeType存在確認
- imageFileId参照確認

## 17. 画像Node

画像Nodeの場合、extensionJsonのimageFileIdはそのまま参照する。

画像ファイル本体をClipboardに保持しない。

別Project貼付では画像参照権限の扱いを別途検討する。

## 18. Duplicate

DuplicateSelectionCommandは、コピーと貼付を一体で行う。

ユーザー操作:

- Alt + Drag
- 複製メニュー

DuplicateはUndo/Redo対象である。

## 19. Clipboard Store

```ts
type ClipboardStoreState = {
  data?: EditorClipboardData
  pasteCount: number
}
```

pasteCountは連続貼付時の座標ずらしに使う。

## 20. OS Clipboard

初期実装ではブラウザ内Clipboardを優先する。

将来的にOS ClipboardへJSONとして書き込むことを検討する。

## 21. 禁止事項

- コピーをUndo Stackへ積む
- 貼付時に元IDを流用する
- 選択範囲外Nodeを参照するLinkを貼付する
- 画像本体をBase64でClipboard保持する
- 存在しないLane / Stageへ貼付する

## 22. テスト観点

- Nodeをコピーできる
- 複数Nodeをコピーできる
- 選択範囲内Linkのみコピーされる
- Node紐付けCommentが新Nodeへ紐付く
- 貼付時にIDが再採番される
- 貼付時に座標がずれる
- 貼付をUndoできる
- 貼付をRedoできる
- 画像Node貼付でimageFileIdが保持される

## 23. 完了条件

- Clipboard形式が定義されている
- コピーと貼付の責務が分離されている
- 貼付がCommand化されている
- ID再採番により構造が破綻しない

# 13_17_Clipboard設計

## 1. 目的

本書は、AI Flow DesignerのClipboard設計を定義する。

Clipboardは、Node、Link、Commentをコピー、カット、ペースト、複製するための機能である。SSOTのID整合性を壊さないよう、貼り付け時にはID再採番と参照更新が必要である。

## 2. 基本方針

- Clipboard内容はFrontend一時状態として保持する。
- Paste時にIDを再採番する。
- Node間Linkもコピー対象にできる。
- 外部アプリとのClipboard連携は将来対応とする。
- Copy / PasteはUndo対象とする。

## 3. ClipboardModel

```ts
interface FlowClipboardData {
  nodes: FlowNode[];
  links: FlowLink[];
  comments: FlowComment[];
  copiedAt: string;
}
```

## 4. Copy

選択中要素をClipboardへ保存する。

対象:

- Node
- Link
- Comment
- 複数選択

Node複数選択時、選択Node間のLinkもコピー対象にする。

## 5. Cut

Copy後に対象要素を削除Commandとして扱う。

処理:

1. Clipboardへ保存。
2. DeleteElementCommand生成。
3. editingFlowから削除状態へ変更。

## 6. Paste

Paste時はIDを再採番する。

処理:

1. ClipboardData取得。
2. NodeId再採番。
3. LinkId再採番。
4. CommentId再採番。
5. Linkのsource / targetを新NodeIdへ変換。
6. 貼り付け位置をOffset。
7. AddElementsCommand生成。

## 7. Duplicate

DuplicateはCopy + Pasteを1操作として扱う。

Shortcut:

```text
Ctrl + D
```

## 8. 禁止事項

- コピー元IDをそのまま貼り付ける。
- Linkの参照先を再変換しない。
- 削除済要素をPasteする。
- Backend保存前にClipboard内容をSSOT扱いする。

## 9. テスト観点

- NodeをCopy / Pasteできる。
- Paste時にIDが再採番される。
- Node間Linkも正しく再接続される。
- DuplicateがUndo対象になる。

## 10. 完了条件

- Copy / Cut / Paste / Duplicateの方針が定義されている。
- ID再採番とLink参照更新が定義されている。
- 実装者がClipboard機能を実装できる。

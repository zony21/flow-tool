# 13_10_ZIndex設計

## 1. 目的

本書は、AI Flow DesignerのZIndex設計を定義する。

Canvas上では、Lane、Stage、Link、Node、Comment、Selection、Guide、Dialogなど複数レイヤが重なる。描画順が不安定だと、選択できない、Linkが隠れる、Nodeが見えないなどの問題が発生する。

## 2. 基本方針

- レイヤ順を固定定義する。
- 選択中要素は視認性を上げる。
- Drag中要素は一時的に前面表示する。
- ZIndexは表示制御であり、SSOTの業務意味ではない。
- Node同士の重なり順は必要最小限だけ保存する。

## 3. レイヤ順

```text
0  Canvas Background
10 Grid
20 Lane / Stage Background
30 Link
40 Node
50 Comment
60 Selection Rectangle
70 Guide Line
80 Drag Preview
90 Context Menu
100 Dialog Overlay
```

## 4. Node ZIndex

通常Nodeは同じZIndexで描画する。

例外:

- selected Nodeは通常より前面
- dragging Nodeはさらに前面
- validation focus対象は前面

## 5. Link ZIndex

LinkはNodeより背面に表示する。

ただし選択中Linkは強調表示し、クリック判定用の透明Hit Areaを持つ。

## 6. Comment ZIndex

CommentはNodeより前面に表示する。
ただし、Node紐付けCommentは対象Nodeに近い位置に表示し、Node操作を邪魔しない。

## 7. Context Menu / Dialog

Context MenuとDialogはCanvas内レイヤではなくOverlayとして扱う。
Canvas変換やZoomの影響を受けない。

## 8. 禁止事項

- Componentごとに任意のz-indexを直書きする。
- DialogをCanvas内座標で表示する。
- LinkがNode前面に常時出る。
- ZIndexをSSOTの業務情報として扱う。

## 9. テスト観点

- NodeがLane / Stage背景より前面に表示される。
- LinkがNode背面に表示される。
- 選択中Nodeが前面に表示される。
- Drag中Nodeが最前面に表示される。
- Context MenuがCanvas Zoomの影響を受けない。

## 10. 完了条件

- Canvas内レイヤ順が定義されている。
- Node / Link / Comment / OverlayのZIndex方針が明確である。
- 実装者が重なり制御を実装できる。

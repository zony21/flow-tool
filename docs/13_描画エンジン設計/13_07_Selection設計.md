# 13_07_Selection設計

## 1. 目的

本書は、AI Flow DesignerのSelection設計を定義する。

Selectionは、Canvas上のLane / Stage / Node / Link / Commentを選択し、Property Panel、Toolbar、Keyboard操作、Undo / Redoへつなぐための基盤である。

## 2. 基本方針

- selectionStoreで選択状態を管理する。
- 単一選択と複数選択に対応する。
- 選択状態はSSOTへ保存しない。
- Property Panelは選択中要素に応じて表示を切り替える。
- 範囲選択は将来の大量編集を想定して設計する。

## 3. SelectionState

```ts
interface SelectionState {
  selectedType: 'none' | 'flow' | 'lane' | 'stage' | 'node' | 'link' | 'comment';
  selectedId?: string;
  selectedIds: string[];
  lastSelectedId?: string;
}
```

## 4. 単一選択

対象要素クリックで単一選択する。

処理:

1. Canvas要素Click。
2. selectionStore.selectElement。
3. Property Panel切替。
4. Canvas上でSelection Highlight表示。

## 5. 複数選択

CtrlまたはShiftを押しながら選択する。

対象:

- Node
- Link
- Comment

Lane / Stageの複数選択は初期では必須としない。

## 6. 範囲選択

Canvas空白からDragして矩形範囲を作る。

処理:

1. Pointer down on canvas background。
2. Selection rectangle表示。
3. IntersectするNode / Commentを抽出。
4. selectionStore.setMultiSelection。

## 7. Property Panel連携

単一選択:

- 対象要素のProperty Formを表示。

複数選択:

- 共通操作Panelを表示。
- 一括削除、一括移動、整列などを将来対応。

## 8. Keyboard連携

- Escape: 選択解除
- Delete: 削除確認
- Ctrl + A: Canvas内要素全選択
- Arrow: 将来Node微移動

## 9. Selection Highlight

選択中要素は視覚的に示す。

- Node: Border強調
- Link: 線強調
- Comment: Border強調
- Lane / Stage: Header強調

色だけに依存せず、BorderやBadgeも併用する。

## 10. 禁止事項

- selectionをSSOTへ保存する。
- selected状態をSaveRequestへ含める。
- Property Panel側で独自Selection状態を持つ。
- Vue Flow内部Selectionだけを正とする。

## 11. テスト観点

- NodeクリックでNode選択になる。
- LinkクリックでLink選択になる。
- Escapeで選択解除される。
- 複数選択でselectedIdsが更新される。
- Selection状態がSaveRequestへ混入しない。

## 12. 完了条件

- 単一選択、複数選択、範囲選択の方針が定義されている。
- Property Panel、Keyboard、Canvas Highlightとの連携が定義されている。
- 実装者がSelection機能を実装できる。

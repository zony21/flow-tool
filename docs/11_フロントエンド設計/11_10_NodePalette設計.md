# 11_10_NodePalette設計

## 1. 目的

本書は、AI Flow DesignerのNode Palette設計を定義する。

Node Paletteは、Flow Canvasへ追加できるNode種別をユーザーへ提示する領域である。
本システムではNodeが拡張可能であり、初期Nodeだけでなく、将来API、DB、PLC、通信、AIレビュー向けNodeを追加できる必要がある。

## 2. 基本方針

- 初期Nodeを明確に定義する。
- Node種別は拡張可能にする。
- Palette表示とNode定義を分離する。
- Drag & DropでCanvasへ追加する。
- 追加直後はProperty Panelで詳細入力を促す。
- Node定義にはAI理解に必要なメタ情報を持たせる。

## 3. 初期Node一覧

| Node種別 | 表示名 | 用途 |
| --- | --- | --- |
| start | 開始 | Flow開始点 |
| end | 終了 | Flow終了点 |
| process | 処理 | 通常処理 |
| decision | 判定 | 条件分岐 |
| hexagon | 六角形 | 外部処理・特殊処理 |
| balloon | 吹き出し | 補足・表示コメント |
| image | 画像 | 画面・図・参考画像 |

## 4. Nodeカテゴリ

初期カテゴリ:

- 基本
- 分岐
- 補足
- メディア

将来カテゴリ:

- API
- DB
- PLC
- 通信
- AI
- テンプレート

## 5. Node定義Model

```ts
export interface NodePaletteItem {
  nodeType: FlowNodeType;
  label: string;
  description: string;
  category: NodePaletteCategory;
  icon: string;
  defaultTitle: string;
  defaultProperties: Record<string, unknown>;
  requiredProperties: string[];
  aiDescription: string;
  enabled: boolean;
}
```

## 6. Palette表示

表示項目:

- アイコン
- Node名
- 短い説明
- カテゴリ

表示方式:

- 左Panelにカテゴリ別表示
- 折りたたみ可能
- 将来検索対応
- 将来お気に入り対応

## 7. Drag & Drop仕様

### 7.1 操作フロー

```text
User drag palette item
  ↓
Canvas hover
  ↓
Drop
  ↓
Create temporary Node
  ↓
flowStore.addNode
  ↓
selectionStore.selectNode
  ↓
PropertyPanel opens NodePropertyForm
```

### 7.2 Drop時設定

Drop時に設定するもの:

- nodeId
- nodeType
- title
- position
- size
- defaultProperties
- laneId候補
- stageId候補

laneId / stageIdはDrop位置から推定する。
ただし、最終的にはProperty Panelで変更可能にする。

## 8. Node追加Command

Node追加はUndo / Redo対象とする。

```text
AddNodeCommand
  ├─ node
  ├─ previousSelection
  └─ timestamp
```

追加後にUndoした場合、Nodeと関連する未保存Link / Commentも扱う必要がある。
初期では追加直後Node単体のUndoを対象とする。

## 9. 拡張Node方針

Nodeは拡張可能とする。

将来追加例:

| Node | 用途 |
| --- | --- |
| apiCall | API呼び出し |
| dbUpdate | DB更新 |
| plcSignal | PLC信号 |
| externalSystem | 外部システム連携 |
| aiCheck | AIレビュー観点 |

拡張Nodeは以下を持つ。

- nodeType
- 表示Component
- PropertyForm
- Validation Rule
- Mermaid変換
- PDF変換
- AI DSL変換

## 10. 検索・お気に入り

初期実装では必須ではない。

将来対応:

- Node名検索
- 説明検索
- カテゴリ絞り込み
- よく使うNodeの上部表示
- ProjectごとのPalette設定

## 11. 権限制御

将来、権限によりNode追加を制御できるようにする。

例:

- ViewerはNode追加不可
- Editorは基本Node追加可
- Ownerは拡張Node設定可

Frontendは表示制御を行うが、最終判定はBackendで行う。

## 12. 禁止事項

- Palette表示名だけでNode仕様を決める。
- Node追加時にProperty Panelを開かない。
- 拡張NodeをComponent直書きだけで増やす。
- Node定義にAI説明を持たせない。
- Drop位置だけでLane / Stageを確定し、変更不能にする。

## 13. テスト観点

- 初期NodeがPaletteに表示される。
- Drag & DropでNodeが追加される。
- 追加直後にNodeが選択される。
- Property PanelがNode編集表示に切り替わる。
- Undoで追加Nodeを戻せる。
- 拡張Node定義を追加してPaletteに表示できる。

## 14. 完了条件

- 初期Node一覧が定義されている。
- Node定義Modelが定義されている。
- Drag & Drop追加フローが明確である。
- 拡張Nodeに必要な情報が整理されている。
- AIが本書を読んでNode Palette実装に着手できる。

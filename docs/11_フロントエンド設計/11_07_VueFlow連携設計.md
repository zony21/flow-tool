# 11_07_VueFlow連携設計

## 1. 目的

本書は、AI Flow DesignerにおけるVue Flow連携設計を定義する。

Vue FlowはFlow Canvasの表示・操作に利用するが、Vue FlowのNode / EdgeはSSOTではない。
正となるのは、Backendで検証・保存されるProject / Flow / Lane / Stage / Node / Link / Commentの構造化データである。

## 2. 基本方針

- Vue Flowは表示・操作用ライブラリとして扱う。
- SSOT構造からVue Flow Node / Edgeを生成する。
- Vue Flow操作結果は編集Commandへ変換してStoreへ反映する。
- Vue Flow内部状態をそのままDB保存しない。
- Linkの初期表示はBezierとする。
- Loopは許可する。

## 3. 変換全体像

```text
Flow Structure
  ↓ flowToVueFlowAdapter
Vue Flow Nodes / Edges
  ↓ User Operation
Vue Flow Event
  ↓ vueFlowToCommandAdapter
Editing Command
  ↓ flowStore update
Editing Flow Structure
```

## 4. Vue Flow Node設計

### 4.1 表示Node型

Vue Flow Nodeは表示用Modelとして扱う。

```ts
interface FlowCanvasNodeData {
  flowNodeId: string;
  nodeType: FlowNodeType;
  title: string;
  laneId?: string;
  stageId?: string;
  hasError: boolean;
  aiNoteExists: boolean;
}
```

### 4.2 SSOT Nodeとの関係

SSOT Node:

- nodeId
- laneId
- stageId
- nodeType
- title
- description
- properties
- position
- size
- aiNotes

Vue Flow Node:

- id
- type
- position
- data
- selected
- draggable

Vue Flow NodeのidはSSOTのnodeIdと一致させる。
ただし、Vue Flow固有のselectedやdragging状態はSSOTへ保存しない。

## 5. Vue Flow Edge設計

### 5.1 表示Edge型

```ts
interface FlowCanvasEdgeData {
  linkId: string;
  label?: string;
  condition?: string;
  dataName?: string;
  communicationType?: string;
  hasError: boolean;
}
```

### 5.2 Linkとの関係

SSOT Link:

- linkId
- sourceNodeId
- targetNodeId
- label
- condition
- dataName
- communicationType
- style

Vue Flow Edge:

- id
- source
- target
- label
- type
- data

初期Edge typeはBezierとする。
将来、Straight / Step / SmoothStepへ切り替え可能な設計にする。

## 6. Adapter設計

### 6.1 flowToVueFlowAdapter

責務:

- Flow構造からVue Flow Nodesを生成する。
- Flow構造からVue Flow Edgesを生成する。
- Node種別に応じた表示typeを設定する。
- Link styleに応じたEdge typeを設定する。
- Validation Error状態をdataへ設定する。

### 6.2 vueFlowToCommandAdapter

責務:

- Node移動EventをMoveNodeCommandへ変換する。
- Edge接続EventをAddLinkCommandへ変換する。
- Selection変更EventをselectionStoreへ反映する。
- Node削除EventをDeleteNodeCommandへ変換する。

## 7. Node種別マッピング

| SSOT nodeType | Vue Flow type | Component |
| --- | --- | --- |
| start | startNode | StartNode.vue |
| end | endNode | EndNode.vue |
| process | processNode | ProcessNode.vue |
| decision | decisionNode | DecisionNode.vue |
| hexagon | hexagonNode | HexagonNode.vue |
| balloon | balloonNode | BalloonNode.vue |
| image | imageNode | ImageNode.vue |

## 8. Canvas操作

### 8.1 Node Drag

```text
Vue Flow nodeDragStop
  ↓
MoveNodeCommand
  ↓
flowStore.updateNodePosition
  ↓
undoRedoStore.pushCommand
```

Node positionはSSOTに保存する。
ただし、dragging中の一時座標は保存しない。

### 8.2 Link接続

```text
Vue Flow connect
  ↓
AddLinkCommand
  ↓
flowStore.addLink
  ↓
selectionStore.selectLink
  ↓
PropertyPanelで条件入力
```

判定NodeからのLinkはcondition入力を促す。

### 8.3 Selection

```text
Vue Flow selectionChange
  ↓
selectionStore.selectElement
  ↓
PropertyPanel切替
```

### 8.4 Zoom / Pan

Zoom / PanはeditorStoreで管理する。
SSOTへ保存しない。

## 9. Lane / Stage表示

Lane / StageはVue Flow Nodeとして扱う方法とCanvas背景として扱う方法がある。

初期方針:

- Lane / StageはCanvas背景レイヤとして表示する。
- NodeはlaneId / stageIdを保持する。
- Node移動時に座標からLane / Stage候補を判定する。
- 最終的なlaneId / stageIdはProperty Panelでも変更可能にする。

## 10. Loop対応

Loopは許可する。

対応方針:

- sourceNodeIdとtargetNodeIdが循環関係になっても保存禁止にしない。
- 自己参照Linkは初期では警告扱い、禁止にはしない。
- Mermaid / PDF / AI DSL出力側でLoopを表現する。

## 11. Validation表示

Validation ErrorはCanvas上でも表示する。

例:

- NodeにエラーBadgeを表示する。
- LinkにエラーStyleを適用する。
- Property Panelに詳細エラーを表示する。
- Status Barにエラー件数を表示する。

## 12. 禁止事項

- Vue Flow NodeをそのままDBへ保存する。
- Vue Flow EdgeをそのままLink Entityとして扱う。
- selected / dragging / hoveredなどの一時状態をSSOTへ入れる。
- Canvas DOMからPDFを直接生成する。
- Node位置だけでLane責務を確定し、Propertyを更新しない。

## 13. テスト観点

- Flow構造からVue Flow Nodes / Edgesを生成できる。
- Node移動EventでSSOT Node positionが更新される。
- Link接続EventでSSOT Linkが追加される。
- Loop接続が拒否されない。
- Selection変更でProperty Panelが切り替わる。
- Vue Flow固有状態が保存Requestに混入しない。

## 14. 完了条件

- SSOTとVue Flow表示モデルが分離されている。
- Adapter責務が明確である。
- Node / Link操作がCommandへ変換される。
- Bezier LinkとLoop許可方針が定義されている。
- AIが本書を読んでVue Flow連携実装に着手できる。

# 13_12_VueFlow連携設計

## 1. 目的

本書は、描画エンジンにおけるVue Flow連携設計を定義する。

Vue FlowはCanvas描画と基本操作を支援するライブラリである。ただし、Vue Flow内部ModelはSSOTではなく、保存やExportの正はFlow構造である。

## 2. 基本方針

- Vue Flowは表示・操作用として利用する。
- Flow構造からVue Flow nodes / edgesを生成する。
- Vue Flow eventをCommandへ変換する。
- Vue Flow依存をAdapter層へ閉じる。
- 将来ライブラリ変更に備え、Component直結を避ける。

## 3. Adapter一覧

- flowToVueFlowAdapter
- vueFlowToCommandAdapter
- drawingNodeToVueFlowNodeAdapter
- drawingLinkToVueFlowEdgeAdapter
- vueFlowSelectionAdapter

## 4. Node変換

SSOT FlowNodeからDrawingNodeを作り、Vue Flow Nodeへ変換する。

保存しない情報:

- selected
- dragging
- hovered
- internal dimensions

## 5. Edge変換

SSOT FlowLinkからDrawingLinkを作り、Vue Flow Edgeへ変換する。

Edge dataへ入れるもの:

- linkId
- label
- condition
- hasValidationError

## 6. Event変換

Vue Flow eventはCommandへ変換する。

| Event | Command |
| --- | --- |
| nodeDragStop | MoveNodeCommand |
| connect | AddLinkCommand |
| edgeClick | SelectLinkCommand |
| nodeClick | SelectNodeCommand |
| nodesDelete | DeleteElementCommand |

## 7. Vue Flow Component登録

NodeTypeごとのComponentをVue Flowへ登録する。

- startNode
- endNode
- processNode
- decisionNode
- hexagonNode
- balloonNode
- imageNode

拡張NodeはNodeRegistryから登録する。

## 8. 禁止事項

- Vue Flow NodeをStoreのSSOTとして保持する。
- Vue Flow EdgeをAPIへ送る。
- Component内で直接Backend保存する。
- Vue Flow固有EventをFeature全体へ散らす。

## 9. テスト観点

- FlowNodeからVue Flow Nodeへ変換できる。
- FlowLinkからVue Flow Edgeへ変換できる。
- nodeDragStopがMoveNodeCommandになる。
- connectがAddLinkCommandになる。
- Vue Flow固有状態がSaveRequestへ混入しない。

## 10. 完了条件

- Vue Flow利用範囲が定義されている。
- Adapter責務が明確である。
- EventからCommandへの変換が定義されている。
- 実装者がVue Flow連携を安全に実装できる。

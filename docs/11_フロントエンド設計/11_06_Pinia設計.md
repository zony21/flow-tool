# 11_06_Pinia設計

## 1. 目的

本書は、AI Flow DesignerのPinia Store設計を定義する。

Pinia Storeは、Frontendにおける編集中状態を管理する。
ただし、StoreはDBの正ではない。保存の正はBackendで検証・永続化されたSSOTである。

そのため、Storeは「ユーザーが編集している一時状態」と「Backend保存済み状態」の差分を安全に扱うための層として設計する。

## 2. 基本方針

- Storeは機能単位で分割する。
- 1つの巨大Storeに全状態を詰め込まない。
- API通信の詳細はStoreに直接書きすぎず、api層を経由する。
- SSOTとVue Flow表示モデルを混同しない。
- 保存成功後はBackend ResponseでStoreを再同期する。
- Undo / RedoはVersion Snapshotとは分離する。

## 3. Store一覧

```text
src/stores/
 ├─ authStore.ts
 ├─ projectStore.ts
 ├─ flowStore.ts
 ├─ editorStore.ts
 ├─ selectionStore.ts
 ├─ undoRedoStore.ts
 ├─ exportStore.ts
 └─ uiStore.ts
```

## 4. authStore

### 4.1 目的

ログイン状態とユーザー情報を管理する。

### 4.2 State

- currentUser
- isAuthenticated
- isLoading
- authError

### 4.3 Actions

- fetchCurrentUser
- loginWithGitHub
- logout
- clearAuthError

### 4.4 注意点

Frontendの認証状態は表示制御用である。
API実行時の最終認証・権限判定はBackendで行う。

## 5. projectStore

### 5.1 目的

Project一覧、選択中Project、Project作成・更新状態を管理する。

### 5.2 State

- projects
- selectedProjectId
- selectedProject
- isLoading
- error

### 5.3 Actions

- fetchProjects
- selectProject
- createProject
- updateProject
- clearProjectState

### 5.4 Getter

- selectedProject
- editableProjects
- viewableProjects

## 6. flowStore

### 6.1 目的

Flow一覧、選択中Flow、編集中Flow構造、保存状態を管理する。

### 6.2 State

- flows
- selectedFlowId
- savedFlow
- editingFlow
- isDirty
- isSaving
- saveError
- lastSavedAt

### 6.3 Actions

- fetchFlows
- fetchFlow
- createFlow
- selectFlow
- updateEditingFlow
- saveFlow
- resetEditingFlow
- markDirty
- clearSaveError

### 6.4 保存方針

保存時は以下の流れとする。

```text
editingFlow
  ↓ flowToSaveRequestAdapter
SaveFlowRequest
  ↓ flowApi.saveFlow
Backend Validation / Save
  ↓ Response
apiResponseToFlowAdapter
  ↓
savedFlow / editingFlow 再同期
```

保存成功後は、Frontendで保持していた一時IDや更新日時をBackend Responseで置き換える。

## 7. editorStore

### 7.1 目的

Canvas表示・編集補助状態を管理する。

### 7.2 State

- zoom
- viewport
- gridVisible
- snapEnabled
- snapSize
- canvasMode
- isDragging
- isConnecting
- hoveredElementId

### 7.3 Actions

- setZoom
- setViewport
- toggleGrid
- toggleSnap
- setCanvasMode
- setDragging
- setConnecting

### 7.4 注意点

editorStoreの状態は表示・操作補助であり、SSOTではない。
ただし、Node positionなどFlow構造に保存すべき情報はflowStoreへ反映する。

## 8. selectionStore

### 8.1 目的

現在選択中の要素を管理する。

### 8.2 State

- selectedType
- selectedId
- multiSelectedIds

### 8.3 selectedType

- project
- flow
- lane
- stage
- node
- link
- comment
- none

### 8.4 Actions

- selectElement
- selectNode
- selectLink
- selectComment
- clearSelection
- setMultiSelection

### 8.5 用途

Property PanelはselectionStoreを参照し、選択対象に応じたFormを表示する。

## 9. undoRedoStore

### 9.1 目的

Frontendの一時操作履歴を管理する。

### 9.2 State

- undoStack
- redoStack
- maxHistory

### 9.3 Actions

- pushCommand
- undo
- redo
- clearHistory

### 9.4 Command例

- AddNodeCommand
- MoveNodeCommand
- DeleteNodeCommand
- AddLinkCommand
- DeleteLinkCommand
- UpdatePropertyCommand
- AddCommentCommand

### 9.5 Version Snapshotとの違い

Undo / Redoは編集中の一時操作履歴である。
Version Snapshotは保存済みFlowの正式履歴でありBackendで作成する。

## 10. exportStore

### 10.1 目的

Export UIの状態を管理する。

### 10.2 State

- exportDialogVisible
- exportType
- exportOptions
- isExporting
- exportError
- exportResult

### 10.3 Actions

- openExportDialog
- closeExportDialog
- setExportType
- updateExportOptions
- executeExport
- clearExportResult

### 10.4 注意点

Export対象は保存済みFlowまたはSnapshotである。
未保存変更がある場合は警告する。

## 11. uiStore

### 11.1 目的

画面全体のUI状態を管理する。

### 11.2 State

- leftPanelCollapsed
- rightPanelCollapsed
- activeDialog
- toastMessages
- globalLoading
- theme

## 12. Store間依存ルール

- Store同士の直接依存は最小限にする。
- 複数Storeをまたぐ処理はComposableまたはUseCase関数に切り出す。
- flowStoreがeditorStoreの表示補助状態に依存しない。
- selectionStoreは選択IDのみを持ち、実体取得はGetterまたはComposableで行う。

## 13. テスト観点

- Flow取得後にeditingFlowが作成される。
- 保存成功後にisDirtyがfalseになる。
- 保存失敗時にeditingFlowが失われない。
- Undo実行で直前操作が戻る。
- Redo実行で戻した操作が再適用される。
- selection変更に応じてProperty Panel表示対象が変わる。

## 14. 完了条件

- Storeごとの責務が明確である。
- 編集状態と保存済み状態が分離されている。
- Undo / RedoとVersion Snapshotが分離されている。
- StoreがVue Flow表示モデルをSSOTとして扱わない。
- AIが本書を読んでPinia Storeを実装できる。

# 11_05_Component設計

## 1. 目的

本書は、AI Flow DesignerのFrontend Component設計を定義する。

Componentは画面を構成する最小単位である。
ただし、ComponentへAPI通信、Store更新、Vue Flow変換、Validation、Export処理をすべて詰め込むと、保守性が低下し、SSOTとの境界も崩れる。

そのため、Componentの責務を明確化し、画面構成と実装方針を定義する。

## 2. Component設計原則

- Componentは表示とユーザー操作受付を中心にする。
- API通信はapi層へ寄せる。
- 状態管理はPinia Storeへ寄せる。
- SSOTとVue Flowの変換はAdapterへ寄せる。
- Componentは大きくしすぎない。
- props / emitsの型を明確にする。
- Node / Link / Commentなどの業務意味は型定義で表す。

## 3. Component分類

```text
components/
 ├─ common/
 ├─ form/
 ├─ dialog/
 ├─ feedback/
 └─ icon/

features/
 ├─ editor/components/
 ├─ node/components/
 ├─ link/components/
 ├─ comment/components/
 ├─ project/components/
 ├─ flow/components/
 └─ export/components/
```

## 4. common Component

### 4.1 目的

複数機能で使い回す汎用UIを配置する。

### 4.2 例

- AppButton
- AppIconButton
- AppPanel
- AppSplitter
- AppEmptyState
- AppLoading
- AppDropdown

### 4.3 禁止事項

- Flow固有ロジックを持たない。
- Storeへ直接依存しない。
- API通信しない。

## 5. form Component

### 5.1 目的

入力部品を統一する。

### 5.2 例

- FormTextInput
- FormTextarea
- FormSelect
- FormCheckbox
- FormNumberInput
- FormColorPicker
- FormValidationMessage

### 5.3 方針

PrimeVueを直接使うのではなく、必要に応じて薄いWrapperを作る。

理由:

- Validation表示を統一するため。
- 将来UI部品を変更しても影響を小さくするため。
- ラベル、必須表示、説明文、エラー表示の形式を統一するため。

## 6. dialog Component

### 6.1 目的

確認、入力、Export、Template適用などのDialogを統一する。

### 6.2 例

- ConfirmDialog
- UnsavedChangesDialog
- ExportDialog
- SnapshotCreateDialog
- TemplateApplyDialog
- DeleteConfirmDialog

### 6.3 方針

Dialogは以下を明確にする。

- 表示条件
- 入力項目
- OK時処理
- Cancel時処理
- Loading状態
- Error表示

## 7. feedback Component

### 7.1 目的

状態表示、通知、エラー表示を統一する。

### 7.2 例

- SaveStatusBadge
- ValidationSummary
- ToastMessage
- ErrorMessageBox
- SyncStatusIndicator

### 7.3 方針

Validation Errorは項目単位表示と全体表示を分ける。

例:

- Property Panel内: Node名未入力
- Status Bar: エラー2件
- Dialog: 保存失敗

## 8. editor Component

### 8.1 目的

Flow Editor画面の中核Componentを配置する。

### 8.2 例

```text
features/editor/components/
 ├─ EditorHeader.vue
 ├─ EditorToolbar.vue
 ├─ EditorMain.vue
 ├─ NodePalette.vue
 ├─ FlowCanvas.vue
 ├─ PropertyPanel.vue
 ├─ StatusBar.vue
 └─ CanvasMiniMap.vue
```

### 8.3 責務

| Component | 責務 |
| --- | --- |
| EditorHeader | Project / Flow / Save / Export / AIレビュー |
| EditorToolbar | Undo / Redo / Zoom / Grid / Snap / Template |
| NodePalette | 追加可能Node表示 |
| FlowCanvas | Vue Flow表示・Canvas操作 |
| PropertyPanel | 選択要素の詳細編集 |
| StatusBar | 保存状態・エラー・選択情報表示 |

## 9. node Component

### 9.1 目的

Node種別ごとの表示とProperty編集を管理する。

### 9.2 例

```text
features/node/components/
 ├─ BaseFlowNode.vue
 ├─ StartNode.vue
 ├─ EndNode.vue
 ├─ ProcessNode.vue
 ├─ DecisionNode.vue
 ├─ HexagonNode.vue
 ├─ BalloonNode.vue
 ├─ ImageNode.vue
 └─ NodePropertyForm.vue
```

### 9.3 方針

- Node表示Componentは見た目を担当する。
- NodePropertyFormは構造化情報の編集を担当する。
- Node種別ごとの必須Propertyは型とValidationで定義する。
- Node表示Component内で保存APIを呼ばない。

## 10. link Component

### 10.1 目的

Link表示とLink Property編集を管理する。

### 10.2 例

- LinkPropertyForm
- LinkLabelEditor
- LinkConditionEditor

### 10.3 編集項目

- ラベル
- 条件
- データ名
- 通信方式
- 補足説明

Linkは単なる線ではなく、処理の接続、条件、データ受け渡しを表す。

## 11. comment Component

### 11.1 目的

独立Comment、Node紐付けComment、AI専用メモを管理する。

### 11.2 例

- CommentBox
- CommentPropertyForm
- AiMemoEditor

### 11.3 方針

- Commentは表示注釈だけでなく、AI理解補助情報として扱う。
- aiVisibleを持ち、AI出力へ含めるか制御できる。
- Node紐付けと独立配置の両方を扱う。

## 12. project / flow Component

### 12.1 Project Component

例:

- ProjectList
- ProjectCard
- ProjectCreateDialog
- ProjectHeader

### 12.2 Flow Component

例:

- FlowList
- FlowCard
- FlowCreateDialog
- FlowSelector

## 13. export Component

### 13.1 目的

Export UIを管理する。

### 13.2 例

- ExportDialog
- ExportTypeSelector
- ExportOptionForm
- ExportResultPanel

### 13.3 方針

- Export実行はBackend APIへ要求する。
- Export対象は保存済みFlowまたはSnapshotとする。
- 未保存変更がある場合は警告する。

## 14. props / emits方針

Componentはprops / emitsを明確にする。

例:

```ts
interface NodePropertyFormProps {
  node: FlowNode;
  validationErrors: ValidationError[];
}

interface NodePropertyFormEmits {
  update: [node: FlowNode];
}
```

## 15. 禁止事項

- ComponentからBackend APIを直接呼ぶ。
- Component内に巨大な変換処理を書く。
- Vue Flow NodeをSSOT型として扱う。
- Property Panelに全Node種別の処理を直書きする。
- Dialogごとにバラバラのエラー表示を実装する。

## 16. 完了条件

- 共通Componentと機能Componentの責務が分離されている。
- Editor主要Componentの責務が明確である。
- Node / Link / CommentのComponent方針が明確である。
- API通信、Store更新、Adapter変換がComponentへ混入しない設計である。
- AIが本書を読んでComponent単位の実装に着手できる。

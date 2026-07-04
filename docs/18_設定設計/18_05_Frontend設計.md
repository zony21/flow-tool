# 18_05_Frontend設計

## 1. 目的

設定機能のFrontend設計を定義する。

Vue3、TypeScript、PrimeVue、Piniaを前提に、設定画面、Store、API連携、Validation、権限制御を整理する。

## 2. 基本方針

- 設定分類ごとにPanelを分ける
- Pinia Storeで設定状態を管理する
- Backend APIから取得した値を正とする
- 未保存変更を検知する
- 保存前にFrontend Validationを行う
- Backend 403時は権限情報を再取得する

## 3. 画面構成

設定画面は以下の構成とする。

```text
SettingsPage
├─ SettingsSideMenu
├─ UserSettingsPanel
├─ ProjectSettingsPanel
├─ EditorSettingsPanel
├─ AiSettingsPanel
└─ ExportSettingsPanel
```

Project配下で開く設定画面では、Project Setting / Editor Setting / AI Setting / Export Settingを表示する。

User設定はGlobal HeaderまたはUser Menuから開く。

## 4. Component一覧

| Component | 目的 |
| --- | --- |
| SettingsPage.vue | 設定画面全体 |
| SettingsSideMenu.vue | 設定分類切替 |
| UserSettingsPanel.vue | User Setting編集 |
| ProjectSettingsPanel.vue | Project Setting編集 |
| EditorSettingsPanel.vue | Editor Setting編集 |
| AiSettingsPanel.vue | AI Setting編集 |
| ExportSettingsPanel.vue | Export Setting編集 |
| SettingActionBar.vue | 保存 / 破棄 / 初期値に戻す |

## 5. Pinia Store

### 5.1 UserSettingStore

```ts
interface UserSettingState {
  theme: 'light' | 'dark' | 'system'
  language: 'ja' | 'en'
  defaultZoom: number
  density: 'compact' | 'normal' | 'comfortable'
  showMiniMap: boolean
  showGrid: boolean
  autoSave: boolean
  isDirty: boolean
  isLoading: boolean
}
```

### 5.2 ProjectSettingStore

```ts
interface ProjectSettingState {
  projectNameRule: string
  flowNameRule: string
  defaultExportFormat: 'pdf' | 'mermaid' | 'json' | 'ai_dsl'
  allowViewerExport: boolean
  versionRequiredOnExport: boolean
  archiveLocked: boolean
  isDirty: boolean
  isLoading: boolean
}
```

### 5.3 EditorSettingStore

```ts
interface EditorSettingState {
  gridEnabled: boolean
  snapEnabled: boolean
  gridSize: number
  defaultLinkType: 'bezier' | 'straight' | 'step' | 'smoothstep'
  showLaneHeader: boolean
  showStageHeader: boolean
  nodeLabelVisible: boolean
  canvasBackground: 'default' | 'dot' | 'plain'
  isDirty: boolean
  isLoading: boolean
}
```

### 5.4 AiSettingStore

```ts
interface AiSettingState {
  aiReviewEnabled: boolean
  reviewStrictness: 'low' | 'normal' | 'high'
  detectMissingProcess: boolean
  detectResponsibilityIssue: boolean
  detectDbImpact: boolean
  detectApiImpact: boolean
  dslDetailLevel: 'simple' | 'standard' | 'detailed'
  includeMetadataInDsl: boolean
  isDirty: boolean
  isLoading: boolean
}
```

### 5.5 ExportSettingStore

```ts
interface ExportSettingState {
  pdfPageSize: 'A4' | 'A3'
  pdfOrientation: 'portrait' | 'landscape'
  includeComments: boolean
  includeImages: boolean
  includeMetadata: boolean
  mermaidDirection: 'LR' | 'TB'
  jsonPrettyPrint: boolean
  aiDslFormatVersion: 'v1'
  isDirty: boolean
  isLoading: boolean
}
```

## 6. 権限制御

設定画面では17章のProject Permission Storeを利用する。

| Panel | 表示条件 | 保存条件 |
| --- | --- | --- |
| UserSettingsPanel | ログイン済み | Setting.UserUpdate |
| ProjectSettingsPanel | Project.Read | Setting.ProjectUpdate |
| EditorSettingsPanel | Flow.Read | Setting.UserUpdate |
| AiSettingsPanel | Project.Read | Setting.AiUpdate |
| ExportSettingsPanel | Project.Read | Setting.ExportUpdate |

保存権限がない場合、入力項目はreadonlyまたはdisabledにする。

## 7. 未保存変更制御

各Storeは初期取得値と現在値を比較してisDirtyを管理する。

画面遷移時にisDirty=trueの場合、確認Dialogを表示する。

表示文言:

```text
未保存の設定があります。変更を破棄して移動しますか？
```

## 8. Frontend Validation

Frontendでは以下を確認する。

- 必須項目
- 選択肢
- 数値範囲
- 依存関係

例:

- defaultZoomは0.2〜3.0
- gridSizeは4〜64
- pdfPageSizeはA4 / A3
- reviewStrictnessはlow / normal / high

## 9. API連携

各Storeは以下のActionを持つ。

```ts
fetch()
save()
reset()
restoreDefault()
```

restoreDefaultは初期値に戻すだけで、保存はsave実行時に行う。

## 10. 403時の処理

設定保存APIで403が返った場合:

1. Project Permissionを再取得する
2. 対象Panelをreadonlyに切り替える
3. エラーメッセージを表示する
4. 入力中の値は破棄しない

## 11. 完了条件

- 設定画面構成が明確である
- Store構成が定義されている
- 権限制御が定義されている
- 未保存変更制御が定義されている
- API連携とValidationが実装可能な粒度である

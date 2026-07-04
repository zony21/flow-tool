# 20_11_Phase9_Settings

## 1. 目的

18_設定設計に基づき、User / Project / Editor / AI / Exportの設定機能を実装する。

## 2. Task一覧

| Task ID | Task Name | 優先度 |
| --- | --- | --- |
| P9-001 | Setting API実装 | A |
| P9-002 | Setting Default実装 | A |
| P9-003 | Setting Validation実装 | A |
| P9-004 | Setting Store実装 | A |
| P9-005 | Settings UI実装 | B |
| P9-006 | Export/Editor反映 | B |

## 3. P9-001 Setting API実装

目的:

設定分類ごとの取得・更新APIを実装する。

実装内容:

- User Setting API
- Project Setting API
- Editor Setting API
- AI Setting API
- Export Setting API
- Upsert処理

関連設計:

- 18_04_API設計

テスト観点:

- 19_03 Settings API

完了条件:

- 各設定を取得・更新できる

## 4. P9-002 Setting Default実装

目的:

設定未登録時にDefault値を返す。

実装内容:

- SettingDefaultService
- User/Project/Editor/AI/Export Default

関連設計:

- 18_06_Backend設計

テスト観点:

- 未登録時Default値が返る

完了条件:

- 設定未登録でも画面が動作する

## 5. P9-003 Setting Validation実装

目的:

不正な設定値を保存しない。

実装内容:

- theme
- defaultZoom
- gridSize
- linkType
- reviewStrictness
- pdfPageSize
- mermaidDirection

関連設計:

- 18_設定設計

テスト観点:

- 不正値は400

完了条件:

- 設定ValidationがBackendで動作する

## 6. P9-004 Setting Store実装

目的:

Frontendで設定状態を管理する。

実装内容:

- UserSettingStore
- ProjectSettingStore
- EditorSettingStore
- AiSettingStore
- ExportSettingStore
- isDirty
- fetch/save/reset

関連設計:

- 18_05_Frontend設計

テスト観点:

- Store更新、dirty制御

完了条件:

- 設定画面からStoreを利用できる

## 7. P9-005 Settings UI実装

目的:

設定画面を実装する。

実装内容:

- SettingsPage
- UserSettingsPanel
- ProjectSettingsPanel
- EditorSettingsPanel
- AiSettingsPanel
- ExportSettingsPanel
- 保存/破棄/初期値に戻す

関連設計:

- 18_05_Frontend設計

テスト観点:

- Owner/Editor/Viewer別のreadonly制御

完了条件:

- 設定画面で各設定を編集できる

## 8. P9-006 Export/Editor反映

目的:

設定値をEditorとExport動作へ反映する。

実装内容:

- Grid/Snap反映
- defaultLinkType反映
- Mermaid方向反映
- PDF設定反映
- AI DSL detailLevel反映

関連設計:

- 18_設定設計
- 07/08/09 Export設計

テスト観点:

- 設定変更後に出力やEditor挙動が変わる

完了条件:

- 設定値が実機能へ反映される

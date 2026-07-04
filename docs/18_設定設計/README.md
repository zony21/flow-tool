# 18_設定設計

## 1. 目的

AI Flow Designerの設定機能を定義する。

設定は、利用者個人、Project、Editor、AI、Exportの動作を制御するために利用する。

本章では、設定の分類、保存先、DB、API、Frontend、Backend、Validation、権限制御、Test観点を整理する。

## 2. 設計思想

設定はSSOTを壊さない範囲で、表示・編集体験・出力形式・AI支援動作を調整するための補助情報である。

Flow本体の正はProject / Flow / Lane / Stage / Node / Link / Comment / Image / Version / Metadataの構造化データであり、設定はその生成・表示・出力の振る舞いを制御する。

## 3. 設定分類

| 分類 | 対象 | 主な内容 |
| --- | --- | --- |
| User Setting | 利用者個人 | テーマ、言語、初期ズーム、表示密度 |
| Project Setting | Project全体 | 既定Export、Project権限、命名規則 |
| Editor Setting | Flow Editor | Grid、Snap、接続線初期値、Canvas表示 |
| AI Setting | AI Review / AI DSL | AIレビュー強度、DSL出力粒度、検出項目 |
| Export Setting | PDF / Mermaid / JSON / AI DSL | 出力形式、ページ設定、含める情報 |

## 4. 設計一覧

| ファイル | 内容 | 状態 |
| --- | --- | --- |
| 18_01_設定管理概要.md | 設定全体方針 | 詳細化済み |
| 18_02_設定分類設計.md | User / Project / Editor / AI / Export分類 | 詳細化済み |
| 18_03_DB設計.md | 設定DB設計 | 詳細化済み |
| 18_04_API設計.md | 設定API設計 | 詳細化済み |
| 18_05_Frontend設計.md | 設定画面・Store設計 | 詳細化済み |
| 18_06_Backend設計.md | 設定Service・Validation設計 | 詳細化済み |
| 18_07_権限制御.md | 17章との連携 | 詳細化済み |
| 18_08_設定設計まとめ.md | 18章まとめ | 詳細化済み |

## 5. 実装時の中心部品

Frontend:

- UserSettingStore
- ProjectSettingStore
- EditorSettingStore
- AiSettingStore
- ExportSettingStore
- SettingsPage.vue
- ProjectSettingsPanel.vue
- EditorSettingsPanel.vue
- AiSettingsPanel.vue
- ExportSettingsPanel.vue

Backend:

- UserSettingController
- ProjectSettingController
- SettingService
- SettingValidationService
- SettingDefaultService

DB:

- USER_SETTING
- PROJECT_SETTING
- EDITOR_SETTING
- AI_SETTING
- EXPORT_SETTING

## 6. 権限制御

設定変更は17章のPermissionと連携する。

| 設定 | 参照 | 更新 |
| --- | --- | --- |
| User Setting | ログイン済みUser | Setting.UserUpdate |
| Project Setting | Project.Read | Setting.ProjectUpdate |
| Editor Setting | Flow.Read | Setting.EditorUpdate または User個人設定 |
| AI Setting | Project.Read | Setting.AiUpdate |
| Export Setting | Project.Read | Setting.ExportUpdate |

初期実装では、User Settingは全Role更新可、Project / AI / ExportのProject共通設定はOwnerのみ更新可とする。

## 7. 状態

18章は詳細化済み。

## 8. 次工程

次工程は `19_テスト仕様` の詳細化。

19章では、以下を設計する。

- Unit Test
- API Test
- Frontend Test
- E2E Test
- AI DSL出力検証
- Role別権限テスト
- Export検証

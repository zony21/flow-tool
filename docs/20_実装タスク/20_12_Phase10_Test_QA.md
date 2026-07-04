# 20_12_Phase10_Test_QA

## 1. 目的

19_テスト仕様に基づき、実装後の品質確認を行う。

## 2. Task一覧

| Task ID | Task Name | 優先度 |
| --- | --- | --- |
| P10-001 | Unit Test整備 | A |
| P10-002 | API Test整備 | A |
| P10-003 | Frontend Test整備 | A |
| P10-004 | E2E Test整備 | A |
| P10-005 | Export / AI DSL Test整備 | A |
| P10-006 | docs整合性監査 | A |
| P10-007 | README更新 | A |

## 3. P10-001 Unit Test整備

目的:

Service / Store / UtilityのUnit Testを整備する。

関連設計:

- 19_02_Unitテスト仕様

完了条件:

- PermissionService、SettingService、ExportServiceの主要テストがある

## 4. P10-002 API Test整備

目的:

主要APIの正常系・異常系を確認する。

関連設計:

- 19_03_APIテスト仕様

完了条件:

- Project / Flow / Settings / Export / Permission APIのテストがある

## 5. P10-003 Frontend Test整備

目的:

Component、Store、権限制御のテストを整備する。

関連設計:

- 19_04_Frontendテスト仕様

完了条件:

- FlowCanvas、PropertyPanel、Settings、Permission Storeのテストがある

## 6. P10-004 E2E Test整備

目的:

主要ユーザー導線のE2E Testを整備する。

関連設計:

- 19_05_E2Eテスト仕様

完了条件:

- Project作成、Flow編集、保存、Export、Viewer readonlyを確認できる

## 7. P10-005 Export / AI DSL Test整備

目的:

ExportとAI DSLの品質を確認する。

関連設計:

- 19_07_Exportテスト仕様
- 19_08_AI_DSLテスト仕様

完了条件:

- JSON、Mermaid、AI DSLの必須出力が確認されている

## 8. P10-006 docs整合性監査

目的:

docs全体の用語、DB、API、Entity、Frontend Modelのズレを確認する。

確認対象:

- DBテーブル名
- Entity名
- API path
- Permission Code
- Setting名
- Export形式
- AI DSL用語

完了条件:

- READMEとdocs状態が一致している
- 重大な用語ズレがない

## 9. P10-007 README更新

目的:

READMEを最新状態に更新する。

実装内容:

- 起動手順
- 実装状態
- docs索引
- 開発ルール

完了条件:

- READMEから現在状態が判断できる

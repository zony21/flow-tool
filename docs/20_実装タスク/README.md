# 20_実装タスク

## 1. 目的

AI Flow Designerを実装するためのタスク分解を定義する。

本章は、ChatGPT、Codex、Claude Code、GitHub Copilot等が、1タスクずつ実装できる粒度で作業を進めるための基準である。

## 2. 基本方針

- mainを唯一の正とする
- 1タスクは小さく完了条件を明確にする
- 1タスクで複数領域を大きくまたがない
- 各タスクに関連設計とテスト観点を紐付ける
- 画面だけ、APIだけ、DBだけで終わらせず、必要に応じて接続タスクを分ける
- 実装前に不要ファイルを作らない

## 3. タスク記述フォーマット

各タスクは以下の形式で定義する。

```text
Task ID:
Task Name:
目的:
対象:
実装内容:
関連設計:
テスト観点:
完了条件:
注意点:
```

## 4. 実装Phase

| Phase | 内容 | 状態 |
| --- | --- | --- |
| Phase 0 | 開発基盤 | 詳細化済み |
| Phase 1 | Backend基盤 / DB | 詳細化済み |
| Phase 2 | Auth / Permission | 詳細化済み |
| Phase 3 | Project / Flow API | 詳細化済み |
| Phase 4 | Frontend基盤 | 詳細化済み |
| Phase 5 | Flow Editor | 詳細化済み |
| Phase 6 | Version / UndoRedo | 詳細化済み |
| Phase 7 | Template / Image | 詳細化済み |
| Phase 8 | Export / AI DSL | 詳細化済み |
| Phase 9 | Settings | 詳細化済み |
| Phase 10 | Test / QA / 仕上げ | 詳細化済み |

## 5. 設計一覧

| ファイル | 内容 |
| --- | --- |
| 20_01_実装方針.md | 実装全体方針 |
| 20_02_Phase0_開発基盤.md | Repository / Frontend / Backend初期構成 |
| 20_03_Phase1_Backend_DB.md | DB / EF Core / Entity |
| 20_04_Phase2_Auth_Permission.md | GitHub OAuth / 権限管理 |
| 20_05_Phase3_Project_Flow_API.md | Project / Flow API |
| 20_06_Phase4_Frontend基盤.md | Vue / Pinia / API Client |
| 20_07_Phase5_FlowEditor.md | Editor / Canvas / Node / Link |
| 20_08_Phase6_Version_UndoRedo.md | Version / UndoRedo |
| 20_09_Phase7_Template_Image.md | Template / Image |
| 20_10_Phase8_Export_AI_DSL.md | Export / AI DSL |
| 20_11_Phase9_Settings.md | Settings |
| 20_12_Phase10_Test_QA.md | Test / QA / 仕上げ |
| 20_13_実装完了条件.md | 全体完了条件 |

## 6. 実装順序

推奨順序:

1. Phase 0 開発基盤
2. Phase 1 Backend / DB
3. Phase 2 Auth / Permission
4. Phase 3 Project / Flow API
5. Phase 4 Frontend基盤
6. Phase 5 Flow Editor
7. Phase 6 Version / UndoRedo
8. Phase 7 Template / Image
9. Phase 8 Export / AI DSL
10. Phase 9 Settings
11. Phase 10 Test / QA

## 7. 完了条件

20章は詳細化済み。

次工程は全体監査、README確認、必要に応じた実装開始である。

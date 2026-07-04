# 19_テスト仕様

## 1. 目的

AI Flow Designerの品質を担保するためのテスト方針、対象、観点、完了条件を定義する。

本章は、実装後の確認だけではなく、AI/Codex/GitHub Copilot等が実装する際に、各タスクの完了判定として使える粒度を目指す。

## 2. 基本方針

- SSOTである構造化データを最優先で検証する
- 図の見た目だけではなく、保存データ・生成物・AI DSLを検証する
- Unit / API / Frontend / E2E / Export / AI DSLを分けて確認する
- Role別権限テストを必須とする
- 失敗時に原因を特定しやすいテスト名にする
- 20_実装タスクの完了条件へ接続できる粒度にする

## 3. テスト分類

| 分類 | 目的 |
| --- | --- |
| Unit Test | 関数・Service単位の検証 |
| API Test | Controller / Service / DB連携の検証 |
| Frontend Test | Component / Store / Validationの検証 |
| E2E Test | 画面操作から保存・出力までの検証 |
| Permission Test | Owner / Editor / Viewer別の操作可否検証 |
| Export Test | Mermaid / PDF / JSON / AI DSL出力検証 |
| AI DSL Test | AIが曖昧なく解析できるDSL品質検証 |
| Regression Test | 既存仕様の退行防止 |

## 4. 設計一覧

| ファイル | 内容 | 状態 |
| --- | --- | --- |
| 19_01_テスト全体方針.md | テスト全体方針 | 詳細化済み |
| 19_02_Unitテスト仕様.md | Unit Test | 詳細化済み |
| 19_03_APIテスト仕様.md | API Test | 詳細化済み |
| 19_04_Frontendテスト仕様.md | Frontend Test | 詳細化済み |
| 19_05_E2Eテスト仕様.md | E2E Test | 詳細化済み |
| 19_06_権限テスト仕様.md | Permission Test | 詳細化済み |
| 19_07_Exportテスト仕様.md | Export Test | 詳細化済み |
| 19_08_AI_DSLテスト仕様.md | AI DSL Test | 詳細化済み |
| 19_09_テストデータ設計.md | Test Data | 詳細化済み |
| 19_10_テスト完了条件.md | 完了条件 | 詳細化済み |

## 5. 優先テスト

初期実装で必ず行うテストは以下とする。

1. 構造化データ保存・取得
2. Node / Link / Lane / Stageの整合性
3. Undo / Redo
4. Version Snapshot
5. Role別権限
6. 設定Default値・更新
7. Mermaid出力
8. JSON出力
9. AI DSL出力
10. Export設定反映

## 6. 完了条件

19章は詳細化済み。

次工程は `20_実装タスク` の詳細化。

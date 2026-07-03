# 09_AI構造化データ設計

## 1. 本書の位置付け

本フォルダは、AI Flow DesignerのSSOTであるAI構造化データの正式設計書を管理する。
AI構造化データは、画面描画、Mermaid出力、PDF出力、JSON出力、AI DSL、将来のAPI仕様生成・DB更新一覧生成・コード生成の共通入力である。

## 2. 設計思想

図は結果であり、正ではない。
正となるのは Project / Flow / Lane / Stage / Node / Link / Comment を中心とする構造化データである。
すべての出力機能は、この構造化データから生成する。

## 3. 正式設計書一覧

| ファイル | 内容 |
| --- | --- |
| `09_01_AI構造化データ概要.md` | 全体概要 |
| `09_02_Project構造.md` | Project構造 |
| `09_03_Flow構造.md` | Flow構造 |
| `09_04_Lane構造.md` | Lane構造 |
| `09_05_Stage構造.md` | Stage構造 |
| `09_06_Node構造.md` | Node構造 |
| `09_07_Link構造.md` | Link構造 |
| `09_08_Comment構造.md` | Comment構造 |
| `09_09_Property設計.md` | 共通Property設計 |
| `09_10_型定義.md` | TypeScript / C# 型定義 |
| `09_11_JSON保存形式.md` | JSON保存形式 |
| `09_12_JSONSchema.md` | JSON Schema |
| `09_13_バリデーション.md` | 構造検証 |
| `09_14_VersionSnapshot.md` | Version Snapshot |
| `09_15_AI_DSL変換.md` | AI DSL変換 |
| `09_16_Import仕様.md` | Import仕様 |
| `09_17_Export仕様.md` | Export仕様 |
| `09_18_互換性設計.md` | 互換性・移行 |
| `09_19_テスト仕様.md` | テスト仕様 |
| `09_20_AI構造化データ設計まとめ.md` | 総まとめ |

## 4. 管理方針

`09_AI構造化データ設計` 配下には正式設計書のみを残す。
更新メモ、draft、temp、old、数行だけの旧設計書は正式設計書へ統合後に削除する。

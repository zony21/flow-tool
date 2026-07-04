# 21_Import設計

## 1. 目的

AI Flow DesignerのImport機能を定義する。

ExportだけでなくImportを持つことで、外部生成されたJSONやAI DSLからFlowを復元・生成できるようにする。

## 2. 基本方針

- ImportしてもSSOT構造を壊さない
- Import前に必ずValidationする
- 既存Projectへ追加する場合はID再採番する
- Import結果をPreviewしてから確定する
- AI DSL Importは将来のAI生成Flowの入口とする

## 3. 対象形式

| 形式 | 優先度 | 内容 |
| --- | --- | --- |
| JSON | A | AI Flow Designer保存互換形式 |
| AI DSL | A | AI理解形式からFlow生成 |
| Mermaid | B | Mermaid flowchartから簡易Flow生成 |

## 4. 設計一覧

| ファイル | 内容 |
| --- | --- |
| 21_01_Import概要.md | Import全体方針 |
| 21_02_JSON_Import仕様.md | JSON Import |
| 21_03_AI_DSL_Import仕様.md | AI DSL Import |
| 21_04_ImportValidation仕様.md | Import Validation |

## 5. 権限制御

ImportはFlow編集操作であるため、初期実装ではOwner / Editorのみ可能とする。

ViewerはImport不可。

使用Permission:

```text
Flow.Update
```

## 6. 次工程

21章は追加設計であり、実装PhaseではExport / AI DSL後に対応する。

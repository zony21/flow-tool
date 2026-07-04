# 21_01_Import概要

## 1. 目的

Import機能の全体方針を定義する。

Importは、外部ファイルやAI生成結果からAI Flow DesignerのSSOT構造化データを作成・更新する機能である。

## 2. 設計思想

Importは単なるファイル読込ではない。

外部データをProject / Flow / Lane / Stage / Node / Link / Comment / Image / Metadataへ変換し、AI Flow Designerの正規構造として扱える状態にする。

## 3. Import処理流れ

1. ファイル選択
2. 形式判定
3. Parse
4. Import Validation
5. Preview表示
6. ID再採番
7. Flow構造へ変換
8. 保存
9. AuditLog記録

## 4. Import Mode

| Mode | 内容 |
| --- | --- |
| createNewFlow | 新規Flowとして作成 |
| appendToFlow | 既存Flowへ追加 |
| replaceFlow | 既存Flowを置換 |

初期実装ではcreateNewFlowを優先する。

## 5. ID再採番

既存ProjectへImportする場合、外部IDをそのまま使用しない。

- Node ID再採番
- Link ID再採番
- Lane ID再採番
- Stage ID再採番
- Comment ID再採番
- Link source/target再解決

## 6. Error方針

Importできない重大不備はerrorとする。

修正候補として扱えるものはwarningとする。

例:

| 状態 | 扱い |
| --- | --- |
| Link先Nodeなし | error |
| Decision条件なし | warning |
| Lane未指定Node | warning |
| DSL Version未対応 | error |

## 7. 完了条件

- Import対象形式が定義されている
- Import処理流れが定義されている
- Import Modeが定義されている
- ID再採番方針が定義されている
- Validation方針が定義されている

# 22_AIレビュー設計

## 1. 目的

AI Review機能の設計を定義する。

AI Reviewは、Flow構造化データとAI DSLをもとに、処理抜け、責務違反、DB/API影響、通信漏れ、テスト不足を検出する機能である。

## 2. 基本方針

- AI Reviewの入力はSSOT構造化データとAI DSLとする
- AIの回答をそのまま正にしない
- Review結果はwarning/error/suggestionとして扱う
- confidenceを持たせる
- 指摘は対象Node/Link/Flowへ紐付ける

## 3. 設計一覧

| ファイル | 内容 |
| --- | --- |
| 22_01_AIレビュー概要.md | AI Review全体方針 |
| 22_02_AIレビュー入出力仕様.md | 入力・出力Schema |
| 22_03_AIレビューRule仕様.md | Review Rule |

## 4. 次工程

AI ReviewはExport / AI DSL実装後に着手する。

# 05_API設計

## 1. 本書の位置付け

本ファイルは、`05_API設計` 配下の正式設計書の索引である。
短文メモ、更新メモ、前版維持のみの内容は正式設計書として扱わない。

## 2. API設計の目的

AI Flow Designer のAPIは、Frontend、Backend、DB、出力処理を接続し、Single Source of Truthである構造化データを安全に扱うための境界である。

APIは以下を実現する。

- Projectを管理できること
- Flowを管理できること
- FlowVersionをSnapshotとして保存できること
- Lane、Stage、Node、Link、Commentを一括取得・保存できること
- TemplateからFlowを作成できること
- Mermaid、PDF、JSONを生成できること
- GitHub OAuthログインと権限制御へ対応できること

## 3. 正式設計書一覧

| ファイル | 内容 |
| --- | --- |
| `05_01_API設計方針.md` | API共通方針 |
| `05_02_RESTエンドポイント一覧.md` | RESTエンドポイント一覧 |
| `05_03_DTO設計.md` | Request/Response DTO設計 |
| `05_04_Project_Flow_API設計.md` | Project/Flow API |
| `05_05_FlowVersion_API設計.md` | FlowVersion API |
| `05_06_Template_Export_API設計.md` | Template/Export API |
| `05_07_認証認可_API設計.md` | Auth/Permission API |
| `05_08_エラー設計.md` | 共通ErrorResponse |
| `05_09_OpenAPI設計.md` | OpenAPI/Swagger方針 |
| `05_10_API設計まとめ.md` | API設計まとめ |

## 4. 不要ファイル削除ルール

`05_API設計` 配下には正式設計書のみを残す。
以下は内容確認後、正式設計書へ統合済みであれば削除する。

- `05_更新.md`
- `05_更新概要.md`
- `README.md` が短文または前版維持のみの場合
- `*_概要.md`
- 作業メモ
- draft
- temp
- old

## 5. 今後の詳細化対象

- APIごとのRequest/Response完全定義
- Controller/Service/Repository対応表
- OpenAPI YAML
- API単体テスト
- Frontend API client設計

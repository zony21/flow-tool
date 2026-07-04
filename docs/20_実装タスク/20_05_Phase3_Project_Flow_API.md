# 20_05_Phase3_Project_Flow_API

## 1. 目的

Project、Flow、Lane、Stage、Node、Link、CommentのBackend APIを実装する。

## 2. Task一覧

| Task ID | Task Name | 優先度 |
| --- | --- | --- |
| P3-001 | Project API実装 | A |
| P3-002 | Flow一覧・詳細API実装 | A |
| P3-003 | Flow保存API実装 | A |
| P3-004 | Node / Link整合性Validation | A |
| P3-005 | Comment API実装 | B |
| P3-006 | API DTO整備 | A |

## 3. P3-001 Project API実装

目的:

Projectの作成、参照、更新、削除を実装する。

実装内容:

- GET /api/projects
- POST /api/projects
- GET /api/projects/{projectId}
- PUT /api/projects/{projectId}
- DELETE /api/projects/{projectId}
- 作成者をOwnerとしてProjectMember登録

関連設計:

- 05_API設計
- 17_権限管理設計

テスト観点:

- 19_03 Project API

完了条件:

- Project作成後、作成者がOwnerになる
- 参加Projectのみ一覧取得できる

## 4. P3-002 Flow一覧・詳細API実装

目的:

Project配下のFlowを取得できるAPIを実装する。

実装内容:

- GET /api/projects/{projectId}/flows
- GET /api/projects/{projectId}/flows/{flowId}
- Flow構造化データ返却

関連設計:

- 05_API設計
- 09_AI構造化データ設計

テスト観点:

- ViewerでもFlow参照可能
- 未参加Userは403

完了条件:

- Flow詳細でLane/Stage/Node/Link/Commentを取得できる

## 5. P3-003 Flow保存API実装

目的:

Editorで編集したFlow構造化データを保存する。

実装内容:

- PUT /api/projects/{projectId}/flows/{flowId}
- Lane / Stage / Node / Link / Comment保存
- 差分または全置換方式の決定と実装
- UpdatedAt更新

関連設計:

- 05_API設計
- 06_フローエディタ設計
- 09_AI構造化データ設計

テスト観点:

- Node追加保存
- Link追加保存
- Loop Link保存
- Viewerは403

完了条件:

- 保存後に再取得して同じ構造が返る

## 6. P3-004 Node / Link整合性Validation

目的:

不正なFlow構造の保存を防ぐ。

実装内容:

- Link source/target存在確認
- Node ID重複確認
- Lane/Stage参照確認
- Loop許可
- 必須Label確認

関連設計:

- 06_フローエディタ設計
- 13_描画エンジン設計

テスト観点:

- Link先Nodeなしは400
- Loopは許可

完了条件:

- 不正構造は400で返る

## 7. P3-005 Comment API実装

目的:

独立CommentとNode紐付Commentを扱えるようにする。

実装内容:

- Comment追加
- Comment更新
- Comment削除
- linkedNodeId対応
- position対応

関連設計:

- 06_フローエディタ設計
- 09_AI構造化データ設計

テスト観点:

- Node紐付Comment保存
- 独立Comment保存

完了条件:

- CommentがFlow取得時に返る

## 8. P3-006 API DTO整備

目的:

API Request / Response DTOを整理する。

実装内容:

- Project DTO
- Flow DTO
- Node DTO
- Link DTO
- Comment DTO
- Validation Attribute

関連設計:

- 05_API設計
- 12_バックエンド設計

テスト観点:

- DTOとEntity変換が成功する

完了条件:

- ControllerがEntityを直接返さない

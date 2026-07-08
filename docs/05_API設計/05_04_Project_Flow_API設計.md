# 05_04_Project_Flow_API設計

## 1. 目的

Project APIとFlow APIの詳細仕様を定義する。
Projectは最上位管理単位、FlowはProject配下の作業単位である。

Flowは通常フローとAGF/AGV搬送フローを区別するため、flowTypeを保持する。

## 2. Project一覧取得

GET /api/projects

### Response

ProjectResponseの配列を返す。
論理削除済みProjectは通常返さない。

## 3. Project作成

POST /api/projects

### Request

ProjectSaveRequestを受け取る。
projectNameは必須、最大100文字とする。

### 処理

PROJECTを作成し、作成者を監査カラムへ設定する。
必要に応じてPROJECT_MEMBERへOwnerを追加する。

## 4. Project更新

PUT /api/projects/{projectId}

Projectの名称、説明、表示順を更新する。
存在しないprojectIdの場合は404を返す。

## 5. Project削除

DELETE /api/projects/{projectId}

PROJECTを論理削除する。
配下Flowの扱いはDB設計に従い、初期実装では自動削除しない。

## 6. Flow一覧取得

GET /api/projects/{projectId}/flows

Project配下のFlow一覧を返す。
最新版Version番号、Node数などの集計情報を含める。

ResponseにはflowTypeを含める。

## 7. Flow作成

POST /api/projects/{projectId}/flows

FLOWを作成し、初期FLOW_VERSIONを作成する。
必要に応じて初期Lane、Stageを作成してもよい。

### Request

| 項目 | 必須 | 説明 |
| --- | --- | --- |
| name | YES | Flow名 |
| description | NO | 説明 |
| flowType | NO | NORMALまたはTRANSPORT。未指定時はNORMAL |

flowTypeがTRANSPORTの場合、フローエディタでTransport専用項目を表示する。

## 8. Flow更新

PUT /api/projects/{projectId}/flows/{flowId}

Flow名、カテゴリ、説明、表示順、flowTypeを更新する。
Version配下の構造は更新しない。

NormalからTransportへ変更する場合は可能とする。
TransportからNormalへ変更する場合は、Transport属性が無効になる可能性があるため確認を必須とする。

## 9. Flow削除

DELETE /api/projects/{projectId}/flows/{flowId}

FLOWを論理削除する。
FLOW_VERSIONは履歴として保持する。

## 10. Flow複製

POST /api/projects/{projectId}/flows/{flowId}/duplicate

既存Flowの最新Versionを複製し、新しいFlowと初期Versionを作成する。
Lane、Stage、Node、Link、CommentはIDを再採番する。

flowTypeも複製する。
Transport Flowの場合、NodeのTransport属性も複製する。

## 11. FlowType

FlowTypeは以下とする。

| 値 | 説明 |
| --- | --- |
| NORMAL | 通常フロー |
| TRANSPORT | AGF/AGV搬送フロー |

## 12. エラー

- 400: 入力値不正
- 404: ProjectまたはFlowが存在しない
- 409: 同時更新競合
- 422: flowType不正、Transport属性不整合
- 500: 処理失敗

## 13. テスト観点

- Projectを作成できること
- Flow作成時に初期Versionが作られること
- Flow作成時にflowTypeを保存できること
- flowType未指定時はNORMALになること
- Flow複製時にIDが再採番されること
- Transport Flow複製時にTransport属性が保持されること
- 論理削除済みデータが一覧に出ないこと

## 14. 完了条件

ProjectとFlowの管理APIが画面設計・DB設計と対応していること。

FlowTypeにより通常フローとAGF/AGV搬送フローを区別できること。

# 12_18_Validation設計

## 1. 目的

本書は、AI Flow Designer BackendにおけるValidation設計を定義する。

Backendは、Frontendで事前検証されたデータであっても必ず再検証する。SSOTを破壊する不正なFlow構造が保存されると、Export、AIレビュー、Version Snapshot、将来のコード生成に影響するためである。

## 2. Validation分類

| 分類 | 内容 | 主な場所 |
| --- | --- | --- |
| DTO Validation | 必須、文字数、型、範囲 | Application Validator |
| Authorization Validation | 権限、Role | ProjectAuthorizationService |
| Structure Validation | Flow構造整合性 | FlowStructureValidator |
| Business Validation | 業務ルール | Service / Domain Rule |
| Export Validation | Export可能性 | ExportService |
| File Validation | 画像・ファイル | ImageFileService |

## 3. 基本方針

- Frontend検証を信用しない。
- Backend保存前に必ず検証する。
- DTO Validationと構造Validationを分ける。
- Validation Errorはfield単位で返す。
- errorCodeを返す。
- Flow保存時は構造全体を検証する。
- Export前にも最低限の整合性を確認する。

## 4. DTO Validation

FluentValidationまたは独自Validatorを利用する。

対象例:

- CreateProjectRequest
- CreateFlowRequest
- SaveFlowRequest
- CreateVersionRequest
- ExportRequest
- ApplyTemplateRequest
- UploadImageRequest

検証例:

- 必須項目
- 最大文字数
- GUID形式
- enum範囲
- 座標範囲
- サイズ範囲
- ファイルサイズ

## 5. SaveFlowRequest Validation

検証項目:

- flowId必須
- revision必須
- lanes配列null禁止
- stages配列null禁止
- nodes配列null禁止
- links配列null禁止
- comments配列null禁止
- Node title最大文字数
- Link label最大文字数
- positionX / positionYの範囲
- width / heightが0以下でない

## 6. FlowStructureValidator

Flow構造全体の整合性を検証する。

検証項目:

- LaneId重複禁止
- StageId重複禁止
- NodeId重複禁止
- LinkId重複禁止
- CommentId重複禁止
- NodeのLaneIdが存在する
- NodeのStageIdが存在する
- LinkのSourceNodeIdが存在する
- LinkのTargetNodeIdが存在する
- Linkが削除済Nodeを参照しない
- CommentのTargetIdが存在する
- image NodeのImageFileが存在する
- decision NodeからのLinkにConditionがある

## 7. Loop方針

Loopは許可する。

Validationで禁止しないもの:

- A -> B -> A
- A -> A の自己参照Link

ただし、自己参照LinkはWarningとして返せる余地を持つ。

## 8. Lane / Stage削除Validation

LaneまたはStage削除時は、内包Nodeの扱いが必要である。

必須項目:

- deleteMode
- move先LaneIdまたはStageId

deleteMode:

- MoveNodes
- DeleteWithNodes

MoveNodesの場合は移動先が存在することを確認する。

## 9. NodeType Validation

NodeTypeごとの必須Propertyを検証する。

| NodeType | 必須Property |
| --- | --- |
| start | startCondition 任意 |
| end | endCondition 任意 |
| process | processDescription 推奨 |
| decision | decisionCondition 必須 |
| image | imageFileId 必須 |

拡張NodeTypeはNodeType定義から必須Propertyを取得する。

## 10. Export Validation

Export前に以下を確認する。

- FlowまたはSnapshotが存在する。
- Project権限がある。
- ExportTypeが対応対象である。
- SnapshotIdがFlowに属する。
- PDF Exportに必要な設定がある。

Exportは保存済みFlowまたはSnapshotを対象とする。

## 11. File Validation

画像アップロードでは以下を確認する。

- fileが存在する。
- sizeが上限以下。
- 拡張子が許可対象。
- MIMEが許可対象。
- 拡張子とMIMEが矛盾しない。
- SVGが安全である。
- 画像として読み取れる。

## 12. Validation Error形式

```json
{
  "success": false,
  "errorCode": "FLOW_VALIDATION_FAILED",
  "message": "Flow validation failed.",
  "details": [
    {
      "field": "nodes[0].title",
      "message": "Node title is required."
    }
  ],
  "traceId": "00-..."
}
```

## 13. Warning方針

保存を妨げない注意事項はWarningとして扱う。

例:

- Node説明未入力
- Lane責務未入力
- 自己参照Link
- AI専用メモなし
- Link label未入力

Warningは保存を妨げないが、AIレビュー前に提示できるようにする。

## 14. テスト観点

- NodeId重複が検出される。
- 存在しないNodeをLinkが参照するとValidation Errorになる。
- Loopが保存禁止にならない。
- decision NodeのCondition不足が検出される。
- image NodeのimageFileId不足が検出される。
- Lane削除時にdeleteMode不足が検出される。
- Validation Errorがfield単位で返る。

## 15. 完了条件

- DTO Validationと構造Validationが分離されている。
- Flow保存時の整合性検証が定義されている。
- Node / Link / Lane / Stage / CommentのValidationが定義されている。
- WarningとErrorの扱いが定義されている。
- AIが本書を読んでValidationを実装できる。

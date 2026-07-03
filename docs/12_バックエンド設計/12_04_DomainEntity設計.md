# 12_04_DomainEntity設計

## 1. 目的

本書は、AI Flow Designer BackendのDomain Entity設計を定義する。

AI Flow Designerでは、図形ではなくProject / Flow / Lane / Stage / Node / Link / Commentの構造化データをSSOTとする。Domain Entityは、このSSOTの意味と整合性を表す中核である。

## 2. 基本方針

- Domain Entityは業務意味を表す。
- EF Core Entityと完全一致させる必要はない。
- API DTOをDomain Entityにしない。
- IDはGUIDを基本とする。
- 論理削除を基本とする。
- 作成者・更新者・作成日時・更新日時を保持する。
- Flow構造の整合性はDomain Ruleで検証する。

## 3. Entity一覧

```text
Project
ProjectMember
Flow
Lane
Stage
FlowNode
FlowLink
FlowComment
FlowVersionSnapshot
Template
ImageFile
UserAccount
Setting
```

## 4. Project

ProjectはFlowを束ねる単位である。

主な項目:

- ProjectId
- ProjectName
- Description
- OwnerUserId
- IsDeleted
- CreatedAt
- UpdatedAt

関連:

- Flow
- ProjectMember
- Template
- ImageFile

## 5. ProjectMember

Project単位の権限を表す。

主な項目:

- ProjectMemberId
- ProjectId
- UserId
- Role
- IsDeleted
- CreatedAt
- UpdatedAt

Role:

- Admin
- ProjectAdmin
- Editor
- Viewer

## 6. Flow

Flowは設計対象の中核である。

主な項目:

- FlowId
- ProjectId
- FlowName
- Purpose
- Description
- Revision
- IsDeleted
- CreatedAt
- UpdatedAt

関連:

- Lane
- Stage
- FlowNode
- FlowLink
- FlowComment
- FlowVersionSnapshot

## 7. Lane

Laneは責務・担当領域を表す。

主な項目:

- LaneId
- FlowId
- LaneName
- Responsibility
- OrderNo
- StyleJson
- IsDeleted

LaneはAIが「どこが工程を担うか」を理解するための重要情報である。
単なる背景帯ではない。

## 8. Stage

Stageは工程区切りを表す。

主な項目:

- StageId
- FlowId
- StageName
- Purpose
- OrderNo
- StyleJson
- IsDeleted

## 9. FlowNode

FlowNodeは処理、判定、開始、終了、画像、補足などを表す。

主な項目:

- NodeId
- FlowId
- LaneId
- StageId
- NodeType
- Title
- Description
- PropertyJson
- PositionX
- PositionY
- Width
- Height
- AiNotes
- IsDeleted

NodeType初期値:

- start
- end
- process
- decision
- hexagon
- balloon
- image

NodeTypeは将来拡張可能とする。

## 10. FlowLink

FlowLinkはNode間の接続、条件、データ受け渡し、通信を表す。

主な項目:

- LinkId
- FlowId
- SourceNodeId
- TargetNodeId
- Label
- Condition
- DataName
- CommunicationType
- StyleJson
- IsDeleted

Loopは許可する。
自己参照Linkは初期では警告扱いとし、保存禁止にはしない。

## 11. FlowComment

FlowCommentは補足、レビュー、AI専用メモを表す。

主な項目:

- CommentId
- FlowId
- TargetType
- TargetId
- CommentType
- Body
- PositionX
- PositionY
- AiVisible
- IsDeleted

TargetType:

- flow
- lane
- stage
- node
- link
- none

## 12. FlowVersionSnapshot

FlowVersionSnapshotは保存済みFlowの正式履歴である。

主な項目:

- VersionId
- FlowId
- VersionNo
- Title
- Description
- SnapshotJson
- SchemaVersion
- CreatedBy
- CreatedAt

SnapshotJsonには当時のFlow構造を再現できる情報を保持する。

## 13. Template

TemplateはFlow構造の再利用単位である。

主な項目:

- TemplateId
- ProjectId
- TemplateName
- Description
- TemplateJson
- IsStandard
- IsDeleted

Template適用時はIDを再採番する。

## 14. ImageFile

ImageFileは画像Nodeで利用するファイルメタデータである。

主な項目:

- ImageFileId
- ProjectId
- FlowId
- OriginalFileName
- ContentType
- FileSize
- HashSha256
- StorageKey
- Width
- Height
- IsSvg
- IsDeleted

物理パスはAPIへ返さない。

## 15. UserAccount

GitHub OAuthログインユーザーを表す。

主な項目:

- UserId
- GitHubUserId
- LoginName
- DisplayName
- AvatarUrl
- Email
- LastLoginAt
- IsDeleted

## 16. 共通監査項目

多くのEntityに以下を持たせる。

- SystemCreateUserId
- SystemCreateDatetime
- SystemUpdateUserId
- SystemUpdateDatetime
- IsDeleted

## 17. Domain Rule

主なRule:

- Flow内NodeId重複禁止
- Flow内LinkId重複禁止
- LinkのSource / Target Node存在確認
- NodeのLane存在確認
- NodeのStage存在確認
- NodeType存在確認
- decision Nodeの条件Link確認
- image NodeのImageFile存在確認

## 18. EF Coreとの関係

Domain EntityとEF Core Entityは同一にしてもよいが、以下を守る。

- DomainがEF Core属性へ依存しすぎない。
- Fluent APIでDB設定を行う。
- DB都合の項目をDomainロジックへ混ぜない。

## 19. テスト観点

- Linkが存在しないNodeを参照すると検証エラーになる。
- Loopが保存禁止にならない。
- Lane削除時にNode処理方針が必要になる。
- Template適用時にID再採番される。
- Snapshot作成後に元Flowを変更してもSnapshotが変わらない。

## 20. 完了条件

- Backendの主要Domain Entityが定義されている。
- SSOT構造がDomain Entityとして表現されている。
- Link、Lane、Stage、Comment、Snapshotの意味が明確である。
- AIが本書を読んでDomain Entity実装に着手できる。

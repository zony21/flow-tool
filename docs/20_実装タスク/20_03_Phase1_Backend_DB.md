# 20_03_Phase1_Backend_DB

## 1. 目的

AI Flow DesignerのSSOTとなるDB、Entity、DbContext、Migrationを実装する。

## 2. Task一覧

| Task ID | Task Name | 優先度 |
| --- | --- | --- |
| P1-001 | Core Entity作成 | A |
| P1-002 | 権限Entity作成 | A |
| P1-003 | 設定Entity作成 | A |
| P1-004 | DbContext設定 | A |
| P1-005 | 初期Migration作成 | A |
| P1-006 | Seed Data作成 | B |

## 3. P1-001 Core Entity作成

目的:

Project / Flow / Lane / Stage / Node / Link / Comment / Image / Version / MetadataのEntityを作成する。

対象:

- backend/Entities

実装内容:

- ProjectEntity
- FlowEntity
- LaneEntity
- StageEntity
- NodeEntity
- LinkEntity
- CommentEntity
- ImageEntity
- VersionEntity
- MetadataEntity

関連設計:

- 04_DB設計
- 09_AI構造化データ設計
- 12_バックエンド設計

テスト観点:

- EntityがDbContextへ登録できる
- リレーションが循環参照で壊れない

完了条件:

- SSOT中核Entityが作成されている

## 4. P1-002 権限Entity作成

目的:

17章の権限管理Entityを作成する。

対象:

- backend/Entities/Auth

実装内容:

- UserEntity
- ProjectMemberEntity
- RoleEntity
- PermissionEntity
- RolePermissionEntity
- ProjectInviteEntity
- AuditLogEntity

関連設計:

- 17_権限管理設計

テスト観点:

- ProjectにOwnerを設定できる
- RolePermissionが多対多で表現できる

完了条件:

- 権限管理Entityが作成されている

## 5. P1-003 設定Entity作成

目的:

18章の設定Entityを作成する。

対象:

- backend/Entities/Settings

実装内容:

- UserSettingEntity
- ProjectSettingEntity
- EditorSettingEntity
- AiSettingEntity
- ExportSettingEntity

関連設計:

- 18_設定設計

テスト観点:

- User単位、Project単位、User+Project単位が一意制約で表現できる

完了条件:

- 設定Entityが作成されている

## 6. P1-004 DbContext設定

目的:

全EntityをEF Coreで管理できるようにする。

対象:

- AppDbContext

実装内容:

- DbSet追加
- Fluent API設定
- 一意制約設定
- DeleteBehavior設定
- DateTime管理方針設定

関連設計:

- 04_DB設計
- 12_バックエンド設計

テスト観点:

- dotnet build成功
- DbContext生成成功

完了条件:

- すべてのEntityがDbContextに登録されている

## 7. P1-005 初期Migration作成

目的:

SQLite用の初期Migrationを作成する。

対象:

- backend/Migrations

実装内容:

- InitialCreate Migration作成
- SQLite DB作成確認

関連設計:

- 04_DB設計

テスト観点:

- Migration適用成功
- DBファイル作成成功

完了条件:

- 初期DBが作成できる

## 8. P1-006 Seed Data作成

目的:

初期Role、Permission、Setting Default確認用のSeedを作成する。

対象:

- backend/Data/Seed

実装内容:

- Owner / Editor / Viewer Role
- Permission Code一覧
- RolePermission
- テスト用Project必要最小Seed

関連設計:

- 17_権限管理設計
- 19_09_テストデータ設計

テスト観点:

- PermissionServiceで初期Roleが判定できる

完了条件:

- 初期Role/Permission Seedが作成されている

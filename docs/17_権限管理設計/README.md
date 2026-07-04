# 17_権限管理設計

## 1. 目的

AI Flow Designerの利用者管理、GitHub OAuth、Project単位Role、Permission、API認可、Frontend制御、Project共有に関する設計を管理する。

本章は、複数ユーザーでProjectを扱う場合でも、設計データの参照・編集・削除・Export・設定変更を安全に制御するための基準である。

## 2. 基本方針

- GitHub OAuthで認証する
- 認証と認可を分離する
- Project単位でRoleを管理する
- RoleとPermissionを分離する
- Frontendでは操作可否を分かりやすく制御する
- Backend APIでは必ずPermissionを確認する
- 重要操作はAuditLogへ記録する

## 3. 初期Role

初期実装では以下の3Roleに絞る。

| Role | 概要 |
| --- | --- |
| Owner | Project所有者。全操作可能 |
| Editor | Flow編集者。Project管理・Member管理は不可 |
| Viewer | 閲覧者。参照とExportのみ可能 |

Adminは初期実装では採用しない。

将来、組織管理やチーム運用が必要になった場合に追加できる構造とする。

## 4. 主要設計一覧

| ファイル | 内容 | 状態 |
| --- | --- | --- |
| 17_01_権限管理概要 | 権限管理全体、Role Matrix、Frontend/Backend責務 | 再詳細化済み |
| 17_02_ユーザーデータモデル設計 | USER、PROJECT_MEMBER、ROLE、PERMISSION、ROLE_PERMISSION、INVITE、AUDIT_LOG | 再詳細化済み |
| 17_03_GitHubOAuth設計 | GitHub OAuth認証方針 | 詳細化済み |
| 17_04_ProjectRole設計 | Owner/Editor/Viewer、Role変更、最後のOwner保護 | 再詳細化済み |
| 17_05_Permission設計 | Permission Code、Role別Permission、Frontend/Backend定数方針 | 再詳細化済み |
| 17_06_API認可設計 | PermissionService、RequirePermission、API別Permission | 再詳細化済み |
| 17_07_画面制御設計 | Route Guard、Pinia Store、Button/Menu/Dialog制御 | 再詳細化済み |
| 17_08_共同利用設計 | 複数人利用方針 | 詳細化済み |
| 17_09_履歴管理設計 | 権限変更履歴 | 詳細化済み |
| 17_10_招待設計 | Project招待 | 詳細化済み |
| 17_11_利用状態管理設計 | active / invited / suspended等 | 詳細化済み |
| 17_12_Project管理者設計 | Project管理責務 | 詳細化済み |
| 17_13_操作範囲設計 | 操作対象整理 | 詳細化済み |
| 17_14_エラー設計 | 401 / 403等 | 詳細化済み |
| 17_15_共通判定設計 | 共通Permission判定 | 詳細化済み |
| 17_16_設定連携設計 | 設定機能との連携 | 詳細化済み |
| 17_17_Export連携設計 | Export権限制御 | 詳細化済み |
| 17_18_確認項目設計 | 実装前確認 | 詳細化済み |
| 17_19_運用設計 | 運用方針 | 詳細化済み |
| 17_20_権限管理設計まとめ | 章全体まとめ | 再詳細化済み |

## 5. 実装時の中心部品

Backend:

- AuthMiddleware
- CurrentUserService
- PermissionService
- RequirePermissionAttribute
- AuditLogService

Frontend:

- Auth Store
- Project Permission Store
- Route Guard
- PermissionCodes定数
- Button/Menu/Dialog制御

DB:

- USER
- PROJECT_MEMBER
- ROLE
- PERMISSION
- ROLE_PERMISSION
- PROJECT_INVITE
- AUDIT_LOG

## 6. 状態

17章は再詳細化済み。

特に以下を実装可能粒度まで補強した。

- Role Matrix
- DBテーブル案
- Permission Code一覧
- Role別Permission
- API別Permission対応
- Backend PermissionService方針
- Frontend Pinia Store / Route Guard / Button制御
- 401 / 403 の使い分け
- 最後のOwner保護

## 7. 次工程

次工程は `18_設定設計` の詳細化。

18章では以下を設計する。

- User Setting
- Project Setting
- Editor Setting
- AI Setting
- Export Setting
- 設定DB
- 設定API
- 設定画面
- 権限制御との連携

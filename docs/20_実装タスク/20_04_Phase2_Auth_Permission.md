# 20_04_Phase2_Auth_Permission

## 1. 目的

GitHub OAuth、User管理、ProjectMember、Role、Permission、API認可、Frontend権限制御を実装する。

## 2. Task一覧

| Task ID | Task Name | 優先度 |
| --- | --- | --- |
| P2-001 | GitHub OAuth設定 | A |
| P2-002 | CurrentUserService実装 | A |
| P2-003 | PermissionService実装 | A |
| P2-004 | RequirePermissionAttribute実装 | A |
| P2-005 | ProjectMember管理API実装 | A |
| P2-006 | Frontend Auth Store実装 | A |
| P2-007 | Project Permission Store実装 | A |
| P2-008 | Route Guard実装 | A |

## 3. P2-001 GitHub OAuth設定

目的:

GitHub OAuthでログインできる状態を作る。

対象:

- backend Auth設定
- frontend Login画面

実装内容:

- GitHub OAuth Client設定
- Callback API作成
- User登録/更新
- Login状態確認API作成

関連設計:

- 17_03_GitHubOAuth設計

テスト観点:

- 未ログインは401
- ログイン後CurrentUserが取得できる

完了条件:

- GitHub OAuthでログインできる

## 4. P2-002 CurrentUserService実装

目的:

API処理中にログインUserを取得できるServiceを作る。

実装内容:

- CurrentUser取得
- UserId取得
- 未ログイン判定

関連設計:

- 17_06_API認可設計

テスト観点:

- ログインUserを取得できる
- 未ログイン時はnullまたは例外扱いになる

完了条件:

- Controller/ServiceからCurrentUserを参照できる

## 5. P2-003 PermissionService実装

目的:

UserがProject内で指定Permissionを持つか判定する。

実装内容:

- CanAsync(userId, projectId, permissionCode)
- GetPermissionsAsync(userId, projectId)
- ProjectMember/Role/Permission照会

関連設計:

- 17_05_Permission設計
- 17_06_API認可設計

テスト観点:

- 19_02 PermissionService Unit Test
- 19_06 権限テスト

完了条件:

- Owner/Editor/Viewerの判定が期待通りである

## 6. P2-004 RequirePermissionAttribute実装

目的:

Controller/ActionでPermissionを宣言できる仕組みを作る。

実装内容:

- Attribute作成
- FilterまたはMiddleware連携
- RouteからprojectId取得
- 401/403返却

関連設計:

- 17_06_API認可設計

テスト観点:

- 未ログイン401
- Permissionなし403

完了条件:

- APIごとにPermission指定できる

## 7. P2-005 ProjectMember管理API実装

目的:

Project Member参照、招待、Role変更、削除を実装する。

実装内容:

- Member一覧API
- Invite API
- ChangeRole API
- RemoveMember API
- 最後のOwner保護

関連設計:

- 17_04_ProjectRole設計
- 17_10_招待設計

テスト観点:

- OwnerのみMember管理可能
- 最後のOwner降格不可

完了条件:

- Member管理がOwner権限で動作する

## 8. P2-006 Frontend Auth Store実装

目的:

Frontendでログイン状態を管理する。

実装内容:

- Auth Store
- CurrentUser取得
- Login/Logout

関連設計:

- 17_07_画面制御設計

テスト観点:

- ログイン状態がStoreに反映される

完了条件:

- Header等でログインUserを表示できる

## 9. P2-007 Project Permission Store実装

目的:

Project単位のRole/PermissionをFrontendで保持する。

実装内容:

- projectId
- roleCode
- permissions
- can(permissionCode)

関連設計:

- 17_07_画面制御設計

テスト観点:

- can()が期待通りtrue/falseを返す

完了条件:

- Button/Menu制御でPermissionを利用できる

## 10. P2-008 Route Guard実装

目的:

画面遷移時にログイン状態とPermissionを確認する。

実装内容:

- 未ログイン時Loginへ遷移
- Project.Read確認
- Flow.Read確認
- 403画面表示

関連設計:

- 17_07_画面制御設計

テスト観点:

- ViewerはFlow画面に入れる
- 未参加Userは403画面へ遷移

完了条件:

- 画面遷移時の権限制御が動作する

# 17_04_ProjectRole設計

## 1. 目的

Project単位のRole管理仕様を定義する。

AI Flow Designerでは、Userそのものに固定権限を持たせず、ProjectMemberにRoleを持たせる。

## 2. 基本方針

- User単位ではなくProject参加単位でRoleを保持する
- 同じUserでもProjectごとに異なるRoleを設定可能にする
- Role変更はOwnerのみ可能とする
- Projectには必ず1人以上のOwnerが必要
- 最後のOwnerを削除・降格できない

## 3. 初期Role

初期実装ではRoleを3種類に絞る。

| Role | 概要 | 主な用途 |
| --- | --- | --- |
| Owner | Project所有者 | Project管理、Member管理、全編集 |
| Editor | 編集者 | Flow作成、Flow編集、Template利用 |
| Viewer | 閲覧者 | 参照、Export |

Adminは初期実装では採用しない。

将来、組織管理やチーム管理が必要になった場合に追加する。

## 4. Role別責務

### 4.1 Owner

OwnerはProjectの管理責任を持つ。

可能な操作:

- Project参照
- Project更新
- Project削除
- Member招待
- Member削除
- Role変更
- Flow作成
- Flow更新
- Flow削除
- Template作成
- Template更新
- Imageアップロード
- Image削除
- Export実行
- Project設定変更

### 4.2 Editor

Editorは設計内容の作成・編集を担当する。

可能な操作:

- Project参照
- Flow参照
- Flow作成
- Flow更新
- Flow削除
- Node編集
- Link編集
- Comment編集
- Imageアップロード
- Template適用
- Template作成
- Export実行

不可:

- Project削除
- Project設定変更
- Member管理
- Role変更

### 4.3 Viewer

Viewerは内容確認を担当する。

可能な操作:

- Project参照
- Flow参照
- Version参照
- Image参照
- Template参照
- Export実行
- User個人設定変更

不可:

- Flow編集
- Node編集
- Link編集
- Comment編集
- Imageアップロード
- Template登録
- Project設定変更
- Member管理

## 5. Role Matrix

| 操作 | Permission | Owner | Editor | Viewer |
| --- | --- | --- | --- | --- |
| Project参照 | Project.Read | ○ | ○ | ○ |
| Project更新 | Project.Update | ○ | × | × |
| Project削除 | Project.Delete | ○ | × | × |
| Member参照 | Member.Read | ○ | × | × |
| Member招待 | Member.Invite | ○ | × | × |
| Member削除 | Member.Remove | ○ | × | × |
| Role変更 | Member.ChangeRole | ○ | × | × |
| Flow参照 | Flow.Read | ○ | ○ | ○ |
| Flow作成 | Flow.Create | ○ | ○ | × |
| Flow更新 | Flow.Update | ○ | ○ | × |
| Flow削除 | Flow.Delete | ○ | ○ | × |
| Version参照 | Version.Read | ○ | ○ | ○ |
| Version作成 | Version.Create | ○ | ○ | × |
| Node編集 | Node.Update | ○ | ○ | × |
| Link編集 | Link.Update | ○ | ○ | × |
| Comment編集 | Comment.Update | ○ | ○ | × |
| Image参照 | Image.Read | ○ | ○ | ○ |
| Imageアップロード | Image.Upload | ○ | ○ | × |
| Image削除 | Image.Delete | ○ | ○ | × |
| Template参照 | Template.Read | ○ | ○ | ○ |
| Template適用 | Template.Apply | ○ | ○ | × |
| Template作成 | Template.Create | ○ | ○ | × |
| Template更新 | Template.Update | ○ | ○ | × |
| Template削除 | Template.Delete | ○ | ○ | × |
| Export実行 | Export.Execute | ○ | ○ | ○ |
| Project設定変更 | Setting.ProjectUpdate | ○ | × | × |
| User設定変更 | Setting.UserUpdate | ○ | ○ | ○ |

## 6. Role変更ルール

### 6.1 変更可能者

Role変更はOwnerのみ可能とする。

### 6.2 変更不可ケース

以下の場合はRole変更不可とする。

- 自分自身を最後のOwnerからViewer/Editorへ変更する
- Project内のOwnerが0人になる
- suspended状態のMemberをOwnerへ変更する
- Projectがarchived状態である

### 6.3 Role変更時の処理

1. 操作者がOwnerか確認する
2. 対象MemberがProjectに所属しているか確認する
3. 変更後Roleが存在するか確認する
4. Owner数が0にならないか確認する
5. PROJECT_MEMBER.ROLE_IDを更新する
6. AUDIT_LOGへ変更前後を記録する

## 7. Project作成時のRole付与

Project作成者は自動でOwnerになる。

処理:

1. Project作成
2. User取得
3. PROJECT_MEMBER作成
4. ROLE=OWNERを付与
5. AUDIT_LOGへProject作成を記録

## 8. 判定タイミング

| タイミング | 判定内容 |
| --- | --- |
| Project一覧取得 | 参加Projectのみ取得 |
| Project詳細取得 | Project.Read確認 |
| Flow操作 | Flow系Permission確認 |
| Member操作 | Member系Permission確認 |
| Export実行 | Export.Execute確認 |
| 設定変更 | Setting系Permission確認 |

## 9. 完了条件

- 初期RoleがOwner/Editor/Viewerに整理されている
- Roleごとの責務が明確である
- Role Matrixで操作可否が判断できる
- 最後のOwnerを保護するルールがある
- API認可、画面制御、DB設計へ展開可能である

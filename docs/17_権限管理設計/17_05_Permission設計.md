# 17_05_Permission設計

## 1. 目的

Roleから具体的な操作可否を判断するPermission仕様を定義する。

PermissionはBackend API、Frontend画面制御、テスト仕様で共通利用する。

## 2. 基本方針

- Roleと操作判定を分離する
- APIではPermission単位で確認する
- Frontendも同じPermission Codeを利用する
- Permission Codeは文字列定数として管理する
- 将来的な細分化に対応できる命名にする

## 3. Permission命名規則

形式:

```text
Resource.Action
```

例:

```text
Project.Read
Flow.Update
Export.Execute
```

命名ルール:

- ResourceはPascalCase
- ActionはPascalCase
- DB値とFrontend定数とBackend定数を一致させる
- 意味が重複するPermissionを作らない

## 4. Resource一覧

| Resource | 対象 |
| --- | --- |
| Project | Project本体 |
| Member | Project参加者 |
| Flow | Flow本体 |
| Version | Flow Version |
| Node | Node |
| Link | Link |
| Comment | Comment |
| Image | Image |
| Template | Template |
| Export | Export |
| Setting | 設定 |

## 5. Permission一覧

### 5.1 Project

| Permission | 説明 |
| --- | --- |
| Project.Read | Project参照 |
| Project.Create | Project作成 |
| Project.Update | Project更新 |
| Project.Delete | Project削除 |
| Project.Archive | Projectアーカイブ |

### 5.2 Member

| Permission | 説明 |
| --- | --- |
| Member.Read | Member参照 |
| Member.Invite | Member招待 |
| Member.Remove | Member削除 |
| Member.ChangeRole | Role変更 |

### 5.3 Flow

| Permission | 説明 |
| --- | --- |
| Flow.Read | Flow参照 |
| Flow.Create | Flow作成 |
| Flow.Update | Flow更新 |
| Flow.Delete | Flow削除 |

### 5.4 Version

| Permission | 説明 |
| --- | --- |
| Version.Read | Version参照 |
| Version.Create | Version作成 |
| Version.Restore | Version復元 |
| Version.Delete | Version削除 |

### 5.5 Node / Link / Comment

| Permission | 説明 |
| --- | --- |
| Node.Update | Node追加・更新・削除・移動 |
| Link.Update | Link追加・更新・削除 |
| Comment.Update | Comment追加・更新・削除 |

Node.Create、Node.Deleteを分けず、初期実装ではNode.Updateに集約する。

理由は、Editor操作の大半が同一画面内の編集操作であり、細分化しすぎると初期実装とテストが複雑化するためである。

将来、操作制御を細分化する場合はNode.Create、Node.Delete、Node.Moveへ分割する。

### 5.6 Image

| Permission | 説明 |
| --- | --- |
| Image.Read | Image参照 |
| Image.Upload | Imageアップロード |
| Image.Update | Image情報更新 |
| Image.Delete | Image削除 |

### 5.7 Template

| Permission | 説明 |
| --- | --- |
| Template.Read | Template参照 |
| Template.Apply | Template適用 |
| Template.Create | Template作成 |
| Template.Update | Template更新 |
| Template.Delete | Template削除 |

### 5.8 Export

| Permission | 説明 |
| --- | --- |
| Export.Execute | Export実行 |

初期実装ではExport種類ごとにPermissionを分けない。

ただし将来、AI DSLやPDFのみ制限したい場合に備え、ExportTypeはAPI Requestで受け取りAuditLogへ記録する。

### 5.9 Setting

| Permission | 説明 |
| --- | --- |
| Setting.UserUpdate | User個人設定更新 |
| Setting.ProjectUpdate | Project設定更新 |
| Setting.EditorUpdate | Editor設定更新 |
| Setting.AiUpdate | AI設定更新 |
| Setting.ExportUpdate | Export設定更新 |

## 6. Role別Permission

### 6.1 Owner

Ownerは全Permissionを持つ。

### 6.2 Editor

Editorが持つPermission:

- Project.Read
- Flow.Read
- Flow.Create
- Flow.Update
- Flow.Delete
- Version.Read
- Version.Create
- Node.Update
- Link.Update
- Comment.Update
- Image.Read
- Image.Upload
- Image.Update
- Image.Delete
- Template.Read
- Template.Apply
- Template.Create
- Template.Update
- Template.Delete
- Export.Execute
- Setting.UserUpdate

### 6.3 Viewer

Viewerが持つPermission:

- Project.Read
- Flow.Read
- Version.Read
- Image.Read
- Template.Read
- Export.Execute
- Setting.UserUpdate

## 7. Backend定数方針

BackendではPermission Codeを定数化する。

例:

```csharp
public static class PermissionCodes
{
    public const string ProjectRead = "Project.Read";
    public const string FlowUpdate = "Flow.Update";
    public const string ExportExecute = "Export.Execute";
}
```

ControllerやServiceで文字列直書きしない。

## 8. Frontend定数方針

FrontendでもPermission Codeを定数化する。

例:

```ts
export const PermissionCodes = {
  ProjectRead: 'Project.Read',
  FlowUpdate: 'Flow.Update',
  ExportExecute: 'Export.Execute',
} as const
```

Button制御やRoute Guardではこの定数を利用する。

## 9. 判定方針

Permission判定は以下の形で統一する。

```text
Can(userId, projectId, permissionCode)
```

判定結果:

| 結果 | 意味 |
| --- | --- |
| true | 操作可能 |
| false | 操作不可 |

操作不可の場合、Backendは403 Forbiddenを返す。

未ログインの場合は401 Unauthorizedを返す。

## 10. 完了条件

- Permission Codeの命名規則が明確である
- 主要操作に必要なPermissionが定義されている
- Role別Permissionが定義されている
- BackendとFrontendで同じPermission Codeを利用できる
- 19_テスト仕様でRole別テストへ展開できる

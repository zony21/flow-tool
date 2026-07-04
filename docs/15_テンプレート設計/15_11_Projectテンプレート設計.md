# 15_11_Projectテンプレート設計

## 1. 目的

本書は、AI Flow DesignerのProject Template設計を定義する。

Project Templateは、特定Project内で作成・管理されるTemplateである。Project固有の業務ルール、設備、通信、DB更新、責務分担を再利用するために利用する。

## 2. 基本方針

- Project TemplateはProjectに紐付ける。
- Project権限を持つユーザーのみ参照できる。
- Editor以上が作成・更新できる。
- ProjectAdmin以上が削除できる方針を基本とする。
- Standard Templateを複製してProject Template化できる。

## 3. 保存項目

- templateId
- projectId
- templateName
- description
- category
- tags
- templateJson
- schemaVersion

## 4. 作成方法

- 現在Flowから作成
- 選択範囲から作成
- Standard Templateから複製
- Importから作成

## 5. 権限

| 操作 | ProjectAdmin | Editor | Viewer |
| --- | --- | --- | --- |
| 参照 | ○ | ○ | ○ |
| 作成 | ○ | ○ | × |
| 更新 | ○ | ○ | × |
| 削除 | ○ | × | × |
| 適用 | ○ | ○ | × |

## 6. テスト観点

- Project Templateを作成できる。
- Viewerは作成できない。
- Project外ユーザーは参照できない。
- Standard Templateから複製できる。

## 7. 完了条件

- Project Templateの権限、作成方法、管理方針が定義されている。

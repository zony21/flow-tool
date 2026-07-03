# 12_17_VersionSnapshot設計

## 1. 目的

本書は、AI Flow Designer BackendにおけるVersion Snapshot設計を定義する。

Version Snapshotは、保存済みFlowの正式履歴である。FrontendのUndo / Redoとは異なり、Backendで保存・復元・Export対象として扱う永続的な履歴である。

## 2. 基本方針

- SnapshotはBackendで作成する。
- Snapshot対象は保存済みFlowとする。
- 未保存Frontend状態からSnapshotを作成しない。
- SnapshotはExport入力に利用できる。
- Snapshot復元時は現在状態を事前Snapshotとして保存する。
- SnapshotJsonには当時のFlow構造を再現できる情報を含める。

## 3. Snapshot対象

Snapshotには以下を含める。

- Project情報
- Flow情報
- Lane一覧
- Stage一覧
- Node一覧
- Link一覧
- Comment一覧
- SchemaVersion
- Export関連Metadata

## 4. FLOW_VERSION テーブル

主な項目:

- VERSION_ID
- FLOW_ID
- VERSION_NO
- TITLE
- DESCRIPTION
- SNAPSHOT_JSON
- SCHEMA_VERSION
- CREATED_BY
- SYSTEM_CREATE_DATETIME
- IS_DELETED

## 5. VersionNo採番

VersionNoはFlow単位の連番とする。

採番処理:

1. FlowIdで最新VersionNoを取得する。
2. latest + 1を採番する。
3. Transaction内でFLOW_VERSIONへ追加する。

同時作成競合を避けるため、Flow単位で排他または一意制約を検討する。

一意制約候補:

```text
FLOW_ID + VERSION_NO
```

## 6. SnapshotJson構造

```json
{
  "schemaVersion": "1.0",
  "project": {},
  "flow": {},
  "lanes": [],
  "stages": [],
  "nodes": [],
  "links": [],
  "comments": [],
  "metadata": {}
}
```

SnapshotJsonは当時の構造再現を優先する。
DBの最新テーブル構造に完全依存しない。

## 7. Snapshot作成処理

```text
VersionController
 -> FlowVersionService
 -> FlowQueryService
 -> FlowVersionRepository
 -> UnitOfWork / Transaction
```

処理:

1. JWT認証。
2. Project権限確認。
3. Editor以上確認。
4. 保存済みFlow取得。
5. Flow構造Validation。
6. VersionNo採番。
7. SnapshotJson生成。
8. FLOW_VERSION追加。
9. SaveChanges。
10. Commit。
11. VersionDto返却。

## 8. Snapshot復元処理

```text
VersionController
 -> FlowVersionService
 -> FlowSaveService
 -> UnitOfWork / Transaction
```

処理:

1. JWT認証。
2. Project権限確認。
3. Editor以上確認。
4. 対象Version取得。
5. 現在Flowを復元前Snapshotとして保存。
6. SnapshotJsonを現在Flow構造へ変換。
7. Flowへ反映。
8. Revision更新。
9. SaveChanges。
10. Commit。
11. 復元結果返却。

失敗時は現在Flowを変更しない。

## 9. Snapshot比較

Version比較は将来機能として扱うが、設計上は以下を可能にする。

比較対象:

- Lane差分
- Stage差分
- Node追加・変更・削除
- Link追加・変更・削除
- Comment追加・変更・削除
- Property差分

比較結果Model:

```csharp
public sealed class VersionDiffResult
{
    public IReadOnlyList<DiffItem> Items { get; init; }
}
```

## 10. SnapshotとExport

ExportRequestにSnapshotIdが指定された場合、現在FlowではなくSnapshotを使用する。

対象:

- Mermaid
- PDF
- JSON
- AI DSL

## 11. SchemaVersion

SnapshotJsonにはSchemaVersionを持たせる。

将来、構造変更があった場合はMigration Converterを用意する。

例:

```text
SnapshotSchemaV1ToV2Converter
```

## 12. 例外処理

errorCode例:

- VERSION_NOT_FOUND
- VERSION_CREATE_FAILED
- VERSION_RESTORE_FAILED
- VERSION_CONFLICT
- VERSION_SCHEMA_UNSUPPORTED

## 13. テスト観点

- Snapshot作成時にVersionNoが連番になる。
- Snapshot作成後に現在Flowを変更してもSnapshotJsonが変わらない。
- SnapshotからExportできる。
- 復元失敗時に現在Flowが変更されない。
- SchemaVersion不一致を検出できる。

## 14. 完了条件

- Snapshotの保存対象と構造が定義されている。
- VersionNo採番方針が定義されている。
- 作成・復元・比較・Export連携が定義されている。
- AIが本書を読んでVersion Snapshotを実装できる。

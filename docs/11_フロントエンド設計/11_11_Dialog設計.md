# 11_11_Dialog設計

## 1. 目的

本書は、AI Flow DesignerのDialog設計を定義する。

Dialogは、保存確認、削除確認、Export、Snapshot作成、Template適用、AIレビュー開始など、ユーザーが重要な判断を行う場面で使用する。
Dialogの設計が統一されていないと、操作ミス、未保存データ消失、Export対象誤り、権限誤認が発生する。

そのため、Dialogの種類、責務、状態管理、入力項目、操作結果を統一する。

## 2. 基本方針

- Dialogは目的別にComponentを分割する。
- 共通Dialog部品を用意し、見た目と操作感を統一する。
- 重要操作では確認Dialogを必ず表示する。
- Loading、Error、Cancelの状態を明確にする。
- Dialogから直接SSOTを壊す処理を行わない。
- 保存、削除、Exportなどの実処理はStoreまたはUseCase経由で行う。

## 3. Dialog分類

| 分類 | 例 | 用途 |
| --- | --- | --- |
| 確認 | ConfirmDialog | 汎用確認 |
| 破壊的操作 | DeleteConfirmDialog | 削除確認 |
| 未保存確認 | UnsavedChangesDialog | 画面遷移前確認 |
| 入力 | ProjectCreateDialog / FlowCreateDialog | 新規作成 |
| Export | ExportDialog | 出力形式・オプション選択 |
| Snapshot | SnapshotCreateDialog | Version Snapshot作成 |
| Template | TemplateApplyDialog | Template適用 |
| AIレビュー | AiReviewStartDialog | AIレビュー開始確認 |
| Error | ErrorDetailDialog | 詳細エラー確認 |

## 4. 共通Dialog Component

```text
components/dialog/
 ├─ AppDialog.vue
 ├─ ConfirmDialog.vue
 ├─ DeleteConfirmDialog.vue
 ├─ UnsavedChangesDialog.vue
 └─ ErrorDetailDialog.vue
```

### 4.1 AppDialog

全Dialogの土台とする。

Props例:

```ts
interface AppDialogProps {
  visible: boolean;
  title: string;
  width?: string;
  closable?: boolean;
  loading?: boolean;
}
```

Emits例:

```ts
interface AppDialogEmits {
  close: [];
}
```

### 4.2 共通表示

- Title
- Body
- Footer
- Primary Button
- Secondary Button
- Loading表示
- Error表示

## 5. ConfirmDialog

### 5.1 用途

汎用確認に使用する。

例:

- 保存してよいか
- 設定を変更してよいか
- Templateを適用してよいか

### 5.2 ボタン

- 実行
- キャンセル

### 5.3 方針

実行ボタンは右側、キャンセルは左側に配置する。
破壊的操作ではDeleteConfirmDialogを使用する。

## 6. DeleteConfirmDialog

### 6.1 用途

削除など元に戻しにくい操作に使用する。

対象:

- Project削除
- Flow削除
- Lane削除
- Stage削除
- Node削除
- Link削除
- Comment削除
- Snapshot削除

### 6.2 表示内容

- 削除対象名
- 削除影響
- 復元可否
- 関連要素への影響

### 6.3 Lane / Stage削除時

Lane / Stage削除では、内包Nodeの扱いを選択させる。

選択肢:

- 別Lane / Stageへ移動する。
- 内包Node / Link / Commentをまとめて削除する。
- キャンセルする。

## 7. UnsavedChangesDialog

### 7.1 用途

未保存変更がある状態で画面遷移する場合に表示する。

対象操作:

- Project切替
- Flow切替
- 画面離脱
- ブラウザ戻る
- ログアウト

### 7.2 選択肢

- 保存して移動
- 保存せず移動
- キャンセル

### 7.3 注意点

保存して移動を選択した場合、saveFlow成功後に遷移する。
保存失敗時は遷移しない。

## 8. ExportDialog

### 8.1 用途

Mermaid、PDF、JSON、AI DSLのExport条件を指定する。

### 8.2 入力項目

- Export形式
- 対象Flow
- Snapshot指定
- コメントを含めるか
- AI専用メモを含めるか
- Metadataを含めるか
- PDFページサイズ
- レイアウトモード

### 8.3 注意点

未保存変更がある場合は警告を表示する。
Export対象は保存済みFlowまたはSnapshotとする。

## 9. SnapshotCreateDialog

### 9.1 用途

Version Snapshotを作成する。

### 9.2 入力項目

- Snapshot名
- 説明
- 作成対象Flow

### 9.3 注意点

未保存変更がある場合は、保存後にSnapshot作成する導線を表示する。
未保存状態から直接Snapshotを作成しない。

## 10. TemplateApplyDialog

### 10.1 用途

Templateを現在のFlowへ適用する。

### 10.2 入力項目

- Template選択
- 適用対象
- 既存Flowへ追加するか
- 既存Flowを置き換えるか

### 10.3 注意点

Template適用時はID再採番する。
適用後はUndo対象にする。

## 11. AiReviewStartDialog

### 11.1 用途

AIレビュー実行前に対象とオプションを確認する。

### 11.2 入力項目

- 対象Flow
- Snapshot指定
- レビュー観点
- AI専用メモを含めるか
- コメントを含めるか

### 11.3 注意点

AIレビューは保存済みFlowまたはSnapshotを対象とする。
未保存状態では保存確認を表示する。

## 12. Dialog状態管理

uiStoreでDialog状態を管理する。

```ts
interface DialogState {
  activeDialog: string | null;
  dialogPayload?: unknown;
  loading: boolean;
  error?: string;
}
```

ただし、各Feature固有の複雑な入力状態はFeature側Storeで管理する。

## 13. キーボード操作

- Escapeで閉じる。
- 破壊的操作DialogではEscape可能だが、実行扱いにはしない。
- EnterでPrimary実行する場合は、誤操作が起きないDialogに限定する。
- Tab移動順を自然にする。

## 14. 禁止事項

- DialogごとにバラバラのFooter配置にする。
- 未保存変更を確認せずに遷移する。
- 削除影響を表示せず削除する。
- Export対象に未保存状態を直接使う。
- Snapshotを未保存状態から直接作成する。

## 15. テスト観点

- 未保存変更時にUnsavedChangesDialogが表示される。
- 保存して移動で保存成功後に遷移する。
- 保存失敗時に遷移しない。
- DeleteConfirmDialogで削除影響が表示される。
- ExportDialogで未保存警告が表示される。
- EscapeでDialogを閉じられる。

## 16. 完了条件

- Dialog分類と用途が明確である。
- 共通Dialog Component方針が定義されている。
- 未保存、削除、Export、Snapshot、Template、AIレビューのDialog仕様が定義されている。
- AIが本書を読んでDialog実装に着手できる。

# 19_04_Frontendテスト仕様

## 1. 目的

Vue3 / TypeScript / PrimeVue / Vue Flow / Piniaで構成されるFrontendのテスト仕様を定義する。

Frontend Testでは、Component表示、Store状態、権限制御、Editor操作、設定画面、Export操作を確認する。

## 2. 基本方針

- Component単位で表示・イベントを確認する
- Pinia Storeの状態遷移を確認する
- Permissionによる表示・非活性制御を確認する
- Editor操作後の構造化データ変更を確認する
- API 403時の再取得・readonly切替を確認する

## 3. Header / Toolbar

| Test | 条件 | 期待結果 |
| --- | --- | --- |
| Header_Owner_ShowsSaveVersionExport | Owner | Save/Version/Export表示 |
| Header_Viewer_DisablesSave | Viewer | Save非活性 |
| Toolbar_Editor_ShowsNodeTools | Editor | Node操作表示 |
| Toolbar_Viewer_HidesEditTools | Viewer | 編集Tool非表示 |
| Toolbar_Search_AlwaysAvailable | 全Role | Search利用可 |

## 4. Flow Canvas

| Test | 条件 | 期待結果 |
| --- | --- | --- |
| Canvas_Editor_CanMoveNode | Editor | Node移動可 |
| Canvas_Viewer_CannotMoveNode | Viewer | Node移動不可 |
| Canvas_AddNode_UpdatesStore | Node追加 | StoreにNode追加 |
| Canvas_AddLink_UpdatesStore | Link追加 | StoreにLink追加 |
| Canvas_LoopLink_Allowed | Loop接続 | Link作成可 |
| Canvas_InvalidLink_ShowsError | 不正Link | Error表示 |

## 5. Property Panel

| Test | 条件 | 期待結果 |
| --- | --- | --- |
| PropertyPanel_NodeSelected_ShowsNodeForm | Node選択 | Node情報表示 |
| PropertyPanel_Editor_CanEditLabel | Editor | Label編集可 |
| PropertyPanel_Viewer_Readonly | Viewer | 入力不可 |
| PropertyPanel_Save_UpdatesStore | 保存 | Store更新 |

## 6. Node Palette

| Test | 条件 | 期待結果 |
| --- | --- | --- |
| NodePalette_Editor_ShowsInitialNodes | Editor | Start/End/Process等表示 |
| NodePalette_Viewer_HiddenOrReadonly | Viewer | 追加不可 |
| NodePalette_DragProcess_CreatesNode | Process Drag | Node作成 |
| NodePalette_ImageNode_CreatesImageNode | Image Drag | Image Node作成 |

## 7. Settings画面

| Test | 条件 | 期待結果 |
| --- | --- | --- |
| UserSettings_LoginUser_CanEdit | ログイン済み | 編集可 |
| ProjectSettings_Editor_Readonly | Editor | readonly |
| ProjectSettings_Owner_CanSave | Owner | 保存可 |
| AiSettings_Viewer_Readonly | Viewer | readonly |
| ExportSettings_InvalidValue_ShowsValidation | 不正値 | Error表示 |
| Settings_ChangeValue_SetsDirty | 値変更 | isDirty=true |
| Settings_NavigateDirty_ShowsConfirm | 未保存遷移 | 確認Dialog |

## 8. Permission制御

| Test | 条件 | 期待結果 |
| --- | --- | --- |
| Button_NoPermission_Disabled | Permissionなし | disabled |
| Menu_Viewer_ShowsOnlyReadActions | Viewer | 詳細表示のみ |
| Dialog_NoPermission_DisablesExecute | 権限なし | 実行不可 |
| Api403_RefreshesPermission | API 403 | Permission再取得 |
| Api403_KeepsInputValue | API 403 | 入力値保持 |

## 9. Export操作

| Test | 条件 | 期待結果 |
| --- | --- | --- |
| ExportButton_Viewer_Visible | Viewer | Export表示 |
| ExportDialog_LoadsExportSetting | Dialog表示 | 設定反映 |
| ExportMermaid_Click_CallsApi | Mermaid実行 | API呼出 |
| ExportAiDsl_Click_CallsApi | AI DSL実行 | API呼出 |

## 10. 完了条件

- 主要Componentの表示・操作が確認されている
- Store状態遷移が確認されている
- Role別UI制御が確認されている
- Editor操作が構造化データへ反映されることが確認されている
- 設定画面のdirty/validation/403が確認されている

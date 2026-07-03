# 10_07_Frontend構成方針

## 1. 目的

本書は、AI Flow DesignerのFrontend構成方針を定義する。

Frontendは、モダンな設計ツールとしての編集体験を提供する。
ただし、Frontendの表示状態やVue Flow内部モデルはSSOTではない。
保存の正はBackendで検証された構造化データである。

## 2. 採用技術

- Vue3
- TypeScript
- PrimeVue
- Vue Flow
- Pinia
- Vite

## 3. 推奨フォルダ構成

```text
frontend/
 ├─ src/
 │   ├─ app/
 │   ├─ pages/
 │   ├─ layouts/
 │   ├─ components/
 │   ├─ features/
 │   │   ├─ project/
 │   │   ├─ flow/
 │   │   ├─ editor/
 │   │   ├─ export/
 │   │   └─ auth/
 │   ├─ stores/
 │   ├─ api/
 │   ├─ adapters/
 │   ├─ types/
 │   ├─ utils/
 │   └─ assets/
```

## 4. 画面構成

```text
Header
Toolbar
MainLayout
 ├─ NodePalette
 ├─ FlowCanvas
 └─ PropertyPanel
StatusBar
```

### 4.1 Header

責務:

- Project選択
- Flow選択
- 保存
- Version表示
- Export
- AIレビュー

### 4.2 Toolbar

責務:

- Undo
- Redo
- Zoom
- Grid
- Snap
- Template
- 検索

### 4.3 NodePalette

責務:

- 開始
- 終了
- 処理
- 判定
- 六角形
- 吹き出し
- 画像
- 将来追加Node

### 4.4 FlowCanvas

責務:

- Lane表示
- Stage表示
- Node配置
- Link接続
- Comment配置
- Drag編集
- Zoom / Pan

### 4.5 PropertyPanel

責務:

- 選択Node編集
- 選択Link編集
- 選択Comment編集
- Lane / Stage編集
- AI専用メモ編集

### 4.6 StatusBar

責務:

- 保存状態
- 検証エラー数
- 選択中要素情報
- 同期状態

## 5. Store方針

Pinia Storeは編集中状態を管理する。

主なStore:

- projectStore
- flowStore
- editorStore
- selectionStore
- undoRedoStore
- exportStore
- authStore

Storeは一時状態であり、DBの正ではない。
保存成功後はBackend Responseで再同期する。

## 6. Adapter方針

Frontendでは、SSOT構造とVue Flow表示モデルを分離する。

Adapter例:

- flowToVueFlowAdapter
- vueFlowToFlowCommandAdapter
- nodePropertyFormAdapter
- linkPropertyFormAdapter
- exportRequestAdapter

変換ルール:

- Flow構造からVue Flow Nodes / Edgesを生成する。
- Vue Flow操作結果は編集コマンドへ変換する。
- 編集コマンドをFlow Storeへ反映する。
- 保存時はFlow StoreからSaveFlowRequestを作成する。

## 7. API Client方針

API呼び出しはapi配下に集約する。

禁止事項:

- Componentからfetchやaxiosを直接散らばらせる。
- API ResponseをそのままComponentの深い場所で加工する。
- BackendのDomain Entity前提で画面を組む。

API Client例:

- projectApi
- flowApi
- snapshotApi
- exportApi
- templateApi
- authApi

## 8. Undo / Redo方針

Undo / RedoはFrontendの一時操作履歴である。
Version Snapshotとは別責務とする。

対象:

- Node追加
- Node移動
- Node削除
- Link追加
- Link削除
- Property変更
- Comment追加
- Lane / Stage編集

保存済みVersionを戻す処理はUndo / RedoではなくVersion機能で扱う。

## 9. 入力検証方針

Frontendでは、ユーザー体験向上のため即時検証を行う。

ただし、Frontend検証は最終検証ではない。
Backend保存時に必ず同等以上の検証を行う。

Frontend検証例:

- 必須入力
- Node名未入力
- Link条件未入力
- 存在しないLaneへの配置防止
- 画像未選択

## 10. 完了条件

- Vue Flow内部モデルをSSOTとして扱っていない。
- Storeが一時編集状態であることが明確である。
- API Clientが集約されている。
- Adapterで表示モデルと保存モデルが分離されている。
- Undo / RedoとVersion Snapshotが分離されている。

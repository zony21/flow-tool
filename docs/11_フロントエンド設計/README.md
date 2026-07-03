# 11_フロントエンド設計

## 1. 本書の位置付け

本フォルダは、AI Flow DesignerのFrontend実装設計を管理する。

Frontendは、ユーザーがProject / Flow / Lane / Stage / Node / Link / Commentを直感的に編集するための画面を提供する。
ただし、Frontendの表示状態やVue Flow内部モデルはSSOTではない。
保存の正は、Backendで検証された構造化データである。

## 2. 採用技術

- Vue3
- TypeScript
- PrimeVue
- Vue Flow
- Pinia
- Vite

## 3. 設計思想

- Figma / Miro / Notion / Linear のようなモダンUIを目指す。
- Excel風セル画面や手書き線中心のVisio風画面にはしない。
- Flow Canvasは編集体験を担うが、正はSSOTである。
- Vue FlowのNode / Edgeは表示用派生モデルとして扱う。
- Pinia Storeは編集中状態を管理し、保存後はBackend Responseで再同期する。
- Componentは責務を小さく分割し、API通信や変換処理を直接抱え込まない。

## 4. 正式設計書一覧

| ファイル | 内容 | 状態 |
| --- | --- | --- |
| `11_01_フロントエンド全体方針.md` | Frontend全体方針 | 追加済み |
| `11_02_ディレクトリ構成.md` | src配下構成・命名規則 | 追加済み |
| `11_03_レイアウト設計.md` | Header / Toolbar / Canvas / Palette / PropertyPanel | 追加済み |
| `11_04_画面遷移設計.md` | Project / Flow / Editor導線 | 追加済み |
| `11_05_Component設計.md` | Component分割・責務 | 追加済み |
| `11_06_Pinia設計.md` | Store分割・状態管理 | 追加済み |
| `11_07_VueFlow連携設計.md` | Vue Flow / SSOT Adapter | 追加済み |
| `11_08_API通信設計.md` | API Client / DTO / Error | 追加済み |
| `11_09_PropertyPanel設計.md` | Property Panel編集UI | 追加済み |
| `11_10_NodePalette設計.md` | Node Palette / Drag & Drop | 追加済み |
| `11_11_Dialog設計.md` | Dialog分類・状態管理 | 追加済み |
| `11_12_テーマUI設計.md` | Design Token / Theme / UI | 追加済み |
| `11_13_エラー表示設計.md` | Validation / API Error表示 | 追加済み |
| `11_14_パフォーマンス設計.md` | 大規模Flow・描画最適化 | 追加済み |
| `11_15_テスト設計.md` | Frontend Test方針 | 追加済み |

## 5. 今後追加する設計書

- `11_16_AIレビューUI設計.md`
- `11_17_ExportUI設計.md`
- `11_18_アクセシビリティ設計.md`
- `11_19_将来拡張設計.md`
- `11_20_フロントエンド設計まとめ.md`

## 6. 管理方針

`11_フロントエンド設計` 配下には正式設計書のみを残す。

作業メモ、draft、temp、old、更新概要、数行だけの旧設計書は正式設計書へ統合後に削除する。

## 7. 現在の状態

11章は、精度を担保できる範囲として `11_01`〜`11_15` を追加済み。

11章全体は未完了であり、次工程では `11_16_AIレビューUI設計` 以降を詳細化する。

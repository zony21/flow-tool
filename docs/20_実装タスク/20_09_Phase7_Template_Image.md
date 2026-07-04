# 20_09_Phase7_Template_Image

## 1. 目的

Template機能とImage管理機能を実装する。

TemplateはLane / Stage / Node / Link / Comment / Imageを保持し、適用時にIDを再採番する。

ImageはFlow内の画像参照とUpload管理を担う。

## 2. Task一覧

| Task ID | Task Name | 優先度 |
| --- | --- | --- |
| P7-001 | Template Entity / API | A |
| P7-002 | Template適用処理 | A |
| P7-003 | Template UI | B |
| P7-004 | Image Upload API | A |
| P7-005 | Image Node連携 | A |
| P7-006 | Image削除制御 | B |

## 3. P7-001 Template Entity / API

目的:

Templateを保存・取得できるようにする。

実装内容:

- TemplateEntity
- TemplateContent JSON
- GET /templates
- POST /templates
- PUT /templates/{templateId}
- DELETE /templates/{templateId}

関連設計:

- 15_テンプレート設計

テスト観点:

- EditorはTemplate作成可能
- ViewerはTemplate作成不可

完了条件:

- Templateを保存・取得できる

## 4. P7-002 Template適用処理

目的:

Template適用時にIDを再採番してFlowへ展開する。

実装内容:

- Node ID再採番
- Link source/target再解決
- Lane/Stage再採番
- Comment/Image参照再解決
- Flow Storeへ反映

関連設計:

- 15_テンプレート設計

テスト観点:

- 適用後IDが重複しない
- Linkが新IDを参照する

完了条件:

- TemplateをFlowへ安全に適用できる

## 5. P7-003 Template UI

目的:

画面からTemplateを選択・適用・登録できるようにする。

実装内容:

- TemplateDialog
- Template一覧
- 適用ボタン
- 登録ボタン

関連設計:

- 03_画面設計
- 15_テンプレート設計

テスト観点:

- Editorは適用可
- Viewerは適用不可

完了条件:

- Template UIから適用できる

## 6. P7-004 Image Upload API

目的:

画像をUploadしてFlow内で参照できるようにする。

実装内容:

- POST /images
- 画像メタデータ保存
- ファイル保存方式の初期実装
- 拡張子/MIME/サイズValidation

関連設計:

- 16_画像管理設計

テスト観点:

- 正常画像Upload成功
- 不正拡張子は400

完了条件:

- Image Upload APIが動作する

## 7. P7-005 Image Node連携

目的:

Image NodeからUpload済み画像を参照できるようにする。

実装内容:

- Image Node property
- imageId参照
- thumbnail表示
- AI DSL metadata出力準備

関連設計:

- 16_画像管理設計
- 09_AI構造化データ設計

テスト観点:

- Image Nodeに画像を紐付けられる

完了条件:

- Image Nodeが画像を表示できる

## 8. P7-006 Image削除制御

目的:

利用中画像の削除事故を防ぐ。

実装内容:

- 参照中Image確認
- 削除確認Dialog
- 参照中は削除不可または警告

関連設計:

- 16_画像管理設計

テスト観点:

- 参照中Image削除時に警告される

完了条件:

- Image削除ルールが実装されている

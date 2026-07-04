# 19_07_Exportテスト仕様

## 1. 目的

Mermaid、PDF、JSON、AI DSLなどのExport機能のテスト仕様を定義する。

ExportはAI Flow Designerの成果物であり、構造化データから正しく生成されることを確認する。

## 2. 基本方針

- Export元は構造化データとする
- 出力形式ごとに必須情報を確認する
- 設定値が出力へ反映されることを確認する
- ViewerのExport権限を確認する
- 生成失敗時のErrorを確認する

## 3. 共通Export観点

| 観点 | 内容 |
| --- | --- |
| 対象Flow | 正しいFlowが出力対象になっている |
| Node | Node ID / 種別 / Labelが含まれる |
| Link | Link元 / Link先 / 条件が含まれる |
| Lane | Lane情報が含まれる |
| Stage | Stage情報が含まれる |
| Comment | 設定に応じて含まれる |
| Image | 設定に応じて含まれる |
| Metadata | 設定に応じて含まれる |
| Version | 必要に応じてVersion情報が含まれる |

## 4. Mermaid Export

| Test | 条件 | 期待結果 |
| --- | --- | --- |
| Mermaid_BasicFlow_ContainsFlowchart | 基本Flow | flowchartを含む |
| Mermaid_DirectionLR_UsesLR | mermaidDirection=LR | flowchart LR |
| Mermaid_DirectionTB_UsesTB | mermaidDirection=TB | flowchart TB |
| Mermaid_Decision_IncludesBranch | Decision Node | 分岐Link出力 |
| Mermaid_Loop_IncludesLoopLink | Loopあり | 循環Link出力 |
| Mermaid_InvalidFlow_ReturnsError | 不正Flow | Error |

## 5. JSON Export

| Test | 条件 | 期待結果 |
| --- | --- | --- |
| Json_BasicFlow_IncludesProjectFlow | 基本Flow | Project / Flowあり |
| Json_IncludesNodesLinks | Node/Linkあり | nodes / linksあり |
| Json_PrettyPrintTrue_Formatted | prettyPrint=true | 改行・インデントあり |
| Json_PrettyPrintFalse_Minified | prettyPrint=false | minify |
| Json_MetadataEnabled_IncludesMetadata | includeMetadata=true | metadataあり |
| Json_MetadataDisabled_ExcludesMetadata | includeMetadata=false | metadataなし |

## 6. PDF Export

| Test | 条件 | 期待結果 |
| --- | --- | --- |
| Pdf_A4Landscape_UsesSetting | A4 landscape | 設定反映 |
| Pdf_A3Portrait_UsesSetting | A3 portrait | 設定反映 |
| Pdf_IncludeCommentsTrue_IncludesComments | コメント含有 | コメント出力 |
| Pdf_IncludeImagesTrue_IncludesImages | 画像含有 | 画像出力 |
| Pdf_EmptyFlow_ReturnsReadablePdf | Empty Flow | PDF生成可能 |

PDFは見た目の完全一致ではなく、必須情報の存在とページ設定を優先して確認する。

## 7. AI DSL Export

AI DSLの詳細は19_08_AI_DSLテスト仕様で定義する。

Export側では以下を確認する。

| Test | 条件 | 期待結果 |
| --- | --- | --- |
| AiDsl_ValidFlow_ReturnsDsl | 正常Flow | DSL出力 |
| AiDsl_FormatVersionV1_UsesV1 | v1指定 | version=v1 |
| AiDsl_DetailLevelDetailed_IncludesDetails | detailed | 詳細情報あり |
| AiDsl_IncludeMetadataFalse_ExcludesMetadata | metadata=false | metadataなし |

## 8. 権限テスト

| Test | 条件 | 期待結果 |
| --- | --- | --- |
| Export_Owner_Returns200 | Owner | 200 |
| Export_Editor_Returns200 | Editor | 200 |
| Export_ViewerAllowed_Returns200 | Viewer許可 | 200 |
| Export_ViewerDenied_Returns403 | Viewer禁止 | 403 |
| Export_NotMember_Returns403 | 未参加 | 403 |

## 9. Error Test

| Test | 条件 | 期待結果 |
| --- | --- | --- |
| Export_MissingFlow_Returns404 | Flowなし | 404 |
| Export_InvalidFormat_Returns400 | 未対応形式 | 400 |
| Export_InvalidSetting_Returns400 | 設定不正 | 400 |
| Export_NoPermission_Returns403 | 権限なし | 403 |

## 10. 完了条件

- Mermaid / PDF / JSON / AI DSLの主要出力が確認されている
- Export Settingの反映が確認されている
- 権限制御が確認されている
- 異常系Errorが確認されている

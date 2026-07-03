# 07_07_Stage変換仕様

## 1. 本書の目的

StageをMermaidへ変換する仕様を定義する。
Stageは工程を表し、処理がどの段階で行われるかを示す。

## 2. flowchartでの扱い

Stageはコメント、区切り、またはsubgraphとして表現する。
初期実装ではLane subgraphを優先し、StageはNodeラベルまたはコメントで補足する。

## 3. sequenceDiagramでの扱い

StageはNoteまたはコメントとして表現する。
Mermaid sequenceDiagramは工程レーンを直接表しにくいため、補助情報として扱う。

## 4. 表示順

StageのdisplayOrderに従ってNodeを並べる。

## 5. 出力例

```mermaid
%% Stage: RFID読取り
N001[RFID読取り]
```

## 6. テスト観点

- Stage名が出力から失われないこと
- Stage表示順がNode並びに反映されること
- sequenceDiagramでもStage補足が残ること

## 7. 完了条件

Stageの工程情報がMermaid出力で確認できること。

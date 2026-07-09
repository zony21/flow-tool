# 05_06_Template_Export_API設計

## Transport搬送表生成 追加仕様

## Stage分類

Transport FlowではStageは単なる設備分類ではない。

Stageは処理主体を表す。

例:

- AGF / AGV
- PLC
- WCS
- コンベア
- シャッター
- タブレット
- 有人作業

## Stage種別

Stageに種別を保持する。

stageType:

- AUTO（設備・システム制御）
- MANUAL（有人作業）

例:

Stage: 作業者
stageType: MANUAL

Stage: AGV
stageType: AUTO


## 搬送表生成対象

搬送テンプレートはAGF/AGVに実行させる順序表である。

そのため有人作業はフロー上には表示するが、搬送表には出力しない。

ルール:

- StageType = AUTO
  - 搬送表出力対象

- StageType = MANUAL
  - 搬送表出力対象外


例:

Flow:

Lane: 倉庫 → 包装機搬送

1. 作業者: パレット準備
2. 作業者: タブレット完了報告
3. AGV: A1へ移動
4. AGV: 荷上げ
5. PLC: シャッター開指令
6. AGV: 搬送開始


搬送表生成結果:

# 倉庫 → 包装機搬送

|No|処理|動作|ロケ|設備|R/W|処理区分|
|-|-|-|-|-|-|-|
|1|A1へ移動|TravelToPosture|A1|-|-|移動|
|2|荷上げ|Loading|A1|-|-|荷上げ|
|3|シャッター開指令|-|-|SH1|Write|PLC書込|
|4|搬送開始|TravelToPosture|P1|-|-|移動|


## 生成順序

1. FlowType = TRANSPORT確認
2. Lane単位で搬送パターン分割
3. Lane内NodeをLink順解析
4. Node所属Stage取得
5. StageType=MANUALを除外
6. AUTO NodeのみStep生成
7. Lane単位でNo採番


重要:

フロー図:
人作業 + 設備 + システムを含める

搬送テンプレート:
AGF/AGV動作に必要な設備・システム処理のみ出力する

# AI Flow Designer フロー種別・AGF/AGV搬送設計拡張仕様

## 目的

通常のシステムフローとAGF/AGV搬送フローを分離する。

基本思想:
Single Source of Truth

構造データを正とし、表示・出力は生成結果とする。

---

## フロー種別

FlowにflowTypeを追加する。

種類:

- Normal: 通常フロー
- Transport: AGF/AGV搬送フロー

既存フローはNormal扱い。

---

## Normal（通常フロー）

対象:

- 業務フロー
- システム処理
- 設備処理
- 通信フロー

利用可能:

- Node
- Link
- Lane
- Stage
- Comment
- Mermaid出力
- JSON出力
- AI DSL出力
- 設計書出力
- API仕様出力

利用不可:

- 搬送表生成
- AGFメーカー設定
- AGFコマンド設定

---

## Transport（AGF/AGV搬送フロー）

Normal機能を継承する。

追加機能:

- AGF/AGVメーカー管理
- コマンド管理
- ロケーション管理
- 設備管理
- 搬送表生成

---

## 新規フロー作成

作成時に選択する。

デフォルト:
Normal

選択:

- 通常フロー
- AGF/AGV搬送フロー

---

## AGFメーカー管理

Transportのみ。

メーカーごとにコマンドを保持する。

例:

- Mujin
- Toyota
- Nichiyu

---

## Command管理

メーカー単位。

例:

|Command|処理区分|
|-|-|
|TravelToPosture|移動|
|Loading|荷上げ|
|Unloading|荷下ろし|

ユーザー追加可能。

---

## Location管理

搬送位置を構造管理する。

例:

|ID|種類|
|-|-|
|P1|経由点|
|A1|荷役場所|
|ST1|充電位置|

---

## Equipment管理

Lane/Stageとは別管理。

対象:

- PLC
- RCS
- WCS
- AGF
- AGV
- AMR
- コンベア
- シャッター
- ロボット
- センサー
- 安全機器
- その他

---

## Transport Node拡張

Transportの場合のみ追加。

追加項目:

- commandId
- locationId
- equipmentId
- rwType

---

## R/W

PLC通信方向。

値:

- None
- Read
- Write

---

## 処理区分自動判定

優先順:

1. Command設定
2. R/W設定

例:

TravelToPosture → 移動

Loading → 荷上げ

Write → PLC書込み

Read → PLC読込み

---

## 搬送表生成

Transportのみ使用可能。

流れ:

Flow
↓
Node順解析
↓
Link追跡
↓
Step生成

---

## Markdown出力例

|No|処理|動作|ロケ|設備|R/W|処理区分|
|-|-|-|-|-|-|-|
|1|A1占有ON|-|-|-|-|DB更新|
|2|SH1開指令|-|-|SH1|Write|PLC書込|
|3|A1荷上げ|Loading|A1|-|-|荷上げ|
|4|P2移動|TravelToPosture|P2|-|-|移動|

---

## Export

Normal:

- Mermaid
- JSON
- AI DSL
- 設計書
- API仕様

Transport追加:

- 搬送表

---

## 実装Phase

### Phase1
FlowType追加

### Phase2
Transportマスタ追加

- Manufacturer
- Command
- Location
- Equipment

### Phase3
Node詳細拡張

### Phase4
搬送表Generator追加

### Phase5
Markdown出力追加

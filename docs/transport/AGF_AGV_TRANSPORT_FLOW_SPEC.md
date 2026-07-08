# AGF/AGV搬送フロー設計仕様

## 1. 本書の目的

本書は、AI Flow DesignerにおけるAGF/AGV搬送設計機能の仕様を定義する。

通常の業務フロー・システムフローと、AGF/AGV固有の搬送フローを分離し、汎用性を維持したまま搬送設計を可能にする。

本機能もSingle Source of Truthを原則とし、構造化Flowデータを正として各種資料を生成する。

## 2. 基本方針

Flowには種別を保持する。

- Normal: 通常フロー
- Transport: AGF/AGV搬送フロー

既存FlowはNormalとして扱う。

TransportはNormalを拡張する形式とし、通常機能を破壊しない。

## 3. 通常フロー（Normal）

通常フローは以下を対象とする。

- 業務処理
- システム処理
- 設備連携
- 通信処理

利用可能機能:

- Node / Link / Lane / Stage / Comment編集
- Mermaid出力
- JSON出力
- AI DSL出力
- 設計書出力
- API仕様出力

AGF/AGV専用設定、搬送表生成は利用不可とする。

## 4. AGF/AGV搬送フロー（Transport）

TransportではNormal機能に加えて以下を利用可能にする。

- AGF/AGVメーカー管理
- メーカー別コマンド管理
- ロケーション管理
- 設備管理
- 搬送表生成

## 5. フロー作成仕様

新規Flow作成時にFlow種別を選択する。

初期値はNormalとする。

選択項目:

- 通常フロー
- AGF/AGV搬送フロー

NormalからTransportへの変更は可能とする。

変更時は、メーカー・コマンド・ロケーション・設備情報の追加設定が必要になることを通知する。

## 6. メーカー管理

Transportのみ使用する。

AGF/AGVメーカーごとに使用可能な動作コマンドを管理する。

例:

- Mujin
- Toyota
- Nichiyu

## 7. コマンド管理

メーカー単位で搬送コマンドを保持する。

| コマンド | 処理区分 |
| --- | --- |
| TravelToPosture | 移動 |
| Loading | 荷上げ |
| Unloading | 荷下ろし |

コマンドはユーザー追加可能とする。

## 8. ロケーション管理

搬送位置情報を構造化して管理する。

| ロケーション | 種類 |
| --- | --- |
| P1 | 経由点 |
| A1 | 荷役場所 |
| ST1 | 充電位置 |

## 9. 設備管理

Lane / Stageとは別に実設備情報を管理する。

対象例:

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

## 10. Node拡張仕様

Transportの場合のみNodeへ搬送属性を追加する。

追加項目:

- commandId
- locationId
- equipmentId
- rwType

通常Flowでは表示しない。

## 11. 通信方向（R/W）

PLC等との通信方向を表す。

値:

- None
- Read
- Write

## 12. 処理区分判定

処理区分は可能な限り自動判定する。

判定優先順:

1. Command設定
2. R/W設定

例:

- TravelToPosture → 移動
- Loading → 荷上げ
- Write → PLC書込み
- Read → PLC読込み

## 13. 搬送表生成仕様

Transportのみ搬送表出力を許可する。

生成順:

1. Flow取得
2. Link順解析
3. Node実行順決定
4. Step番号採番
5. 搬送表生成

## 14. Markdown出力仕様

出力例:

| No | 処理 | 動作 | ロケ | 設備 | R/W | 処理区分 |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | A1占有ON | - | - | - | - | DB更新 |
| 2 | SH1開指令 | - | - | SH1 | Write | PLC書込 |
| 3 | A1荷上げ | Loading | A1 | - | - | 荷上げ |
| 4 | P2移動 | TravelToPosture | P2 | - | - | 移動 |

## 15. Export仕様

Normal:

- Mermaid
- JSON
- AI DSL
- 設計書
- API仕様

Transport追加:

- 搬送表

## 16. 実装Phase

Phase1:

- FlowType追加

Phase2:

- Manufacturer管理
- Command管理
- Location管理
- Equipment管理

Phase3:

- Node詳細拡張

Phase4:

- 搬送表Generator追加

Phase5:

- Markdown出力追加

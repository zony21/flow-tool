# 07_14_Subgraph設計

## 1. 本書の目的

Mermaid flowchartにおけるsubgraph設計を定義する。
LaneやStageを構造として表現し、担当や工程を見失わない出力にする。

## 2. 基本方針

初期実装ではLaneをsubgraphとして出力する。
StageはNodeラベルやコメントで補足する。

## 3. Lane subgraph

```mermaid
subgraph LANE_001[包装PLC]
  N001[RFID読取り]
end
```

## 4. Stage subgraph

Stage subgraphは将来オプションとする。
LaneとStageの両方をsubgraph化すると階層が深くなり、Mermaid表示が崩れる可能性があるためである。

## 5. 表示順

Lane displayOrderに従ってsubgraphを出力する。
NodeはStage displayOrder、Node displayOrderで並べる。

## 6. sequenceDiagramとの違い

sequenceDiagramではsubgraphを使わず、Laneをparticipantとして扱う。

## 7. テスト観点

- Laneがsubgraphとして出力されること
- Nodeが所属Lane内に出力されること
- 表示順が安定すること

## 8. 完了条件

subgraphにより担当単位がMermaid上で明確になること。

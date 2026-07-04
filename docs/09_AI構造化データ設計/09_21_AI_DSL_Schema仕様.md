# 09_21_AI_DSL_Schema仕様

## 1. 目的

AI DSLの正式Schemaを定義する。

AI DSLは保存形式であるJSONとは別に、AIがFlowを曖昧なく解析するための理解形式である。

本仕様では、AI DSLの必須構造、Node種別別表現、条件分岐、Loop、責務、外部通信、DB/API影響、Validation結果を定義する。

## 2. 基本方針

- JSON保存形式とAI DSLを分離する
- AI DSLはAI理解を最優先する
- すべてのIDは元のSSOT構造化データと対応可能にする
- Decision、Loop、外部通信、DB操作は省略しない
- 不完全なFlowも出力可能にし、欠落はwarningとして表現する
- DSL Versionを必ず持つ

## 3. DSL全体構造

```text
AI_FLOW_DSL v1
PROJECT
FLOW
LANES
STAGES
NODES
LINKS
RESPONSIBILITIES
DATA_EFFECTS
COMMUNICATIONS
VALIDATION
METADATA
END
```

## 4. 必須セクション

| セクション | 必須 | 説明 |
| --- | --- | --- |
| DSL_VERSION | ○ | DSL形式Version |
| PROJECT | ○ | Project情報 |
| FLOW | ○ | Flow情報 |
| NODES | ○ | Node一覧 |
| LINKS | ○ | Link一覧 |
| VALIDATION | ○ | Warning/Error一覧 |
| METADATA | ○ | 生成日時、生成条件 |

LANES、STAGES、RESPONSIBILITIES、DATA_EFFECTS、COMMUNICATIONSは該当データがない場合でも空として出力する。

## 5. DSL Version

初期Versionはv1とする。

```text
DSL_VERSION: v1
```

Version変更ルール:

- 後方互換を壊す変更はv2へ上げる
- 項目追加のみならminor扱いとしてmetadataに記録する
- 既存項目の意味変更は禁止

## 6. Node共通Schema

```text
NODE {
  id: string
  type: string
  label: string
  laneId: string | null
  stageId: string | null
  responsibility: string | null
  description: string | null
  properties: object
  warnings: string[]
}
```

必須項目:

- id
- type
- label

## 7. Node Type別Schema

### 7.1 Start

```text
NODE Start {
  id
  label
  nextLinks
}
```

### 7.2 End

```text
NODE End {
  id
  label
  incomingLinks
}
```

### 7.3 Process

```text
NODE Process {
  id
  label
  action
  input
  output
  owner
}
```

### 7.4 Decision

```text
NODE Decision {
  id
  label
  condition
  branches[] {
    label
    expression
    targetNodeId
  }
  missingConditionWarning
}
```

Decision Nodeはconditionまたはbranch expressionを持つ必要がある。

不足する場合はVALIDATIONへwarningを出力する。

### 7.5 Hexagon

Hexagonは外部接続、外部システム、特殊処理を表す。

```text
NODE Hexagon {
  id
  label
  externalType
  targetSystem
  communicationType
}
```

### 7.6 Image

```text
NODE Image {
  id
  label
  imageId
  fileName
  mimeType
  purpose
}
```

### 7.7 Comment

```text
NODE Comment {
  id
  text
  linkedNodeId
  position
}
```

## 8. Link Schema

```text
LINK {
  id: string
  sourceNodeId: string
  targetNodeId: string
  label: string | null
  condition: string | null
  linkType: bezier | straight | step | smoothstep
  isLoop: boolean
  warnings: string[]
}
```

sourceNodeIdとtargetNodeIdはNODESに存在する必要がある。

## 9. Loop表現

LoopはLinkのisLoop=trueで表す。

```text
LINK {
  sourceNodeId: decision-001
  targetNodeId: process-001
  condition: retry
  isLoop: true
}
```

AI DSLではLoopを展開しない。

Loop関係として明示し、無限処理として解釈しないようにする。

## 10. Responsibility表現

責務はLane、Node、Commentから判断できるようにする。

```text
RESPONSIBILITY {
  nodeId
  laneId
  roleName
  responsibilityType
}
```

NodeがLaneに所属していない場合はwarningを出力する。

## 11. DB影響表現

DB操作NodeまたはMetadataからDB影響を表現する。

```text
DATA_EFFECT {
  nodeId
  tableName
  operation: select | insert | update | delete | upsert
  keyFields[]
  affectedFields[]
}
```

## 12. API通信表現

```text
COMMUNICATION {
  nodeId
  type: api
  method
  endpoint
  request
  response
  targetSystem
}
```

## 13. PLC通信表現

```text
COMMUNICATION {
  nodeId
  type: plc
  signalName
  direction: send | receive
  onValue
  offValue
  targetDevice
}
```

## 14. Validation表現

```text
VALIDATION {
  level: warning | error
  code
  message
  targetType
  targetId
}
```

代表例:

| code | level | 内容 |
| --- | --- | --- |
| MISSING_DECISION_CONDITION | warning | Decision条件がない |
| LINK_TARGET_NOT_FOUND | error | Link先Nodeが存在しない |
| NODE_WITHOUT_LANE | warning | NodeがLane未所属 |
| EXTERNAL_TARGET_MISSING | warning | 外部通信先がない |

## 15. AI Instruction

AI DSLをAIへ渡す場合、以下のInstructionを付与する。

```text
このDSLはシステム設計Flowを表す。
図形の見た目ではなく、NODE、LINK、RESPONSIBILITY、DATA_EFFECT、COMMUNICATIONを正として解析する。
VALIDATIONのwarning/errorは設計上の不備候補として扱う。
Loopは明示された循環であり、無限展開してはならない。
```

## 16. 完了条件

- AI DSLの必須セクションが定義されている
- Node Type別Schemaが定義されている
- Decision、Loop、DB、API、PLC通信が表現できる
- Validation warning/errorが表現できる
- AIへ渡すInstructionが定義されている

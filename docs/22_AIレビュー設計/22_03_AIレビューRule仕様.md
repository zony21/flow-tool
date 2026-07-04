# 22_03_AIレビューRule仕様

## 1. 目的

AI Reviewで利用するRule仕様を定義する。

RuleはAIへの曖昧な依頼ではなく、検出観点を構造化して管理するために利用する。

## 2. Rule Schema

```json
{
  "ruleCode": "MISSING_DECISION_CONDITION",
  "category": "condition",
  "level": "warning",
  "description": "Decision Nodeに条件が設定されているか確認する。",
  "targetTypes": ["decision"],
  "enabled": true
}
```

## 3. 初期Rule

| ruleCode | category | level | 内容 |
| --- | --- | --- | --- |
| MISSING_DECISION_CONDITION | condition | warning | Decision条件なし |
| LOOP_EXIT_CONDITION_MISSING | condition | warning | Loop終了条件なし |
| NODE_WITHOUT_RESPONSIBILITY | responsibility | warning | 責務不明 |
| DB_EFFECT_MISSING | db | suggestion | DB影響記載不足 |
| API_EFFECT_MISSING | api | suggestion | API影響記載不足 |
| COMMUNICATION_TARGET_MISSING | communication | warning | 通信先不明 |
| ERROR_HANDLING_MISSING | error_handling | suggestion | Error処理不足 |
| TEST_VIEWPOINT_MISSING | test | suggestion | Test観点不足 |

## 4. Strictness

AI SettingのreviewStrictnessで検出強度を変える。

| strictness | 内容 |
| --- | --- |
| low | error中心 |
| normal | error + warning |
| high | error + warning + suggestion |

## 5. 完了条件

- Rule Schemaが定義されている
- 初期Ruleが定義されている
- Strictnessとの関係が定義されている

export type NodeSample = {
  type: string
  label: string
  defaultName: string
  description: string
}

export const nodeSamples: NodeSample[] = [
  {
    type: 'start',
    label: '開始',
    defaultName: '開始',
    description: '処理の開始点を表します。',
  },
  {
    type: 'process',
    label: '処理',
    defaultName: '処理',
    description: '通常の作業・処理を表します。',
  },
  {
    type: 'decision',
    label: '判定',
    defaultName: '判定',
    description: '条件分岐や判断を表します。',
  },
  {
    type: 'document',
    label: '帳票・データ',
    defaultName: '帳票・データ',
    description: '帳票、ファイル、データ出力を表します。',
  },
  {
    type: 'wait',
    label: '待機',
    defaultName: '待機',
    description: '待機、保留、応答待ちを表します。',
  },
  {
    type: 'end',
    label: '終了',
    defaultName: '終了',
    description: '処理の終了点を表します。',
  },
]

export function getNodeSample(type: string): NodeSample {
  return nodeSamples.find((sample) => sample.type === type) ?? nodeSamples[1]
}

export function getNodeTypeLabel(type: string): string {
  return getNodeSample(type).label
}

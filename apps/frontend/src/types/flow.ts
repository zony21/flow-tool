import type { FlowNode } from './node'

export type FlowDetail = {
  flowId: string
  name: string
  nodes: FlowNode[]
}

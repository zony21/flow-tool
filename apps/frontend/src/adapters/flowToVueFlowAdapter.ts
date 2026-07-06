import type { FlowDetail } from '../types/flow'

export type VueFlowNode = {
  id: string
  label: string
}

export function flowToVueFlowAdapter(flow: FlowDetail): VueFlowNode[] {
  return flow.nodes.map((node) => ({
    id: node.nodeId,
    label: node.name,
  }))
}

import type { FlowDetail } from '../types/flow'

export type SaveFlowRequest = {
  flowId: string
  name: string
}

export function flowToSaveRequestAdapter(flow: FlowDetail): SaveFlowRequest {
  return {
    flowId: flow.flowId,
    name: flow.name,
  }
}

import { httpClient } from './httpClient'
import type { FlowDetail } from '../types/flow'

export async function fetchFlow(flowId: string): Promise<FlowDetail> {
  const response = await httpClient.get<FlowDetail>(`/api/flows/${flowId}`)
  return response.data
}

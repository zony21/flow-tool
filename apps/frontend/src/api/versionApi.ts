import { httpClient } from './httpClient'
import type { CreateFlowVersionRequest, FlowVersionSummary } from '../types/version'

export async function fetchFlowVersions(projectId: string, flowId: string): Promise<FlowVersionSummary[]> {
  const response = await httpClient.get<FlowVersionSummary[]>(`/api/projects/${projectId}/flows/${flowId}/versions`)
  return response.data
}

export async function createFlowVersion(projectId: string, flowId: string, request: CreateFlowVersionRequest): Promise<FlowVersionSummary> {
  const response = await httpClient.post<FlowVersionSummary>(`/api/projects/${projectId}/flows/${flowId}/versions`, request)
  return response.data
}

import { httpClient } from './httpClient'
import type {
  CreateFlowVersionRequest,
  FlowVersionCompareResponse,
  FlowVersionSummary,
  RestoreFlowVersionResponse,
} from '../types/version'

export async function fetchFlowVersions(projectId: string, flowId: string): Promise<FlowVersionSummary[]> {
  const response = await httpClient.get<FlowVersionSummary[]>(`/api/projects/${projectId}/flows/${flowId}/versions`)
  return response.data
}

export async function createFlowVersion(projectId: string, flowId: string, request: CreateFlowVersionRequest): Promise<FlowVersionSummary> {
  const response = await httpClient.post<FlowVersionSummary>(`/api/projects/${projectId}/flows/${flowId}/versions`, request)
  return response.data
}

export async function restoreFlowVersion(projectId: string, flowId: string, versionId: string): Promise<RestoreFlowVersionResponse> {
  const response = await httpClient.post<RestoreFlowVersionResponse>(`/api/projects/${projectId}/flows/${flowId}/versions/${versionId}/restore`)
  return response.data
}

export async function compareFlowVersions(projectId: string, flowId: string, leftVersionId: string, rightVersionId: string): Promise<FlowVersionCompareResponse> {
  const response = await httpClient.get<FlowVersionCompareResponse>(`/api/projects/${projectId}/flows/${flowId}/versions/compare`, {
    params: {
      leftVersionId,
      rightVersionId,
    },
  })
  return response.data
}

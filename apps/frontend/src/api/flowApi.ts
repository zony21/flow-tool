import { httpClient } from './httpClient'
import type { FlowDetail, FlowSummary, SaveFlowStructureRequest, SaveFlowStructureResponse } from '../types/flow'

export type FlowSaveRequest = {
  name: string
  description?: string | null
}

export async function fetchFlows(projectId: string): Promise<FlowSummary[]> {
  const response = await httpClient.get<FlowSummary[]>(`/api/projects/${projectId}/flows`)
  return response.data
}

export async function fetchFlow(projectId: string, flowId: string): Promise<FlowDetail> {
  const response = await httpClient.get<FlowDetail>(`/api/projects/${projectId}/flows/${flowId}`)
  return response.data
}

export async function createFlow(projectId: string, request: FlowSaveRequest): Promise<FlowDetail> {
  const response = await httpClient.post<FlowDetail>(`/api/projects/${projectId}/flows`, request)
  return response.data
}

export async function updateFlow(projectId: string, flowId: string, request: FlowSaveRequest): Promise<FlowDetail> {
  const response = await httpClient.put<FlowDetail>(`/api/projects/${projectId}/flows/${flowId}`, request)
  return response.data
}

export async function duplicateFlow(projectId: string, flowId: string): Promise<FlowDetail> {
  const response = await httpClient.post<FlowDetail>(`/api/projects/${projectId}/flows/${flowId}/duplicate`, {})
  return response.data
}

export async function deleteFlow(projectId: string, flowId: string): Promise<void> {
  await httpClient.delete(`/api/projects/${projectId}/flows/${flowId}`)
}

export async function saveFlowStructure(projectId: string, flowId: string, request: SaveFlowStructureRequest): Promise<SaveFlowStructureResponse> {
  const response = await httpClient.put<SaveFlowStructureResponse>(`/api/projects/${projectId}/flows/${flowId}/structure`, request)
  return response.data
}

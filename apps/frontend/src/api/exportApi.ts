import { httpClient } from './httpClient'

export type MermaidExportRequest = {
  type?: 'flowchart'
  direction?: 'TD' | 'TB' | 'BT' | 'LR' | 'RL'
  includeComments?: boolean
}

export type TextExportResponse = {
  fileName: string
  content: string
}

export async function exportJson(projectId: string, flowId: string): Promise<Blob> {
  const response = await httpClient.post(`/api/projects/${projectId}/flows/${flowId}/export/json`, undefined, {
    responseType: 'blob',
  })

  return response.data
}

export async function exportMermaid(
  projectId: string,
  flowId: string,
  request: MermaidExportRequest = { type: 'flowchart', direction: 'TD', includeComments: true },
): Promise<TextExportResponse> {
  const response = await httpClient.post<TextExportResponse>(`/api/projects/${projectId}/flows/${flowId}/export/mermaid`, request)
  return response.data
}

export async function exportAiDsl(projectId: string, flowId: string): Promise<TextExportResponse> {
  const response = await httpClient.post<TextExportResponse>(`/api/projects/${projectId}/flows/${flowId}/export/ai-dsl-v2`)
  return response.data
}

export async function exportDesignDocument(projectId: string, flowId: string): Promise<TextExportResponse> {
  const response = await httpClient.post<TextExportResponse>(`/api/projects/${projectId}/flows/${flowId}/export/design-doc`)
  return response.data
}

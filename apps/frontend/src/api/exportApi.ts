import { httpClient } from './httpClient'

export async function exportJson(flowId: string): Promise<Blob> {
  const response = await httpClient.get(`/api/flows/${flowId}/export/json`, {
    responseType: 'blob',
  })

  return response.data
}

import { httpClient } from './httpClient'

export async function fetchTemplates(): Promise<Array<{ templateId: string; name: string }>> {
  const response = await httpClient.get<Array<{ templateId: string; name: string }>>('/api/templates')
  return response.data
}

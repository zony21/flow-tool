import { httpClient } from './httpClient'
import type { ProjectSummary } from '../types/project'

export async function fetchProjects(): Promise<ProjectSummary[]> {
  const response = await httpClient.get<ProjectSummary[]>('/api/projects')
  return response.data
}

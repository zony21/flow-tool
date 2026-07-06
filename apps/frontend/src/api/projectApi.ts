import { httpClient } from './httpClient'
import type { ProjectDetail, ProjectSummary } from '../types/project'

export type ProjectSaveRequest = {
  name: string
  description?: string | null
}

export async function fetchProjects(): Promise<ProjectSummary[]> {
  const response = await httpClient.get<ProjectSummary[]>('/api/projects')
  return response.data
}

export async function fetchProject(projectId: string): Promise<ProjectDetail> {
  const response = await httpClient.get<ProjectDetail>(`/api/projects/${projectId}`)
  return response.data
}

export async function createProject(request: ProjectSaveRequest): Promise<ProjectDetail> {
  const response = await httpClient.post<ProjectDetail>('/api/projects', request)
  return response.data
}

export async function updateProject(projectId: string, request: ProjectSaveRequest): Promise<ProjectDetail> {
  const response = await httpClient.put<ProjectDetail>(`/api/projects/${projectId}`, request)
  return response.data
}

export async function deleteProject(projectId: string): Promise<void> {
  await httpClient.delete(`/api/projects/${projectId}`)
}

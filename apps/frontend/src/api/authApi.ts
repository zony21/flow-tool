import { httpClient } from './httpClient'
import type { CurrentUser, ProjectPermission } from '../types/auth'

export async function getLoginUrl(): Promise<{ url: string }> {
  const response = await httpClient.get<{ url: string }>('/api/auth/github/login-url')
  return response.data
}

export async function fetchCurrentUser(): Promise<CurrentUser> {
  const response = await httpClient.get<CurrentUser>('/api/auth/me', { withCredentials: true })
  return response.data
}

export async function loginWithDemo(): Promise<CurrentUser> {
  const response = await httpClient.get<CurrentUser>('/api/auth/github/callback?demo=true', {
    withCredentials: true,
  })
  return response.data
}

export async function logout(): Promise<void> {
  await httpClient.post('/api/auth/logout', undefined, { withCredentials: true })
}

export async function fetchProjectPermission(projectId: string): Promise<ProjectPermission> {
  const response = await httpClient.get<ProjectPermission>(`/api/projects/${projectId}/permissions/me`, {
    withCredentials: true,
  })
  return response.data
}

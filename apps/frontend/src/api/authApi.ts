import { httpClient } from './httpClient'

export async function getLoginUrl(): Promise<{ url: string }> {
  const response = await httpClient.get<{ url: string }>('/api/auth/github/login-url')
  return response.data
}

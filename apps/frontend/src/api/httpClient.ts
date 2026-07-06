import axios from 'axios'

export const httpClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000',
  timeout: 15000,
  withCredentials: true,
})

httpClient.interceptors.response.use(
  (response) => response,
  (error) => Promise.reject(error),
)

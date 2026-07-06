export type ApiErrorDetails = Record<string, string[]>

export type ApiErrorResponse = {
  code: string
  message: string
  traceId: string
  details?: ApiErrorDetails | null
}

export type NormalizedApiError = {
  code: string
  message: string
  traceId?: string
  details?: ApiErrorDetails | null
  status?: number
}

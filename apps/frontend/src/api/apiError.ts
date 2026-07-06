import axios from 'axios'
import type { ApiErrorResponse, NormalizedApiError } from '../types/apiError'

export function isApiErrorResponse(value: unknown): value is ApiErrorResponse {
  if (!value || typeof value !== 'object') {
    return false
  }

  const candidate = value as Partial<ApiErrorResponse>
  return typeof candidate.code === 'string'
    && typeof candidate.message === 'string'
    && typeof candidate.traceId === 'string'
}

export function normalizeApiError(error: unknown): NormalizedApiError {
  if (axios.isAxiosError(error)) {
    const status = error.response?.status
    const data = error.response?.data

    if (isApiErrorResponse(data)) {
      return {
        code: data.code,
        message: data.message,
        traceId: data.traceId,
        details: data.details,
        status,
      }
    }

    return {
      code: status === 401 ? 'UNAUTHORIZED' : 'HTTP_ERROR',
      message: error.message || 'API request failed.',
      status,
    }
  }

  if (error instanceof Error) {
    return {
      code: 'CLIENT_ERROR',
      message: error.message,
    }
  }

  return {
    code: 'UNKNOWN_ERROR',
    message: 'Unexpected error occurred.',
  }
}

export function getValidationMessages(error: NormalizedApiError, field: string): string[] {
  return error.details?.[field] ?? []
}

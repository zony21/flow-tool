export type ExportRequest = {
  flowId: string
  format: 'mermaid' | 'pdf' | 'json'
}

export function exportRequestAdapter(flowId: string, format: ExportRequest['format']): ExportRequest {
  return { flowId, format }
}

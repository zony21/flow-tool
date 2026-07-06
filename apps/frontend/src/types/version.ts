export type FlowVersionSummary = {
  versionId: string
  flowId: string
  versionNumber: number
  displayVersion: string
  comment?: string | null
  createdAtUtc: string
  nodeCount: number
  linkCount: number
  commentCount: number
}

export type CreateFlowVersionRequest = {
  comment?: string | null
}

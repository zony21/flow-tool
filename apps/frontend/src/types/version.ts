export type FlowVersionSummary = {
  versionId: string
  flowId: string
  versionNumber: number
  displayVersion: string
  comment?: string | null
  createdByDisplayName?: string | null
  createdAtUtc: string
  nodeCount: number
  linkCount: number
  commentCount: number
}

export type CreateFlowVersionRequest = {
  comment?: string | null
}

export type RestoreFlowVersionResponse = {
  flowId: string
  restoredVersionId: string
  currentRevision: number
}

export type VersionDiffItem = {
  entityType: string
  entityId: string
  changeType: string
  label: string
}

export type FlowVersionCompareResponse = {
  leftVersionId: string
  rightVersionId: string
  laneDiffs: VersionDiffItem[]
  stageDiffs: VersionDiffItem[]
  nodeDiffs: VersionDiffItem[]
  linkDiffs: VersionDiffItem[]
  commentDiffs: VersionDiffItem[]
}

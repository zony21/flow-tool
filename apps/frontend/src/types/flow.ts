export type FlowSummary = {
  flowId: string
  projectId: string
  name: string
  description?: string | null
  sortOrder: number
  createdAtUtc: string
  updatedAtUtc: string
}

export type FlowDetail = FlowSummary & {
  currentRevision: number
  lanes: Lane[]
  stages: Stage[]
  nodes: FlowNode[]
  links: FlowLink[]
  comments: FlowComment[]
  metadata: FlowMetadata[]
}

export type Lane = {
  laneId: string
  flowId: string
  name: string
  sortOrder: number
}

export type Stage = {
  stageId: string
  flowId: string
  name: string
  sortOrder: number
}

export type FlowNode = {
  nodeId: string
  flowId: string
  laneId?: string | null
  stageId?: string | null
  nodeType: string
  name: string
  description?: string | null
  x: number
  y: number
}

export type FlowLink = {
  linkId: string
  flowId: string
  sourceNodeId: string
  targetNodeId: string
  label?: string | null
  condition?: string | null
}

export type FlowComment = {
  commentId: string
  flowId: string
  nodeId?: string | null
  text: string
  x: number
  y: number
}

export type FlowMetadata = {
  metadataId: string
  flowId: string
  metaKey: string
  metaValue: string
}

export type SaveLaneRequest = {
  laneId: string
  name: string
  sortOrder: number
}

export type SaveStageRequest = {
  stageId: string
  name: string
  sortOrder: number
}

export type SaveNodeRequest = {
  nodeId: string
  laneId?: string | null
  stageId?: string | null
  nodeType: string
  name: string
  description?: string | null
  x: number
  y: number
}

export type SaveLinkRequest = {
  linkId: string
  sourceNodeId: string
  targetNodeId: string
  label?: string | null
  condition?: string | null
}

export type SaveCommentRequest = {
  commentId: string
  nodeId?: string | null
  text: string
  x: number
  y: number
}

export type SaveFlowStructureRequest = {
  flowId: string
  clientRevision: number
  lanes: SaveLaneRequest[]
  stages: SaveStageRequest[]
  nodes: SaveNodeRequest[]
  links: SaveLinkRequest[]
  comments: SaveCommentRequest[]
  createVersion: boolean
  changeSummary?: string | null
}

export type SaveFlowStructureResponse = {
  flowId: string
  serverRevision: number
  updatedAtUtc: string
}

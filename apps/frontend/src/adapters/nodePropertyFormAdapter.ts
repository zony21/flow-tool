import type { FlowNode } from '../types/node'

export type NodePropertyForm = {
  nodeId: string
  name: string
}

export function nodePropertyFormAdapter(node: FlowNode): NodePropertyForm {
  return {
    nodeId: node.nodeId,
    name: node.name,
  }
}

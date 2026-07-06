export type MoveNodeCommand = {
  type: 'MOVE_NODE'
  nodeId: string
  x: number
  y: number
}

export function vueFlowToCommandAdapter(nodeId: string, x: number, y: number): MoveNodeCommand {
  return {
    type: 'MOVE_NODE',
    nodeId,
    x,
    y,
  }
}

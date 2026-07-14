import { describe, expect, it } from 'vitest'
import { enrichFlowStructureRequest } from './flowStore'
import type { FlowDetail, SaveFlowStructureRequest } from '../types/flow'

describe('enrichFlowStructureRequest', () => {
  it('明示された最新LocationIdをstoreの古い値より優先する', () => {
    const flow = {
      flowId: 'flow-1', projectId: 'project-1', name: 'Flow', flowType: 'TRANSPORT', sortOrder: 1, createdAtUtc: '', updatedAtUtc: '', currentRevision: 1,
      lanes: [], stages: [], links: [], comments: [], metadata: [],
      nodes: [{ nodeId: 'node-1', flowId: 'flow-1', nodeType: 'Task', name: 'Node', x: 0, y: 0, locationId: 'old-location' }],
    } as FlowDetail
    const request = {
      flowId: 'flow-1', clientRevision: 1, lanes: [], stages: [], links: [], comments: [], createVersion: false, changeSummary: null,
      nodes: [{ nodeId: 'node-1', nodeType: 'Task', name: 'Node', x: 0, y: 0, locationId: 'new-location' }],
    } as SaveFlowStructureRequest

    expect(enrichFlowStructureRequest(flow, request).nodes[0]?.locationId).toBe('new-location')
  })

  it('Normal FlowのTransport値をクリアする', () => {
    const flow = { flowType: 'NORMAL', nodes: [], stages: [] } as unknown as FlowDetail
    const request = {
      flowId: 'flow-1', clientRevision: 1, lanes: [], stages: [], links: [], comments: [], createVersion: false, changeSummary: null,
      nodes: [{ nodeId: 'node-1', nodeType: 'Task', name: 'Node', x: 0, y: 0, commandId: 'command', locationId: 'location', equipmentId: 'equipment', rwType: 'READ' }],
    } as SaveFlowStructureRequest

    const node = enrichFlowStructureRequest(flow, request).nodes[0]!
    expect(node).toMatchObject({ commandId: null, locationId: null, equipmentId: null, rwType: 'NONE' })
  })
})

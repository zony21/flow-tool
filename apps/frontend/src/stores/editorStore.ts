import { defineStore } from 'pinia'
import type { FlowDetail, FlowLink, FlowNode } from '../types/flow'
import { useFlowStore } from './flowStore'
import { useUndoRedoStore } from './undoRedoStore'

type ApplyFlowChangeOptions = {
  actionKey?: string
  coalesceWindowMs?: number
}

type MoveNodePayload = {
  nodeId: string
  x: number
  y: number
  laneId?: string
  stageId?: string
}

type AddLinkPayload = {
  sourceNodeId: string
  targetNodeId: string
}

export const useEditorStore = defineStore('editor', {
  state: () => ({
    zoom: 1,
    gridEnabled: true,
    selectedNodeId: null as string | null,
    selectedLinkId: null as string | null,
    isDirty: false,
  }),
  actions: {
    setZoom(zoom: number): void {
      this.zoom = zoom
    },
    selectNode(nodeId: string): void {
      this.selectedNodeId = nodeId
      this.selectedLinkId = null
    },
    selectLink(linkId: string): void {
      this.selectedLinkId = linkId
      this.selectedNodeId = null
    },
    clearSelection(): void {
      this.selectedNodeId = null
      this.selectedLinkId = null
    },
    clearMissingSelection(existingNodeIds: string[], existingLinkIds: string[]): void {
      if (this.selectedNodeId && !existingNodeIds.includes(this.selectedNodeId)) {
        this.selectedNodeId = null
      }

      if (this.selectedLinkId && !existingLinkIds.includes(this.selectedLinkId)) {
        this.selectedLinkId = null
      }
    },
    markDirty(): void {
      this.isDirty = true
    },
    markSaved(): void {
      this.isDirty = false
      const flowStore = useFlowStore()
      const undoRedoStore = useUndoRedoStore()
      if (flowStore.currentFlow) {
        undoRedoStore.markSaved(flowStore.currentFlow)
      }
    },
    reset(): void {
      this.selectedNodeId = null
      this.selectedLinkId = null
      this.isDirty = false
    },
    initHistory(): void {
      const flowStore = useFlowStore()
      const undoRedoStore = useUndoRedoStore()
      if (flowStore.currentFlow) {
        undoRedoStore.init(flowStore.currentFlow)
        this.isDirty = false
      }
    },
    applyFlowChange(mutator: (flow: FlowDetail) => FlowDetail, options?: ApplyFlowChangeOptions): void {
      const flowStore = useFlowStore()
      const undoRedoStore = useUndoRedoStore()
      if (!flowStore.currentFlow) return

      const before = flowStore.currentFlow
      undoRedoStore.record(before, options)
      flowStore.setCurrentFlow(mutator(before))
      this.markDirty()
    },
    addNode(): void {
      const flowStore = useFlowStore()
      const flow = flowStore.currentFlow
      if (!flow) return

      const defaultLane = flow.lanes.slice().sort((a, b) => a.sortOrder - b.sortOrder)[0]
      const defaultStage = flow.stages.slice().sort((a, b) => a.sortOrder - b.sortOrder)[0]
      const nodeIndex = flow.nodes.length
      const nodeId = crypto.randomUUID()

      this.applyFlowChange(
        (currentFlow) => ({
          ...currentFlow,
          nodes: [
            ...currentFlow.nodes,
            {
              nodeId,
              flowId: currentFlow.flowId,
              laneId: defaultLane?.laneId,
              stageId: defaultStage?.stageId,
              nodeType: 'process',
              name: `Node ${nodeIndex + 1}`,
              description: null,
              x: 40 + (nodeIndex % 3) * 80,
              y: 40 + Math.floor(nodeIndex / 3) * 80,
            },
          ],
        }),
        { actionKey: 'node:add', coalesceWindowMs: 0 },
      )
      this.selectNode(nodeId)
    },
    updateNode(updatedNode: FlowNode): void {
      this.applyFlowChange(
        (flow) => ({
          ...flow,
          nodes: flow.nodes.map((node) => (node.nodeId === updatedNode.nodeId ? updatedNode : node)),
        }),
        { actionKey: `node:update:${updatedNode.nodeId}` },
      )
    },
    moveNode(payload: MoveNodePayload): void {
      this.applyFlowChange(
        (flow) => ({
          ...flow,
          nodes: flow.nodes.map((node) =>
            node.nodeId === payload.nodeId
              ? {
                  ...node,
                  x: payload.x,
                  y: payload.y,
                  laneId: payload.laneId ?? null,
                  stageId: payload.stageId ?? null,
                }
              : node,
          ),
        }),
        { actionKey: `node:move:${payload.nodeId}` },
      )
    },
    deleteNode(payload: { nodeId: string }): void {
      this.applyFlowChange(
        (flow) => ({
          ...flow,
          nodes: flow.nodes.filter((node) => node.nodeId !== payload.nodeId),
          links: flow.links.filter((link) => link.sourceNodeId !== payload.nodeId && link.targetNodeId !== payload.nodeId),
          comments: flow.comments.filter((comment) => comment.nodeId !== payload.nodeId),
        }),
        { actionKey: `node:delete:${payload.nodeId}`, coalesceWindowMs: 0 },
      )

      if (this.selectedNodeId === payload.nodeId) {
        this.clearSelection()
      }
    },
    addLink(payload: AddLinkPayload): void {
      const flowStore = useFlowStore()
      const flow = flowStore.currentFlow
      if (!flow) return
      if (payload.sourceNodeId === payload.targetNodeId) return

      const exists = flow.links.some(
        (link) => link.sourceNodeId === payload.sourceNodeId && link.targetNodeId === payload.targetNodeId,
      )
      if (exists) return

      const linkId = crypto.randomUUID()
      this.applyFlowChange(
        (currentFlow) => ({
          ...currentFlow,
          links: [
            ...currentFlow.links,
            {
              linkId,
              flowId: currentFlow.flowId,
              sourceNodeId: payload.sourceNodeId,
              targetNodeId: payload.targetNodeId,
              label: null,
              condition: null,
            },
          ],
        }),
        { actionKey: 'link:add', coalesceWindowMs: 0 },
      )
      this.selectLink(linkId)
    },
    updateLink(updatedLink: FlowLink): void {
      this.applyFlowChange(
        (flow) => ({
          ...flow,
          links: flow.links.map((link) => (link.linkId === updatedLink.linkId ? updatedLink : link)),
        }),
        { actionKey: `link:update:${updatedLink.linkId}` },
      )
    },
    deleteLink(payload: { linkId: string }): void {
      this.applyFlowChange(
        (flow) => ({
          ...flow,
          links: flow.links.filter((link) => link.linkId !== payload.linkId),
        }),
        { actionKey: `link:delete:${payload.linkId}`, coalesceWindowMs: 0 },
      )

      if (this.selectedLinkId === payload.linkId) {
        this.clearSelection()
      }
    },
    undo(): void {
      const flowStore = useFlowStore()
      const undoRedoStore = useUndoRedoStore()
      if (!flowStore.currentFlow) return

      const previous = undoRedoStore.undo(flowStore.currentFlow)
      if (!previous) return

      flowStore.setCurrentFlow(previous)
      this.clearMissingSelection(
        previous.nodes.map((node) => node.nodeId),
        previous.links.map((link) => link.linkId),
      )
      this.isDirty = !undoRedoStore.isSaved(previous)
    },
    redo(): void {
      const flowStore = useFlowStore()
      const undoRedoStore = useUndoRedoStore()
      if (!flowStore.currentFlow) return

      const next = undoRedoStore.redo(flowStore.currentFlow)
      if (!next) return

      flowStore.setCurrentFlow(next)
      this.clearMissingSelection(
        next.nodes.map((node) => node.nodeId),
        next.links.map((link) => link.linkId),
      )
      this.isDirty = !undoRedoStore.isSaved(next)
    },
  },
})

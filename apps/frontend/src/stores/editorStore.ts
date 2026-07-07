import { defineStore } from 'pinia'
import type { FlowDetail, FlowLink, FlowNode, Lane, Stage } from '../types/flow'
import { getNodeSample } from '../constants/nodeSamples'
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

type AddNodePayload = {
  nodeType?: string
  name?: string
  laneId?: string
  stageId?: string
  x?: number
  y?: number
}

type AddLinkPayload = {
  sourceNodeId: string
  targetNodeId: string
}

const laneHeight = 160
const stageWidth = 260
const nodeOffsetX = 36
const nodeOffsetY = 34
const nodeSpacingX = 36
const nodeSpacingY = 76

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
    addLane(): void {
      const flowStore = useFlowStore()
      const flow = flowStore.currentFlow
      if (!flow) return

      const nextSortOrder = Math.max(0, ...flow.lanes.map((lane) => lane.sortOrder)) + 1
      const lane: Lane = {
        laneId: crypto.randomUUID(),
        flowId: flow.flowId,
        name: `担当・責務 ${nextSortOrder}`,
        sortOrder: nextSortOrder,
      }

      this.applyFlowChange(
        (currentFlow) => ({
          ...currentFlow,
          lanes: [...currentFlow.lanes, lane],
        }),
        { actionKey: 'lane:add', coalesceWindowMs: 0 },
      )
    },
    updateLane(updatedLane: Lane): void {
      if (!updatedLane.name.trim()) return

      this.applyFlowChange(
        (flow) => ({
          ...flow,
          lanes: flow.lanes.map((lane) => (lane.laneId === updatedLane.laneId ? { ...updatedLane, name: updatedLane.name.trim() } : lane)),
        }),
        { actionKey: `lane:update:${updatedLane.laneId}` },
      )
    },
    deleteLane(payload: { laneId: string }): void {
      this.applyFlowChange(
        (flow) => ({
          ...flow,
          lanes: flow.lanes.filter((lane) => lane.laneId !== payload.laneId),
          nodes: flow.nodes.map((node) => (node.laneId === payload.laneId ? { ...node, laneId: null } : node)),
        }),
        { actionKey: `lane:delete:${payload.laneId}`, coalesceWindowMs: 0 },
      )
    },
    addStage(): void {
      const flowStore = useFlowStore()
      const flow = flowStore.currentFlow
      if (!flow) return

      const nextSortOrder = Math.max(0, ...flow.stages.map((stage) => stage.sortOrder)) + 1
      const stage: Stage = {
        stageId: crypto.randomUUID(),
        flowId: flow.flowId,
        name: `設備・場所 ${nextSortOrder}`,
        sortOrder: nextSortOrder,
      }

      this.applyFlowChange(
        (currentFlow) => ({
          ...currentFlow,
          stages: [...currentFlow.stages, stage],
        }),
        { actionKey: 'stage:add', coalesceWindowMs: 0 },
      )
    },
    updateStage(updatedStage: Stage): void {
      if (!updatedStage.name.trim()) return

      this.applyFlowChange(
        (flow) => ({
          ...flow,
          stages: flow.stages.map((stage) => (stage.stageId === updatedStage.stageId ? { ...updatedStage, name: updatedStage.name.trim() } : stage)),
        }),
        { actionKey: `stage:update:${updatedStage.stageId}` },
      )
    },
    deleteStage(payload: { stageId: string }): void {
      this.applyFlowChange(
        (flow) => ({
          ...flow,
          stages: flow.stages.filter((stage) => stage.stageId !== payload.stageId),
          nodes: flow.nodes.map((node) => (node.stageId === payload.stageId ? { ...node, stageId: null } : node)),
        }),
        { actionKey: `stage:delete:${payload.stageId}`, coalesceWindowMs: 0 },
      )
    },
    addNode(payload?: AddNodePayload): void {
      const flowStore = useFlowStore()
      const flow = flowStore.currentFlow
      if (!flow) return

      const lanes = flow.lanes.slice().sort((a, b) => a.sortOrder - b.sortOrder)
      const stages = flow.stages.slice().sort((a, b) => a.sortOrder - b.sortOrder)
      const defaultLane = lanes.find((lane) => lane.laneId === payload?.laneId) ?? lanes[0]
      const defaultStage = stages.find((stage) => stage.stageId === payload?.stageId) ?? stages[0]
      const laneIndex = Math.max(0, lanes.findIndex((lane) => lane.laneId === defaultLane?.laneId))
      const stageIndex = Math.max(0, stages.findIndex((stage) => stage.stageId === defaultStage?.stageId))
      const nodeType = payload?.nodeType ?? 'process'
      const sample = getNodeSample(nodeType)
      const sameCellNodes = flow.nodes.filter((node) => node.laneId === defaultLane?.laneId && node.stageId === defaultStage?.stageId)
      const sameCellCount = sameCellNodes.length
      const nodeId = crypto.randomUUID()
      const x = payload?.x ?? stageIndex * stageWidth + nodeOffsetX + (sameCellCount % 3) * nodeSpacingX
      let y = payload?.y ?? laneIndex * laneHeight + nodeOffsetY + Math.floor(sameCellCount / 3) * nodeSpacingY
      const rowTop = laneIndex * laneHeight + nodeOffsetY
      const rowBottom = (laneIndex + 1) * laneHeight - nodeOffsetY

      while (sameCellNodes.some((node) => Math.abs(node.y - y) < nodeSpacingY)) {
        y += nodeSpacingY
      }

      if (y < rowTop) {
        y = rowTop
      }

      if (y > rowBottom) {
        y = rowTop + sameCellCount * nodeSpacingY
      }

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
              nodeType,
              name: payload?.name ?? `${sample.defaultName} ${sameCellCount + 1}`,
              description: null,
              x,
              y,
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

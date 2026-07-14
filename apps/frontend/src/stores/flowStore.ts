import { defineStore } from 'pinia'
import type { AxiosError } from 'axios'
import { createFlow, deleteFlow, duplicateFlow, fetchFlow, fetchFlows, saveFlowStructure, updateFlow, type FlowSaveRequest } from '../api/flowApi'
import type { SaveFlowStructureRequest } from '../types/flow'
import type { FlowDetail, FlowSummary } from '../types/flow'

export function enrichFlowStructureRequest(
  currentFlow: FlowDetail | null,
  request: SaveFlowStructureRequest,
): SaveFlowStructureRequest {
  const currentNodes = new Map(currentFlow?.nodes.map((node) => [node.nodeId, node]) ?? [])
  const isTransportFlow = currentFlow?.flowType === 'TRANSPORT'

  return {
    ...request,
    stages: request.stages.map((stage) => {
      const currentStage = currentFlow?.stages.find((item) => item.stageId === stage.stageId)
      return {
        ...stage,
        stageType: stage.stageType ?? currentStage?.stageType ?? 'AUTO',
      }
    }),
    nodes: request.nodes.map((node) => {
      const currentNode = currentNodes.get(node.nodeId)
      return {
        ...node,
        commandId: isTransportFlow ? node.commandId ?? currentNode?.commandId ?? null : null,
        locationId: isTransportFlow ? node.locationId ?? currentNode?.locationId ?? null : null,
        equipmentId: isTransportFlow ? node.equipmentId ?? currentNode?.equipmentId ?? null : null,
        vehicleModelId: isTransportFlow ? node.vehicleModelId ?? currentNode?.vehicleModelId ?? null : null,
        rwType: isTransportFlow ? node.rwType ?? currentNode?.rwType ?? 'NONE' : 'NONE',
      }
    }),
  }
}

export const useFlowStore = defineStore('flow', {
  state: () => ({
    flows: [] as FlowSummary[],
    currentFlow: null as FlowDetail | null,
    loading: false,
    lastError: null as string | null,
  }),
  actions: {
    setCurrentFlow(flow: FlowDetail): void {
      this.currentFlow = flow
    },
    clearError(): void {
      this.lastError = null
    },
    setError(error: unknown): void {
      const axiosError = error as AxiosError<{ message?: string; detail?: string; code?: string }>
      this.lastError = axiosError.response?.data?.message
        ?? axiosError.response?.data?.detail
        ?? axiosError.message
        ?? '処理に失敗しました。'
    },
    async loadFlows(projectId: string): Promise<void> {
      this.loading = true
      this.clearError()
      try {
        this.flows = await fetchFlows(projectId)
      } catch (error) {
        this.setError(error)
        throw error
      } finally {
        this.loading = false
      }
    },
    async loadFlow(projectId: string, flowId: string): Promise<void> {
      this.loading = true
      this.clearError()
      try {
        this.currentFlow = await fetchFlow(projectId, flowId)
      } catch (error) {
        this.setError(error)
        throw error
      } finally {
        this.loading = false
      }
    },
    async create(projectId: string, request: FlowSaveRequest): Promise<FlowDetail> {
      const flow = await createFlow(projectId, request)
      await this.loadFlows(projectId)
      return flow
    },
    async update(projectId: string, flowId: string, request: FlowSaveRequest): Promise<FlowDetail> {
      const flow = await updateFlow(projectId, flowId, request)
      this.currentFlow = flow
      await this.loadFlows(projectId)
      return flow
    },
    async duplicate(projectId: string, flowId: string): Promise<FlowDetail> {
      const flow = await duplicateFlow(projectId, flowId)
      await this.loadFlows(projectId)
      return flow
    },
    async remove(projectId: string, flowId: string): Promise<void> {
      await deleteFlow(projectId, flowId)
      if (this.currentFlow?.flowId === flowId) {
        this.currentFlow = null
      }
      await this.loadFlows(projectId)
    },
    async saveStructure(projectId: string, request: SaveFlowStructureRequest): Promise<void> {
      this.loading = true
      this.clearError()
      try {
        const enrichedRequest = enrichFlowStructureRequest(this.currentFlow, request)

        await saveFlowStructure(projectId, request.flowId, enrichedRequest)
        this.currentFlow = await fetchFlow(projectId, request.flowId)
      } catch (error) {
        this.setError(error)
        throw error
      } finally {
        this.loading = false
      }
    },
  },
})

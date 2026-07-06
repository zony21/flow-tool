import { defineStore } from 'pinia'
import { createFlow, deleteFlow, fetchFlow, fetchFlows, updateFlow, type FlowSaveRequest } from '../api/flowApi'
import type { FlowDetail, FlowSummary } from '../types/flow'

export const useFlowStore = defineStore('flow', {
  state: () => ({
    flows: [] as FlowSummary[],
    currentFlow: null as FlowDetail | null,
    loading: false,
  }),
  actions: {
    setCurrentFlow(flow: FlowDetail): void {
      this.currentFlow = flow
    },
    async loadFlows(projectId: string): Promise<void> {
      this.loading = true
      try {
        this.flows = await fetchFlows(projectId)
      } finally {
        this.loading = false
      }
    },
    async loadFlow(projectId: string, flowId: string): Promise<void> {
      this.loading = true
      try {
        this.currentFlow = await fetchFlow(projectId, flowId)
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
    async remove(projectId: string, flowId: string): Promise<void> {
      await deleteFlow(projectId, flowId)
      if (this.currentFlow?.flowId === flowId) {
        this.currentFlow = null
      }
      await this.loadFlows(projectId)
    },
  },
})

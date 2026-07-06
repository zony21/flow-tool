import { defineStore } from 'pinia'
import type { FlowDetail } from '../types/flow'

export const useFlowStore = defineStore('flow', {
  state: () => ({
    currentFlow: null as FlowDetail | null,
  }),
  actions: {
    setCurrentFlow(flow: FlowDetail): void {
      this.currentFlow = flow
    },
  },
})

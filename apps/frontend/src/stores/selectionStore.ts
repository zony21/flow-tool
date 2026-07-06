import { defineStore } from 'pinia'

export const useSelectionStore = defineStore('selection', {
  state: () => ({
    selectedNodeId: '' as string,
  }),
  actions: {
    selectNode(nodeId: string): void {
      this.selectedNodeId = nodeId
    },
  },
})

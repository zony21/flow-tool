import { defineStore } from 'pinia'

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
    },
    reset(): void {
      this.selectedNodeId = null
      this.selectedLinkId = null
      this.isDirty = false
    },
  },
})

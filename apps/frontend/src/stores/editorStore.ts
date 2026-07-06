import { defineStore } from 'pinia'

export const useEditorStore = defineStore('editor', {
  state: () => ({
    zoom: 1,
    gridEnabled: true,
  }),
  actions: {
    setZoom(zoom: number): void {
      this.zoom = zoom
    },
  },
})

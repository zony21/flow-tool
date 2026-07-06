import { defineStore } from 'pinia'

export const useUndoRedoStore = defineStore('undo-redo', {
  state: () => ({
    undoCount: 0,
    redoCount: 0,
  }),
  actions: {
    reset(): void {
      this.undoCount = 0
      this.redoCount = 0
    },
  },
})

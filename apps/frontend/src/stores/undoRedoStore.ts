import { defineStore } from 'pinia'
import type { FlowDetail } from '../types/flow'

type RecordOptions = {
  actionKey?: string
  coalesceWindowMs?: number
}

export const useUndoRedoStore = defineStore('undo-redo', {
  state: () => ({
    undoStack: [] as string[],
    redoStack: [] as string[],
    savedSnapshot: null as string | null,
    maxHistory: 100,
    lastActionKey: null as string | null,
    lastActionAt: 0,
  }),
  getters: {
    undoCount: (state) => state.undoStack.length,
    redoCount: (state) => state.redoStack.length,
    canUndo: (state) => state.undoStack.length > 0,
    canRedo: (state) => state.redoStack.length > 0,
  },
  actions: {
    init(flow: FlowDetail): void {
      this.undoStack = []
      this.redoStack = []
      this.savedSnapshot = this.serialize(flow)
      this.lastActionKey = null
      this.lastActionAt = 0
    },
    record(flow: FlowDetail, options?: RecordOptions): void {
      const now = Date.now()
      const actionKey = options?.actionKey ?? null
      const coalesceWindowMs = options?.coalesceWindowMs ?? 800
      const canCoalesce =
        actionKey !== null &&
        actionKey === this.lastActionKey &&
        now - this.lastActionAt <= coalesceWindowMs

      if (!canCoalesce) {
        this.undoStack.push(this.serialize(flow))
        if (this.undoStack.length > this.maxHistory) {
          this.undoStack.shift()
        }
      }

      this.lastActionKey = actionKey
      this.lastActionAt = now
      if (this.undoStack.length > this.maxHistory) {
        this.undoStack.shift()
      }
      this.redoStack = []
    },
    undo(currentFlow: FlowDetail): FlowDetail | null {
      const previous = this.undoStack.pop()
      if (!previous) {
        return null
      }

      this.redoStack.push(this.serialize(currentFlow))
      return this.deserialize(previous)
    },
    redo(currentFlow: FlowDetail): FlowDetail | null {
      const next = this.redoStack.pop()
      if (!next) {
        return null
      }

      this.undoStack.push(this.serialize(currentFlow))
      return this.deserialize(next)
    },
    markSaved(flow: FlowDetail): void {
      this.savedSnapshot = this.serialize(flow)
    },
    isSaved(flow: FlowDetail): boolean {
      return this.savedSnapshot === this.serialize(flow)
    },
    reset(): void {
      this.undoStack = []
      this.redoStack = []
      this.savedSnapshot = null
      this.lastActionKey = null
      this.lastActionAt = 0
    },
    serialize(flow: FlowDetail): string {
      return JSON.stringify(flow)
    },
    deserialize(serialized: string): FlowDetail {
      return JSON.parse(serialized) as FlowDetail
    },
  },
})

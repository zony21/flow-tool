import { defineStore } from 'pinia'

export const useExportStore = defineStore('export', {
  state: () => ({
    running: false,
    lastFormat: '' as '' | 'mermaid' | 'pdf' | 'json',
  }),
  actions: {
    start(format: 'mermaid' | 'pdf' | 'json'): void {
      this.running = true
      this.lastFormat = format
    },
    finish(): void {
      this.running = false
    },
  },
})

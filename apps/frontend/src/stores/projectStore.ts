import { defineStore } from 'pinia'
import type { ProjectSummary } from '../types/project'

export const useProjectStore = defineStore('project', {
  state: () => ({
    projects: [] as ProjectSummary[],
  }),
  actions: {
    setProjects(projects: ProjectSummary[]): void {
      this.projects = projects
    },
  },
})

import { defineStore } from 'pinia'
import { createProject, deleteProject, fetchProject, fetchProjects, updateProject, type ProjectSaveRequest } from '../api/projectApi'
import type { ProjectDetail, ProjectSummary } from '../types/project'

export const useProjectStore = defineStore('project', {
  state: () => ({
    projects: [] as ProjectSummary[],
    currentProject: null as ProjectDetail | null,
    loading: false,
  }),
  actions: {
    setProjects(projects: ProjectSummary[]): void {
      this.projects = projects
    },
    async loadProjects(): Promise<void> {
      this.loading = true
      try {
        this.projects = await fetchProjects()
      } finally {
        this.loading = false
      }
    },
    async loadProject(projectId: string): Promise<void> {
      this.loading = true
      try {
        this.currentProject = await fetchProject(projectId)
      } finally {
        this.loading = false
      }
    },
    async create(request: ProjectSaveRequest): Promise<ProjectDetail> {
      const project = await createProject(request)
      await this.loadProjects()
      return project
    },
    async update(projectId: string, request: ProjectSaveRequest): Promise<ProjectDetail> {
      const project = await updateProject(projectId, request)
      this.currentProject = project
      await this.loadProjects()
      return project
    },
    async remove(projectId: string): Promise<void> {
      await deleteProject(projectId)
      if (this.currentProject?.projectId === projectId) {
        this.currentProject = null
      }
      await this.loadProjects()
    },
  },
})

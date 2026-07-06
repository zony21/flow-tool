import { defineStore } from 'pinia'
import { createProject, deleteProject, fetchProject, fetchProjects, updateProject, type ProjectSaveRequest } from '../api/projectApi'
import { normalizeApiError } from '../api/apiError'
import type { ProjectDetail, ProjectSummary } from '../types/project'

export const useProjectStore = defineStore('project', {
  state: () => ({
    projects: [] as ProjectSummary[],
    currentProject: null as ProjectDetail | null,
    loading: false,
    saving: false,
    errorMessage: null as string | null,
  }),
  actions: {
    clearError(): void {
      this.errorMessage = null
    },
    async loadProjects(): Promise<void> {
      this.loading = true
      this.clearError()
      try {
        this.projects = await fetchProjects()
      } catch (error) {
        this.errorMessage = normalizeApiError(error).message
      } finally {
        this.loading = false
      }
    },
    async loadProject(projectId: string): Promise<void> {
      this.loading = true
      this.clearError()
      try {
        this.currentProject = await fetchProject(projectId)
      } catch (error) {
        this.currentProject = null
        this.errorMessage = normalizeApiError(error).message
      } finally {
        this.loading = false
      }
    },
    async create(request: ProjectSaveRequest): Promise<ProjectDetail | null> {
      this.saving = true
      this.clearError()
      try {
        const project = await createProject(request)
        this.currentProject = project
        await this.loadProjects()
        return project
      } catch (error) {
        this.errorMessage = normalizeApiError(error).message
        return null
      } finally {
        this.saving = false
      }
    },
    async update(projectId: string, request: ProjectSaveRequest): Promise<ProjectDetail | null> {
      this.saving = true
      this.clearError()
      try {
        const project = await updateProject(projectId, request)
        this.currentProject = project
        await this.loadProjects()
        return project
      } catch (error) {
        this.errorMessage = normalizeApiError(error).message
        return null
      } finally {
        this.saving = false
      }
    },
    async remove(projectId: string): Promise<boolean> {
      this.saving = true
      this.clearError()
      try {
        await deleteProject(projectId)
        if (this.currentProject?.projectId === projectId) {
          this.currentProject = null
        }
        await this.loadProjects()
        return true
      } catch (error) {
        this.errorMessage = normalizeApiError(error).message
        return false
      } finally {
        this.saving = false
      }
    },
  },
})

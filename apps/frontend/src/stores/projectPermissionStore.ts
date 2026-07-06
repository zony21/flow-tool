import { defineStore } from 'pinia'
import type { ProjectPermission } from '../types/auth'

export const useProjectPermissionStore = defineStore('project-permission', {
  state: () => ({
    current: null as ProjectPermission | null,
  }),
  getters: {
    can: (state) => {
      return (permissionCode: string) => {
        return state.current?.permissions.includes(permissionCode) ?? false
      }
    },
  },
  actions: {
    setPermission(permission: ProjectPermission | null): void {
      this.current = permission
    },
    clear(): void {
      this.current = null
    },
  },
})

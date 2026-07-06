import { defineStore } from 'pinia'
import { fetchCurrentUser, loginWithDemo, logout as logoutApi } from '../api/authApi'
import type { CurrentUser } from '../types/auth'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    currentUser: null as CurrentUser | null,
    loading: false,
  }),
  actions: {
    async bootstrap(): Promise<void> {
      this.loading = true
      try {
        this.currentUser = await fetchCurrentUser()
      } catch {
        this.currentUser = null
      } finally {
        this.loading = false
      }
    },
    async login(): Promise<void> {
      this.currentUser = await loginWithDemo()
    },
    async logout(): Promise<void> {
      await logoutApi()
      this.currentUser = null
    },
  },
})

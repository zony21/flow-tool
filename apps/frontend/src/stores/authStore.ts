import { defineStore } from 'pinia'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    isAuthenticated: false,
    userName: '' as string,
  }),
  actions: {
    setAuthenticated(userName: string): void {
      this.isAuthenticated = true
      this.userName = userName
    },
  },
})

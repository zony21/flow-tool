export type CurrentUser = {
  userId: string
  userName: string
  displayName: string
  email?: string | null
}

export type ProjectPermission = {
  projectId: string
  roleCode: string
  permissions: string[]
}

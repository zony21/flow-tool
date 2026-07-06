export type ProjectSummary = {
  projectId: string
  name: string
  description?: string | null
  createdAtUtc: string
}

export type ProjectDetail = ProjectSummary & {
  updatedAtUtc?: string | null
}

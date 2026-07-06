<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import MainLayout from '../layouts/MainLayout.vue'
import { useProjectStore } from '../stores/projectStore'

const router = useRouter()
const projectStore = useProjectStore()
const projectName = ref('')
const projectDescription = ref('')
const createProjectDialogVisible = ref(false)
const projectDeleteTarget = ref<{ projectId: string; name: string } | null>(null)

const canCreate = computed(() => projectName.value.trim().length > 0 && !projectStore.saving)
const deleteProjectDialogVisible = computed({
  get: () => projectDeleteTarget.value !== null,
  set: (visible: boolean) => {
    if (!visible) cancelDeleteProject()
  },
})

onMounted(async () => {
  await projectStore.loadProjects()
})

async function createProject(): Promise<void> {
  const name = projectName.value.trim()
  if (!name) return

  const project = await projectStore.create({
    name,
    description: projectDescription.value.trim() || null,
  })

  if (!project) return

  closeCreateProjectDialog()
  await router.push({ name: 'project-detail', params: { projectId: project.projectId } })
}

function openCreateProjectDialog(): void {
  projectName.value = ''
  projectDescription.value = ''
  createProjectDialogVisible.value = true
}

function closeCreateProjectDialog(): void {
  createProjectDialogVisible.value = false
  projectName.value = ''
  projectDescription.value = ''
}

async function openProject(projectId: string): Promise<void> {
  await router.push({ name: 'project-detail', params: { projectId } })
}

function requestDeleteProject(projectId: string, name: string): void {
  projectDeleteTarget.value = { projectId, name }
}

function cancelDeleteProject(): void {
  projectDeleteTarget.value = null
}

async function deleteProject(): Promise<void> {
  if (!projectDeleteTarget.value) return

  const deleted = await projectStore.remove(projectDeleteTarget.value.projectId)
  if (deleted) {
    projectDeleteTarget.value = null
  }
}

function formatDate(value: string): string {
  return new Date(value).toLocaleString()
}
</script>

<template>
  <MainLayout>
    <section class="page project-list-page">
      <div class="card page-header">
        <div>
          <h1>プロジェクト一覧</h1>
          <p>参照権限を持つProjectを表示します。</p>
        </div>
        <div class="header-actions">
          <Button label="新規Project" icon="pi pi-plus" :disabled="projectStore.saving" @click="openCreateProjectDialog()" />
          <Button label="再読み込み" severity="secondary" :disabled="projectStore.loading" @click="projectStore.loadProjects()" />
        </div>
      </div>

      <p v-if="projectStore.errorMessage" class="error-message">{{ projectStore.errorMessage }}</p>
      <p v-if="projectStore.loading">読み込み中...</p>

      <div v-else-if="projectStore.projects.length === 0" class="card empty-card">
        <h2>Projectがありません</h2>
        <p>最初のProjectを作成すると、Flow作成とEditor起動へ進めます。</p>
      </div>

      <div v-else class="project-grid">
        <article v-for="project in projectStore.projects" :key="project.projectId" class="card project-card">
          <div>
            <h2>{{ project.name }}</h2>
            <p>{{ project.description || '説明なし' }}</p>
            <p class="meta">作成日時: {{ formatDate(project.createdAtUtc) }}</p>
          </div>
          <div class="actions">
            <Button label="詳細" @click="openProject(project.projectId)" />
            <Button label="削除" severity="danger" :disabled="projectStore.saving" @click="requestDeleteProject(project.projectId, project.name)" />
          </div>
        </article>
      </div>

      <Dialog v-model:visible="createProjectDialogVisible" modal header="Project作成" :style="{ width: 'min(520px, 92vw)' }">
        <div class="dialog-body">
          <label>
            Project名
            <input v-model="projectName" type="text" placeholder="例: WCS搬送フロー" :disabled="projectStore.saving" />
          </label>
          <label>
            説明
            <textarea v-model="projectDescription" rows="4" placeholder="Projectの目的や対象範囲を入力" :disabled="projectStore.saving" />
          </label>
        </div>
        <template #footer>
          <Button label="キャンセル" severity="secondary" :disabled="projectStore.saving" @click="closeCreateProjectDialog()" />
          <Button label="作成して開く" icon="pi pi-arrow-right" :disabled="!canCreate" @click="createProject()" />
        </template>
      </Dialog>

      <Dialog v-model:visible="deleteProjectDialogVisible" modal header="Project削除" :style="{ width: 'min(440px, 92vw)' }">
        <p class="confirm-message">{{ projectDeleteTarget?.name }} を削除します。</p>
        <template #footer>
          <Button label="キャンセル" severity="secondary" :disabled="projectStore.saving" @click="cancelDeleteProject()" />
          <Button label="削除" severity="danger" :disabled="projectStore.saving" @click="deleteProject()" />
        </template>
      </Dialog>
    </section>
  </MainLayout>
</template>

<style scoped>
.project-list-page {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.page-header h1,
.project-card h2,
.empty-card h2 {
  margin: 0 0 8px;
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 12px;
}

label {
  display: flex;
  flex-direction: column;
  gap: 6px;
  font-weight: 600;
}

input,
textarea {
  width: 100%;
  padding: 10px 12px;
  border: 1px solid var(--border);
  border-radius: 8px;
  font: inherit;
}

textarea {
  resize: vertical;
}

.header-actions,
.actions {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 8px;
  flex-wrap: wrap;
}

.dialog-body {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.confirm-message {
  margin: 0;
}

.project-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
  gap: 16px;
}

.project-card {
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  gap: 16px;
}

.meta {
  font-size: 0.85rem;
}

.error-message {
  padding: 12px 14px;
  color: #991b1b;
  background: #fee2e2;
  border: 1px solid #fecaca;
  border-radius: 8px;
}
</style>

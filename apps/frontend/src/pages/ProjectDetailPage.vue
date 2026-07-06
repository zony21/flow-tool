<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import MainLayout from '../layouts/MainLayout.vue'
import { normalizeApiError } from '../api/apiError'
import { useFlowStore } from '../stores/flowStore'
import { useProjectPermissionStore } from '../stores/projectPermissionStore'
import { useProjectStore } from '../stores/projectStore'
import { PermissionCodes } from '../types/permission'

const route = useRoute()
const router = useRouter()
const projectStore = useProjectStore()
const flowStore = useFlowStore()
const permissionStore = useProjectPermissionStore()

const flowName = ref('')
const flowDescription = ref('')
const flowErrorMessage = ref<string | null>(null)

const projectId = computed(() => String(route.params.projectId ?? ''))
const canCreateFlow = computed(() => permissionStore.can(PermissionCodes.FlowUpdate))
const canSubmitFlow = computed(() => flowName.value.trim().length > 0 && !flowStore.loading && canCreateFlow.value)

onMounted(async () => {
  await loadPage()
})

async function loadPage(): Promise<void> {
  if (!projectId.value) return

  flowErrorMessage.value = null
  await Promise.all([
    projectStore.loadProject(projectId.value),
    loadFlows(),
  ])
}

async function loadFlows(): Promise<void> {
  if (!projectId.value) return

  try {
    await flowStore.loadFlows(projectId.value)
  } catch (error) {
    flowErrorMessage.value = normalizeApiError(error).message
  }
}

async function createFlow(): Promise<void> {
  const name = flowName.value.trim()
  if (!name || !canCreateFlow.value) return

  flowErrorMessage.value = null
  try {
    const flow = await flowStore.create(projectId.value, {
      name,
      description: flowDescription.value.trim() || null,
    })
    flowName.value = ''
    flowDescription.value = ''
    await router.push({ name: 'flow-editor', params: { projectId: projectId.value, flowId: flow.flowId } })
  } catch (error) {
    flowErrorMessage.value = normalizeApiError(error).message
  }
}

async function openFlow(flowId: string): Promise<void> {
  await router.push({ name: 'flow-editor', params: { projectId: projectId.value, flowId } })
}

async function backToProjects(): Promise<void> {
  await router.push({ name: 'project-list' })
}

function formatDate(value: string): string {
  return new Date(value).toLocaleString()
}
</script>

<template>
  <MainLayout>
    <section class="page project-detail-page">
      <div class="card page-header">
        <div>
          <h1>{{ projectStore.currentProject?.name ?? 'プロジェクト詳細' }}</h1>
          <p>{{ projectStore.currentProject?.description || 'Project詳細とFlow一覧を表示します。' }}</p>
          <p class="meta">projectId: {{ projectId }}</p>
        </div>
        <div class="header-actions">
          <Button label="Project一覧へ戻る" severity="secondary" @click="backToProjects()" />
          <Button label="再読み込み" severity="secondary" :disabled="projectStore.loading || flowStore.loading" @click="loadPage()" />
        </div>
      </div>

      <p v-if="projectStore.errorMessage" class="error-message">{{ projectStore.errorMessage }}</p>
      <p v-if="flowErrorMessage" class="error-message">{{ flowErrorMessage }}</p>

      <div class="card create-card">
        <h2>Flow作成</h2>
        <p v-if="!canCreateFlow" class="viewer-message">Viewer権限ではFlow作成・編集はできません。</p>
        <div class="form-grid">
          <label>
            Flow名
            <input v-model="flowName" type="text" placeholder="例: 入庫搬送フロー" :disabled="!canCreateFlow || flowStore.loading" />
          </label>
          <label>
            説明
            <textarea v-model="flowDescription" rows="3" placeholder="Flowの対象工程や目的を入力" :disabled="!canCreateFlow || flowStore.loading" />
          </label>
        </div>
        <div class="actions">
          <Button label="Flow作成してEditorを開く" :disabled="!canSubmitFlow" @click="createFlow()" />
        </div>
      </div>

      <div class="card">
        <div class="section-header">
          <h2>Flow一覧</h2>
          <span>{{ flowStore.flows.length }}件</span>
        </div>
        <p v-if="flowStore.loading">読み込み中...</p>
        <p v-else-if="flowStore.flows.length === 0">Flowはまだありません。</p>
        <table v-else class="flow-table">
          <thead>
            <tr>
              <th>Flow名</th>
              <th>説明</th>
              <th>更新日時</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="flow in flowStore.flows" :key="flow.flowId">
              <td>{{ flow.name }}</td>
              <td>{{ flow.description || '説明なし' }}</td>
              <td>{{ formatDate(flow.updatedAtUtc) }}</td>
              <td>
                <Button label="Editor" size="small" @click="openFlow(flow.flowId)" />
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  </MainLayout>
</template>

<style scoped>
.project-detail-page {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.page-header,
.section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.header-actions,
.actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}

.page-header h1,
.create-card h2,
.section-header h2 {
  margin: 0 0 8px;
}

.meta {
  font-size: 0.85rem;
}

.create-card {
  display: flex;
  flex-direction: column;
  gap: 12px;
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

.flow-table {
  width: 100%;
  border-collapse: collapse;
}

.flow-table th,
.flow-table td {
  padding: 10px 12px;
  border-bottom: 1px solid var(--border);
  text-align: left;
  vertical-align: top;
}

.flow-table th {
  color: var(--text-sub);
  font-size: 0.85rem;
}

.error-message {
  padding: 12px 14px;
  color: #991b1b;
  background: #fee2e2;
  border: 1px solid #fecaca;
  border-radius: 8px;
}

.viewer-message {
  padding: 10px 12px;
  color: #92400e;
  background: #fef3c7;
  border: 1px solid #fde68a;
  border-radius: 8px;
}
</style>

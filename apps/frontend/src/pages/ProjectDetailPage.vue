<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import MainLayout from '../layouts/MainLayout.vue'
import LocationCreateDialog from '../components/transport/LocationCreateDialog.vue'
import { normalizeApiError } from '../api/apiError'
import { fetchTransportLocations } from '../api/transportApi'
import { useFlowStore } from '../stores/flowStore'
import { useProjectPermissionStore } from '../stores/projectPermissionStore'
import { useProjectStore } from '../stores/projectStore'
import { PermissionCodes } from '../types/permission'
import type { FlowType } from '../types/flow'
import type { TransportLocation } from '../types/transport'

const route = useRoute()
const router = useRouter()
const projectStore = useProjectStore()
const flowStore = useFlowStore()
const permissionStore = useProjectPermissionStore()

const projectEditName = ref('')
const projectEditDescription = ref('')
const editingProject = ref(false)
const createFlowDialogVisible = ref(false)
const savingFlowId = ref<string | null>(null)
const creatingFlow = ref(false)
const duplicatingFlowId = ref<string | null>(null)
const deletingFlowId = ref<string | null>(null)
const flowDeleteTarget = ref<{ flowId: string; name: string } | null>(null)
const editingFlowId = ref<string | null>(null)
const editingFlowName = ref('')
const editingFlowDescription = ref('')
const editingFlowType = ref<FlowType>('NORMAL')
const flowName = ref('')
const flowDescription = ref('')
const flowType = ref<FlowType>('NORMAL')
const flowErrorMessage = ref<string | null>(null)
const transportLocations = ref<TransportLocation[]>([])
const locationDialogVisible = ref(false)
const locationLoading = ref(false)
const locationErrorMessage = ref<string | null>(null)

const flowTypeOptions: { value: FlowType; label: string; description: string }[] = [
  { value: 'NORMAL', label: 'Normal', description: '通常フロー' },
  { value: 'TRANSPORT', label: 'Transport', description: 'AGF/AGV搬送フロー' },
]

const projectId = computed(() => String(route.params.projectId ?? ''))
const canUpdateProject = computed(() => permissionStore.can(PermissionCodes.ProjectUpdate))
const canCreateFlow = computed(() => permissionStore.can(PermissionCodes.FlowUpdate))
const canSubmitProject = computed(() => projectEditName.value.trim().length > 0 && !projectStore.saving && canUpdateProject.value)
const canSubmitFlow = computed(() => flowName.value.trim().length > 0 && !creatingFlow.value && canCreateFlow.value)
const canSubmitFlowEdit = computed(() => editingFlowName.value.trim().length > 0 && savingFlowId.value === null && canCreateFlow.value)
const deleteFlowDialogVisible = computed({
  get: () => flowDeleteTarget.value !== null,
  set: (visible: boolean) => {
    if (!visible) cancelDeleteFlow()
  },
})

onMounted(async () => {
  await loadPage()
})

async function loadPage(): Promise<void> {
  if (!projectId.value) return

  flowErrorMessage.value = null
  await Promise.all([
    projectStore.loadProject(projectId.value),
    loadFlows(),
    loadLocations(),
  ])
  resetProjectEditForm()
}

async function loadLocations(): Promise<void> {
  if (!projectId.value) return

  locationLoading.value = true
  locationErrorMessage.value = null
  try {
    const locations = await fetchTransportLocations(projectId.value)
    transportLocations.value = locations.slice().sort((a, b) => a.sortOrder - b.sortOrder || a.name.localeCompare(b.name, 'ja'))
  } catch (error) {
    locationErrorMessage.value = normalizeApiError(error).message
  } finally {
    locationLoading.value = false
  }
}

async function handleLocationCreated(): Promise<void> {
  await loadLocations()
  if (!locationErrorMessage.value) {
    locationDialogVisible.value = false
  }
}

async function loadFlows(): Promise<void> {
  if (!projectId.value) return

  try {
    await flowStore.loadFlows(projectId.value)
  } catch (error) {
    flowErrorMessage.value = normalizeApiError(error).message
  }
}

function resetProjectEditForm(): void {
  projectEditName.value = projectStore.currentProject?.name ?? ''
  projectEditDescription.value = projectStore.currentProject?.description ?? ''
}

function startProjectEdit(): void {
  if (!canUpdateProject.value) return
  resetProjectEditForm()
  editingProject.value = true
}

function cancelProjectEdit(): void {
  editingProject.value = false
  resetProjectEditForm()
}

async function saveProject(): Promise<void> {
  const name = projectEditName.value.trim()
  if (!name || !canUpdateProject.value) return

  const project = await projectStore.update(projectId.value, {
    name,
    description: projectEditDescription.value.trim() || null,
  })
  if (!project) return

  editingProject.value = false
  resetProjectEditForm()
}

async function createFlow(): Promise<void> {
  const name = flowName.value.trim()
  if (!name || !canCreateFlow.value) return

  flowErrorMessage.value = null
  creatingFlow.value = true
  try {
    const flow = await flowStore.create(projectId.value, {
      name,
      flowType: flowType.value,
      description: flowDescription.value.trim() || null,
    })
    closeCreateFlowDialog()
    await router.push({ name: 'flow-editor', params: { projectId: projectId.value, flowId: flow.flowId } })
  } catch (error) {
    flowErrorMessage.value = normalizeApiError(error).message
  } finally {
    creatingFlow.value = false
  }
}

function openCreateFlowDialog(): void {
  if (!canCreateFlow.value) return
  flowName.value = ''
  flowDescription.value = ''
  flowType.value = 'NORMAL'
  createFlowDialogVisible.value = true
}

function closeCreateFlowDialog(): void {
  createFlowDialogVisible.value = false
  flowName.value = ''
  flowDescription.value = ''
  flowType.value = 'NORMAL'
}

function startFlowEdit(flowId: string, name: string, flowTypeValue: FlowType, description?: string | null): void {
  if (!canCreateFlow.value) return
  editingFlowId.value = flowId
  editingFlowName.value = name
  editingFlowType.value = flowTypeValue
  editingFlowDescription.value = description ?? ''
}

function cancelFlowEdit(): void {
  editingFlowId.value = null
  editingFlowName.value = ''
  editingFlowDescription.value = ''
  editingFlowType.value = 'NORMAL'
}

async function saveFlow(flowId: string): Promise<void> {
  const name = editingFlowName.value.trim()
  if (!name || !canCreateFlow.value) return

  savingFlowId.value = flowId
  flowErrorMessage.value = null
  try {
    await flowStore.update(projectId.value, flowId, {
      name,
      flowType: editingFlowType.value,
      description: editingFlowDescription.value.trim() || null,
    })
    cancelFlowEdit()
  } catch (error) {
    flowErrorMessage.value = normalizeApiError(error).message
  } finally {
    savingFlowId.value = null
  }
}

async function openFlow(flowId: string): Promise<void> {
  await router.push({ name: 'flow-editor', params: { projectId: projectId.value, flowId } })
}

async function duplicateFlow(flowId: string): Promise<void> {
  if (!canCreateFlow.value) return

  duplicatingFlowId.value = flowId
  flowErrorMessage.value = null
  try {
    const duplicated = await flowStore.duplicate(projectId.value, flowId)
    await router.push({ name: 'flow-editor', params: { projectId: projectId.value, flowId: duplicated.flowId } })
  } catch (error) {
    flowErrorMessage.value = normalizeApiError(error).message
  } finally {
    duplicatingFlowId.value = null
  }
}

function requestDeleteFlow(flowId: string, name: string): void {
  if (!canCreateFlow.value) return
  flowDeleteTarget.value = { flowId, name }
}

function cancelDeleteFlow(): void {
  flowDeleteTarget.value = null
}

async function deleteFlow(): Promise<void> {
  if (!flowDeleteTarget.value || !canCreateFlow.value) return

  deletingFlowId.value = flowDeleteTarget.value.flowId
  flowErrorMessage.value = null
  try {
    await flowStore.remove(projectId.value, flowDeleteTarget.value.flowId)
    flowDeleteTarget.value = null
  } catch (error) {
    flowErrorMessage.value = normalizeApiError(error).message
  } finally {
    deletingFlowId.value = null
  }
}

async function backToProjects(): Promise<void> {
  await router.push({ name: 'project-list' })
}

function formatDate(value: string): string {
  return new Date(value).toLocaleString()
}

function flowTypeLabel(value: FlowType): string {
  return value === 'TRANSPORT' ? 'Transport' : 'Normal'
}

function flowTypeDescription(value: FlowType): string {
  return value === 'TRANSPORT' ? 'AGF/AGV搬送フロー' : '通常フロー'
}
</script>

<template>
  <MainLayout>
    <section class="page project-detail-page">
      <div class="card page-header">
        <div v-if="!editingProject">
          <h1>{{ projectStore.currentProject?.name ?? 'プロジェクト詳細' }}</h1>
          <p>{{ projectStore.currentProject?.description || 'Project詳細とFlow一覧を表示します。' }}</p>
          <p class="meta">projectId: {{ projectId }}</p>
        </div>
        <div v-else class="edit-panel">
          <h1>Project編集</h1>
          <div class="form-grid single-column">
            <label>
              Project名
              <input v-model="projectEditName" type="text" :disabled="projectStore.saving" />
            </label>
            <label>
              説明
              <textarea v-model="projectEditDescription" rows="3" :disabled="projectStore.saving" />
            </label>
          </div>
        </div>
        <div class="header-actions">
          <Button label="Project一覧へ戻る" severity="secondary" @click="backToProjects()" />
          <Button v-if="!editingProject && canUpdateProject" label="Project編集" severity="secondary" @click="startProjectEdit()" />
          <Button v-if="editingProject" label="キャンセル" severity="secondary" :disabled="projectStore.saving" @click="cancelProjectEdit()" />
          <Button v-if="editingProject" label="保存" :disabled="!canSubmitProject" @click="saveProject()" />
          <Button label="再読み込み" severity="secondary" :disabled="projectStore.loading || flowStore.loading" @click="loadPage()" />
        </div>
      </div>

      <p v-if="projectStore.errorMessage" class="error-message">{{ projectStore.errorMessage }}</p>
      <p v-if="flowErrorMessage" class="error-message">{{ flowErrorMessage }}</p>
      <p v-if="locationErrorMessage" class="error-message">{{ locationErrorMessage }}</p>

      <div class="card">
        <div class="section-header">
          <div>
            <h2>ロケーション一覧</h2>
            <p>このProjectのTransport Flowで共通利用するロケーションです。</p>
          </div>
          <div class="section-actions">
            <span>{{ transportLocations.length }}件</span>
            <Button
              v-if="canCreateFlow"
              label="新規ロケーション"
              icon="pi pi-plus"
              size="small"
              :disabled="locationLoading"
              @click="locationDialogVisible = true"
            />
          </div>
        </div>
        <p v-if="!canCreateFlow" class="viewer-message">Viewer権限ではロケーションを作成できません。</p>
        <p v-if="locationLoading">読み込み中...</p>
        <p v-else-if="transportLocations.length === 0">ロケーションはまだありません。</p>
        <table v-else class="flow-table">
          <thead>
            <tr>
              <th>ロケーション名</th>
              <th>種別</th>
              <th>説明</th>
              <th>表示順</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="location in transportLocations" :key="location.locationId">
              <td>{{ location.name }}</td>
              <td>{{ location.locationType }}</td>
              <td>{{ location.description || '説明なし' }}</td>
              <td>{{ location.sortOrder }}</td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="card">
        <div class="section-header">
          <h2>Flow一覧</h2>
          <div class="section-actions">
            <span>{{ flowStore.flows.length }}件</span>
            <Button v-if="canCreateFlow" label="新規Flow" icon="pi pi-plus" size="small" @click="openCreateFlowDialog()" />
          </div>
        </div>
        <p v-if="!canCreateFlow" class="viewer-message">Viewer権限ではFlow作成・編集はできません。</p>
        <p v-if="flowStore.loading">読み込み中...</p>
        <p v-else-if="flowStore.flows.length === 0">Flowはまだありません。</p>
        <table v-else class="flow-table">
          <thead>
            <tr>
              <th>Flow名</th>
              <th>種別</th>
              <th>説明</th>
              <th>更新日時</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="flow in flowStore.flows" :key="flow.flowId">
              <template v-if="editingFlowId === flow.flowId">
                <td>
                  <input v-model="editingFlowName" type="text" :disabled="savingFlowId === flow.flowId" />
                </td>
                <td>
                  <select v-model="editingFlowType" :disabled="savingFlowId === flow.flowId">
                    <option v-for="option in flowTypeOptions" :key="option.value" :value="option.value">
                      {{ option.label }} - {{ option.description }}
                    </option>
                  </select>
                </td>
                <td>
                  <textarea v-model="editingFlowDescription" rows="2" :disabled="savingFlowId === flow.flowId" />
                </td>
                <td>{{ formatDate(flow.updatedAtUtc) }}</td>
                <td>
                  <div class="row-actions">
                    <Button label="保存" size="small" :disabled="!canSubmitFlowEdit" @click="saveFlow(flow.flowId)" />
                    <Button label="キャンセル" size="small" severity="secondary" :disabled="savingFlowId === flow.flowId" @click="cancelFlowEdit()" />
                  </div>
                </td>
              </template>
              <template v-else>
                <td>{{ flow.name }}</td>
                <td>
                  <span class="flow-type-badge" :class="{ 'is-transport': flow.flowType === 'TRANSPORT' }">
                    {{ flowTypeLabel(flow.flowType) }}
                  </span>
                </td>
                <td>{{ flow.description || '説明なし' }}</td>
                <td>{{ formatDate(flow.updatedAtUtc) }}</td>
                <td>
                  <div class="row-actions">
                    <Button label="Editor" size="small" @click="openFlow(flow.flowId)" />
                    <Button
                      v-if="canCreateFlow"
                      label="複製"
                      icon="pi pi-copy"
                      size="small"
                      severity="secondary"
                      :disabled="duplicatingFlowId === flow.flowId"
                      @click="duplicateFlow(flow.flowId)"
                    />
                    <Button v-if="canCreateFlow" label="編集" size="small" severity="secondary" @click="startFlowEdit(flow.flowId, flow.name, flow.flowType, flow.description)" />
                    <Button
                      v-if="canCreateFlow"
                      label="削除"
                      size="small"
                      severity="danger"
                      :disabled="deletingFlowId === flow.flowId"
                      @click="requestDeleteFlow(flow.flowId, flow.name)"
                    />
                  </div>
                </td>
              </template>
            </tr>
          </tbody>
        </table>
      </div>

      <Dialog v-model:visible="createFlowDialogVisible" modal header="Flow作成" :style="{ width: 'min(520px, 92vw)' }">
        <div class="dialog-body">
          <label>
            Flow名
            <input v-model="flowName" type="text" placeholder="例: 入庫搬送フロー" :disabled="creatingFlow" />
          </label>
          <label>
            種別
            <select v-model="flowType" :disabled="creatingFlow">
              <option v-for="option in flowTypeOptions" :key="option.value" :value="option.value">
                {{ option.label }} - {{ option.description }}
              </option>
            </select>
            <span class="field-help">{{ flowTypeDescription(flowType) }}</span>
          </label>
          <label>
            説明
            <textarea v-model="flowDescription" rows="4" placeholder="Flowの対象工程や目的を入力" :disabled="creatingFlow" />
          </label>
        </div>
        <template #footer>
          <Button label="キャンセル" severity="secondary" :disabled="creatingFlow" @click="closeCreateFlowDialog()" />
          <Button label="作成してEditorを開く" icon="pi pi-arrow-right" :disabled="!canSubmitFlow" @click="createFlow()" />
        </template>
      </Dialog>

      <Dialog v-model:visible="deleteFlowDialogVisible" modal header="Flow削除" :style="{ width: 'min(440px, 92vw)' }">
        <p class="confirm-message">{{ flowDeleteTarget?.name }} を削除します。</p>
        <template #footer>
          <Button label="キャンセル" severity="secondary" :disabled="deletingFlowId !== null" @click="cancelDeleteFlow()" />
          <Button label="削除" severity="danger" :disabled="deletingFlowId !== null" @click="deleteFlow()" />
        </template>
      </Dialog>

      <LocationCreateDialog
        v-model:visible="locationDialogVisible"
        :project-id="projectId"
        :readonly="!canCreateFlow"
        @created="handleLocationCreated"
      />
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
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
}

.header-actions,
.section-actions,
.actions,
.row-actions {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 8px;
  flex-wrap: wrap;
}

.page-header h1,
.create-card h2,
.section-header h2 {
  margin: 0 0 8px;
}

.meta {
  font-size: 0.85rem;
}

.edit-panel {
  flex: 1 1 auto;
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 12px;
}

.single-column {
  grid-template-columns: 1fr;
}

label {
  display: flex;
  flex-direction: column;
  gap: 6px;
  font-weight: 600;
}

input,
select,
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

.dialog-body {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.field-help {
  color: var(--text-sub);
  font-size: 0.85rem;
  font-weight: 400;
}

.confirm-message {
  margin: 0;
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

.flow-type-badge {
  display: inline-flex;
  align-items: center;
  padding: 3px 8px;
  border-radius: 999px;
  border: 1px solid var(--border);
  font-size: 0.8rem;
  font-weight: 700;
}

.flow-type-badge.is-transport {
  border-style: dashed;
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

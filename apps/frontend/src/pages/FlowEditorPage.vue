<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import MainLayout from '../layouts/MainLayout.vue'
import EditorLayout from '../layouts/EditorLayout.vue'
import FlowCanvas from '../components/flow/FlowCanvas.vue'
import FlowOperationPanel from '../components/flow/FlowOperationPanel.vue'
import LaneStagePanel from '../components/flow/LaneStagePanel.vue'
import LinkPropertyPanel from '../components/flow/LinkPropertyPanel.vue'
import NodePropertyPanel from '../components/flow/NodePropertyPanel.vue'
import { useEditorStore } from '../stores/editorStore'
import { useFlowStore } from '../stores/flowStore'
import { useProjectPermissionStore } from '../stores/projectPermissionStore'
import { useUndoRedoStore } from '../stores/undoRedoStore'
import { PermissionCodes } from '../types/permission'
import { exportAiDsl, exportJson, exportMermaid } from '../api/exportApi'

const route = useRoute()
const router = useRouter()
const flowStore = useFlowStore()
const editorStore = useEditorStore()
const permissionStore = useProjectPermissionStore()
const undoRedoStore = useUndoRedoStore()

const projectId = computed(() => String(route.params.projectId ?? ''))
const flowId = computed(() => String(route.params.flowId ?? ''))
const flow = computed(() => flowStore.currentFlow)
const canUpdateFlow = computed(() => permissionStore.can(PermissionCodes.FlowUpdate))
const canUpdateNode = computed(() => permissionStore.can(PermissionCodes.NodeUpdate))
const canUpdateLink = computed(() => permissionStore.can(PermissionCodes.NodeUpdate) || permissionStore.can(PermissionCodes.LinkUpdate))
const canUpdateComment = computed(() => permissionStore.can(PermissionCodes.CommentUpdate))
const canSaveStructure = computed(() => Boolean(flow.value) && !flowStore.loading)
const canEdit = computed(() => canSaveStructure.value || canUpdateFlow.value || canUpdateNode.value || canUpdateLink.value || canUpdateComment.value)
const canCreateVersion = computed(() => Boolean(flow.value) && !flowStore.loading)
const canExport = computed(() => Boolean(flow.value) && !flowStore.loading)
const hasDetailPanel = computed(() => Boolean(editorStore.selectedNodeId || editorStore.selectedLinkId))
const settingsDialogVisible = ref(false)
const saveMessage = ref<string | null>(null)
const saveError = ref<string | null>(null)
const stageWidth = 240
const nodeX = 42

onMounted(async () => {
  editorStore.reset()
  undoRedoStore.reset()
  if (projectId.value && flowId.value) {
    await flowStore.loadFlow(projectId.value, flowId.value)
    editorStore.initHistory()
  }

  window.addEventListener('keydown', handleKeydown)
})

onUnmounted(() => {
  window.removeEventListener('keydown', handleKeydown)
})

watch(flow, (currentFlow) => {
  if (!currentFlow) {
    editorStore.clearSelection()
    return
  }

  editorStore.clearMissingSelection(
    currentFlow.nodes.map((node) => node.nodeId),
    currentFlow.links.map((link) => link.linkId),
  )
})

function buildSaveRequest(createVersion: boolean, changeSummary: string | null = null) {
  if (!flow.value) return null

  const sortedStages = flow.value.stages.slice().sort((a, b) => a.sortOrder - b.sortOrder)
  const sortedLanes = flow.value.lanes.slice().sort((a, b) => a.sortOrder - b.sortOrder)
  const validStageIds = new Set(sortedStages.map((stage) => stage.stageId))
  const validLaneIds = new Set(sortedLanes.map((lane) => lane.laneId))

  return {
    flowId: flow.value.flowId,
    clientRevision: flow.value.currentRevision,
    lanes: flow.value.lanes.map((lane) => ({
      laneId: lane.laneId,
      name: lane.name,
      sortOrder: lane.sortOrder,
    })),
    stages: flow.value.stages.map((stage) => ({
      stageId: stage.stageId,
      name: stage.name,
      sortOrder: stage.sortOrder,
    })),
    nodes: flow.value.nodes.map((node) => ({
      nodeId: node.nodeId,
      laneId: node.laneId && validLaneIds.has(node.laneId) ? node.laneId : null,
      stageId: node.stageId && validStageIds.has(node.stageId) ? node.stageId : resolveStageIdByX(node.x, sortedStages),
      nodeType: node.nodeType,
      name: node.name,
      description: node.description,
      x: node.x,
      y: node.y,
    })),
    links: flow.value.links.map((link) => ({
      linkId: link.linkId,
      sourceNodeId: link.sourceNodeId,
      targetNodeId: link.targetNodeId,
      label: link.label,
      condition: link.condition,
    })),
    comments: flow.value.comments.map((comment) => ({
      commentId: comment.commentId,
      nodeId: comment.nodeId,
      text: comment.text,
      x: comment.x,
      y: comment.y,
    })),
    createVersion,
    changeSummary,
  }
}

function resolveStageIdByX(x: number, sortedStages: { stageId: string }[]): string | null {
  if (sortedStages.length === 0) return null
  const index = Math.max(0, Math.min(Math.round((x - nodeX) / stageWidth), sortedStages.length - 1))
  return sortedStages[index]?.stageId ?? null
}

async function saveCurrentStructure(): Promise<void> {
  const request = buildSaveRequest(false)
  if (!request || flowStore.loading) return

  saveMessage.value = '保存中です...'
  saveError.value = null
  try {
    await flowStore.saveStructure(projectId.value, request)
    editorStore.markSaved()
    saveMessage.value = '保存しました。'
  } catch (error) {
    saveMessage.value = null
    saveError.value = flowStore.lastError ?? getErrorMessage(error)
  }
}

async function saveBeforeExportIfNeeded(): Promise<boolean> {
  if (!editorStore.isDirty) {
    return true
  }

  const request = buildSaveRequest(false)
  if (!request || flowStore.loading) {
    return false
  }

  saveMessage.value = '出力前に保存しています...'
  saveError.value = null
  try {
    await flowStore.saveStructure(projectId.value, request)
    editorStore.markSaved()
    saveMessage.value = null
    return true
  } catch (error) {
    saveMessage.value = null
    saveError.value = flowStore.lastError ?? getErrorMessage(error)
    return false
  }
}

async function createVersionFromCurrentFlow(): Promise<void> {
  if (!flow.value || flowStore.loading) return

  const changeSummary = window.prompt('バージョンコメントを入力してください（任意）', '')
  if (changeSummary === null) {
    return
  }

  const request = buildSaveRequest(true, changeSummary || null)
  if (!request) return

  saveMessage.value = 'バージョンを作成中です...'
  saveError.value = null
  try {
    await flowStore.saveStructure(projectId.value, request)
    editorStore.markSaved()
    saveMessage.value = 'バージョンを作成しました。'
  } catch (error) {
    saveMessage.value = null
    saveError.value = flowStore.lastError ?? getErrorMessage(error)
  }
}

async function downloadMermaidExport(): Promise<void> {
  if (!flow.value || flowStore.loading) return

  saveMessage.value = null
  saveError.value = null
  try {
    if (!(await saveBeforeExportIfNeeded()) || !flow.value) return
    const result = await exportMermaid(projectId.value, flow.value.flowId)
    downloadBlob(new Blob([result.content], { type: 'text/plain;charset=utf-8' }), result.fileName || `${flow.value.name || 'flow'}.mmd`)
  } catch (error) {
    saveError.value = getErrorMessage(error)
  }
}

async function downloadAiDslExport(): Promise<void> {
  if (!flow.value || flowStore.loading) return

  saveMessage.value = null
  saveError.value = null
  try {
    if (!(await saveBeforeExportIfNeeded()) || !flow.value) return
    const result = await exportAiDsl(projectId.value, flow.value.flowId)
    downloadBlob(new Blob([result.content], { type: 'text/plain;charset=utf-8' }), result.fileName || `${flow.value.name || 'flow'}_ai.flowdsl.txt`)
  } catch (error) {
    saveError.value = getErrorMessage(error)
  }
}

async function downloadJsonExport(): Promise<void> {
  if (!flow.value || flowStore.loading) return

  saveMessage.value = null
  saveError.value = null
  try {
    if (!(await saveBeforeExportIfNeeded()) || !flow.value) return
    const blob = await exportJson(projectId.value, flow.value.flowId)
    downloadBlob(blob, `${flow.value.name || 'flow'}.json`)
  } catch (error) {
    saveError.value = getErrorMessage(error)
  }
}

function downloadBlob(blob: Blob, fileName: string): void {
  const url = URL.createObjectURL(blob)
  const anchor = document.createElement('a')
  anchor.href = url
  anchor.download = fileName
  anchor.click()
  URL.revokeObjectURL(url)
}

function getErrorMessage(error: unknown): string {
  if (error instanceof Error) return error.message
  return '処理に失敗しました。'
}

function goBack(): void {
  if (window.history.length > 1) {
    router.back()
    return
  }

  router.push({ name: 'project-detail', params: { projectId: projectId.value } })
}

function handleKeydown(event: KeyboardEvent): void {
  if (!canEdit.value) return

  const target = event.target as HTMLElement | null
  const tag = target?.tagName?.toLowerCase()
  const isTextEditing = tag === 'input' || tag === 'textarea' || tag === 'select' || target?.isContentEditable
  if (isTextEditing) {
    return
  }

  const zPressed = event.key.toLowerCase() === 'z'
  const yPressed = event.key.toLowerCase() === 'y'
  const withMod = event.ctrlKey || event.metaKey

  if (event.key === 'Delete') {
    event.preventDefault()
    if (editorStore.selectedNodeId) {
      editorStore.deleteNode({ nodeId: editorStore.selectedNodeId })
      return
    }
    if (editorStore.selectedLinkId) {
      editorStore.deleteLink({ linkId: editorStore.selectedLinkId })
    }
    return
  }

  if (!withMod) return

  if (zPressed && !event.shiftKey) {
    event.preventDefault()
    editorStore.undo()
    return
  }

  if (yPressed || (zPressed && event.shiftKey)) {
    event.preventDefault()
    editorStore.redo()
  }
}
</script>

<template>
  <MainLayout>
    <EditorLayout>
      <section class="flow-editor-page">
        <div class="flow-editor-header">
          <div class="title-area">
            <Button label="戻る" icon="pi pi-arrow-left" severity="secondary" class="back-button" @click="goBack" />
            <div class="flow-title">
              <h1>フローエディタ</h1>
              <p v-if="flow">
                {{ flow.name }} / revision {{ flow.currentRevision }}
                <span v-if="editorStore.isDirty" class="dirty-badge">未保存</span>
              </p>
              <p v-else>projectId: {{ projectId }} / flowId: {{ flowId }}</p>
              <p v-if="!canEdit" class="viewer-badge">閲覧モード：編集は無効です</p>
            </div>
          </div>
          <div class="header-actions">
            <Button label="設備/分類設定" severity="secondary" :disabled="!flow" @click="settingsDialogVisible = true" />
            <Button label="バージョン管理" severity="secondary" @click="router.push({ name: 'flow-versions', params: { projectId, flowId } })" />
            <Button label="バージョン作成" severity="secondary" :disabled="!canCreateVersion" @click="createVersionFromCurrentFlow" />
            <Button label="Mermaid出力" icon="pi pi-download" severity="secondary" :disabled="!canExport" @click="downloadMermaidExport" />
            <Button label="JSON出力" icon="pi pi-download" severity="secondary" :disabled="!canExport" @click="downloadJsonExport" />
            <Button label="AI用出力" icon="pi pi-download" severity="secondary" :disabled="!canExport" @click="downloadAiDslExport" />
            <Button label="元に戻す" severity="secondary" :disabled="!canEdit || !undoRedoStore.canUndo" @click="editorStore.undo" />
            <Button label="やり直す" severity="secondary" :disabled="!canEdit || !undoRedoStore.canRedo" @click="editorStore.redo" />
            <Button :label="flowStore.loading ? '保存中...' : '保存'" :disabled="!canSaveStructure" @click="saveCurrentStructure" />
          </div>
        </div>

        <p v-if="saveMessage" class="status-message success">{{ saveMessage }}</p>
        <p v-if="saveError" class="status-message error">{{ saveError }}</p>

        <p v-if="flowStore.loading" class="loading-text">読み込み中...</p>
        <div v-else-if="flow" class="editor-workspace" :class="{ 'details-open': hasDetailPanel }">
          <aside class="palette-column">
            <FlowOperationPanel :flow="flow" :readonly="!canEdit" />
          </aside>

          <div class="canvas-column">
            <FlowCanvas
              :flow="flow"
              :readonly="!canEdit"
              :selected-node-id="editorStore.selectedNodeId"
              :selected-link-id="editorStore.selectedLinkId"
              @add-node="editorStore.addNode"
              @add-link="editorStore.addLink"
              @node-moved="editorStore.moveNode"
              @node-selected="editorStore.selectNode($event.nodeId)"
              @link-selected="editorStore.selectLink($event.linkId)"
              @canvas-cleared="editorStore.clearSelection"
            />
          </div>

          <aside v-if="hasDetailPanel" class="detail-drawer">
            <NodePropertyPanel
              v-if="editorStore.selectedNodeId"
              :flow="flow"
              :node-id="editorStore.selectedNodeId"
              :readonly="!canEdit"
              @update-node="editorStore.updateNode"
              @delete-node="editorStore.deleteNode"
            />
            <LinkPropertyPanel
              v-if="editorStore.selectedLinkId"
              :flow="flow"
              :link-id="editorStore.selectedLinkId"
              :readonly="!canEdit"
              @update-link="editorStore.updateLink"
              @delete-link="editorStore.deleteLink"
            />
          </aside>
        </div>
        <p v-else class="loading-text">フローを取得できませんでした。</p>

        <Dialog v-model:visible="settingsDialogVisible" modal header="設備/分類設定" :style="{ width: 'min(720px, 94vw)' }">
          <LaneStagePanel
            v-if="flow"
            :flow="flow"
            :readonly="!canEdit"
            @add-lane="editorStore.addLane"
            @update-lane="editorStore.updateLane"
            @delete-lane="editorStore.deleteLane"
            @add-stage="editorStore.addStage"
            @update-stage="editorStore.updateStage"
            @delete-stage="editorStore.deleteStage"
          />
        </Dialog>
      </section>
    </EditorLayout>
  </MainLayout>
</template>

<style scoped>
.flow-editor-page {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.flow-editor-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
}

.title-area {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  min-width: 260px;
}

.back-button {
  flex: 0 0 auto;
}

.flow-title h1 {
  margin: 0;
}

.flow-title p {
  margin: 6px 0 0;
  color: #64748b;
}

.viewer-badge {
  color: #92400e !important;
  font-weight: 700;
}

.dirty-badge {
  display: inline-block;
  margin-left: 8px;
  padding: 2px 8px;
  color: #92400e;
  background: #fef3c7;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 700;
}

.header-actions {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 8px;
}

.status-message {
  margin: 0;
  padding: 10px 12px;
  border-radius: 8px;
  font-size: 0.9rem;
  font-weight: 700;
}

.status-message.success {
  color: #166534;
  background: #dcfce7;
}

.status-message.error {
  color: #991b1b;
  background: #fee2e2;
}

.editor-workspace {
  display: grid;
  grid-template-columns: 220px minmax(0, 1fr);
  gap: 16px;
  min-height: 680px;
}

.editor-workspace.details-open {
  grid-template-columns: 220px minmax(0, 1fr) 340px;
}

.palette-column,
.detail-drawer {
  min-width: 0;
}

.canvas-column {
  min-width: 0;
}

.loading-text {
  color: #64748b;
}
</style>

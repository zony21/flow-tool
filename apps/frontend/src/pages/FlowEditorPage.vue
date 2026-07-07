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
import { exportJson, exportMermaid } from '../api/exportApi'

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
const canUpdateLink = computed(() => permissionStore.can(PermissionCodes.LinkUpdate))
const canUpdateComment = computed(() => permissionStore.can(PermissionCodes.CommentUpdate))
const canSaveStructure = computed(
  () => canUpdateFlow.value && canUpdateNode.value && canUpdateLink.value && canUpdateComment.value,
)
const canEdit = computed(() => canUpdateFlow.value)
const canCreateVersion = computed(() => permissionStore.can(PermissionCodes.VersionCreate))
const canExport = computed(() => permissionStore.can(PermissionCodes.ExportExecute))
const hasDetailPanel = computed(() => Boolean(editorStore.selectedNodeId || editorStore.selectedLinkId))
const settingsDialogVisible = ref(false)

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

async function saveCurrentStructure(): Promise<void> {
  if (!flow.value || !canSaveStructure.value) return

  await flowStore.saveStructure(projectId.value, {
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
      laneId: node.laneId,
      stageId: node.stageId,
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
    createVersion: false,
    changeSummary: null,
  })
  editorStore.markSaved()
}

async function createVersionFromCurrentFlow(): Promise<void> {
  if (!flow.value || !canSaveStructure.value || !canCreateVersion.value) return

  const changeSummary = window.prompt('バージョンコメントを入力してください（任意）', '')
  if (changeSummary === null) {
    return
  }

  await flowStore.saveStructure(projectId.value, {
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
      laneId: node.laneId,
      stageId: node.stageId,
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
    createVersion: true,
    changeSummary: changeSummary || null,
  })
  editorStore.markSaved()
}

async function downloadMermaidExport(): Promise<void> {
  if (!flow.value || editorStore.isDirty) return

  const result = await exportMermaid(projectId.value, flow.value.flowId)
  downloadBlob(new Blob([result.content], { type: 'text/plain;charset=utf-8' }), result.fileName || `${flow.value.name || 'flow'}.mmd`)
}

async function downloadJsonExport(): Promise<void> {
  if (!flow.value || editorStore.isDirty) return

  const blob = await exportJson(projectId.value, flow.value.flowId)
  downloadBlob(blob, `${flow.value.name || 'flow'}.json`)
}

function downloadBlob(blob: Blob, fileName: string): void {
  const url = URL.createObjectURL(blob)
  const anchor = document.createElement('a')
  anchor.href = url
  anchor.download = fileName
  anchor.click()
  URL.revokeObjectURL(url)
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
          <div class="flow-title">
            <h1>フローエディタ</h1>
            <p v-if="flow">
              {{ flow.name }} / revision {{ flow.currentRevision }}
              <span v-if="editorStore.isDirty" class="dirty-badge">未保存</span>
            </p>
            <p v-else>projectId: {{ projectId }} / flowId: {{ flowId }}</p>
            <p v-if="!canEdit" class="viewer-badge">閲覧モード：編集は無効です</p>
          </div>
          <div class="header-actions">
            <Button label="戻る" icon="pi pi-arrow-left" severity="secondary" @click="goBack" />
            <Button label="設備/分類設定" severity="secondary" :disabled="!flow" @click="settingsDialogVisible = true" />
            <Button label="バージョン管理" severity="secondary" @click="router.push({ name: 'flow-versions', params: { projectId, flowId } })" />
            <Button label="バージョン作成" severity="secondary" :disabled="!flow || flowStore.loading || !canSaveStructure || !canCreateVersion" @click="createVersionFromCurrentFlow" />
            <Button label="Mermaid出力" icon="pi pi-download" severity="secondary" :disabled="!flow || flowStore.loading || !canExport || editorStore.isDirty" @click="downloadMermaidExport" />
            <Button label="JSON出力" icon="pi pi-download" severity="secondary" :disabled="!flow || flowStore.loading || !canExport || editorStore.isDirty" @click="downloadJsonExport" />
            <Button label="元に戻す" severity="secondary" :disabled="!canEdit || !undoRedoStore.canUndo" @click="editorStore.undo" />
            <Button label="やり直す" severity="secondary" :disabled="!canEdit || !undoRedoStore.canRedo" @click="editorStore.redo" />
            <Button label="保存" :disabled="!flow || flowStore.loading || !canSaveStructure || !editorStore.isDirty" @click="saveCurrentStructure" />
          </div>
        </div>

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
  gap: 12px;
  width: 100%;
  min-width: 0;
  min-height: calc(100vh - 76px);
}

.flow-editor-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
  width: 100%;
  min-width: 0;
}

.flow-title {
  flex: 0 0 auto;
  min-width: 220px;
}

.flow-title h1 {
  margin: 0;
  color: #0f172a;
  font-size: 1.35rem;
  line-height: 1.2;
  white-space: nowrap;
}

.flow-title p {
  margin: 4px 0 0;
  color: #64748b;
}

.header-actions {
  display: flex;
  flex: 1 1 auto;
  flex-wrap: wrap;
  align-items: center;
  justify-content: flex-end;
  gap: 8px;
  min-width: 0;
}

.editor-workspace {
  display: grid;
  grid-template-columns: 240px minmax(0, 1fr);
  gap: 12px;
  width: 100%;
  min-width: 0;
  flex: 1 1 auto;
  min-height: 0;
}

.editor-workspace.details-open {
  grid-template-columns: 240px minmax(0, 1fr) 340px;
}

.palette-column {
  min-width: 0;
  max-height: calc(100vh - 148px);
  overflow: auto;
}

.canvas-column {
  min-width: 0;
  overflow: hidden;
}

.canvas-column :deep(.flow-canvas) {
  width: 100%;
  height: calc(100vh - 148px);
  min-height: 640px;
}

.detail-drawer {
  min-width: 0;
  max-height: calc(100vh - 148px);
  overflow: auto;
}

.detail-drawer :deep(.node-property-panel),
.detail-drawer :deep(.link-property-panel),
.palette-column :deep(.operation-panel) {
  width: 100%;
  min-width: 0;
  box-sizing: border-box;
}

.viewer-badge {
  margin-top: 8px;
  color: #92400e;
  font-size: 12px;
  font-weight: 700;
}

.dirty-badge {
  display: inline-flex;
  margin-left: 8px;
  padding: 2px 6px;
  color: #92400e;
  background: #fef3c7;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 700;
}

.loading-text {
  margin: 0;
  color: #64748b;
}

@media (max-width: 1100px) {
  .flow-editor-header {
    align-items: stretch;
    flex-direction: column;
  }

  .header-actions {
    justify-content: flex-start;
  }

  .editor-workspace,
  .editor-workspace.details-open {
    grid-template-columns: 1fr;
  }

  .palette-column,
  .detail-drawer,
  .canvas-column :deep(.flow-canvas) {
    max-height: none;
    height: auto;
  }
}
</style>

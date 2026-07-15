<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import MainLayout from '../layouts/MainLayout.vue'
import EditorLayout from '../layouts/EditorLayout.vue'
import CommentManagementPanel from '../components/flow/CommentManagementPanel.vue'
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
import type { Stage, StageCategory, StageType } from '../types/flow'
import { exportAiDsl, exportApiSpecification, exportDesignDocument, exportJson, exportMermaid } from '../api/exportApi'

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
const canEdit = computed(() => canUpdateFlow.value || canUpdateNode.value || canUpdateLink.value || canUpdateComment.value)
const canSaveStructure = computed(() => Boolean(flow.value) && !flowStore.loading && canUpdateFlow.value)
const canCreateVersion = computed(() => canSaveStructure.value)
const canExport = computed(() => Boolean(flow.value) && !flowStore.loading)
const hasDetailPanel = computed(() => Boolean(editorStore.selectedNodeId || editorStore.selectedLinkId))

const settingsDialogVisible = ref(false)
const exportDialogVisible = ref(false)
const commentDialogVisible = ref(false)
const stageDialogVisible = ref(false)
const editingStageId = ref<string | null>(null)
const stageName = ref('')
const stageCategory = ref<StageCategory>('EQUIPMENT')
const stageType = ref<StageType>('AUTO')
const stageSortOrder = ref(1)
const stageDialogError = ref<string | null>(null)
const saveMessage = ref<string | null>(null)
const saveError = ref<string | null>(null)

const stageWidth = 240
const nodeX = 42
const minRowHeight = 132
const nodeVisualHeight = 100
const rowPaddingBottom = 80
const stageCategories: Array<{ value: StageCategory; label: string }> = [
  { value: 'PERSON', label: '人' }, { value: 'SERVER', label: 'サーバー' }, { value: 'PLC', label: 'PLC' },
  { value: 'WCS', label: 'WCS' }, { value: 'RCS', label: 'RCS' }, { value: 'AGF', label: 'AGF' },
  { value: 'AGV', label: 'AGV' }, { value: 'EQUIPMENT', label: 'その他機器' }, { value: 'OTHER', label: 'その他' },
]

onMounted(async () => {
  editorStore.reset()
  undoRedoStore.reset()
  if (projectId.value && flowId.value) {
    await flowStore.loadFlow(projectId.value, flowId.value)
    editorStore.initHistory()
  }
  window.addEventListener('keydown', handleKeydown)
})
onUnmounted(() => window.removeEventListener('keydown', handleKeydown))
watch(flow, (currentFlow) => {
  if (!currentFlow) { editorStore.clearSelection(); return }
  editorStore.clearMissingSelection(currentFlow.nodes.map((node) => node.nodeId), currentFlow.links.map((link) => link.linkId))
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
    lanes: flow.value.lanes.map((lane) => ({ laneId: lane.laneId, name: lane.name, sortOrder: lane.sortOrder })),
    stages: flow.value.stages.map((stage) => ({
      stageId: stage.stageId,
      name: stage.name,
      category: stage.category ?? 'EQUIPMENT',
      stageType: stage.stageType ?? 'AUTO',
      sortOrder: stage.sortOrder,
    })),
    nodes: flow.value.nodes.map((node) => ({
      nodeId: node.nodeId,
      laneId: node.laneId && validLaneIds.has(node.laneId) ? node.laneId : resolveLaneIdByY(node.y, sortedLanes),
      stageId: node.stageId && validStageIds.has(node.stageId) ? node.stageId : resolveStageIdByX(node.x, sortedStages),
      nodeType: node.nodeType,
      name: node.name,
      description: node.description,
      x: node.x,
      y: node.y,
      commandId: node.commandId ?? null,
      locationId: node.locationId ?? null,
      equipmentId: null,
      manufacturerVehicleTypeId: node.manufacturerVehicleTypeId ?? null,
      rwType: node.rwType ?? null,
    })),
    links: flow.value.links.map((link) => ({ linkId: link.linkId, sourceNodeId: link.sourceNodeId, targetNodeId: link.targetNodeId, label: link.label, condition: link.condition })),
    comments: flow.value.comments.map((comment) => ({ commentId: comment.commentId, nodeId: comment.nodeId, text: comment.text, x: comment.x, y: comment.y })),
    createVersion,
    changeSummary,
  }
}

function resolveStageIdByX(x: number, sortedStages: { stageId: string }[]): string | null {
  if (sortedStages.length === 0) return null
  const index = Math.max(0, Math.min(Math.round((x - nodeX) / stageWidth), sortedStages.length - 1))
  return sortedStages[index]?.stageId ?? null
}
function resolveLaneIdByY(y: number, sortedLanes: { laneId: string }[]): string | null {
  if (!flow.value || sortedLanes.length === 0) return null
  let top = 0
  for (const lane of sortedLanes) {
    const laneNodes = flow.value.nodes.filter((node) => node.laneId === lane.laneId)
    const maxNodeBottom = Math.max(0, ...laneNodes.map((node) => (Number.isFinite(node.y) ? node.y : 28) + nodeVisualHeight))
    const height = Math.max(minRowHeight, maxNodeBottom + rowPaddingBottom)
    if (y >= top && y < top + height) return lane.laneId
    top += height
  }
  return sortedLanes[sortedLanes.length - 1]?.laneId ?? null
}

function openCreateStage(): void {
  if (!canUpdateFlow.value || !flow.value) return
  editingStageId.value = null
  stageName.value = ''
  stageCategory.value = 'EQUIPMENT'
  stageType.value = 'AUTO'
  stageSortOrder.value = Math.max(0, ...flow.value.stages.map((stage) => stage.sortOrder)) + 1
  stageDialogError.value = null
  stageDialogVisible.value = true
}
function openEditStage(stage: Stage): void {
  if (!canUpdateFlow.value) return
  editingStageId.value = stage.stageId
  stageName.value = stage.name
  stageCategory.value = stage.category ?? 'EQUIPMENT'
  stageType.value = stage.stageType ?? 'AUTO'
  stageSortOrder.value = stage.sortOrder
  stageDialogError.value = null
  stageDialogVisible.value = true
}
function saveStage(): void {
  if (!flow.value || !canUpdateFlow.value) return
  const name = stageName.value.trim()
  if (!name) { stageDialogError.value = '機器名を入力してください。'; return }
  if (editingStageId.value) {
    const current = flow.value.stages.find((stage) => stage.stageId === editingStageId.value)
    if (!current) return
    editorStore.updateStage({ ...current, name, category: stageCategory.value, stageType: stageType.value, sortOrder: stageSortOrder.value })
  } else {
    const stage: Stage = { stageId: crypto.randomUUID(), flowId: flow.value.flowId, name, category: stageCategory.value, stageType: stageType.value, sortOrder: stageSortOrder.value }
    editorStore.applyFlowChange((current) => ({ ...current, stages: [...current.stages, stage] }), { actionKey: 'stage:add', coalesceWindowMs: 0 })
  }
  stageDialogVisible.value = false
}
function deleteStage(): void {
  if (!flow.value || !editingStageId.value || !canUpdateFlow.value) return
  if (flow.value.nodes.some((node) => node.stageId === editingStageId.value)) {
    stageDialogError.value = 'この機器にはNodeが配置されているため削除できません。'
    return
  }
  editorStore.deleteStage({ stageId: editingStageId.value })
  stageDialogVisible.value = false
}

async function saveCurrentStructure(): Promise<void> {
  const request = buildSaveRequest(false)
  if (!request || flowStore.loading || !canUpdateFlow.value) return
  saveMessage.value = '保存中です...'; saveError.value = null
  try { await flowStore.saveStructure(projectId.value, request); editorStore.markSaved(); saveMessage.value = '保存しました。' }
  catch (error) { saveMessage.value = null; saveError.value = flowStore.lastError ?? getErrorMessage(error) }
}
async function saveBeforeExportIfNeeded(): Promise<boolean> {
  if (!editorStore.isDirty) return true
  const request = buildSaveRequest(false)
  if (!request || flowStore.loading || !canUpdateFlow.value) return false
  saveMessage.value = '出力前に保存しています...'; saveError.value = null
  try { await flowStore.saveStructure(projectId.value, request); editorStore.markSaved(); saveMessage.value = null; return true }
  catch (error) { saveMessage.value = null; saveError.value = flowStore.lastError ?? getErrorMessage(error); return false }
}
async function createVersionFromCurrentFlow(): Promise<void> {
  if (!flow.value || flowStore.loading || !canUpdateFlow.value) return
  const changeSummary = window.prompt('バージョンコメントを入力してください（任意）', '')
  if (changeSummary === null) return
  const request = buildSaveRequest(true, changeSummary || null)
  if (!request) return
  saveMessage.value = 'バージョンを作成中です...'; saveError.value = null
  try { await flowStore.saveStructure(projectId.value, request); editorStore.markSaved(); saveMessage.value = 'バージョンを作成しました。' }
  catch (error) { saveMessage.value = null; saveError.value = flowStore.lastError ?? getErrorMessage(error) }
}

async function downloadMermaidExport(): Promise<void> { await runExport(async () => { const r = await exportMermaid(projectId.value, flow.value!.flowId); downloadBlob(new Blob([r.content], { type: 'text/plain;charset=utf-8' }), r.fileName || `${flow.value!.name}.mmd`) }) }
async function downloadAiDslExport(): Promise<void> { await runExport(async () => { const r = await exportAiDsl(projectId.value, flow.value!.flowId); downloadBlob(new Blob([r.content], { type: 'text/plain;charset=utf-8' }), r.fileName || `${flow.value!.name}_ai.flowdsl.txt`) }) }
async function downloadDesignDocumentExport(): Promise<void> { await runExport(async () => { const r = await exportDesignDocument(projectId.value, flow.value!.flowId); downloadBlob(new Blob([r.content], { type: 'text/markdown;charset=utf-8' }), r.fileName || `${flow.value!.name}_design.md`) }) }
async function downloadApiSpecificationExport(): Promise<void> { await runExport(async () => { const r = await exportApiSpecification(projectId.value, flow.value!.flowId); downloadBlob(new Blob([r.content], { type: 'text/markdown;charset=utf-8' }), r.fileName || `${flow.value!.name}_api_spec.md`) }) }
async function downloadJsonExport(): Promise<void> { await runExport(async () => { const blob = await exportJson(projectId.value, flow.value!.flowId); downloadBlob(blob, `${flow.value!.name}.json`) }) }
async function runExport(action: () => Promise<void>): Promise<void> {
  if (!flow.value || flowStore.loading) return
  saveMessage.value = null; saveError.value = null
  try { if (!(await saveBeforeExportIfNeeded()) || !flow.value) return; await action(); exportDialogVisible.value = false }
  catch (error) { saveError.value = getErrorMessage(error) }
}
function downloadBlob(blob: Blob, fileName: string): void { const url = URL.createObjectURL(blob); const anchor = document.createElement('a'); anchor.href = url; anchor.download = fileName; anchor.click(); URL.revokeObjectURL(url) }
function getErrorMessage(error: unknown): string { return error instanceof Error ? error.message : '処理に失敗しました。' }
function goBack(): void { if (window.history.length > 1) router.back(); else router.push({ name: 'project-detail', params: { projectId: projectId.value } }) }
function handleKeydown(event: KeyboardEvent): void {
  if (!canEdit.value) return
  const target = event.target as HTMLElement | null
  const tag = target?.tagName?.toLowerCase()
  if (tag === 'input' || tag === 'textarea' || tag === 'select' || target?.isContentEditable) return
  const zPressed = event.key.toLowerCase() === 'z'; const yPressed = event.key.toLowerCase() === 'y'; const withMod = event.ctrlKey || event.metaKey
  if (event.key === 'Delete') { event.preventDefault(); if (editorStore.selectedNodeId) editorStore.deleteNode({ nodeId: editorStore.selectedNodeId }); else if (editorStore.selectedLinkId) editorStore.deleteLink({ linkId: editorStore.selectedLinkId }); return }
  if (!withMod) return
  if (zPressed && !event.shiftKey) { event.preventDefault(); editorStore.undo(); return }
  if (yPressed || (zPressed && event.shiftKey)) { event.preventDefault(); editorStore.redo() }
}
</script>

<template>
  <MainLayout><EditorLayout><section class="flow-editor-page">
    <div class="flow-editor-header">
      <div class="flow-title"><h1>フローエディタ</h1><p v-if="flow">{{ flow.name }} / revision {{ flow.currentRevision }} <span v-if="editorStore.isDirty" class="dirty-badge">未保存</span></p><p v-else>projectId: {{ projectId }} / flowId: {{ flowId }}</p><p v-if="!canEdit" class="viewer-badge">閲覧モード：編集は無効です</p></div>
      <div class="header-actions">
        <Button label="設備/分類設定" severity="secondary" :disabled="!flow" @click="settingsDialogVisible = true" />
        <Button label="コメント" severity="secondary" :disabled="!flow" @click="commentDialogVisible = true" />
        <Button label="バージョン管理" severity="secondary" @click="router.push({ name: 'flow-versions', params: { projectId, flowId } })" />
        <Button label="バージョン作成" severity="secondary" :disabled="!canCreateVersion" @click="createVersionFromCurrentFlow" />
        <Button label="出力" icon="pi pi-download" severity="secondary" :disabled="!canExport" @click="exportDialogVisible = true" />
        <Button label="元に戻す" severity="secondary" :disabled="!canEdit || !undoRedoStore.canUndo" @click="editorStore.undo" />
        <Button label="やり直す" severity="secondary" :disabled="!canEdit || !undoRedoStore.canRedo" @click="editorStore.redo" />
        <Button :label="flowStore.loading ? '保存中...' : '保存'" :disabled="!canSaveStructure" @click="saveCurrentStructure" />
      </div>
    </div>
    <div v-if="saveMessage" class="status-message success"><span>{{ saveMessage }}</span><button type="button" class="status-close" @click="saveMessage = null">×</button></div>
    <div v-if="saveError" class="status-message error"><span>{{ saveError }}</span><button type="button" class="status-close" @click="saveError = null">×</button></div>
    <p v-if="flowStore.loading" class="loading-text">読み込み中...</p>
    <div v-else-if="flow" class="editor-workspace" :class="{ 'details-open': hasDetailPanel }">
      <aside class="palette-column"><FlowOperationPanel :flow="flow" :readonly="!canEdit" /></aside>
      <div class="canvas-column"><FlowCanvas :flow="flow" :readonly="!canUpdateFlow" :selected-node-id="editorStore.selectedNodeId" :selected-link-id="editorStore.selectedLinkId" @add-node="editorStore.addNode" @add-link="editorStore.addLink" @node-moved="editorStore.moveNode" @node-selected="editorStore.selectNode($event.nodeId)" @link-selected="editorStore.selectLink($event.linkId)" @stage-selected="openEditStage" @add-stage="openCreateStage" @canvas-cleared="editorStore.clearSelection" /></div>
      <aside v-if="hasDetailPanel" class="detail-drawer">
        <NodePropertyPanel v-if="editorStore.selectedNodeId" :flow="flow" :node-id="editorStore.selectedNodeId" :readonly="!canUpdateNode" @update-node="editorStore.updateNode" @delete-node="editorStore.deleteNode" />
        <LinkPropertyPanel v-if="editorStore.selectedLinkId" :flow="flow" :link-id="editorStore.selectedLinkId" :readonly="!canUpdateLink" @update-link="editorStore.updateLink" @delete-link="editorStore.deleteLink" />
      </aside>
    </div>
    <p v-else class="loading-text">フローを取得できませんでした。</p>
    <div class="footer-actions"><Button label="戻る" icon="pi pi-arrow-left" severity="secondary" @click="goBack" /></div>

    <Dialog v-model:visible="stageDialogVisible" modal :header="editingStageId ? '機器を編集' : '機器を追加'" :style="{ width: 'min(520px, 94vw)' }">
      <div class="stage-form">
        <label><span>機器名</span><input v-model="stageName" :disabled="!canUpdateFlow" /></label>
        <label><span>分類</span><select v-model="stageCategory" :disabled="!canUpdateFlow"><option v-for="item in stageCategories" :key="item.value" :value="item.value">{{ item.label }}</option></select></label>
        <label><span>処理区分</span><select v-model="stageType" :disabled="!canUpdateFlow"><option value="AUTO">AUTO</option><option value="MANUAL">MANUAL</option></select></label>
        <label><span>表示順</span><input v-model.number="stageSortOrder" type="number" min="1" :disabled="!canUpdateFlow" /></label>
        <p v-if="stageDialogError" class="dialog-error">{{ stageDialogError }}</p>
        <div class="dialog-actions"><Button label="キャンセル" severity="secondary" @click="stageDialogVisible = false" /><Button v-if="editingStageId" label="機器を削除" severity="danger" :disabled="!canUpdateFlow" @click="deleteStage" /><Button :label="editingStageId ? '更新' : '追加'" :disabled="!canUpdateFlow" @click="saveStage" /></div>
      </div>
    </Dialog>

    <Dialog v-model:visible="settingsDialogVisible" modal header="設備/分類設定" :style="{ width: 'min(720px, 94vw)' }"><LaneStagePanel v-if="flow" :flow="flow" :readonly="!canUpdateFlow" @add-lane="editorStore.addLane" @update-lane="editorStore.updateLane" @delete-lane="editorStore.deleteLane" @add-stage="openCreateStage" @update-stage="openEditStage" @delete-stage="openEditStage(flow.stages.find(stage => stage.stageId === $event.stageId)!)" /></Dialog>
    <Dialog v-model:visible="commentDialogVisible" modal header="コメント" :style="{ width: 'min(760px, 94vw)' }"><CommentManagementPanel v-if="flow" :flow="flow" :selected-node-id="editorStore.selectedNodeId" :readonly="!canUpdateComment" @add-comment="editorStore.addComment" @update-comment="editorStore.updateComment" @delete-comment="editorStore.deleteComment" /></Dialog>
    <Dialog v-model:visible="exportDialogVisible" modal header="出力" :style="{ width: 'min(560px, 94vw)' }"><div class="export-options"><button class="export-option" @click="downloadMermaidExport"><strong>Mermaid出力</strong><span>フロー図をMermaid形式で出力します。</span></button><button class="export-option" @click="downloadJsonExport"><strong>JSON出力</strong><span>構造データをJSON形式で出力します。</span></button><button class="export-option" @click="downloadAiDslExport"><strong>AI用出力</strong><span>AI解析向けDSL v2形式で出力します。</span></button><button class="export-option" @click="downloadDesignDocumentExport"><strong>設計書出力</strong><span>Markdownで出力します。</span></button><button class="export-option" @click="downloadApiSpecificationExport"><strong>API仕様出力</strong><span>API・通信仕様書として出力します。</span></button></div></Dialog>
  </section></EditorLayout></MainLayout>
</template>

<style scoped>
.flow-editor-page{display:flex;flex-direction:column;gap:10px;height:calc(100vh - 72px);min-height:0;overflow:hidden}.flow-editor-header{display:flex;flex:0 0 auto;align-items:flex-start;justify-content:space-between;gap:12px}.flow-title h1{margin:0}.flow-title p{margin:4px 0 0;color:#64748b}.viewer-badge{color:#92400e!important;font-weight:700}.dirty-badge{display:inline-block;margin-left:8px;padding:2px 8px;color:#92400e;background:#fef3c7;border-radius:999px;font-size:12px;font-weight:700}.header-actions{display:flex;flex-wrap:wrap;justify-content:flex-end;gap:6px}.status-message{display:flex;align-items:center;justify-content:space-between;gap:12px;padding:8px 12px;border-radius:8px;font-weight:700}.status-message.success{color:#166534;background:#dcfce7}.status-message.error,.dialog-error{color:#991b1b;background:#fee2e2}.status-close{width:24px;height:24px;border-radius:999px;border:1px solid currentColor;background:#fff;cursor:pointer}.editor-workspace{display:grid;flex:1 1 auto;grid-template-columns:220px minmax(0,1fr);gap:12px;min-height:0}.editor-workspace.details-open{grid-template-columns:220px minmax(0,1fr) 340px}.palette-column,.detail-drawer,.canvas-column{min-width:0;min-height:0}.canvas-column{display:flex}.canvas-column :deep(.flow-canvas){height:100%!important;min-height:0!important}.footer-actions{display:flex;flex:0 0 auto;padding-top:2px}.loading-text{color:#64748b}.export-options{display:grid;gap:10px}.export-option{display:flex;flex-direction:column;gap:4px;width:100%;padding:12px 14px;text-align:left;background:#fff;border:1px solid #dbe3ef;border-radius:10px;cursor:pointer}.stage-form{display:grid;gap:14px}.stage-form label{display:grid;gap:6px;font-weight:700}.stage-form input,.stage-form select{padding:9px 10px;border:1px solid #cbd5e1;border-radius:8px;font:inherit}.dialog-error{margin:0;padding:10px;border-radius:8px}.dialog-actions{display:flex;justify-content:flex-end;gap:8px}
</style>

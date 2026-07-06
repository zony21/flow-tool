<script setup lang="ts">
import { computed, onMounted, onUnmounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import MainLayout from '../layouts/MainLayout.vue'
import EditorLayout from '../layouts/EditorLayout.vue'
import FlowCanvas from '../components/flow/FlowCanvas.vue'
import LaneStagePanel from '../components/flow/LaneStagePanel.vue'
import LinkPropertyPanel from '../components/flow/LinkPropertyPanel.vue'
import NodePropertyPanel from '../components/flow/NodePropertyPanel.vue'
import { useEditorStore } from '../stores/editorStore'
import { useFlowStore } from '../stores/flowStore'
import { useProjectPermissionStore } from '../stores/projectPermissionStore'
import { useUndoRedoStore } from '../stores/undoRedoStore'
import { PermissionCodes } from '../types/permission'

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

  const changeSummary = window.prompt('Versionコメントを入力してください（任意）', '')
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

function handleKeydown(event: KeyboardEvent): void {
  if (!canEdit.value) return

  const target = event.target as HTMLElement | null
  const tag = target?.tagName?.toLowerCase()
  const isTextEditing = tag === 'input' || tag === 'textarea' || target?.isContentEditable
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
          <div>
            <h1>フローエディタ</h1>
            <p v-if="flow">
              {{ flow.name }} / revision {{ flow.currentRevision }}
              <span v-if="editorStore.isDirty" class="dirty-badge">未保存</span>
            </p>
            <p v-else>projectId: {{ projectId }} / flowId: {{ flowId }}</p>
            <p v-if="!canEdit" class="viewer-badge">Viewerモード: 編集は無効です</p>
          </div>
          <div class="header-actions">
            <Button label="Version管理" severity="secondary" @click="router.push({ name: 'flow-versions', params: { projectId, flowId } })" />
            <Button label="Version作成" severity="secondary" :disabled="!flow || flowStore.loading || !canSaveStructure || !canCreateVersion" @click="createVersionFromCurrentFlow" />
            <Button label="Undo" severity="secondary" :disabled="!canEdit || !undoRedoStore.canUndo" @click="editorStore.undo" />
            <Button label="Redo" severity="secondary" :disabled="!canEdit || !undoRedoStore.canRedo" @click="editorStore.redo" />
            <Button label="保存" :disabled="!flow || flowStore.loading || !canSaveStructure || !editorStore.isDirty" @click="saveCurrentStructure" />
          </div>
        </div>

        <p v-if="flowStore.loading">読み込み中...</p>
        <div v-else-if="flow" class="editor-workspace">
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
          <div class="property-panels">
            <LaneStagePanel
              :flow="flow"
              :readonly="!canEdit"
              @add-lane="editorStore.addLane"
              @update-lane="editorStore.updateLane"
              @delete-lane="editorStore.deleteLane"
              @add-stage="editorStore.addStage"
              @update-stage="editorStore.updateStage"
              @delete-stage="editorStore.deleteStage"
            />
            <NodePropertyPanel
              :flow="flow"
              :node-id="editorStore.selectedNodeId"
              :readonly="!canEdit"
              @update-node="editorStore.updateNode"
              @delete-node="editorStore.deleteNode"
            />
            <LinkPropertyPanel
              :flow="flow"
              :link-id="editorStore.selectedLinkId"
              :readonly="!canEdit"
              @update-link="editorStore.updateLink"
              @delete-link="editorStore.deleteLink"
            />
          </div>
        </div>
        <p v-else>Flowを取得できませんでした。</p>
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
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.flow-editor-header h1 {
  margin: 0;
}

.flow-editor-header p {
  margin: 4px 0 0;
  color: #64748b;
}

.editor-workspace {
  display: flex;
  align-items: stretch;
  gap: 16px;
}

.property-panels {
  display: flex;
  flex: 0 0 320px;
  flex-direction: column;
  gap: 16px;
  max-height: calc(100vh - 160px);
  overflow: auto;
}

.viewer-badge {
  margin-top: 8px;
  color: #92400e;
  font-size: 12px;
  font-weight: 700;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 8px;
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

.editor-workspace :deep(.flow-canvas) {
  flex: 1 1 auto;
}
</style>

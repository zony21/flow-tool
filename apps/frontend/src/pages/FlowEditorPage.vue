<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import Button from 'primevue/button'
import MainLayout from '../layouts/MainLayout.vue'
import EditorLayout from '../layouts/EditorLayout.vue'
import FlowCanvas from '../components/flow/FlowCanvas.vue'
import LinkPropertyPanel from '../components/flow/LinkPropertyPanel.vue'
import NodePropertyPanel from '../components/flow/NodePropertyPanel.vue'
import { useFlowStore } from '../stores/flowStore'
import { useProjectPermissionStore } from '../stores/projectPermissionStore'
import type { FlowLink, FlowNode } from '../types/flow'
import { PermissionCodes } from '../types/permission'

const route = useRoute()
const flowStore = useFlowStore()
const permissionStore = useProjectPermissionStore()

const projectId = computed(() => String(route.params.projectId ?? ''))
const flowId = computed(() => String(route.params.flowId ?? ''))
const flow = computed(() => flowStore.currentFlow)
const selectedNodeId = ref<string | null>(null)
const selectedLinkId = ref<string | null>(null)
const canEdit = computed(() => permissionStore.can(PermissionCodes.FlowUpdate))

onMounted(async () => {
  if (projectId.value && flowId.value) {
    await flowStore.loadFlow(projectId.value, flowId.value)
  }
})

watch(flow, (currentFlow) => {
  if (!currentFlow) {
    selectedNodeId.value = null
    return
  }

  if (selectedNodeId.value && !currentFlow.nodes.some((node) => node.nodeId === selectedNodeId.value)) {
    selectedNodeId.value = null
  }

  if (selectedLinkId.value && !currentFlow.links.some((link) => link.linkId === selectedLinkId.value)) {
    selectedLinkId.value = null
  }
})

async function saveCurrentStructure(): Promise<void> {
  if (!flow.value || !canEdit.value) return

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
}

function addNode(): void {
  if (!flow.value || !canEdit.value) return

  const defaultLane = flow.value.lanes.slice().sort((a, b) => a.sortOrder - b.sortOrder)[0]
  const defaultStage = flow.value.stages.slice().sort((a, b) => a.sortOrder - b.sortOrder)[0]
  const nodeIndex = flow.value.nodes.length
  const nodeId = crypto.randomUUID()

  flowStore.setCurrentFlow({
    ...flow.value,
    nodes: [
      ...flow.value.nodes,
      {
        nodeId,
        flowId: flow.value.flowId,
        laneId: defaultLane?.laneId,
        stageId: defaultStage?.stageId,
        nodeType: 'process',
        name: `Node ${nodeIndex + 1}`,
        description: null,
        x: 40 + (nodeIndex % 3) * 80,
        y: 40 + Math.floor(nodeIndex / 3) * 80,
      },
    ],
  })
  selectedNodeId.value = nodeId
  selectedLinkId.value = null
}

function addLink(payload: { sourceNodeId: string; targetNodeId: string }): void {
  if (!flow.value || !canEdit.value) return
  if (payload.sourceNodeId === payload.targetNodeId) return

  const exists = flow.value.links.some(
    (link) => link.sourceNodeId === payload.sourceNodeId && link.targetNodeId === payload.targetNodeId,
  )
  if (exists) return

  const linkId = crypto.randomUUID()
  flowStore.setCurrentFlow({
    ...flow.value,
    links: [
      ...flow.value.links,
      {
        linkId,
        flowId: flow.value.flowId,
        sourceNodeId: payload.sourceNodeId,
        targetNodeId: payload.targetNodeId,
        label: null,
        condition: null,
      },
    ],
  })
  selectedLinkId.value = linkId
}

function updateNodePosition(payload: { nodeId: string; x: number; y: number; laneId?: string; stageId?: string }): void {
  if (!flow.value || !canEdit.value) return

  flowStore.setCurrentFlow({
    ...flow.value,
    nodes: flow.value.nodes.map((node) =>
      node.nodeId === payload.nodeId
        ? {
            ...node,
            x: payload.x,
            y: payload.y,
            laneId: payload.laneId ?? null,
            stageId: payload.stageId ?? null,
          }
        : node,
    ),
  })
}

function updateNode(updatedNode: FlowNode): void {
  if (!flow.value || !canEdit.value) return

  flowStore.setCurrentFlow({
    ...flow.value,
    nodes: flow.value.nodes.map((node) => (node.nodeId === updatedNode.nodeId ? updatedNode : node)),
  })
}

function updateLink(updatedLink: FlowLink): void {
  if (!flow.value || !canEdit.value) return

  flowStore.setCurrentFlow({
    ...flow.value,
    links: flow.value.links.map((link) => (link.linkId === updatedLink.linkId ? updatedLink : link)),
  })
}

function deleteNode(payload: { nodeId: string }): void {
  if (!flow.value || !canEdit.value) return

  flowStore.setCurrentFlow({
    ...flow.value,
    nodes: flow.value.nodes.filter((node) => node.nodeId !== payload.nodeId),
    links: flow.value.links.filter((link) => link.sourceNodeId !== payload.nodeId && link.targetNodeId !== payload.nodeId),
    comments: flow.value.comments.filter((comment) => comment.nodeId !== payload.nodeId),
  })

  if (selectedNodeId.value === payload.nodeId) {
    selectedNodeId.value = null
  }
}

function handleNodeSelected(payload: { nodeId: string }): void {
  selectedNodeId.value = payload.nodeId
  selectedLinkId.value = null
}

function handleLinkSelected(payload: { linkId: string }): void {
  selectedLinkId.value = payload.linkId
  selectedNodeId.value = null
}

function handleCanvasCleared(): void {
  selectedNodeId.value = null
  selectedLinkId.value = null
}
</script>

<template>
  <MainLayout>
    <EditorLayout>
      <section class="flow-editor-page">
        <div class="flow-editor-header">
          <div>
            <h1>フローエディタ</h1>
            <p v-if="flow">{{ flow.name }} / revision {{ flow.currentRevision }}</p>
            <p v-else>projectId: {{ projectId }} / flowId: {{ flowId }}</p>
            <p v-if="!canEdit" class="viewer-badge">Viewerモード: 編集は無効です</p>
          </div>
          <Button label="保存" :disabled="!flow || flowStore.loading || !canEdit" @click="saveCurrentStructure" />
        </div>

        <p v-if="flowStore.loading">読み込み中...</p>
        <div v-else-if="flow" class="editor-workspace">
          <FlowCanvas
            :flow="flow"
            :readonly="!canEdit"
            :selected-node-id="selectedNodeId"
            :selected-link-id="selectedLinkId"
            @add-node="addNode"
            @add-link="addLink"
            @node-moved="updateNodePosition"
            @node-selected="handleNodeSelected"
            @link-selected="handleLinkSelected"
            @canvas-cleared="handleCanvasCleared"
          />
          <div class="property-panels">
            <NodePropertyPanel
              :flow="flow"
              :node-id="selectedNodeId"
              :readonly="!canEdit"
              @update-node="updateNode"
              @delete-node="deleteNode"
            />
            <LinkPropertyPanel
              :flow="flow"
              :link-id="selectedLinkId"
              :readonly="!canEdit"
              @update-link="updateLink"
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
  flex-direction: column;
  gap: 16px;
}

.viewer-badge {
  margin-top: 8px;
  color: #92400e;
  font-size: 12px;
  font-weight: 700;
}

.editor-workspace :deep(.flow-canvas) {
  flex: 1 1 auto;
}
</style>

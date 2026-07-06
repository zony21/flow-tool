<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import Button from 'primevue/button'
import MainLayout from '../layouts/MainLayout.vue'
import EditorLayout from '../layouts/EditorLayout.vue'
import FlowCanvas from '../components/flow/FlowCanvas.vue'
import NodePropertyPanel from '../components/flow/NodePropertyPanel.vue'
import { useFlowStore } from '../stores/flowStore'
import type { FlowNode } from '../types/flow'

const route = useRoute()
const flowStore = useFlowStore()

const projectId = computed(() => String(route.params.projectId ?? ''))
const flowId = computed(() => String(route.params.flowId ?? ''))
const flow = computed(() => flowStore.currentFlow)
const selectedNodeId = ref<string | null>(null)

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
})

async function saveCurrentStructure(): Promise<void> {
  if (!flow.value) return

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
  if (!flow.value) return

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
}

function addLink(payload: { sourceNodeId: string; targetNodeId: string }): void {
  if (!flow.value) return
  if (payload.sourceNodeId === payload.targetNodeId) return

  const exists = flow.value.links.some(
    (link) => link.sourceNodeId === payload.sourceNodeId && link.targetNodeId === payload.targetNodeId,
  )
  if (exists) return

  flowStore.setCurrentFlow({
    ...flow.value,
    links: [
      ...flow.value.links,
      {
        linkId: crypto.randomUUID(),
        flowId: flow.value.flowId,
        sourceNodeId: payload.sourceNodeId,
        targetNodeId: payload.targetNodeId,
        label: null,
        condition: null,
      },
    ],
  })
}

function updateNodePosition(payload: { nodeId: string; x: number; y: number; laneId?: string; stageId?: string }): void {
  if (!flow.value) return

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
  if (!flow.value) return

  flowStore.setCurrentFlow({
    ...flow.value,
    nodes: flow.value.nodes.map((node) => (node.nodeId === updatedNode.nodeId ? updatedNode : node)),
  })
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
          </div>
          <Button label="保存" :disabled="!flow || flowStore.loading" @click="saveCurrentStructure" />
        </div>

        <p v-if="flowStore.loading">読み込み中...</p>
        <div v-else-if="flow" class="editor-workspace">
          <FlowCanvas
            :flow="flow"
            :selected-node-id="selectedNodeId"
            @add-node="addNode"
            @add-link="addLink"
            @node-moved="updateNodePosition"
            @node-selected="selectedNodeId = $event.nodeId"
          />
          <NodePropertyPanel :flow="flow" :node-id="selectedNodeId" @update-node="updateNode" />
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

.editor-workspace :deep(.flow-canvas) {
  flex: 1 1 auto;
}
</style>

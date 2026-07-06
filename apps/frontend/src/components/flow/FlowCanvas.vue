<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { VueFlow, type Edge, type Node, type NodeDragEvent } from '@vue-flow/core'
import { Background } from '@vue-flow/background'
import { Controls } from '@vue-flow/controls'
import type { FlowDetail } from '../../types/flow'
import '@vue-flow/core/dist/style.css'
import '@vue-flow/core/dist/theme-default.css'

const props = defineProps<{
  flow: FlowDetail
}>()

const emit = defineEmits<{
  (event: 'add-node'): void
  (event: 'add-link', payload: { sourceNodeId: string; targetNodeId: string }): void
  (event: 'node-moved', payload: { nodeId: string; x: number; y: number; laneId?: string; stageId?: string }): void
}>()

const laneHeight = 140
const stageWidth = 240
const headerHeight = 48
const laneHeaderWidth = 160

const stageColumns = computed(() => props.flow.stages.slice().sort((a, b) => a.sortOrder - b.sortOrder))
const laneRows = computed(() => props.flow.lanes.slice().sort((a, b) => a.sortOrder - b.sortOrder))
const nodeOptions = computed(() =>
  props.flow.nodes.map((node) => ({
    nodeId: node.nodeId,
    label: `${node.nodeType}: ${node.name}`,
  })),
)

const selectedSourceNodeId = ref('')
const selectedTargetNodeId = ref('')

watch(
  nodeOptions,
  (options) => {
    if (!options.some((option) => option.nodeId === selectedSourceNodeId.value)) {
      selectedSourceNodeId.value = options[0]?.nodeId ?? ''
    }

    if (!options.some((option) => option.nodeId === selectedTargetNodeId.value)) {
      selectedTargetNodeId.value = options[1]?.nodeId ?? options[0]?.nodeId ?? ''
    }
  },
  { immediate: true },
)

const nodes = computed<Node[]>(() =>
  props.flow.nodes.map((flowNode) => {
    const laneIndex = Math.max(0, laneRows.value.findIndex((lane) => lane.laneId === flowNode.laneId))
    const stageIndex = Math.max(0, stageColumns.value.findIndex((stage) => stage.stageId === flowNode.stageId))

    const fallbackX = stageIndex * stageWidth + 40
    const fallbackY = laneIndex * laneHeight + 40

    return {
      id: flowNode.nodeId,
      type: 'default',
      label: `${flowNode.nodeType}: ${flowNode.name}`,
      position: {
        x: flowNode.x || fallbackX,
        y: flowNode.y || fallbackY,
      },
      data: flowNode,
      draggable: true,
    }
  }),
)

const edges = computed<Edge[]>(() =>
  props.flow.links.map((link) => ({
    id: link.linkId,
    source: link.sourceNodeId,
    target: link.targetNodeId,
    label: link.condition || link.label || undefined,
    type: 'default',
  })),
)

function clampIndex(index: number, max: number): number {
  if (max <= 0) return 0
  if (index < 0) return 0
  if (index >= max) return max - 1
  return index
}

function createLink(): void {
  if (!selectedSourceNodeId.value || !selectedTargetNodeId.value) {
    return
  }

  emit('add-link', {
    sourceNodeId: selectedSourceNodeId.value,
    targetNodeId: selectedTargetNodeId.value,
  })
}

function onNodeDragStop(event: NodeDragEvent): void {
  const node = event.node
  const laneIndex = clampIndex(Math.floor(node.position.y / laneHeight), laneRows.value.length)
  const stageIndex = clampIndex(Math.floor(node.position.x / stageWidth), stageColumns.value.length)
  const laneId = laneRows.value[laneIndex]?.laneId
  const stageId = stageColumns.value[stageIndex]?.stageId

  // Snap dropped nodes to the target lane/stage cell for stable layout.
  const snappedX = stageIndex * stageWidth + 40
  const snappedY = laneIndex * laneHeight + 40

  emit('node-moved', {
    nodeId: node.id,
    x: snappedX,
    y: snappedY,
    laneId,
    stageId,
  })
}
</script>

<template>
  <div class="flow-canvas">
    <div class="canvas-toolbar">
      <button type="button" class="canvas-button" @click="emit('add-node')">Node追加</button>
      <div class="link-builder">
        <select v-model="selectedSourceNodeId" class="canvas-select" :disabled="flow.nodes.length < 2">
          <option v-for="option in nodeOptions" :key="`source-${option.nodeId}`" :value="option.nodeId">
            {{ option.label }}
          </option>
        </select>
        <span class="link-arrow">→</span>
        <select v-model="selectedTargetNodeId" class="canvas-select" :disabled="flow.nodes.length < 2">
          <option v-for="option in nodeOptions" :key="`target-${option.nodeId}`" :value="option.nodeId">
            {{ option.label }}
          </option>
        </select>
        <button
          type="button"
          class="canvas-button"
          :disabled="flow.nodes.length < 2 || !selectedSourceNodeId || !selectedTargetNodeId"
          @click="createLink"
        >
          Link追加
        </button>
      </div>
    </div>

    <div class="stage-header" :style="{ marginLeft: `${laneHeaderWidth}px` }">
      <div
        v-for="stage in stageColumns"
        :key="stage.stageId"
        class="stage-cell"
        :style="{ width: `${stageWidth}px` }"
      >
        {{ stage.name }}
      </div>
    </div>

    <div class="lane-column" :style="{ top: `${headerHeight}px`, width: `${laneHeaderWidth}px` }">
      <div
        v-for="lane in laneRows"
        :key="lane.laneId"
        class="lane-cell"
        :style="{ height: `${laneHeight}px` }"
      >
        {{ lane.name }}
      </div>
    </div>

    <div class="grid-layer" :style="{ left: `${laneHeaderWidth}px`, top: `${headerHeight}px` }">
      <div
        v-for="lane in laneRows"
        :key="lane.laneId"
        class="grid-row"
        :style="{ height: `${laneHeight}px` }"
      >
        <div
          v-for="stage in stageColumns"
          :key="stage.stageId"
          class="grid-cell"
          :style="{ width: `${stageWidth}px` }"
        />
      </div>
    </div>

    <VueFlow class="vue-flow" :nodes="nodes" :edges="edges" :fit-view-on-init="true" @node-drag-stop="onNodeDragStop">
      <Background />
      <Controls />
    </VueFlow>
  </div>
</template>

<style scoped>
.flow-canvas {
  position: relative;
  width: 100%;
  height: calc(100vh - 160px);
  min-height: 560px;
  overflow: hidden;
  background: #f8fafc;
  border: 1px solid #dbe3ef;
  border-radius: 12px;
}

.canvas-toolbar {
  position: absolute;
  top: 8px;
  left: 8px;
  z-index: 4;
  display: flex;
  align-items: center;
  gap: 8px;
}

.link-builder {
  display: flex;
  align-items: center;
  gap: 6px;
}

.canvas-select {
  max-width: 200px;
  padding: 6px 8px;
  color: #0f172a;
  background: #fff;
  border: 1px solid #94a3b8;
  border-radius: 8px;
}

.link-arrow {
  color: #334155;
  font-weight: 700;
}

.canvas-button {
  padding: 6px 12px;
  color: #0f172a;
  background: #e2e8f0;
  border: 1px solid #94a3b8;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 700;
  cursor: pointer;
}

.canvas-button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.stage-header {
  position: absolute;
  top: 0;
  right: 0;
  z-index: 3;
  display: flex;
  height: 48px;
  background: #eef2ff;
  border-bottom: 1px solid #cbd5e1;
}

.stage-cell {
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  color: #1e293b;
  border-right: 1px solid #cbd5e1;
}

.lane-column {
  position: absolute;
  left: 0;
  bottom: 0;
  z-index: 3;
  background: #f1f5f9;
  border-right: 1px solid #cbd5e1;
}

.lane-cell {
  display: flex;
  align-items: center;
  padding: 0 16px;
  font-weight: 700;
  color: #334155;
  border-bottom: 1px solid #cbd5e1;
}

.grid-layer {
  position: absolute;
  right: 0;
  bottom: 0;
  z-index: 1;
  pointer-events: none;
}

.grid-row {
  display: flex;
}

.grid-cell {
  height: 100%;
  border-right: 1px solid #e2e8f0;
  border-bottom: 1px solid #e2e8f0;
  background: rgb(255 255 255 / 58%);
}

.vue-flow {
  position: absolute;
  inset: 48px 0 0 160px;
  z-index: 2;
  background: transparent;
}
</style>

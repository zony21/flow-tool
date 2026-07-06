<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import {
  VueFlow,
  type Edge,
  type EdgeMouseEvent,
  type Node,
  type NodeDragEvent,
  type NodeMouseEvent,
} from '@vue-flow/core'
import { Background } from '@vue-flow/background'
import { Controls } from '@vue-flow/controls'
import { getNodeTypeLabel, nodeSamples } from '../../constants/nodeSamples'
import type { FlowDetail } from '../../types/flow'
import '@vue-flow/core/dist/style.css'
import '@vue-flow/core/dist/theme-default.css'

const props = defineProps<{
  flow: FlowDetail
  selectedNodeId?: string | null
  selectedLinkId?: string | null
  readonly?: boolean
}>()

const emit = defineEmits<{
  (event: 'add-node', payload: { nodeType: string; name: string; laneId?: string; stageId?: string }): void
  (event: 'add-link', payload: { sourceNodeId: string; targetNodeId: string }): void
  (event: 'node-moved', payload: { nodeId: string; x: number; y: number; laneId?: string; stageId?: string }): void
  (event: 'node-selected', payload: { nodeId: string }): void
  (event: 'link-selected', payload: { linkId: string }): void
  (event: 'canvas-cleared'): void
}>()

const laneHeight = 160
const stageWidth = 260
const headerHeight = 56
const laneHeaderWidth = 170

const stageColumns = computed(() => props.flow.stages.slice().sort((a, b) => a.sortOrder - b.sortOrder))
const laneRows = computed(() => props.flow.lanes.slice().sort((a, b) => a.sortOrder - b.sortOrder))
const hasStructureGrid = computed(() => stageColumns.value.length > 0 && laneRows.value.length > 0)
const nodeOptions = computed(() =>
  props.flow.nodes.map((node) => ({
    nodeId: node.nodeId,
    label: `${getNodeTypeLabel(node.nodeType)}：${node.name}`,
  })),
)

const selectedSourceNodeId = ref('')
const selectedTargetNodeId = ref('')
const selectedNodeType = ref(nodeSamples[1].type)
const selectedLaneId = ref('')
const selectedStageId = ref('')

watch(
  () => [laneRows.value, stageColumns.value] as const,
  ([lanes, stages]) => {
    if (!lanes.some((lane) => lane.laneId === selectedLaneId.value)) {
      selectedLaneId.value = lanes[0]?.laneId ?? ''
    }
    if (!stages.some((stage) => stage.stageId === selectedStageId.value)) {
      selectedStageId.value = stages[0]?.stageId ?? ''
    }
  },
  { immediate: true },
)

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
    const selectedClass = flowNode.nodeId === props.selectedNodeId ? ' selected-flow-node' : ''

    return {
      id: flowNode.nodeId,
      type: 'default',
      label: `${getNodeTypeLabel(flowNode.nodeType)}\n${flowNode.name}`,
      position: {
        x: Number.isFinite(flowNode.x) ? flowNode.x : fallbackX,
        y: Number.isFinite(flowNode.y) ? flowNode.y : fallbackY,
      },
      data: flowNode,
      draggable: true,
      class: `node-shape node-shape-${flowNode.nodeType}${selectedClass}`,
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
    class: link.linkId === props.selectedLinkId ? 'selected-flow-link' : undefined,
  })),
)

function clampIndex(index: number, max: number): number {
  if (max <= 0) return 0
  if (index < 0) return 0
  if (index >= max) return max - 1
  return index
}

function createNode(): void {
  if (props.readonly || !hasStructureGrid.value) return
  const sample = nodeSamples.find((item) => item.type === selectedNodeType.value) ?? nodeSamples[1]
  emit('add-node', {
    nodeType: sample.type,
    name: sample.defaultName,
    laneId: selectedLaneId.value || undefined,
    stageId: selectedStageId.value || undefined,
  })
}

function createLink(): void {
  if (props.readonly) return
  if (!selectedSourceNodeId.value || !selectedTargetNodeId.value) return
  emit('add-link', {
    sourceNodeId: selectedSourceNodeId.value,
    targetNodeId: selectedTargetNodeId.value,
  })
}

function onNodeDragStop(event: NodeDragEvent): void {
  if (props.readonly) return
  const node = event.node
  const laneIndex = clampIndex(Math.floor(node.position.y / laneHeight), laneRows.value.length)
  const stageIndex = clampIndex(Math.floor(node.position.x / stageWidth), stageColumns.value.length)
  emit('node-moved', {
    nodeId: node.id,
    x: Math.max(0, node.position.x),
    y: Math.max(0, node.position.y),
    laneId: laneRows.value[laneIndex]?.laneId,
    stageId: stageColumns.value[stageIndex]?.stageId,
  })
}

function onNodeClick(event: NodeMouseEvent): void {
  emit('node-selected', { nodeId: event.node.id })
}

function onEdgeClick(event: EdgeMouseEvent): void {
  emit('link-selected', { linkId: event.edge.id })
}

function onPaneClick(_: unknown): void {
  emit('canvas-cleared')
}
</script>

<template>
  <div class="flow-canvas">
    <div class="canvas-toolbar">
      <div class="node-palette" aria-label="図形サンプル">
        <button
          v-for="sample in nodeSamples"
          :key="sample.type"
          type="button"
          class="sample-button"
          :class="[`sample-${sample.type}`, { selected: selectedNodeType === sample.type }]"
          :title="sample.description"
          :disabled="readonly || !hasStructureGrid"
          @click="selectedNodeType = sample.type"
        >
          {{ sample.label }}
        </button>
      </div>
      <select v-model="selectedStageId" class="canvas-select" :disabled="readonly || !hasStructureGrid">
        <option v-for="stage in stageColumns" :key="stage.stageId" :value="stage.stageId">{{ stage.name }} の下</option>
      </select>
      <select v-model="selectedLaneId" class="canvas-select" :disabled="readonly || !hasStructureGrid">
        <option v-for="lane in laneRows" :key="lane.laneId" :value="lane.laneId">{{ lane.name }} に配置</option>
      </select>
      <button type="button" class="canvas-button" :disabled="readonly || !hasStructureGrid" @click="createNode">図形追加</button>
      <div class="link-builder">
        <select v-model="selectedSourceNodeId" class="canvas-select" :disabled="readonly || flow.nodes.length < 2">
          <option v-for="option in nodeOptions" :key="`source-${option.nodeId}`" :value="option.nodeId">{{ option.label }}</option>
        </select>
        <span class="link-arrow">→</span>
        <select v-model="selectedTargetNodeId" class="canvas-select" :disabled="readonly || flow.nodes.length < 2">
          <option v-for="option in nodeOptions" :key="`target-${option.nodeId}`" :value="option.nodeId">{{ option.label }}</option>
        </select>
        <button type="button" class="canvas-button" :disabled="readonly || flow.nodes.length < 2 || !selectedSourceNodeId || !selectedTargetNodeId" @click="createLink">接続線追加</button>
      </div>
    </div>

    <div v-if="!hasStructureGrid" class="canvas-guidance">
      <h2>担当または設備が未作成です</h2>
      <p>図形は「担当」と「設備・場所」に紐付けて配置します。</p>
      <p>既存の空フローは右側の編集パネルから追加してください。</p>
    </div>

    <div class="stage-header" :style="{ marginLeft: `${laneHeaderWidth}px` }">
      <div v-for="stage in stageColumns" :key="stage.stageId" class="stage-cell" :style="{ width: `${stageWidth}px` }">{{ stage.name }}</div>
    </div>
    <div class="lane-column" :style="{ top: `${headerHeight}px`, width: `${laneHeaderWidth}px` }">
      <div v-for="lane in laneRows" :key="lane.laneId" class="lane-cell" :style="{ height: `${laneHeight}px` }">{{ lane.name }}</div>
    </div>
    <div class="grid-layer" :style="{ left: `${laneHeaderWidth}px`, top: `${headerHeight}px` }">
      <div v-for="lane in laneRows" :key="lane.laneId" class="grid-row" :style="{ height: `${laneHeight}px` }">
        <div v-for="stage in stageColumns" :key="stage.stageId" class="grid-cell" :style="{ width: `${stageWidth}px` }" />
      </div>
    </div>

    <VueFlow class="vue-flow" :nodes="nodes" :edges="edges" :fit-view-on-init="true" @node-drag-stop="onNodeDragStop" @node-click="onNodeClick" @edge-click="onEdgeClick" @pane-click="onPaneClick">
      <Background />
      <Controls />
    </VueFlow>
  </div>
</template>

<style scoped>
.flow-canvas { position: relative; width: 100%; height: calc(100vh - 160px); min-height: 620px; overflow: hidden; background: #f8fafc; border: 1px solid #dbe3ef; border-radius: 12px; }
.canvas-toolbar { position: absolute; top: 8px; left: 8px; right: 8px; z-index: 4; display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }
.node-palette, .link-builder { display: flex; align-items: center; gap: 6px; }
.sample-button { min-width: 72px; min-height: 38px; padding: 4px 8px; color: #0f172a; background: #fff; border: 2px solid #cbd5e1; border-radius: 10px; font-size: 12px; font-weight: 700; cursor: pointer; }
.sample-button.selected { border-color: #2563eb; box-shadow: 0 0 0 2px rgb(37 99 235 / 18%); }
.sample-button:disabled, .canvas-button:disabled { opacity: 0.5; cursor: not-allowed; }
.sample-start, .sample-end { border-radius: 999px; }
.sample-decision { transform: skew(-8deg); }
.sample-document { border-bottom-right-radius: 22px; }
.canvas-select { max-width: 220px; padding: 6px 8px; color: #0f172a; background: #fff; border: 1px solid #94a3b8; border-radius: 8px; }
.link-arrow { color: #334155; font-weight: 700; }
.canvas-button { padding: 6px 12px; color: #0f172a; background: #e2e8f0; border: 1px solid #94a3b8; border-radius: 8px; font-size: 13px; font-weight: 700; cursor: pointer; }
.canvas-guidance { position: absolute; top: 96px; left: 50%; z-index: 5; width: min(520px, calc(100% - 48px)); padding: 20px; color: #334155; background: #fff; border: 1px solid #cbd5e1; border-radius: 12px; box-shadow: 0 12px 32px rgb(15 23 42 / 14%); transform: translateX(-50%); }
.canvas-guidance h2 { margin: 0 0 8px; font-size: 1rem; color: #0f172a; }
.canvas-guidance p { margin: 6px 0 0; }
.stage-header { position: absolute; top: 0; right: 0; z-index: 3; display: flex; height: 56px; background: #eef2ff; border-bottom: 1px solid #cbd5e1; }
.stage-cell { display: flex; align-items: center; justify-content: center; padding: 0 12px; font-weight: 700; color: #1e293b; border-right: 1px solid #cbd5e1; }
.lane-column { position: absolute; left: 0; bottom: 0; z-index: 3; background: #f1f5f9; border-right: 1px solid #cbd5e1; }
.lane-cell { display: flex; align-items: center; padding: 0 16px; font-weight: 700; color: #334155; border-bottom: 1px solid #cbd5e1; }
.grid-layer { position: absolute; right: 0; bottom: 0; z-index: 1; pointer-events: none; }
.grid-row { display: flex; }
.grid-cell { height: 100%; border-right: 1px solid #e2e8f0; border-bottom: 1px solid #e2e8f0; background: rgb(255 255 255 / 58%); }
.vue-flow { position: absolute; inset: 56px 0 0 170px; z-index: 2; background: transparent; }
:deep(.node-shape) { min-width: 130px; padding: 10px 14px; white-space: pre-line; text-align: center; border: 2px solid #64748b; border-radius: 10px; box-shadow: 0 8px 18px rgb(15 23 42 / 12%); }
:deep(.node-shape-start), :deep(.node-shape-end) { border-radius: 999px; }
:deep(.node-shape-decision) { border-radius: 18px; transform: skew(-10deg); }
:deep(.node-shape-document) { border-bottom-right-radius: 28px; }
:deep(.node-shape-wait) { border-style: dashed; }
:deep(.selected-flow-node) { outline: 3px solid #2563eb; }
:deep(.selected-flow-link path) { stroke: #dc2626; stroke-width: 3; }
</style>

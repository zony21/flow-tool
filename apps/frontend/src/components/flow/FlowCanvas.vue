<script setup lang="ts">
import { computed, ref } from 'vue'
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
  (event: 'add-node', payload: { nodeType: string; stageId?: string; x: number; y: number }): void
  (event: 'node-moved', payload: { nodeId: string; x: number; y: number; stageId?: string }): void
  (event: 'node-selected', payload: { nodeId: string }): void
  (event: 'link-selected', payload: { linkId: string }): void
  (event: 'canvas-cleared'): void
}>()

const stageWidth = 260
const headerHeight = 76
const nodeColumnOffset = 54
const minCanvasHeight = 1200

const stageColumns = computed(() => props.flow.stages.slice().sort((a, b) => a.sortOrder - b.sortOrder))
const hasEquipment = computed(() => stageColumns.value.length > 0)
const activeStageId = ref<string | null>(null)

const canvasWidth = computed(() => Math.max(stageColumns.value.length * stageWidth, stageWidth))

const nodes = computed<Node[]>(() =>
  props.flow.nodes.map((flowNode) => {
    const stageIndex = Math.max(0, stageColumns.value.findIndex((stage) => stage.stageId === flowNode.stageId))
    const selectedClass = flowNode.nodeId === props.selectedNodeId ? ' selected-flow-node' : ''

    return {
      id: flowNode.nodeId,
      type: 'default',
      label: flowNode.name,
      position: {
        x: stageIndex * stageWidth + nodeColumnOffset,
        y: Number.isFinite(flowNode.y) ? flowNode.y : 100,
      },
      data: flowNode,
      draggable: !props.readonly,
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

function getCanvasPoint(event: DragEvent): { y: number; stageIndex: number } | null {
  const rect = (event.currentTarget as HTMLElement).getBoundingClientRect()
  const x = event.clientX - rect.left
  const y = event.clientY - rect.top - headerHeight
  if (x < 0 || y < 0 || stageColumns.value.length === 0) {
    return null
  }

  return {
    y,
    stageIndex: clampIndex(Math.floor(x / stageWidth), stageColumns.value.length),
  }
}

function onDragOver(event: DragEvent): void {
  if (props.readonly || !event.dataTransfer) return
  const point = getCanvasPoint(event)
  activeStageId.value = point ? stageColumns.value[point.stageIndex]?.stageId ?? null : null
  if (!point) return

  event.preventDefault()
  event.dataTransfer.dropEffect = 'copy'
}

function onDrop(event: DragEvent): void {
  if (props.readonly || !event.dataTransfer) return
  const nodeType = event.dataTransfer.getData('application/x-flow-node-type') || event.dataTransfer.getData('text/plain')
  const point = getCanvasPoint(event)
  activeStageId.value = null
  if (!nodeType || !point) return

  event.preventDefault()
  emit('add-node', {
    nodeType,
    stageId: stageColumns.value[point.stageIndex]?.stageId,
    x: point.stageIndex * stageWidth + nodeColumnOffset,
    y: Math.max(0, point.y),
  })
}

function onDragLeave(event: DragEvent): void {
  const current = event.currentTarget as HTMLElement
  const next = event.relatedTarget as globalThis.Node | null
  if (!next || !current.contains(next)) {
    activeStageId.value = null
  }
}

function onNodeDragStop(event: NodeDragEvent): void {
  if (props.readonly) return
  const node = event.node
  const stageIndex = clampIndex(Math.floor(node.position.x / stageWidth), stageColumns.value.length)
  emit('node-moved', {
    nodeId: node.id,
    x: stageIndex * stageWidth + nodeColumnOffset,
    y: Math.max(0, node.position.y),
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
  <div class="flow-canvas" @dragover="onDragOver" @drop="onDrop" @dragleave="onDragLeave">
    <div v-if="!hasEquipment" class="canvas-guidance">
      <h2>設備が未設定です</h2>
      <p>右側パネルで設備を追加してください。</p>
      <p>設備列にNode Paletteから図形をドラッグすると、ノードを配置できます。</p>
    </div>

    <div class="equipment-header" :style="{ width: `${canvasWidth}px` }">
      <div v-for="stage in stageColumns" :key="stage.stageId" class="equipment-cell" :style="{ width: `${stageWidth}px` }">
        <strong>{{ stage.name }}</strong>
        <span>設備</span>
      </div>
    </div>

    <div class="lane-layer" :style="{ top: `${headerHeight}px`, width: `${canvasWidth}px`, minHeight: `${minCanvasHeight}px` }">
      <div
        v-for="stage in stageColumns"
        :key="stage.stageId"
        class="equipment-lane"
        :class="{ active: activeStageId === stage.stageId }"
        :style="{ width: `${stageWidth}px` }"
      />
    </div>

    <VueFlow class="vue-flow" :nodes="nodes" :edges="edges" :fit-view-on-init="true" @node-drag-stop="onNodeDragStop" @node-click="onNodeClick" @edge-click="onEdgeClick" @pane-click="onPaneClick">
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
  min-height: 620px;
  overflow: auto;
  background: #f8fafc;
  border: 1px solid #dbe3ef;
  border-radius: 8px;
}

.canvas-guidance {
  position: absolute;
  top: 120px;
  left: 50%;
  z-index: 5;
  width: min(520px, calc(100% - 48px));
  padding: 20px;
  color: #334155;
  background: #fff;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  box-shadow: 0 12px 32px rgb(15 23 42 / 14%);
  transform: translateX(-50%);
}

.canvas-guidance h2 {
  margin: 0 0 8px;
  font-size: 1rem;
  color: #0f172a;
}

.canvas-guidance p {
  margin: 6px 0 0;
}

.equipment-header {
  position: sticky;
  top: 0;
  z-index: 4;
  display: flex;
  height: 76px;
  background: #eef2ff;
  border-bottom: 1px solid #cbd5e1;
}

.equipment-cell {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 4px;
  padding: 0 12px;
  color: #1e293b;
  border-right: 1px solid #cbd5e1;
}

.equipment-cell strong {
  font-size: 0.95rem;
}

.equipment-cell span {
  color: #64748b;
  font-size: 0.75rem;
}

.lane-layer {
  position: absolute;
  left: 0;
  z-index: 1;
  display: flex;
  pointer-events: none;
}

.equipment-lane {
  min-height: 100%;
  border-right: 1px solid #dbe3ef;
  background:
    linear-gradient(to bottom, rgb(148 163 184 / 16%) 1px, transparent 1px) 0 0 / 100% 120px,
    rgb(255 255 255 / 60%);
}

.equipment-lane:nth-child(even) {
  background:
    linear-gradient(to bottom, rgb(148 163 184 / 16%) 1px, transparent 1px) 0 0 / 100% 120px,
    rgb(248 250 252 / 90%);
}

.equipment-lane.active {
  background:
    linear-gradient(to bottom, rgb(37 99 235 / 18%) 1px, transparent 1px) 0 0 / 100% 120px,
    rgb(96 165 250 / 18%);
  box-shadow: inset 0 0 0 2px rgb(37 99 235 / 28%);
}

.vue-flow {
  position: absolute;
  top: 76px;
  left: 0;
  z-index: 2;
  width: 100%;
  min-width: 100%;
  height: calc(100% - 76px);
  min-height: 1200px;
  background: transparent;
}

:deep(.node-shape) {
  min-width: 130px;
  padding: 10px 14px;
  white-space: pre-line;
  text-align: center;
  border: 2px solid #64748b;
  border-radius: 8px;
  box-shadow: 0 8px 18px rgb(15 23 42 / 12%);
}

:deep(.node-shape-start),
:deep(.node-shape-end) {
  border-radius: 999px;
}

:deep(.node-shape-decision) {
  width: 108px;
  min-width: 108px;
  height: 108px;
  display: flex;
  align-items: center;
  justify-content: center;
  transform: rotate(45deg);
}

:deep(.node-shape-decision .vue-flow__node-default) {
  transform: rotate(-45deg);
}

:deep(.node-shape-document) {
  border-bottom-right-radius: 28px;
}

:deep(.node-shape-wait) {
  border-style: dashed;
}

:deep(.selected-flow-node) {
  outline: 3px solid #2563eb;
}

:deep(.selected-flow-link path) {
  stroke: #dc2626;
  stroke-width: 3;
}
</style>

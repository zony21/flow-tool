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
import FlowShapeNode from './FlowShapeNode.vue'
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
  (event: 'add-node', payload: { nodeType: string; stageId?: string; laneId?: string; x: number; y: number }): void
  (event: 'node-moved', payload: { nodeId: string; x: number; y: number; stageId?: string; laneId?: string }): void
  (event: 'node-selected', payload: { nodeId: string }): void
  (event: 'link-selected', payload: { linkId: string }): void
  (event: 'canvas-cleared'): void
}>()

const categoryWidth = 156
const stageWidth = 240
const headerHeight = 92
const categoryHeight = 132
const nodeColumnOffset = 50
const nodeRowOffset = 38
const fallbackCanvasHeight = 1200

const stageColumns = computed(() => props.flow.stages.slice().sort((a, b) => a.sortOrder - b.sortOrder))
const categoryRows = computed(() => props.flow.lanes.slice().sort((a, b) => a.sortOrder - b.sortOrder))
const hasEquipment = computed(() => stageColumns.value.length > 0)
const activeStageId = ref<string | null>(null)
const activeLaneId = ref<string | null>(null)

const bodyHeight = computed(() => Math.max(categoryRows.value.length * categoryHeight, fallbackCanvasHeight))
const canvasWidth = computed(() => Math.max(categoryWidth + stageColumns.value.length * stageWidth, categoryWidth + stageWidth))

const nodes = computed<Node[]>(() =>
  props.flow.nodes.map((flowNode) => {
    const stageIndex = Math.max(0, stageColumns.value.findIndex((stage) => stage.stageId === flowNode.stageId))
    const selectedClass = flowNode.nodeId === props.selectedNodeId ? ' selected-flow-node' : ''

    return {
      id: flowNode.nodeId,
      type: 'flowShape',
      position: {
        x: stageIndex * stageWidth + nodeColumnOffset,
        y: Number.isFinite(flowNode.y) ? flowNode.y : nodeRowOffset,
      },
      data: flowNode,
      draggable: !props.readonly,
      class: `flow-node-shell${selectedClass}`,
    }
  }),
)

const edges = computed<Edge[]>(() =>
  props.flow.links.map((link) => ({
    id: link.linkId,
    source: link.sourceNodeId,
    target: link.targetNodeId,
    label: link.condition || link.label || undefined,
    type: 'step',
    class: link.linkId === props.selectedLinkId ? 'selected-flow-link' : undefined,
  })),
)

function clampIndex(index: number, max: number): number {
  if (max <= 0) return 0
  if (index < 0) return 0
  if (index >= max) return max - 1
  return index
}

function resolveLaneIndex(y: number): number {
  if (categoryRows.value.length === 0) return 0
  return clampIndex(Math.floor(y / categoryHeight), categoryRows.value.length)
}

function snapYToLane(y: number): number {
  if (categoryRows.value.length === 0) return Math.max(nodeRowOffset, y)
  return resolveLaneIndex(y) * categoryHeight + nodeRowOffset
}

function getCanvasPoint(event: DragEvent): { y: number; stageIndex: number; laneIndex: number } | null {
  const rect = (event.currentTarget as HTMLElement).getBoundingClientRect()
  const x = event.clientX - rect.left - categoryWidth
  const y = event.clientY - rect.top - headerHeight
  if (x < 0 || y < 0 || stageColumns.value.length === 0) {
    return null
  }

  return {
    y,
    stageIndex: clampIndex(Math.floor(x / stageWidth), stageColumns.value.length),
    laneIndex: resolveLaneIndex(y),
  }
}

function onDragOver(event: DragEvent): void {
  if (props.readonly || !event.dataTransfer) return
  const point = getCanvasPoint(event)
  activeStageId.value = point ? stageColumns.value[point.stageIndex]?.stageId ?? null : null
  activeLaneId.value = point ? categoryRows.value[point.laneIndex]?.laneId ?? null : null
  if (!point) return

  event.preventDefault()
  event.dataTransfer.dropEffect = 'copy'
}

function onDrop(event: DragEvent): void {
  if (props.readonly || !event.dataTransfer) return
  const nodeType = event.dataTransfer.getData('application/x-flow-node-type') || event.dataTransfer.getData('text/plain')
  const point = getCanvasPoint(event)
  activeStageId.value = null
  activeLaneId.value = null
  if (!nodeType || !point) return

  event.preventDefault()
  emit('add-node', {
    nodeType,
    stageId: stageColumns.value[point.stageIndex]?.stageId,
    laneId: categoryRows.value[point.laneIndex]?.laneId,
    x: point.stageIndex * stageWidth + nodeColumnOffset,
    y: snapYToLane(point.y),
  })
}

function onDragLeave(event: DragEvent): void {
  const current = event.currentTarget as HTMLElement
  const next = event.relatedTarget as globalThis.Node | null
  if (!next || !current.contains(next)) {
    activeStageId.value = null
    activeLaneId.value = null
  }
}

function onNodeDragStop(event: NodeDragEvent): void {
  if (props.readonly) return
  const node = event.node
  const stageIndex = clampIndex(Math.floor(node.position.x / stageWidth), stageColumns.value.length)
  const laneIndex = resolveLaneIndex(node.position.y)
  emit('node-moved', {
    nodeId: node.id,
    x: stageIndex * stageWidth + nodeColumnOffset,
    y: snapYToLane(node.position.y),
    stageId: stageColumns.value[stageIndex]?.stageId,
    laneId: categoryRows.value[laneIndex]?.laneId,
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
      <p>設備列に図形パレットから図形をドラッグすると、ノードを配置できます。</p>
    </div>

    <div class="flow-table" :style="{ width: `${canvasWidth}px`, minHeight: `${headerHeight + bodyHeight}px` }">
      <div class="category-header" :style="{ width: `${categoryWidth}px`, height: `${headerHeight}px` }">
        工程分類
      </div>
      <div class="equipment-header" :style="{ left: `${categoryWidth}px`, height: `${headerHeight}px` }">
        <div v-for="stage in stageColumns" :key="stage.stageId" class="equipment-cell" :style="{ width: `${stageWidth}px` }">
          <div class="equipment-icon">{{ stage.name.slice(0, 1) }}</div>
          <strong>{{ stage.name }}</strong>
          <span>設備</span>
        </div>
      </div>

      <div class="category-column" :style="{ top: `${headerHeight}px`, width: `${categoryWidth}px`, minHeight: `${bodyHeight}px` }">
        <div
          v-for="lane in categoryRows"
          :key="lane.laneId"
          class="category-cell"
          :class="{ active: activeLaneId === lane.laneId }"
          :style="{ height: `${categoryHeight}px` }"
        >
          {{ lane.name }}
        </div>
      </div>

      <div class="lane-layer" :style="{ top: `${headerHeight}px`, left: `${categoryWidth}px`, width: `${canvasWidth - categoryWidth}px`, minHeight: `${bodyHeight}px` }">
        <div
          v-for="stage in stageColumns"
          :key="stage.stageId"
          class="equipment-lane"
          :class="{ active: activeStageId === stage.stageId }"
          :style="{ width: `${stageWidth}px` }"
        >
          <div
            v-for="lane in categoryRows"
            :key="`${stage.stageId}-${lane.laneId}`"
            class="category-band"
            :class="{ active: activeLaneId === lane.laneId && activeStageId === stage.stageId }"
            :style="{ height: `${categoryHeight}px` }"
          />
        </div>
      </div>

      <VueFlow class="vue-flow" :nodes="nodes" :edges="edges" :fit-view-on-init="true" @node-drag-stop="onNodeDragStop" @node-click="onNodeClick" @edge-click="onEdgeClick" @pane-click="onPaneClick">
        <template #node-flowShape="{ data }">
          <FlowShapeNode :node="data" />
        </template>
        <Background />
        <Controls />
      </VueFlow>
    </div>
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

.flow-table {
  position: relative;
  background: #ffffff;
}

.canvas-guidance {
  position: absolute;
  top: 128px;
  left: 50%;
  z-index: 8;
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
  color: #0f172a;
  font-size: 1rem;
}

.canvas-guidance p {
  margin: 6px 0 0;
}

.category-header,
.equipment-header {
  position: sticky;
  top: 0;
  z-index: 5;
  background: #f1f5f9;
  border-bottom: 1px solid #cbd5e1;
}

.category-header {
  left: 0;
  display: grid;
  place-items: center;
  color: #334155;
  border-right: 1px solid #cbd5e1;
  font-size: 0.9rem;
  font-weight: 800;
}

.equipment-header {
  display: flex;
}

.equipment-cell {
  display: grid;
  grid-template-rows: 26px auto auto;
  place-items: center;
  gap: 3px;
  padding: 8px 12px;
  color: #1e293b;
  background: #eef2ff;
  border-right: 1px solid #cbd5e1;
}

.equipment-icon {
  display: grid;
  width: 26px;
  height: 26px;
  place-items: center;
  color: #ffffff;
  background: #2563eb;
  border-radius: 6px;
  font-size: 0.78rem;
  font-weight: 800;
}

.equipment-cell strong {
  max-width: 100%;
  overflow: hidden;
  font-size: 0.92rem;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.equipment-cell span {
  color: #64748b;
  font-size: 0.72rem;
  font-weight: 700;
}

.category-column {
  position: absolute;
  left: 0;
  z-index: 3;
  background: #f8fafc;
  border-right: 1px solid #cbd5e1;
}

.category-cell {
  display: flex;
  align-items: center;
  padding: 0 14px;
  color: #334155;
  border-bottom: 1px solid #dbe3ef;
  font-size: 0.86rem;
  font-weight: 800;
  line-height: 1.35;
}

.category-cell.active {
  background: #dbeafe;
  color: #1d4ed8;
}

.lane-layer {
  position: absolute;
  z-index: 1;
  display: flex;
  pointer-events: none;
}

.equipment-lane {
  min-height: 100%;
  border-right: 1px solid #dbe3ef;
}

.equipment-lane:nth-child(even) {
  background: #f8fafc;
}

.equipment-lane.active {
  box-shadow: inset 0 0 0 2px rgb(37 99 235 / 30%);
}

.category-band {
  border-bottom: 1px solid #dbe3ef;
  background:
    linear-gradient(to right, rgb(148 163 184 / 14%) 1px, transparent 1px) 0 0 / 40px 100%,
    rgb(255 255 255 / 64%);
}

.equipment-lane:nth-child(even) .category-band {
  background:
    linear-gradient(to right, rgb(148 163 184 / 14%) 1px, transparent 1px) 0 0 / 40px 100%,
    rgb(248 250 252 / 80%);
}

.category-band.active {
  background: rgb(96 165 250 / 18%);
}

.vue-flow {
  position: absolute;
  top: 92px;
  left: 156px;
  z-index: 2;
  width: calc(100% - 156px);
  min-width: calc(100% - 156px);
  height: calc(100% - 92px);
  min-height: 1200px;
  background: transparent;
}

:deep(.flow-node-shell) {
  border: 0;
  background: transparent;
  box-shadow: none;
}

:deep(.selected-flow-node .flow-shape-node) {
  outline: 3px solid #2563eb;
  outline-offset: 3px;
}

:deep(.selected-flow-link path) {
  stroke: #dc2626;
  stroke-width: 3;
}

:deep(.vue-flow__edge-path) {
  stroke: #475569;
  stroke-width: 2;
}
</style>

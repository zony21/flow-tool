<script setup lang="ts">
import { computed, ref } from 'vue'
import { VueFlow, type Edge, type EdgeMouseEvent, type Node, type NodeDragEvent, type NodeMouseEvent } from '@vue-flow/core'
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
const rowHeight = 132
const nodeX = 42
const nodeY = 28
const ySnap = 20
const minBodyHeight = 1200
const fixedViewport = { x: 0, y: 0, zoom: 1 }

const stages = computed(() => props.flow.stages.slice().sort((a, b) => a.sortOrder - b.sortOrder))
const lanes = computed(() => props.flow.lanes.slice().sort((a, b) => a.sortOrder - b.sortOrder))
const hasEquipment = computed(() => stages.value.length > 0)
const activeStageId = ref<string | null>(null)
const activeLaneId = ref<string | null>(null)

const equipmentWidth = computed(() => Math.max(stages.value.length * stageWidth, stageWidth))
const maxNodeY = computed(() => Math.max(0, ...props.flow.nodes.map((node) => Number.isFinite(node.y) ? node.y : 0)))
const bodyHeight = computed(() => Math.max(lanes.value.length * rowHeight, maxNodeY.value + 240, minBodyHeight))
const tableWidth = computed(() => categoryWidth + equipmentWidth.value)

const nodes = computed<Node[]>(() =>
  props.flow.nodes.map((flowNode) => {
    const stageIndex = Math.max(0, stages.value.findIndex((stage) => stage.stageId === flowNode.stageId))
    const laneIndex = Math.max(0, lanes.value.findIndex((lane) => lane.laneId === flowNode.laneId))
    const fallbackY = laneIndex * rowHeight + nodeY

    return {
      id: flowNode.nodeId,
      type: 'flowShape',
      position: {
        x: stageIndex * stageWidth + nodeX,
        y: Number.isFinite(flowNode.y) ? flowNode.y : fallbackY,
      },
      data: flowNode,
      draggable: !props.readonly,
      class: flowNode.nodeId === props.selectedNodeId ? 'selected-flow-node' : undefined,
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

function clamp(index: number, max: number): number {
  if (max <= 0) return 0
  return Math.max(0, Math.min(index, max - 1))
}

function snapY(value: number): number {
  return Math.max(nodeY, Math.round(value / ySnap) * ySnap)
}

function stageIndexFromNodeX(x: number): number {
  return clamp(Math.round((x - nodeX) / stageWidth), stages.value.length)
}

function nodeXFromStageIndex(stageIndex: number): number {
  return stageIndex * stageWidth + nodeX
}

function laneIndexFromY(y: number): number {
  return lanes.value.length === 0 ? 0 : clamp(Math.floor(y / rowHeight), lanes.value.length)
}

function getPoint(event: DragEvent): { stageIndex: number; laneIndex: number; y: number } | null {
  const rect = (event.currentTarget as HTMLElement).getBoundingClientRect()
  const x = event.clientX - rect.left - categoryWidth
  const y = event.clientY - rect.top - headerHeight
  if (x < 0 || y < 0 || x >= equipmentWidth.value || stages.value.length === 0) return null
  return {
    stageIndex: clamp(Math.floor(x / stageWidth), stages.value.length),
    laneIndex: laneIndexFromY(y),
    y: snapY(y),
  }
}

function onDragOver(event: DragEvent): void {
  if (props.readonly || !event.dataTransfer) return
  const point = getPoint(event)
  activeStageId.value = point ? stages.value[point.stageIndex]?.stageId ?? null : null
  activeLaneId.value = point ? lanes.value[point.laneIndex]?.laneId ?? null : null
  if (!point) return
  event.preventDefault()
  event.dataTransfer.dropEffect = 'copy'
}

function onDrop(event: DragEvent): void {
  if (props.readonly || !event.dataTransfer) return
  const nodeType = event.dataTransfer.getData('application/x-flow-node-type') || event.dataTransfer.getData('text/plain')
  const point = getPoint(event)
  activeStageId.value = null
  activeLaneId.value = null
  if (!nodeType || !point) return
  event.preventDefault()
  emit('add-node', {
    nodeType,
    stageId: stages.value[point.stageIndex]?.stageId,
    laneId: lanes.value[point.laneIndex]?.laneId,
    x: nodeXFromStageIndex(point.stageIndex),
    y: point.y,
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

function onNodeDrag(event: NodeDragEvent): void {
  if (props.readonly) return
  const stageIndex = stageIndexFromNodeX(event.node.position.x)
  event.node.position.x = nodeXFromStageIndex(stageIndex)
}

function onNodeDragStop(event: NodeDragEvent): void {
  if (props.readonly) return
  const stageIndex = stageIndexFromNodeX(event.node.position.x)
  const y = snapY(event.node.position.y)
  const laneIndex = laneIndexFromY(y)
  emit('node-moved', {
    nodeId: event.node.id,
    x: nodeXFromStageIndex(stageIndex),
    y,
    stageId: stages.value[stageIndex]?.stageId,
    laneId: lanes.value[laneIndex]?.laneId,
  })
}

function onNodeClick(event: NodeMouseEvent): void {
  emit('node-selected', { nodeId: event.node.id })
}

function onEdgeClick(event: EdgeMouseEvent): void {
  emit('link-selected', { linkId: event.edge.id })
}
</script>

<template>
  <div class="flow-canvas" @dragover="onDragOver" @drop="onDrop" @dragleave="onDragLeave">
    <div v-if="!hasEquipment" class="canvas-guidance">
      <h2>設備が未設定です</h2>
      <p>設備/分類設定から設備を追加してください。</p>
    </div>

    <div class="flow-table" :style="{ width: `max(100%, ${tableWidth}px)`, minWidth: `${tableWidth}px`, minHeight: `${headerHeight + bodyHeight}px` }">
      <div class="category-header" :style="{ width: `${categoryWidth}px`, height: `${headerHeight}px` }">工程分類</div>
      <div class="equipment-header" :style="{ left: `${categoryWidth}px`, width: `${equipmentWidth}px`, height: `${headerHeight}px` }">
        <div v-for="stage in stages" :key="stage.stageId" class="equipment-cell" :style="{ width: `${stageWidth}px` }">
          <div class="equipment-icon">{{ stage.name.slice(0, 1) }}</div>
          <strong>{{ stage.name }}</strong>
          <span>設備</span>
        </div>
      </div>

      <div class="category-column" :style="{ top: `${headerHeight}px`, width: `${categoryWidth}px`, minHeight: `${bodyHeight}px` }">
        <div v-for="lane in lanes" :key="lane.laneId" class="category-cell" :class="{ active: activeLaneId === lane.laneId }" :style="{ height: `${rowHeight}px` }">
          {{ lane.name }}
        </div>
      </div>

      <div class="grid-layer" :style="{ top: `${headerHeight}px`, left: `${categoryWidth}px`, width: `${equipmentWidth}px`, minHeight: `${bodyHeight}px` }">
        <div v-for="stage in stages" :key="stage.stageId" class="equipment-lane" :class="{ active: activeStageId === stage.stageId }" :style="{ width: `${stageWidth}px` }">
          <div v-for="lane in lanes" :key="`${stage.stageId}-${lane.laneId}`" class="category-band" :class="{ active: activeLaneId === lane.laneId && activeStageId === stage.stageId }" :style="{ height: `${rowHeight}px` }" />
        </div>
      </div>

      <VueFlow
        class="vue-flow"
        :style="{ width: `${equipmentWidth}px`, height: `${bodyHeight}px` }"
        :nodes="nodes"
        :edges="edges"
        :fit-view-on-init="false"
        :default-viewport="fixedViewport"
        :viewport="fixedViewport"
        :min-zoom="1"
        :max-zoom="1"
        :zoom-on-scroll="false"
        :zoom-on-pinch="false"
        :zoom-on-double-click="false"
        :pan-on-drag="false"
        :pan-on-scroll="false"
        :prevent-scrolling="false"
        @node-drag="onNodeDrag"
        @node-drag-stop="onNodeDragStop"
        @node-click="onNodeClick"
        @edge-click="onEdgeClick"
        @pane-click="emit('canvas-cleared')"
      >
        <template #node-flowShape="{ data }">
          <FlowShapeNode :node="data" />
        </template>
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
  background: #fff;
}

.category-header,
.equipment-header,
.category-column,
.grid-layer,
.vue-flow {
  position: absolute;
}

.category-header,
.equipment-header {
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
  box-sizing: border-box;
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
  color: #fff;
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
  left: 0;
  z-index: 3;
  background: #f8fafc;
  border-right: 1px solid #cbd5e1;
}

.category-cell {
  display: flex;
  align-items: center;
  box-sizing: border-box;
  padding: 0 14px;
  color: #334155;
  border-bottom: 1px solid #dbe3ef;
  font-size: 0.86rem;
  font-weight: 800;
}

.category-cell.active {
  background: #dbeafe;
  color: #1d4ed8;
}

.grid-layer {
  z-index: 1;
  display: flex;
  pointer-events: none;
}

.equipment-lane {
  box-sizing: border-box;
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
  box-sizing: border-box;
  border-bottom: 1px solid #dbe3ef;
  background: rgb(255 255 255 / 64%);
}

.equipment-lane:nth-child(even) .category-band {
  background: rgb(248 250 252 / 80%);
}

.category-band.active {
  background: rgb(96 165 250 / 18%);
}

.vue-flow {
  top: 92px;
  left: 156px;
  z-index: 2;
  background: transparent;
  overflow: hidden;
}

.vue-flow :deep(.vue-flow__pane) {
  background: transparent;
}

.vue-flow :deep(.vue-flow__viewport),
.vue-flow :deep(.vue-flow__transformationpane) {
  transform: translate(0px, 0px) scale(1) !important;
}

:deep(.flow-node-shell) {
  border: 0;
  background: transparent;
  box-shadow: none;
}

:deep(.selected-flow-node .flow-shape-node) {
  outline: 3px solid #2563eb;
}

:deep(.selected-flow-link path) {
  stroke: #dc2626;
  stroke-width: 3;
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
</style>

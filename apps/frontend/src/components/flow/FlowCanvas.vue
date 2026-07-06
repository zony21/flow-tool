<script setup lang="ts">
import { computed } from 'vue'
import { VueFlow, type Edge, type Node } from '@vue-flow/core'
import { Background } from '@vue-flow/background'
import { Controls } from '@vue-flow/controls'
import type { FlowDetail } from '../../types/flow'
import '@vue-flow/core/dist/style.css'
import '@vue-flow/core/dist/theme-default.css'

const props = defineProps<{
  flow: FlowDetail
}>()

const laneHeight = 140
const stageWidth = 240
const headerHeight = 48
const laneHeaderWidth = 160

const stageColumns = computed(() => props.flow.stages.slice().sort((a, b) => a.sortOrder - b.sortOrder))
const laneRows = computed(() => props.flow.lanes.slice().sort((a, b) => a.sortOrder - b.sortOrder))

const nodes = computed<Node[]>(() =>
  props.flow.nodes.map((flowNode) => {
    const laneIndex = Math.max(0, laneRows.value.findIndex((lane) => lane.laneId === flowNode.laneId))
    const stageIndex = Math.max(0, stageColumns.value.findIndex((stage) => stage.stageId === flowNode.stageId))

    const fallbackX = laneHeaderWidth + stageIndex * stageWidth + 40
    const fallbackY = headerHeight + laneIndex * laneHeight + 40

    return {
      id: flowNode.nodeId,
      type: 'default',
      label: `${flowNode.nodeType}: ${flowNode.name}`,
      position: {
        x: flowNode.x || fallbackX,
        y: flowNode.y || fallbackY,
      },
      data: flowNode,
      draggable: false,
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
</script>

<template>
  <div class="flow-canvas">
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

    <VueFlow class="vue-flow" :nodes="nodes" :edges="edges" :fit-view-on-init="true">
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

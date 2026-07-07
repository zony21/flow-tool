<script setup lang="ts">
import { computed } from 'vue'
import { Handle, Position } from '@vue-flow/core'
import type { FlowNode } from '../../types/flow'

const props = defineProps<{
  node: FlowNode
}>()

const polygonPoints = computed(() => {
  switch (props.node.nodeType) {
    case 'decision':
      return '70,4 136,50 70,96 4,50'
    case 'preparation':
      return '34,4 106,4 136,50 106,96 34,96 4,50'
    case 'document':
      return '8,8 132,8 132,74 114,90 96,82 78,90 60,82 42,90 24,82 8,90'
    default:
      return '8,8 132,8 132,92 8,92'
  }
})
</script>

<template>
  <div class="flow-shape-node" :class="`shape-${node.nodeType}`">
    <svg class="shape-svg" viewBox="0 0 140 100" aria-hidden="true">
      <rect v-if="node.nodeType === 'start' || node.nodeType === 'end'" x="6" y="16" width="128" height="68" rx="34" ry="34" />
      <rect v-else-if="node.nodeType === 'wait'" x="8" y="14" width="124" height="72" rx="6" ry="6" class="dashed" />
      <polygon v-else :points="polygonPoints" />
    </svg>

    <Handle type="target" :position="Position.Top" class="node-handle handle-top" />
    <Handle type="source" :position="Position.Bottom" class="node-handle handle-bottom" />
    <Handle type="target" :position="Position.Left" class="node-handle handle-left" />
    <Handle type="source" :position="Position.Right" class="node-handle handle-right" />

    <div class="shape-content">
      {{ node.name }}
    </div>
  </div>
</template>

<style scoped>
.flow-shape-node {
  position: relative;
  display: grid;
  width: 140px;
  height: 100px;
  place-items: center;
  color: #0f172a;
  background: transparent;
  border: 0;
  box-shadow: none;
  font-size: 13px;
  font-weight: 700;
  line-height: 1.35;
  text-align: center;
}

.shape-content {
  position: relative;
  z-index: 1;
  max-width: 96px;
  overflow-wrap: anywhere;
  pointer-events: none;
}

.shape-decision .shape-content {
  max-width: 76px;
}

.shape-preparation .shape-content {
  max-width: 94px;
}

.shape-svg {
  position: absolute;
  inset: 0;
  width: 100%;
  height: 100%;
  overflow: visible;
  pointer-events: none;
}

.shape-svg rect,
.shape-svg polygon {
  fill: #ffffff;
  stroke: #475569;
  stroke-width: 2.5;
  vector-effect: non-scaling-stroke;
  filter: drop-shadow(0 8px 8px rgb(15 23 42 / 12%));
}

.shape-svg .dashed {
  stroke-dasharray: 8 6;
}

.node-handle {
  z-index: 2;
  width: 9px;
  height: 9px;
  background: #2563eb;
  border: 2px solid #ffffff;
  opacity: 0;
  transition: opacity 0.12s ease;
}

.flow-shape-node:hover .node-handle,
.flow-shape-node:focus-within .node-handle {
  opacity: 1;
}

.handle-top,
.handle-bottom {
  left: 50%;
}

.handle-left,
.handle-right {
  top: 50%;
}
</style>

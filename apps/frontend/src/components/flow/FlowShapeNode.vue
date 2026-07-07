<script setup lang="ts">
import { Handle, Position } from '@vue-flow/core'
import type { FlowNode } from '../../types/flow'

defineProps<{
  node: FlowNode
}>()
</script>

<template>
  <div class="flow-shape-node" :class="`shape-${node.nodeType}`">
    <svg v-if="node.nodeType === 'decision'" class="shape-svg" viewBox="0 0 140 100" aria-hidden="true">
      <polygon points="70,4 136,50 70,96 4,50" />
    </svg>
    <svg v-else-if="node.nodeType === 'preparation'" class="shape-svg" viewBox="0 0 140 100" aria-hidden="true">
      <polygon points="34,4 106,4 136,50 106,96 34,96 4,50" />
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
  place-items: center;
  min-width: 132px;
  min-height: 52px;
  padding: 10px 14px;
  color: #0f172a;
  background: #ffffff;
  border: 2px solid #475569;
  box-shadow: 0 8px 18px rgb(15 23 42 / 12%);
  font-size: 13px;
  font-weight: 700;
  line-height: 1.35;
  text-align: center;
}

.shape-content {
  position: relative;
  z-index: 1;
  max-width: 132px;
  overflow-wrap: anywhere;
  pointer-events: none;
}

.shape-svg {
  position: absolute;
  inset: 0;
  width: 100%;
  height: 100%;
  overflow: visible;
  pointer-events: none;
}

.shape-svg polygon {
  fill: #ffffff;
  stroke: #475569;
  stroke-width: 2.5;
  vector-effect: non-scaling-stroke;
  filter: drop-shadow(0 8px 8px rgb(15 23 42 / 12%));
}

.shape-start,
.shape-end {
  border-radius: 999px;
}

.shape-process {
  border-radius: 4px;
}

.shape-decision,
.shape-preparation {
  width: 132px;
  height: 92px;
  min-width: 132px;
  min-height: 92px;
  padding: 0;
  border: 0;
  background: transparent;
  box-shadow: none;
}

.shape-decision .shape-content {
  max-width: 78px;
}

.shape-preparation .shape-content {
  max-width: 94px;
}

.shape-document {
  border-radius: 4px 4px 18px 18px;
}

.shape-wait {
  border-style: dashed;
  border-radius: 4px;
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

<script setup lang="ts">
import { Handle, Position } from '@vue-flow/core'
import type { FlowNode } from '../../types/flow'

defineProps<{
  node: FlowNode
}>()
</script>

<template>
  <div class="flow-shape-node" :class="`shape-${node.nodeType}`">
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

.shape-start,
.shape-end {
  border-radius: 999px;
}

.shape-process {
  border-radius: 4px;
}

.shape-decision,
.shape-preparation {
  width: 120px;
  height: 92px;
  min-width: 120px;
  min-height: 92px;
  padding: 0;
  border: 0;
  background: transparent;
  box-shadow: none;
}

.shape-decision::before,
.shape-preparation::before {
  position: absolute;
  inset: 8px;
  content: '';
  background: #ffffff;
  border: 2px solid #475569;
  box-shadow: 0 8px 18px rgb(15 23 42 / 12%);
}

.shape-decision::before {
  transform: rotate(45deg) scale(0.78);
}

.shape-decision .shape-content {
  max-width: 70px;
}

.shape-preparation::before {
  clip-path: polygon(24% 0, 76% 0, 100% 50%, 76% 100%, 24% 100%, 0 50%);
}

.shape-preparation .shape-content {
  max-width: 88px;
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

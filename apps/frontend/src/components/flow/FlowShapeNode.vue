<script setup lang="ts">
import type { FlowNode } from '../../types/flow'

defineProps<{
  node: FlowNode
}>()
</script>

<template>
  <div class="flow-shape-node" :class="`shape-${node.nodeType}`">
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
  max-width: 132px;
  overflow-wrap: anywhere;
}

.shape-start,
.shape-end {
  border-radius: 999px;
}

.shape-process {
  border-radius: 6px;
}

.shape-decision {
  width: 112px;
  height: 112px;
  min-width: 112px;
  min-height: 112px;
  padding: 0;
  border-radius: 4px;
  clip-path: polygon(50% 0, 100% 50%, 50% 100%, 0 50%);
}

.shape-decision .shape-content {
  max-width: 76px;
}

.shape-document {
  border-radius: 6px 6px 22px 6px;
}

.shape-wait {
  border-style: dashed;
  border-radius: 6px;
}
</style>

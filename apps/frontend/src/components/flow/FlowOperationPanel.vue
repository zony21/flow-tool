<script setup lang="ts">
import { computed } from 'vue'
import { nodeSamples } from '../../constants/nodeSamples'
import type { FlowDetail } from '../../types/flow'

const props = defineProps<{
  flow: FlowDetail
  readonly?: boolean
}>()

const hasEquipment = computed(() => props.flow.stages.length > 0)

function onPaletteDragStart(event: DragEvent, nodeType: string): void {
  if (props.readonly || !event.dataTransfer) return

  event.dataTransfer.effectAllowed = 'copy'
  event.dataTransfer.setData('application/x-flow-node-type', nodeType)
  event.dataTransfer.setData('text/plain', nodeType)
}
</script>

<template>
  <aside class="operation-panel">
    <h2>図形パレット</h2>
    <div class="sample-grid">
      <button
        v-for="sample in nodeSamples"
        :key="sample.type"
        type="button"
        class="sample-button"
        :class="`sample-${sample.type}`"
        :title="sample.description"
        :disabled="readonly || !hasEquipment"
        draggable="true"
        @dragstart="onPaletteDragStart($event, sample.type)"
      >
        <span>{{ sample.label }}</span>
      </button>
    </div>
    <p v-if="!hasEquipment" class="empty-message">設備/分類設定から設備を追加してください。</p>
  </aside>
</template>

<style scoped>
.operation-panel {
  width: 100%;
  box-sizing: border-box;
  padding: 12px;
  background: #ffffff;
  border: 1px solid #dbe3ef;
  border-radius: 8px;
}

h2 {
  margin: 0 0 10px;
  color: #0f172a;
  font-size: 0.95rem;
}

.sample-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 8px;
}

.sample-button {
  min-height: 42px;
  padding: 6px 8px;
  color: #0f172a;
  background: #fff;
  border: 2px solid #cbd5e1;
  border-radius: 8px;
  font-size: 12px;
  font-weight: 700;
  cursor: grab;
}

.sample-button:active {
  cursor: grabbing;
}

.sample-button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.sample-start,
.sample-end {
  border-radius: 999px;
}

.sample-decision {
  border-radius: 4px;
  clip-path: polygon(50% 0, 100% 50%, 50% 100%, 0 50%);
}

.sample-document {
  border-bottom-right-radius: 22px;
}

.sample-wait {
  border-style: dashed;
}

.empty-message {
  margin: 10px 0 0;
  padding: 8px 10px;
  color: #64748b;
  background: #f8fafc;
  border: 1px dashed #cbd5e1;
  border-radius: 8px;
  font-size: 0.82rem;
}
</style>

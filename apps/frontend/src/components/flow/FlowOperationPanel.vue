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
    <p class="palette-help">図形を設備列へドラッグして配置します。</p>
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
    <p class="connect-help">配置後、図形の端の接続点から別図形へドラッグすると矢印で接続できます。</p>
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
  margin: 0 0 6px;
  color: #0f172a;
  font-size: 0.95rem;
}

.palette-help,
.connect-help {
  margin: 0 0 10px;
  color: #64748b;
  font-size: 0.78rem;
  line-height: 1.5;
}

.connect-help {
  margin: 10px 0 0;
}

.sample-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 10px;
}

.sample-button {
  position: relative;
  display: grid;
  min-height: 48px;
  place-items: center;
  padding: 6px 8px;
  color: #0f172a;
  background: #fff;
  border: 2px solid #475569;
  border-radius: 4px;
  box-shadow: 0 4px 10px rgb(15 23 42 / 8%);
  font-size: 12px;
  font-weight: 800;
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

.sample-process {
  border-radius: 4px;
}

.sample-decision {
  width: 58px;
  height: 58px;
  min-height: 58px;
  justify-self: center;
  padding: 0;
  clip-path: polygon(50% 0, 100% 50%, 50% 100%, 0 50%);
}

.sample-decision span {
  max-width: 36px;
  font-size: 11px;
  line-height: 1.2;
}

.sample-document {
  border-radius: 4px 4px 18px 18px;
}

.sample-wait {
  border-style: dashed;
  border-radius: 4px;
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

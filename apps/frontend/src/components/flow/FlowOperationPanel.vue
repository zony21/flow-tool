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

function polygonPoints(type: string): string {
  switch (type) {
    case 'start':
    case 'end':
      return ''
    case 'decision':
      return '48,4 92,32 48,60 4,32'
    case 'preparation':
      return '24,4 72,4 92,32 72,60 24,60 4,32'
    case 'document':
      return '6,6 90,6 90,48 78,58 66,52 54,58 42,52 30,58 18,52 6,58'
    default:
      return '6,6 90,6 90,58 6,58'
  }
}
</script>

<template>
  <aside class="operation-panel">
    <h2>図形パレット</h2>
    <p class="palette-help">図形を列へドラッグして配置します。</p>
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
        <svg class="sample-svg" viewBox="0 0 96 64" aria-hidden="true">
          <rect v-if="sample.type === 'start' || sample.type === 'end'" x="4" y="6" width="88" height="52" rx="26" ry="26" />
          <rect v-else-if="sample.type === 'wait'" x="6" y="6" width="84" height="52" rx="4" ry="4" class="dashed" />
          <polygon v-else :points="polygonPoints(sample.type)" />
        </svg>
        <span>{{ sample.label }}</span>
      </button>
    </div>
    <p class="connect-help">配置後、図形の端の接続点から別図形へドラッグすると矢印で接続できます。</p>
    <p v-if="!hasEquipment" class="empty-message">設備/分類設定から列を追加してください。</p>
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
  min-height: 54px;
  place-items: center;
  padding: 6px 8px;
  color: #0f172a;
  background: transparent;
  border: 0;
  box-shadow: none;
  font-size: 12px;
  font-weight: 800;
  cursor: grab;
}

.sample-button span {
  position: relative;
  z-index: 1;
}

.sample-button:active {
  cursor: grabbing;
}

.sample-button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.sample-svg {
  position: absolute;
  inset: 0;
  width: 100%;
  height: 100%;
  overflow: visible;
  pointer-events: none;
}

.sample-svg rect,
.sample-svg polygon {
  fill: #ffffff;
  stroke: #475569;
  stroke-width: 2.4;
  vector-effect: non-scaling-stroke;
  filter: drop-shadow(0 4px 5px rgb(15 23 42 / 10%));
}

.sample-svg .dashed {
  stroke-dasharray: 7 5;
}

.sample-decision span {
  max-width: 38px;
  font-size: 11px;
  line-height: 1.2;
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

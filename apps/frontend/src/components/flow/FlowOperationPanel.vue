<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import Button from 'primevue/button'
import { nodeSamples } from '../../constants/nodeSamples'
import type { FlowDetail } from '../../types/flow'

const props = defineProps<{
  flow: FlowDetail
  readonly?: boolean
}>()

const emit = defineEmits<{
  (event: 'add-link', payload: { sourceNodeId: string; targetNodeId: string }): void
}>()

const lanes = computed(() => props.flow.lanes.slice().sort((a, b) => a.sortOrder - b.sortOrder))
const stages = computed(() => props.flow.stages.slice().sort((a, b) => a.sortOrder - b.sortOrder))
const nodeOptions = computed(() =>
  props.flow.nodes.map((node) => ({
    nodeId: node.nodeId,
    label: node.name,
  })),
)

const selectedSourceNodeId = ref('')
const selectedTargetNodeId = ref('')

watch(
  nodeOptions,
  (options) => {
    if (!options.some((option) => option.nodeId === selectedSourceNodeId.value)) {
      selectedSourceNodeId.value = options[0]?.nodeId ?? ''
    }

    if (!options.some((option) => option.nodeId === selectedTargetNodeId.value)) {
      selectedTargetNodeId.value = options[1]?.nodeId ?? options[0]?.nodeId ?? ''
    }
  },
  { immediate: true },
)

function onPaletteDragStart(event: DragEvent, nodeType: string): void {
  if (props.readonly || !event.dataTransfer) return

  event.dataTransfer.effectAllowed = 'copy'
  event.dataTransfer.setData('application/x-flow-node-type', nodeType)
  event.dataTransfer.setData('text/plain', nodeType)
}

function addLink(): void {
  if (props.readonly) return
  if (!selectedSourceNodeId.value || !selectedTargetNodeId.value) return

  emit('add-link', {
    sourceNodeId: selectedSourceNodeId.value,
    targetNodeId: selectedTargetNodeId.value,
  })
}
</script>

<template>
  <aside class="operation-panel">
    <section>
      <h2>Node Palette</h2>
      <p class="help-text">図形をCanvasへドラッグして、設備・場所の列に配置します。</p>

      <div class="sample-grid">
        <button
          v-for="sample in nodeSamples"
          :key="sample.type"
          type="button"
          class="sample-button"
          :class="`sample-${sample.type}`"
          :title="sample.description"
          :disabled="readonly || stages.length === 0 || lanes.length === 0"
          draggable="true"
          @dragstart="onPaletteDragStart($event, sample.type)"
        >
          <span>{{ sample.label }}</span>
        </button>
      </div>
    </section>

    <section>
      <h2>設備・場所一覧</h2>
      <p class="help-text">列の上にドロップすると、その設備・場所に所属します。</p>
      <div v-if="stages.length === 0" class="empty-message">設備・場所がありません。</div>
      <ul v-else class="plain-list">
        <li v-for="stage in stages" :key="stage.stageId">{{ stage.name }}</li>
      </ul>
    </section>

    <section>
      <h2>担当・責務一覧</h2>
      <p class="help-text">行の位置で、担当・責務が決まります。</p>
      <div v-if="lanes.length === 0" class="empty-message">担当・責務がありません。</div>
      <ul v-else class="plain-list">
        <li v-for="lane in lanes" :key="lane.laneId">{{ lane.name }}</li>
      </ul>
    </section>

    <section>
      <h2>接続線</h2>
      <p class="help-text">処理順を表す線を追加します。</p>

      <label class="field">
        <span>接続元</span>
        <select v-model="selectedSourceNodeId" :disabled="readonly || flow.nodes.length < 2">
          <option v-for="option in nodeOptions" :key="`source-${option.nodeId}`" :value="option.nodeId">
            {{ option.label }}
          </option>
        </select>
      </label>

      <label class="field">
        <span>接続先</span>
        <select v-model="selectedTargetNodeId" :disabled="readonly || flow.nodes.length < 2">
          <option v-for="option in nodeOptions" :key="`target-${option.nodeId}`" :value="option.nodeId">
            {{ option.label }}
          </option>
        </select>
      </label>

      <Button label="接続線を追加" class="full-button" severity="secondary" :disabled="readonly || flow.nodes.length < 2 || !selectedSourceNodeId || !selectedTargetNodeId" @click="addLink" />
    </section>
  </aside>
</template>

<style scoped>
.operation-panel {
  width: 320px;
  padding: 16px;
  background: #fff;
  border: 1px solid #dbe3ef;
  border-radius: 8px;
}

.operation-panel section + section {
  margin-top: 20px;
  padding-top: 16px;
  border-top: 1px solid #e2e8f0;
}

h2 {
  margin: 0;
  font-size: 1rem;
}

.help-text {
  margin: 6px 0 12px;
  color: #64748b;
  font-size: 0.85rem;
}

.sample-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 8px;
}

.sample-button {
  min-height: 44px;
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
  transform: skew(-8deg);
}

.sample-decision span {
  display: inline-block;
  transform: skew(8deg);
}

.sample-document {
  border-bottom-right-radius: 22px;
}

.sample-wait {
  border-style: dashed;
}

.plain-list {
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: 0;
  margin: 0;
  list-style: none;
}

.plain-list li,
.empty-message {
  padding: 8px 10px;
  color: #334155;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  font-size: 0.9rem;
}

.empty-message {
  color: #64748b;
  border-style: dashed;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 6px;
  margin-top: 10px;
  color: #334155;
  font-size: 13px;
  font-weight: 700;
}

.field select {
  width: 100%;
  padding: 8px 10px;
  color: #0f172a;
  background: #fff;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  font: inherit;
  font-weight: 400;
}

.full-button {
  width: 100%;
  margin-top: 12px;
}
</style>

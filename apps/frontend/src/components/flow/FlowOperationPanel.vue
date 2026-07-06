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
  (event: 'add-node', payload: { nodeType: string; name: string; laneId?: string; stageId?: string }): void
  (event: 'add-link', payload: { sourceNodeId: string; targetNodeId: string }): void
}>()

const lanes = computed(() => props.flow.lanes.slice().sort((a, b) => a.sortOrder - b.sortOrder))
const stages = computed(() => props.flow.stages.slice().sort((a, b) => a.sortOrder - b.sortOrder))
const hasStructureGrid = computed(() => lanes.value.length > 0 && stages.value.length > 0)
const nodeOptions = computed(() =>
  props.flow.nodes.map((node) => ({
    nodeId: node.nodeId,
    label: node.name,
  })),
)

const selectedNodeType = ref(nodeSamples[1].type)
const selectedLaneId = ref('')
const selectedStageId = ref('')
const selectedSourceNodeId = ref('')
const selectedTargetNodeId = ref('')

watch(
  () => [lanes.value, stages.value] as const,
  ([currentLanes, currentStages]) => {
    if (!currentLanes.some((lane) => lane.laneId === selectedLaneId.value)) {
      selectedLaneId.value = currentLanes[0]?.laneId ?? ''
    }

    if (!currentStages.some((stage) => stage.stageId === selectedStageId.value)) {
      selectedStageId.value = currentStages[0]?.stageId ?? ''
    }
  },
  { immediate: true },
)

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

function addNode(): void {
  if (props.readonly || !hasStructureGrid.value) return

  const sample = nodeSamples.find((item) => item.type === selectedNodeType.value) ?? nodeSamples[1]
  emit('add-node', {
    nodeType: sample.type,
    name: sample.defaultName,
    laneId: selectedLaneId.value || undefined,
    stageId: selectedStageId.value || undefined,
  })
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
      <h2>図形追加</h2>
      <p class="help-text">追加する図形を選び、配置先を指定します。</p>

      <div class="sample-grid">
        <button
          v-for="sample in nodeSamples"
          :key="sample.type"
          type="button"
          class="sample-button"
          :class="[`sample-${sample.type}`, { selected: selectedNodeType === sample.type }]"
          :title="sample.description"
          :disabled="readonly || !hasStructureGrid"
          @click="selectedNodeType = sample.type"
        >
          <span>{{ sample.label }}</span>
        </button>
      </div>

      <label class="field">
        <span>設備・場所</span>
        <select v-model="selectedStageId" :disabled="readonly || !hasStructureGrid">
          <option v-for="stage in stages" :key="stage.stageId" :value="stage.stageId">
            {{ stage.name }} の下
          </option>
        </select>
      </label>

      <label class="field">
        <span>担当・責務</span>
        <select v-model="selectedLaneId" :disabled="readonly || !hasStructureGrid">
          <option v-for="lane in lanes" :key="lane.laneId" :value="lane.laneId">
            {{ lane.name }} に配置
          </option>
        </select>
      </label>

      <Button label="図形を追加" class="full-button" :disabled="readonly || !hasStructureGrid" @click="addNode" />
    </section>

    <section>
      <h2>接続線追加</h2>
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
  border-radius: 12px;
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
  margin-bottom: 12px;
}

.sample-button {
  min-height: 42px;
  padding: 6px 8px;
  color: #0f172a;
  background: #fff;
  border: 2px solid #cbd5e1;
  border-radius: 10px;
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
}

.sample-button.selected {
  border-color: #2563eb;
  box-shadow: 0 0 0 2px rgb(37 99 235 / 18%);
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

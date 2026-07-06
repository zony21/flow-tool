<script setup lang="ts">
import { computed } from 'vue'
import Button from 'primevue/button'
import type { FlowDetail, Lane, Stage } from '../../types/flow'

const props = defineProps<{
  flow: FlowDetail
  readonly?: boolean
}>()

const emit = defineEmits<{
  (event: 'add-lane'): void
  (event: 'update-lane', payload: Lane): void
  (event: 'delete-lane', payload: { laneId: string }): void
  (event: 'add-stage'): void
  (event: 'update-stage', payload: Stage): void
  (event: 'delete-stage', payload: { stageId: string }): void
}>()

const sortedLanes = computed(() => props.flow.lanes.slice().sort((a, b) => a.sortOrder - b.sortOrder))
const sortedStages = computed(() => props.flow.stages.slice().sort((a, b) => a.sortOrder - b.sortOrder))

function updateLaneName(lane: Lane, name: string): void {
  emit('update-lane', {
    ...lane,
    name,
  })
}

function updateStageName(stage: Stage, name: string): void {
  emit('update-stage', {
    ...stage,
    name,
  })
}
</script>

<template>
  <aside class="lane-stage-panel">
    <section>
      <div class="section-header">
        <h2>Lane</h2>
        <Button label="追加" size="small" :disabled="readonly" @click="emit('add-lane')" />
      </div>
      <p class="help-text">責務・担当範囲を表します。</p>
      <div v-if="sortedLanes.length === 0" class="empty-message">Laneがありません。</div>
      <div v-for="lane in sortedLanes" :key="lane.laneId" class="item-row">
        <input
          :value="lane.name"
          type="text"
          :disabled="readonly"
          @change="updateLaneName(lane, ($event.target as HTMLInputElement).value)"
        />
        <Button label="削除" size="small" severity="danger" :disabled="readonly" @click="emit('delete-lane', { laneId: lane.laneId })" />
      </div>
    </section>

    <section>
      <div class="section-header">
        <h2>Stage</h2>
        <Button label="追加" size="small" :disabled="readonly" @click="emit('add-stage')" />
      </div>
      <p class="help-text">工程・処理段階を表します。</p>
      <div v-if="sortedStages.length === 0" class="empty-message">Stageがありません。</div>
      <div v-for="stage in sortedStages" :key="stage.stageId" class="item-row">
        <input
          :value="stage.name"
          type="text"
          :disabled="readonly"
          @change="updateStageName(stage, ($event.target as HTMLInputElement).value)"
        />
        <Button label="削除" size="small" severity="danger" :disabled="readonly" @click="emit('delete-stage', { stageId: stage.stageId })" />
      </div>
    </section>
  </aside>
</template>

<style scoped>
.lane-stage-panel {
  width: 320px;
  padding: 16px;
  background: #fff;
  border: 1px solid #dbe3ef;
  border-radius: 12px;
}

.lane-stage-panel section + section {
  margin-top: 20px;
}

.section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}

.section-header h2 {
  margin: 0;
  font-size: 1rem;
}

.help-text {
  margin: 6px 0 10px;
  font-size: 0.85rem;
}

.item-row {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: 8px;
}

input {
  min-width: 0;
  flex: 1 1 auto;
  padding: 8px 10px;
  border: 1px solid var(--border);
  border-radius: 8px;
  font: inherit;
}

.empty-message {
  padding: 10px 12px;
  color: #64748b;
  background: #f8fafc;
  border: 1px dashed #cbd5e1;
  border-radius: 8px;
  font-size: 0.9rem;
}
</style>

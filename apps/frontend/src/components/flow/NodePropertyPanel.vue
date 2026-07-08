<script setup lang="ts">
import { computed } from 'vue'
import { nodeSamples } from '../../constants/nodeSamples'
import type { FlowDetail, FlowNode } from '../../types/flow'

const props = defineProps<{
  flow: FlowDetail
  nodeId: string | null
  readonly?: boolean
}>()

const emit = defineEmits<{
  (event: 'update-node', payload: FlowNode): void
  (event: 'delete-node', payload: { nodeId: string }): void
}>()

const selectedNode = computed(() => props.flow.nodes.find((node) => node.nodeId === props.nodeId) ?? null)
const categories = computed(() => props.flow.lanes.slice().sort((a, b) => a.sortOrder - b.sortOrder))
const equipment = computed(() => props.flow.stages.slice().sort((a, b) => a.sortOrder - b.sortOrder))

function updateNode(patch: Partial<FlowNode>): void {
  if (!selectedNode.value || props.readonly) return
  emit('update-node', {
    ...selectedNode.value,
    ...patch,
  })
}

function deleteSelectedNode(): void {
  if (!selectedNode.value || props.readonly) return
  emit('delete-node', { nodeId: selectedNode.value.nodeId })
}
</script>

<template>
  <aside v-if="selectedNode" class="node-property-panel">
    <header class="panel-header">
      <h2>図形詳細</h2>
      <p>選択中の図形を編集します。</p>
    </header>

    <div class="form-grid">
      <label class="field">
        <span>ノード名</span>
        <input :value="selectedNode.name" :disabled="readonly" @input="updateNode({ name: ($event.target as HTMLInputElement).value })" />
      </label>

      <label class="field">
        <span>図形</span>
        <select :value="selectedNode.nodeType" :disabled="readonly" @change="updateNode({ nodeType: ($event.target as HTMLSelectElement).value })">
          <option v-for="sample in nodeSamples" :key="sample.type" :value="sample.type">
            {{ sample.label }}
          </option>
        </select>
      </label>

      <label class="field">
        <span>設備</span>
        <select :value="selectedNode.stageId ?? ''" :disabled="readonly" @change="updateNode({ stageId: ($event.target as HTMLSelectElement).value || null })">
          <option value="">未設定</option>
          <option v-for="stage in equipment" :key="stage.stageId" :value="stage.stageId">
            {{ stage.name }}
          </option>
        </select>
      </label>

      <label class="field">
        <span>工程分類</span>
        <select :value="selectedNode.laneId ?? ''" :disabled="readonly" @change="updateNode({ laneId: ($event.target as HTMLSelectElement).value || null })">
          <option value="">未設定</option>
          <option v-for="lane in categories" :key="lane.laneId" :value="lane.laneId">
            {{ lane.name }}
          </option>
        </select>
      </label>

      <label class="field">
        <span>説明</span>
        <textarea
          rows="4"
          :value="selectedNode.description ?? ''"
          :disabled="readonly"
          @input="updateNode({ description: ($event.target as HTMLTextAreaElement).value || null })"
        />
      </label>

      <div class="read-only-grid">
        <div>
          <span>ID</span>
          <strong>{{ selectedNode.nodeId }}</strong>
        </div>
        <div>
          <span>表示位置</span>
          <strong>Xは設備から自動算出 / Y {{ Math.round(selectedNode.y) }}</strong>
        </div>
      </div>

      <button type="button" class="delete-button" :disabled="readonly" @click="deleteSelectedNode">
        図形を削除
      </button>
    </div>
  </aside>
</template>

<style scoped>
.node-property-panel {
  display: flex;
  flex-direction: column;
  gap: 16px;
  width: 320px;
  min-width: 320px;
  padding: 16px;
  background: #ffffff;
  border: 1px solid #dbe3ef;
  border-radius: 8px;
}

.panel-header h2 {
  margin: 0;
  font-size: 18px;
}

.panel-header p {
  margin: 4px 0 0;
  color: #64748b;
  font-size: 13px;
}

.form-grid {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 6px;
  color: #334155;
  font-size: 13px;
  font-weight: 700;
}

.field input,
.field select,
.field textarea {
  width: 100%;
  box-sizing: border-box;
  padding: 8px 10px;
  color: #0f172a;
  background: #fff;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  font: inherit;
  font-weight: 400;
}

.read-only-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 8px;
  margin-top: 4px;
}

.read-only-grid div {
  display: flex;
  flex-direction: column;
  gap: 4px;
  padding: 8px;
  background: #f8fafc;
  border-radius: 8px;
}

.read-only-grid span {
  color: #64748b;
  font-size: 12px;
}

.read-only-grid strong {
  overflow-wrap: anywhere;
  color: #0f172a;
  font-size: 12px;
}

.delete-button {
  width: 100%;
  margin-top: 4px;
  padding: 10px 12px;
  color: #991b1b;
  background: #fee2e2;
  border: 1px solid #fca5a5;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 700;
  cursor: pointer;
}

.delete-button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>

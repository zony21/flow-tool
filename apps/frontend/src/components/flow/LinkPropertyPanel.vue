<script setup lang="ts">
import { computed } from 'vue'
import type { FlowDetail, FlowLink } from '../../types/flow'

const props = defineProps<{
  flow: FlowDetail
  linkId: string | null
  readonly?: boolean
}>()

const emit = defineEmits<{
  (event: 'update-link', payload: FlowLink): void
  (event: 'delete-link', payload: { linkId: string }): void
}>()

const selectedLink = computed(() => props.flow.links.find((link) => link.linkId === props.linkId) ?? null)

function updateField<K extends keyof FlowLink>(key: K, value: FlowLink[K]): void {
  if (!selectedLink.value || props.readonly) return
  emit('update-link', {
    ...selectedLink.value,
    [key]: value,
  })
}

function deleteSelectedLink(): void {
  if (!selectedLink.value || props.readonly) return
  emit('delete-link', { linkId: selectedLink.value.linkId })
}
</script>

<template>
  <aside v-if="selectedLink" class="link-property-panel">
    <header class="panel-header">
      <h2>接続線詳細</h2>
      <p>処理順や条件を編集します。</p>
    </header>

    <div class="form-grid">
      <label class="field">
        <span>表示名</span>
        <input
          :value="selectedLink.label ?? ''"
          :disabled="readonly"
          @input="updateField('label', ($event.target as HTMLInputElement).value || null)"
        />
      </label>

      <label class="field">
        <span>条件</span>
        <textarea
          rows="4"
          :value="selectedLink.condition ?? ''"
          :disabled="readonly"
          @input="updateField('condition', ($event.target as HTMLTextAreaElement).value || null)"
        />
      </label>

      <div class="read-only-grid">
        <div>
          <span>ID</span>
          <strong>{{ selectedLink.linkId }}</strong>
        </div>
        <div>
          <span>接続元 → 接続先</span>
          <strong>{{ selectedLink.sourceNodeId }} → {{ selectedLink.targetNodeId }}</strong>
        </div>
      </div>

      <button type="button" class="delete-button" :disabled="readonly" @click="deleteSelectedLink">
        接続線を削除
      </button>
    </div>
  </aside>
</template>

<style scoped>
.link-property-panel {
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
  font-size: 13px;
  font-weight: 700;
  color: #334155;
}

.field input,
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

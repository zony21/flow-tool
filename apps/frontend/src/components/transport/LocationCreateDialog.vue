<script setup lang="ts">
import { reactive, ref, watch } from 'vue'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import { normalizeApiError } from '../../api/apiError'
import { createTransportLocation } from '../../api/transportApi'
import type { TransportLocation, TransportLocationType } from '../../types/transport'

const props = defineProps<{
  visible: boolean
  projectId: string
  readonly?: boolean
}>()

const emit = defineEmits<{
  (event: 'update:visible', visible: boolean): void
  (event: 'created', location: TransportLocation): void
}>()

const locationTypes: TransportLocationType[] = ['経由点', '荷役', '充電', '待機', 'その他']
const form = reactive({
  name: '',
  locationType: '' as TransportLocationType | '',
  description: '',
  sortOrder: null as number | null,
})
const fieldErrors = reactive({
  name: [] as string[],
  locationType: [] as string[],
})
const apiError = ref<string | null>(null)
const saving = ref(false)

watch(() => props.visible, (visible) => {
  if (visible) resetForm()
})

function resetForm(): void {
  form.name = ''
  form.locationType = ''
  form.description = ''
  form.sortOrder = null
  clearErrors()
}

function clearErrors(): void {
  fieldErrors.name = []
  fieldErrors.locationType = []
  apiError.value = null
}

function validate(): boolean {
  clearErrors()
  if (!form.name.trim()) fieldErrors.name = ['ロケーション名は必須です。']
  if (!form.locationType) fieldErrors.locationType = ['ロケーション種別は必須です。']
  return fieldErrors.name.length === 0 && fieldErrors.locationType.length === 0
}

function close(): void {
  if (saving.value) return
  emit('update:visible', false)
}

async function save(): Promise<void> {
  if (props.readonly || saving.value || !validate()) return

  saving.value = true
  try {
    const location = await createTransportLocation(props.projectId, {
      name: form.name.trim(),
      locationType: form.locationType,
      description: form.description.trim() || null,
      sortOrder: form.sortOrder,
    })
    emit('created', location)
  } catch (error) {
    const normalized = normalizeApiError(error)
    fieldErrors.name = normalized.details?.name ?? normalized.details?.Name ?? []
    fieldErrors.locationType = normalized.details?.locationType ?? normalized.details?.LocationType ?? []
    apiError.value = normalized.message
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <Dialog
    :visible="visible"
    modal
    header="ロケーション作成"
    :closable="!saving"
    :close-on-escape="!saving"
    :style="{ width: 'min(520px, 92vw)' }"
    @update:visible="emit('update:visible', $event)"
  >
    <form class="location-form" @submit.prevent="save">
      <label class="field">
        <span>ロケーション名 <em>必須</em></span>
        <input v-model="form.name" :disabled="readonly || saving" maxlength="200" />
        <small v-for="message in fieldErrors.name" :key="message" class="field-error">{{ message }}</small>
      </label>

      <label class="field">
        <span>ロケーション種別 <em>必須</em></span>
        <select v-model="form.locationType" :disabled="readonly || saving">
          <option value="">選択してください</option>
          <option v-for="type in locationTypes" :key="type" :value="type">{{ type }}</option>
        </select>
        <small v-for="message in fieldErrors.locationType" :key="message" class="field-error">{{ message }}</small>
      </label>

      <label class="field">
        <span>説明</span>
        <textarea v-model="form.description" rows="4" :disabled="readonly || saving" />
      </label>

      <label class="field">
        <span>表示順</span>
        <input v-model.number="form.sortOrder" type="number" min="0" step="1" :disabled="readonly || saving" />
      </label>

      <p v-if="apiError" class="api-error" role="alert">{{ apiError }}</p>

      <div class="dialog-actions">
        <Button type="button" label="キャンセル" severity="secondary" :disabled="saving" @click="close" />
        <Button type="submit" :label="saving ? '保存中...' : '保存'" :disabled="readonly || saving" />
      </div>
    </form>
  </Dialog>
</template>

<style scoped>
.location-form,
.field {
  display: flex;
  flex-direction: column;
}

.location-form { gap: 16px; }
.field { gap: 6px; color: #334155; font-size: 13px; font-weight: 700; }
.field em { color: #b91c1c; font-size: 11px; font-style: normal; }
.field input,
.field select,
.field textarea { box-sizing: border-box; width: 100%; padding: 9px 10px; color: #0f172a; background: #fff; border: 1px solid #cbd5e1; border-radius: 8px; font: inherit; font-weight: 400; }
.field-error,
.api-error { color: #b91c1c; font-size: 12px; }
.api-error { margin: 0; padding: 10px; background: #fef2f2; border: 1px solid #fecaca; border-radius: 8px; }
.dialog-actions { display: flex; justify-content: flex-end; gap: 8px; }
</style>

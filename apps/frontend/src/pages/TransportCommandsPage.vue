<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import InputNumber from 'primevue/inputnumber'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import Textarea from 'primevue/textarea'
import ToggleSwitch from 'primevue/toggleswitch'
import MainLayout from '../layouts/MainLayout.vue'
import {
  createTransportCommand,
  deleteTransportCommand,
  fetchTransportCommands,
  fetchTransportManufacturers,
  fetchTransportManufacturerVehicleTypes,
  updateTransportCommand,
} from '../api/transportApi'
import type { TransportCommand, TransportManufacturer, TransportManufacturerVehicleType } from '../types/transport'
import './transport-master-page.css'

const route = useRoute()
const router = useRouter()
const manufacturerId = computed(() => String(route.params.manufacturerId ?? ''))
const typeId = computed(() => String(route.params.typeId ?? ''))
const manufacturers = ref<TransportManufacturer[]>([])
const types = ref<TransportManufacturerVehicleType[]>([])
const items = ref<TransportCommand[]>([])
const loading = ref(false)
const saving = ref(false)
const dialogVisible = ref(false)
const editingId = ref<string | null>(null)
const pageError = ref<string | null>(null)
const formError = ref<string | null>(null)
const keyword = ref('')
const processTypeFilter = ref('ALL')
const statusFilter = ref<'ALL' | 'ACTIVE' | 'INACTIVE'>('ALL')
const form = reactive({ commandCode: '', commandName: '', processType: 'MOVE', description: '', sortOrder: null as number | null, isActive: true })
const processTypeOptions = [
  { value: 'ALL', label: 'すべて' },
  { value: 'MOVE', label: '移動' },
  { value: 'LOADING', label: '荷積み' },
  { value: 'UNLOADING', label: '荷下ろし' },
  { value: 'WAIT', label: '待機' },
  { value: 'STATUS', label: '状態取得' },
  { value: 'TASK_CONTROL', label: 'タスク制御' },
  { value: 'OTHER', label: 'その他' },
]
const commandProcessTypeOptions = processTypeOptions.filter((option) => option.value !== 'ALL')
const statusOptions = [
  { label: 'すべて', value: 'ALL' },
  { label: '有効', value: 'ACTIVE' },
  { label: '無効', value: 'INACTIVE' },
]
const manufacturer = computed(() => manufacturers.value.find((item) => item.manufacturerId === manufacturerId.value) ?? null)
const vehicleType = computed(() => types.value.find((item) => item.manufacturerVehicleTypeId === typeId.value) ?? null)
const filteredItems = computed(() => {
  const word = keyword.value.trim().toLocaleLowerCase('ja')
  return items.value.filter((item) => {
    const keywordMatched = !word || item.commandCode.toLocaleLowerCase('ja').includes(word)
      || item.commandName.toLocaleLowerCase('ja').includes(word)
      || (item.description ?? '').toLocaleLowerCase('ja').includes(word)
    const processMatched = processTypeFilter.value === 'ALL' || item.processType === processTypeFilter.value
    const statusMatched = statusFilter.value === 'ALL'
      || (statusFilter.value === 'ACTIVE' && item.isActive)
      || (statusFilter.value === 'INACTIVE' && !item.isActive)
    return keywordMatched && processMatched && statusMatched
  })
})
function message(error: unknown, fallback: string): string { return error instanceof Error ? error.message : fallback }
function processTypeLabel(value: string): string { return commandProcessTypeOptions.find((option) => option.value === value)?.label ?? value }
async function load(): Promise<void> {
  loading.value = true
  pageError.value = null
  try {
    const [manufacturerList, typeList, commandList] = await Promise.all([
      fetchTransportManufacturers(), fetchTransportManufacturerVehicleTypes(true), fetchTransportCommands(typeId.value, true),
    ])
    manufacturers.value = manufacturerList
    types.value = typeList
    items.value = commandList
  } catch (error) { pageError.value = message(error, 'コマンドの取得に失敗しました。') }
  finally { loading.value = false }
}
function open(item?: TransportCommand): void {
  editingId.value = item?.commandId ?? null
  Object.assign(form, {
    commandCode: item?.commandCode ?? '', commandName: item?.commandName ?? '', processType: item?.processType ?? 'MOVE',
    description: item?.description ?? '', sortOrder: item?.sortOrder ?? null, isActive: item?.isActive ?? true,
  })
  formError.value = null
  dialogVisible.value = true
}
function closeDialog(): void { if (!saving.value) { dialogVisible.value = false; formError.value = null } }
async function save(): Promise<void> {
  if (!form.commandCode.trim() || !form.commandName.trim()) { formError.value = 'コマンドコードとコマンド名は必須です。'; return }
  saving.value = true
  formError.value = null
  try {
    const request = { ...form, commandCode: form.commandCode.trim(), commandName: form.commandName.trim(), description: form.description.trim() || null }
    if (editingId.value) await updateTransportCommand(editingId.value, request)
    else await createTransportCommand(typeId.value, request)
    dialogVisible.value = false
    await load()
  } catch (error) { formError.value = message(error, 'コマンドの保存に失敗しました。') }
  finally { saving.value = false }
}
async function remove(item: TransportCommand): Promise<void> {
  if (!window.confirm(`コマンド「${item.commandName}」を削除しますか？Nodeで使用中の場合は削除できません。`)) return
  pageError.value = null
  try { await deleteTransportCommand(item.commandId); await load() }
  catch (error) { pageError.value = message(error, 'コマンドの削除に失敗しました。') }
}
function backToTypes(): void { router.push({ name: 'transport-vehicle-types', params: { manufacturerId: manufacturerId.value } }) }
onMounted(load)
</script>

<template>
  <MainLayout>
    <section class="page transport-master-page">
      <nav class="transport-breadcrumbs" aria-label="パンくず">
        <RouterLink :to="{ name: 'transport-manufacturers' }">AGF・AGVマスタ</RouterLink><span>›</span>
        <RouterLink :to="{ name: 'transport-vehicle-types', params: { manufacturerId } }">{{ manufacturer?.name ?? 'メーカー' }}</RouterLink><span>›</span>
        <strong>{{ vehicleType?.vehicleType ?? '種別' }} コマンド</strong>
      </nav>

      <div class="card transport-page-header">
        <div>
          <h1>{{ manufacturer?.name ?? 'メーカー' }} / {{ vehicleType?.vehicleType ?? '-' }} コマンド一覧</h1>
          <p>このメーカー・種別で使用できるAPIコマンドを管理します。</p>
        </div>
        <div class="transport-header-actions">
          <Button label="AGF・AGV一覧へ戻る" severity="secondary" @click="backToTypes" />
          <Button label="コマンドを追加" icon="pi pi-plus" @click="open()" />
        </div>
      </div>

      <p v-if="pageError" class="transport-message--error" role="alert">{{ pageError }}</p>

      <div class="card">
        <div class="transport-section-header">
          <h2>登録コマンド</h2>
          <div class="transport-section-actions"><span>{{ filteredItems.length }}件</span></div>
        </div>

        <div class="transport-toolbar">
          <label class="transport-toolbar-field is-search">
            <span>コード・名称・説明で検索</span>
            <InputText v-model="keyword" placeholder="コード、名称、説明で検索" />
          </label>
          <label class="transport-toolbar-field">
            <span>処理種別</span>
            <Select v-model="processTypeFilter" :options="processTypeOptions" option-label="label" option-value="value" />
          </label>
          <label class="transport-toolbar-field">
            <span>状態</span>
            <Select v-model="statusFilter" :options="statusOptions" option-label="label" option-value="value" />
          </label>
        </div>

        <p v-if="loading" class="transport-loading">読み込み中...</p>
        <p v-else-if="!filteredItems.length" class="transport-empty">
          {{ items.length ? '条件に一致するコマンドがありません。' : 'コマンドはまだありません。' }}
        </p>
        <div v-else class="transport-table-wrap">
          <table class="transport-table">
            <thead><tr><th>コード</th><th>コマンド名</th><th>処理種別</th><th>説明</th><th>状態</th><th>操作</th></tr></thead>
            <tbody>
              <tr v-for="item in filteredItems" :key="item.commandId">
                <td><code>{{ item.commandCode }}</code></td>
                <td>{{ item.commandName }}</td>
                <td><span class="transport-process-badge">{{ processTypeLabel(item.processType) }}</span></td>
                <td class="transport-description">{{ item.description || '説明なし' }}</td>
                <td><span class="transport-status" :class="{ 'is-inactive': !item.isActive }">{{ item.isActive ? '有効' : '無効' }}</span></td>
                <td>
                  <div class="transport-row-actions">
                    <Button label="編集" size="small" severity="secondary" @click="open(item)" />
                    <Button label="削除" size="small" severity="danger" @click="remove(item)" />
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <Dialog v-model:visible="dialogVisible" modal :header="editingId ? 'コマンドを編集' : 'コマンドを追加'" :style="{ width: 'min(600px, 94vw)' }" :closable="!saving">
        <p class="transport-context">登録先：{{ manufacturer?.name ?? 'メーカー' }} / {{ vehicleType?.vehicleType ?? '種別' }}</p>
        <div class="transport-form transport-form--grid">
          <label><span>コマンドコード <b>必須</b></span><InputText v-model="form.commandCode" autofocus /></label>
          <label><span>コマンド名 <b>必須</b></span><InputText v-model="form.commandName" /></label>
          <label class="transport-form__wide"><span>処理種別 <b>必須</b></span><Select v-model="form.processType" :options="commandProcessTypeOptions" option-label="label" option-value="value" /></label>
          <label class="transport-form__wide"><span>説明</span><Textarea v-model="form.description" rows="4" auto-resize /></label>
          <label><span>表示順</span><InputNumber v-model="form.sortOrder" :min="0" /></label>
          <label class="transport-toggle-field">
            <span><strong>有効状態</strong><small>無効にすると新しいNode設定では選択できなくなります。</small></span>
            <ToggleSwitch v-model="form.isActive" />
          </label>
          <p v-if="formError" class="transport-message--error transport-form__wide" role="alert">{{ formError }}</p>
        </div>
        <template #footer>
          <Button label="キャンセル" severity="secondary" :disabled="saving" @click="closeDialog" />
          <Button :label="editingId ? '更新' : '登録'" :loading="saving" @click="save" />
        </template>
      </Dialog>
    </section>
  </MainLayout>
</template>

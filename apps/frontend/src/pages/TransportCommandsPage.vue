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
  { value: 'ALL', label: 'すべての処理種別' },
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
    <section class="transport-master-page commands-page">
      <nav class="transport-breadcrumbs" aria-label="パンくず">
        <RouterLink :to="{ name: 'transport-manufacturers' }">AGF・AGVマスタ</RouterLink>
        <i class="pi pi-angle-right" />
        <RouterLink :to="{ name: 'transport-vehicle-types', params: { manufacturerId } }">{{ manufacturer?.name ?? 'メーカー' }}</RouterLink>
        <i class="pi pi-angle-right" />
        <strong>{{ vehicleType?.vehicleType ?? '種別' }} コマンド</strong>
      </nav>

      <section class="transport-hero">
        <div class="transport-hero__icon transport-hero__icon--command"><i class="pi pi-code" /></div>
        <div class="transport-hero__body">
          <span class="transport-eyebrow">COMMAND API</span>
          <h1>{{ manufacturer?.name ?? 'メーカー' }} / {{ vehicleType?.vehicleType ?? '-' }}</h1>
          <p>このメーカー・種別で使用できるAPIコマンドと処理種別を管理します。</p>
        </div>
        <div class="transport-hero__actions">
          <Button label="AGF・AGV一覧" icon="pi pi-arrow-left" severity="secondary" outlined @click="backToTypes" />
          <Button label="コマンドを追加" icon="pi pi-plus" @click="open()" />
        </div>
      </section>

      <div class="transport-stepper" aria-label="設定手順">
        <div class="transport-step is-complete"><span><i class="pi pi-check" /></span><strong>メーカー</strong></div>
        <i class="pi pi-angle-right" />
        <div class="transport-step is-complete"><span><i class="pi pi-check" /></span><strong>AGF・AGV</strong></div>
        <i class="pi pi-angle-right" />
        <div class="transport-step is-active"><span>3</span><strong>コマンド</strong></div>
      </div>

      <p v-if="pageError" class="transport-message transport-message--error" role="alert">
        <i class="pi pi-exclamation-circle" />{{ pageError }}
      </p>

      <section class="transport-panel">
        <div class="transport-toolbar transport-toolbar--commands">
          <div class="transport-search"><i class="pi pi-search" /><InputText v-model="keyword" placeholder="コード・名称・説明で検索" /></div>
          <Select v-model="processTypeFilter" :options="processTypeOptions" option-label="label" option-value="value" class="transport-filter transport-filter--wide" />
          <Select v-model="statusFilter" :options="statusOptions" option-label="label" option-value="value" class="transport-filter" />
          <span class="transport-count">{{ filteredItems.length }} / {{ items.length }}件</span>
        </div>

        <div class="transport-table transport-table--commands">
          <div class="transport-table__head">
            <span>コード</span><span>コマンド</span><span>処理種別</span><span>状態</span><span>操作</span>
          </div>
          <div v-if="loading" class="transport-empty"><i class="pi pi-spin pi-spinner" /><strong>コマンドを読み込んでいます</strong></div>
          <div v-else-if="!filteredItems.length" class="transport-empty">
            <i class="pi pi-code" />
            <strong>{{ items.length ? '条件に一致するコマンドがありません' : 'コマンドが登録されていません' }}</strong>
            <p>{{ items.length ? '検索条件を変更してください。' : 'この種別で使用する最初のAPIコマンドを追加してください。' }}</p>
            <Button v-if="!items.length" label="コマンドを追加" icon="pi pi-plus" outlined @click="open()" />
          </div>
          <article v-for="item in filteredItems" v-else :key="item.commandId" class="transport-table__row">
            <code class="command-code">{{ item.commandCode }}</code>
            <div class="command-name-cell"><strong>{{ item.commandName }}</strong><small>{{ item.description || '説明は登録されていません。' }}</small></div>
            <span class="process-type-tag"><small>{{ item.processType }}</small><strong>{{ processTypeLabel(item.processType) }}</strong></span>
            <span class="transport-status" :class="{ 'is-inactive': !item.isActive }">
              <i :class="item.isActive ? 'pi pi-check-circle' : 'pi pi-pause-circle'" />{{ item.isActive ? '有効' : '無効' }}
            </span>
            <div class="transport-actions">
              <Button label="編集" icon="pi pi-pencil" severity="secondary" size="small" @click="open(item)" />
              <Button label="削除" icon="pi pi-trash" severity="danger" size="small" @click="remove(item)" />
            </div>
          </article>
        </div>
      </section>

      <Dialog v-model:visible="dialogVisible" modal :header="editingId ? 'コマンドを編集' : 'コマンドを追加'" :style="{ width: 'min(600px, 94vw)' }" :closable="!saving">
        <div class="transport-context-banner"><i class="pi pi-code" /><span><small>登録先</small><strong>{{ manufacturer?.name ?? 'メーカー' }} / {{ vehicleType?.vehicleType ?? '種別' }}</strong></span></div>
        <div class="transport-form transport-form--grid">
          <label><span>コマンドコード <b>必須</b></span><InputText v-model="form.commandCode" autofocus placeholder="例：TravelToPosture" /></label>
          <label><span>コマンド名 <b>必須</b></span><InputText v-model="form.commandName" placeholder="例：移動指示" /></label>
          <label class="transport-form__wide"><span>処理種別 <b>必須</b></span><Select v-model="form.processType" :options="commandProcessTypeOptions" option-label="label" option-value="value" /></label>
          <label class="transport-form__wide"><span>説明</span><Textarea v-model="form.description" rows="4" auto-resize placeholder="APIの用途や補足情報" /></label>
          <label><span>表示順</span><InputNumber v-model="form.sortOrder" :min="0" placeholder="未指定" /></label>
          <label class="transport-toggle-field"><span><strong>有効状態</strong><small>無効にすると新しいNode設定では選択できなくなります。</small></span><ToggleSwitch v-model="form.isActive" /></label>
          <p v-if="formError" class="transport-message transport-message--error transport-form__wide" role="alert"><i class="pi pi-exclamation-circle" />{{ formError }}</p>
        </div>
        <template #footer>
          <Button label="キャンセル" severity="secondary" text :disabled="saving" @click="closeDialog" />
          <Button :label="editingId ? '更新' : '登録'" icon="pi pi-check" :loading="saving" @click="save" />
        </template>
      </Dialog>
    </section>
  </MainLayout>
</template>
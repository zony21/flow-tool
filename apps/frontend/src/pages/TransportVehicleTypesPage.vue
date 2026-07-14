<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import InputNumber from 'primevue/inputnumber'
import Select from 'primevue/select'
import Textarea from 'primevue/textarea'
import ToggleSwitch from 'primevue/toggleswitch'
import MainLayout from '../layouts/MainLayout.vue'
import {
  createTransportManufacturerVehicleType,
  deleteTransportManufacturerVehicleType,
  fetchTransportManufacturers,
  fetchTransportManufacturerVehicleTypes,
  updateTransportManufacturerVehicleType,
} from '../api/transportApi'
import type { TransportManufacturer, TransportManufacturerVehicleType, VehicleType } from '../types/transport'
import './transport-master-page.css'

const route = useRoute()
const router = useRouter()
const manufacturerId = computed(() => String(route.params.manufacturerId ?? ''))
const manufacturers = ref<TransportManufacturer[]>([])
const items = ref<TransportManufacturerVehicleType[]>([])
const loading = ref(false)
const saving = ref(false)
const dialogVisible = ref(false)
const editingId = ref<string | null>(null)
const pageError = ref<string | null>(null)
const formError = ref<string | null>(null)
const form = reactive({ vehicleType: 'AGF' as VehicleType, description: '', sortOrder: null as number | null, isActive: true })
const vehicleTypeOptions = [
  { label: 'AGF', value: 'AGF', description: 'フォークリフト型搬送機' },
  { label: 'AGV', value: 'AGV', description: '無人搬送車' },
]
const manufacturer = computed(() => manufacturers.value.find((item) => item.manufacturerId === manufacturerId.value) ?? null)
const visibleItems = computed(() => items.value.filter((item) => item.manufacturerId === manufacturerId.value)
  .sort((a, b) => a.sortOrder - b.sortOrder || a.vehicleType.localeCompare(b.vehicleType)))
const availableTypes = computed(() => vehicleTypeOptions.filter((option) => !visibleItems.value.some((item) => item.vehicleType === option.value)))
function message(error: unknown, fallback: string): string { return error instanceof Error ? error.message : fallback }
async function load(): Promise<void> {
  loading.value = true
  pageError.value = null
  try {
    const [manufacturerList, typeList] = await Promise.all([fetchTransportManufacturers(), fetchTransportManufacturerVehicleTypes(true)])
    manufacturers.value = manufacturerList
    items.value = typeList
  } catch (error) { pageError.value = message(error, 'AGF・AGV種別の取得に失敗しました。') }
  finally { loading.value = false }
}
function open(item?: TransportManufacturerVehicleType): void {
  editingId.value = item?.manufacturerVehicleTypeId ?? null
  Object.assign(form, {
    vehicleType: item?.vehicleType ?? availableTypes.value[0]?.value ?? 'AGF',
    description: item?.description ?? '', sortOrder: item?.sortOrder ?? null, isActive: item?.isActive ?? true,
  })
  formError.value = null
  dialogVisible.value = true
}
function closeDialog(): void { if (!saving.value) { dialogVisible.value = false; formError.value = null } }
async function save(): Promise<void> {
  saving.value = true
  formError.value = null
  try {
    const request = { ...form, description: form.description.trim() || null }
    if (editingId.value) await updateTransportManufacturerVehicleType(editingId.value, request)
    else await createTransportManufacturerVehicleType(manufacturerId.value, request)
    dialogVisible.value = false
    await load()
  } catch (error) { formError.value = message(error, 'AGF・AGV種別の保存に失敗しました。') }
  finally { saving.value = false }
}
async function remove(item: TransportManufacturerVehicleType): Promise<void> {
  if (!window.confirm(`${item.vehicleType}を削除しますか？登録済みコマンドがある場合は削除できません。`)) return
  pageError.value = null
  try { await deleteTransportManufacturerVehicleType(item.manufacturerVehicleTypeId); await load() }
  catch (error) { pageError.value = message(error, 'AGF・AGV種別の削除に失敗しました。') }
}
function showCommands(item: TransportManufacturerVehicleType): void {
  router.push({ name: 'transport-commands', params: { manufacturerId: manufacturerId.value, typeId: item.manufacturerVehicleTypeId } })
}
function backToManufacturers(): void { router.push({ name: 'transport-manufacturers' }) }
onMounted(load)
</script>

<template>
  <MainLayout>
    <section class="page transport-master-page">
      <nav class="transport-breadcrumbs" aria-label="パンくず">
        <RouterLink :to="{ name: 'transport-manufacturers' }">AGF・AGVマスタ</RouterLink><span>›</span><strong>{{ manufacturer?.name ?? 'メーカー' }}</strong>
      </nav>

      <div class="card transport-page-header">
        <div>
          <h1>{{ manufacturer?.name ?? 'メーカー' }} - AGF・AGV一覧</h1>
          <p>このメーカーが対応するAGF・AGV種別を管理します。</p>
        </div>
        <div class="transport-header-actions">
          <Button label="メーカー一覧へ戻る" severity="secondary" @click="backToManufacturers" />
          <Button label="種別を追加" icon="pi pi-plus" :disabled="availableTypes.length === 0" @click="open()" />
        </div>
      </div>

      <p v-if="pageError" class="transport-message--error" role="alert">{{ pageError }}</p>

      <div class="card">
        <div class="transport-section-header">
          <div><h2>対応種別</h2><p>AGFとAGVはそれぞれ1件まで登録できます。</p></div>
          <div class="transport-section-actions"><span>{{ visibleItems.length }}件</span></div>
        </div>

        <p v-if="loading" class="transport-loading">読み込み中...</p>
        <p v-else-if="!visibleItems.length" class="transport-empty">AGF・AGV種別はまだありません。</p>
        <div v-else class="transport-table-wrap">
          <table class="transport-table">
            <thead><tr><th>種別</th><th>説明</th><th>表示順</th><th>状態</th><th>操作</th></tr></thead>
            <tbody>
              <tr v-for="item in visibleItems" :key="item.manufacturerVehicleTypeId">
                <td><button class="transport-name-button" type="button" @click="showCommands(item)">{{ item.vehicleType }}</button></td>
                <td class="transport-description">{{ item.description || '説明なし' }}</td>
                <td>{{ item.sortOrder }}</td>
                <td><span class="transport-status" :class="{ 'is-inactive': !item.isActive }">{{ item.isActive ? '有効' : '無効' }}</span></td>
                <td>
                  <div class="transport-row-actions">
                    <Button label="コマンド" size="small" @click="showCommands(item)" />
                    <Button label="編集" size="small" severity="secondary" @click="open(item)" />
                    <Button label="削除" size="small" severity="danger" @click="remove(item)" />
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <Dialog v-model:visible="dialogVisible" modal :header="editingId ? 'AGF・AGV種別を編集' : 'AGF・AGV種別を追加'" :style="{ width: 'min(560px, 94vw)' }" :closable="!saving">
        <p class="transport-context">メーカー：{{ manufacturer?.name ?? '読み込み中' }}</p>
        <div class="transport-form">
          <label><span>種別 <b>必須</b></span><Select v-model="form.vehicleType" :options="editingId ? vehicleTypeOptions : availableTypes" option-label="label" option-value="value" :disabled="!!editingId" /></label>
          <label><span>説明</span><Textarea v-model="form.description" rows="4" auto-resize /></label>
          <label><span>表示順</span><InputNumber v-model="form.sortOrder" :min="0" /></label>
          <label class="transport-toggle-field">
            <span><strong>有効状態</strong><small>無効にすると新しいNode設定では選択できなくなります。</small></span>
            <ToggleSwitch v-model="form.isActive" />
          </label>
          <p v-if="formError" class="transport-message--error" role="alert">{{ formError }}</p>
        </div>
        <template #footer>
          <Button label="キャンセル" severity="secondary" :disabled="saving" @click="closeDialog" />
          <Button :label="editingId ? '更新' : '登録'" :loading="saving" @click="save" />
        </template>
      </Dialog>
    </section>
  </MainLayout>
</template>

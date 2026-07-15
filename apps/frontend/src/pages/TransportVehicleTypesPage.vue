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
import type {
  TransportManufacturer,
  TransportManufacturerVehicleType,
  VehicleType,
} from '../types/transport'
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

const form = reactive({
  vehicleType: 'AGF' as VehicleType,
  description: '',
  sortOrder: null as number | null,
  isActive: true,
})

const vehicleTypeOptions = [
  { label: 'AGF', value: 'AGF', description: 'フォークリフト型搬送機' },
  { label: 'AGV', value: 'AGV', description: '無人搬送車' },
]

const manufacturer = computed(() => (
  manufacturers.value.find((item) => item.manufacturerId === manufacturerId.value) ?? null
))
const visibleItems = computed(() => (
  items.value
    .filter((item) => item.manufacturerId === manufacturerId.value)
    .sort((a, b) => a.sortOrder - b.sortOrder || a.vehicleType.localeCompare(b.vehicleType))
))
const availableTypes = computed(() => vehicleTypeOptions.filter((option) => (
  !visibleItems.value.some((item) => item.vehicleType === option.value)
)))

function message(error: unknown, fallback: string): string {
  return error instanceof Error ? error.message : fallback
}

async function load(): Promise<void> {
  loading.value = true
  pageError.value = null
  try {
    const [manufacturerList, typeList] = await Promise.all([
      fetchTransportManufacturers(),
      fetchTransportManufacturerVehicleTypes(true),
    ])
    manufacturers.value = manufacturerList
    items.value = typeList
  } catch (error) {
    pageError.value = message(error, 'AGF・AGV種別の取得に失敗しました。')
  } finally {
    loading.value = false
  }
}

function open(item?: TransportManufacturerVehicleType): void {
  editingId.value = item?.manufacturerVehicleTypeId ?? null
  Object.assign(form, {
    vehicleType: item?.vehicleType ?? availableTypes.value[0]?.value ?? 'AGF',
    description: item?.description ?? '',
    sortOrder: item?.sortOrder ?? null,
    isActive: item?.isActive ?? true,
  })
  formError.value = null
  dialogVisible.value = true
}

function closeDialog(): void {
  if (saving.value) return
  dialogVisible.value = false
  formError.value = null
}

async function save(): Promise<void> {
  saving.value = true
  formError.value = null
  try {
    const request = {
      ...form,
      description: form.description.trim() || null,
    }
    if (editingId.value) {
      await updateTransportManufacturerVehicleType(editingId.value, request)
    } else {
      await createTransportManufacturerVehicleType(manufacturerId.value, request)
    }
    dialogVisible.value = false
    await load()
  } catch (error) {
    formError.value = message(error, 'AGF・AGV種別の保存に失敗しました。')
  } finally {
    saving.value = false
  }
}

async function remove(item: TransportManufacturerVehicleType): Promise<void> {
  if (!window.confirm(`${item.vehicleType}を削除しますか？登録済みコマンドがある場合は削除できません。`)) return
  pageError.value = null
  try {
    await deleteTransportManufacturerVehicleType(item.manufacturerVehicleTypeId)
    await load()
  } catch (error) {
    pageError.value = message(error, 'AGF・AGV種別の削除に失敗しました。')
  }
}

function showCommands(item: TransportManufacturerVehicleType): void {
  router.push({
    name: 'transport-commands',
    params: {
      manufacturerId: manufacturerId.value,
      typeId: item.manufacturerVehicleTypeId,
    },
  })
}

function backToManufacturers(): void {
  router.push({ name: 'transport-manufacturers' })
}

onMounted(load)
</script>

<template>
  <MainLayout>
    <section class="transport-master-page vehicle-types-page">
      <nav class="transport-breadcrumbs" aria-label="パンくず">
        <RouterLink :to="{ name: 'transport-manufacturers' }">AGF・AGVマスタ</RouterLink>
        <i class="pi pi-angle-right" />
        <strong>{{ manufacturer?.name ?? 'メーカー' }}</strong>
      </nav>

      <section class="transport-hero">
        <div class="transport-hero__icon transport-hero__icon--type">
          <i class="pi pi-truck" />
        </div>
        <div class="transport-hero__body">
          <span class="transport-eyebrow">MANUFACTURER</span>
          <h1>{{ manufacturer?.name ?? 'メーカー' }}</h1>
          <p>このメーカーが対応するAGF・AGV種別と、その配下のコマンドAPIを管理します。</p>
        </div>
        <div class="transport-hero__actions">
          <Button label="メーカー一覧" icon="pi pi-arrow-left" severity="secondary" outlined @click="backToManufacturers" />
          <Button
            label="種別を追加"
            icon="pi pi-plus"
            :disabled="availableTypes.length === 0"
            @click="open()"
          />
        </div>
      </section>

      <div class="transport-stepper" aria-label="設定手順">
        <div class="transport-step is-complete"><span><i class="pi pi-check" /></span><strong>メーカー</strong></div>
        <i class="pi pi-angle-right" />
        <div class="transport-step is-active"><span>2</span><strong>AGF・AGV</strong></div>
        <i class="pi pi-angle-right" />
        <div class="transport-step"><span>3</span><strong>コマンド</strong></div>
      </div>

      <p v-if="pageError" class="transport-message transport-message--error" role="alert">
        <i class="pi pi-exclamation-circle" />
        {{ pageError }}
      </p>

      <section class="transport-panel">
        <div class="transport-section-header">
          <div>
            <span class="transport-eyebrow">SUPPORTED VEHICLE TYPES</span>
            <h2>対応種別</h2>
            <p>AGFとAGVはそれぞれ1件まで登録できます。</p>
          </div>
          <span class="transport-count">{{ visibleItems.length }} / 2件</span>
        </div>

        <div v-if="loading" class="transport-empty">
          <i class="pi pi-spin pi-spinner" />
          <strong>対応種別を読み込んでいます</strong>
        </div>

        <div v-else-if="!visibleItems.length" class="transport-empty">
          <i class="pi pi-truck" />
          <strong>AGF・AGV種別が登録されていません</strong>
          <p>このメーカーが対応する種別を追加してください。</p>
          <Button label="種別を追加" icon="pi pi-plus" outlined @click="open()" />
        </div>

        <div v-else class="vehicle-type-grid">
          <article
            v-for="item in visibleItems"
            :key="item.manufacturerVehicleTypeId"
            class="vehicle-type-card"
            :class="{ 'is-inactive': !item.isActive }"
          >
            <div class="vehicle-type-card__top">
              <span class="vehicle-type-badge">{{ item.vehicleType }}</span>
              <span class="transport-status" :class="{ 'is-inactive': !item.isActive }">
                <i :class="item.isActive ? 'pi pi-check-circle' : 'pi pi-pause-circle'" />
                {{ item.isActive ? '有効' : '無効' }}
              </span>
            </div>
            <div class="vehicle-type-card__body">
              <span class="transport-eyebrow">VEHICLE TYPE</span>
              <h3>{{ item.vehicleType }}</h3>
              <p>{{ item.description || `${item.vehicleType}向けコマンドAPIを管理します。` }}</p>
            </div>
            <div class="vehicle-type-card__footer">
              <Button label="コマンド管理" icon="pi pi-arrow-right" @click="showCommands(item)" />
              <div class="transport-actions">
                <Button icon="pi pi-pencil" text rounded aria-label="編集" @click="open(item)" />
                <Button icon="pi pi-trash" text rounded severity="danger" aria-label="削除" @click="remove(item)" />
              </div>
            </div>
          </article>

          <button
            v-if="availableTypes.length"
            type="button"
            class="vehicle-type-card vehicle-type-card--add"
            @click="open()"
          >
            <span><i class="pi pi-plus" /></span>
            <strong>{{ availableTypes[0]?.label }}を追加</strong>
            <small>{{ availableTypes[0]?.description }}</small>
          </button>
        </div>
      </section>

      <Dialog
        v-model:visible="dialogVisible"
        modal
        :header="editingId ? 'AGF・AGV種別を編集' : 'AGF・AGV種別を追加'"
        :style="{ width: 'min(560px, 94vw)' }"
        :closable="!saving"
      >
        <div class="transport-context-banner">
          <i class="pi pi-building" />
          <span>
            <small>メーカー</small>
            <strong>{{ manufacturer?.name ?? '読み込み中' }}</strong>
          </span>
        </div>

        <div class="transport-form">
          <label>
            <span>種別 <b>必須</b></span>
            <Select
              v-model="form.vehicleType"
              :options="editingId ? vehicleTypeOptions : availableTypes"
              option-label="label"
              option-value="value"
              :disabled="!!editingId"
            />
          </label>
          <label>
            <span>説明</span>
            <Textarea v-model="form.description" rows="4" auto-resize placeholder="この種別に関する補足情報" />
          </label>
          <label>
            <span>表示順</span>
            <InputNumber v-model="form.sortOrder" :min="0" placeholder="未指定" />
          </label>
          <label class="transport-toggle-field">
            <span>
              <strong>有効状態</strong>
              <small>無効にすると新しいNode設定では選択できなくなります。</small>
            </span>
            <ToggleSwitch v-model="form.isActive" />
          </label>
          <p v-if="formError" class="transport-message transport-message--error" role="alert">
            <i class="pi pi-exclamation-circle" />
            {{ formError }}
          </p>
        </div>

        <template #footer>
          <Button label="キャンセル" severity="secondary" text :disabled="saving" @click="closeDialog" />
          <Button :label="editingId ? '更新' : '登録'" icon="pi pi-check" :loading="saving" @click="save" />
        </template>
      </Dialog>
    </section>
  </MainLayout>
</template>
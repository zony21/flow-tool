<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import InputNumber from 'primevue/inputnumber'
import InputText from 'primevue/inputtext'
import Select from 'primevue/select'
import Textarea from 'primevue/textarea'
import ToggleSwitch from 'primevue/toggleswitch'
import MainLayout from '../layouts/MainLayout.vue'
import {
  createTransportManufacturer,
  deleteTransportManufacturer,
  fetchTransportManufacturers,
  updateTransportManufacturer,
} from '../api/transportApi'
import type { TransportManufacturer } from '../types/transport'
import './transport-master-page.css'

const router = useRouter()

const items = ref<TransportManufacturer[]>([])
const loading = ref(false)
const saving = ref(false)
const dialogVisible = ref(false)
const editingId = ref<string | null>(null)
const pageError = ref<string | null>(null)
const formError = ref<string | null>(null)
const keyword = ref('')
const statusFilter = ref<'ALL' | 'ACTIVE' | 'INACTIVE'>('ALL')

const form = reactive({
  name: '',
  description: '',
  sortOrder: null as number | null,
  isActive: true,
})

const statusOptions = [
  { label: 'すべて', value: 'ALL' },
  { label: '有効', value: 'ACTIVE' },
  { label: '無効', value: 'INACTIVE' },
]

const filteredItems = computed(() => {
  const normalizedKeyword = keyword.value.trim().toLocaleLowerCase('ja')
  return items.value.filter((item) => {
    const matchesKeyword = !normalizedKeyword
      || item.name.toLocaleLowerCase('ja').includes(normalizedKeyword)
      || (item.description ?? '').toLocaleLowerCase('ja').includes(normalizedKeyword)
    const matchesStatus = statusFilter.value === 'ALL'
      || (statusFilter.value === 'ACTIVE' && item.isActive)
      || (statusFilter.value === 'INACTIVE' && !item.isActive)
    return matchesKeyword && matchesStatus
  })
})

function message(error: unknown, fallback: string): string {
  return error instanceof Error ? error.message : fallback
}

async function load(): Promise<void> {
  loading.value = true
  pageError.value = null
  try {
    items.value = await fetchTransportManufacturers()
  } catch (error) {
    pageError.value = message(error, 'メーカーの取得に失敗しました。')
  } finally {
    loading.value = false
  }
}

function open(item?: TransportManufacturer): void {
  editingId.value = item?.manufacturerId ?? null
  Object.assign(form, {
    name: item?.name ?? '',
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
  if (!form.name.trim()) {
    formError.value = 'メーカー名は必須です。'
    return
  }

  saving.value = true
  formError.value = null
  try {
    const request = {
      ...form,
      name: form.name.trim(),
      description: form.description.trim() || null,
    }
    if (editingId.value) {
      await updateTransportManufacturer(editingId.value, request)
    } else {
      await createTransportManufacturer(request)
    }
    dialogVisible.value = false
    await load()
  } catch (error) {
    formError.value = message(error, 'メーカーの保存に失敗しました。')
  } finally {
    saving.value = false
  }
}

async function remove(item: TransportManufacturer): Promise<void> {
  if (!window.confirm(`メーカー「${item.name}」を削除しますか？`)) return
  pageError.value = null
  try {
    await deleteTransportManufacturer(item.manufacturerId)
    await load()
  } catch (error) {
    pageError.value = message(error, 'メーカーの削除に失敗しました。')
  }
}

function showTypes(item: TransportManufacturer): void {
  router.push({
    name: 'transport-vehicle-types',
    params: { manufacturerId: item.manufacturerId },
  })
}

onMounted(load)
</script>

<template>
  <MainLayout>
    <section class="transport-master-page">
      <nav class="transport-breadcrumbs" aria-label="パンくず">
        <span>グローバル設定</span>
        <i class="pi pi-angle-right" />
        <strong>AGF・AGVマスタ</strong>
      </nav>

      <section class="transport-hero">
        <div class="transport-hero__icon">
          <i class="pi pi-building" />
        </div>
        <div class="transport-hero__body">
          <span class="transport-eyebrow">TRANSPORT MASTER</span>
          <h1>メーカー管理</h1>
          <p>AGF・AGVを提供するメーカーと、メーカーごとの対応種別・APIコマンドを管理します。</p>
        </div>
        <Button label="メーカーを追加" icon="pi pi-plus" @click="open()" />
      </section>

      <div class="transport-stepper" aria-label="設定手順">
        <div class="transport-step is-active"><span>1</span><strong>メーカー</strong></div>
        <i class="pi pi-angle-right" />
        <div class="transport-step"><span>2</span><strong>AGF・AGV</strong></div>
        <i class="pi pi-angle-right" />
        <div class="transport-step"><span>3</span><strong>コマンド</strong></div>
      </div>

      <p v-if="pageError" class="transport-message transport-message--error" role="alert">
        <i class="pi pi-exclamation-circle" />
        {{ pageError }}
      </p>

      <section class="transport-panel">
        <div class="transport-toolbar">
          <div class="transport-search">
            <i class="pi pi-search" />
            <InputText v-model="keyword" placeholder="メーカー名・説明で検索" />
          </div>
          <Select
            v-model="statusFilter"
            :options="statusOptions"
            option-label="label"
            option-value="value"
            class="transport-filter"
          />
          <span class="transport-count">{{ filteredItems.length }} / {{ items.length }}件</span>
        </div>

        <div class="transport-table transport-table--manufacturers">
          <div class="transport-table__head">
            <span>メーカー</span>
            <span>説明</span>
            <span>状態</span>
            <span>操作</span>
          </div>

          <div v-if="loading" class="transport-empty">
            <i class="pi pi-spin pi-spinner" />
            <strong>メーカーを読み込んでいます</strong>
          </div>

          <div v-else-if="!filteredItems.length" class="transport-empty">
            <i class="pi pi-inbox" />
            <strong>{{ items.length ? '条件に一致するメーカーがありません' : 'メーカーが登録されていません' }}</strong>
            <p>{{ items.length ? '検索条件を変更してください。' : '最初のメーカーを追加してください。' }}</p>
            <Button v-if="!items.length" label="メーカーを追加" icon="pi pi-plus" outlined @click="open()" />
          </div>

          <article
            v-for="item in filteredItems"
            v-else
            :key="item.manufacturerId"
            class="transport-table__row"
          >
            <button class="transport-primary-cell" type="button" @click="showTypes(item)">
              <span class="transport-avatar"><i class="pi pi-building" /></span>
              <span>
                <strong>{{ item.name }}</strong>
                <small>AGF・AGV設定を開く</small>
              </span>
            </button>
            <p class="transport-description">{{ item.description || '説明は登録されていません。' }}</p>
            <span class="transport-status" :class="{ 'is-inactive': !item.isActive }">
              <i :class="item.isActive ? 'pi pi-check-circle' : 'pi pi-pause-circle'" />
              {{ item.isActive ? '有効' : '無効' }}
            </span>
            <div class="transport-actions">
              <Button label="AGF・AGV" icon="pi pi-arrow-right" text @click="showTypes(item)" />
              <Button icon="pi pi-pencil" text rounded aria-label="編集" @click="open(item)" />
              <Button icon="pi pi-trash" text rounded severity="danger" aria-label="削除" @click="remove(item)" />
            </div>
          </article>
        </div>
      </section>

      <Dialog
        v-model:visible="dialogVisible"
        modal
        :header="editingId ? 'メーカーを編集' : 'メーカーを追加'"
        :style="{ width: 'min(560px, 94vw)' }"
        :closable="!saving"
      >
        <div class="transport-dialog-intro">
          <span class="transport-avatar"><i class="pi pi-building" /></span>
          <div>
            <strong>{{ editingId ? '登録済みメーカーの情報を変更します' : '新しいメーカーを登録します' }}</strong>
            <p>AGF・AGV種別とコマンドは、保存後にメーカー詳細から設定できます。</p>
          </div>
        </div>

        <div class="transport-form">
          <label>
            <span>メーカー名 <b>必須</b></span>
            <InputText v-model="form.name" autofocus placeholder="例：メーカーA" />
          </label>
          <label>
            <span>説明</span>
            <Textarea v-model="form.description" rows="4" auto-resize placeholder="メーカーに関する補足情報" />
          </label>
          <label>
            <span>表示順</span>
            <InputNumber v-model="form.sortOrder" :min="0" placeholder="未指定" />
          </label>
          <label class="transport-toggle-field">
            <span>
              <strong>有効状態</strong>
              <small>無効にすると新しい設定では選択できなくなります。</small>
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

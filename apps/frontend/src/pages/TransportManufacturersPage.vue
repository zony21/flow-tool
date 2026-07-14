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
const form = reactive({ name: '', description: '', sortOrder: null as number | null, isActive: true })
const statusOptions = [
  { label: 'すべて', value: 'ALL' },
  { label: '有効', value: 'ACTIVE' },
  { label: '無効', value: 'INACTIVE' },
]
const filteredItems = computed(() => {
  const word = keyword.value.trim().toLocaleLowerCase('ja')
  return items.value.filter((item) => {
    const keywordMatched = !word || item.name.toLocaleLowerCase('ja').includes(word)
      || (item.description ?? '').toLocaleLowerCase('ja').includes(word)
    const statusMatched = statusFilter.value === 'ALL'
      || (statusFilter.value === 'ACTIVE' && item.isActive)
      || (statusFilter.value === 'INACTIVE' && !item.isActive)
    return keywordMatched && statusMatched
  })
})
function message(error: unknown, fallback: string): string { return error instanceof Error ? error.message : fallback }
async function load(): Promise<void> {
  loading.value = true
  pageError.value = null
  try { items.value = await fetchTransportManufacturers() }
  catch (error) { pageError.value = message(error, 'メーカーの取得に失敗しました。') }
  finally { loading.value = false }
}
function open(item?: TransportManufacturer): void {
  editingId.value = item?.manufacturerId ?? null
  Object.assign(form, {
    name: item?.name ?? '', description: item?.description ?? '',
    sortOrder: item?.sortOrder ?? null, isActive: item?.isActive ?? true,
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
  if (!form.name.trim()) { formError.value = 'メーカー名は必須です。'; return }
  saving.value = true
  formError.value = null
  try {
    const request = { ...form, name: form.name.trim(), description: form.description.trim() || null }
    if (editingId.value) await updateTransportManufacturer(editingId.value, request)
    else await createTransportManufacturer(request)
    dialogVisible.value = false
    await load()
  } catch (error) { formError.value = message(error, 'メーカーの保存に失敗しました。') }
  finally { saving.value = false }
}
async function remove(item: TransportManufacturer): Promise<void> {
  if (!window.confirm(`メーカー「${item.name}」を削除しますか？`)) return
  pageError.value = null
  try { await deleteTransportManufacturer(item.manufacturerId); await load() }
  catch (error) { pageError.value = message(error, 'メーカーの削除に失敗しました。') }
}
function showTypes(item: TransportManufacturer): void {
  router.push({ name: 'transport-vehicle-types', params: { manufacturerId: item.manufacturerId } })
}
onMounted(load)
</script>

<template>
  <MainLayout>
    <section class="page transport-master-page">
      <nav class="transport-breadcrumbs" aria-label="パンくず">
        <span>グローバル設定</span><span>›</span><strong>AGF・AGVマスタ</strong>
      </nav>

      <div class="card transport-page-header">
        <div>
          <h1>メーカー一覧</h1>
          <p>AGF・AGVシステムを提供するメーカーを管理します。</p>
        </div>
        <div class="transport-header-actions">
          <Button label="メーカーを追加" icon="pi pi-plus" @click="open()" />
        </div>
      </div>

      <p v-if="pageError" class="transport-message--error" role="alert">{{ pageError }}</p>

      <div class="card">
        <div class="transport-section-header">
          <h2>登録メーカー</h2>
          <div class="transport-section-actions"><span>{{ filteredItems.length }}件</span></div>
        </div>

        <div class="transport-toolbar">
          <label class="transport-toolbar-field is-search">
            <span>メーカー名・説明で検索</span>
            <InputText v-model="keyword" placeholder="メーカー名や説明で検索" />
          </label>
          <label class="transport-toolbar-field">
            <span>状態</span>
            <Select v-model="statusFilter" :options="statusOptions" option-label="label" option-value="value" />
          </label>
        </div>

        <p v-if="loading" class="transport-loading">読み込み中...</p>
        <p v-else-if="!filteredItems.length" class="transport-empty">
          {{ items.length ? '条件に一致するメーカーがありません。' : 'メーカーはまだありません。' }}
        </p>
        <div v-else class="transport-table-wrap">
          <table class="transport-table">
            <thead><tr><th>メーカー名</th><th>説明</th><th>状態</th><th>操作</th></tr></thead>
            <tbody>
              <tr v-for="item in filteredItems" :key="item.manufacturerId">
                <td><button class="transport-name-button" type="button" @click="showTypes(item)">{{ item.name }}</button></td>
                <td class="transport-description">{{ item.description || '説明なし' }}</td>
                <td><span class="transport-status" :class="{ 'is-inactive': !item.isActive }">{{ item.isActive ? '有効' : '無効' }}</span></td>
                <td>
                  <div class="transport-row-actions">
                    <Button label="設定" size="small" @click="showTypes(item)" />
                    <Button label="編集" size="small" severity="secondary" @click="open(item)" />
                    <Button label="削除" size="small" severity="danger" @click="remove(item)" />
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <Dialog v-model:visible="dialogVisible" modal :header="editingId ? 'メーカーを編集' : 'メーカーを追加'" :style="{ width: 'min(560px, 94vw)' }" :closable="!saving">
        <div class="transport-form">
          <label><span>メーカー名 <b>必須</b></span><InputText v-model="form.name" autofocus /></label>
          <label><span>説明</span><Textarea v-model="form.description" rows="4" auto-resize /></label>
          <label><span>表示順</span><InputNumber v-model="form.sortOrder" :min="0" /></label>
          <label class="transport-toggle-field">
            <span><strong>有効状態</strong><small>無効にすると新しい設定では選択できなくなります。</small></span>
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

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import MainLayout from '../layouts/MainLayout.vue'
import { createTransportManufacturer, deleteTransportManufacturer, fetchTransportManufacturers, updateTransportManufacturer } from '../api/transportApi'
import type { TransportManufacturer } from '../types/transport'

const router = useRouter()
const items = ref<TransportManufacturer[]>([])
const loading = ref(false), saving = ref(false), dialogVisible = ref(false)
const editingId = ref<string | null>(null), error = ref<string | null>(null)
const form = reactive({ name: '', description: '', sortOrder: null as number | null, isActive: true })
const message = (e: unknown, fallback: string) => e instanceof Error ? e.message : fallback
async function load() { loading.value = true; error.value = null; try { items.value = await fetchTransportManufacturers() } catch (e) { error.value = message(e, 'メーカーの取得に失敗しました。') } finally { loading.value = false } }
function open(item?: TransportManufacturer) { editingId.value = item?.manufacturerId ?? null; Object.assign(form, { name: item?.name ?? '', description: item?.description ?? '', sortOrder: item?.sortOrder ?? null, isActive: item?.isActive ?? true }); error.value = null; dialogVisible.value = true }
async function save() { if (!form.name.trim()) { error.value = 'メーカー名は必須です。'; return } saving.value = true; error.value = null; try { const request = { ...form, name: form.name.trim(), description: form.description.trim() || null }; editingId.value ? await updateTransportManufacturer(editingId.value, request) : await createTransportManufacturer(request); dialogVisible.value = false; await load() } catch (e) { error.value = message(e, 'メーカーの保存に失敗しました。') } finally { saving.value = false } }
async function remove(item: TransportManufacturer) { if (!confirm(`メーカー「${item.name}」を削除しますか？`)) return; try { await deleteTransportManufacturer(item.manufacturerId); await load() } catch (e) { error.value = message(e, 'メーカーの削除に失敗しました。') } }
function showTypes(item: TransportManufacturer) { router.push({ name: 'transport-vehicle-types', params: { manufacturerId: item.manufacturerId } }) }
onMounted(load)
</script>

<template><MainLayout><section class="page master-page">
  <div class="breadcrumbs"><span>グローバル設定</span><span>›</span><strong>AGF・AGVメーカー</strong></div>
  <header class="card page-header"><div><h1>メーカー一覧</h1><p>AGF・AGVシステムを提供するメーカーを管理します。</p></div><Button label="メーカーを追加" icon="pi pi-plus" @click="open()" /></header>
  <p v-if="error" class="error" role="alert">{{ error }}</p>
  <section class="card list-card"><div class="list-header"><strong>登録メーカー</strong><span>{{ items.length }}件</span></div><p v-if="loading" class="empty">読み込み中...</p><p v-else-if="!items.length" class="empty">メーカーがありません。メーカーを追加してください。</p>
    <button v-for="item in items" :key="item.manufacturerId" class="master-row" @click="showTypes(item)"><span class="row-main"><strong>{{ item.name }}</strong><small>{{ item.description || '説明なし' }}</small></span><span class="status" :class="{ inactive: !item.isActive }">{{ item.isActive ? '有効' : '無効' }}</span><span class="actions"><Button icon="pi pi-pencil" text rounded aria-label="編集" @click.stop="open(item)" /><Button icon="pi pi-trash" text rounded severity="danger" aria-label="削除" @click.stop="remove(item)" /><i class="pi pi-chevron-right" /></span></button>
  </section>
  <Dialog v-model:visible="dialogVisible" modal :header="editingId ? 'メーカーを編集' : 'メーカーを追加'" :style="{ width: 'min(520px, 92vw)' }"><div class="form"><label>メーカー名 <b>必須</b><input v-model="form.name" /></label><label>説明<textarea v-model="form.description" rows="3" /></label><label>表示順<input v-model.number="form.sortOrder" type="number" min="0" /></label><label class="check"><input v-model="form.isActive" type="checkbox" />有効にする</label><p v-if="error" class="error">{{ error }}</p></div><template #footer><Button label="キャンセル" severity="secondary" text @click="dialogVisible=false" /><Button label="保存" :loading="saving" @click="save" /></template></Dialog>
</section></MainLayout></template>

<style scoped>
.master-page{display:flex;flex-direction:column;gap:14px;max-width:960px}.breadcrumbs{display:flex;gap:8px;color:#64748b;font-size:12px}.breadcrumbs strong{color:#334155}.page-header{display:flex;align-items:center;justify-content:space-between;gap:20px}.page-header h1{margin:0 0 6px;font-size:21px}.page-header p{margin:0;color:#64748b;font-size:13px}.list-card{padding:0;overflow:hidden}.list-header{display:flex;justify-content:space-between;padding:14px 18px;border-bottom:1px solid #e2e8f0}.list-header span{color:#64748b;font-size:12px}.master-row{display:flex;align-items:center;gap:14px;width:100%;min-height:72px;padding:12px 18px;color:#0f172a;text-align:left;background:#fff;border:0;border-bottom:1px solid #e2e8f0;cursor:pointer}.master-row:hover{background:#f8fafc}.row-main{display:flex;flex:1;flex-direction:column;gap:5px;min-width:0}.row-main small{overflow:hidden;color:#64748b;text-overflow:ellipsis;white-space:nowrap}.actions{display:flex;align-items:center;gap:3px;color:#94a3b8}.status{padding:3px 9px;color:#166534;background:#dcfce7;border-radius:999px;font-size:11px;font-weight:700}.status.inactive{color:#475569;background:#e2e8f0}.empty{padding:50px;text-align:center;color:#64748b}.error{padding:10px;color:#991b1b;background:#fef2f2;border:1px solid #fecaca;border-radius:7px}.form{display:flex;flex-direction:column;gap:14px}.form label{display:flex;flex-direction:column;gap:6px;font-size:12px;font-weight:700}.form label b{color:#dc2626;font-size:10px}.form input,.form textarea{padding:9px;border:1px solid #cbd5e1;border-radius:7px;font:inherit;font-weight:400}.form .check{align-items:center;flex-direction:row}.form .check input{width:auto}@media(max-width:640px){.page-header{align-items:flex-start;flex-direction:column}.master-row{padding:10px}.row-main small{max-width:180px}}
</style>

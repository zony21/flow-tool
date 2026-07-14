<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import Button from 'primevue/button'
import MainLayout from '../layouts/MainLayout.vue'
import { createTransportManufacturer, createTransportVehicleModel, deleteTransportVehicleModel, fetchTransportManufacturers, fetchTransportVehicleModels, updateTransportVehicleModel } from '../api/transportApi'
import type { TransportManufacturer, TransportVehicleModel } from '../types/transport'

const manufacturers = ref<TransportManufacturer[]>([])
const models = ref<TransportVehicleModel[]>([])
const editingId = ref<string | null>(null)
const error = ref<string | null>(null)
const form = reactive({ manufacturerName: '', vehicleType: 'AGF' as 'AGF' | 'AGV', modelCode: '', modelName: '', description: '', sortOrder: null as number | null, isActive: true })

async function load(): Promise<void> {
  try { [manufacturers.value, models.value] = await Promise.all([fetchTransportManufacturers(), fetchTransportVehicleModels({ includeInactive: true })]) }
  catch (e) { error.value = e instanceof Error ? e.message : '取得に失敗しました。' }
}
function reset(): void { editingId.value = null; Object.assign(form, { manufacturerName: '', vehicleType: 'AGF', modelCode: '', modelName: '', description: '', sortOrder: null, isActive: true }) }
function edit(item: TransportVehicleModel): void { editingId.value = item.vehicleModelId; Object.assign(form, { manufacturerName: item.manufacturerName, vehicleType: item.vehicleType, modelCode: item.modelCode, modelName: item.modelName, description: item.description ?? '', sortOrder: item.sortOrder, isActive: item.isActive }) }
async function save(): Promise<void> {
  error.value = null
  const manufacturerName = form.manufacturerName.trim()
  if (!manufacturerName || !form.modelCode.trim() || !form.modelName.trim()) { error.value = 'メーカー、型式コード、型式名は必須です。'; return }
  try {
    let manufacturer = manufacturers.value.find((item) => item.vehicleType === form.vehicleType && item.name.localeCompare(manufacturerName, undefined, { sensitivity: 'accent' }) === 0)
    if (!manufacturer) {
      manufacturer = await createTransportManufacturer({ name: manufacturerName, vehicleType: form.vehicleType, description: null, sortOrder: null })
      manufacturers.value.push(manufacturer)
    }
    const request = { manufacturerId: manufacturer.manufacturerId, vehicleType: form.vehicleType, modelCode: form.modelCode.trim(), modelName: form.modelName.trim(), description: form.description.trim() || null, sortOrder: form.sortOrder, isActive: form.isActive }
    editingId.value ? await updateTransportVehicleModel(editingId.value, request) : await createTransportVehicleModel(request); reset(); await load()
  }
  catch (e) { error.value = e instanceof Error ? e.message : '保存に失敗しました。' }
}
async function remove(item: TransportVehicleModel): Promise<void> { if (!confirm(`${item.modelName}を削除しますか？`)) return; try { await deleteTransportVehicleModel(item.vehicleModelId); await load() } catch (e) { error.value = e instanceof Error ? e.message : '削除に失敗しました。' } }
onMounted(load)
</script>

<template><MainLayout><section class="page settings-page"><div class="card"><h1>AGF・AGVマスタ管理</h1><p>Project共通で利用するメーカー機体モデルを管理します。</p></div>
<p v-if="error" class="error">{{ error }}</p>
<div class="card form"><h2>{{ editingId ? 'AGF/AGVモデルを編集' : 'AGF/AGVモデルを登録' }}</h2>
<label>種別<select v-model="form.vehicleType"><option>AGF</option><option>AGV</option></select></label>
<label>メーカー<input v-model="form.manufacturerName" type="text" placeholder="例: 豊田自動織機" /></label>
<label>型式コード<input v-model="form.modelCode" /></label><label>型式名<input v-model="form.modelName" /></label><label>説明<textarea v-model="form.description" /></label><label>表示順<input v-model.number="form.sortOrder" type="number" /></label><label class="check"><input v-model="form.isActive" type="checkbox" />有効</label>
<div><Button label="保存" @click="save" /> <Button v-if="editingId" label="キャンセル" severity="secondary" @click="reset" /></div></div>
<div class="card"><table><thead><tr><th>メーカー</th><th>種別</th><th>型式コード</th><th>型式名</th><th>状態</th><th>操作</th></tr></thead><tbody><tr v-for="item in models" :key="item.vehicleModelId"><td>{{ item.manufacturerName }}</td><td>{{ item.vehicleType }}</td><td>{{ item.modelCode }}</td><td>{{ item.modelName }}</td><td>{{ item.isActive ? '有効' : '無効' }}</td><td><Button label="編集" size="small" @click="edit(item)" /> <Button label="削除" size="small" severity="danger" @click="remove(item)" /></td></tr></tbody></table></div>
</section></MainLayout></template>
<style scoped>.settings-page,.form,label{display:flex;flex-direction:column;gap:12px}.form{max-width:720px}input,select,textarea{padding:8px;border:1px solid var(--border);border-radius:6px}.check{flex-direction:row}table{width:100%;border-collapse:collapse}th,td{padding:10px;border-bottom:1px solid var(--border);text-align:left}.error{color:#991b1b}</style>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import MainLayout from '../layouts/MainLayout.vue'
import { createFlowVersion, fetchFlowVersions, restoreFlowVersion } from '../api/versionApi'
import { useProjectPermissionStore } from '../stores/projectPermissionStore'
import { PermissionCodes } from '../types/permission'
import type { FlowVersionSummary } from '../types/version'

const route = useRoute()
const router = useRouter()
const permissionStore = useProjectPermissionStore()

const projectId = computed(() => String(route.params.projectId ?? ''))
const flowId = computed(() => String(route.params.flowId ?? ''))
const versions = ref<FlowVersionSummary[]>([])
const loading = ref(false)
const comment = ref('')
const canCreateVersion = computed(() => permissionStore.can(PermissionCodes.VersionCreate))
const canRestoreVersion = computed(() => permissionStore.can(PermissionCodes.VersionCreate))

async function loadVersions(): Promise<void> {
  if (!projectId.value || !flowId.value) return
  loading.value = true
  try {
    versions.value = await fetchFlowVersions(projectId.value, flowId.value)
  } finally {
    loading.value = false
  }
}

async function createVersion(): Promise<void> {
  if (!projectId.value || !flowId.value || !canCreateVersion.value) return
  loading.value = true
  try {
    await createFlowVersion(projectId.value, flowId.value, { comment: comment.value || null })
    comment.value = ''
    await loadVersions()
  } finally {
    loading.value = false
  }
}

async function restoreVersion(versionId: string): Promise<void> {
  if (!projectId.value || !flowId.value || !canRestoreVersion.value) return

  const confirmed = window.confirm('現在状態を復元前Versionとして保存し、選択Versionを復元します。続行しますか？')
  if (!confirmed) {
    return
  }

  loading.value = true
  try {
    await restoreFlowVersion(projectId.value, flowId.value, versionId)
    await router.push({ name: 'flow-editor', params: { projectId: projectId.value, flowId: flowId.value } })
  } finally {
    loading.value = false
  }
}

onMounted(loadVersions)
</script>

<template>
  <MainLayout>
    <section class="page">
      <div class="card page-header">
        <div>
          <h1>バージョン管理</h1>
          <p>Flowの保存スナップショットを一覧表示します。</p>
        </div>
        <Button label="エディタへ戻る" severity="secondary" @click="router.push({ name: 'flow-editor', params: { projectId, flowId } })" />
      </div>

      <div class="card create-card">
        <h2>新しいVersionを作成</h2>
        <textarea v-model="comment" rows="3" placeholder="変更概要を入力（任意）" :disabled="loading || !canCreateVersion" />
        <div class="actions">
          <Button label="Version作成" :disabled="loading || !canCreateVersion" @click="createVersion" />
        </div>
      </div>

      <div class="card">
        <h2>Version一覧</h2>
        <p v-if="loading">読み込み中...</p>
        <p v-else-if="versions.length === 0">Versionはまだありません。</p>
        <table v-else class="version-table">
          <thead>
            <tr>
              <th>Version</th>
              <th>コメント</th>
              <th>作成日時</th>
              <th>Node</th>
              <th>Link</th>
              <th>Comment</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="version in versions" :key="version.versionId">
              <td>{{ version.displayVersion }}</td>
              <td>{{ version.comment || 'コメントなし' }}</td>
              <td>{{ new Date(version.createdAtUtc).toLocaleString() }}</td>
              <td>{{ version.nodeCount }}</td>
              <td>{{ version.linkCount }}</td>
              <td>{{ version.commentCount }}</td>
              <td>
                <Button label="復元" size="small" :disabled="loading || !canRestoreVersion" @click="restoreVersion(version.versionId)" />
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  </MainLayout>
</template>

<style scoped>
.page {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.card {
  padding: 16px;
  background: #fff;
  border: 1px solid #dbe3ef;
  border-radius: 12px;
}

.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.create-card {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

textarea {
  width: 100%;
  box-sizing: border-box;
  padding: 10px 12px;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  resize: vertical;
}

.actions {
  display: flex;
  justify-content: flex-end;
}

.version-table {
  width: 100%;
  border-collapse: collapse;
}

.version-table th,
.version-table td {
  padding: 10px 12px;
  text-align: left;
  border-bottom: 1px solid #e2e8f0;
}
</style>

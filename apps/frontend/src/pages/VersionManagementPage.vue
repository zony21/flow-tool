<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import MainLayout from '../layouts/MainLayout.vue'
import { compareFlowVersions, createFlowVersion, fetchFlowVersions, restoreFlowVersion } from '../api/versionApi'
import { useProjectPermissionStore } from '../stores/projectPermissionStore'
import { PermissionCodes } from '../types/permission'
import type { FlowVersionCompareResponse, FlowVersionSummary } from '../types/version'

const route = useRoute()
const router = useRouter()
const permissionStore = useProjectPermissionStore()

const projectId = computed(() => String(route.params.projectId ?? ''))
const flowId = computed(() => String(route.params.flowId ?? ''))
const versions = ref<FlowVersionSummary[]>([])
const loading = ref(false)
const comment = ref('')
const leftVersionId = ref('')
const rightVersionId = ref('')
const compareResult = ref<FlowVersionCompareResponse | null>(null)

const canCreateVersion = computed(() => permissionStore.can(PermissionCodes.VersionCreate))
const canRestoreVersion = computed(() => permissionStore.can(PermissionCodes.VersionCreate))
const canCompareVersion = computed(() => permissionStore.can(PermissionCodes.VersionRead))

async function loadVersions(): Promise<void> {
  if (!projectId.value || !flowId.value) return
  loading.value = true
  try {
    versions.value = await fetchFlowVersions(projectId.value, flowId.value)
    if (!leftVersionId.value) {
      leftVersionId.value = versions.value[0]?.versionId ?? ''
    }
    if (!rightVersionId.value) {
      rightVersionId.value = versions.value[1]?.versionId ?? versions.value[0]?.versionId ?? ''
    }
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

async function compareVersions(): Promise<void> {
  if (!projectId.value || !flowId.value || !canCompareVersion.value || !leftVersionId.value || !rightVersionId.value) return
  loading.value = true
  try {
    compareResult.value = await compareFlowVersions(projectId.value, flowId.value, leftVersionId.value, rightVersionId.value)
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
              <th>作成者</th>
              <th>作成日時</th>
              <th>Node</th>
              <th>Link</th>
              <th>Comment</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="version in versions" :key="version.versionId">
              <td>{{ version.displayVersion }}</td>
              <td>{{ version.comment || 'コメントなし' }}</td>
              <td>{{ version.createdByDisplayName || '不明' }}</td>
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

      <div class="card create-card">
        <h2>Version比較</h2>
        <div class="compare-controls">
          <select v-model="leftVersionId" :disabled="loading || !canCompareVersion">
            <option v-for="version in versions" :key="`left-${version.versionId}`" :value="version.versionId">
              {{ version.displayVersion }}
            </option>
          </select>
          <span>vs</span>
          <select v-model="rightVersionId" :disabled="loading || !canCompareVersion">
            <option v-for="version in versions" :key="`right-${version.versionId}`" :value="version.versionId">
              {{ version.displayVersion }}
            </option>
          </select>
          <Button label="比較" :disabled="loading || !canCompareVersion || !leftVersionId || !rightVersionId" @click="compareVersions" />
        </div>

        <div v-if="compareResult" class="compare-grid">
          <div class="compare-card">
            <h3>Lane</h3>
            <ul>
              <li v-for="diff in compareResult.laneDiffs" :key="`lane-${diff.entityId}-${diff.changeType}`">{{ diff.changeType }}: {{ diff.label }}</li>
              <li v-if="compareResult.laneDiffs.length === 0">差分なし</li>
            </ul>
          </div>
          <div class="compare-card">
            <h3>Stage</h3>
            <ul>
              <li v-for="diff in compareResult.stageDiffs" :key="`stage-${diff.entityId}-${diff.changeType}`">{{ diff.changeType }}: {{ diff.label }}</li>
              <li v-if="compareResult.stageDiffs.length === 0">差分なし</li>
            </ul>
          </div>
          <div class="compare-card">
            <h3>Node</h3>
            <ul>
              <li v-for="diff in compareResult.nodeDiffs" :key="`node-${diff.entityId}-${diff.changeType}`">{{ diff.changeType }}: {{ diff.label }}</li>
              <li v-if="compareResult.nodeDiffs.length === 0">差分なし</li>
            </ul>
          </div>
          <div class="compare-card">
            <h3>Link</h3>
            <ul>
              <li v-for="diff in compareResult.linkDiffs" :key="`link-${diff.entityId}-${diff.changeType}`">{{ diff.changeType }}: {{ diff.label }}</li>
              <li v-if="compareResult.linkDiffs.length === 0">差分なし</li>
            </ul>
          </div>
          <div class="compare-card">
            <h3>Comment</h3>
            <ul>
              <li v-for="diff in compareResult.commentDiffs" :key="`comment-${diff.entityId}-${diff.changeType}`">{{ diff.changeType }}: {{ diff.label }}</li>
              <li v-if="compareResult.commentDiffs.length === 0">差分なし</li>
            </ul>
          </div>
        </div>
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

.compare-controls {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}

.compare-controls select {
  min-width: 180px;
  padding: 8px 10px;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
}

.compare-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 12px;
}

.compare-card {
  padding: 12px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
}

.compare-card h3 {
  margin: 0 0 8px;
}

.compare-card ul {
  margin: 0;
  padding-left: 18px;
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

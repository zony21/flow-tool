<script setup lang="ts">
import { computed, ref } from 'vue'
import type { FlowComment, FlowCommentKind, FlowDetail } from '../../types/flow'

const props = defineProps<{
  flow: FlowDetail
  selectedNodeId?: string | null
  readonly?: boolean
}>()

const emit = defineEmits<{
  (event: 'add-comment', payload: { nodeId?: string | null; text: string }): void
  (event: 'update-comment', payload: FlowComment): void
  (event: 'delete-comment', payload: { commentId: string }): void
}>()

const commentKinds: { value: FlowCommentKind; label: string }[] = [
  { value: 'note', label: '補足' },
  { value: 'warning', label: '注意' },
  { value: 'todo', label: '未確定' },
  { value: 'exception', label: '例外' },
  { value: 'api', label: 'API' },
]

const targetNodeId = ref<string | null>(props.selectedNodeId ?? null)
const commentKind = ref<FlowCommentKind>('note')
const commentText = ref('')

const nodes = computed(() => props.flow.nodes.slice().sort((a, b) => a.y - b.y || a.x - b.x || a.name.localeCompare(b.name)))
const comments = computed(() => props.flow.comments.slice().sort((a, b) => a.y - b.y || a.x - b.x || a.text.localeCompare(b.text)))

function parseComment(text: string): { kind: FlowCommentKind; body: string } {
  const matched = text.match(/^\[(補足|注意|未確定|例外|API)]\s*(.*)$/)
  if (!matched) {
    return { kind: 'note', body: text }
  }

  const label = matched[1]
  const kind = commentKinds.find((item) => item.label === label)?.value ?? 'note'
  return { kind, body: matched[2] ?? '' }
}

function buildComment(kind: FlowCommentKind, body: string): string {
  const label = commentKinds.find((item) => item.value === kind)?.label ?? '補足'
  return `[${label}] ${body.trim()}`
}

function nodeName(nodeId?: string | null): string {
  if (!nodeId) return 'フロー全体'
  return props.flow.nodes.find((node) => node.nodeId === nodeId)?.name ?? '不明なノード'
}

function addComment(): void {
  if (props.readonly || !commentText.value.trim()) return
  emit('add-comment', {
    nodeId: targetNodeId.value || null,
    text: buildComment(commentKind.value, commentText.value),
  })
  commentText.value = ''
}

function updateCommentKind(comment: FlowComment, kind: FlowCommentKind): void {
  if (props.readonly) return
  const parsed = parseComment(comment.text)
  emit('update-comment', {
    ...comment,
    text: buildComment(kind, parsed.body),
  })
}

function updateCommentBody(comment: FlowComment, body: string): void {
  if (props.readonly) return
  const parsed = parseComment(comment.text)
  emit('update-comment', {
    ...comment,
    text: buildComment(parsed.kind, body),
  })
}
</script>

<template>
  <section class="comment-panel">
    <div class="comment-form">
      <label class="field">
        <span>対象</span>
        <select v-model="targetNodeId" :disabled="readonly">
          <option :value="null">フロー全体</option>
          <option v-for="node in nodes" :key="node.nodeId" :value="node.nodeId">
            {{ node.name }}
          </option>
        </select>
      </label>
      <label class="field">
        <span>種別</span>
        <select v-model="commentKind" :disabled="readonly">
          <option v-for="kind in commentKinds" :key="kind.value" :value="kind.value">
            {{ kind.label }}
          </option>
        </select>
      </label>
      <label class="field full">
        <span>コメント</span>
        <textarea v-model="commentText" rows="3" :disabled="readonly" placeholder="図では表しきれない補足・注意・未確定事項など" />
      </label>
      <button type="button" class="add-button" :disabled="readonly || !commentText.trim()" @click="addComment">
        コメント追加
      </button>
    </div>

    <div class="comment-list">
      <p v-if="comments.length === 0" class="empty-text">コメントはありません。</p>
      <article v-for="comment in comments" :key="comment.commentId" class="comment-item">
        <div class="comment-meta">
          <strong>{{ nodeName(comment.nodeId) }}</strong>
          <button type="button" class="delete-button" :disabled="readonly" @click="emit('delete-comment', { commentId: comment.commentId })">
            削除
          </button>
        </div>
        <div class="comment-edit-row">
          <select :value="parseComment(comment.text).kind" :disabled="readonly" @change="updateCommentKind(comment, ($event.target as HTMLSelectElement).value as FlowCommentKind)">
            <option v-for="kind in commentKinds" :key="kind.value" :value="kind.value">
              {{ kind.label }}
            </option>
          </select>
          <textarea
            rows="2"
            :value="parseComment(comment.text).body"
            :disabled="readonly"
            @input="updateCommentBody(comment, ($event.target as HTMLTextAreaElement).value)"
          />
        </div>
      </article>
    </div>
  </section>
</template>

<style scoped>
.comment-panel {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.comment-form {
  display: grid;
  grid-template-columns: 1fr 160px;
  gap: 12px;
  padding: 12px;
  background: #f8fafc;
  border: 1px solid #dbe3ef;
  border-radius: 10px;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 6px;
  color: #334155;
  font-size: 13px;
  font-weight: 700;
}

.field.full {
  grid-column: 1 / -1;
}

.field select,
.field textarea,
.comment-edit-row select,
.comment-edit-row textarea {
  width: 100%;
  box-sizing: border-box;
  padding: 8px 10px;
  color: #0f172a;
  background: #fff;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  font: inherit;
  font-weight: 400;
}

.add-button {
  grid-column: 1 / -1;
  padding: 10px 12px;
  color: #fff;
  background: #0f766e;
  border: none;
  border-radius: 8px;
  font-weight: 700;
  cursor: pointer;
}

.add-button:disabled,
.delete-button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.comment-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
  max-height: 46vh;
  overflow: auto;
}

.empty-text {
  margin: 0;
  color: #64748b;
}

.comment-item {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 12px;
  background: #ffffff;
  border: 1px solid #dbe3ef;
  border-radius: 10px;
}

.comment-meta {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}

.comment-meta strong {
  overflow-wrap: anywhere;
  color: #0f172a;
}

.delete-button {
  padding: 6px 10px;
  color: #991b1b;
  background: #fee2e2;
  border: 1px solid #fca5a5;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 700;
}

.comment-edit-row {
  display: grid;
  grid-template-columns: 140px 1fr;
  gap: 8px;
}
</style>

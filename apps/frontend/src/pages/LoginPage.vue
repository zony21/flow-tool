<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import AuthLayout from '../layouts/AuthLayout.vue'
import { normalizeApiError } from '../api/apiError'
import { useAuthStore } from '../stores/authStore'

const router = useRouter()
const authStore = useAuthStore()
const loggingIn = ref(false)
const errorMessage = ref<string | null>(null)

async function handleLogin(): Promise<void> {
  loggingIn.value = true
  errorMessage.value = null
  try {
    await authStore.login()
    await router.push({ name: 'project-list' })
  } catch (error) {
    errorMessage.value = normalizeApiError(error).message
  } finally {
    loggingIn.value = false
  }
}
</script>

<template>
  <AuthLayout>
    <h1>ログイン</h1>
    <p>開発環境ではデモログインを使います。</p>
    <p v-if="errorMessage" class="error-message">{{ errorMessage }}</p>
    <button type="button" :disabled="loggingIn" @click="handleLogin">
      {{ loggingIn ? 'ログイン中...' : 'デモログイン' }}
    </button>
  </AuthLayout>
</template>

<style scoped>
.error-message {
  padding: 10px 12px;
  color: #991b1b;
  background: #fee2e2;
  border: 1px solid #fecaca;
  border-radius: 8px;
}
</style>

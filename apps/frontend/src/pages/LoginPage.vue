<script setup lang="ts">
import { useRouter } from 'vue-router'
import AuthLayout from '../layouts/AuthLayout.vue'
import { useAuthStore } from '../stores/authStore'

const router = useRouter()
const authStore = useAuthStore()

async function handleLogin(): Promise<void> {
  await authStore.login()
  await router.push('/')
}
</script>

<template>
  <AuthLayout>
    <h1>ログイン</h1>
    <p>開発環境ではデモログインを使います。</p>
    <button type="button" @click="handleLogin">デモログイン</button>
  </AuthLayout>
</template>

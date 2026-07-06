<script setup lang="ts">
import { useRoute } from 'vue-router'
import MainLayout from '../layouts/MainLayout.vue'

const route = useRoute()
</script>

<template>
  <MainLayout>
    <section class="page">
      <div class="card">
        <h1>プロジェクト詳細</h1>
        <p>projectId: {{ route.params.projectId }}</p>
      </div>
    </section>
  </MainLayout>
</template>

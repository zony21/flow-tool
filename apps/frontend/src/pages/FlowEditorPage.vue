<script setup lang="ts">
import { useRoute } from 'vue-router'
import MainLayout from '../layouts/MainLayout.vue'
import EditorLayout from '../layouts/EditorLayout.vue'

const route = useRoute()
</script>

<template>
  <MainLayout>
    <EditorLayout>
      <h1>フローエディタ</h1>
      <p>projectId: {{ route.params.projectId }}</p>
      <p>flowId: {{ route.params.flowId }}</p>
    </EditorLayout>
  </MainLayout>
</template>

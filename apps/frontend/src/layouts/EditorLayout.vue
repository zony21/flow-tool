<template>
  <div class="editor-layout">
    <aside class="left">Node Palette</aside>
    <section class="center"><slot /></section>
    <aside class="right">Property Panel</aside>
  </div>
</template>

<style scoped>
.editor-layout {
  display: grid;
  grid-template-columns: 240px 1fr 320px;
  gap: 12px;
  min-height: calc(100vh - 52px);
  padding: 12px;
}

.left,
.right,
.center {
  background: #ffffff;
  border: 1px solid #d1d5db;
  border-radius: 12px;
  padding: 12px;
}

@media (max-width: 960px) {
  .editor-layout {
    grid-template-columns: 1fr;
  }
}
</style>

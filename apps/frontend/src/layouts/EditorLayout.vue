<template>
  <div class="editor-layout">
    <slot />
  </div>
</template>

<style scoped>
.editor-layout {
  width: 100%;
  min-width: 0;
  max-width: none;
  min-height: calc(100vh - 52px);
  padding: 12px;
  box-sizing: border-box;
}
</style>

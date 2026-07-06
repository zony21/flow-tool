<template>
  <div class="layout-main">
    <header class="header">AI Flow Designer</header>
    <main>
      <slot />
    </main>
  </div>
</template>

<style scoped>
.layout-main {
  min-height: 100vh;
}

.header {
  padding: 14px 24px;
  background: #0f172a;
  color: #f8fafc;
  font-weight: 600;
}
</style>

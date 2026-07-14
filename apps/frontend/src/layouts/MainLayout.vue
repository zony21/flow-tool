<template>
  <div class="layout-main">
    <header class="header"><span>AI Flow Designer</span><RouterLink to="/projects">Project</RouterLink><RouterLink to="/settings/transport/manufacturers">AGF・AGVマスタ</RouterLink></header>
    <main class="main-content">
      <slot />
    </main>
  </div>
</template>

<style scoped>
.layout-main {
  display: flex;
  flex-direction: column;
  width: 100%;
  min-width: 0;
  min-height: 100vh;
}

.header {
  flex: 0 0 auto;
  padding: 14px 24px;
  background: #0f172a;
  color: #f8fafc;
  font-weight: 600;
  display: flex;
  gap: 20px;
}
.header a { color: #f8fafc; }

.main-content {
  flex: 1 1 auto;
  width: 100%;
  min-width: 0;
  max-width: none;
}
</style>

<template>
  <main class="auth-layout">
    <section class="auth-card">
      <slot />
    </section>
  </main>
</template>

<style scoped>
.auth-layout {
  min-height: 100vh;
  display: grid;
  place-items: center;
  padding: 16px;
}

.auth-card {
  width: 100%;
  max-width: 420px;
  background: #ffffff;
  border: 1px solid #d1d5db;
  border-radius: 12px;
  padding: 20px;
}
</style>

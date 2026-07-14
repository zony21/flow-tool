<template>
  <div class="layout-main">
    <header class="app-header">
      <RouterLink class="app-brand" to="/projects">
        <span class="app-brand__mark"><i class="pi pi-sitemap" /></span>
        <span>
          <strong>AI Flow Designer</strong>
          <small>Structured Flow Workspace</small>
        </span>
      </RouterLink>

      <nav class="app-navigation" aria-label="メインナビゲーション">
        <RouterLink to="/projects">
          <i class="pi pi-folder" />
          <span>プロジェクト</span>
        </RouterLink>
        <RouterLink to="/settings/transport/manufacturers">
          <i class="pi pi-truck" />
          <span>AGF・AGVマスタ</span>
        </RouterLink>
      </nav>

      <div class="app-header__meta">
        <span class="app-environment">Development</span>
      </div>
    </header>

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
  background: #f4f7fb;
}

.app-header {
  position: sticky;
  z-index: 50;
  top: 0;
  display: grid;
  grid-template-columns: minmax(230px, auto) minmax(0, 1fr) auto;
  align-items: center;
  gap: 28px;
  min-height: 66px;
  padding: 0 24px;
  color: #e2e8f0;
  background: rgb(15 23 42 / 97%);
  border-bottom: 1px solid rgb(148 163 184 / 18%);
  box-shadow: 0 8px 24px rgb(15 23 42 / 18%);
  backdrop-filter: blur(12px);
}

.app-brand {
  display: flex;
  align-items: center;
  gap: 11px;
  color: #f8fafc;
  text-decoration: none;
}

.app-brand__mark {
  display: grid;
  place-items: center;
  width: 38px;
  height: 38px;
  color: #bfdbfe;
  background: linear-gradient(145deg, #2563eb, #1d4ed8);
  border: 1px solid rgb(147 197 253 / 35%);
  border-radius: 11px;
  box-shadow: 0 6px 16px rgb(37 99 235 / 28%);
}

.app-brand > span:last-child {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.app-brand strong {
  font-size: 14px;
  letter-spacing: -.01em;
}

.app-brand small {
  color: #94a3b8;
  font-size: 9px;
  font-weight: 600;
  letter-spacing: .07em;
  text-transform: uppercase;
}

.app-navigation {
  display: flex;
  align-items: stretch;
  align-self: stretch;
  gap: 4px;
}

.app-navigation a {
  position: relative;
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 0 15px;
  color: #94a3b8;
  font-size: 12px;
  font-weight: 700;
  text-decoration: none;
  transition: color .15s ease, background .15s ease;
}

.app-navigation a:hover {
  color: #e2e8f0;
  background: rgb(255 255 255 / 4%);
}

.app-navigation a.router-link-active {
  color: #fff;
  background: rgb(37 99 235 / 13%);
}

.app-navigation a.router-link-active::after {
  position: absolute;
  right: 14px;
  bottom: 0;
  left: 14px;
  height: 3px;
  background: #60a5fa;
  border-radius: 999px 999px 0 0;
  content: '';
}

.app-header__meta {
  display: flex;
  align-items: center;
  justify-content: flex-end;
}

.app-environment {
  padding: 5px 9px;
  color: #93c5fd;
  background: rgb(37 99 235 / 14%);
  border: 1px solid rgb(96 165 250 / 22%);
  border-radius: 999px;
  font-size: 9px;
  font-weight: 800;
  letter-spacing: .08em;
  text-transform: uppercase;
}

.main-content {
  flex: 1 1 auto;
  width: 100%;
  min-width: 0;
}

@media (max-width: 720px) {
  .app-header {
    grid-template-columns: 1fr auto;
    gap: 10px;
    padding: 0 14px;
  }

  .app-brand small,
  .app-header__meta {
    display: none;
  }

  .app-navigation {
    justify-content: flex-end;
  }

  .app-navigation a {
    padding: 0 10px;
  }

  .app-navigation a span {
    display: none;
  }
}
</style>

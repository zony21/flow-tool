import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/authStore'
import { useProjectPermissionStore } from '../stores/projectPermissionStore'
import { PermissionCodes } from '../types/permission'
import ProjectListPage from '../pages/ProjectListPage.vue'
import ProjectDetailPage from '../pages/ProjectDetailPage.vue'
import FlowEditorPage from '../pages/FlowEditorPage.vue'
import VersionManagementPage from '../pages/VersionManagementPage.vue'
import SettingsPage from '../pages/SettingsPage.vue'
import TransportVehicleTypesPage from '../pages/TransportVehicleTypesPage.vue'
import TransportCommandsPage from '../pages/TransportCommandsPage.vue'
import LoginPage from '../pages/LoginPage.vue'
import ForbiddenPage from '../pages/ForbiddenPage.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', redirect: { name: 'project-list' } },
    { path: '/projects', name: 'project-list', component: ProjectListPage, meta: { requiresAuth: true } },
    { path: '/projects/:projectId', name: 'project-detail', component: ProjectDetailPage, meta: { requiresAuth: true, permission: PermissionCodes.ProjectRead } },
    { path: '/projects/:projectId/flows', name: 'flow-list', component: ProjectDetailPage, meta: { requiresAuth: true, permission: PermissionCodes.FlowRead } },
    { path: '/projects/:projectId/flows/:flowId/editor', name: 'flow-editor', component: FlowEditorPage, meta: { requiresAuth: true, permission: PermissionCodes.FlowRead } },
    { path: '/projects/:projectId/flows/:flowId/versions', name: 'flow-versions', component: VersionManagementPage, meta: { requiresAuth: true, permission: PermissionCodes.VersionRead } },
    { path: '/settings', redirect: { name: 'transport-manufacturers' } },
    { path: '/settings/transport/manufacturers', name: 'transport-manufacturers', component: SettingsPage, meta: { requiresAuth: true } },
    { path: '/settings/transport/manufacturers/:manufacturerId/vehicle-types', name: 'transport-vehicle-types', component: TransportVehicleTypesPage, meta: { requiresAuth: true } },
    { path: '/settings/transport/manufacturers/:manufacturerId/vehicle-types/:typeId/commands', name: 'transport-commands', component: TransportCommandsPage, meta: { requiresAuth: true } },
    { path: '/login', name: 'login', component: LoginPage },
    { path: '/forbidden', name: 'forbidden', component: ForbiddenPage },
  ],
})

router.beforeEach(async (to) => {
  const authStore = useAuthStore()
  if (authStore.currentUser === null && !authStore.loading) {
    await authStore.bootstrap()
  }

  if (to.meta.requiresAuth && authStore.currentUser === null) {
    return { name: 'login' }
  }

  const permissionCode = to.meta.permission as string | undefined
  const projectId = to.params.projectId as string | undefined
  if (permissionCode && projectId) {
    const projectPermissionStore = useProjectPermissionStore()
    const permission = await import('../api/permissionApi').then((module) => module.fetchProjectPermission(projectId))
    projectPermissionStore.setPermission(permission)

    if (!projectPermissionStore.can(permissionCode)) {
      return { name: 'forbidden' }
    }
  }

  return true
})

export default router

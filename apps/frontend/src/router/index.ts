import { createRouter, createWebHistory } from 'vue-router'
import ProjectListPage from '../pages/ProjectListPage.vue'
import ProjectDetailPage from '../pages/ProjectDetailPage.vue'
import FlowEditorPage from '../pages/FlowEditorPage.vue'
import SettingsPage from '../pages/SettingsPage.vue'
import LoginPage from '../pages/LoginPage.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', name: 'project-list', component: ProjectListPage },
    { path: '/projects/:projectId', name: 'project-detail', component: ProjectDetailPage },
    { path: '/flows/:flowId/editor', name: 'flow-editor', component: FlowEditorPage },
    { path: '/settings', name: 'settings', component: SettingsPage },
    { path: '/login', name: 'login', component: LoginPage },
  ],
})

export default router

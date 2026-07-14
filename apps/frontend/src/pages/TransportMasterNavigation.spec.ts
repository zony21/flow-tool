import { flushPromises, mount } from '@vue/test-utils'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import SettingsPage from './SettingsPage.vue'

const push = vi.fn()
vi.mock('vue-router', () => ({ useRouter: () => ({ push }) }))
const api = vi.hoisted(() => ({
  fetchTransportManufacturers: vi.fn(), createTransportManufacturer: vi.fn(),
  updateTransportManufacturer: vi.fn(), deleteTransportManufacturer: vi.fn(),
}))
vi.mock('../api/transportApi', () => api)
const Button = { props: ['label'], emits: ['click'], template: '<button @click="$emit(\'click\')">{{label}}</button>' }
const Dialog = { props: ['visible'], template: '<div v-if="visible"><slot/><slot name="footer"/></div>' }

describe('Transportマスタ導線', () => {
  beforeEach(() => {
    push.mockReset()
    api.fetchTransportManufacturers.mockResolvedValue([{ manufacturerId: 'maker-1', name: '三菱', description: null, sortOrder: 1, isActive: true, createdAtUtc: '', updatedAtUtc: '' }])
  })
  it('最初の画面にはメーカー一覧と追加ボタンだけを表示する', async () => {
    const wrapper = mount(SettingsPage, { global: { stubs: { MainLayout: { template: '<main><slot/></main>' }, Button, Dialog } } }); await flushPromises()
    expect(wrapper.text()).toContain('メーカー一覧'); expect(wrapper.text()).toContain('メーカーを追加')
    expect(wrapper.text()).not.toContain('コマンド一覧'); expect(wrapper.text()).not.toContain('AGF・AGVを追加')
  })
  it('メーカー押下でそのメーカーのAGF・AGV一覧へ遷移する', async () => {
    const wrapper = mount(SettingsPage, { global: { stubs: { MainLayout: { template: '<main><slot/></main>' }, Button, Dialog } } }); await flushPromises()
    await wrapper.find('.master-row').trigger('click')
    expect(push).toHaveBeenCalledWith({ name: 'transport-vehicle-types', params: { manufacturerId: 'maker-1' } })
  })
})

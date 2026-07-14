import { flushPromises, mount } from '@vue/test-utils'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import NodePropertyPanel from './NodePropertyPanel.vue'
import type { FlowDetail } from '../../types/flow'

const transportApi = vi.hoisted(() => ({
  fetchTransportCommands: vi.fn(),
  fetchTransportEquipments: vi.fn(),
  fetchTransportLocations: vi.fn(),
  fetchTransportManufacturerVehicleTypes: vi.fn(),
}))

vi.mock('../../api/transportApi', () => transportApi)

function createFlow(flowType: 'NORMAL' | 'TRANSPORT'): FlowDetail {
  return {
    flowId: 'flow-1',
    projectId: 'project-1',
    name: 'Flow',
    flowType,
    description: null,
    sortOrder: 1,
    createdAtUtc: '',
    updatedAtUtc: '',
    currentRevision: 1,
    lanes: [],
    stages: [],
    nodes: [{ nodeId: 'node-1', flowId: 'flow-1', laneId: null, stageId: null, nodeType: 'Task', name: 'Node', x: 0, y: 0, locationId: null }],
    links: [],
    comments: [],
    metadata: [],
  }
}

describe('NodePropertyPanel Location selector', () => {
  beforeEach(() => {
    transportApi.fetchTransportCommands.mockResolvedValue([])
    transportApi.fetchTransportEquipments.mockResolvedValue([])
    transportApi.fetchTransportLocations.mockResolvedValue([{
      locationId: 'location-1', projectId: 'project-1', name: 'P1', locationType: '経由点', sortOrder: 1, isDeleted: false, createdAtUtc: '', updatedAtUtc: '',
    }])
    transportApi.fetchTransportManufacturerVehicleTypes.mockResolvedValue([])
  })

  it('Transport FlowだけにLocation selectorを表示する', async () => {
    const transport = mount(NodePropertyPanel, { props: { flow: createFlow('TRANSPORT'), nodeId: 'node-1' } })
    await flushPromises()
    expect(transport.text()).toContain('ロケーション')

    const normal = mount(NodePropertyPanel, { props: { flow: createFlow('NORMAL'), nodeId: 'node-1' } })
    await flushPromises()
    expect(normal.text()).not.toContain('ロケーション')
  })

  it('Location選択時に更新後のlocationIdをemitする', async () => {
    const wrapper = mount(NodePropertyPanel, { props: { flow: createFlow('TRANSPORT'), nodeId: 'node-1' } })
    await flushPromises()
    const locationField = wrapper.findAll('label').find((label) => label.text().includes('ロケーション'))
    expect(locationField).toBeDefined()
    await locationField!.find('select').setValue('location-1')
    const updatedNode = wrapper.emitted('update-node')?.[0]?.[0] as { locationId?: string }
    expect(updatedNode.locationId).toBe('location-1')
  })
})

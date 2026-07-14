import { httpClient } from './httpClient'
import type {
  SaveTransportCommandRequest,
  SaveTransportEquipmentRequest,
  SaveTransportLocationRequest,
  SaveTransportManufacturerRequest,
  SaveTransportVehicleModelRequest,
  TransportCommand,
  TransportEquipment,
  TransportLocation,
  TransportManufacturer,
  TransportVehicleModel,
} from '../types/transport'

export async function fetchTransportVehicleModels(params?: { manufacturerId?: string; vehicleType?: 'AGF' | 'AGV'; includeInactive?: boolean }): Promise<TransportVehicleModel[]> {
  return (await httpClient.get<TransportVehicleModel[]>('/api/transport/vehicle-models', { params })).data
}

export async function createTransportVehicleModel(request: SaveTransportVehicleModelRequest): Promise<TransportVehicleModel> {
  return (await httpClient.post<TransportVehicleModel>('/api/transport/vehicle-models', request)).data
}

export async function updateTransportVehicleModel(id: string, request: SaveTransportVehicleModelRequest): Promise<TransportVehicleModel> {
  return (await httpClient.put<TransportVehicleModel>(`/api/transport/vehicle-models/${id}`, request)).data
}

export async function deleteTransportVehicleModel(id: string): Promise<void> {
  await httpClient.delete(`/api/transport/vehicle-models/${id}`)
}

export async function fetchTransportManufacturers(): Promise<TransportManufacturer[]> {
  const response = await httpClient.get<TransportManufacturer[]>('/api/transport/manufacturers')
  return response.data
}

export async function createTransportManufacturer(request: SaveTransportManufacturerRequest): Promise<TransportManufacturer> {
  const response = await httpClient.post<TransportManufacturer>('/api/transport/manufacturers', request)
  return response.data
}

export async function updateTransportManufacturer(manufacturerId: string, request: SaveTransportManufacturerRequest): Promise<TransportManufacturer> {
  const response = await httpClient.put<TransportManufacturer>(`/api/transport/manufacturers/${manufacturerId}`, request)
  return response.data
}

export async function deleteTransportManufacturer(manufacturerId: string): Promise<void> {
  await httpClient.delete(`/api/transport/manufacturers/${manufacturerId}`)
}

export async function fetchTransportCommands(manufacturerId?: string | null): Promise<TransportCommand[]> {
  const response = await httpClient.get<TransportCommand[]>('/api/transport/commands', {
    params: manufacturerId ? { manufacturerId } : undefined,
  })
  return response.data
}

export async function createTransportCommand(request: SaveTransportCommandRequest): Promise<TransportCommand> {
  const response = await httpClient.post<TransportCommand>('/api/transport/commands', request)
  return response.data
}

export async function updateTransportCommand(commandId: string, request: SaveTransportCommandRequest): Promise<TransportCommand> {
  const response = await httpClient.put<TransportCommand>(`/api/transport/commands/${commandId}`, request)
  return response.data
}

export async function deleteTransportCommand(commandId: string): Promise<void> {
  await httpClient.delete(`/api/transport/commands/${commandId}`)
}

export async function fetchTransportLocations(projectId: string): Promise<TransportLocation[]> {
  const response = await httpClient.get<TransportLocation[]>(`/api/projects/${projectId}/transport/locations`)
  return response.data
}

export async function createTransportLocation(projectId: string, request: SaveTransportLocationRequest): Promise<TransportLocation> {
  const response = await httpClient.post<TransportLocation>(`/api/projects/${projectId}/transport/locations`, request)
  return response.data
}

export async function updateTransportLocation(projectId: string, locationId: string, request: SaveTransportLocationRequest): Promise<TransportLocation> {
  const response = await httpClient.put<TransportLocation>(`/api/projects/${projectId}/transport/locations/${locationId}`, request)
  return response.data
}

export async function deleteTransportLocation(projectId: string, locationId: string): Promise<void> {
  await httpClient.delete(`/api/projects/${projectId}/transport/locations/${locationId}`)
}

export async function fetchTransportEquipments(projectId: string): Promise<TransportEquipment[]> {
  const response = await httpClient.get<TransportEquipment[]>(`/api/projects/${projectId}/transport/equipments`)
  return response.data
}

export async function createTransportEquipment(projectId: string, request: SaveTransportEquipmentRequest): Promise<TransportEquipment> {
  const response = await httpClient.post<TransportEquipment>(`/api/projects/${projectId}/transport/equipments`, request)
  return response.data
}

export async function updateTransportEquipment(projectId: string, equipmentId: string, request: SaveTransportEquipmentRequest): Promise<TransportEquipment> {
  const response = await httpClient.put<TransportEquipment>(`/api/projects/${projectId}/transport/equipments/${equipmentId}`, request)
  return response.data
}

export async function deleteTransportEquipment(projectId: string, equipmentId: string): Promise<void> {
  await httpClient.delete(`/api/projects/${projectId}/transport/equipments/${equipmentId}`)
}

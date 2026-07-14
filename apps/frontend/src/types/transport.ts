export type VehicleType = 'AGF' | 'AGV'

export type TransportProcessType = '移動' | '荷上げ' | '荷下ろし' | '待機' | '充電' | 'その他'

export type TransportLocationType = '経由点' | '荷役' | '充電' | '待機' | 'その他'

export type TransportEquipmentCategory =
  | 'PLC'
  | 'RCS'
  | 'WCS'
  | 'AGF'
  | 'AGV'
  | 'AMR'
  | 'コンベア'
  | 'シャッター'
  | 'ロボット'
  | 'センサー'
  | '安全機器'
  | 'その他'

export type RwType = 'NONE' | 'READ' | 'WRITE'

export type TransportManufacturer = {
  manufacturerId: string
  name: string
  description?: string | null
  sortOrder: number
  isActive: boolean
  createdAtUtc: string
  updatedAtUtc: string
}

export type TransportManufacturerVehicleType = {
  manufacturerVehicleTypeId: string
  manufacturerId: string
  manufacturerName: string
  vehicleType: VehicleType
  description?: string | null
  sortOrder: number
  isActive: boolean
  createdAtUtc: string
  updatedAtUtc: string
}

export type TransportCommand = {
  commandId: string
  manufacturerVehicleTypeId: string
  manufacturerId: string
  manufacturerName: string
  vehicleType: VehicleType
  commandCode: string
  commandName: string
  processType: TransportProcessType | string
  description?: string | null
  sortOrder: number
  isActive: boolean
  createdAtUtc: string
  updatedAtUtc: string
}

export type TransportLocation = {
  locationId: string
  projectId: string
  name: string
  locationType: TransportLocationType | string
  description?: string | null
  sortOrder: number
  createdAtUtc: string
  updatedAtUtc: string
}

export type TransportEquipment = {
  equipmentId: string
  projectId: string
  name: string
  category: TransportEquipmentCategory | string
  description?: string | null
  sortOrder: number
  createdAtUtc: string
  updatedAtUtc: string
}

export type SaveTransportManufacturerVehicleTypeRequest = {
  vehicleType: VehicleType
  description?: string | null
  sortOrder?: number | null
  isActive: boolean
}

export type SaveTransportManufacturerRequest = {
  name: string
  description?: string | null
  sortOrder?: number | null
  isActive: boolean
}

export type SaveTransportCommandRequest = {
  commandCode: string
  commandName: string
  processType: TransportProcessType | string
  description?: string | null
  sortOrder?: number | null
  isActive: boolean
}

export type SaveTransportLocationRequest = {
  name: string
  locationType: TransportLocationType | string
  description?: string | null
  sortOrder?: number | null
}

export type SaveTransportEquipmentRequest = {
  name: string
  category: TransportEquipmentCategory | string
  description?: string | null
  sortOrder?: number | null
}

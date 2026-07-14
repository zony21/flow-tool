# 05_11 Transport Settings API Design

## 1. Purpose

Transport Settings APIs provide global AGF / AGV command master data and Project-level facility data used by Transport Flows.

Ownership is separated as follows:

```text
Global Transport Master
├── Manufacturer
├── Manufacturer Vehicle Type
└── Command

Project-level Transport Data
├── Location
└── Equipment
```

The global command hierarchy is:

```text
Manufacturer
├── AGF
│    └── Commands
└── AGV
     └── Commands
```

A Manufacturer may support AGF, AGV, or both.

## 2. Global Manufacturer API

```http
GET    /api/transport/manufacturers
POST   /api/transport/manufacturers
PUT    /api/transport/manufacturers/{manufacturerId}
DELETE /api/transport/manufacturers/{manufacturerId}
```

### Manufacturer DTO

```text
ManufacturerId
Name
Description
SortOrder
IsActive
CreatedAtUtc
UpdatedAtUtc
```

The Manufacturer DTO does not contain one fixed `VehicleType`.

Create and update validate:

```text
Name is required
Name is unique according to the existing master policy
IsActive is valid
```

Delete uses soft deletion and is rejected while dependent Vehicle Types, Commands, or Flow assignments exist.

## 3. Manufacturer Vehicle Type API

```http
GET    /api/transport/manufacturers/{manufacturerId}/vehicle-types
POST   /api/transport/manufacturers/{manufacturerId}/vehicle-types
PUT    /api/transport/manufacturers/{manufacturerId}/vehicle-types/{manufacturerVehicleTypeId}
DELETE /api/transport/manufacturers/{manufacturerId}/vehicle-types/{manufacturerVehicleTypeId}
```

### Manufacturer Vehicle Type DTO

```text
ManufacturerVehicleTypeId
ManufacturerId
ManufacturerName
VehicleType
Description
SortOrder
IsActive
CreatedAtUtc
UpdatedAtUtc
```

Allowed values:

```text
AGF
AGV
```

Create and update validate:

```text
Manufacturer exists and is not deleted
VehicleType is AGF or AGV
Manufacturer + VehicleType is not duplicated among non-deleted records
```

Delete uses soft deletion and is rejected while Commands or Flow Action Unit assignments reference the record.

## 4. Global Command API

```http
GET    /api/transport/manufacturer-vehicle-types/{manufacturerVehicleTypeId}/commands
POST   /api/transport/manufacturer-vehicle-types/{manufacturerVehicleTypeId}/commands
PUT    /api/transport/manufacturer-vehicle-types/{manufacturerVehicleTypeId}/commands/{commandId}
DELETE /api/transport/manufacturer-vehicle-types/{manufacturerVehicleTypeId}/commands/{commandId}
```

Optional global search may be provided:

```http
GET /api/transport/commands?manufacturerId={manufacturerId}&vehicleType=AGF
```

### Command DTO

```text
CommandId
ManufacturerVehicleTypeId
ManufacturerId
ManufacturerName
VehicleType
CommandCode
CommandName
ProcessType
Description
SortOrder
IsActive
CreatedAtUtc
UpdatedAtUtc
```

Create and update validate:

```text
Manufacturer Vehicle Type exists and is not deleted
CommandCode is required
CommandName is required
ProcessType is required
CommandCode is unique within the Manufacturer Vehicle Type
```

A Command is not Project-owned.

Delete uses soft deletion and is rejected while a Node references the Command.

## 5. Action Unit Assignment API

The left-side Flow unit currently displayed as `動作` may assign multiple Manufacturer Vehicle Types.

The actual structured entity corresponding to this UI unit must be confirmed from the implementation before finalizing the physical route name.

Logical operations required:

```http
GET    /api/projects/{projectId}/flows/{flowId}/action-units/{actionUnitId}/manufacturer-vehicle-types
PUT    /api/projects/{projectId}/flows/{flowId}/action-units/{actionUnitId}/manufacturer-vehicle-types
```

The PUT operation replaces the assignment set atomically.

### Assignment Request

```text
ManufacturerVehicleTypeIds: Guid[]
```

### Assignment Response

```text
ActionUnitId
Assignments[]
  ManufacturerVehicleTypeId
  ManufacturerId
  ManufacturerName
  VehicleType
  ActiveCommandCount
  IsActive
```

Validation:

```text
Action Unit belongs to the specified Flow
Flow belongs to the specified Project
all Manufacturer Vehicle Types exist and are not deleted
duplicate IDs are rejected or normalized consistently
inactive values cannot be newly assigned
existing inactive assignments remain loadable
```

If Action Unit assignments are included directly in the Flow Structure API instead of separate endpoints, the same validation and response information remain required.

## 6. Node Command Filtering

The Node Command selector must use the Manufacturer Vehicle Types assigned to the Node's Action Unit.

The available command set is:

```text
active, non-deleted Commands
whose ManufacturerVehicleTypeId is assigned to the Action Unit
```

Recommended option display:

```text
Manufacturer A / AGF / TravelToPosture
Manufacturer B / AGV / MoveTask
```

If no Manufacturer Vehicle Type is assigned, the command list is empty and the UI displays guidance.

Flow Structure save validation rejects a new Node Command reference that does not belong to the Action Unit's assigned Manufacturer Vehicle Types.

Existing inactive Command references remain loadable and visible with an inactive indication.

## 7. Project Location API

```http
GET    /api/projects/{projectId}/transport/locations
POST   /api/projects/{projectId}/transport/locations
PUT    /api/projects/{projectId}/transport/locations/{locationId}
DELETE /api/projects/{projectId}/transport/locations/{locationId}
```

Location is Project-level master data.

The list supplies the Node Location selector with:

```text
LocationId
ProjectId
Name
LocationType
Description
SortOrder
IsDeleted
CreatedAtUtc
UpdatedAtUtc
```

Soft-deleted Locations are excluded from normal lists.

Flow Structure validation rejects a Location that is missing, deleted, or belongs to another Project.

## 8. Project Equipment API

```http
GET    /api/projects/{projectId}/transport/equipments
POST   /api/projects/{projectId}/transport/equipments
PUT    /api/projects/{projectId}/transport/equipments/{equipmentId}
DELETE /api/projects/{projectId}/transport/equipments/{equipmentId}
```

Equipment remains Project-level facility or system data.

It is not the AGF / AGV Manufacturer Vehicle Type master.

Flow Structure validation rejects Equipment that is missing, deleted, or belongs to another Project.

## 9. Deactivation and Existing References

Inactive Manufacturer, Manufacturer Vehicle Type, or Command records:

```text
cannot be newly assigned or newly selected
remain loadable when already referenced
must be returned with enough status information for the UI to display an inactive indication
```

Deactivation must not silently clear Flow assignments or Node Command references.

## 10. Errors

Recommended responses:

```text
400: invalid request format or required field
404: Project, Flow, Action Unit, or master record not found
409: duplicate master value or delete blocked by references
422: invalid Flow assignment or Node reference
```

Use the application's existing error response structure.

## 11. Permissions

Global Manufacturer, Manufacturer Vehicle Type, and Command mutation requires the global Transport master administration permission available in the application.

Project Location, Equipment, and Flow Action Unit assignment require the corresponding Project Flow update permission.

If a dedicated global permission does not yet exist, the implementation must report the gap rather than treating Project ownership as the replacement design.

## 12. Snapshot and Duplication

Action Unit assignments must be preserved through:

```text
Flow save
Flow load
Version Snapshot
Flow duplication
JSON export
AI DSL export
```

Flow duplication retains global Manufacturer Vehicle Type IDs and remaps assignment records to duplicated Action Unit IDs.

## 13. Completion Condition

The APIs support the global Manufacturer > AGF / AGV > Command hierarchy, Project-level Location and Equipment remain separate, and one Flow Action Unit can assign multiple Manufacturer Vehicle Types that control the Node Command selection scope.
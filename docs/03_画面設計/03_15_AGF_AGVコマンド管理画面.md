# 03_15 AGF / AGV Command Master Management Screen

## 1. Purpose

This document defines the global AGF / AGV Command Master Management screen.

The screen manages the command APIs supported by each manufacturer and vehicle type.

The authoritative hierarchy is:

```text
Manufacturer
├── AGF
│    └── Commands
└── AGV
     └── Commands
```

A Manufacturer may support AGF, AGV, or both.

Vehicle models, physical vehicles, quantities, serial numbers, and Project ownership are not managed by this screen.

## 2. Ownership

Manufacturer, Manufacturer Vehicle Type, and Command are global Transport master data reusable across Projects.

```text
Global Transport Master
└── Manufacturer
     └── Manufacturer Vehicle Type
          └── Command
```

Location and facility Equipment remain Project-level master data and are outside this screen.

## 3. Screen Responsibilities

The screen provides:

- Manufacturer list and selection;
- AGF / AGV type management for the selected Manufacturer;
- Command list for the selected Manufacturer Vehicle Type;
- Manufacturer creation, editing, activation, deactivation, and deletion;
- AGF / AGV type creation, activation, deactivation, and deletion;
- Command creation, editing, activation, deactivation, and deletion;
- Process Type assignment to each Command.

## 4. Recommended Layout

Use a three-level master-detail layout.

```text
Left panel
  Manufacturer list

Center panel
  Vehicle Types for the selected Manufacturer
  - AGF
  - AGV

Right panel
  Commands for the selected Manufacturer Vehicle Type
```

A tree or equivalent three-column layout may be used when it preserves the same hierarchy.

## 5. Manufacturer Fields

| Field | Required | Description |
| --- | ---: | --- |
| Name | Yes | Manufacturer name |
| Description | No | Additional notes |
| Sort Order | No | Display order |
| Active | Yes | Whether the Manufacturer can be newly selected |

A Manufacturer does not contain one fixed Vehicle Type.

## 6. Manufacturer Vehicle Type Fields

| Field | Required | Description |
| --- | ---: | --- |
| Vehicle Type | Yes | `AGF` or `AGV` |
| Description | No | Additional notes for the Manufacturer and type combination |
| Sort Order | No | Display order |
| Active | Yes | Whether the combination can be newly assigned |

The same Manufacturer may contain both AGF and AGV.

The same active Vehicle Type must not be registered twice under one Manufacturer.

## 7. Command Fields

| Field | Required | Description |
| --- | ---: | --- |
| Command Code | Yes | API or manufacturer command identifier |
| Command Name | Yes | Human-readable command name |
| Process Type | Yes | Transport process classification |
| Description | No | Parameters, response notes, or other details |
| Sort Order | No | Display order |
| Active | Yes | Whether the Command can be newly selected |

Example Command Codes:

```text
TravelToPosture
Loading
Unloading
Pause
Resume
Cancel
GetStatus
```

Command Code must be unique within one Manufacturer Vehicle Type among non-deleted records.

## 8. Process Type

Initial Process Type values may include:

```text
MOVE
LOADING
UNLOADING
WAIT
CHARGE
STATUS
TASK_CONTROL
OTHER
```

Command-specific required-field validation is outside this screen implementation cycle.

## 9. Operations

### 9.1 Manufacturer

Support:

```text
Create
Edit
Activate
Deactivate
Delete
```

A Manufacturer cannot be deleted while Vehicle Types, Commands, or Flow assignments reference it.

### 9.2 Manufacturer Vehicle Type

Support:

```text
Add AGF
Add AGV
Edit description
Activate
Deactivate
Delete
```

A Manufacturer Vehicle Type cannot be deleted while Commands or Flow assignments reference it.

### 9.3 Command

Support:

```text
Create
Edit
Activate
Deactivate
Delete
```

A Command referenced by a Node cannot be deleted.

All deletion is soft deletion.

Deactivation must preserve existing references and display them with an inactive indication.

## 10. Flow Editor Relationship

The left-side unit currently displayed as `動作` may assign multiple Manufacturer Vehicle Types.

Example:

```text
Product Transport Action
├── Manufacturer A / AGF
├── Manufacturer B / AGF
└── Manufacturer C / AGV
```

The actual structured entity corresponding to the current `動作` UI must be confirmed from the implementation before adding the assignment table.

Node Command options are filtered by the Manufacturer Vehicle Types assigned to the Node's Action Unit.

Recommended Command option display:

```text
Manufacturer A / AGF / TravelToPosture
Manufacturer B / AGV / MoveTask
```

## 11. API

Use global Transport master APIs.

### Manufacturer

```http
GET    /api/transport/manufacturers
POST   /api/transport/manufacturers
PUT    /api/transport/manufacturers/{manufacturerId}
DELETE /api/transport/manufacturers/{manufacturerId}
```

### Manufacturer Vehicle Type

```http
GET    /api/transport/manufacturers/{manufacturerId}/vehicle-types
POST   /api/transport/manufacturers/{manufacturerId}/vehicle-types
PUT    /api/transport/manufacturers/{manufacturerId}/vehicle-types/{manufacturerVehicleTypeId}
DELETE /api/transport/manufacturers/{manufacturerId}/vehicle-types/{manufacturerVehicleTypeId}
```

### Command

```http
GET    /api/transport/manufacturer-vehicle-types/{manufacturerVehicleTypeId}/commands
POST   /api/transport/manufacturer-vehicle-types/{manufacturerVehicleTypeId}/commands
PUT    /api/transport/manufacturer-vehicle-types/{manufacturerVehicleTypeId}/commands/{commandId}
DELETE /api/transport/manufacturer-vehicle-types/{manufacturerVehicleTypeId}/commands/{commandId}
```

## 12. Test Conditions

Confirm that:

- one Manufacturer can register AGF and AGV;
- duplicate AGF or AGV under the same Manufacturer is rejected;
- Commands are registered under Manufacturer plus Vehicle Type;
- the same Command Code may exist under different Manufacturer Vehicle Types;
- duplicate Command Code under the same Manufacturer Vehicle Type is rejected;
- referenced Commands cannot be deleted;
- inactive existing references remain visible;
- the Flow Action Unit may assign multiple Manufacturer Vehicle Types;
- Node Command options contain only Commands belonging to assigned Manufacturer Vehicle Types.

## 13. Completion Condition

Manufacturer, AGF / AGV type, and Command API definitions are globally managed in the confirmed hierarchy and can later be assigned to Flow Action Units without Project-owned duplication.
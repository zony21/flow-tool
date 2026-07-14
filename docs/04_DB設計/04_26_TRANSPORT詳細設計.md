# 04_26 TRANSPORT Detailed Design

## 1. Purpose

This document defines the Transport master and Project Transport data used by AGF / AGV Transport Flows.

The design separates global reusable master data from Project-level facility data.

```text
Global Transport Master
├── TRANSPORT_MANUFACTURER
├── TRANSPORT_MANUFACTURER_VEHICLE_TYPE
└── TRANSPORT_COMMAND

Project-level Transport Data
├── TRANSPORT_LOCATION
└── TRANSPORT_EQUIPMENT
```

## 2. Authoritative Hierarchy

The AGF / AGV command hierarchy is:

```text
Manufacturer
├── AGF
│    └── Commands
└── AGV
     └── Commands
```

A Manufacturer may support AGF, AGV, or both.

A Command belongs to one Manufacturer Vehicle Type, not directly to a Project.

Vehicle models and physical vehicle instances are outside this design.

## 3. TRANSPORT_MANUFACTURER

### 3.1 Responsibility

Stores a global AGF / AGV manufacturer reusable across Projects.

A Manufacturer does not store one fixed Vehicle Type.

### 3.2 Columns

| Column | Type | NULL | Description |
| --- | --- | --- | --- |
| manufacturer_id | TEXT | NO | Primary key, GUID |
| name | TEXT | NO | Manufacturer name |
| description | TEXT | YES | Description |
| sort_order | INTEGER | NO | Display order |
| is_active | INTEGER | NO | Active status |
| is_deleted | INTEGER | NO | Soft-deletion flag |
| created_at_utc | TEXT | NO | Creation timestamp |
| created_by_user_id | TEXT | YES | Creator |
| updated_at_utc | TEXT | NO | Update timestamp |
| updated_by_user_id | TEXT | YES | Updater |
| deleted_at_utc | TEXT | YES | Deletion timestamp |
| deleted_by_user_id | TEXT | YES | Deleter |

The former Manufacturer `vehicle_type` column is obsolete after migration to `TRANSPORT_MANUFACTURER_VEHICLE_TYPE`.

## 4. TRANSPORT_MANUFACTURER_VEHICLE_TYPE

### 4.1 Responsibility

Stores one Vehicle Type supported by one Manufacturer.

Allowed values in this cycle:

```text
AGF
AGV
```

A Manufacturer may have both records.

### 4.2 Columns

| Column | Type | NULL | Description |
| --- | --- | --- | --- |
| manufacturer_vehicle_type_id | TEXT | NO | Primary key, GUID |
| manufacturer_id | TEXT | NO | TRANSPORT_MANUFACTURER reference |
| vehicle_type | TEXT | NO | `AGF` or `AGV` |
| description | TEXT | YES | Description for this combination |
| sort_order | INTEGER | NO | Display order |
| is_active | INTEGER | NO | Active status |
| is_deleted | INTEGER | NO | Soft-deletion flag |
| created_at_utc | TEXT | NO | Creation timestamp |
| created_by_user_id | TEXT | YES | Creator |
| updated_at_utc | TEXT | NO | Update timestamp |
| updated_by_user_id | TEXT | YES | Updater |
| deleted_at_utc | TEXT | YES | Deletion timestamp |
| deleted_by_user_id | TEXT | YES | Deleter |

### 4.3 Constraints

Unique among non-deleted records:

```text
manufacturer_id + vehicle_type
```

Delete behavior from Manufacturer must be `RESTRICT` or equivalent application-level protection.

## 5. TRANSPORT_COMMAND

### 5.1 Responsibility

Stores one command or API supported by one Manufacturer Vehicle Type.

The authoritative ownership path is:

```text
TRANSPORT_COMMAND
  -> TRANSPORT_MANUFACTURER_VEHICLE_TYPE
  -> TRANSPORT_MANUFACTURER
```

### 5.2 Columns

| Column | Type | NULL | Description |
| --- | --- | --- | --- |
| command_id | TEXT | NO | Primary key, GUID |
| manufacturer_vehicle_type_id | TEXT | NO | Manufacturer Vehicle Type reference |
| command_code | TEXT | NO | API or command identifier |
| command_name | TEXT | NO | Human-readable name |
| process_type | TEXT | NO | Transport process classification |
| description | TEXT | YES | Parameters, response notes, or description |
| sort_order | INTEGER | NO | Display order |
| is_active | INTEGER | NO | Active status |
| is_deleted | INTEGER | NO | Soft-deletion flag |
| created_at_utc | TEXT | NO | Creation timestamp |
| created_by_user_id | TEXT | YES | Creator |
| updated_at_utc | TEXT | NO | Update timestamp |
| updated_by_user_id | TEXT | YES | Updater |
| deleted_at_utc | TEXT | YES | Deletion timestamp |
| deleted_by_user_id | TEXT | YES | Deleter |

### 5.3 Constraints

Unique among non-deleted records:

```text
manufacturer_vehicle_type_id + command_code
```

The former direct `manufacturer_id` relationship is obsolete after migration.

## 6. Action Unit Assignment

The left-side Flow unit currently displayed as `動作` may assign multiple Manufacturer Vehicle Types.

The actual structured entity represented by this UI item must be confirmed from the implementation before naming the physical table.

The required logical relationship is:

```text
ACTION_UNIT N:M TRANSPORT_MANUFACTURER_VEHICLE_TYPE
```

Recommended assignment columns:

```text
assignment_id
action_unit_id
manufacturer_vehicle_type_id
sort_order
created_at_utc
created_by_user_id
```

Unique constraint:

```text
action_unit_id + manufacturer_vehicle_type_id
```

If the current Action Unit is implemented as `STAGE`, use a Stage assignment table and `stage_id`.

If it is implemented as another structured entity, use that authoritative entity instead of creating a duplicate Action entity.

## 7. TRANSPORT_LOCATION

### 7.1 Responsibility

Stores physical facility positions per Project.

A Transport Node references an existing Project Location through `Node.LocationId`.

### 7.2 Main Columns

```text
location_id
project_id
name
location_type
description
sort_order
is_deleted
audit columns
```

## 8. TRANSPORT_EQUIPMENT

### 8.1 Responsibility

Stores Project-level facility or system equipment such as PLC, RCS, WCS, conveyor, shutter, robot, or sensor.

It is not the AGF / AGV manufacturer command master.

### 8.2 Main Columns

```text
equipment_id
project_id
name
category
description
sort_order
is_deleted
audit columns
```

## 9. Relationships

```text
TRANSPORT_MANUFACTURER 1:N TRANSPORT_MANUFACTURER_VEHICLE_TYPE
TRANSPORT_MANUFACTURER_VEHICLE_TYPE 1:N TRANSPORT_COMMAND
ACTION_UNIT N:M TRANSPORT_MANUFACTURER_VEHICLE_TYPE
PROJECT 1:N TRANSPORT_LOCATION
PROJECT 1:N TRANSPORT_EQUIPMENT
NODE N:1 TRANSPORT_COMMAND (optional)
NODE N:1 TRANSPORT_LOCATION (optional)
NODE N:1 TRANSPORT_EQUIPMENT (optional)
```

## 10. Indexes

Recommended indexes:

```text
TRANSPORT_MANUFACTURER:
  is_deleted, is_active, sort_order

TRANSPORT_MANUFACTURER_VEHICLE_TYPE:
  manufacturer_id, vehicle_type, is_deleted
  manufacturer_id, is_active, sort_order

TRANSPORT_COMMAND:
  manufacturer_vehicle_type_id, command_code, is_deleted
  manufacturer_vehicle_type_id, is_active, sort_order

Action assignment:
  action_unit_id, sort_order
  manufacturer_vehicle_type_id

TRANSPORT_LOCATION:
  project_id, is_deleted, sort_order

TRANSPORT_EQUIPMENT:
  project_id, is_deleted, sort_order
```

## 11. Migration Rule

Migrate existing data without changing existing Command IDs.

Recommended sequence:

```text
1. Create TRANSPORT_MANUFACTURER_VEHICLE_TYPE.
2. Create one Vehicle Type row from each existing Manufacturer.vehicle_type.
3. Add manufacturer_vehicle_type_id to TRANSPORT_COMMAND.
4. Map each existing Command to the generated Vehicle Type row.
5. Preserve command_id values.
6. Deprecate or remove Manufacturer.vehicle_type.
7. Deprecate or remove Command.manufacturer_id.
```

If immediate removal is unsafe, obsolete columns may remain temporarily, but they must not remain authoritative.

## 12. Deactivation and Deletion

All master deletion is soft deletion.

Inactive records cannot be newly assigned or selected.

Existing references remain stored and must display an inactive indication.

Deletion restrictions:

```text
Manufacturer:
  cannot delete while Vehicle Types, Commands, or Flow assignments exist

Manufacturer Vehicle Type:
  cannot delete while Commands or Flow assignments exist

Command:
  cannot delete while referenced by a Node

Location / Equipment:
  cannot delete while referenced by a Node
```

## 13. Save, Snapshot, and Duplication

Action Unit assignments must be preserved through:

```text
Flow save
Flow load
Version Snapshot
Flow duplication
JSON export
AI DSL export
```

Flow duplication regenerates Action Unit and assignment IDs, remaps assignments to duplicated Action Units, and retains global `manufacturer_vehicle_type_id` references.

## 14. Test Conditions

Confirm that:

- one Manufacturer can own AGF and AGV;
- duplicate AGF or AGV under one Manufacturer is rejected;
- Commands belong to Manufacturer plus Vehicle Type;
- duplicate Command Code is rejected within one Manufacturer Vehicle Type;
- the same Command Code is allowed under another Manufacturer Vehicle Type;
- one Action Unit may assign multiple Manufacturer Vehicle Types;
- duplicate Action Unit assignments are rejected;
- Node Command validation rejects a Command outside the Action Unit assignments;
- assignments survive save, load, Snapshot, and duplication;
- existing Location and Equipment behavior remains Project-scoped.

## 15. Completion Condition

Global Manufacturer, Manufacturer Vehicle Type, and Command master data are structurally separated from Project Location and Equipment data, and Flow Action Units can reference multiple Manufacturer Vehicle Types without duplicating master definitions per Project.
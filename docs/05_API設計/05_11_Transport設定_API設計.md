# 05_11 Transport Settings API Design

## 1. Purpose

Transport Settings APIs provide Transport master data used by Transport Flows. Location and Equipment are Project-level master data. The current Command ownership model is global through its Manufacturer relationship.

## 2. Project Location API

### List

```http
GET /api/projects/{projectId}/transport/locations
```

Returns non-deleted Locations for the Project, ordered by `SortOrder` and name. The response supplies the Node Detail selector and Project Transport Settings with:

- `LocationId`
- `ProjectId`
- `Name`
- `LocationType`
- `Description`
- `SortOrder`
- `IsDeleted`
- creation and update timestamps

Soft-deleted Locations are excluded from the list, and the response also exposes the current deletion flag. An active/inactive redesign is outside this cycle.

### Create

```http
POST /api/projects/{projectId}/transport/locations
```

Required fields are `Name` and `LocationType`. Optional fields are `Description` and `SortOrder`.

### Update and Delete

```http
PUT /api/projects/{projectId}/transport/locations/{locationId}
DELETE /api/projects/{projectId}/transport/locations/{locationId}
```

Delete uses soft deletion. Project Transport Settings owns these management operations; Node Detail only selects an existing Location.

## 3. Project Equipment API

```http
GET    /api/projects/{projectId}/transport/equipments
POST   /api/projects/{projectId}/transport/equipments
PUT    /api/projects/{projectId}/transport/equipments/{equipmentId}
DELETE /api/projects/{projectId}/transport/equipments/{equipmentId}
```

Equipment belongs to a Project and is validated against the Flow Project when referenced by a Node.

## 4. Manufacturer and Command API

```http
GET    /api/transport/manufacturers
POST   /api/transport/manufacturers
GET    /api/transport/commands
POST   /api/transport/commands
```

Update and delete use the corresponding master ID path. Commands remain associated with Manufacturers according to the current implementation.

## 5. Errors and Permissions

- `400`: invalid request data
- `404`: Project or master record not found
- `422`: invalid Flow structure reference or value

Read operations require Flow read permission. Create, update, and delete operations require Flow update permission in the current API implementation.

## 6. Node Reference Rule

A Transport Node stores only `LocationId`. Flow Structure validation rejects a Location that is missing, deleted, or belongs to another Project. Normal Flow clears the reference.

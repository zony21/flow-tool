# 05_06 Template Export API Design

## 1. Scope

This document defines the intended Transport Template export mapping. The Transport Template Generator is not implemented in the Location foundation cycle.

## 2. Stage Classification

In a Transport Flow, Stage represents the processing owner rather than only an equipment category.

Examples include AGF/AGV, PLC, WCS, conveyor, shutter, tablet, and human operator.

`StageType` values:

- `AUTO`: equipment or system control;
- `MANUAL`: human work.

The future Transport Template includes `AUTO` stages and excludes `MANUAL` stages. The Flow diagram may continue to contain both.

## 3. Location Mapping

The Transport Template Location column must use this mapping:

```text
Location
  <- Node.LocationId
  <- resolved TransportLocation code or display value
```

The generator must not infer Location from:

- Equipment;
- Node coordinates;
- Lane;
- Stage;
- Project layout;
- screen layout.

Location is Project-level master data, while the relevant Transport Node stores only its `LocationId` reference.

## 4. Planned Generation Sequence

The planned generator will:

1. require `FlowType = TRANSPORT`;
2. split Transport patterns by Lane;
3. traverse Nodes in Link order within each Lane;
4. resolve each Node's Stage;
5. exclude `StageType = MANUAL`;
6. generate steps for eligible `AUTO` Nodes;
7. number steps within each Lane;
8. resolve the Location column through `Node.LocationId`.

Traversal, pattern splitting, filtering, Markdown export, and PDF export remain outside the current implementation cycle.

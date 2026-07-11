# CutListTool Test Input Guide

Use this as a quick reference when filling out strict JSON test input files.

Your blank input file should have this top-level shape:

```json
{
  "preferences": {},
  "ductmateFrames": [],
  "liners": [],
  "turnVanes": [],
  "flexConnectors": [],
  "connectionTypes": []
}
```

JSON property names can be written in camel case, like `width`, `height`, and `connectionTypeKey`.

Enum values should be written as strings and should match the C# enum names exactly, like `"Roll56"`, `"OneAndHalf_Inch"`, `"FourPiece"`, or `"Out"`.

---

## Top-level sections

| JSON section | What it is for |
|---|---|
| `preferences` | Optional settings override. Use `{}` to keep defaults. |
| `ductmateFrames` | Ductmate frame build items. |
| `liners` | Liner build items. |
| `turnVanes` | Turn vane build items. |
| `flexConnectors` | Flex connector build items. |
| `connectionTypes` | Connection definitions used by flex connectors. |

---

## Preferences

Use this only when you want to override default settings.

```json
"preferences": {
  "dmCutAllowance": 1.25,
  "fourPieceWidthDeduction": 2,
  "fourPieceHeightDeduction": 0.5,
  "twoPieceDeduction": 1.5,
  "diagonalDeduction": 2,
  "vaneSpacing": 4.75,
  "splitterGap": 0.125,
  "canvasAddPerSide": 3
}
```

For normal tests, this is fine:

```json
"preferences": {}
```

---

## Ductmate frames

List name:

```json
"ductmateFrames": []
```

Fields:

| Field | Type | Notes |
|---|---|---|
| `width` | decimal | Width in inches. |
| `height` | decimal | Height in inches. |
| `qty` | int | Number of frames. |
| `label` | string or null | Optional build label. |

Example:

```json
{
  "width": 24,
  "height": 12,
  "qty": 1,
  "label": "AHU-1"
}
```

---

## Liners

List name:

```json
"liners": []
```

Fields:

| Field | Type | Notes |
|---|---|---|
| `width` | decimal | Width in inches. |
| `height` | decimal | Height in inches. |
| `qty` | int | Quantity. |
| `rollLength` | enum string | `Roll56` or `Roll59`. |
| `thickness` | enum string | Liner thickness. |
| `pieceMode` | enum string | `TwoPiece` or `FourPiece`. |
| `label` | string or null | Optional build label. |

`rollLength` options:

```text
Roll56
Roll59
```

`thickness` options:

```text
None
Half_Inch
One_Inch
OneAndHalf_Inch
```

`pieceMode` options:

```text
TwoPiece
FourPiece
```

Example:

```json
{
  "width": 24,
  "height": 12,
  "qty": 1,
  "rollLength": "Roll56",
  "thickness": "OneAndHalf_Inch",
  "pieceMode": "FourPiece",
  "label": "LINER-1"
}
```

---

## Turn vanes

List name:

```json
"turnVanes": []
```

Fields:

| Field | Type | Notes |
|---|---|---|
| `cheekA` | decimal | First cheek dimension in inches. |
| `cheekB` | decimal | Second cheek dimension in inches. |
| `heel` | decimal | Heel dimension in inches. |
| `liner` | enum string | Liner thickness inside the elbow. |
| `qty` | int | Number of elbows. |
| `label` | string or null | Optional build label. |
| `splitVanes` | int | Usually `1`; use more for split vanes. |

`liner` options:

```text
None
Half_Inch
One_Inch
OneAndHalf_Inch
```

Example:

```json
{
  "cheekA": 24,
  "cheekB": 24,
  "heel": 12,
  "liner": "OneAndHalf_Inch",
  "qty": 1,
  "label": "TV-1",
  "splitVanes": 1
}
```

---

## Flex connectors

List name:

```json
"flexConnectors": []
```

Fields:

| Field | Type | Notes |
|---|---|---|
| `dimA` | decimal | First dimension in inches. |
| `dimB` | decimal | Second dimension in inches. |
| `qty` | int | Number of connectors. |
| `size` | enum string | `Large` or `Small`. |
| `pieceCount` | enum string | Number of canvas pieces. |
| `connectionA` | object | First side/end connection. |
| `connectionB` | object | Second side/end connection. |
| `label` | string or null | Optional build label. |
| `shape` | enum string | Usually `Rectangular`; `Round` exists. |

`size` options:

```text
Large
Small
```

`pieceCount` options:

```text
OnePiece
TwoPiece
FourPiece
```

`shape` options:

```text
Rectangular
Round
```

Example:

```json
{
  "dimA": 24,
  "dimB": 12,
  "qty": 1,
  "size": "Large",
  "pieceCount": "TwoPiece",
  "connectionA": {
    "connectionTypeKey": "raw",
    "flangeDirection": null,
    "flangeSize": null,
    "sideConnections": null,
    "smallEnd": false
  },
  "connectionB": {
    "connectionTypeKey": "ductmate",
    "flangeDirection": "Out",
    "flangeSize": 1.5,
    "sideConnections": null,
    "smallEnd": false
  },
  "label": "FLEX-1",
  "shape": "Rectangular"
}
```

---

## Connections inside flex connectors

Connection fields:

| Field | Type | Notes |
|---|---|---|
| `connectionTypeKey` | string | Must match a key from `connectionTypes`. |
| `flangeDirection` | enum string or null | Use only when needed. |
| `flangeSize` | decimal or null | Use only when needed. |
| `sideConnections` | array or null | Use when different sides need different connections. |
| `smallEnd` | bool | Usually `false`. |

`flangeDirection` options:

```text
In
Out
Straight
```

Normal all-around connection:

```json
{
  "connectionTypeKey": "ductmate",
  "flangeDirection": "Out",
  "flangeSize": 1.5,
  "sideConnections": null,
  "smallEnd": false
}
```

Per-side connection format:

```json
"sideConnections": [
  {
    "side": "Top",
    "connectionTypeKey": "ductmate",
    "flangeDirection": "Out",
    "flangeSize": 1.5
  },
  {
    "side": "Bottom",
    "connectionTypeKey": "raw",
    "flangeDirection": null,
    "flangeSize": null
  }
]
```

`side` options:

```text
Top
Bottom
Left
Right
```

---

## Connection types

List name:

```json
"connectionTypes": []
```

Every `connectionTypeKey` used in a flex connector should have a matching `key` here.

Fields:

| Field | Type | Notes |
|---|---|---|
| `key` | string | Short internal name used by flex connector connections. |
| `displayName` | string | Human-readable output name. |
| `usesFlangeOptions` | bool | Whether `flangeDirection` and `flangeSize` matter. |
| `shape` | enum string | `Rectangular` or `Round`. |

Example:

```json
{
  "key": "ductmate",
  "displayName": "Ductmate",
  "usesFlangeOptions": true,
  "shape": "Rectangular"
}
```

Common starter examples:

```json
{
  "key": "raw",
  "displayName": "Raw Edge",
  "usesFlangeOptions": false,
  "shape": "Rectangular"
}
```

```json
{
  "key": "ductmate",
  "displayName": "Ductmate",
  "usesFlangeOptions": true,
  "shape": "Rectangular"
}
```

---

## Current enum values

### BuildItemType

Used internally by the engine/request system.

```text
Ductmate
Liner
TurnVane
Flex
FilterRackChannel
```

`FilterRackChannel` exists as an enum value, but there is not currently a `filterRackChannels` input list in `CutListInputData`.

### LinerThickness

```text
None
Half_Inch
One_Inch
OneAndHalf_Inch
```

### LinerRollLength

```text
Roll56
Roll59
```

### LinerPieceMode

```text
TwoPiece
FourPiece
```

### FlexSize

```text
Large
Small
```

### FlexPieceCount

```text
OnePiece
TwoPiece
FourPiece
```

### FittingSide

```text
Top
Bottom
Left
Right
```

### FlangeDirection

```text
In
Out
Straight
```

### ConnectorShape

```text
Rectangular
Round
```

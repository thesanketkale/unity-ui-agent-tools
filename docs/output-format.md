# UI Context Output Formats

`unity-ui-agent-tools` produces deterministic, machine-readable artifacts designed for both automated AI consumption and human auditing.

---

## 1. `ui-tree.json` (Exhaustive Hierarchy)
Conforms to [`schema/ui-tree.schema.json`](../schema/ui-tree.schema.json).
Contains the complete object tree, including:
- **Node Locator:** Path-based deterministic ID (e.g., `/Canvas/HUD/HealthBar/Fill`).
- **Resolved Pixel Rect:** Evaluated in reference canvas coordinates, factoring in anchor spreads and `CanvasScaler` reference resolution.
- **Component Dictionary:** All attached uGUI/TMP components with their serialized properties and persistent listener references.
- **Prefab Linkage:** Source prefab GUID, relative asset path, and list of overridden property paths.

---

## 2. `ui-outline.md` (Token-Optimized Hierarchy)
Conforms to [`schema/ui-outline.schema.json`](../schema/ui-outline.schema.json).
Designed to convey layout and interactability to AI agents with minimal context overhead (~80% token reduction compared to raw JSON).

### Shorthand Conventions:
- **Anchors:**
  - `[stretch-stretch]`: Stretches both horizontally and vertically.
  - `[top-left]`: Anchored to the top-left corner.
  - `[middle-center]`: Centered in parent rect.
- **Dimensions:** `[<anchor> <width>x<height>@<x>,<y>]` in canvas reference pixels.
- **Interactive Badges:**
  - `**[BTN -> Target.Method()]**`: Interactive button with persistent click listener.
  - `**[TOGGLE: ON/OFF]**`: Active toggle switch.
  - `**[SLIDER: 0.8]**`: Slider with current normalized value.
  - `**[INPUT: "Placeholder"]**`: Text input field.
  - `"[IMG: sprite #raycast]"`: Graphic element with raycast target enabled.

---

## 3. `ui-validation.json` (SARIF 2.1.0 Diagnostics)
Conforms to [`schema/ui-validation.schema.json`](../schema/ui-validation.schema.json) and OASIS SARIF v2.1.0 standard.
- Can be uploaded natively to GitHub Advanced Security code scanning.
- Each finding reports the rule ID (`UI001` - `UI009`), severity (`error`, `warning`, `note`), exact node locator, and remediation message.

---

## 4. `ui-diff.json` and `ui-diff.md` (Semantic Changes)
Conforms to [`schema/ui-diff.schema.json`](../schema/ui-diff.schema.json).
Records categorized changes between two snapshots:
- `Added`: New GameObject in hierarchy.
- `Removed`: Deleted GameObject.
- `Resized`: Changes in resolved rect dimensions.
- `Moved`: Changes in anchored position.
- `TextChanged`: Alterations in string values.
- `ComponentAdded` / `ComponentRemoved`: Structural script changes.

---

## 5. `captures.json` (Screenshot Coordinates)
Conforms to [`schema/captures.schema.json`](../schema/captures.schema.json).
Maps rendered PNG screenshots across target aspect ratios to numbered bounding boxes and node IDs for visual verification.

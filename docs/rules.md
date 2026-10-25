# UI Validation Rules Reference

`unity-ui-agent-tools` includes 9 built-in deterministic validation rules evaluated against exported `UiNode` trees. Severities and thresholds are configured via `.uicontext.json`.

---

## Summary of Rules

| Rule ID | Name | Default Severity | Description |
|---|---|---|---|
| **UI001** | `MissingReferences` | `error` | Missing MonoScript or dangling UnityEvent persistent listener |
| **UI002** | `OverlappingInteractables` | `error` | Active interactive hitboxes visually overlapping on screen |
| **UI003** | `MinimumTargetSize` | `warning` | Hit targets smaller than accessible touch/click standards |
| **UI004** | `TextOverflow` | `warning` | Unwrapped text content exceeding RectTransform bounds |
| **UI005** | `OutsideCanvasBoundary` | `warning` | UI element positioned outside canvas or safe area |
| **UI006** | `NonInteractiveRaycastTarget`| `note` | Graphic element with raycast enabled without interactive parent |
| **UI007** | `DeepLayoutGroupNesting` | `note` | LayoutGroup hierarchy depth exceeding performance limit |
| **UI008** | `DuplicateSiblingNames` | `warning` | Siblings sharing identical GameObject names |
| **UI009** | `AbsolutePositionInLayoutGroup` | `warning` | Manual offsets inside LayoutGroups that override them |

---

## Detailed Rule Specifications

### UI001: MissingReferences
- **Rationale:** Dangling scripts or listeners crash at runtime when events fire.
- **Trigger:** Any component with `isMissingScript=true` or a UnityEvent listener target set to `None`/empty.
- **Fix:** Remove unused component or reconnect target GameObject and method.

### UI002: OverlappingInteractables
- **Rationale:** When buttons overlap, Unity's graphic raycaster unpredictably swallows clicks on the lower element.
- **Trigger:** Two selectable elements whose resolved pixel bounds intersect by > 1px.
- **Fix:** Adjust layout anchors or parent elements inside a LayoutGroup.

### UI003: MinimumTargetSize
- **Rationale:** Small buttons cause user error, especially on touch devices (Apple HIG requires 44pt, Android Material 48dp, OpenXR 64px).
- **Trigger:** Selectable element with width or height less than `minInteractableWidth`/`minInteractableHeight`.
- **Fix:** Increase RectTransform `sizeDelta` or add transparent raycast padding.

### UI004: TextOverflow
- **Rationale:** Text strings clipped by narrow containers produce unreadable interfaces.
- **Trigger:** Text length exceeds rect width while `enableWordWrapping` is `false` and `overflowMode` is `Overflow`.
- **Fix:** Enable word wrapping, set overflow mode to `Ellipsis`/`Truncate`, or add `ContentSizeFitter`.

### UI005: OutsideCanvasBoundary
- **Rationale:** Elements with positive coordinates outside the Canvas are invisible to users.
- **Trigger:** Element's resolved rect does not intersect the Canvas reference resolution.
- **Fix:** Realign anchors to keep element within viewport bounds.

### UI006: NonInteractiveRaycastTarget
- **Rationale:** Every graphic with `raycastTarget=true` is tested per click, degrading CPU performance.
- **Trigger:** `Image` or `Text` with `raycastTarget=true` having no `Button`, `Toggle`, or interactive parent.
- **Fix:** Uncheck `Raycast Target` in the Inspector.

### UI007: DeepLayoutGroupNesting
- **Rationale:** Layout groups recalculate recursively in $O(N \cdot D)$; deep nesting causes GC allocations and frame drops.
- **Trigger:** Nesting depth of layout groups exceeds `maxLayoutGroupDepth` (default: 3).
- **Fix:** Flatten layout hierarchy using anchors or anchored positions.

### UI008: DuplicateSiblingNames
- **Rationale:** Path-based hierarchy addressing (`/Canvas/Panel/Button`) requires unique sibling names for deterministic test locators.
- **Trigger:** Multiple sibling GameObjects sharing the same `name`.
- **Fix:** Rename siblings with distinctive suffixes (e.g., `SubmitButton`, `CancelButton`).

### UI009: AbsolutePositionInLayoutGroup
- **Rationale:** LayoutGroups dynamically calculate child positions; manual anchored offsets are silently overwritten.
- **Trigger:** Non-zero `anchoredPosition` on a child of a LayoutGroup without `LayoutElement(ignoreLayout=true)`.
- **Fix:** Set child `anchoredPosition` to `(0, 0)` or add `LayoutElement` with `Ignore Layout` enabled.

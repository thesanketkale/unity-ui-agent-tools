# ADR-0002: Core Engine Isolation from UnityEngine

## Status
Accepted

## Date
2026-10-25

## Context
Unity Editor code typically binds tightly to `UnityEngine.GameObject`, `UnityEngine.RectTransform`, and `UnityEditor.PrefabUtility`. However, running automated tests against code that depends on `UnityEngine` requires either a full Unity Editor installation with valid licensing or complex mock frameworks.

We need a design that guarantees fast, deterministic CI verification of all rules, serializers, and diff engines on hosted GitHub Actions runners without requiring a licensed Unity build server.

## Decision
We enforce an architectural boundary:
1. **`UiContext.Core` is an isolated pure C# assembly** compiling to both `netstandard2.1` and `net8.0`. It contains no references to `UnityEngine`, `UnityEngine.UI`, `TMPro`, or `UnityEditor`.
2. The domain model `UiNode` holds plain primitives (`float`, `string`, `bool`, `Dictionary<string, object>`) representing rect transforms, component properties, and layout parameters.
3. The `Editor/` assembly acts solely as a read-only "adapter" that walks live Unity scenes/prefabs, extracts values from `RectTransform` and UI components, and populates the `UiNode` data tree.
4. Once the `UiNode` tree is populated, all subsequent operations—including JSON serialization, markdown outline generation, 9 validation rules, SARIF reporting, and semantic diff computation—are executed purely within `Core`.

## Alternatives Rejected
- **Mocking UnityEngine via NSubstitute or Moq:** Creating mock `GameObject` or `Component` interfaces.
  - *Reason for Rejection:* Fragile, heavyweight, and prone to breaking across Unity engine minor releases. Plain data transfer models are cleaner, serialize natively to JSON, and have zero execution overhead.
- **Requiring Unity Batchmode in Hosted CI:** Running `Unity -batchmode -runTests` in every CI run.
  - *Reason for Rejection:* Requires Unity licensing credentials (ULS or username/password secrets), runner disk provisioning (20GB+ for editor install), and takes 5–10 minutes per test run compared to 2 seconds for `dotnet test`.

## Consequences
- 100% of validation rules, edge cases (zero size, overlaps, overflow, missing references), and semantic diff logic can be tested in automated xUnit suites with passing and failing fixtures.
- The package remains cleanly decoupled and future-proof.

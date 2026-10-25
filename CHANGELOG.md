# Changelog

All notable changes to `unity-ui-agent-tools` will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-10-25

### Added
- **Pure .NET Core Engine (`UiContext.Core`):**
  - Zero-`UnityEngine` hierarchy data structures (`UiNode`, `RectTransformData`, `CanvasScalerData`, `HierarchyTree`).
  - Multi-targeting `net8.0` and `netstandard2.1` for fast, license-free CI testing.
  - Deterministic JSON serializer (`UiTreeSerializer`) with sorted keys.
  - Token-efficient Markdown hierarchy outliner (`UiOutlineFormatter`) with shorthand anchors and component badges.
- **Rules Engine & SARIF 2.1.0 Diagnostics:**
  - 9 built-in deterministic validation rules (`UI001` - `UI009`):
    - `UI001`: Missing MonoScript or dangling UnityEvent listener target.
    - `UI002`: Overlapping interactive hitboxes.
    - `UI003`: Accessible minimum target size standards (desktop 32px, mobile 48px, XR 64px).
    - `UI004`: Text overflow clipping bounds without wrapping or ellipsis.
    - `UI005`: Visual elements positioned outside canvas boundary.
    - `UI006`: Graphic element raycast targets without interactive ancestors.
    - `UI007`: Deeply nested LayoutGroup hierarchies exceeding depth threshold.
    - `UI008`: Duplicate sibling names breaking deterministic locators.
    - `UI009`: Absolute anchoredPosition inside auto-layout groups.
  - SARIF 2.1.0 exporter for native GitHub Code Scanning ingestion.
  - `UiContextAssert.NoErrors(result)` test assertion helper for EditMode test suites.
- **Semantic UI Differ:**
  - Change detection identifying added, removed, moved, resized, text-changed, and component-modified nodes.
  - Formatted Markdown diff table generation.
- **Unity Editor Package (`Packages/io.github.thesanketkale.ui-context`):**
  - Hierarchy walkers extracting rects, anchors, and uGUI/TMP component properties.
  - Batchmode CLI entry point `UiContext.Editor.UiContextCli.Export` with documented exit codes (`0`, `1`, `2`, `3`).
  - In-editor screenshot harness across resolution and aspect ratio matrices.
  - In-editor localhost HTTP server bridge (`http://127.0.0.1:49152`) for locked editor workspaces.
- **TypeScript CLI & MCP Server (`cli/`):**
  - Official Model Context Protocol (`@modelcontextprotocol/sdk`) stdio server exposing `describe_ui`, `capture_ui`, `validate_ui`, and `diff_ui`.
  - Terminal CLI (`ui-context diff`, `ui-context validate`, `ui-context mcp`).
- **Sample Unity Project (`Projects/Sample`):**
  - Hand-crafted minimal project with `HUD.prefab`, `SettingsDialog.prefab`, and `CharacterStatus.prefab`.
  - EditMode test suite proving zero validation errors.
- **JSON Schemas (`schema/`):**
  - `ui-tree.schema.json`, `ui-outline.schema.json`, `ui-validation.schema.json`, `ui-diff.schema.json`, `captures.schema.json`, `config.schema.json`.
- **CI & Security Baseline:**
  - GitHub Actions workflows for multi-runtime CI, CodeQL, dependency review, gitleaks full-history scanning, and release-please.
  - Repository clean-room and secret scanner (`tools/guard.ps1`, `tools/guard.sh`).

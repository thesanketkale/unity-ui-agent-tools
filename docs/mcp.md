# Model Context Protocol (MCP) Server Reference

`unity-ui-agent-tools` ships with a stdio-based MCP server providing high-level tools to AI coding assistants.

## MCP Tools Catalog

### 1. `describe_ui`
- **Purpose:** Returns a token-efficient Markdown outline or JSON tree of the target UI.
- **Input Schema:**
  - `targetPath` *(string, optional)*: Path to `ui-outline.md`, `ui-tree.json`, or output directory.
- **Output:** Indented Markdown outline detailing hierarchy, dimensions, text values, and listeners.

### 2. `validate_ui`
- **Purpose:** Evaluates UI quality rules and returns structured SARIF findings.
- **Input Schema:**
  - `targetPath` *(string, optional)*: Path to `ui-validation.json` or output directory.
- **Output:** Formatted diagnostic report summarizing errors, warnings, and exact node paths.

### 3. `diff_ui`
- **Purpose:** Compares before and after UI snapshots to detect layout or content drift.
- **Input Schema:**
  - `beforePath` *(string, required)*: Path to before `ui-tree.json`.
  - `afterPath` *(string, required)*: Path to after `ui-tree.json`.
- **Output:** Markdown diff table listing added, removed, resized, moved, and text-modified elements.

### 4. `capture_ui`
- **Purpose:** Retrieves screenshot metadata and numbered bounding box coordinates.
- **Input Schema:**
  - `targetPath` *(string, optional)*: Path to `captures.json` or captures directory.
- **Output:** List of captured PNG resolutions, bounding-box overlays, and node ID legends.

---

## Editor Bridge (Lock Bypass)
When the Unity Editor is running, the project directory is locked by Unity. In this state, an optional lightweight localhost HTTP server (`http://127.0.0.1:49152`) running inside Unity handles requests seamlessly without file lock conflicts. Start via Unity Menu: `Tools > UI Context > Start Local Agent Bridge`.

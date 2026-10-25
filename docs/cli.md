# CLI Reference: `ui-context`

The `ui-context` command-line tool provides terminal commands for diffing snapshots, inspecting SARIF reports, and running the MCP server.

## Installation
Run via `npx` or install globally:
```bash
npx @thesanketkale/ui-context --help
```

## Commands

### `ui-context diff <before> <after>`
Computes semantic differences between two `ui-tree.json` snapshot files.
- Options:
  - `-j, --json`: Emit structured JSON matching `schema/ui-diff.schema.json` instead of Markdown.
- Example:
  ```bash
  ui-context diff snapshots/before.json snapshots/after.json
  ```

### `ui-context validate <sarifPath>`
Parses a SARIF 2.1.0 diagnostic file and prints a formatted terminal summary.
- Example:
  ```bash
  ui-context validate ui-context-out/ui-validation.json
  ```

### `ui-context mcp`
Launches the Model Context Protocol (MCP) server over standard input/output (`stdio`).

---

## Unity Batchmode Execution

External build pipelines and agents can trigger in-engine export without human interaction:
```bash
"$UNITY_PATH" -batchmode -nographics \
  -projectPath Projects/Sample \
  -executeMethod UiContext.Cli.Export \
  -uiContext.target Assets/Prefabs/HUD.prefab \
  -uiContext.out ui-context-out
```

### Batchmode Exit Codes:
- `0`: Success (all artifacts exported successfully).
- `1`: Invalid arguments or missing `-uiContext.target`.
- `2`: Target prefab/scene failed to load or runtime exception.
- `3`: Validation rule errors detected (when `-uiContext.failOnErrors` is set).

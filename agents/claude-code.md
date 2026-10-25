# Claude Code Integration Guide

This guide describes how to configure Anthropic's Claude Code CLI with `unity-ui-agent-tools`.

## MCP Setup

Add the UI Context MCP server to your Claude Code environment:
```bash
claude mcp add unity-ui-context -- node <repo-root>/cli/dist/mcp-server.js
```

Or configure `.mcp.json` at your project root:
```json
{
  "mcpServers": {
    "unity-ui-context": {
      "command": "node",
      "args": ["./cli/dist/mcp-server.js"]
    }
  }
}
```

## Recommended Prompts

- **Inspect UI:**
  `"Claude, describe the HUD prefab layout using describe_ui and summarize all interactive buttons."`
- **Check Diff:**
  `"Compare the UI state before and after my changes using diff_ui."`
- **Verify Quality:**
  `"Run validate_ui on SettingsDialog and confirm there are no accessibility errors."`

# Google Gemini CLI Integration Guide

This guide describes how to connect Google Gemini CLI and Antigravity agents with `unity-ui-agent-tools`.

## Setup via MCP

Register the MCP server in your Antigravity configuration:
```json
{
  "mcpServers": {
    "unity-ui-context": {
      "command": "node",
      "args": ["d:/Repositories/git/thesanketkale/unity-ui-agent-tools/cli/dist/mcp-server.js"]
    }
  }
}
```

## Agent Workflows
- **Autonomous Verification:** When completing UI tasks, Antigravity agents invoke `describe_ui` to read the outline, perform code refactoring, run `diff_ui` to ensure no regression, and check `validate_ui` to confirm zero SARIF errors before committing.

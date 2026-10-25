# ADR-0003: Model Context Protocol (MCP) and CLI Architecture

## Status
Accepted

## Date
2026-10-25

## Context
AI agents interacting with the Unity project require a low-latency, reliable protocol to query UI state, trigger validation, and inspect visual diffs. The Anthropic Model Context Protocol (MCP) has emerged as the open standard for connecting AI coding assistants (such as Claude Code, Codex CLI, and Gemini) with external development tools.

We must decide how the MCP server will interact with the Unity Editor, handle locked workspaces when the Editor is open, and provide both CLI and RPC workflows.

## Decision
1. **Official MCP SDK over `stdio`:** We implement the MCP server in TypeScript (`cli/src/mcp-server.ts`) using the official `@modelcontextprotocol/sdk`. The server communicates over standard input/output (`stdio`), making it zero-configuration for Claude Code (`claude mcp add`) and other agents.
2. **Dual-Mode Operation:**
   - **Mode A (Cached/Artifact Mode):** Reads pre-exported artifacts (`ui-tree.json`, `ui-outline.md`, `ui-validation.json`) produced during the last save, build, or batch export. This provides sub-millisecond response times without touching Unity.
   - **Mode B (Batchmode Headless On-Demand):** When an agent requests fresh export and the Unity Editor is closed, the CLI launches `Unity.exe -batchmode -projectPath ... -executeMethod UiContext.Cli.Export`.
   - **Mode C (Localhost Bridge for Active Editor):** When the project is open in the interactive Unity Editor, the workspace's `Library/` directory is locked by Unity. In this state, an optional lightweight localhost HTTP/WebSocket bridge (`http://127.0.0.1:49152`) running inside Unity Editor handles on-demand export requests without locking conflicts.
3. **Exposed MCP Tools:**
   - `describe_ui`: Returns the token-efficient Markdown outline and component summary of a target prefab or the open scene.
   - `validate_ui`: Runs validation rules and returns structured SARIF findings with actionable line/node locators.
   - `diff_ui`: Compares two UI snapshots and outputs a formatted Markdown diff highlighting layout and styling changes.
   - `capture_ui`: Retrieves screenshot image paths and bounding-box legend mappings for visual verification.

## Alternatives Rejected
- **Custom TCP / WebSocket Protocol:** Requiring custom agent client plugins.
  - *Reason for Rejection:* Lacks interoperability. The Model Context Protocol is natively supported by modern coding agents.
- **Requiring the Unity Editor to Always Be Open:**
  - *Reason for Rejection:* Precludes headless CI agent workflows and automated background linting.

## Consequences
- AI agents get instant access to clean UI diagnostics directly within their natural workflow.
- Clear fallbacks ensure the tool works seamlessly whether Unity Editor is closed, open interactively, or running in CI.

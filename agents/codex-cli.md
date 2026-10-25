# OpenAI Codex CLI Integration Guide

This guide describes how to use `unity-ui-agent-tools` from command-line agent environments like OpenAI Codex CLI.

## CLI Invocation

Codex CLI interacts via standard terminal commands:

1. **Semantic Diff Inspection:**
   ```bash
   npx @thesanketkale/ui-context diff ./snapshots/before.json ./snapshots/after.json
   ```

2. **SARIF Report Parsing:**
   ```bash
   npx @thesanketkale/ui-context validate ./ui-context-out/ui-validation.json
   ```

3. **Batchmode Prefab Introspection:**
   ```bash
   "$UNITY_PATH" -batchmode -nographics -projectPath . -executeMethod UiContext.Cli.Export -uiContext.target Assets/Prefabs/Dialog.prefab -uiContext.out ./out
   ```

# UI Context Package Documentation

`io.github.thesanketkale.ui-context` provides tools for converting Unity uGUI and TextMeshPro hierarchies into machine-readable JSON trees, compact token-saving Markdown outlines, and SARIF 2.1.0 validation reports.

## Installation
Add the package via the Unity Package Manager (UPM) Git URL:
```
https://github.com/thesanketkale/unity-ui-agent-tools.git?path=/Packages/io.github.thesanketkale.ui-context#v0.1.0
```

## Features
- **Deterministic UI Extraction:** Exports layout geometry, anchors, component properties, and event listeners.
- **Rules Engine:** 9 built-in rules flagging overlapping hitboxes, undersized targets, text overflow, and layout nesting.
- **Headless CLI:** Run `Unity -batchmode -executeMethod UiContext.Cli.Export` in continuous integration.
- **Localhost Bridge:** Optional in-editor HTTP server allowing AI coding agents to introspect UI while the project is open in the Unity Editor.

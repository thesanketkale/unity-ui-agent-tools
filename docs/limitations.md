# Known Limitations and Constraints

`unity-ui-agent-tools` focuses on delivering deterministic introspection for Unity's primary UI systems. Please note the following current boundaries:

## 1. uGUI and TextMeshPro Scope
- Supported in v0.1 - v0.3: standard uGUI (`Canvas`, `RectTransform`, `Image`, `Button`, `Toggle`, `Slider`, `Scrollbar`, `ScrollRect`, layout groups) and `TextMeshProUGUI`.
- **UI Toolkit (`UIDocument`, UXML, USS):** Not supported in v0.1. UI Toolkit support is scheduled for the v0.4 roadmap.

## 2. Runtime-Generated UI
- The tool inspects serialized prefab assets and static scene hierarchies.
- UI elements generated dynamically at runtime via procedural scripts without prefabs are not introspected by the offline batch exporter.

## 3. Screenshot Capture Headless Mode
- Visual rendering (`UiScreenshotHarness`) requires an active GPU graphics device context.
- Unity running in `-nographics` mode cannot render camera passes to `RenderTexture`. Consequently, screenshot generation is an **owner-verified** or desktop-runner workflow.

## 4. Custom Shaders and Procedural Effects
- Custom material shaders (e.g., custom UI blur or distortion shaders) that rely on external frame buffers are rendered with fallback solid colors during offscreen capture passes.

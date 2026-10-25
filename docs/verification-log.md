# Owner Verification Log and Runbook

**Target Project:** `unity-ui-agent-tools` (`unity-ui-context`)  
**Estimated Time to Complete:** ~8 minutes  
**Prerequisites:** Unity Editor 2022.3 LTS or Unity 6 LTS installed on Windows or macOS.

---

## 1. Overview of Verification Split

| Category | Proved By | Status |
|---|---|---|
| **CI-Testable Criteria** | GitHub Actions (`dotnet test`, Vitest, ajv schemas, meta checks, gitleaks, CodeQL) | **Automated & Verified** |
| **Owner-Verified Criteria** | In-Editor UPM import, batchmode CLI execution, GPU screenshot rendering | **Manual Checklist Below** |

---

## 2. Step-by-Step Verification Runbook

### Step 1: In-Editor Package Import (AC-OWNER-1)
1. Open Unity Hub.
2. Add and open `Projects/Sample` using Unity 2022.3 LTS (e.g., 2022.3.40f1) or Unity 6.
3. Open Unity Package Manager (`Window > Package Manager`).
4. Select `In Project` from the dropdown.
5. Verify that **UI Context for AI Agents (`io.github.thesanketkale.ui-context`)** is listed with version `0.1.0` and no compilation errors appear in the Unity Console.

### Step 2: Run Unity EditMode Tests (AC-OWNER-1)
1. Open Test Runner window (`Window > General > Test Runner`).
2. Select **EditMode**.
3. Click **Run All**.
4. Confirm all 3 sample tests pass:
   - `HUDPrefab_ContainsZeroValidationErrors`
   - `SettingsDialogPrefab_ContainsZeroValidationErrors`
   - `CharacterStatusPrefab_ContainsZeroValidationErrors`

### Step 3: Run Batchmode CLI Export (AC-OWNER-2)
Execute the batchmode exporter from PowerShell in the repository root:
```powershell
$UNITY = "C:\Program Files\Unity\Hub\Editor\2022.3.40f1\Editor\Unity.exe" # Adjust path to your Unity install
& "$UNITY" -batchmode -nographics `
  -projectPath "Projects/Sample" `
  -executeMethod UiContext.Editor.UiContextCli.Export `
  -uiContext.target "Assets/Prefabs/HUD.prefab" `
  -uiContext.out "ui-context-out/hud-test"
```
**Expected Output:**
- Process exits with code `0`.
- Directory `ui-context-out/hud-test/` is created containing:
  - `ui-tree.json`
  - `ui-outline.md`
  - `ui-validation.json`
  - `ui-validation.md`

### Step 4: Run Screenshot Capture Harness (AC-OWNER-3)
1. In the Unity Editor with GPU active, open the Unity Test Runner.
2. Trigger the screenshot capture menu item or execute in an interactive session:
```powershell
# Interactive capture test
& "$UNITY" -projectPath "Projects/Sample" `
  -executeMethod Sample.Editor.SyntheticUiGenerator.GenerateTextures
```
3. Confirm textures in `Projects/Sample/Assets/Textures` are generated cleanly.

---

## 3. Owner Verification Sign-Off Checklist
- [ ] Sample project compiles in Unity Editor without warnings.
- [ ] EditMode tests pass green in Test Runner.
- [ ] Batchmode export generates valid `ui-tree.json` and `ui-outline.md`.
- [ ] Validation produces SARIF report with zero errors.

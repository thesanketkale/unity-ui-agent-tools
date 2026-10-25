# ADR-0004: UI Validation Rules and SARIF 2.1.0 Diagnostics Standard

## Status
Accepted

## Date
2026-10-25

## Context
Unity UI projects frequently suffer from silent layout bugs that compile without warnings:
- Overlapping hitboxes on interactive buttons.
- Touch/click targets too small for mobile or XR hand tracking (< 48px or < 64px).
- Missing component references or dangling UnityEvent persistent calls.
- Excessive nested LayoutGroups causing per-frame performance drops.
- Text overflow clipping outside RectTransform boundaries.

We need a standardized, machine-readable format for reporting these issues so both AI agents and CI security/linting gates (e.g., GitHub Code Scanning) can parse findings deterministically.

## Decision
1. **SARIF 2.1.0 as the Standard Output:** The validation engine produces `ui-validation.json` complying with the OASIS Static Analysis Results Interchange Format (SARIF) v2.1.0 standard.
   - Each rule has a stable rule ID (e.g., `UI001`, `UI002`).
   - Findings provide precise physical locations (prefab file path, node hierarchy path, and line/property hints).
   - Findings map to standard severities: `error`, `warning`, `note`.
2. **Deterministic Rules Engine (`IUiRule`):**
   - Implements 9 built-in rules with zero external dependencies.
   - Configurable via `.uicontext.json` located at the project root or package folder.
   - Provides an assertion API (`UiContextAssert.NoErrors(prefabPath)`) for easy integration into Unity EditMode test suites.
3. **Markdown Summary Reporter:** In addition to SARIF, the engine outputs a human- and agent-friendly Markdown summary (`ui-validation.md`) for instant chat rendering.

## Alternatives Rejected
- **Custom Proprietary JSON Format:**
  - *Reason for Rejection:* Incompatible with GitHub Actions code scanning annotations (`github/codeql-action/upload-sarif`) and requires custom parser logic in every agent.
- **Console Log Text Only:**
  - *Reason for Rejection:* Easily lost in Unity Editor console spam and difficult for AI agents to parse reliably.

## Consequences
- Validation results can be uploaded directly to GitHub Code Scanning on pull requests.
- Agents receive unambiguous, structured diagnostics with exact node IDs and remediation advice.

# Contributing to unity-ui-agent-tools

Thank you for your interest in contributing to `unity-ui-agent-tools`! We welcome contributions, bug fixes, and suggestions to make Unity UI development more accessible to AI coding agents and human developers alike.

## Code of Conduct
All participants must adhere to our [Code of Conduct](CODE_OF_CONDUCT.md) (Contributor Covenant 2.1).

## Clean-Room Rule
This repository operates under a strict clean-room standard:
- Do not submit proprietary code, commercial assets, private client names, or confidential identifiers.
- All contributions must be original works authored by you or derived from permissive MIT-compatible sources with attribution in `NOTICE`.

## Development Guidelines
1. **Architectural Isolation:** All logic in `Core/` must remain pure C# with **zero dependencies on `UnityEngine`** or `UnityEditor`.
2. **Conventional Commits:** Use standard Conventional Commit prefixes: `feat:`, `fix:`, `docs:`, `test:`, `ci:`, `chore:`.
3. **Unity Meta Integrity:** Any asset or folder added under `Packages/` or `Projects/` must include a valid matching `.meta` file.
4. **Local Verification:**
   Before submitting a pull request, run the verification runner:
   ```powershell
   powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify.ps1 full
   ```
   Or on Linux/macOS:
   ```bash
   ./scripts/verify.sh full
   ```

## Pull Request Process
1. Fork the repository and create a feature branch (`feat/your-feature-name`).
2. Implement your changes, adding automated tests in `tests/UiContext.Core.Tests/` or `cli/tests/`.
3. Ensure CI passes with 100% green status.
4. Open a pull request against `main`.

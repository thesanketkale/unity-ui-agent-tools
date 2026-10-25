## Description
<!-- Provide a brief description of the changes introduced by this pull request. -->

## Changes Checklist
- [ ] Conforms to Conventional Commits format (`feat:`, `fix:`, `docs:`, `test:`, `ci:`).
- [ ] Core changes contain zero references to `UnityEngine`.
- [ ] All 9 UI validation rules and tests pass (`dotnet test`, `npm test`).
- [ ] Clean-room check passes (`tools/guard.ps1` / `tools/guard.sh`).
- [ ] Any added Unity asset has a corresponding `.meta` file.

## Verification
- CI Status: <!-- Green / Red -->
- Verified locally via: `powershell -File scripts/verify.ps1 full`

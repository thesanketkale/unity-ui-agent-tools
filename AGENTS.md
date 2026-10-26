# AGENTS.md: unity-ui-agent-tools

Shared rules for every AI agent on this repo, in Claude Code (through CLAUDE.md) and Google Antigravity (which reads this file directly). Keep it under ~150 lines. It is read at the start of every run. Write `TBD` rather than guessing.

## Project
- What it is: Unity editor package that exports uGUI hierarchies, prefab structure and annotated screenshots as agent-readable artefacts, with UI validation rules, a semantic diff and an MCP adapter for AI coding agents.
- Users: Unity developers, technical artists, and AI coding agents inspecting, auditing, and modifying Unity UI hierarchies.
- Related repos and contracts:
  - `unity-openxr-foundation` (downstream consumer of UI tools)
  - `unity-build-pipeline` (CI test and build automation)
  - Schemas in `schema/` (`ui-tree.schema.json`, `ui-validation.schema.json`, `ui-diff.schema.json`, `config.schema.json`)

## Stack packs
<!-- One per line. Agents load {skills dir}/stack-<name>/SKILL.md for each. -->
- unity-xr
- github-actions

## Commands
- Install: `npm --prefix cli install` and `dotnet restore tests/UiContext.Core.Tests`
- Run locally: none. The CLI runs as `node cli/dist/index.js`
- Verify fast: `bash scripts/verify.sh fast` (Windows PowerShell: `./scripts/verify.ps1 fast`)
- Verify full: `bash scripts/verify.sh full`
- Disposable database: none

## Git
- Base branch: main
- Branches: `feat/<slug>`, `fix/<slug>`, `chore/<slug>`
- PR: manual

## Conventions
- This is a public MIT repository written clean-room: no code, assets, names or data from client or employer projects. Sample data is synthetic.
- The standards and this repository's requirements are in `.pipeline/SPEC.md` (local, never committed). Requirement IDs appear in AC lines as `AC-n [ID]` and in test names.
- Docs describe only what exists and was run. What an agent cannot run (push, live cloud, signing, a Unity Editor, GitHub settings) is a `[USER]` item in `docs/release-checklist.md` and is never described as tested.
- GitHub Actions: top-level `permissions: {}` plus minimal per-job permissions; `timeout-minutes` and `concurrency` on every workflow; actions pinned to a full SHA with the version in a comment; explicit `shell:`; LF line endings; no `${{ github.event.* }}` inside `run:` (use `env:`); OIDC for cloud login. Check with `actionlint`.
- Commits are small and follow `<type>(<scope>): <goal> [step N]`. Dates are real and history is never rewritten.
- Dependencies need MIT-compatible licences. Record stack choices and open-decision outcomes in `docs/adr/`.
- Everything that decides anything lives in `Core` (plain C#, an assembly with `noEngineReferences`) and works on `UiNode`. `tests/Core.Tests/Core.Tests.csproj` (or `tests/UiContext.Core.Tests`) compiles the same source files from the package folder, so the Unity package and `dotnet test` share one copy of the code. `Editor` only walks Unity objects, fills the model and renders.
- Output is deterministic: sorted keys, stable ordering, stable node IDs from the hierarchy path, and no timestamps or machine paths. Schemas in `schema/` are versioned. A change to an output shape changes the schema version and the golden files together.
- `cli/` is strict TypeScript, ESM, with Vitest, an ESLint flat config and Prettier. The MCP server uses the official MCP SDK over stdio and writes nothing but protocol messages to stdout.
- Every rule (`IUiRule`) has a passing fixture, a failing fixture and an entry in `docs/rules.md`.
- Unity-side code compiles only in Unity. Without `UNITY_PATH`, agents write it carefully, report it INCOMPLETE and leave the proof to the `[USER]` items. Every `.meta` is generated with `scripts/gen-meta.mjs`.
- The package id is `io.github.thesanketkale.ui-context`. Supported: Unity 2022.3 LTS and Unity 6 LTS, uGUI with TextMeshPro.
- The sample UI in `Projects/Sample/` is invented: generated sprites and made-up content.

## Design
- Direction: None (`--no-design`). The sample UI in `Projects/Sample/` is invented: generated sprites and made-up content.

## Safety: agents never do these. They prepare the command and hand it over.
- Deploy, publish, release, upload, sign or notarize builds
- `terraform` / `terragrunt` apply, destroy, import or any state command
- Any write to a database other than the disposable one above
- Change real secrets, `.env` values, CI/CD or production configuration
- Push to the base branch, force-push, rewrite published history
- Never push, tag, create releases, change GitHub settings or run `git filter-repo`; write the command into `docs/release-checklist.md` instead. Never print secret values: show only file, line, rule and the last four characters.
- Open the Unity Editor GUI or execute non-batchmode interactive commands

## Known baseline failures
<!-- Pre-existing failures moved out of fast, with a note on why. Remove as they get fixed. -->

## Lessons
<!-- One line each, added only with the user's approval after a review found a repeatable mistake.
     Format: - [area] Do X, not Y, because Z. -->

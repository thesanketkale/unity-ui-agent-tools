# ADR-0001: Technology Stack and Repository Structure Selection

## Status
Accepted

## Date
2026-10-25

## Context
AI agents need to introspect, validate, and compare Unity UI hierarchies (uGUI and TextMeshPro) across both hosted CI runners and local developer environments. We need to decide on:
1. The packaging and distribution format for the Unity package.
2. The implementation language and runtime for core validation and diff logic.
3. The interface mechanism between AI coding agents (Claude Code, Codex, Gemini) and Unity.

## Decision
1. **Unity Package Manager (UPM) Git Package:** We package the tool as a standard UPM package under `Packages/io.github.thesanketkale.ui-context/`, installable via Git URL with subpath reference and SemVer tags (`v0.1.0`).
2. **Pure C# Core Engine (`netstandard2.1` / `net8.0`):** Core data structures, validation rules, diffing algorithms, and serialization reside in an isolated `Core/` assembly with **zero references to UnityEngine**.
3. **TypeScript CLI and Model Context Protocol (MCP) Server:** We build the agent interface using TypeScript, Node.js 18+, and the official `@modelcontextprotocol/sdk` over `stdio`, distributed in `cli/`.

## Alternatives Rejected
- **Monolithic In-Editor C# Package:** Placing all validation and diff algorithms directly in `UnityEditor` assemblies.
  - *Reason for Rejection:* Requires running full Unity batchmode just to run unit tests, validate schemas, or compare snapshots. This prevents fast testing on lightweight hosted CI runners (GitHub-hosted Linux/Windows runners).
- **Python-Based CLI & Daemon:** Using Python for the MCP server and diff CLI.
  - *Reason for Rejection:* Increases prerequisite burden for game developers, who standardly have Node.js and .NET SDKs already installed. TypeScript provides end-to-end schema synchronization with JSON Schema and native compatibility with the official MCP SDK.
- **Direct Unity Prefab YAML Parsing in TypeScript:** Having the CLI parse `.prefab` YAML files directly without Unity.
  - *Reason for Rejection:* Unity's serialized YAML changes across engine versions, omits calculated canvas pixel dimensions, fails to resolve CanvasScaler reference ratios, and cannot determine dynamic layout group sizes. Unity's editor API is required to resolve actual layout math accurately.

## Consequences
- Fast, deterministic testing: 100% of Core logic is tested via `dotnet test` in seconds on any OS.
- Developers can install the package into Unity 2022.3 LTS and Unity 6 projects without external dependencies.
- AI agents get instant, first-class MCP tool calling through a standard Node.js server.

#!/usr/bin/env node
import { Command } from "commander";
import fs from "node:fs";
import { computeDiff, formatDiffMarkdown } from "./diff-service.js";
import { parseSarifReport, formatSarifSummary } from "./sarif-service.js";
import { startMcpServer } from "./mcp-server.js";

const program = new Command();

program
  .name("ui-context")
  .description("Unity UI introspection, validation, diff, and MCP adapter for AI coding agents")
  .version("0.1.0");

program
  .command("diff")
  .description("Compute semantic difference between two ui-tree.json snapshots")
  .argument("<before>", "Path to before JSON snapshot")
  .argument("<after>", "Path to after JSON snapshot")
  .option("-j, --json", "Output structured JSON instead of Markdown")
  .action((beforePath, afterPath, options) => {
    if (!fs.existsSync(beforePath) || !fs.existsSync(afterPath)) {
      console.error(`Error: Snapshot file does not exist.`);
      process.exit(1);
    }

    const before = JSON.parse(fs.readFileSync(beforePath, "utf-8"));
    const after = JSON.parse(fs.readFileSync(afterPath, "utf-8"));
    const diff = computeDiff(before, after);

    if (options.json) {
      console.log(JSON.stringify(diff, null, 2));
    } else {
      console.log(formatDiffMarkdown(diff));
    }
  });

program
  .command("validate")
  .description("Inspect and display summary of ui-validation.json SARIF report")
  .argument("<sarifPath>", "Path to ui-validation.json file")
  .action((sarifPath) => {
    if (!fs.existsSync(sarifPath)) {
      console.error(`Error: File not found at ${sarifPath}`);
      process.exit(1);
    }
    const report = parseSarifReport(sarifPath);
    console.log(formatSarifSummary(report));
  });

program
  .command("mcp")
  .description("Run the Model Context Protocol (MCP) server over stdio")
  .action(async () => {
    await startMcpServer();
  });

program.parse(process.argv);

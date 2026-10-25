import fs from "node:fs";

export interface SarifResult {
  ruleId: string;
  level: "error" | "warning" | "note" | "none";
  message: { text: string };
  locations?: Array<{
    physicalLocation?: { artifactLocation?: { uri?: string } };
    logicalLocations?: Array<{ fullyQualifiedName?: string }>;
  }>;
}

export interface SarifReport {
  version: string;
  runs: Array<{
    tool: { driver: { name: string; version: string } };
    results: SarifResult[];
  }>;
}

export function parseSarifReport(filePath: string): SarifReport {
  const content = fs.readFileSync(filePath, "utf-8");
  return JSON.parse(content) as SarifReport;
}

export function formatSarifSummary(report: SarifReport): string {
  const results = report.runs[0]?.results ?? [];
  const errors = results.filter(r => r.level === "error").length;
  const warnings = results.filter(r => r.level === "warning").length;
  const notes = results.filter(r => r.level === "note").length;

  const lines: string[] = [];
  lines.push(`SARIF Validation Summary:`);
  lines.push(`  Errors:   ${errors}`);
  lines.push(`  Warnings: ${warnings}`);
  lines.push(`  Notes:    ${notes}`);
  lines.push("");

  if (results.length === 0) {
    lines.push("PASSED: Zero rule violations detected.");
    return lines.join("\n");
  }

  for (const r of results) {
    const loc = r.locations?.[0]?.logicalLocations?.[0]?.fullyQualifiedName ?? "unknown";
    lines.push(`  [${r.level.toUpperCase()}] ${r.ruleId} on ${loc}: ${r.message.text}`);
  }

  return lines.join("\n");
}

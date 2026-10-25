import { describe, it, expect } from "vitest";
import fs from "node:fs";
import path from "node:path";
import { parseSarifReport, formatSarifSummary } from "../src/sarif-service.js";

describe("SARIF Report Parser and Formatter Tests", () => {
  const fixturePath = path.resolve(__dirname, "../../tests/fixtures/golden-hud-validation.json");

  it("parses SARIF 2.1.0 report correctly", () => {
    const report = parseSarifReport(fixturePath);
    expect(report.version).toBe("2.1.0");
    expect(report.runs[0].tool.driver.name).toBe("UiContext");
  });

  it("formats SARIF clean summary correctly", () => {
    const report = parseSarifReport(fixturePath);
    const summary = formatSarifSummary(report);
    expect(summary).toContain("SARIF Validation Summary:");
    expect(summary).toContain("PASSED: Zero rule violations detected.");
  });
});

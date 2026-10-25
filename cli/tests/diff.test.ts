import { describe, it, expect } from "vitest";
import { computeDiff, formatDiffMarkdown, UiTree } from "../src/diff-service.js";

describe("Semantic Diff Service Tests", () => {
  it("detects added, removed, and resized elements", () => {
    const before: UiTree = {
      targetName: "Dialog",
      root: {
        id: "/Dialog",
        name: "Dialog",
        activeSelf: true,
        rect: { resolvedRect: { x: 0, y: 0, width: 400, height: 300 } },
        components: [],
        children: [
          {
            id: "/Dialog/OldBtn",
            name: "OldBtn",
            activeSelf: true,
            rect: { resolvedRect: { x: 10, y: 10, width: 100, height: 40 } },
            components: [],
            children: []
          }
        ]
      }
    };

    const after: UiTree = {
      targetName: "Dialog",
      root: {
        id: "/Dialog",
        name: "Dialog",
        activeSelf: true,
        rect: { resolvedRect: { x: 0, y: 0, width: 500, height: 350 } }, // Resized
        components: [],
        children: [
          {
            id: "/Dialog/NewBtn",
            name: "NewBtn",
            activeSelf: true,
            rect: { resolvedRect: { x: 10, y: 10, width: 120, height: 48 } }, // Added
            components: [],
            children: []
          }
        ]
      }
    };

    const diff = computeDiff(before, after);
    expect(diff.changes.length).toBeGreaterThanOrEqual(3);

    const added = diff.changes.find(c => c.changeType === "Added");
    expect(added).toBeDefined();
    expect(added?.nodeName).toBe("NewBtn");

    const removed = diff.changes.find(c => c.changeType === "Removed");
    expect(removed).toBeDefined();
    expect(removed?.nodeName).toBe("OldBtn");

    const resized = diff.changes.find(c => c.changeType === "Resized");
    expect(resized).toBeDefined();
    expect(resized?.nodeName).toBe("Dialog");

    const md = formatDiffMarkdown(diff);
    expect(md).toContain("# UI Semantic Diff: Dialog -> Dialog");
    expect(md).toContain("NewBtn");
    expect(md).toContain("OldBtn");
  });
});

import fs from "node:fs";

export interface UiNode {
  id: string;
  name: string;
  activeSelf: boolean;
  rect: {
    resolvedRect: {
      x: number;
      y: number;
      width: number;
      height: number;
    };
    anchoredPosition?: { x: number; y: number };
  };
  components: Array<{
    typeName: string;
    properties?: Record<string, unknown>;
  }>;
  children: UiNode[];
}

export interface UiTree {
  targetName: string;
  root: UiNode;
}

export interface DiffChange {
  nodeId: string;
  nodeName: string;
  changeType: string;
  description: string;
  beforeValue?: string | null;
  afterValue?: string | null;
}

export interface DiffReport {
  targetNameBefore: string;
  targetNameAfter: string;
  timestamp: string;
  changes: DiffChange[];
}

function flattenNodes(node: UiNode, map: Map<string, UiNode>): void {
  map.set(node.id, node);
  if (node.children) {
    for (const child of node.children) {
      flattenNodes(child, map);
    }
  }
}

export function computeDiff(beforeTree: UiTree, afterTree: UiTree): DiffReport {
  const beforeMap = new Map<string, UiNode>();
  const afterMap = new Map<string, UiNode>();

  if (beforeTree.root) flattenNodes(beforeTree.root, beforeMap);
  if (afterTree.root) flattenNodes(afterTree.root, afterMap);

  const changes: DiffChange[] = [];

  // Added
  for (const [id, afterNode] of afterMap.entries()) {
    if (!beforeMap.has(id)) {
      changes.push({
        nodeId: id,
        nodeName: afterNode.name,
        changeType: "Added",
        description: `Node '${afterNode.name}' was added to hierarchy at '${id}'.`,
        afterValue: `Size: ${Math.round(afterNode.rect.resolvedRect.width)}x${Math.round(afterNode.rect.resolvedRect.height)}`
      });
    }
  }

  // Removed
  for (const [id, beforeNode] of beforeMap.entries()) {
    if (!afterMap.has(id)) {
      changes.push({
        nodeId: id,
        nodeName: beforeNode.name,
        changeType: "Removed",
        description: `Node '${beforeNode.name}' was removed from hierarchy.`,
        beforeValue: `Size: ${Math.round(beforeNode.rect.resolvedRect.width)}x${Math.round(beforeNode.rect.resolvedRect.height)}`
      });
    }
  }

  // Modified
  for (const [id, beforeNode] of beforeMap.entries()) {
    const afterNode = afterMap.get(id);
    if (!afterNode) continue;

    // Active
    if (beforeNode.activeSelf !== afterNode.activeSelf) {
      changes.push({
        nodeId: id,
        nodeName: beforeNode.name,
        changeType: "ActiveStateChanged",
        description: `Active state changed from ${beforeNode.activeSelf} to ${afterNode.activeSelf}.`,
        beforeValue: String(beforeNode.activeSelf),
        afterValue: String(afterNode.activeSelf)
      });
    }

    // Size
    const bRect = beforeNode.rect.resolvedRect;
    const aRect = afterNode.rect.resolvedRect;
    if (Math.abs(bRect.width - aRect.width) > 0.5 || Math.abs(bRect.height - aRect.height) > 0.5) {
      changes.push({
        nodeId: id,
        nodeName: beforeNode.name,
        changeType: "Resized",
        description: `Size changed from ${Math.round(bRect.width)}x${Math.round(bRect.height)} to ${Math.round(aRect.width)}x${Math.round(aRect.height)}.`,
        beforeValue: `${Math.round(bRect.width)}x${Math.round(bRect.height)}`,
        afterValue: `${Math.round(aRect.width)}x${Math.round(aRect.height)}`
      });
    }
  }

  return {
    targetNameBefore: beforeTree.targetName,
    targetNameAfter: afterTree.targetName,
    timestamp: new Date().toISOString(),
    changes
  };
}

export function formatDiffMarkdown(report: DiffReport): string {
  const lines: string[] = [];
  lines.push(`# UI Semantic Diff: ${report.targetNameBefore} -> ${report.targetNameAfter}`);
  lines.push(`*Generated at: ${report.timestamp}*`);
  lines.push("");
  lines.push(`**Total Changes:** ${report.changes.length}`);
  lines.push("");

  if (report.changes.length === 0) {
    lines.push("No semantic differences detected between UI snapshots.");
    return lines.join("\n");
  }

  lines.push("| Type | Node | Description | Before | After |");
  lines.push("|---|---|---|---|---|");

  for (const c of report.changes) {
    const before = (c.beforeValue ?? "-").replace(/\|/g, "\\|");
    const after = (c.afterValue ?? "-").replace(/\|/g, "\\|");
    const desc = c.description.replace(/\|/g, "\\|");
    lines.push(`| ${c.changeType} | \`${c.nodeName}\` | ${desc} | \`${before}\` | \`${after}\` |`);
  }

  return lines.join("\n");
}

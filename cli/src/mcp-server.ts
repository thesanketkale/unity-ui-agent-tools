import { Server } from "@modelcontextprotocol/sdk/server/index.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import {
  CallToolRequestSchema,
  ListToolsRequestSchema,
  Tool
} from "@modelcontextprotocol/sdk/types.js";
import fs from "node:fs";
import path from "node:path";
import { computeDiff, formatDiffMarkdown } from "./diff-service.js";
import { parseSarifReport, formatSarifSummary } from "./sarif-service.js";

const server = new Server(
  {
    name: "unity-ui-context",
    version: "0.1.0"
  },
  {
    capabilities: {
      tools: {}
    }
  }
);

const DESCRIBE_TOOL: Tool = {
  name: "describe_ui",
  description: "Returns a compact, token-efficient Markdown outline and component structure of a Unity UI prefab or exported hierarchy.",
  inputSchema: {
    type: "object",
    properties: {
      targetPath: {
        type: "string",
        description: "Path to exported ui-outline.md, ui-tree.json, or directory containing export artifacts (defaults to ui-context-out)."
      }
    }
  }
};

const VALIDATE_TOOL: Tool = {
  name: "validate_ui",
  description: "Evaluates UI validation rules and returns a structured diagnostic report (SARIF format) with exact node locators.",
  inputSchema: {
    type: "object",
    properties: {
      targetPath: {
        type: "string",
        description: "Path to exported ui-validation.json or directory containing export artifacts."
      }
    }
  }
};

const DIFF_TOOL: Tool = {
  name: "diff_ui",
  description: "Computes a semantic difference report between two UI tree snapshots (detects moved, resized, text changed, added, removed).",
  inputSchema: {
    type: "object",
    required: ["beforePath", "afterPath"],
    properties: {
      beforePath: {
        type: "string",
        description: "Path to before ui-tree.json snapshot."
      },
      afterPath: {
        type: "string",
        description: "Path to after ui-tree.json snapshot."
      }
    }
  }
};

const CAPTURE_TOOL: Tool = {
  name: "capture_ui",
  description: "Retrieves screenshot capture metadata and numbered bounding box legend for visual verification.",
  inputSchema: {
    type: "object",
    properties: {
      targetPath: {
        type: "string",
        description: "Path to captures.json or captures directory."
      }
    }
  }
};

server.setRequestHandler(ListToolsRequestSchema, async () => {
  return {
    tools: [DESCRIBE_TOOL, VALIDATE_TOOL, DIFF_TOOL, CAPTURE_TOOL]
  };
});

server.setRequestHandler(CallToolRequestSchema, async (request) => {
  const { name, arguments: args } = request.params;

  try {
    switch (name) {
      case "describe_ui": {
        const targetPath = (args?.targetPath as string) || "ui-context-out";
        let resolvedPath = targetPath;
        if (fs.existsSync(targetPath) && fs.statSync(targetPath).isDirectory()) {
          const mdPath = path.join(targetPath, "ui-outline.md");
          const jsonPath = path.join(targetPath, "ui-tree.json");
          if (fs.existsSync(mdPath)) resolvedPath = mdPath;
          else if (fs.existsSync(jsonPath)) resolvedPath = jsonPath;
        }

        if (!fs.existsSync(resolvedPath)) {
          return {
            content: [{ type: "text", text: `Artifact not found at '${resolvedPath}'. Run 'ui-context export' first or verify export path.` }]
          };
        }

        const content = fs.readFileSync(resolvedPath, "utf-8");
        return {
          content: [{ type: "text", text: content }]
        };
      }

      case "validate_ui": {
        const targetPath = (args?.targetPath as string) || "ui-context-out";
        let resolvedPath = targetPath;
        if (fs.existsSync(targetPath) && fs.statSync(targetPath).isDirectory()) {
          resolvedPath = path.join(targetPath, "ui-validation.json");
        }

        if (!fs.existsSync(resolvedPath)) {
          return {
            content: [{ type: "text", text: `Validation report not found at '${resolvedPath}'. Run export first.` }]
          };
        }

        const report = parseSarifReport(resolvedPath);
        const summary = formatSarifSummary(report);
        return {
          content: [{ type: "text", text: summary }]
        };
      }

      case "diff_ui": {
        const before = args?.beforePath as string;
        const after = args?.afterPath as string;
        if (!fs.existsSync(before) || !fs.existsSync(after)) {
          return {
            content: [{ type: "text", text: `Snapshot paths invalid: before='${before}', after='${after}'` }]
          };
        }

        const beforeTree = JSON.parse(fs.readFileSync(before, "utf-8"));
        const afterTree = JSON.parse(fs.readFileSync(after, "utf-8"));
        const diffReport = computeDiff(beforeTree, afterTree);
        const md = formatDiffMarkdown(diffReport);

        return {
          content: [{ type: "text", text: md }]
        };
      }

      case "capture_ui": {
        const targetPath = (args?.targetPath as string) || "ui-context-out";
        let resolvedPath = targetPath;
        if (fs.existsSync(targetPath) && fs.statSync(targetPath).isDirectory()) {
          resolvedPath = path.join(targetPath, "captures.json");
        }

        if (!fs.existsSync(resolvedPath)) {
          return {
            content: [{ type: "text", text: `Captures metadata not found at '${resolvedPath}'. Note: Visual screenshot capture requires active GPU graphics device.` }]
          };
        }

        const content = fs.readFileSync(resolvedPath, "utf-8");
        return {
          content: [{ type: "text", text: content }]
        };
      }

      default:
        throw new Error(`Unknown tool: ${name}`);
    }
  } catch (err: unknown) {
    const msg = err instanceof Error ? err.message : String(err);
    return {
      content: [{ type: "text", text: `Error executing ${name}: ${msg}` }],
      isError: true
    };
  }
});

export async function startMcpServer(): Promise<void> {
  const transport = new StdioServerTransport();
  await server.connect(transport);
  process.stderr.write("[unity-ui-context] MCP server started on stdio.\n");
}

if (process.argv[1]?.endsWith("mcp-server.js") || process.argv.includes("--mcp")) {
  startMcpServer().catch(err => {
    process.stderr.write(`Failed to start MCP server: ${err}\n`);
    process.exit(1);
  });
}

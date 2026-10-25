using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace UiContext.Core.Validation;

/// <summary>
/// Exporter producing SARIF 2.1.0 diagnostic reports and Markdown summaries.
/// </summary>
public static class SarifExporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string ToSarifJson(ValidationResult result, string toolVersion = "0.1.0")
    {
        var rulesMap = new Dictionary<string, string>();
        foreach (var issue in result.Issues)
        {
            if (!rulesMap.ContainsKey(issue.RuleId))
            {
                rulesMap[issue.RuleId] = issue.RuleName;
            }
        }

        var sarifRules = rulesMap.Select(kv => new
        {
            id = kv.Key,
            name = kv.Value,
            shortDescription = new { text = kv.Value }
        }).ToList();

        var sarifResults = result.Issues.Select(issue => new
        {
            ruleId = issue.RuleId,
            level = issue.Severity switch
            {
                IssueSeverity.Error => "error",
                IssueSeverity.Warning => "warning",
                _ => "note"
            },
            message = new { text = issue.Message },
            locations = new[]
            {
                new
                {
                    physicalLocation = new
                    {
                        artifactLocation = new
                        {
                            uri = string.IsNullOrEmpty(issue.AssetPath) ? result.TargetName : issue.AssetPath
                        }
                    },
                    logicalLocations = new[]
                    {
                        new
                        {
                            fullyQualifiedName = issue.NodeId,
                            kind = "node"
                        }
                    }
                }
            },
            properties = issue.Details
        }).ToList();

        var document = new
        {
            schema = "https://json.schemastore.org/sarif-2.1.0.json",
            version = "2.1.0",
            runs = new[]
            {
                new
                {
                    tool = new
                    {
                        driver = new
                        {
                            name = "UiContext",
                            version = toolVersion,
                            informationUri = "https://github.com/thesanketkale/unity-ui-agent-tools",
                            rules = sarifRules
                        }
                    },
                    results = sarifResults
                }
            }
        };

        return JsonSerializer.Serialize(document, JsonOptions);
    }

    public static string ToMarkdownSummary(ValidationResult result)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# UI Validation Report: {result.TargetName}");
        sb.AppendLine($"**Status:** {(result.HasErrors ? "FAILED" : (result.WarningCount > 0 ? "WARNINGS" : "PASSED"))}");
        sb.AppendLine($"- Errors: {result.ErrorCount}");
        sb.AppendLine($"- Warnings: {result.WarningCount}");
        sb.AppendLine($"- Notes: {result.NoteCount}");
        sb.AppendLine();

        if (result.Issues.Count == 0)
        {
            sb.AppendLine("No issues detected. UI structure conforms to all active rules.");
            return sb.ToString();
        }

        sb.AppendLine("| Severity | Rule | Node | Message |");
        sb.AppendLine("|---|---|---|---|");

        foreach (var issue in result.Issues)
        {
            string badge = issue.Severity switch
            {
                IssueSeverity.Error => "🔴 Error",
                IssueSeverity.Warning => "🟡 Warning",
                _ => "🔵 Note"
            };
            sb.AppendLine($"| {badge} | `{issue.RuleId}` | `{issue.NodeName}` | {issue.Message.Replace("|", "\\|")} |");
        }

        return sb.ToString();
    }
}

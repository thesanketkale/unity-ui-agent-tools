using System;
using System.Collections.Generic;
using System.Linq;
using UiContext.Core.Config;
using UiContext.Core.Models;
using UiContext.Core.Validation;

namespace UiContext.Core.Rules;

/// <summary>
/// UI008: Duplicate sibling names prevent deterministic path-based hierarchy addressing.
/// </summary>
public class UI008_DuplicateSiblingNamesRule : IUiRule
{
    public string RuleId => "UI008";
    public string Name => "DuplicateSiblingNames";
    public string Description => "Detects siblings sharing identical names under the same parent transform.";
    public IssueSeverity DefaultSeverity => IssueSeverity.Warning;

    public IEnumerable<ValidationIssue> Evaluate(HierarchyTree tree, UiContextConfig config)
    {
        if (tree.Root == null || !config.FailOnDuplicateSiblingNames) return [];

        var issues = new List<ValidationIssue>();
        CheckDuplicates(tree.Root, issues);
        return issues;
    }

    private void CheckDuplicates(UiNode node, List<ValidationIssue> issues)
    {
        var grouped = node.Children
            .GroupBy(c => c.Name, StringComparer.Ordinal)
            .Where(g => g.Count() > 1);

        foreach (var group in grouped)
        {
            foreach (var duplicate in group)
            {
                issues.Add(new ValidationIssue
                {
                    RuleId = RuleId,
                    RuleName = Name,
                    Severity = DefaultSeverity,
                    NodeId = duplicate.Id,
                    NodeName = duplicate.Name,
                    AssetPath = duplicate.PrefabInfo.SourceAssetPath,
                    Message = $"Parent '{node.Name}' contains {group.Count()} children named '{duplicate.Name}'. Duplicate names prevent reliable locator addressing.",
                    Details = new Dictionary<string, string>
                    {
                        ["duplicateCount"] = group.Count().ToString(),
                        ["parentNodeId"] = node.Id
                    }
                });
            }
        }

        foreach (var child in node.Children)
        {
            CheckDuplicates(child, issues);
        }
    }
}

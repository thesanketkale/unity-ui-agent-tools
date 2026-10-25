using System;
using System.Collections.Generic;
using System.Linq;
using UiContext.Core.Config;
using UiContext.Core.Models;
using UiContext.Core.Validation;

namespace UiContext.Core.Rules;

/// <summary>
/// UI007: Deeply nested layout groups cause recursive recalculation spikes.
/// </summary>
public class UI007_DeepLayoutNestingRule : IUiRule
{
    public string RuleId => "UI007";
    public string Name => "DeepLayoutGroupNesting";
    public string Description => "Flags layout group hierarchies exceeding safe nesting depth limits.";
    public IssueSeverity DefaultSeverity => IssueSeverity.Note;

    public IEnumerable<ValidationIssue> Evaluate(HierarchyTree tree, UiContextConfig config)
    {
        if (tree.Root == null) return [];

        var issues = new List<ValidationIssue>();
        TraverseLayoutDepth(tree.Root, 0, config.MaxLayoutGroupDepth, issues);
        return issues;
    }

    private void TraverseLayoutDepth(UiNode node, int currentDepth, int maxDepth, List<ValidationIssue> issues)
    {
        bool isLayoutGroup = node.HasComponent("VerticalLayoutGroup") ||
                             node.HasComponent("HorizontalLayoutGroup") ||
                             node.HasComponent("GridLayoutGroup");

        int nextDepth = isLayoutGroup ? currentDepth + 1 : currentDepth;

        if (isLayoutGroup && nextDepth > maxDepth)
        {
            issues.Add(new ValidationIssue
            {
                RuleId = RuleId,
                RuleName = Name,
                Severity = DefaultSeverity,
                NodeId = node.Id,
                NodeName = node.Name,
                AssetPath = node.PrefabInfo.SourceAssetPath,
                Message = $"Layout group on '{node.Name}' has nesting depth of {nextDepth}, exceeding recommended limit of {maxDepth} levels.",
                Details = new Dictionary<string, string>
                {
                    ["currentDepth"] = nextDepth.ToString(),
                    ["maxDepth"] = maxDepth.ToString()
                }
            });
        }

        foreach (var child in node.Children)
        {
            TraverseLayoutDepth(child, nextDepth, maxDepth, issues);
        }
    }
}

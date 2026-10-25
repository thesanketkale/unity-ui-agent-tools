using System;
using System.Collections.Generic;
using System.Linq;
using UiContext.Core.Config;
using UiContext.Core.Models;
using UiContext.Core.Validation;

namespace UiContext.Core.Rules;

/// <summary>
/// UI009: Non-zero offsets or explicit anchors on child elements inside auto-layout groups.
/// </summary>
public class UI009_AbsolutePositionInLayoutRule : IUiRule
{
    public string RuleId => "UI009";
    public string Name => "AbsolutePositionInLayoutGroup";
    public string Description => "Flags manual offsets or anchor definitions inside LayoutGroups that override them.";
    public IssueSeverity DefaultSeverity => IssueSeverity.Warning;

    public IEnumerable<ValidationIssue> Evaluate(HierarchyTree tree, UiContextConfig config)
    {
        if (tree.Root == null) return [];

        var issues = new List<ValidationIssue>();
        CheckLayoutChildren(tree.Root, issues);
        return issues;
    }

    private void CheckLayoutChildren(UiNode parent, List<ValidationIssue> issues)
    {
        bool isLayoutGroup = parent.HasComponent("VerticalLayoutGroup") ||
                             parent.HasComponent("HorizontalLayoutGroup") ||
                             parent.HasComponent("GridLayoutGroup");

        if (isLayoutGroup)
        {
            foreach (var child in parent.Children)
            {
                var layoutElement = child.GetComponent("LayoutElement");
                bool ignoreLayout = layoutElement?.GetProperty("ignoreLayout", false) ?? false;
                if (ignoreLayout) continue;

                var pos = child.Rect.AnchoredPosition;
                if (Math.Abs(pos.X) > 0.01f || Math.Abs(pos.Y) > 0.01f)
                {
                    issues.Add(new ValidationIssue
                    {
                        RuleId = RuleId,
                        RuleName = Name,
                        Severity = DefaultSeverity,
                        NodeId = child.Id,
                        NodeName = child.Name,
                        AssetPath = child.PrefabInfo.SourceAssetPath,
                        Message = $"Child '{child.Name}' has manual anchoredPosition ({pos.X:0.0}, {pos.Y:0.0}) inside '{parent.Name}' LayoutGroup without LayoutElement(ignoreLayout=true). Position will be overwritten at runtime.",
                        Details = new Dictionary<string, string>
                        {
                            ["anchoredPosition"] = $"({pos.X:0.0}, {pos.Y:0.0})",
                            ["parentLayoutGroup"] = parent.Id
                        }
                    });
                }
            }
        }

        foreach (var child in parent.Children)
        {
            CheckLayoutChildren(child, issues);
        }
    }
}

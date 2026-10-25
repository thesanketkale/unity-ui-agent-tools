using System;
using System.Collections.Generic;
using System.Linq;
using UiContext.Core.Config;
using UiContext.Core.Models;
using UiContext.Core.Validation;

namespace UiContext.Core.Rules;

/// <summary>
/// UI003: Interactive targets must meet minimum physical touch/click dimensions.
/// </summary>
public class UI003_MinimumTargetSizeRule : IUiRule
{
    public string RuleId => "UI003";
    public string Name => "MinimumTargetSize";
    public string Description => "Validates that touch/click hit targets meet accessible size standards.";
    public IssueSeverity DefaultSeverity => IssueSeverity.Warning;

    public IEnumerable<ValidationIssue> Evaluate(HierarchyTree tree, UiContextConfig config)
    {
        if (tree.Root == null) yield break;

        float minWidth = config.MinInteractableWidth;
        float minHeight = config.MinInteractableHeight;

        foreach (var node in tree.Root.Flatten().Where(n => n.ActiveInHierarchy && n.IsInteractable()))
        {
            var r = node.Rect.ResolvedRect;
            if (r.Width < minWidth || r.Height < minHeight)
            {
                yield return new ValidationIssue
                {
                    RuleId = RuleId,
                    RuleName = Name,
                    Severity = DefaultSeverity,
                    NodeId = node.Id,
                    NodeName = node.Name,
                    AssetPath = node.PrefabInfo.SourceAssetPath,
                    Message = $"Interactive element '{node.Name}' size ({r.Width:0}x{r.Height:0}) is smaller than target standard ({minWidth:0}x{minHeight:0}) for {config.TargetPlatform} profile.",
                    Details = new Dictionary<string, string>
                    {
                        ["resolvedWidth"] = r.Width.ToString("0.0"),
                        ["resolvedHeight"] = r.Height.ToString("0.0"),
                        ["requiredWidth"] = minWidth.ToString("0.0"),
                        ["requiredHeight"] = minHeight.ToString("0.0")
                    }
                };
            }
        }
    }
}

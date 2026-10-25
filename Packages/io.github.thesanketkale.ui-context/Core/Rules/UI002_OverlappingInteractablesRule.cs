using System;
using System.Collections.Generic;
using System.Linq;
using UiContext.Core.Config;
using UiContext.Core.Models;
using UiContext.Core.Validation;

namespace UiContext.Core.Rules;

/// <summary>
/// UI002: Overlapping interactive hitboxes that can intercept or conflict with user clicks.
/// </summary>
public class UI002_OverlappingInteractablesRule : IUiRule
{
    public string RuleId => "UI002";
    public string Name => "OverlappingInteractables";
    public string Description => "Detects multiple active interactive elements that visually overlap on screen.";
    public IssueSeverity DefaultSeverity => IssueSeverity.Error;

    public IEnumerable<ValidationIssue> Evaluate(HierarchyTree tree, UiContextConfig config)
    {
        if (tree.Root == null) yield break;

        var interactables = tree.Root.Flatten()
            .Where(n => n.ActiveInHierarchy && n.IsInteractable())
            .ToList();

        for (int i = 0; i < interactables.Count; i++)
        {
            var a = interactables[i];
            var rectA = a.Rect.ResolvedRect;
            if (rectA.Width <= 1f || rectA.Height <= 1f) continue;

            for (int j = i + 1; j < interactables.Count; j++)
            {
                var b = interactables[j];
                var rectB = b.Rect.ResolvedRect;
                if (rectB.Width <= 1f || rectB.Height <= 1f) continue;

                // Ignore parent-child relationships where inner button or toggle is embedded intentionally
                if (a.Id.StartsWith(b.Id + "/") || b.Id.StartsWith(a.Id + "/")) continue;

                if (rectA.Intersects(rectB))
                {
                    yield return new ValidationIssue
                    {
                        RuleId = RuleId,
                        RuleName = Name,
                        Severity = DefaultSeverity,
                        NodeId = a.Id,
                        NodeName = a.Name,
                        AssetPath = a.PrefabInfo.SourceAssetPath,
                        Message = $"Interactive element '{a.Name}' ({a.Id}) visually overlaps with '{b.Name}' ({b.Id}) on screen.",
                        Details = new Dictionary<string, string>
                        {
                            ["conflictingNodeId"] = b.Id,
                            ["rectA"] = $"{rectA.Width:0}x{rectA.Height:0}@{rectA.X:0},{rectA.Y:0}",
                            ["rectB"] = $"{rectB.Width:0}x{rectB.Height:0}@{rectB.X:0},{rectB.Y:0}"
                        }
                    };
                }
            }
        }
    }
}

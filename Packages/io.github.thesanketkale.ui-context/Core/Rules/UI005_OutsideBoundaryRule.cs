using System;
using System.Collections.Generic;
using System.Linq;
using UiContext.Core.Config;
using UiContext.Core.Models;
using UiContext.Core.Validation;

namespace UiContext.Core.Rules;

/// <summary>
/// UI005: Visual elements positioned outside the canvas or display viewport.
/// </summary>
public class UI005_OutsideBoundaryRule : IUiRule
{
    public string RuleId => "UI005";
    public string Name => "OutsideCanvasBoundary";
    public string Description => "Detects active UI elements placed completely outside the screen/canvas boundaries.";
    public IssueSeverity DefaultSeverity => IssueSeverity.Warning;

    public IEnumerable<ValidationIssue> Evaluate(HierarchyTree tree, UiContextConfig config)
    {
        if (tree.Root == null) yield break;

        var canvasRect = tree.Root.Rect.ResolvedRect;
        if (canvasRect.Width <= 10f || canvasRect.Height <= 10f)
        {
            if (tree.CanvasContext?.Scaler != null)
            {
                var refRes = tree.CanvasContext.Scaler.ReferenceResolution;
                canvasRect = new ResolvedRectData(0f, 0f, refRes.X, refRes.Y);
            }
        }

        foreach (var node in tree.Root.Flatten().Where(n => n.ActiveInHierarchy && n != tree.Root))
        {
            var r = node.Rect.ResolvedRect;
            if (r.Width <= 0 || r.Height <= 0) continue;

            // Check if element is completely disjoint from the canvas area
            if (!r.Intersects(canvasRect))
            {
                yield return new ValidationIssue
                {
                    RuleId = RuleId,
                    RuleName = Name,
                    Severity = DefaultSeverity,
                    NodeId = node.Id,
                    NodeName = node.Name,
                    AssetPath = node.PrefabInfo.SourceAssetPath,
                    Message = $"Element '{node.Name}' (rect: {r.Width:0}x{r.Height:0}@{r.X:0},{r.Y:0}) is completely outside the canvas bounds ({canvasRect.Width:0}x{canvasRect.Height:0}).",
                    Details = new Dictionary<string, string>
                    {
                        ["elementRect"] = $"{r.Width:0}x{r.Height:0}@{r.X:0},{r.Y:0}",
                        ["canvasRect"] = $"{canvasRect.Width:0}x{canvasRect.Height:0}@{canvasRect.X:0},{canvasRect.Y:0}"
                    }
                };
            }
        }
    }
}

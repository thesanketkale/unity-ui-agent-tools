using System;
using System.Collections.Generic;
using System.Linq;
using UiContext.Core.Config;
using UiContext.Core.Models;
using UiContext.Core.Validation;

namespace UiContext.Core.Rules;

/// <summary>
/// UI006: Raycast target enabled on non-interactive graphics causes unnecessary raycast traversal overhead.
/// </summary>
public class UI006_NonInteractiveRaycastRule : IUiRule
{
    public string RuleId => "UI006";
    public string Name => "NonInteractiveRaycastTarget";
    public string Description => "Identifies graphic elements with raycastTarget=true that have no interactive ancestor.";
    public IssueSeverity DefaultSeverity => IssueSeverity.Note;

    public IEnumerable<ValidationIssue> Evaluate(HierarchyTree tree, UiContextConfig config)
    {
        if (tree.Root == null || !config.CheckNonInteractiveRaycasts) yield break;

        var allNodes = tree.Root.Flatten().ToList();
        var nodeMap = allNodes.ToDictionary(n => n.Id, StringComparer.Ordinal);

        foreach (var node in allNodes.Where(n => n.ActiveInHierarchy))
        {
            var graphic = node.GetComponent("Image") ??
                          node.GetComponent("RawImage") ??
                          node.GetComponent("TextMeshProUGUI") ??
                          node.GetComponent("Text");

            if (graphic == null) continue;

            bool raycastTarget = graphic.GetProperty("raycastTarget", true);
            if (!raycastTarget) continue;

            // Check if node itself is interactable or has interactive ancestor
            if (node.IsInteractable()) continue;

            bool hasInteractiveAncestor = false;
            string currentId = node.Id;
            while (true)
            {
                int lastSlash = currentId.LastIndexOf('/');
                if (lastSlash <= 0) break;
                currentId = currentId[..lastSlash];
                if (nodeMap.TryGetValue(currentId, out var parent) && parent.IsInteractable())
                {
                    hasInteractiveAncestor = true;
                    break;
                }
            }

            if (!hasInteractiveAncestor)
            {
                yield return new ValidationIssue
                {
                    RuleId = RuleId,
                    RuleName = Name,
                    Severity = DefaultSeverity,
                    NodeId = node.Id,
                    NodeName = node.Name,
                    AssetPath = node.PrefabInfo.SourceAssetPath,
                    Message = $"Graphic '{graphic.TypeName}' on '{node.Name}' has raycastTarget=true but is not interactive, causing unnecessary raycast CPU overhead."
                };
            }
        }
    }
}

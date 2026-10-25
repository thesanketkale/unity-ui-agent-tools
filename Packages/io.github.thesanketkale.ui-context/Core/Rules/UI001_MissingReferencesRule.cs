using System;
using System.Collections.Generic;
using UiContext.Core.Config;
using UiContext.Core.Models;
using UiContext.Core.Validation;

namespace UiContext.Core.Rules;

/// <summary>
/// UI001: Missing script or missing event target reference.
/// </summary>
public class UI001_MissingReferencesRule : IUiRule
{
    public string RuleId => "UI001";
    public string Name => "MissingReferences";
    public string Description => "Detects missing scripts or missing UnityEvent target references.";
    public IssueSeverity DefaultSeverity => IssueSeverity.Error;

    public IEnumerable<ValidationIssue> Evaluate(HierarchyTree tree, UiContextConfig config)
    {
        if (tree.Root == null) yield break;

        foreach (var node in tree.Root.Flatten())
        {
            foreach (var comp in node.Components)
            {
                if (string.Equals(comp.TypeName, "MissingScript", StringComparison.OrdinalIgnoreCase) ||
                    comp.GetProperty("isMissingScript", false))
                {
                    yield return new ValidationIssue
                    {
                        RuleId = RuleId,
                        RuleName = Name,
                        Severity = DefaultSeverity,
                        NodeId = node.Id,
                        NodeName = node.Name,
                        AssetPath = node.PrefabInfo.SourceAssetPath,
                        Message = $"Node '{node.Name}' has a missing or unresolvable MonoScript component."
                    };
                }

                foreach (var listener in comp.Listeners)
                {
                    if (string.IsNullOrEmpty(listener.TargetObjectName) ||
                        string.IsNullOrEmpty(listener.MethodName) ||
                        string.Equals(listener.TargetObjectName, "None", StringComparison.OrdinalIgnoreCase))
                    {
                        yield return new ValidationIssue
                        {
                            RuleId = RuleId,
                            RuleName = Name,
                            Severity = DefaultSeverity,
                            NodeId = node.Id,
                            NodeName = node.Name,
                            AssetPath = node.PrefabInfo.SourceAssetPath,
                            Message = $"Component '{comp.TypeName}' on '{node.Name}' has a dangling UnityEvent persistent listener pointing to missing target or method."
                        };
                    }
                }
            }
        }
    }
}

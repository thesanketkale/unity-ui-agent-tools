using System;
using System.Collections.Generic;
using System.Linq;
using UiContext.Core.Config;
using UiContext.Core.Models;
using UiContext.Core.Validation;

namespace UiContext.Core.Rules;

/// <summary>
/// UI004: Text content overflowing bounds without truncation or wrapping.
/// </summary>
public class UI004_TextOverflowRule : IUiRule
{
    public string RuleId => "UI004";
    public string Name => "TextOverflow";
    public string Description => "Detects unconstrained text elements likely clipping or overflowing bounds.";
    public IssueSeverity DefaultSeverity => IssueSeverity.Warning;

    public IEnumerable<ValidationIssue> Evaluate(HierarchyTree tree, UiContextConfig config)
    {
        if (tree.Root == null) yield break;

        foreach (var node in tree.Root.Flatten().Where(n => n.ActiveInHierarchy))
        {
            var textComp = node.GetComponent("TextMeshProUGUI") ??
                           node.GetComponent("TMP_Text") ??
                           node.GetComponent("Text");

            if (textComp == null) continue;

            string? text = textComp.GetProperty<string>("text");
            if (string.IsNullOrEmpty(text)) continue;

            float fontSize = textComp.GetProperty("fontSize", 14f);
            bool enableWrapping = textComp.GetProperty("enableWordWrapping", true);
            string overflowMode = textComp.GetProperty<string>("overflowMode") ?? "Overflow";

            var rect = node.Rect.ResolvedRect;
            if (rect.Width <= 0 || rect.Height <= 0) continue;

            // Safe heuristic: average proportional character width ~0.55 * fontSize
            float estimatedWidth = text.Length * (fontSize * 0.55f);
            bool isOverflow = string.Equals(overflowMode, "Overflow", StringComparison.OrdinalIgnoreCase);

            if (!enableWrapping && isOverflow && estimatedWidth > rect.Width * 1.05f)
            {
                yield return new ValidationIssue
                {
                    RuleId = RuleId,
                    RuleName = Name,
                    Severity = DefaultSeverity,
                    NodeId = node.Id,
                    NodeName = node.Name,
                    AssetPath = node.PrefabInfo.SourceAssetPath,
                    Message = $"Text on '{node.Name}' ({text.Length} chars, est. width {estimatedWidth:0}px) exceeds rect width ({rect.Width:0}px) with word wrapping disabled and overflow enabled.",
                    Details = new Dictionary<string, string>
                    {
                        ["estimatedTextWidth"] = estimatedWidth.ToString("0"),
                        ["rectWidth"] = rect.Width.ToString("0"),
                        ["fontSize"] = fontSize.ToString("0")
                    }
                };
            }
        }
    }
}

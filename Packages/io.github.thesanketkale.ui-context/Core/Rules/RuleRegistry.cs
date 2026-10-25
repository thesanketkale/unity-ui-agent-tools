using System;
using System.Collections.Generic;
using UiContext.Core.Config;
using UiContext.Core.Models;
using UiContext.Core.Validation;

namespace UiContext.Core.Rules;

/// <summary>
/// Registry managing all active UI rules and running validation passes.
/// </summary>
public class RuleRegistry
{
    private readonly List<IUiRule> _rules = [];

    public RuleRegistry()
    {
        RegisterDefaultRules();
    }

    public IReadOnlyList<IUiRule> Rules => _rules;

    public void RegisterRule(IUiRule rule)
    {
        _rules.RemoveAll(r => string.Equals(r.RuleId, rule.RuleId, StringComparison.OrdinalIgnoreCase));
        _rules.Add(rule);
    }

    private void RegisterDefaultRules()
    {
        _rules.Add(new UI001_MissingReferencesRule());
        _rules.Add(new UI002_OverlappingInteractablesRule());
        _rules.Add(new UI003_MinimumTargetSizeRule());
        _rules.Add(new UI004_TextOverflowRule());
        _rules.Add(new UI005_OutsideBoundaryRule());
        _rules.Add(new UI006_NonInteractiveRaycastRule());
        _rules.Add(new UI007_DeepLayoutNestingRule());
        _rules.Add(new UI008_DuplicateSiblingNamesRule());
        _rules.Add(new UI009_AbsolutePositionInLayoutRule());
    }

    public ValidationResult Validate(HierarchyTree tree, UiContextConfig? config = null)
    {
        config ??= UiContextConfig.CreateDefault();
        var result = new ValidationResult { TargetName = tree.TargetName };

        foreach (var rule in _rules)
        {
            // Check if rule is disabled in config
            if (config.RuleSeverities.TryGetValue(rule.RuleId, out var severityStr) &&
                string.Equals(severityStr, "off", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var issues = rule.Evaluate(tree, config);
            foreach (var issue in issues)
            {
                // Override severity if configured
                if (!string.IsNullOrEmpty(severityStr))
                {
                    issue.Severity = severityStr.ToLowerInvariant() switch
                    {
                        "error" => IssueSeverity.Error,
                        "warning" => IssueSeverity.Warning,
                        "note" or "info" => IssueSeverity.Note,
                        _ => issue.Severity
                    };
                }
                result.Issues.Add(issue);
            }
        }

        return result;
    }
}

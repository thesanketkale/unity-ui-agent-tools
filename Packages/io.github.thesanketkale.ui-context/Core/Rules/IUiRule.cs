using System.Collections.Generic;
using UiContext.Core.Config;
using UiContext.Core.Models;
using UiContext.Core.Validation;

namespace UiContext.Core.Rules;

/// <summary>
/// Interface implemented by all deterministic UI validation rules.
/// </summary>
public interface IUiRule
{
    string RuleId { get; }
    string Name { get; }
    string Description { get; }
    IssueSeverity DefaultSeverity { get; }

    IEnumerable<ValidationIssue> Evaluate(HierarchyTree tree, UiContextConfig config);
}

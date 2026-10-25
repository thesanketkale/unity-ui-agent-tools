using System;
using System.Collections.Generic;

namespace UiContext.Core.Validation;

public enum IssueSeverity
{
    Note,
    Warning,
    Error
}

/// <summary>
/// Diagnostic finding reported by a UI validation rule.
/// </summary>
public class ValidationIssue
{
    public string RuleId { get; set; } = string.Empty;
    public string RuleName { get; set; } = string.Empty;
    public IssueSeverity Severity { get; set; } = IssueSeverity.Warning;
    public string Message { get; set; } = string.Empty;
    public string NodeId { get; set; } = string.Empty;
    public string NodeName { get; set; } = string.Empty;
    public string? AssetPath { get; set; }
    public Dictionary<string, string> Details { get; set; } = new(StringComparer.Ordinal);
}

/// <summary>
/// Collection of validation diagnostics for a tree.
/// </summary>
public class ValidationResult
{
    public string TargetName { get; set; } = string.Empty;
    public List<ValidationIssue> Issues { get; set; } = [];

    public int ErrorCount => Issues.Count(i => i.Severity == IssueSeverity.Error);
    public int WarningCount => Issues.Count(i => i.Severity == IssueSeverity.Warning);
    public int NoteCount => Issues.Count(i => i.Severity == IssueSeverity.Note);
    public bool HasErrors => ErrorCount > 0;
    public bool IsClean => Issues.Count == 0;
}

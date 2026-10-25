using System;
using System.Linq;
using System.Text;

namespace UiContext.Core.Validation;

/// <summary>
/// Exception thrown when UI assertions fail during automated tests.
/// </summary>
public class UiValidationException : Exception
{
    public ValidationResult Result { get; }

    public UiValidationException(string message, ValidationResult result) : base(message)
    {
        Result = result;
    }
}

/// <summary>
/// Test assertion helpers for Unity EditMode and automated C# test suites.
/// </summary>
public static class UiContextAssert
{
    /// <summary>
    /// Asserts that a validation result contains zero errors.
    /// </summary>
    public static void NoErrors(ValidationResult result)
    {
        if (result.HasErrors)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"UiContextAssert.NoErrors failed for '{result.TargetName}' with {result.ErrorCount} error(s):");
            foreach (var err in result.Issues.Where(i => i.Severity == IssueSeverity.Error))
            {
                sb.AppendLine($"  - [{err.RuleId}] on '{err.NodeId}': {err.Message}");
            }
            throw new UiValidationException(sb.ToString(), result);
        }
    }

    /// <summary>
    /// Asserts that a validation result contains zero errors and zero warnings.
    /// </summary>
    public static void Clean(ValidationResult result)
    {
        if (!result.IsClean)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"UiContextAssert.Clean failed for '{result.TargetName}' with {result.Issues.Count} issue(s):");
            foreach (var issue in result.Issues)
            {
                sb.AppendLine($"  - [{issue.RuleId}][{issue.Severity}] on '{issue.NodeId}': {issue.Message}");
            }
            throw new UiValidationException(sb.ToString(), result);
        }
    }
}

using UiContext.Core.Validation;
using Xunit;

namespace UiContext.Core.Tests;

public class UiContextAssertTests
{
    [Fact]
    public void NoErrors_PassesWhenOnlyWarningsPresent()
    {
        var result = new ValidationResult
        {
            TargetName = "Test",
            Issues = new List<ValidationIssue>
            {
                new() { RuleId = "UI003", Severity = IssueSeverity.Warning, Message = "Small touch target" }
            }
        };

        UiContextAssert.NoErrors(result); // Should not throw
    }

    [Fact]
    public void NoErrors_ThrowsExceptionWhenErrorsPresent()
    {
        var result = new ValidationResult
        {
            TargetName = "Test",
            Issues = new List<ValidationIssue>
            {
                new() { RuleId = "UI001", Severity = IssueSeverity.Error, Message = "Missing script" }
            }
        };

        var ex = Assert.Throws<UiValidationException>(() => UiContextAssert.NoErrors(result));
        Assert.Contains("UiContextAssert.NoErrors failed", ex.Message);
        Assert.Contains("UI001", ex.Message);
    }

    [Fact]
    public void Clean_ThrowsExceptionWhenWarningsPresent()
    {
        var result = new ValidationResult
        {
            TargetName = "Test",
            Issues = new List<ValidationIssue>
            {
                new() { RuleId = "UI003", Severity = IssueSeverity.Warning, Message = "Warning target" }
            }
        };

        Assert.Throws<UiValidationException>(() => UiContextAssert.Clean(result));
    }
}

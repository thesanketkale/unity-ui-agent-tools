using System;

namespace UiContext.Core.Diff;

/// <summary>
/// Granular semantic difference recorded on a specific node.
/// </summary>
public class UiNodeDiff
{
    public string NodeId { get; set; } = string.Empty;
    public string NodeName { get; set; } = string.Empty;
    public UiDiffChangeType ChangeType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? BeforeValue { get; set; }
    public string? AfterValue { get; set; }
}

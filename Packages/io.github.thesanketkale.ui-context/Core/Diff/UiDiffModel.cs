using System;
using System.Collections.Generic;
using System.Linq;

namespace UiContext.Core.Diff;

/// <summary>
/// Root model containing structured semantic differences between two UI trees.
/// </summary>
public class UiDiffModel
{
    public string TargetNameBefore { get; set; } = string.Empty;
    public string TargetNameAfter { get; set; } = string.Empty;
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");
    public List<UiNodeDiff> Changes { get; set; } = [];

    public int TotalChanges => Changes.Count;
    public int AddedCount => Changes.Count(c => c.ChangeType == UiDiffChangeType.Added);
    public int RemovedCount => Changes.Count(c => c.ChangeType == UiDiffChangeType.Removed);
    public int MovedCount => Changes.Count(c => c.ChangeType == UiDiffChangeType.Moved);
    public int ResizedCount => Changes.Count(c => c.ChangeType == UiDiffChangeType.Resized);
    public int TextChangesCount => Changes.Count(c => c.ChangeType == UiDiffChangeType.TextChanged);
    public bool HasDifferences => Changes.Count > 0;
}

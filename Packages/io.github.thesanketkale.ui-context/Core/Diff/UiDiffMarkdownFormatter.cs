using System.Text;

namespace UiContext.Core.Diff;

/// <summary>
/// Formats a semantic UI diff into clean, token-efficient Markdown.
/// </summary>
public static class UiDiffMarkdownFormatter
{
    public static string Format(UiDiffModel diff)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# UI Semantic Diff: {diff.TargetNameBefore} -> {diff.TargetNameAfter}");
        sb.AppendLine($"*Timestamp: {diff.Timestamp}*");
        sb.AppendLine();

        sb.AppendLine($"**Summary:** {diff.TotalChanges} total change(s)");
        sb.AppendLine($"- Added Nodes: {diff.AddedCount}");
        sb.AppendLine($"- Removed Nodes: {diff.RemovedCount}");
        sb.AppendLine($"- Moved Nodes: {diff.MovedCount}");
        sb.AppendLine($"- Resized Nodes: {diff.ResizedCount}");
        sb.AppendLine($"- Text Changes: {diff.TextChangesCount}");
        sb.AppendLine();

        if (!diff.HasDifferences)
        {
            sb.AppendLine("No semantic differences detected between UI snapshots.");
            return sb.ToString();
        }

        sb.AppendLine("| Type | Node | Description | Before | After |");
        sb.AppendLine("|---|---|---|---|---|");

        foreach (var change in diff.Changes)
        {
            string typeBadge = change.ChangeType switch
            {
                UiDiffChangeType.Added => "🟢 Added",
                UiDiffChangeType.Removed => "🔴 Removed",
                UiDiffChangeType.Moved => "🔄 Moved",
                UiDiffChangeType.Resized => "📐 Resized",
                UiDiffChangeType.TextChanged => "✏️ Text",
                UiDiffChangeType.ActiveStateChanged => "👁️ State",
                UiDiffChangeType.ComponentAdded => "➕ Comp+",
                UiDiffChangeType.ComponentRemoved => "➖ Comp-",
                _ => "⚡ Mod"
            };

            string beforeVal = (change.BeforeValue ?? "-").Replace("|", "\\|");
            string afterVal = (change.AfterValue ?? "-").Replace("|", "\\|");
            string desc = change.Description.Replace("|", "\\|");

            sb.AppendLine($"| {typeBadge} | `{change.NodeName}` | {desc} | `{beforeVal}` | `{afterVal}` |");
        }

        return sb.ToString();
    }
}

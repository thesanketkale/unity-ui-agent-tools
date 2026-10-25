namespace UiContext.Core.Diff;

/// <summary>
/// Categorized semantic change types for UI elements.
/// </summary>
public enum UiDiffChangeType
{
    Added,
    Removed,
    Moved,
    Resized,
    TextChanged,
    ActiveStateChanged,
    ComponentAdded,
    ComponentRemoved,
    ComponentModified
}

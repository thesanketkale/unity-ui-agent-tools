using System;
using System.Collections.Generic;
using System.Linq;

namespace UiContext.Core.Models;

/// <summary>
/// Hierarchy export summary statistics.
/// </summary>
public class TreeStatistics
{
    public int TotalNodeCount { get; set; }
    public int ActiveNodeCount { get; set; }
    public int InteractableCount { get; set; }
    public int TextElementCount { get; set; }
    public int MaxHierarchyDepth { get; set; }
}

/// <summary>
/// Complete exported UI tree document.
/// </summary>
public class HierarchyTree
{
    public string SchemaVersion { get; set; } = "1.0.0";
    public string ExportTimestamp { get; set; } = DateTime.UtcNow.ToString("o");
    public string UnityVersion { get; set; } = "2022.3.40f1";
    public string TargetName { get; set; } = string.Empty;
    public string TargetType { get; set; } = "Prefab"; // Prefab, Scene, Selection
    public CanvasData? CanvasContext { get; set; } = new();
    public UiNode? Root { get; set; }
    public TreeStatistics Statistics { get; set; } = new();

    /// <summary>
    /// Recalculates summary statistics across the tree.
    /// </summary>
    public void RecomputeStatistics()
    {
        if (Root == null)
        {
            Statistics = new TreeStatistics();
            return;
        }

        var allNodes = Root.Flatten().ToList();
        Statistics.TotalNodeCount = allNodes.Count;
        Statistics.ActiveNodeCount = allNodes.Count(n => n.ActiveInHierarchy);
        Statistics.InteractableCount = allNodes.Count(n => n.IsInteractable());
        Statistics.TextElementCount = allNodes.Count(n => !string.IsNullOrEmpty(n.GetTextContent()));
        Statistics.MaxHierarchyDepth = ComputeMaxDepth(Root, 1);
    }

    private static int ComputeMaxDepth(UiNode current, int currentDepth)
    {
        if (current.Children.Count == 0) return currentDepth;
        int max = currentDepth;
        foreach (var child in current.Children)
        {
            max = Math.Max(max, ComputeMaxDepth(child, currentDepth + 1));
        }
        return max;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UiContext.Core.Models;

namespace UiContext.Core.Diff;

/// <summary>
/// Computes semantic differences between two UI hierarchy snapshots.
/// </summary>
public static class UiTreeDiffer
{
    public static UiDiffModel Compare(HierarchyTree before, HierarchyTree after)
    {
        var model = new UiDiffModel
        {
            TargetNameBefore = before.TargetName,
            TargetNameAfter = after.TargetName
        };

        var beforeNodes = before.Root?.Flatten().ToDictionary(n => n.Id, StringComparer.Ordinal)
                          ?? new Dictionary<string, UiNode>();
        var afterNodes = after.Root?.Flatten().ToDictionary(n => n.Id, StringComparer.Ordinal)
                         ?? new Dictionary<string, UiNode>();

        // 1. Detect Added Nodes
        foreach (var (id, node) in afterNodes)
        {
            if (!beforeNodes.ContainsKey(id))
            {
                model.Changes.Add(new UiNodeDiff
                {
                    NodeId = id,
                    NodeName = node.Name,
                    ChangeType = UiDiffChangeType.Added,
                    Description = $"Node '{node.Name}' was added to hierarchy at '{id}'.",
                    AfterValue = $"Active: {node.ActiveSelf}, Size: {node.Rect.ResolvedRect.Width:0}x{node.Rect.ResolvedRect.Height:0}"
                });
            }
        }

        // 2. Detect Removed Nodes
        foreach (var (id, node) in beforeNodes)
        {
            if (!afterNodes.ContainsKey(id))
            {
                model.Changes.Add(new UiNodeDiff
                {
                    NodeId = id,
                    NodeName = node.Name,
                    ChangeType = UiDiffChangeType.Removed,
                    Description = $"Node '{node.Name}' was removed from hierarchy.",
                    BeforeValue = $"Active: {node.ActiveSelf}, Size: {node.Rect.ResolvedRect.Width:0}x{node.Rect.ResolvedRect.Height:0}"
                });
            }
        }

        // 3. Detect Node Property and Geometric Changes
        foreach (var (id, beforeNode) in beforeNodes)
        {
            if (afterNodes.TryGetValue(id, out var afterNode))
            {
                CompareNodeProperties(beforeNode, afterNode, model.Changes);
            }
        }

        return model;
    }

    private static void CompareNodeProperties(UiNode before, UiNode after, List<UiNodeDiff> changes)
    {
        // Active state change
        if (before.ActiveSelf != after.ActiveSelf)
        {
            changes.Add(new UiNodeDiff
            {
                NodeId = before.Id,
                NodeName = before.Name,
                ChangeType = UiDiffChangeType.ActiveStateChanged,
                Description = $"Active state changed from '{before.ActiveSelf}' to '{after.ActiveSelf}'.",
                BeforeValue = before.ActiveSelf.ToString(),
                AfterValue = after.ActiveSelf.ToString()
            });
        }

        // Resized check
        var bRect = before.Rect.ResolvedRect;
        var aRect = after.Rect.ResolvedRect;
        if (Math.Abs(bRect.Width - aRect.Width) > 0.5f || Math.Abs(bRect.Height - aRect.Height) > 0.5f)
        {
            changes.Add(new UiNodeDiff
            {
                NodeId = before.Id,
                NodeName = before.Name,
                ChangeType = UiDiffChangeType.Resized,
                Description = $"Resolved size changed from ({bRect.Width:0}x{bRect.Height:0}) to ({aRect.Width:0}x{aRect.Height:0}).",
                BeforeValue = $"{bRect.Width:0}x{bRect.Height:0}",
                AfterValue = $"{aRect.Width:0}x{aRect.Height:0}"
            });
        }

        // Moved / Position check
        var bPos = before.Rect.AnchoredPosition;
        var aPos = after.Rect.AnchoredPosition;
        if (Math.Abs(bPos.X - aPos.X) > 0.5f || Math.Abs(bPos.Y - aPos.Y) > 0.5f)
        {
            changes.Add(new UiNodeDiff
            {
                NodeId = before.Id,
                NodeName = before.Name,
                ChangeType = UiDiffChangeType.Moved,
                Description = $"Anchored position moved from ({bPos.X:0.0}, {bPos.Y:0.0}) to ({aPos.X:0.0}, {aPos.Y:0.0}).",
                BeforeValue = $"({bPos.X:0.0}, {bPos.Y:0.0})",
                AfterValue = $"({aPos.X:0.0}, {aPos.Y:0.0})"
            });
        }

        // Text content changes
        string? bText = before.GetTextContent();
        string? aText = after.GetTextContent();
        if (!string.Equals(bText, aText, StringComparison.Ordinal))
        {
            changes.Add(new UiNodeDiff
            {
                NodeId = before.Id,
                NodeName = before.Name,
                ChangeType = UiDiffChangeType.TextChanged,
                Description = $"Text content changed from \"{bText ?? ""}\" to \"{aText ?? ""}\".",
                BeforeValue = bText,
                AfterValue = aText
            });
        }

        // Component additions / removals
        var bTypes = before.Components.Select(c => c.TypeName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var aTypes = after.Components.Select(c => c.TypeName).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var addedType in aTypes.Except(bTypes))
        {
            changes.Add(new UiNodeDiff
            {
                NodeId = before.Id,
                NodeName = before.Name,
                ChangeType = UiDiffChangeType.ComponentAdded,
                Description = $"Component '{addedType}' was added to node.",
                AfterValue = addedType
            });
        }

        foreach (var removedType in bTypes.Except(aTypes))
        {
            changes.Add(new UiNodeDiff
            {
                NodeId = before.Id,
                NodeName = before.Name,
                ChangeType = UiDiffChangeType.ComponentRemoved,
                Description = $"Component '{removedType}' was removed from node.",
                BeforeValue = removedType
            });
        }
    }
}

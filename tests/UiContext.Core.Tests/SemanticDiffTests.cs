using UiContext.Core.Diff;
using UiContext.Core.Models;
using Xunit;

namespace UiContext.Core.Tests;

public class SemanticDiffTests
{
    [Fact]
    public void Differ_DetectsAddedRemovedAndModifiedNodes()
    {
        var before = new HierarchyTree
        {
            TargetName = "HUD_v1",
            Root = new UiNode
            {
                Id = "/HUD",
                Name = "HUD",
                ActiveSelf = true,
                Rect = new RectTransformData { ResolvedRect = new ResolvedRectData(0, 0, 1920, 1080) },
                Children = new List<UiNode>
                {
                    new()
                    {
                        Id = "/HUD/OldButton",
                        Name = "OldButton",
                        ActiveSelf = true,
                        Rect = new RectTransformData { ResolvedRect = new ResolvedRectData(10, 10, 100, 40) }
                    },
                    new()
                    {
                        Id = "/HUD/Title",
                        Name = "Title",
                        Rect = new RectTransformData { ResolvedRect = new ResolvedRectData(0, 0, 200, 40) },
                        Components = new List<UiComponentData>
                        {
                            new() { TypeName = "TMP_Text", Properties = new() { ["text"] = "Welcome" } }
                        }
                    }
                }
            }
        };

        var after = new HierarchyTree
        {
            TargetName = "HUD_v2",
            Root = new UiNode
            {
                Id = "/HUD",
                Name = "HUD",
                ActiveSelf = true,
                Rect = new RectTransformData { ResolvedRect = new ResolvedRectData(0, 0, 1920, 1080) },
                Children = new List<UiNode>
                {
                    new()
                    {
                        Id = "/HUD/NewButton",
                        Name = "NewButton",
                        ActiveSelf = true,
                        Rect = new RectTransformData { ResolvedRect = new ResolvedRectData(10, 10, 120, 48) }
                    },
                    new()
                    {
                        Id = "/HUD/Title",
                        Name = "Title",
                        Rect = new RectTransformData { ResolvedRect = new ResolvedRectData(0, 0, 250, 50) }, // Resized
                        Components = new List<UiComponentData>
                        {
                            new() { TypeName = "TMP_Text", Properties = new() { ["text"] = "Welcome Back!" } } // TextChanged
                        }
                    }
                }
            }
        };

        var diff = UiTreeDiffer.Compare(before, after);

        Assert.True(diff.HasDifferences);
        Assert.Equal(1, diff.AddedCount);
        Assert.Equal(1, diff.RemovedCount);
        Assert.Equal(1, diff.ResizedCount);
        Assert.Equal(1, diff.TextChangesCount);

        string md = UiDiffMarkdownFormatter.Format(diff);
        Assert.Contains("HUD_v1 -> HUD_v2", md);
        Assert.Contains("NewButton", md);
        Assert.Contains("OldButton", md);
        Assert.Contains("Welcome Back!", md);
    }
}

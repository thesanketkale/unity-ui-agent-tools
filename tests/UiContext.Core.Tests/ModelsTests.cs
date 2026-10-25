using UiContext.Core.Models;
using Xunit;

namespace UiContext.Core.Tests;

public class ModelsTests
{
    [Fact]
    public void ResolvedRect_Intersection_CorrectlyIdentifiesOverlaps()
    {
        var rectA = new ResolvedRectData(10, 10, 100, 50);
        var rectB = new ResolvedRectData(50, 20, 100, 50);
        var rectC = new ResolvedRectData(200, 200, 50, 50);

        Assert.True(rectA.Intersects(rectB));
        Assert.True(rectB.Intersects(rectA));
        Assert.False(rectA.Intersects(rectC));
    }

    [Fact]
    public void ResolvedRect_Containment_CorrectlyIdentifiesBounds()
    {
        var container = new ResolvedRectData(0, 0, 1920, 1080);
        var inside = new ResolvedRectData(100, 100, 200, 100);
        var outside = new ResolvedRectData(-10, 100, 200, 100);

        Assert.True(inside.IsContainedWithin(container));
        Assert.False(outside.IsContainedWithin(container));
    }

    [Theory]
    [InlineData(0f, 0f, 1f, 1f, "stretch-stretch")]
    [InlineData(0f, 1f, 0f, 1f, "top-left")]
    [InlineData(1f, 1f, 1f, 1f, "top-right")]
    [InlineData(0.5f, 0.5f, 0.5f, 0.5f, "middle-center")]
    public void RectTransformData_AnchorShorthand_ComputesAccurateNames(float minX, float minY, float maxX, float maxY, string expected)
    {
        var data = new RectTransformData
        {
            AnchorMin = new Vector2Data(minX, minY),
            AnchorMax = new Vector2Data(maxX, maxY)
        };

        Assert.Equal(expected, data.GetAnchorShorthand());
    }

    [Fact]
    public void HierarchyTree_RecomputeStatistics_ComputesAccurateMetrics()
    {
        var root = new UiNode
        {
            Id = "/Canvas",
            Name = "Canvas",
            ActiveInHierarchy = true,
            Rect = new RectTransformData()
        };

        var panel = new UiNode
        {
            Id = "/Canvas/Panel",
            Name = "Panel",
            ActiveInHierarchy = true
        };

        var button = new UiNode
        {
            Id = "/Canvas/Panel/Button",
            Name = "Button",
            ActiveInHierarchy = true,
            Components = new List<UiComponentData>
            {
                new() { TypeName = "Button", Properties = new() { ["interactable"] = true } },
                new() { TypeName = "TextMeshProUGUI", Properties = new() { ["text"] = "Click" } }
            }
        };

        panel.Children.Add(button);
        root.Children.Add(panel);

        var tree = new HierarchyTree
        {
            TargetName = "TestTree",
            Root = root
        };

        tree.RecomputeStatistics();

        Assert.Equal(3, tree.Statistics.TotalNodeCount);
        Assert.Equal(3, tree.Statistics.ActiveNodeCount);
        Assert.Equal(1, tree.Statistics.InteractableCount);
        Assert.Equal(1, tree.Statistics.TextElementCount);
        Assert.Equal(3, tree.Statistics.MaxHierarchyDepth);
    }
}

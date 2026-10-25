using UiContext.Core.Config;
using UiContext.Core.Models;
using UiContext.Core.Rules;
using UiContext.Core.Validation;
using Xunit;

namespace UiContext.Core.Tests;

public class RulesTests
{
    private readonly UiContextConfig _defaultConfig = UiContextConfig.CreateDefault("mobile");

    [Fact]
    public void UI001_MissingReferences_FailsOnMissingScriptAndDanglingListener()
    {
        var rule = new UI001_MissingReferencesRule();
        var tree = new HierarchyTree
        {
            TargetName = "Test",
            Root = new UiNode
            {
                Id = "/Root",
                Name = "Root",
                Components = new List<UiComponentData>
                {
                    new() { TypeName = "MissingScript", Properties = new() { ["isMissingScript"] = true } },
                    new()
                    {
                        TypeName = "Button",
                        Listeners = new List<ButtonListenerData>
                        {
                            new("None", "None", "", "RuntimeOnly")
                        }
                    }
                }
            }
        };

        var issues = rule.Evaluate(tree, _defaultConfig).ToList();
        Assert.Equal(2, issues.Count);
        Assert.All(issues, i => Assert.Equal(IssueSeverity.Error, i.Severity));
    }

    [Fact]
    public void UI002_OverlappingInteractables_FlagsConflictingHitboxes()
    {
        var rule = new UI002_OverlappingInteractablesRule();
        var tree = new HierarchyTree
        {
            TargetName = "Test",
            Root = new UiNode
            {
                Id = "/Canvas",
                Name = "Canvas",
                Children = new List<UiNode>
                {
                    new()
                    {
                        Id = "/Canvas/Btn1",
                        Name = "Btn1",
                        Rect = new RectTransformData { ResolvedRect = new ResolvedRectData(100, 100, 120, 50) },
                        Components = new List<UiComponentData>
                        {
                            new() { TypeName = "Button", Properties = new() { ["interactable"] = true } }
                        }
                    },
                    new()
                    {
                        Id = "/Canvas/Btn2",
                        Name = "Btn2",
                        Rect = new RectTransformData { ResolvedRect = new ResolvedRectData(150, 110, 120, 50) },
                        Components = new List<UiComponentData>
                        {
                            new() { TypeName = "Button", Properties = new() { ["interactable"] = true } }
                        }
                    }
                }
            }
        };

        var issues = rule.Evaluate(tree, _defaultConfig).ToList();
        Assert.Single(issues);
        Assert.Equal("UI002", issues[0].RuleId);
    }

    [Fact]
    public void UI003_MinimumTargetSize_ValidatesTouchAccessibility()
    {
        var rule = new UI003_MinimumTargetSizeRule();
        var tree = new HierarchyTree
        {
            TargetName = "Test",
            Root = new UiNode
            {
                Id = "/Canvas",
                Name = "Canvas",
                Children = new List<UiNode>
                {
                    new()
                    {
                        Id = "/Canvas/TinyBtn",
                        Name = "TinyBtn",
                        Rect = new RectTransformData { ResolvedRect = new ResolvedRectData(10, 10, 24, 24) },
                        Components = new List<UiComponentData>
                        {
                            new() { TypeName = "Button", Properties = new() { ["interactable"] = true } }
                        }
                    },
                    new()
                    {
                        Id = "/Canvas/ProperBtn",
                        Name = "ProperBtn",
                        Rect = new RectTransformData { ResolvedRect = new ResolvedRectData(100, 100, 64, 48) },
                        Components = new List<UiComponentData>
                        {
                            new() { TypeName = "Button", Properties = new() { ["interactable"] = true } }
                        }
                    }
                }
            }
        };

        var issues = rule.Evaluate(tree, _defaultConfig).ToList();
        Assert.Single(issues);
        Assert.Equal("UI003", issues[0].RuleId);
        Assert.Equal("TinyBtn", issues[0].NodeName);
    }

    [Fact]
    public void UI004_TextOverflow_DetectsUnconstrainedOverflow()
    {
        var rule = new UI004_TextOverflowRule();
        var tree = new HierarchyTree
        {
            TargetName = "Test",
            Root = new UiNode
            {
                Id = "/Canvas",
                Name = "Canvas",
                Children = new List<UiNode>
                {
                    new()
                    {
                        Id = "/Canvas/OverflowText",
                        Name = "OverflowText",
                        Rect = new RectTransformData { ResolvedRect = new ResolvedRectData(0, 0, 100, 30) },
                        Components = new List<UiComponentData>
                        {
                            new()
                            {
                                TypeName = "TextMeshProUGUI",
                                Properties = new()
                                {
                                    ["text"] = "This is a very long string that will clearly overflow one hundred pixels",
                                    ["fontSize"] = 16f,
                                    ["enableWordWrapping"] = false,
                                    ["overflowMode"] = "Overflow"
                                }
                            }
                        }
                    }
                }
            }
        };

        var issues = rule.Evaluate(tree, _defaultConfig).ToList();
        Assert.Single(issues);
        Assert.Equal("UI004", issues[0].RuleId);
    }

    [Fact]
    public void UI005_OutsideBoundary_FlagsElementsPlacedOutsideCanvas()
    {
        var rule = new UI005_OutsideBoundaryRule();
        var tree = new HierarchyTree
        {
            TargetName = "Test",
            CanvasContext = new CanvasData
            {
                Scaler = new CanvasScalerData { ReferenceResolution = new Vector2Data(1920, 1080) }
            },
            Root = new UiNode
            {
                Id = "/Canvas",
                Name = "Canvas",
                Rect = new RectTransformData { ResolvedRect = new ResolvedRectData(0, 0, 1920, 1080) },
                Children = new List<UiNode>
                {
                    new()
                    {
                        Id = "/Canvas/OffscreenBadge",
                        Name = "OffscreenBadge",
                        Rect = new RectTransformData { ResolvedRect = new ResolvedRectData(2500, 500, 100, 100) }
                    }
                }
            }
        };

        var issues = rule.Evaluate(tree, _defaultConfig).ToList();
        Assert.Single(issues);
        Assert.Equal("UI005", issues[0].RuleId);
    }

    [Fact]
    public void UI006_NonInteractiveRaycast_FlagsPassiveGraphicWithRaycastEnabled()
    {
        var rule = new UI006_NonInteractiveRaycastRule();
        var tree = new HierarchyTree
        {
            TargetName = "Test",
            Root = new UiNode
            {
                Id = "/Canvas",
                Name = "Canvas",
                Children = new List<UiNode>
                {
                    new()
                    {
                        Id = "/Canvas/PassiveImage",
                        Name = "PassiveImage",
                        Components = new List<UiComponentData>
                        {
                            new() { TypeName = "Image", Properties = new() { ["raycastTarget"] = true } }
                        }
                    }
                }
            }
        };

        var issues = rule.Evaluate(tree, _defaultConfig).ToList();
        Assert.Single(issues);
        Assert.Equal("UI006", issues[0].RuleId);
    }

    [Fact]
    public void UI007_DeepLayoutNesting_DetectsExcessiveLayoutGroupDepth()
    {
        var rule = new UI007_DeepLayoutNestingRule();

        var l4 = new UiNode { Id = "/1/2/3/4", Name = "L4", Components = new() { new() { TypeName = "VerticalLayoutGroup" } } };
        var l3 = new UiNode { Id = "/1/2/3", Name = "L3", Components = new() { new() { TypeName = "HorizontalLayoutGroup" } }, Children = [l4] };
        var l2 = new UiNode { Id = "/1/2", Name = "L2", Components = new() { new() { TypeName = "VerticalLayoutGroup" } }, Children = [l3] };
        var l1 = new UiNode { Id = "/1", Name = "L1", Components = new() { new() { TypeName = "VerticalLayoutGroup" } }, Children = [l2] };

        var tree = new HierarchyTree
        {
            TargetName = "Test",
            Root = l1
        };

        var config = UiContextConfig.CreateDefault();
        config.MaxLayoutGroupDepth = 3;

        var issues = rule.Evaluate(tree, config).ToList();
        Assert.Single(issues);
        Assert.Equal("UI007", issues[0].RuleId);
        Assert.Equal("L4", issues[0].NodeName);
    }

    [Fact]
    public void UI008_DuplicateSiblingNames_FlagsIdenticalSiblingNames()
    {
        var rule = new UI008_DuplicateSiblingNamesRule();
        var tree = new HierarchyTree
        {
            TargetName = "Test",
            Root = new UiNode
            {
                Id = "/Panel",
                Name = "Panel",
                Children = new List<UiNode>
                {
                    new() { Id = "/Panel/Item", Name = "Item" },
                    new() { Id = "/Panel/Item", Name = "Item" }
                }
            }
        };

        var issues = rule.Evaluate(tree, _defaultConfig).ToList();
        Assert.Equal(2, issues.Count);
        Assert.All(issues, i => Assert.Equal("UI008", i.RuleId));
    }

    [Fact]
    public void UI009_AbsolutePositionInLayout_FlagsChildWithManualOffsets()
    {
        var rule = new UI009_AbsolutePositionInLayoutRule();
        var tree = new HierarchyTree
        {
            TargetName = "Test",
            Root = new UiNode
            {
                Id = "/List",
                Name = "List",
                Components = new List<UiComponentData>
                {
                    new() { TypeName = "VerticalLayoutGroup" }
                },
                Children = new List<UiNode>
                {
                    new()
                    {
                        Id = "/List/RowWithOffset",
                        Name = "RowWithOffset",
                        Rect = new RectTransformData { AnchoredPosition = new Vector2Data(25, -10) }
                    },
                    new()
                    {
                        Id = "/List/CleanRow",
                        Name = "CleanRow",
                        Rect = new RectTransformData { AnchoredPosition = new Vector2Data(0, 0) }
                    }
                }
            }
        };

        var issues = rule.Evaluate(tree, _defaultConfig).ToList();
        Assert.Single(issues);
        Assert.Equal("UI009", issues[0].RuleId);
        Assert.Equal("RowWithOffset", issues[0].NodeName);
    }
}

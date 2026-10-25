using UiContext.Core.Formatters;
using UiContext.Core.Models;
using UiContext.Core.Serialization;
using Xunit;

namespace UiContext.Core.Tests;

public class SerializationTests
{
    [Fact]
    public void Serializer_RoundTrip_PreservesAllHierarchyData()
    {
        var tree = new HierarchyTree
        {
            TargetName = "HUD",
            TargetType = "Prefab",
            Root = new UiNode
            {
                Id = "/HUD",
                Name = "HUD",
                ActiveSelf = true,
                ActiveInHierarchy = true,
                Rect = new RectTransformData
                {
                    ResolvedRect = new ResolvedRectData(0, 0, 1920, 1080)
                },
                Components = new List<UiComponentData>
                {
                    new()
                    {
                        TypeName = "Canvas",
                        Properties = new() { ["renderMode"] = "ScreenSpaceOverlay" }
                    }
                },
                Children = new List<UiNode>
                {
                    new()
                    {
                        Id = "/HUD/HealthBar",
                        Name = "HealthBar",
                        Rect = new RectTransformData
                        {
                            ResolvedRect = new ResolvedRectData(100, 100, 200, 30)
                        },
                        Components = new List<UiComponentData>
                        {
                            new()
                            {
                                TypeName = "Image",
                                Properties = new() { ["fillAmount"] = 0.75 }
                            }
                        }
                    }
                }
            }
        };

        var serializer = new UiTreeSerializer();
        string json = serializer.Serialize(tree, indented: true);

        Assert.Contains("\"targetName\": \"HUD\"", json);
        Assert.Contains("\"name\": \"HealthBar\"", json);

        var deserialized = serializer.Deserialize(json);
        Assert.NotNull(deserialized);
        Assert.Equal("HUD", deserialized.TargetName);
        Assert.NotNull(deserialized.Root);
        Assert.Single(deserialized.Root.Children);
        Assert.Equal("HealthBar", deserialized.Root.Children[0].Name);
    }

    [Fact]
    public void OutlineFormatter_FormatsCompactMarkdown()
    {
        var tree = new HierarchyTree
        {
            TargetName = "Settings",
            TargetType = "Prefab",
            Root = new UiNode
            {
                Id = "/Settings",
                Name = "Settings",
                Rect = new RectTransformData
                {
                    AnchorMin = new Vector2Data(0, 0),
                    AnchorMax = new Vector2Data(1, 1),
                    ResolvedRect = new ResolvedRectData(0, 0, 1920, 1080)
                },
                Children = new List<UiNode>
                {
                    new()
                    {
                        Id = "/Settings/CloseBtn",
                        Name = "CloseBtn",
                        Rect = new RectTransformData
                        {
                            ResolvedRect = new ResolvedRectData(1800, 1000, 48, 48)
                        },
                        Components = new List<UiComponentData>
                        {
                            new()
                            {
                                TypeName = "Button",
                                Properties = new() { ["interactable"] = true },
                                Listeners = new List<ButtonListenerData>
                                {
                                    new("SettingsController", "SettingsController", "OnClose", "RuntimeOnly")
                                }
                            }
                        }
                    }
                }
            }
        };

        var formatter = new UiOutlineFormatter();
        string md = formatter.Format(tree);

        Assert.Contains("# UI Outline: Settings", md);
        Assert.Contains("- **Settings**", md);
        Assert.Contains("**CloseBtn**", md);
        Assert.Contains("[BTN -> SettingsController.OnClose()]", md);
    }
}

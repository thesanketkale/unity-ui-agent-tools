using System;
using System.Collections.Generic;
using System.Linq;

namespace UiContext.Core.Models;

/// <summary>
/// Core domain node representing a GameObject in the UI hierarchy.
/// </summary>
public class UiNode
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool ActiveSelf { get; set; } = true;
    public bool ActiveInHierarchy { get; set; } = true;
    public string Tag { get; set; } = "Untagged";
    public string Layer { get; set; } = "UI";
    public RectTransformData Rect { get; set; } = new();
    public List<UiComponentData> Components { get; set; } = [];
    public PrefabMetadata PrefabInfo { get; set; } = new();
    public List<UiNode> Children { get; set; } = [];

    /// <summary>
    /// Searches for a component by short or full type name.
    /// </summary>
    public UiComponentData? GetComponent(string typeName)
    {
        return Components.Find(c => 
            string.Equals(c.TypeName, typeName, StringComparison.OrdinalIgnoreCase) ||
            c.TypeName.EndsWith("." + typeName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if this node contains a component of the specified type.
    /// </summary>
    public bool HasComponent(string typeName) => GetComponent(typeName) != null;

    /// <summary>
    /// Determines if this node represents an interactive element (Button, Toggle, Slider, etc.).
    /// </summary>
    public bool IsInteractable()
    {
        var button = GetComponent("Button");
        var toggle = GetComponent("Toggle");
        var slider = GetComponent("Slider");
        var scrollbar = GetComponent("Scrollbar");
        var dropdown = GetComponent("TMP_Dropdown") ?? GetComponent("Dropdown");
        var inputField = GetComponent("TMP_InputField") ?? GetComponent("InputField");

        var selectable = button ?? toggle ?? slider ?? scrollbar ?? dropdown ?? inputField;
        if (selectable == null) return false;

        return selectable.GetProperty("interactable", true);
    }

    /// <summary>
    /// Returns text content from TMP_Text or Text component if present.
    /// </summary>
    public string? GetTextContent()
    {
        var tmp = GetComponent("TextMeshProUGUI") ?? GetComponent("TMP_Text") ?? GetComponent("Text");
        return tmp?.GetProperty<string>("text");
    }

    /// <summary>
    /// Flattens the hierarchy into an enumerable sequence of all nodes.
    /// </summary>
    public IEnumerable<UiNode> Flatten()
    {
        yield return this;
        foreach (var child in Children)
        {
            foreach (var descendant in child.Flatten())
            {
                yield return descendant;
            }
        }
    }
}

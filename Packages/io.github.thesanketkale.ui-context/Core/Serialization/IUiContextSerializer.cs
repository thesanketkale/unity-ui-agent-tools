using UiContext.Core.Models;

namespace UiContext.Core.Serialization;

/// <summary>
/// Contract for serializing and deserializing UI hierarchies.
/// </summary>
public interface IUiContextSerializer
{
    string Serialize(HierarchyTree tree, bool indented = true);
    HierarchyTree? Deserialize(string json);
}

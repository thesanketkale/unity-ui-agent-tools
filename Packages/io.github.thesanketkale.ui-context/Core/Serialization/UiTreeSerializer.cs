using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using UiContext.Core.Models;

namespace UiContext.Core.Serialization;

/// <summary>
/// Deterministic JSON serializer for UI tree hierarchies.
/// </summary>
public class UiTreeSerializer : IUiContextSerializer
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly JsonSerializerOptions CompactOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public string Serialize(HierarchyTree tree, bool indented = true)
    {
        tree.RecomputeStatistics();
        return JsonSerializer.Serialize(tree, indented ? DefaultOptions : CompactOptions);
    }

    public HierarchyTree? Deserialize(string json)
    {
        return JsonSerializer.Deserialize<HierarchyTree>(json, DefaultOptions);
    }
}

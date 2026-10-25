using System;
using System.Collections.Generic;

namespace UiContext.Core.Models;

/// <summary>
/// UnityEvent persistent listener call descriptor.
/// </summary>
public record ButtonListenerData(string TargetObjectName, string TargetComponentType, string MethodName, string Mode);

/// <summary>
/// Serializable UI component descriptor with dynamic properties.
/// </summary>
public class UiComponentData
{
    public string TypeName { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public Dictionary<string, object?> Properties { get; set; } = new(StringComparer.Ordinal);
    public List<ButtonListenerData> Listeners { get; set; } = [];

    public T? GetProperty<T>(string key, T? defaultValue = default)
    {
        if (Properties.TryGetValue(key, out var val) && val != null)
        {
            if (val is T typed) return typed;
            if (val is System.Text.Json.JsonElement element)
            {
                try
                {
                    return System.Text.Json.JsonSerializer.Deserialize<T>(element.GetRawText());
                }
                catch
                {
                    return defaultValue;
                }
            }
        }
        return defaultValue;
    }
}

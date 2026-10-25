using System;
using System.Collections.Generic;

namespace UiContext.Core.Models;

/// <summary>
/// Prefab instance linkage and override metadata.
/// </summary>
public class PrefabMetadata
{
    public bool IsPrefabInstance { get; set; }
    public string? SourceAssetGuid { get; set; }
    public string? SourceAssetPath { get; set; }
    public List<string> PropertyOverrides { get; set; } = [];
}

using System;
using System.Collections.Generic;

namespace UiContext.Core.Config;

/// <summary>
/// Project-level configuration for UI Context inspection and validation.
/// </summary>
public class UiContextConfig
{
    public string TargetPlatform { get; set; } = "mobile"; // desktop, mobile, xr
    public float MinInteractableWidth { get; set; } = 48f;
    public float MinInteractableHeight { get; set; } = 48f;
    public int MaxLayoutGroupDepth { get; set; } = 3;
    public bool CheckNonInteractiveRaycasts { get; set; } = true;
    public bool FailOnDuplicateSiblingNames { get; set; } = true;

    public List<string> SerializedFieldAllowList { get; set; } = [
        "m_Text", "m_FontSize", "m_Color", "m_Interactable", "m_RaycastTarget"
    ];

    public List<string> SerializedFieldDenyList { get; set; } = [
        "m_Script", "m_ObjectHideFlags"
    ];

    public Dictionary<string, string> RuleSeverities { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ["UI001"] = "error",
        ["UI002"] = "error",
        ["UI003"] = "warning",
        ["UI004"] = "warning",
        ["UI005"] = "warning",
        ["UI006"] = "note",
        ["UI007"] = "note",
        ["UI008"] = "warning",
        ["UI009"] = "warning"
    };

    public static UiContextConfig CreateDefault(string platform = "mobile")
    {
        var config = new UiContextConfig { TargetPlatform = platform };
        if (string.Equals(platform, "xr", StringComparison.OrdinalIgnoreCase))
        {
            config.MinInteractableWidth = 64f;
            config.MinInteractableHeight = 64f;
        }
        else if (string.Equals(platform, "desktop", StringComparison.OrdinalIgnoreCase))
        {
            config.MinInteractableWidth = 32f;
            config.MinInteractableHeight = 32f;
        }
        return config;
    }
}

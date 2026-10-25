namespace UiContext.Core.Models;

/// <summary>
/// CanvasScaler configuration options.
/// </summary>
public class CanvasScalerData
{
    public string UiScaleMode { get; set; } = "ScaleWithScreenSize";
    public Vector2Data ReferenceResolution { get; set; } = new(1920f, 1080f);
    public string ScreenMatchMode { get; set; } = "MatchWidthOrHeight";
    public float MatchWidthOrHeight { get; set; } = 0.5f;
    public float ReferencePixelsPerUnit { get; set; } = 100f;
    public float DynamicPixelsPerUnit { get; set; } = 1f;
}

/// <summary>
/// Canvas root component data.
/// </summary>
public class CanvasData
{
    public string RenderMode { get; set; } = "ScreenSpaceOverlay";
    public bool PixelPerfect { get; set; }
    public int SortingOrder { get; set; }
    public int TargetDisplay { get; set; }
    public CanvasScalerData? Scaler { get; set; } = new();
}

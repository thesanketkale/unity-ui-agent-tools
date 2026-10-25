namespace UiContext.Core.Models;

/// <summary>
/// Serializable 2D vector for rect transformations.
/// </summary>
public record Vector2Data(float X, float Y)
{
    public static readonly Vector2Data Zero = new(0f, 0f);
    public static readonly Vector2Data One = new(1f, 1f);
    public static readonly Vector2Data Half = new(0.5f, 0.5f);
}

/// <summary>
/// Serializable 4D vector for offsets, insets, or margins.
/// </summary>
public record Vector4Data(float X, float Y, float Z, float W);

/// <summary>
/// Computed screen/canvas pixel rectangle.
/// </summary>
public record ResolvedRectData(float X, float Y, float Width, float Height)
{
    public float Left => X;
    public float Bottom => Y;
    public float Right => X + Width;
    public float Top => Y + Height;

    /// <summary>
    /// Checks whether this resolved rect intersects another rect.
    /// </summary>
    public bool Intersects(ResolvedRectData other)
    {
        return Left < other.Right &&
               Right > other.Left &&
               Bottom < other.Top &&
               Top > other.Bottom;
    }

    /// <summary>
    /// Checks whether this rect is fully contained within a boundary.
    /// </summary>
    public bool IsContainedWithin(ResolvedRectData boundary)
    {
        return Left >= boundary.Left &&
               Right <= boundary.Right &&
               Bottom >= boundary.Bottom &&
               Top <= boundary.Top;
    }
}

/// <summary>
/// RectTransform geometric properties.
/// </summary>
public class RectTransformData
{
    public Vector2Data AnchorMin { get; set; } = Vector2Data.Half;
    public Vector2Data AnchorMax { get; set; } = Vector2Data.Half;
    public Vector2Data Pivot { get; set; } = Vector2Data.Half;
    public Vector2Data AnchoredPosition { get; set; } = Vector2Data.Zero;
    public Vector2Data SizeDelta { get; set; } = new(100f, 100f);
    public Vector2Data OffsetMin { get; set; } = Vector2Data.Zero;
    public Vector2Data OffsetMax { get; set; } = Vector2Data.Zero;
    public Vector2Data LocalScale { get; set; } = Vector2Data.One;
    public float LocalRotationZ { get; set; }
    public ResolvedRectData ResolvedRect { get; set; } = new(0f, 0f, 100f, 100f);

    /// <summary>
    /// Returns shorthand anchor notation for compact outline formatting.
    /// </summary>
    public string GetAnchorShorthand()
    {
        bool xStretch = Math.Abs(AnchorMin.X - AnchorMax.X) > 0.001f;
        bool yStretch = Math.Abs(AnchorMin.Y - AnchorMax.Y) > 0.001f;

        if (xStretch && yStretch) return "stretch-stretch";
        if (xStretch) return "stretch-" + GetVerticalAnchorName(AnchorMin.Y);
        if (yStretch) return GetHorizontalAnchorName(AnchorMin.X) + "-stretch";

        return $"{GetVerticalAnchorName(AnchorMin.Y)}-{GetHorizontalAnchorName(AnchorMin.X)}";
    }

    private static string GetHorizontalAnchorName(float x) => x switch
    {
        <= 0.05f => "left",
        >= 0.95f => "right",
        _ => "center"
    };

    private static string GetVerticalAnchorName(float y) => y switch
    {
        <= 0.05f => "bottom",
        >= 0.95f => "top",
        _ => "middle"
    };
}

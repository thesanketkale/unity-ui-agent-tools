using System;
using System.Collections.Generic;
using System.Text;
using UiContext.Core.Models;

namespace UiContext.Core.Formatters;

/// <summary>
/// Formats a UI tree into a compact, token-efficient Markdown outline.
/// </summary>
public class UiOutlineFormatter
{
    public string Format(HierarchyTree tree)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# UI Outline: {tree.TargetName} ({tree.TargetType})");
        sb.AppendLine($"*Exported: {tree.ExportTimestamp} | Unity: {tree.UnityVersion}*");
        if (tree.CanvasContext?.Scaler != null)
        {
            var scaler = tree.CanvasContext.Scaler;
            sb.AppendLine($"*Canvas Ref: {scaler.ReferenceResolution.X}x{scaler.ReferenceResolution.Y} ({scaler.UiScaleMode})*");
        }
        sb.AppendLine();

        if (tree.Root != null)
        {
            FormatNode(tree.Root, sb, 0);
        }
        else
        {
            sb.AppendLine("*(Empty hierarchy)*");
        }

        return sb.ToString();
    }

    private static void FormatNode(UiNode node, StringBuilder sb, int depth)
    {
        string indent = new(' ', depth * 2);
        sb.Append(indent);
        sb.Append("- ");

        // Node name and active status
        sb.Append($"**{node.Name}**");
        if (!node.ActiveSelf)
        {
            sb.Append(" `[inactive]`");
        }

        // Layout rect shorthand
        string anchor = node.Rect.GetAnchorShorthand();
        var r = node.Rect.ResolvedRect;
        sb.Append($" `[{anchor} {r.Width:0}x{r.Height:0}@{r.X:0},{r.Y:0}]`");

        // Component indicators
        var indicators = GetIndicators(node);
        if (indicators.Count > 0)
        {
            sb.Append(' ');
            sb.Append(string.Join(" ", indicators));
        }

        sb.AppendLine();

        foreach (var child in node.Children)
        {
            FormatNode(child, sb, depth + 1);
        }
    }

    private static List<string> GetIndicators(UiNode node)
    {
        var list = new List<string>();

        // Text
        string? text = node.GetTextContent();
        if (!string.IsNullOrEmpty(text))
        {
            string truncated = text.Length > 24 ? text[..21] + "..." : text;
            truncated = truncated.Replace("\n", "\\n").Replace("\r", "");
            list.Add($"\"*{truncated}*\"");
        }

        // Button
        var btn = node.GetComponent("Button");
        if (btn != null)
        {
            string listenerSummary = "";
            if (btn.Listeners.Count > 0)
            {
                var first = btn.Listeners[0];
                listenerSummary = $" -> {first.TargetObjectName}.{first.MethodName}()";
            }
            list.Add($"**[BTN{listenerSummary}]**");
        }

        // Toggle
        var toggle = node.GetComponent("Toggle");
        if (toggle != null)
        {
            bool isOn = toggle.GetProperty("isOn", false);
            list.Add($"**[TOGGLE: {(isOn ? "ON" : "OFF")}]**");
        }

        // Slider
        var slider = node.GetComponent("Slider");
        if (slider != null)
        {
            float val = slider.GetProperty("value", 0f);
            list.Add($"**[SLIDER: {val:0.0}]**");
        }

        // Input Field
        var input = node.GetComponent("TMP_InputField") ?? node.GetComponent("InputField");
        if (input != null)
        {
            string placeholder = input.GetProperty<string>("placeholderText") ?? "input";
            list.Add($"**[INPUT: \"{placeholder}\"]**");
        }

        // Image / Graphic
        var img = node.GetComponent("Image");
        if (img != null)
        {
            bool raycast = img.GetProperty("raycastTarget", true);
            string sprite = img.GetProperty<string>("spriteName") ?? "solid";
            list.Add($"`[IMG: {sprite}{(raycast ? " #raycast" : "")}]`");
        }

        // Prefab source
        if (node.PrefabInfo.IsPrefabInstance && !string.IsNullOrEmpty(node.PrefabInfo.SourceAssetPath))
        {
            string assetName = System.IO.Path.GetFileNameWithoutExtension(node.PrefabInfo.SourceAssetPath);
            list.Add($"*[Prefab: {assetName}]*");
        }

        return list;
    }
}

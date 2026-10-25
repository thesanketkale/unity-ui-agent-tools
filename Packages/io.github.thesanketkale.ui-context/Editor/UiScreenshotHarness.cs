#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UiContext.Core.Models;

namespace UiContext.Editor
{
    public record CaptureResolution(string Name, int Width, int Height, float Scale);

    public class BoundingBoxAnnotation
    {
        public int Number { get; set; }
        public string NodeId { get; set; } = string.Empty;
        public string NodeName { get; set; } = string.Empty;
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
    }

    public class CaptureMetadata
    {
        public string CaptureName { get; set; } = string.Empty;
        public string Resolution { get; set; } = string.Empty;
        public string RawImagePath { get; set; } = string.Empty;
        public string AnnotatedImagePath { get; set; } = string.Empty;
        public List<BoundingBoxAnnotation> Annotations { get; set; } = [];
    }

    /// <summary>
    /// Screenshot harness capturing rendered UI across resolution matrices with numbered bounding boxes.
    /// </summary>
    public static class UiScreenshotHarness
    {
        public static readonly CaptureResolution[] StandardMatrix = new[]
        {
            new CaptureResolution("Desktop_1080p", 1920, 1080, 1.0f),
            new CaptureResolution("Mobile_Portrait_FHD", 1080, 2400, 1.0f),
            new CaptureResolution("Tablet_Landscape", 2048, 1536, 1.0f)
        };

        public static List<CaptureMetadata> CapturePrefab(GameObject prefab, string outDir, CaptureResolution[]? resolutions = null)
        {
            resolutions ??= StandardMatrix;
            Directory.CreateDirectory(outDir);
            var results = new List<CaptureMetadata>();

            // Create temporary preview scene
            Scene previewScene = EditorSceneManager.NewPreviewScene();
            try
            {
                var instance = UnityEngine.Object.Instantiate(prefab);
                SceneManager.MoveGameObjectToScene(instance, previewScene);

                // Setup camera
                var camGo = new GameObject("CaptureCamera");
                SceneManager.MoveGameObjectToScene(camGo, previewScene);
                var cam = camGo.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.12f, 0.12f, 0.14f, 1f);
                cam.orthographic = true;

                var canvas = instance.GetComponentInParent<Canvas>() ?? instance.GetComponentInChildren<Canvas>();
                if (canvas != null)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = cam;
                }

                foreach (var res in resolutions)
                {
                    string baseName = $"{prefab.name}_{res.Name}";
                    string rawPath = Path.Combine(outDir, $"{baseName}.png");
                    string annotatedPath = Path.Combine(outDir, $"{baseName}_annotated.png");

                    var rt = new RenderTexture(res.Width, res.Height, 24, RenderTextureFormat.ARGB32);
                    cam.targetTexture = rt;
                    cam.Render();

                    RenderTexture.active = rt;
                    var tex = new Texture2D(res.Width, res.Height, TextureFormat.RGB24, false);
                    tex.ReadPixels(new Rect(0, 0, res.Width, res.Height), 0, 0);
                    tex.Apply();

                    File.WriteAllBytes(rawPath, tex.EncodeToPNG());

                    // Generate annotations
                    var metadata = new CaptureMetadata
                    {
                        CaptureName = baseName,
                        Resolution = $"{res.Width}x{res.Height}",
                        RawImagePath = rawPath,
                        AnnotatedImagePath = annotatedPath
                    };

                    int number = 1;
                    var uiElements = instance.GetComponentsInChildren<RectTransform>(true);
                    foreach (var elem in uiElements)
                    {
                        var selectable = elem.GetComponent<Selectable>();
                        if (selectable != null && selectable.interactable)
                        {
                            metadata.Annotations.Add(new BoundingBoxAnnotation
                            {
                                Number = number++,
                                NodeId = "/" + elem.name,
                                NodeName = elem.name,
                                X = elem.position.x,
                                Y = elem.position.y,
                                Width = elem.rect.width,
                                Height = elem.rect.height
                            });
                        }
                    }

                    // Save annotated texture placeholder copy
                    File.Copy(rawPath, annotatedPath, overwrite: true);
                    results.Add(metadata);

                    RenderTexture.active = null;
                    cam.targetTexture = null;
                    UnityEngine.Object.DestroyImmediate(rt);
                    UnityEngine.Object.DestroyImmediate(tex);
                }

                string json = JsonSerializer.Serialize(results, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
                File.WriteAllText(Path.Combine(outDir, "captures.json"), json);
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(previewScene);
            }

            return results;
        }
    }
}
#endif

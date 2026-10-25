#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Sample.Editor
{
    /// <summary>
    /// Utility for generating synthetic UI textures and test sprites with zero external art assets.
    /// </summary>
    public static class SyntheticUiGenerator
    {
        [MenuItem("Tools/Sample/Generate Synthetic Textures")]
        public static void GenerateTextures()
        {
            string dir = "Assets/Textures";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            CreateSolidTexture(Path.Combine(dir, "ButtonBackground.png"), 128, 64, new Color(0.2f, 0.45f, 0.85f, 1f));
            CreateSolidTexture(Path.Combine(dir, "PanelBackground.png"), 256, 256, new Color(0.15f, 0.16f, 0.18f, 0.95f));
            CreateSolidTexture(Path.Combine(dir, "HealthFill.png"), 128, 32, new Color(0.85f, 0.2f, 0.2f, 1f));

            AssetDatabase.Refresh();
            Debug.Log("[SyntheticUiGenerator] Generated synthetic UI textures.");
        }

        private static void CreateSolidTexture(string path, int width, int height, Color color)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var colors = new Color[width * height];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = color;
            }
            tex.SetPixels(colors);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }
    }
}
#endif

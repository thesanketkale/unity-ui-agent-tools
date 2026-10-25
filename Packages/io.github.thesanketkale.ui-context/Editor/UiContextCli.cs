#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UiContext.Core.Config;
using UiContext.Core.Formatters;
using UiContext.Core.Rules;
using UiContext.Core.Serialization;
using UiContext.Core.Validation;

namespace UiContext.Editor
{
    /// <summary>
    /// Batchmode CLI entry points for external automation and AI agent tools.
    /// </summary>
    public static class UiContextCli
    {
        /// <summary>
        /// Entry point: Unity -batchmode -executeMethod UiContext.Cli.Export -uiContext.target <asset> -uiContext.out <dir>
        /// </summary>
        public static void Export()
        {
            string[] args = Environment.GetCommandLineArgs();
            string? targetPath = GetArgValue(args, "-uiContext.target");
            string? outDir = GetArgValue(args, "-uiContext.out") ?? "ui-context-out";
            bool failOnErrors = HasArg(args, "-uiContext.failOnErrors");

            if (string.IsNullOrEmpty(targetPath))
            {
                Debug.LogError("[UiContext] Error: -uiContext.target argument is required.");
                EditorApplication.Exit(1);
                return;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(targetPath);
            if (prefab == null)
            {
                Debug.LogError($"[UiContext] Error: Could not load target GameObject at '{targetPath}'.");
                EditorApplication.Exit(2);
                return;
            }

            try
            {
                Directory.CreateDirectory(outDir);
                var config = UiContextConfig.CreateDefault();

                // 1. Extract tree
                var tree = UiHierarchyWalker.ExtractTree(prefab, config);

                // 2. Serialize JSON
                var serializer = new UiTreeSerializer();
                string json = serializer.Serialize(tree, indented: true);
                string jsonPath = Path.Combine(outDir, "ui-tree.json");
                File.WriteAllText(jsonPath, json);
                Debug.Log($"[UiContext] Exported tree JSON: {jsonPath}");

                // 3. Generate Markdown Outline
                var outliner = new UiOutlineFormatter();
                string outline = outliner.Format(tree);
                string outlinePath = Path.Combine(outDir, "ui-outline.md");
                File.WriteAllText(outlinePath, outline);
                Debug.Log($"[UiContext] Exported outline Markdown: {outlinePath}");

                // 4. Run Validation
                var registry = new RuleRegistry();
                var valResult = registry.Validate(tree, config);
                string sarif = SarifExporter.ToSarifJson(valResult);
                string sarifPath = Path.Combine(outDir, "ui-validation.json");
                File.WriteAllText(sarifPath, sarif);

                string valMd = SarifExporter.ToMarkdownSummary(valResult);
                string valMdPath = Path.Combine(outDir, "ui-validation.md");
                File.WriteAllText(valMdPath, valMd);
                Debug.Log($"[UiContext] Exported validation reports: {sarifPath}, {valMdPath}");

                if (failOnErrors && valResult.HasErrors)
                {
                    Debug.LogError($"[UiContext] Validation failed with {valResult.ErrorCount} error(s).");
                    EditorApplication.Exit(3);
                    return;
                }

                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UiContext] Exception during export: {ex}");
                EditorApplication.Exit(2);
            }
        }

        private static string? GetArgValue(string[] args, string flag)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], flag, StringComparison.OrdinalIgnoreCase))
                {
                    return args[i + 1];
                }
            }
            return null;
        }

        private static bool HasArg(string[] args, string flag)
        {
            foreach (var arg in args)
            {
                if (string.Equals(arg, flag, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
#endif

#if UNITY_EDITOR
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UiContext.Core.Config;
using UiContext.Core.Rules;
using UiContext.Core.Validation;
using UiContext.Editor;

namespace Sample.Tests.Editor
{
    public class UiContextSampleValidationTests
    {
        [Test]
        public void HUDPrefab_ContainsZeroValidationErrors()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/HUD.prefab");
            Assert.IsNotNull(prefab, "HUD.prefab should exist in Assets/Prefabs/");

            var config = UiContextConfig.CreateDefault();
            var tree = UiHierarchyWalker.ExtractTree(prefab, config);
            var registry = new RuleRegistry();
            var result = registry.Validate(tree, config);

            UiContextAssert.NoErrors(result);
        }

        [Test]
        public void SettingsDialogPrefab_ContainsZeroValidationErrors()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/SettingsDialog.prefab");
            Assert.IsNotNull(prefab, "SettingsDialog.prefab should exist in Assets/Prefabs/");

            var config = UiContextConfig.CreateDefault();
            var tree = UiHierarchyWalker.ExtractTree(prefab, config);
            var registry = new RuleRegistry();
            var result = registry.Validate(tree, config);

            UiContextAssert.NoErrors(result);
        }

        [Test]
        public void CharacterStatusPrefab_ContainsZeroValidationErrors()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/CharacterStatus.prefab");
            Assert.IsNotNull(prefab, "CharacterStatus.prefab should exist in Assets/Prefabs/");

            var config = UiContextConfig.CreateDefault();
            var tree = UiHierarchyWalker.ExtractTree(prefab, config);
            var registry = new RuleRegistry();
            var result = registry.Validate(tree, config);

            UiContextAssert.NoErrors(result);
        }
    }
}
#endif

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UiContext.Core.Config;
using UiContext.Core.Models;

namespace UiContext.Editor
{
    /// <summary>
    /// Walks live Unity scene or prefab hierarchies to build pure UiNode data trees.
    /// </summary>
    public static class UiHierarchyWalker
    {
        public static HierarchyTree ExtractTree(GameObject rootObject, UiContextConfig? config = null)
        {
            config ??= UiContextConfig.CreateDefault();
            var tree = new HierarchyTree
            {
                TargetName = rootObject.name,
                TargetType = PrefabUtility.IsPartOfPrefabAsset(rootObject) ? "Prefab" : "SceneObject",
                UnityVersion = Application.unityVersion,
                ExportTimestamp = DateTime.UtcNow.ToString("o")
            };

            var canvas = rootObject.GetComponentInParent<Canvas>() ?? rootObject.GetComponentInChildren<Canvas>();
            if (canvas != null)
            {
                tree.CanvasContext = ExtractCanvasData(canvas);
            }

            tree.Root = WalkNode(rootObject.transform, "", tree.CanvasContext, config);
            tree.RecomputeStatistics();
            return tree;
        }

        private static UiNode WalkNode(Transform current, string parentPath, CanvasData? canvasContext, UiContextConfig config)
        {
            string currentPath = string.IsNullOrEmpty(parentPath) ? "/" + current.name : parentPath + "/" + current.name;
            var node = new UiNode
            {
                Id = currentPath,
                Name = current.name,
                ActiveSelf = current.gameObject.activeSelf,
                ActiveInHierarchy = current.gameObject.activeInHierarchy,
                Tag = current.gameObject.tag,
                Layer = LayerMask.LayerToName(current.gameObject.layer)
            };

            if (current is RectTransform rt)
            {
                node.Rect = ExtractRectTransform(rt, canvasContext);
            }

            node.Components = ComponentExtractors.ExtractComponents(current.gameObject, config);
            node.PrefabInfo = ExtractPrefabMetadata(current.gameObject);

            for (int i = 0; i < current.childCount; i++)
            {
                var childTransform = current.GetChild(i);
                node.Children.Add(WalkNode(childTransform, currentPath, canvasContext, config));
            }

            return node;
        }

        private static RectTransformData ExtractRectTransform(RectTransform rt, CanvasData? canvasContext)
        {
            var data = new RectTransformData
            {
                AnchorMin = new Vector2Data(rt.anchorMin.x, rt.anchorMin.y),
                AnchorMax = new Vector2Data(rt.anchorMax.x, rt.anchorMax.y),
                Pivot = new Vector2Data(rt.pivot.x, rt.pivot.y),
                AnchoredPosition = new Vector2Data(rt.anchoredPosition.x, rt.anchoredPosition.y),
                SizeDelta = new Vector2Data(rt.sizeDelta.x, rt.sizeDelta.y),
                OffsetMin = new Vector2Data(rt.offsetMin.x, rt.offsetMin.y),
                OffsetMax = new Vector2Data(rt.offsetMax.x, rt.offsetMax.y),
                LocalScale = new Vector2Data(rt.localScale.x, rt.localScale.y),
                LocalRotationZ = rt.localEulerAngles.z
            };

            Rect rect = rt.rect;
            Vector3 worldPos = rt.position;
            data.ResolvedRect = new ResolvedRectData(worldPos.x, worldPos.y, rect.width, rect.height);

            return data;
        }

        private static CanvasData ExtractCanvasData(Canvas canvas)
        {
            var data = new CanvasData
            {
                RenderMode = canvas.renderMode.ToString(),
                PixelPerfect = canvas.pixelPerfect,
                SortingOrder = canvas.sortingOrder,
                TargetDisplay = canvas.targetDisplay
            };

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                data.Scaler = new CanvasScalerData
                {
                    UiScaleMode = scaler.uiScaleMode.ToString(),
                    ReferenceResolution = new Vector2Data(scaler.referenceResolution.x, scaler.referenceResolution.y),
                    ScreenMatchMode = scaler.screenMatchMode.ToString(),
                    MatchWidthOrHeight = scaler.matchWidthOrHeight,
                    ReferencePixelsPerUnit = scaler.referencePixelsPerUnit,
                    DynamicPixelsPerUnit = scaler.dynamicPixelsPerUnit
                };
            }

            return data;
        }

        private static PrefabMetadata ExtractPrefabMetadata(GameObject go)
        {
            var meta = new PrefabMetadata
            {
                IsPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(go)
            };

            if (meta.IsPrefabInstance)
            {
                var originalSource = PrefabUtility.GetCorrespondingObjectFromSource(go);
                if (originalSource != null)
                {
                    meta.SourceAssetPath = AssetDatabase.GetAssetPath(originalSource);
                    meta.SourceAssetGuid = AssetDatabase.AssetPathToGUID(meta.SourceAssetPath);
                }

                var modifications = PrefabUtility.GetPropertyModifications(go);
                if (modifications != null)
                {
                    foreach (var mod in modifications)
                    {
                        if (mod != null && !string.IsNullOrEmpty(mod.propertyPath))
                        {
                            meta.PropertyOverrides.Add(mod.propertyPath);
                        }
                    }
                }
            }

            return meta;
        }
    }
}
#endif

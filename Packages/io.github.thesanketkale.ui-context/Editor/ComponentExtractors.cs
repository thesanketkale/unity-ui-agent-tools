#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UiContext.Core.Config;
using UiContext.Core.Models;

namespace UiContext.Editor
{
    /// <summary>
    /// Extracts property dictionaries and listener lists from uGUI and TextMeshPro components.
    /// </summary>
    public static class ComponentExtractors
    {
        public static List<UiComponentData> ExtractComponents(GameObject go, UiContextConfig config)
        {
            var results = new List<UiComponentData>();
            var components = go.GetComponents<Component>();

            foreach (var comp in components)
            {
                if (comp == null)
                {
                    results.Add(new UiComponentData
                    {
                        TypeName = "MissingScript",
                        Enabled = false,
                        Properties = new Dictionary<string, object?> { ["isMissingScript"] = true }
                    });
                    continue;
                }

                if (comp is Transform) continue; // Handled in rect transform

                var data = new UiComponentData
                {
                    TypeName = comp.GetType().Name
                };

                if (comp is Behaviour b)
                {
                    data.Enabled = b.enabled;
                }

                ExtractSpecificComponent(comp, data, config);
                results.Add(data);
            }

            return results;
        }

        private static void ExtractSpecificComponent(Component comp, UiComponentData data, UiContextConfig config)
        {
            switch (comp)
            {
                case TextMeshProUGUI tmp:
                    data.Properties["text"] = tmp.text;
                    data.Properties["fontSize"] = tmp.fontSize;
                    data.Properties["color"] = ColorUtility.ToHtmlStringRGBA(tmp.color);
                    data.Properties["enableWordWrapping"] = tmp.enableWordWrapping;
                    data.Properties["overflowMode"] = tmp.overflowMode.ToString();
                    data.Properties["alignment"] = tmp.alignment.ToString();
                    data.Properties["raycastTarget"] = tmp.raycastTarget;
                    break;

                case Text text:
                    data.Properties["text"] = text.text;
                    data.Properties["fontSize"] = text.fontSize;
                    data.Properties["color"] = ColorUtility.ToHtmlStringRGBA(text.color);
                    data.Properties["alignment"] = text.alignment.ToString();
                    data.Properties["horizontalOverflow"] = text.horizontalOverflow.ToString();
                    data.Properties["verticalOverflow"] = text.verticalOverflow.ToString();
                    data.Properties["raycastTarget"] = text.raycastTarget;
                    break;

                case Image img:
                    data.Properties["spriteName"] = img.sprite != null ? img.sprite.name : null;
                    data.Properties["color"] = ColorUtility.ToHtmlStringRGBA(img.color);
                    data.Properties["type"] = img.type.ToString();
                    data.Properties["raycastTarget"] = img.raycastTarget;
                    data.Properties["fillAmount"] = img.fillAmount;
                    break;

                case RawImage raw:
                    data.Properties["textureName"] = raw.texture != null ? raw.texture.name : null;
                    data.Properties["color"] = ColorUtility.ToHtmlStringRGBA(raw.color);
                    data.Properties["raycastTarget"] = raw.raycastTarget;
                    break;

                case Button btn:
                    data.Properties["interactable"] = btn.interactable;
                    data.Listeners = ExtractUnityEventListeners(btn.onClick);
                    break;

                case Toggle toggle:
                    data.Properties["interactable"] = toggle.interactable;
                    data.Properties["isOn"] = toggle.isOn;
                    data.Listeners = ExtractUnityEventListeners(toggle.onValueChanged);
                    break;

                case Slider slider:
                    data.Properties["interactable"] = slider.interactable;
                    data.Properties["value"] = slider.value;
                    data.Properties["minValue"] = slider.minValue;
                    data.Properties["maxValue"] = slider.maxValue;
                    break;

                case Scrollbar scrollbar:
                    data.Properties["interactable"] = scrollbar.interactable;
                    data.Properties["value"] = scrollbar.value;
                    data.Properties["size"] = scrollbar.size;
                    break;

                case ScrollRect scrollRect:
                    data.Properties["horizontal"] = scrollRect.horizontal;
                    data.Properties["vertical"] = scrollRect.vertical;
                    data.Properties["movementType"] = scrollRect.movementType.ToString();
                    break;

                case HorizontalOrVerticalLayoutGroup layoutGroup:
                    data.Properties["spacing"] = layoutGroup.spacing;
                    data.Properties["childAlignment"] = layoutGroup.childAlignment.ToString();
                    data.Properties["childControlWidth"] = layoutGroup.childControlWidth;
                    data.Properties["childControlHeight"] = layoutGroup.childControlHeight;
                    data.Properties["childForceExpandWidth"] = layoutGroup.childForceExpandWidth;
                    data.Properties["childForceExpandHeight"] = layoutGroup.childForceExpandHeight;
                    break;

                case GridLayoutGroup grid:
                    data.Properties["cellSize"] = $"{grid.cellSize.x}x{grid.cellSize.y}";
                    data.Properties["spacing"] = $"{grid.spacing.x}x{grid.spacing.y}";
                    data.Properties["constraint"] = grid.constraint.ToString();
                    break;

                case ContentSizeFitter fitter:
                    data.Properties["horizontalFit"] = fitter.horizontalFit.ToString();
                    data.Properties["verticalFit"] = fitter.verticalFit.ToString();
                    break;

                case LayoutElement elem:
                    data.Properties["ignoreLayout"] = elem.ignoreLayout;
                    data.Properties["minWidth"] = elem.minWidth;
                    data.Properties["minHeight"] = elem.minHeight;
                    data.Properties["preferredWidth"] = elem.preferredWidth;
                    data.Properties["preferredHeight"] = elem.preferredHeight;
                    break;

                case CanvasGroup cg:
                    data.Properties["alpha"] = cg.alpha;
                    data.Properties["interactable"] = cg.interactable;
                    data.Properties["blocksRaycasts"] = cg.blocksRaycasts;
                    data.Properties["ignoreParentGroups"] = cg.ignoreParentGroups;
                    break;

                default:
                    ExtractCustomMonoBehaviour(comp, data, config);
                    break;
            }
        }

        private static void ExtractCustomMonoBehaviour(Component comp, UiComponentData data, UiContextConfig config)
        {
            var so = new SerializedObject(comp);
            var prop = so.GetIterator();

            if (prop.NextVisible(true))
            {
                do
                {
                    if (config.SerializedFieldDenyList.Contains(prop.name)) continue;

                    if (config.SerializedFieldAllowList.Count == 0 || config.SerializedFieldAllowList.Contains(prop.name))
                    {
                        switch (prop.propertyType)
                        {
                            case SerializedPropertyType.Integer:
                                data.Properties[prop.name] = prop.intValue;
                                break;
                            case SerializedPropertyType.Boolean:
                                data.Properties[prop.name] = prop.boolValue;
                                break;
                            case SerializedPropertyType.Float:
                                data.Properties[prop.name] = prop.floatValue;
                                break;
                            case SerializedPropertyType.String:
                                data.Properties[prop.name] = prop.stringValue;
                                break;
                        }
                    }
                } while (prop.NextVisible(false));
            }
        }

        private static List<ButtonListenerData> ExtractUnityEventListeners(UnityEventBase evt)
        {
            var list = new List<ButtonListenerData>();
            int count = evt.GetPersistentEventCount();

            for (int i = 0; i < count; i++)
            {
                var target = evt.GetPersistentTarget(i);
                string methodName = evt.GetPersistentMethodName(i);
                string targetName = target != null ? target.name : "None";
                string targetType = target != null ? target.GetType().Name : "None";
                string mode = evt.GetPersistentListenerState(i).ToString();

                list.Add(new ButtonListenerData(targetName, targetType, methodName, mode));
            }

            return list;
        }
    }
}
#endif

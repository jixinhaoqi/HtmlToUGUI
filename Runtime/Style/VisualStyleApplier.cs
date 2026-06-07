using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Xxhq.Htmltougui
{
    /// <summary>
    /// 通用样式应用器：处理背景、边框、透明度、伪类样式等视觉属性
    /// </summary>
    public static class VisualStyleApplier
    {
        /// <summary>
        /// 图片加载回调，由 Editor/Runtime 各自注入实现。
        /// 参数：(GameObject, srcUrl, htmlFilePath) -> 是否成功加载
        /// </summary>
        public static System.Func<GameObject, string, string, bool> LoadImageFunc { get; set; }

        public static readonly string[] SelectablePseudoClassStyles = { "hover", "active", "enabled", "disabled", "selected", "focus", "checked" };
        private static readonly float _imgRaycastAlphaThreshold = 0.1f; // 透明度阈值，低于该值时不响应点击事件
        /// <summary>
        /// 应用于 GameObject 的通用视觉样式（背景色/图、边框、透明度、伪类交互颜色）
        /// </summary>
        /// <param name="go">目标 GameObject</param>
        /// <param name="styles">样式字典</param>
        /// <param name="pseudoClassStyles">伪类样式字典</param>
        /// <param name="htmlFilePath">HTML 文件路径，用于加载图片</param>
        public static void ApplyCommonStyles(GameObject go, Dictionary<string, string> styles,
            Dictionary<string, Dictionary<string, string>> pseudoClassStyles, string htmlFilePath)
        {
            if (!go) return;

            ApplyBackgroundAndBorder(go, styles, htmlFilePath);
            ApplyOpacity(go, styles);
            ApplyPseudoClassStyles(go, pseudoClassStyles);
            AlphaTipWarning(go);
        }
        /// <summary>
        /// 透明度警告提示：如果图片的透明度很低，则不响应点击事件。此处仅作提示，不做阻断处理。
        /// </summary>
        /// <param name="go">目标 GameObject</param>
        public static void AlphaTipWarning(GameObject go)
        {
            if (go.GetComponentInChildren<MaskableGraphic>() is MaskableGraphic img)
            {
                if (img.color.a < _imgRaycastAlphaThreshold)
                {
                    Debug.LogWarning($"{go.name} 的颜色透明度很低（{img.color.a:F2}），可考虑删除组件", go);
                }
            }
        }
        /// <summary>
        /// 背景色/图、边框样式应用：支持图片和颜色混合，但不支持渐变。
        /// </summary>
        /// <param name="go">目标 GameObject</param>
        /// <param name="styles">样式字典</param>
        /// <param name="htmlFilePath">HTML 文件路径，用于加载图片</param>
        public static void ApplyBackgroundAndBorder(GameObject go, Dictionary<string, string> styles, string htmlFilePath)
        {
            foreach (var kv in styles)
            {
                if (ColorParser.IsBackgroundColor(kv.Key))
                {
                    var img = go.GetComponentInChildren<MaskableGraphic>();
                    if (img != null)
                    {
                        if (ColorParser.BackgroundUrlRegex.IsMatch(kv.Value))
                        {
                            var urls = ColorParser.ExtractCssUrls(kv.Value);
                            string url = urls?.Count > 0 ? urls[0] : "";
                            bool success = LoadImageFunc != null? LoadImageFunc(go, url, htmlFilePath):false;
                            if (!success) img.color = ColorParser.Parse(kv.Value, Color.black);
                        }
                        else
                        {
                            img.color = ColorParser.Parse(kv.Value, Color.black);
                        }
                        if(img.color.a< _imgRaycastAlphaThreshold)
                            img.raycastTarget = false; // 透明度很低时不响应点击事件
                    }
                }

                // 边框处理
                if (kv.Key.StartsWith("border"))
                    ApplyBorder(go, kv.Key, kv.Value);
            }
        }
        /// <summary>
        /// 透明度样式应用：作用于所有 MaskableGraphic 组件。
        /// </summary>
        /// <param name="go">目标 GameObject</param>
        /// <param name="styles">样式字典</param>
        public static void ApplyOpacity(GameObject go, Dictionary<string, string> styles)
        {
            if (!styles.TryGetValue("opacity", out string opacity)) return;
            var img = go.GetComponentInChildren<MaskableGraphic>();
            if (img != null)
            {
                Color color = img.color;
                color.a = UnitParser.Parse(opacity);
                img.color = color;
                if (img.color.a < _imgRaycastAlphaThreshold)
                    img.raycastTarget = false;
            }
        }
        /// <summary>
        /// 伪类样式应用：应用于 Selectable 组件，支持 hover、active 等状态颜色。
        /// </summary>
        /// <param name="go">目标 GameObject</param>
        /// <param name="key">样式键</param>
        /// <param name="value">样式值</param>
        public static void ApplyBorder(GameObject go, string key, string value)
        {
            if (key == "border-radius" && value != "none" && go.GetComponent<MaskableGraphic>())
                return; // TODO: 边缘圆角

            var width = 1f;
            var color = Color.black;
            bool haveBorder = false;

            if (key == "border-width") { haveBorder = true; width = UnitParser.Parse(value); }
            else if (key == "border-color") { haveBorder = true; color = ColorParser.Parse(value, Color.black); }
            else if (key == "border" && value != "none")
            {
                haveBorder = true;
                string[] split = value.Split(' ');
                if (split.Length == 2)
                {
                    float v = UnitParser.Parse(split[0]);
                    if (v != 0) width = v; else color = ColorParser.Parse(split[1], Color.black);
                }
                else if (split.Length == 3)
                {
                    float v = UnitParser.Parse(split[0]);
                    if (v != 0) width = v;
                    color = ColorParser.Parse(split[2], Color.black);
                }
            }

            if (haveBorder)
            {
                Outline outline = go.GetComponent<Outline>() ?? go.AddComponent<Outline>();
                outline.effectDistance = new Vector2(width, width);
                outline.effectColor = color;
            }
        }
        /// <summary>
        /// 伪类样式应用：应用于 Selectable 组件，支持 hover、active 等状态颜色。
        /// </summary>
        /// <param name="go">目标 GameObject</param>
        /// <param name="pseudoClassStyles">伪类样式字典</param>
        public static void ApplyPseudoClassStyles(GameObject go,
            Dictionary<string, Dictionary<string, string>> pseudoClassStyles)
        {
            if (go.GetComponentInChildren<Selectable>() is Selectable selectable)
            {
                ApplySelectablePseudoColors(selectable, pseudoClassStyles);
            }
            else if (go.GetComponentInChildren<MaskableGraphic>() is MaskableGraphic mg)
            {
                // 如果包含伪类样式但不是按钮，则添加 Button 组件以支持交互状态颜色
                if (mg.raycastTarget&& pseudoClassStyles.Keys.Any(i=> SelectablePseudoClassStyles.Contains(i)))
                {
                    if (!go.name.EndsWith("Btn"))
                    {
                        go.name = UnitParser.AddUnitNumericSuffix(go.name, "Btn");
                    }
                    ApplySelectablePseudoColors(go.AddComponent<Button>(), pseudoClassStyles);
                }
            }
        }
        /// <summary>
        /// 应用于 Selectable 组件的伪类颜色设置。支持 hover、active 等状态颜色。
        /// </summary>
        /// <param name="selectable">目标 Selectable 组件</param>
        /// <param name="pseudoClassStyles">伪类样式字典</param>
        public static void ApplySelectablePseudoColors(Selectable selectable,
            Dictionary<string, Dictionary<string, string>> pseudoClassStyles)
        {
            ColorBlock cb = selectable.colors;
            bool hasColor= false;
            bool isText = false;
            if (selectable.targetGraphic is Graphic mg)
            {
                isText = mg is Text|| mg is TextMeshProUGUI;
                cb.normalColor = mg.color;
                cb.highlightedColor = mg.color;
                cb.pressedColor = mg.color;
                cb.disabledColor = mg.color;
                cb.selectedColor = mg.color;
            }

            var setterMap = new Dictionary<string, Action<Color>>
            {
                { "hover", (co) => cb.highlightedColor = co },
                { "active", (co) => cb.pressedColor = co },
                { "enabled", (co) => cb.normalColor = co },
                { "disabled", (co) => cb.disabledColor = co },
                { "selected", (co) => cb.selectedColor = co },
                { "focus", (co) => cb.selectedColor = co },
                { "checked", (co) => cb.selectedColor = co }
            };

            foreach (var kv in setterMap)
            {
                string pseudoKey = kv.Key;
                var setter = kv.Value;
                if (pseudoClassStyles.ContainsKey(pseudoKey) && (isText && pseudoClassStyles[pseudoKey].ContainsKey("color") &&
                ColorParser.TryParseColor(pseudoClassStyles[pseudoKey]["color"], out Color c) ||
                ColorParser.TryParseBackgroundColor(pseudoClassStyles[pseudoKey], out c)))
                {
                    hasColor = true;
                    setter(c);
                }
            }

            if (hasColor)
            {
                selectable.colors = cb;
                if (selectable.targetGraphic is Graphic mg2)
                {
                    mg2.color = Color.white;
                }
            }
                
        }


        /// <summary>
        /// 应用于下拉菜单项的伪类颜色设置。支持 hover、active 等状态颜色。
        /// </summary>
        /// <param name="selectable">目标 Selectable 组件</param>
        /// <param name="pseudoClassStyles">伪类样式字典</param>
        /// <param name="styles">样式字典</param>
        public static void ApplyDropdownItemPseudoColors(Selectable selectable,
    Dictionary<string, Dictionary<string, string>> pseudoClassStyles, Dictionary<string, string> styles)
        {
            ColorBlock cb = selectable.colors;
            bool hasColor = false;
            if(ColorParser.TryParseBackgroundColor(styles, out Color bColor))
            {
                hasColor = true;
                cb.normalColor = bColor;
                cb.highlightedColor = bColor;
                cb.pressedColor = bColor;
                cb.disabledColor = bColor;
                cb.selectedColor = bColor;
            }
            else if (selectable.targetGraphic is Graphic mg)
            {
                cb.normalColor = mg.color;
                cb.highlightedColor = mg.color;
                cb.pressedColor = mg.color;
                cb.disabledColor = mg.color;
                cb.selectedColor = mg.color;
            }

            var setterMap = new Dictionary<string, Action<Color>>
            {
                { "hover", (co) => cb.highlightedColor = co },
                { "active", (co) => cb.pressedColor = co },
                { "enabled", (co) => cb.normalColor = co },
                { "disabled", (co) => cb.disabledColor = co },
                { "selected", (co) => cb.selectedColor = co },
                { "focus", (co) => cb.selectedColor = co },
                { "checked", (co) => cb.selectedColor = co }
            };

            foreach (var kv in setterMap)
            {
                string pseudoKey = kv.Key;
                var setter = kv.Value;
                if (pseudoClassStyles.ContainsKey(pseudoKey) && ColorParser.TryParseBackgroundColor(pseudoClassStyles[pseudoKey], out Color c))
                {
                    hasColor = true;
                    setter(c);
                }
            }

            if (hasColor)
            {
                selectable.colors = cb;
                if (selectable.targetGraphic is Graphic mg2)
                {
                    mg2.color = Color.white;
                }
            }

        }
    }
}

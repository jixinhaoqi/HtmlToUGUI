using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Xxhq.Htmltougui
{
    /// <summary>
    /// 文本样式处理器：将 CSS 文本样式映射到 UGUI Text / TextMeshProUGUI 组件
    /// </summary>
    public static class TextStyleHandler
    {
        public static readonly string[] MultiLineTextNodes = { "p", "pre", "textarea" };
        /// <summary>
        /// 应用文本样式到 MaskableGraphic 组件（Text 或 TextMeshProUGUI）
        /// </summary>
        /// <param name="node">当前节点</param>
        /// <param name="styles">样式字典</param>
        /// <param name="horAlignment">水平对齐方式</param>
        /// <param name="colAlignment">垂直对齐方式</param>
        /// <param name="notSetColor">是否不设置颜色</param>
        /// <param name="isTextOverflow">是否启用文本溢出</param>
        public static void Apply(HtmlNode node, MaskableGraphic text, Dictionary<string, string> styles,
            HorizontalAlignmentOptions horAlignment = HorizontalAlignmentOptions.Center,
            VerticalAlignmentOptions colAlignment = VerticalAlignmentOptions.Middle,
            bool notSetColor = false,
            bool isTextOverflow = true)
        {
            if (!text) return;

            // 颜色
            if (!notSetColor)
            {
                if (styles.TryGetValue("color", out string color))
                    text.color = ColorParser.Parse(color, Color.black);
                else
                    text.color = Color.black;
            }

            // TextMeshProUGUI 分支
            if (text is TMP_Text tmp)
            {
                ApplyTMPStyle(node, tmp, styles);
                // 字体大小（含溢出控制）
                if (styles.TryGetValue("font-size", out string textSize))
                {
                    if (!MultiLineTextNodes.Contains(node.Name.ToLower()) && isTextOverflow)
#if UNITY_6000_0_OR_NEWER
                        tmp.textWrappingMode = TextWrappingModes.NoWrap;
#else
                        tmp.enableWordWrapping = false;
#endif
                    float v = UnitParser.Parse(textSize);
                    if (v != 0) tmp.fontSize = v;
                }

                // 字间距
                if (styles.TryGetValue("letter-spacing", out string letterSpacing))
                    tmp.characterSpacing = UnitParser.Parse(letterSpacing) * 10;
            }
            // Legacy Text 分支
            else if (text is Text txt)
            {
                ApplyLegacyTextStyle(node, txt, styles);

                if (styles.TryGetValue("font-size", out string textSize))
                {
                    if (!MultiLineTextNodes.Contains(node.Name.ToLower()) && isTextOverflow)
                        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
                    float v = UnitParser.Parse(textSize);
                    if (v != 0) txt.fontSize = (int)v;
                }
            }

            TextAlign(text, styles, horAlignment, colAlignment);
        }

        /// <summary>
        /// 应用 TextMeshProUGUI 特有的文本样式（粗体、斜体、下划线/删除线）
        /// </summary>
        /// <param name="node">当前节点</param>
        /// <param name="tmp">TextMeshProUGUI 组件</param>
        /// <param name="styles">样式字典</param>
        public static void ApplyTMPStyle(HtmlNode node, TMP_Text tmp, Dictionary<string, string> styles)
        {
            // 粗体
            if (styles.TryGetValue("font-weight", out string fontWeight))
            {
                if (!int.TryParse(fontWeight, out int fw) || fw >= 550)
                    tmp.fontStyle = FontStyles.Bold;
            }
            // 斜体
            if (styles.TryGetValue("font-style", out string fontStyle) && fontStyle == "italic")
                tmp.fontStyle |= FontStyles.Italic;

            // 下划线/删除线
            if (node.Name.ToLower() == "a")
            {
                if (!styles.TryGetValue("text-decoration", out string textDecoration) || textDecoration == "underline")
                    tmp.fontStyle |= FontStyles.Underline;
                if (!string.IsNullOrEmpty(textDecoration) && textDecoration == "line-through")
                    tmp.fontStyle |= FontStyles.Strikethrough;
            }
            else if (styles.TryGetValue("text-decoration", out string td))
            {
                if (td == "underline") tmp.fontStyle |= FontStyles.Underline;
                else if (td == "line-through") tmp.fontStyle |= FontStyles.Strikethrough;
            }

            // 文本溢出模式
            if (styles.TryGetValue("text-overflow", out string textOverflow))
            {
                if (textOverflow == "ellipsis") tmp.overflowMode = TextOverflowModes.Ellipsis;
                else if (textOverflow == "truncate") tmp.overflowMode = TextOverflowModes.Truncate;
            }
        }
        /// <summary>
        /// 应用 Legacy Text 特有的文本样式（粗体、斜体、文本溢出模式）
        /// </summary>
        /// <param name="node">当前节点</param>
        /// <param name="txt">Legacy Text 组件</param>
        /// <param name="styles">样式字典</param>
        public static void ApplyLegacyTextStyle(HtmlNode node, Text txt, Dictionary<string, string> styles)
        {
            if (styles.TryGetValue("font-weight", out string fontWeight))
            {
                if (!int.TryParse(fontWeight, out int fw) || fw >= 550)
                    txt.fontStyle = FontStyle.Bold;
            }
            if (styles.TryGetValue("font-style", out string fontStyle) && fontStyle == "italic")
                txt.fontStyle |= FontStyle.Italic;

            if (styles.TryGetValue("text-overflow", out string textOverflow))
            {
                if (textOverflow == "ellipsis")
                { txt.horizontalOverflow = HorizontalWrapMode.Wrap; txt.verticalOverflow = VerticalWrapMode.Truncate; }
                else if (textOverflow == "truncate")
                { txt.horizontalOverflow = HorizontalWrapMode.Overflow; txt.verticalOverflow = VerticalWrapMode.Truncate; }
            }
        }

        /// <summary>
        /// 设置文本对齐方式
        /// </summary>
        /// <param name="text">文本组件</param>
        /// <param name="styles">样式字典</param>
        /// <param name="horAlignment">水平对齐方式</param>
        /// <param name="colAlignment">垂直对齐方式</param>
        public static void TextAlign(MaskableGraphic text, Dictionary<string, string> styles,
            HorizontalAlignmentOptions horAlignment = HorizontalAlignmentOptions.Center,
            VerticalAlignmentOptions colAlignment = VerticalAlignmentOptions.Middle)
        {
            styles.TryGetValue("text-align", out string textAlign);
            if (textAlign == null) textAlign = "";
            styles.TryGetValue("display", out string displayFlex);
            string justifyContent = displayFlex == "flex" && styles.TryGetValue("justify-content", out string jc) ? jc : "";

            int height = 0;
            if (styles.TryGetValue("height", out string heightStr) && !heightStr.Contains("%"))
                height = (int)UnitParser.Parse(heightStr);

            VerticalAlignmentOptions verticalAlignment = GetVerticalAlign(styles, height, colAlignment);

            HorizontalAlignmentOptions horizontalAlignment = horAlignment;
            switch (textAlign)
            {
                case "left":
                    horizontalAlignment = HorizontalAlignmentOptions.Left;
                    break;
                case "center":
                    horizontalAlignment = HorizontalAlignmentOptions.Center;
                    break;
                case "right":
                    horizontalAlignment = HorizontalAlignmentOptions.Right;
                    break;
            }

            // flex justify-content 映射
            if (justifyContent == "flex-start" || justifyContent == "start" || justifyContent == "left" || justifyContent == "stretch")
                horizontalAlignment = HorizontalAlignmentOptions.Left;
            else if (justifyContent == "center")
                horizontalAlignment = HorizontalAlignmentOptions.Center;
            else if (justifyContent == "flex-end" || justifyContent == "end" || justifyContent == "right")
                horizontalAlignment = HorizontalAlignmentOptions.Right;

            if (text is TMP_Text tmp)
            { tmp.horizontalAlignment = horizontalAlignment; tmp.verticalAlignment = verticalAlignment; }
            else if (text is Text txt)
                txt.alignment = ToTextAnchor(horizontalAlignment, verticalAlignment);
        }
        /// <summary>
        /// 获取垂直对齐方式
        /// </summary>
        /// <param name="styles">样式字典</param>
        /// <param name="height">文本高度</param>
        /// <param name="defaultAlign">默认对齐方式</param>
        /// <returns>垂直对齐方式</returns>
        public static VerticalAlignmentOptions GetVerticalAlign(Dictionary<string, string> styles, int height,
            VerticalAlignmentOptions defaultAlign)
        {
            if (styles.TryGetValue("line-height", out string lineHeight))
            {
                float lineH = UnitParser.Parse(lineHeight);
                if (height != 0 && lineH < height) return VerticalAlignmentOptions.Top;
                if (height != 0 && lineH > height) return VerticalAlignmentOptions.Bottom;
                return defaultAlign;
            }

            if (styles.TryGetValue("display", out string display))
            {
                if (display == "flex" && styles.TryGetValue("align-items", out string alignItems))
                {
                    if (alignItems == "flex-start" || alignItems == "stretch" || alignItems == "baseline" || alignItems == "initial")
                        return VerticalAlignmentOptions.Top;
                    if (alignItems == "flex-end") return VerticalAlignmentOptions.Bottom;
                    if (alignItems == "center") return VerticalAlignmentOptions.Middle;
                }
                else if (display == "table-cell" && styles.TryGetValue("vertical-align", out string va))
                {
                    if (va == "top") return VerticalAlignmentOptions.Top;
                    if (va == "bottom") return VerticalAlignmentOptions.Bottom;
                    if (va == "middle") return VerticalAlignmentOptions.Middle;
                }
            }

            return defaultAlign;
        }
        /// <summary>
        /// 将水平对齐方式和垂直对齐方式转换为TextAnchor枚举值。
        /// </summary>
        /// <param name="h">水平对齐方式</param>
        /// <param name="v">垂直对齐方式</param>
        /// <returns>TextAnchor枚举值</returns>
        public static TextAnchor ToTextAnchor(HorizontalAlignmentOptions h, VerticalAlignmentOptions v)
        {
            if (h == HorizontalAlignmentOptions.Left && v == VerticalAlignmentOptions.Top) return TextAnchor.UpperLeft;
            if (h == HorizontalAlignmentOptions.Left && v == VerticalAlignmentOptions.Middle) return TextAnchor.MiddleLeft;
            if (h == HorizontalAlignmentOptions.Left && v == VerticalAlignmentOptions.Bottom) return TextAnchor.LowerLeft;
            if (h == HorizontalAlignmentOptions.Center && v == VerticalAlignmentOptions.Top) return TextAnchor.UpperCenter;
            if (h == HorizontalAlignmentOptions.Center && v == VerticalAlignmentOptions.Middle) return TextAnchor.MiddleCenter;
            if (h == HorizontalAlignmentOptions.Center && v == VerticalAlignmentOptions.Bottom) return TextAnchor.LowerCenter;
            if (h == HorizontalAlignmentOptions.Right && v == VerticalAlignmentOptions.Top) return TextAnchor.UpperRight;
            if (h == HorizontalAlignmentOptions.Right && v == VerticalAlignmentOptions.Middle) return TextAnchor.MiddleRight;
            return TextAnchor.LowerRight;
        }
    }
}

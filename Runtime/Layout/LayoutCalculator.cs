using HtmlAgilityPack;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Xxhq.Htmltougui
{

    /// <summary>
    /// 布局计算器：负责将 HTML/CSS 布局属性转换为 UGUI RectTransform 的锚点、尺寸和位置。
    /// 支持三种布局模式：智能、全拉伸、居中。
    /// </summary>
    public abstract class LayoutCalculator
    {
        protected static readonly Regex s_TranslateRegex = new Regex(@"translate\(([^,]+),\s*([^)]+)\)", RegexOptions.Compiled);
        protected static readonly Regex s_Translate1DRegex = new Regex(@"translate\(([^)]+)\)", RegexOptions.Compiled);
        protected static readonly Regex s_TranslateXRegex = new Regex(@"translateX\(([^)]+)\)", RegexOptions.Compiled);
        protected static readonly Regex s_TranslateYRegex = new Regex(@"translateY\(([^)]+)\)", RegexOptions.Compiled);
        public virtual void OnEnable()
        {

        }
        public virtual void OnGUI()
        {

        }
        public virtual void OnDisable()
        {

        }
        /// <summary>
        /// 计算并设置 RectTransform 的锚点、轴心、位置和偏移量
        /// </summary>
        /// <param name="rt">要设置的 RectTransform</param>
        /// <param name="styles">包含 CSS 属性的字典</param>
        /// <param name="node">对应的 HTML 节点</param>
        public virtual void SetAnchorAndSize(RectTransform rt, Dictionary<string, string> styles, HtmlNode node)
        {
            var layoutData = GetLayoutData(rt, styles, node);
            if (layoutData.isAbsoluteLayout)
            {
                CalculateAbsoluteLayout(rt, layoutData.x, layoutData.y, layoutData.width, layoutData.height, layoutData.parentWidth, layoutData.parentHeight);
            }
            else
            {
                CalculateRelativeLayout(rt, styles, layoutData.x, layoutData.y, layoutData.width, layoutData.height, layoutData.parentWidth, layoutData.parentHeight);
            }
        }

        public static (bool isAbsoluteLayout, string x, string y, string width, string height, string parentWidth, string parentHeight) GetLayoutData(RectTransform rt, Dictionary<string, string> styles, HtmlNode node)
        {
            bool isAbsoluteLayout= !string.IsNullOrWhiteSpace(node?.GetAttributeValue("data-u-left", ""));
            string x = "", y = "", width = "", height = "", parentWidth = "", parentHeight = "";
            RectTransform parentRt = rt.parent as RectTransform;
            if (isAbsoluteLayout)
            {
                parentWidth = parentRt? parentRt.rect.width.ToString(): node.ParentNode.GetAttributeValue("data-u-width", "0");
                parentHeight = parentRt? parentRt.rect.height.ToString(): node.ParentNode.GetAttributeValue("data-u-height", "0");
                width = node.GetAttributeValue("data-u-width", "0");
                height = node.GetAttributeValue("data-u-height", "0");
                x = (UnitParser.Parse(node.GetAttributeValue("data-u-left", "0"))
                    - UnitParser.Parse(node.ParentNode.GetAttributeValue("data-u-left", "0"))).ToString();
                y = (UnitParser.Parse(node.GetAttributeValue("data-u-top", "0"))
                    - UnitParser.Parse(node.ParentNode.GetAttributeValue("data-u-top", "0"))).ToString();
            }
            else
            {
                parentWidth = parentRt ? parentRt.rect.width.ToString() : "100%";
                parentHeight = parentRt ? parentRt.rect.height.ToString() : "100%";
                width = styles.TryGetValue("width", out string w) ? w : parentWidth;
                height = styles.TryGetValue("height", out string h) ? h : parentHeight;
                if (width.Contains("calc"))
                    width = UnitParser.Parse(width, UnitParser.Parse(parentWidth)).ToString();
                if (height.Contains("calc"))
                    height = UnitParser.Parse(height, UnitParser.Parse(parentHeight)).ToString();
                x = styles.TryGetValue("left", out string l) ? l : "";
                y = styles.TryGetValue("top", out string t) ? t : "";
            }
            return (isAbsoluteLayout, x, y, width, height, parentWidth, parentHeight);
        }

        #region 绝对定位布局（基于 data-u-* 属性）

        protected virtual void CalculateAbsoluteLayout(RectTransform rt, string x, string y, string width, string height, string parentWidth, string parentHeight)
        {
            Vector2 anchorMin = new Vector2(0, 1);
            Vector2 anchorMax = new Vector2(0, 1);
            Vector2 pivot = Vector2.one * 0.5f;
            Vector2 anchoredPosition = Vector2.zero;
            float? offsetMinX = null, offsetMinY = null, offsetMaxX = null, offsetY = null;

            float xValue = UnitParser.Parse(x);
            float yValue = UnitParser.Parse(y);
            float widthValue = UnitParser.Parse(width);
            float heightValue = UnitParser.Parse(height);
            float parentWidthValue = UnitParser.Parse(parentWidth);
            float parentHeightValue = UnitParser.Parse(parentHeight);
            anchoredPosition.x = xValue + widthValue / 2f;
            anchoredPosition.y = -yValue - heightValue / 2f;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, widthValue);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, heightValue);

            ApplyAbsoluteLayoutStrategy(xValue, yValue, widthValue, heightValue, parentWidthValue, parentHeightValue,
                ref anchorMin, ref anchorMax, ref anchoredPosition,
                out offsetMinX, out offsetMinY, out offsetMaxX, out offsetY);

            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPosition;
            rt.offsetMin = new Vector2(offsetMinX ?? rt.offsetMin.x, offsetMinY ?? rt.offsetMin.y);
            rt.offsetMax = new Vector2(offsetMaxX ?? rt.offsetMax.x, offsetY ?? rt.offsetMax.y);
        }

        #endregion

        #region 相对定位布局（基于 CSS left/top/width/height）

        protected virtual void CalculateRelativeLayout(RectTransform rt, Dictionary<string, string> styles, string x, string y, string width, string height, string parentWidth, string parentHeight)
        {
            Vector2 anchorMin = new Vector2(0, 1);
            Vector2 anchorMax = new Vector2(0, 1);
            Vector2 pivot = new Vector2(0, 1);
            Vector2 anchoredPosition = Vector2.zero;
            float? offsetMinX = null, offsetMinY = null, offsetMaxX = null, offsetMaxY = null;

            RectTransform parentRt = rt.parent as RectTransform;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, UnitParser.Parse(width));
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, UnitParser.Parse(height));
            // transform: translate
            if (styles.TryGetValue("transform", out string transform) && transform.StartsWith("translate"))
            {
                var match = s_TranslateRegex.Match(transform);
                string xStr = "", yStr = "";
                if (match.Success)
                { xStr = match.Groups[1].Value.Trim(); yStr = match.Groups[2].Value.Trim(); }
                else
                {
                    match = s_Translate1DRegex.Match(transform);
                    if (match.Success) { xStr = match.Groups[1].Value.Trim(); yStr = "0"; }
                    else
                    {
                        match = s_TranslateXRegex.Match(transform);
                        if (match.Success) { xStr = match.Groups[1].Value.Trim(); yStr = "0"; }
                        else
                        {
                            match = s_TranslateYRegex.Match(transform);
                            if (match.Success) { xStr = "0"; yStr = match.Groups[1].Value.Trim(); }
                        }
                    }
                }
                if (match.Success)
                {
                    if (xStr.Contains("%")) pivot.x = -UnitParser.Parse(xStr) / 100f;
                    else pivot.x = -UnitParser.Parse(xStr) / rt.rect.width;
                    if (yStr.Contains("%")) pivot.y = 1 + UnitParser.Parse(yStr) / 100f;
                    else pivot.y = 1 + UnitParser.Parse(yStr) / rt.rect.height;
                }
            }

            // left
            if (!string.IsNullOrEmpty(x))
            {
                if (x.Contains("%")) anchorMin.x = anchorMax.x = UnitParser.Parse(x) / 100f;
                else anchoredPosition.x = UnitParser.Parse(x);
            }
            else
            {
                if (width.Contains("%"))
                    offsetMinX= offsetMaxX = 0;
            }

            // width (%)
            if (width.Contains("%"))
            {
                var tempX = anchorMin.x + UnitParser.Parse(width) / 100f;
                anchorMax.x = tempX > 1 ? anchorMin.x : tempX;
                if (tempX <= 1)
                { 
                    offsetMinX= offsetMaxX = 0; 
                }
            }

            // top
            if (!string.IsNullOrEmpty(y))
            {
                if (y.Contains("%")) anchorMin.y = anchorMax.y = 1 - UnitParser.Parse(y) / 100f;
                else anchoredPosition.y = -UnitParser.Parse(y);
            }
            else
            {
                if (height.Contains("%"))
                    offsetMinY= offsetMaxY = 0;
            }

            // height (%)
            if (height.Contains("%"))
            {
                //anchorMin.y =  anchorMin.y - UnitParser.Parse(height) / 100f;

                float anchorMaxY = anchorMax.y;
                anchorMax.y = 1 + anchorMin.y - UnitParser.Parse(height) / 100f;
                anchorMin.y = 1 - anchorMaxY;
                offsetMinY= offsetMaxY = 0;
            }

            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPosition;
            rt.offsetMin = new Vector2(offsetMinX ?? rt.offsetMin.x, offsetMinY ?? rt.offsetMin.y);
            rt.offsetMax = new Vector2(offsetMaxX ?? rt.offsetMax.x, offsetMaxY ?? rt.offsetMax.y);
        }

        #endregion

        #region 布局策略

        public abstract void ApplyAbsoluteLayoutStrategy(
            float relativeX, float relativeY, float selfWidth, float selfHeight,
            float parentWidth, float parentHeight,
            ref Vector2 anchorMin, ref Vector2 anchorMax, ref Vector2 anchoredPosition,
            out float? offMinX, out float? offMinY, out float? offMaxX, out float? offMaxY);

        #endregion

    }
}

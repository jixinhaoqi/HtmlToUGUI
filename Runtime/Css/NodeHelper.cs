using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Xxhq.Htmltougui
{
    /// <summary>
    /// 节点辅助工具
    /// </summary>
    public static class NodeHelper
    {
        /// <summary>
        /// 获取节点用于命名的 ID（优先 data-u-name > id > 第一个 class > 标签名）
        /// </summary>
        /// <param name="node">HTML 节点</param>
        /// <returns>节点的 ID</returns>
        public static string GetNodeId(HtmlNode node)
        {
            if (node.GetAttributeValue("data-u-name", "") != "")
                return node.GetAttributeValue("data-u-name", "");
            if (!string.IsNullOrEmpty(node.Id))
                return node.Id;
            if (node.GetClasses()?.Count() > 0)
            {
                string name = node.GetClasses().First();
                Debug.LogWarning($"HTML 标签 {node.Name} 没有 ID，已自动分配临时 Class: {name}");
                return name;
            }
            Debug.LogWarning($"HTML 标签 {node.Name} 没有 ID，已自动分配临时 Name: {node.Name}");
            return node.Name;
        }

        /// <summary>
        /// 提取节点的纯文本内容（过滤嵌套标签，保留 br 换行和占位宽度）
        /// </summary>
        /// <param name="node">HTML 节点</param>
        /// <param name="styles">样式</param>
        /// <param name="isLegacyText">是否使用旧版文本适配</param>
        /// <returns>纯文本内容</returns>
        public static string GetNodeText(HtmlNode node, Dictionary<string, string> styles, bool isLegacyText = false)
        {
            string v = "";
            foreach (var c in node.ChildNodes)
            {
                if (c.NodeType == HtmlNodeType.Text)
                    v += System.Net.WebUtility.HtmlDecode(Regex.Replace(c.InnerText, @"\s+", ""));
                else if (c.Name.ToLower() == "br")
                    v += "\n";
                else if (c.GetAttributeValue("data-u-width", null) is string width && !string.IsNullOrEmpty(width) && width != "0")
                {
                    float w = UnitParser.Parse(width);
                    var text = Regex.Replace(c.InnerText, @"\s+", "");
                    if (!string.IsNullOrEmpty(text))
                        v += "<color=#00000000>" + text + "</color>";
                    else
                        v += "<size=6>" + new string(' ', (int)w) + "</size>";
                }
            }
            return v;
        }

        /// <summary>
        /// 如果节点有文本，则创建文本对象（填充父容器）
        /// </summary>
        /// <param name="node">HTML 节点</param>
        /// <param name="styles">样式字典</param>
        /// <param name="parent">父容器</param>
        /// <param name="createTextFunc">创建文本对象的函数</param>
        /// <param name="customText">自定义文本</param>
        public static void CreateInterTextIfHas(HtmlNode node, Dictionary<string, string> styles, Transform parent,
            Func<HtmlNode, Dictionary<string, string>, Transform, bool, string, GameObject> createTextFunc, string customText = "")
        {
            bool hasText = !string.IsNullOrEmpty(customText)|| node.ChildNodes?.FirstOrDefault(i => i?.NodeType == HtmlNodeType.Text && !string.IsNullOrWhiteSpace(i?.InnerText)) != null;
            if (hasText)
            {
                var text = createTextFunc(node, styles, parent, true, customText);
                RectTransformHelper.FullyFillParent(text.GetComponent<RectTransform>());
                if (PaddingParser.TryParse(styles, out RectOffset rectOffset))
                {
                    text.GetComponent<RectTransform>().offsetMin = new Vector2(rectOffset.left, rectOffset.bottom);
                    text.GetComponent<RectTransform>().offsetMax = new Vector2(-rectOffset.right, -rectOffset.top);
                }
            }
        }
    }
}

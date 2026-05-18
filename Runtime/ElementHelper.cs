using System.Collections.Generic;

namespace Xxhq.Htmltougui
{
    /// <summary>
    /// UI 元素辅助方法集合
    /// </summary>
    public static class ElementHelper
    {
        /// <summary> 判断是否为滚动容器 </summary>
        /// <param name="styles">样式</param>
        /// <returns>是否为滚动容器</returns>
        public static bool IsScrollContainer(Dictionary<string, string> styles)
        {
            styles.TryGetValue("overflow", out string overflow);
            if (overflow != null && overflow.Contains("scroll")) return true;
            if (overflow != null && overflow.Contains("auto")) return true;
            styles.TryGetValue("overflow-y", out string overflowY);
            if (overflowY != null&& overflowY != "hidden") return true;
            styles.TryGetValue("overflow-x", out string overflowX);
            if (overflowX != null && overflowX != "hidden") return true;
            return false;
        }

        /// <summary> 判断节点是否应被排除（注释、纯文本、option、script、style）</summary>
        /// <param name="node">节点</param>
        /// <returns>是否应被排除</returns>
        public static bool IsExcludeNode(HtmlAgilityPack.HtmlNode node)
        {
            return node.NodeType == HtmlAgilityPack.HtmlNodeType.Comment
                || node.NodeType == HtmlAgilityPack.HtmlNodeType.Text
                || node.Name?.ToLower() == "option"
                || node.Name?.ToLower() == "script"
                || node.Name?.ToLower() == "style"
                || node.Name?.ToLower() == "br";
        }

        /// <summary> 判断节点是否被隐藏（display:none）</summary>
        /// <param name="styles">样式</param>
        /// <returns>是否被隐藏</returns>
        public static bool IsHideNode(Dictionary<string, string> styles)
        {
            return styles.TryGetValue("display", out string display) && display == "none";
        }
    }
}

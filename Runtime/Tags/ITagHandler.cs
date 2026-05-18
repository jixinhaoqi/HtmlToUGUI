using System.Collections.Generic;
using HtmlAgilityPack;
using UnityEngine;

namespace Xxhq.Htmltougui
{
    /// <summary>
    /// HTML 标签处理器接口。实现此接口并通过反射自动发现注册。
    /// </summary>
    public interface ITagHandler
    {
        /// <summary>
        /// 此处理器支持的 HTML 标签名集合（小写）。
        /// 同一个处理器可以处理多个标签，例如 h1~h6。
        /// </summary>
        IReadOnlyList<string> SupportedTags { get; }

    /// <summary>
    /// 根据 HTML 节点创建对应的 UGUI GameObject。
    /// </summary>
    /// <param name="node">当前 HTML 节点</param>
    /// <param name="styles">已解析的 CSS 样式字典（包含继承和级联结果）</param>
    /// <param name="parent">父级 Transform</param>
    /// <param name="factory">UI 元素工厂，用于创建通用 UI 组件</param>
    /// <returns>创建的 GameObject，或 null 表示跳过此节点</returns>
    GameObject CreateElement(HtmlNode node, Dictionary<string, string> styles,
        Transform parent, IElementFactory factory);
    }
}

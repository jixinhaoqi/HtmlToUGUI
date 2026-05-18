using System.Collections.Generic;
using HtmlAgilityPack;
using UnityEngine;

namespace Xxhq.Htmltougui
{
    /// <summary>
    /// UI 元素工厂接口
    /// </summary>
    public interface IElementFactory
    {
        GameObject CreateContainer(HtmlNode node, Dictionary<string, string> styles, Transform parent);
        GameObject CreateText(HtmlNode node, Dictionary<string, string> styles, Transform parent, bool noPadding = false, string customText = "");
        GameObject CreateButton(HtmlNode node, Dictionary<string, string> styles, Transform parent);
        GameObject CreateInputField(HtmlNode node, Dictionary<string, string> styles, Transform parent);
        GameObject CreateToggle(HtmlNode node, Dictionary<string, string> styles, Transform parent);
        GameObject CreateSlider(HtmlNode node, Dictionary<string, string> styles, Transform parent);
        GameObject CreateDropdown(HtmlNode node, Dictionary<string, string> styles, Transform parent);
        GameObject CreateImage(HtmlNode node, Dictionary<string, string> styles, Transform parent);
        GameObject CreateScrollView(HtmlNode node, Dictionary<string, string> styles, Transform parent);
    }
}

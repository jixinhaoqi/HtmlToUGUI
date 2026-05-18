using System.Collections.Generic;
using HtmlAgilityPack;
using UnityEngine;

namespace Xxhq.Htmltougui
{
    /// <summary> body, div, section, a, p, pre, label, span — 容器或滚动容器 </summary>
    public class ContainerTagHandler : ITagHandler
    {
        public virtual IReadOnlyList<string> SupportedTags { get; } =
            new[] { "body", "div", "section", "a", "p", "pre", "label", "span" };

        public virtual GameObject CreateElement(HtmlNode node, Dictionary<string, string> styles,
            Transform parent, IElementFactory factory)
        {
            return ElementHelper.IsScrollContainer(styles)
                ? factory.CreateScrollView(node, styles, parent)
                : factory.CreateContainer(node, styles, parent);
        }
    }

    /// <summary> button, img, select — 交互组件 </summary>
    public class InteractiveTagHandler : ITagHandler
    {
        public virtual IReadOnlyList<string> SupportedTags { get; } =
            new[] { "button", "img", "select" };

        public virtual GameObject CreateElement(HtmlNode node, Dictionary<string, string> styles,
            Transform parent, IElementFactory factory)
        {
            switch (node.Name.ToLower())
            {
                case "button":
                    return factory.CreateButton(node, styles, parent);
                case "img":
                    return factory.CreateImage(node, styles, parent);
                case "select":
                    return factory.CreateDropdown(node, styles, parent);
                default:
                    return factory.CreateContainer(node, styles, parent);
            }
        }
    }

    /// <summary> input, textarea — 根据 type 属性分派 </summary>
    public class InputTagHandler : ITagHandler
    {
        public virtual IReadOnlyList<string> SupportedTags { get; } =
            new[] { "input", "textarea" };

        public virtual GameObject CreateElement(HtmlNode node, Dictionary<string, string> styles,
            Transform parent, IElementFactory factory)
        {
            string type = node.GetAttributeValue("type", "text");
            switch (type)
            {
                case "radio":
                case "checkbox":
                    return factory.CreateToggle(node, styles, parent);
                case "range":
                    return factory.CreateSlider(node, styles, parent);
                case "file":
                case "submit":
                    return factory.CreateButton(node, styles, parent);
                default:
                    return factory.CreateInputField(node, styles, parent);
            }
        }
    }

    /// <summary> h1 ~ h6 — 标题 </summary>
    public class HeadingTagHandler : ITagHandler
    {
        protected static readonly string[] FontSizes = { "32px", "24px", "18.72px", "16px", "13.28px", "12px" };

        public virtual IReadOnlyList<string> SupportedTags { get; } =
            new[] { "h1", "h2", "h3", "h4", "h5", "h6" };

        public virtual GameObject CreateElement(HtmlNode node, Dictionary<string, string> styles,
            Transform parent, IElementFactory factory)
        {
            int level = Mathf.Clamp(node.Name[1] - '1', 0, FontSizes.Length - 1);
            if (!styles.ContainsKey("font-size")) styles["font-size"] = FontSizes[level];
            if (!styles.ContainsKey("font-weight")) styles["font-weight"] = "bold";
            return factory.CreateContainer(node, styles, parent);
        }
    }

    /// <summary> b, strong, i, em, u, ins, s, del, small, th — 带内联样式的容器 </summary>
    public class InlineStyleTagHandler : ITagHandler
    {
        public virtual IReadOnlyList<string> SupportedTags { get; } =
            new[] { "b", "strong", "i", "em", "u", "ins", "s", "del", "small", "th" };

        public virtual GameObject CreateElement(HtmlNode node, Dictionary<string, string> styles,
            Transform parent, IElementFactory factory)
        {
            string tag = node.Name.ToLower();

            switch (tag)
            {
                case "th":
                    if (!styles.ContainsKey("font-weight")) styles["font-weight"] = "bold";
                    if (!styles.ContainsKey("text-align")) styles["text-align"] = "center";
                    break;
                case "b":
                case "strong":
                    if (!styles.ContainsKey("font-weight")) styles["font-weight"] = "bold";
                    break;
                case "i":
                case "em":
                    if (!styles.ContainsKey("font-style")) styles["font-style"] = "italic";
                    break;
                case "u":
                case "ins":
                    if (!styles.ContainsKey("text-decoration")) styles["text-decoration"] = "underline";
                    break;
                case "s":
                case "del":
                    if (!styles.ContainsKey("text-decoration")) styles["text-decoration"] = "line-through";
                    break;
                case "small":
                    if (!styles.ContainsKey("font-size")) styles["font-size"] = "0.875rem";
                    break;
            }

            return factory.CreateContainer(node, styles, parent);
        }
    }
}

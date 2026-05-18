using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Xxhq.Htmltougui;

namespace Xxhq.Htmltougui.Editor
{
    /// <summary>
    /// UGUI 元素工厂：负责根据 HTML 节点创建对应的 Unity UGUI 组件
    /// </summary>
    public class UguiElementFactory : IElementFactory
    {
        private readonly UiPrefabSettings _prefabs;
        private readonly bool _isLegacyText;
        private readonly bool _isTextOverflow;
        private readonly string _htmlFilePath;
        private readonly Dictionary<string, Dictionary<string, string>> _pseudoClassStyles;

        public UguiElementFactory(UiPrefabSettings prefabs, bool isLegacyText,
            string htmlFilePath, Dictionary<string, Dictionary<string, string>> pseudoClassStyles,
            bool isTextOverflow = true)
        {
            _prefabs = prefabs;
            _isLegacyText = isLegacyText;
            _isTextOverflow = isTextOverflow;
            _htmlFilePath = htmlFilePath;
            _pseudoClassStyles = pseudoClassStyles;
            // 注入 Editor 端的图片加载实现到 Runtime 的 VisualStyleApplier
            if (VisualStyleApplier.LoadImageFunc == null)
                VisualStyleApplier.LoadImageFunc = ResourceLoader.LoadImage;
        }

        #region 通用创建方法

        /// <summary>
        /// 从 Prefab 实例化或创建空 GameObject
        /// </summary>
        /// <param name="prefab">预设体</param>
        /// <param name="parent">父级</param>
        /// <param name="name">名称</param>
        /// <returns>创建的 GameObject</returns>
        public static GameObject InstantiateFromPrefab(GameObject prefab, Transform parent, string name)
        {
            GameObject go;
            if (prefab != null)
            {
                go = GameObject.Instantiate(prefab, parent);
                go.name = name;
                return go;
            }
            go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        /// <summary>
        /// 通过菜单创建 UI 元素并设置父级
        /// </summary>
        /// <param name="menuItem">菜单项</param>
        /// <param name="parent">父级</param>
        /// <param name="name">名称</param>
        /// <returns>创建的 GameObject</returns>
        public static GameObject CreateViaMenu(string menuItem, Transform parent, string name)
        {
            EditorApplication.ExecuteMenuItem(menuItem);
            GameObject go = Selection.activeGameObject;
            go.name = name;
            go.transform.SetParent(parent, false);
            return go;
        }

        #endregion

        #region 各元素类型创建方法

        /// <summary> 创建容器（div/p/span/a 等） </summary>
        /// <param name="node">HTML 节点</param>
        /// <param name="styles">样式</param>
        /// <param name="parent">父级</param>
        /// <returns>创建的 GameObject</returns>
        public virtual GameObject CreateContainer(HtmlNode node, Dictionary<string, string> styles, Transform parent)
        {
            bool haveColor = ColorParser.TryParseBackgroundColor(styles, out Color color);
            bool haveUrl = ColorParser.TryParseBackgroundUrl(styles, out string url);
            bool haveAllBorder = styles.ContainsKey("border");
            bool haveBg = haveColor || haveUrl || styles.Keys.Any(i => i.StartsWith("border"));
            bool hasText = node.ChildNodes?.FirstOrDefault(i => i?.NodeType == HtmlNodeType.Text && !string.IsNullOrWhiteSpace(i?.InnerText)) != null;

            if (hasText && !haveBg)
                return CreateText(node, styles, parent);

            GameObject go = InstantiateFromPrefab(_prefabs?.containerPrefab, parent, NodeHelper.GetNodeId(node));

            if (haveBg)
            {
                var img = go.GetComponent<Image>()?? go.AddComponent<Image>();
                img.color = haveColor ? color : haveAllBorder || haveUrl ? Color.white : Color.clear;
                VisualStyleApplier.ApplyCommonStyles(go, styles, _pseudoClassStyles, _htmlFilePath);
                NodeHelper.CreateInterTextIfHas(node, styles, go.transform, CreateText);
            }

            return go;
        }

        public string GetMenuPath(bool isLegacyText,string name)
        {
#if UNITY_2021_3_OR_NEWER
        return isLegacyText ? $"GameObject/UI/Legacy/{name}" : $"GameObject/UI/{name} - TextMeshPro";
#else
        return isLegacyText ? $"GameObject/UI/{name}" : $"GameObject/UI/{name} - TextMeshPro";
#endif
        }

        /// <summary> 创建文本元素 </summary>
        /// <param name="node">HTML 节点</param>
        /// <param name="styles">样式</param>
        /// <param name="parent">父级</param>
        /// <param name="noPadding">是否忽略内边距</param>
        /// <param name="customText">自定义文本内容</param>
        /// <returns>创建的 GameObject</returns>
        public virtual GameObject CreateText(HtmlNode node, Dictionary<string, string> styles, Transform parent, bool noPadding = false, string customText = "")
        {
            if (string.IsNullOrWhiteSpace(node.InnerText) && string.IsNullOrWhiteSpace(customText)) return null;

            GameObject go;
            if (_prefabs?.textPrefab != null)
                go = InstantiateFromPrefab(_prefabs?.textPrefab, parent, GetTextObjectName(node));
            else
            {
                go = CreateViaMenu(GetMenuPath(_isLegacyText, "Text"),
                    parent, GetTextObjectName(node));
            }

            MaskableGraphic text = go.GetComponentInChildren<TextMeshProUGUI>()
                ?? go.GetComponentInChildren<Text>() as MaskableGraphic;

            if (text != null)
            {
                string v = string.IsNullOrWhiteSpace(customText) ? NodeHelper.GetNodeText(node, styles, _isLegacyText) : customText;
                if (text is TextMeshProUGUI tmp)
                { tmp.fontSize = 14; tmp.text = v; }
                else if (text is Text txt)
                    txt.text = v;

                TextStyleHandler.Apply(node, text, styles,
                    node.Name.ToLower() != "textarea"
                        ? node.Name.ToLower() == "img" ? HorizontalAlignmentOptions.Center : HorizontalAlignmentOptions.Left : HorizontalAlignmentOptions.Left,
                    node.Name.ToLower() != "textarea"
                        ? VerticalAlignmentOptions.Middle : VerticalAlignmentOptions.Top,
                    notSetColor: false,
                    isTextOverflow: _isTextOverflow);
            }

            // 超链接处理
            if (node.Name.ToLower() == "a")
            {
                string href = node.GetAttributeValue("href", "");
                // TODO: 链接点击事件
            }

            // Padding 修正
            if (!noPadding && PaddingParser.TryParse(styles, out RectOffset rectOffset))
            {
                string selfLeftString = node.GetAttributeValue("data-u-left", "");
                if (!string.IsNullOrWhiteSpace(selfLeftString))
                {
                    float selfLeft = UnitParser.Parse(selfLeftString);
                    float selfTop = UnitParser.Parse(node.GetAttributeValue("data-u-top", ""));
                    float selfWidth = UnitParser.Parse(node.GetAttributeValue("data-u-width", ""));
                    float selfHeight = UnitParser.Parse(node.GetAttributeValue("data-u-height", ""));
                    selfLeft += rectOffset.left;
                    selfTop += rectOffset.top;
                    selfWidth -= (rectOffset.left + rectOffset.right);
                    selfHeight -= (rectOffset.top + rectOffset.bottom);
                    node.SetAttributeValue("data-u-left", selfLeft.ToString());
                    node.SetAttributeValue("data-u-top", selfTop.ToString());
                    node.SetAttributeValue("data-u-width", selfWidth.ToString());
                    node.SetAttributeValue("data-u-height", selfHeight.ToString());
                }
            }

            VisualStyleApplier.ApplyPseudoClassStyles(go, _pseudoClassStyles);
            return go;
        }
        /// <summary>
        /// 获取文本对象名称，如果已经是结尾为 Txt 则直接返回，否则在末尾加上 Txt 作为后缀。
        /// </summary>
        /// <param name="node">HTML 节点对象</param>
        /// <returns>文本对象名称</returns>
        public static string GetTextObjectName(HtmlNode node)
        {
            string baseName = NodeHelper.GetNodeId(node);
            return baseName.EndsWith("Txt") ? baseName : UnitParser.AddUnitNumericSuffix(baseName, "Txt");
        }

        /// <summary> 创建按钮 </summary>
        /// <param name="node">HTML 节点</param>
        /// <param name="styles">样式</param>
        /// <param name="parent">父级</param>
        /// <returns>创建的 GameObject</returns>
        public virtual GameObject CreateButton(HtmlNode node, Dictionary<string, string> styles, Transform parent)
        {
            GameObject go;
            if (_prefabs?.buttonPrefab != null)
                go = InstantiateFromPrefab(_prefabs?.buttonPrefab, parent, NodeHelper.GetNodeId(node));
            else
            {
                go = CreateViaMenu(GetMenuPath(_isLegacyText, "Button"),
                    parent, NodeHelper.GetNodeId(node));
                go.GetComponent<Image>().sprite = null;
            }

            MaskableGraphic text = _isLegacyText? go.GetComponentInChildren<Text>() as MaskableGraphic : go.GetComponentInChildren<TextMeshProUGUI>() as MaskableGraphic;
            if (text != null)
            {
                string value = node.GetAttributeValue("type", "") == "submit"
                    ? node.GetAttributeValue("value", "") : "";
                if (text is TextMeshProUGUI tmp)
                {
                    tmp.fontSize = 14;
                    tmp.text = !string.IsNullOrWhiteSpace(value) ? value : NodeHelper.GetNodeText(node, styles, _isLegacyText);
                }
                else if (text is Text txt)
                    txt.text = !string.IsNullOrWhiteSpace(value) ? value : NodeHelper.GetNodeText(node, styles, _isLegacyText);

                TextStyleHandler.Apply(node, text, styles, HorizontalAlignmentOptions.Center, VerticalAlignmentOptions.Middle, isTextOverflow: _isTextOverflow);
            }

            VisualStyleApplier.ApplyCommonStyles(go, styles, _pseudoClassStyles, _htmlFilePath);
            return go;
        }

        /// <summary> 创建输入框 </summary>
        /// <param name="node">HTML 节点</param>
        /// <param name="styles">样式</param>
        /// <param name="parent">父级</param>
        /// <returns>创建的 GameObject</returns>
        public virtual GameObject CreateInputField(HtmlNode node, Dictionary<string, string> styles, Transform parent)
        {
            GameObject go;
            if (_prefabs?.inputPrefab != null)
                go = InstantiateFromPrefab(_prefabs?.inputPrefab, parent, NodeHelper.GetNodeId(node));
            else
            {
                go = CreateViaMenu(GetMenuPath(_isLegacyText, "Input Field"),
                    parent, NodeHelper.GetNodeId(node));
                go.GetComponent<Image>().sprite = null;
                if (_isLegacyText)
                {
                    if (go.GetComponentInChildren<InputField>()?.placeholder?.GetComponent<Text>() is var txt)
                    {
                        txt.fontStyle = FontStyle.Normal;
                    }
                }
                else
                {
                    if (go.GetComponentInChildren<TMP_InputField>()?.placeholder?.GetComponent<TextMeshProUGUI>() is var tmp)
                    {
                        tmp.fontStyle = FontStyles.Normal;
                    }
                }
            }

            ApplyInputFieldContent(go, node, styles);
            VisualStyleApplier.ApplyCommonStyles(go, styles, _pseudoClassStyles, _htmlFilePath);
            return go;
        }

        internal virtual void ApplyInputFieldContent(GameObject go, HtmlNode node, Dictionary<string, string> styles)
        {
            bool isTextarea = node.Name.ToLower() == "textarea";
            VerticalAlignmentOptions defaultVertAlign = isTextarea ? VerticalAlignmentOptions.Top : VerticalAlignmentOptions.Middle;
            string value = node.GetAttributeValue("value", "");
            if (_isLegacyText)
            {
                var inputField = go.GetComponent<InputField>();
                if (inputField == null) return;
                inputField.text = value;
                if (inputField.placeholder is Text pText)
                {
                    pText.text = node.GetAttributeValue("placeholder", "").Trim();
                    TextStyleHandler.Apply(node, pText, styles, HorizontalAlignmentOptions.Left, defaultVertAlign, isTextOverflow: _isTextOverflow);
                    pText.color = new Color(pText.color.r, pText.color.g, pText.color.b, 0.5f);
                }

                if (inputField.textComponent != null)
                    TextStyleHandler.Apply(node, inputField.textComponent, styles, HorizontalAlignmentOptions.Left, defaultVertAlign, isTextOverflow: _isTextOverflow);

                if (node.GetAttributeValue("type", "") == "password")
                    inputField.contentType = InputField.ContentType.Password;

                if (PaddingParser.TryParse(styles, out RectOffset rectOffset))
                {
                    if (inputField.placeholder != null)
                        inputField.placeholder.rectTransform.offsetMin = new Vector2(rectOffset.left, rectOffset.bottom);
                    if (inputField.textComponent != null)
                        inputField.textComponent.rectTransform.offsetMax = new Vector2(-rectOffset.right, -rectOffset.top);
                }

                inputField.lineType = isTextarea ? InputField.LineType.MultiLineNewline : InputField.LineType.SingleLine;
            }
            else
            {
                var inputField = go.GetComponent<TMP_InputField>();
                if (inputField == null) return;
                inputField.text = value;
                if (inputField.placeholder is TextMeshProUGUI pText)
                {
                    pText.text = node.GetAttributeValue("placeholder", "").Trim();
                    TextStyleHandler.Apply(node, pText, styles, HorizontalAlignmentOptions.Left, defaultVertAlign, isTextOverflow: _isTextOverflow);
                    pText.color = new Color(pText.color.r, pText.color.g, pText.color.b, 0.5f);
                }

                if (inputField.textComponent != null)
                    TextStyleHandler.Apply(node, inputField.textComponent, styles, HorizontalAlignmentOptions.Left, defaultVertAlign, isTextOverflow: _isTextOverflow);

                if (node.GetAttributeValue("type", "") == "password")
                    inputField.contentType = TMP_InputField.ContentType.Password;

                if (inputField.textViewport != null && PaddingParser.TryParse(styles, out RectOffset rectOffset))
                {
                    inputField.textViewport.offsetMin = new Vector2(rectOffset.left, rectOffset.bottom);
                    inputField.textViewport.offsetMax = new Vector2(-rectOffset.right, -rectOffset.top);
                }

                inputField.lineType = isTextarea ? TMP_InputField.LineType.MultiLineNewline : TMP_InputField.LineType.SingleLine;
            }
        }

        /// <summary> 创建 Toggle（复选框/单选框）</summary>
        /// <param name="node">HTML 节点</param>
        /// <param name="styles">样式</param>
        /// <param name="parent">父级</param>
        /// <returns>创建的 GameObject</returns>
        public virtual GameObject CreateToggle(HtmlNode node, Dictionary<string, string> styles, Transform parent)
        {
            GameObject go;
            if (_prefabs?.togglePrefab != null)
                go = InstantiateFromPrefab(_prefabs?.togglePrefab, parent, NodeHelper.GetNodeId(node));
            else
            {
                go = CreateViaMenu("GameObject/UI/Toggle", parent, NodeHelper.GetNodeId(node));
                RectTransform background = go.transform.Find("Background")?.GetComponent<RectTransform>();
                RectTransform check = background?.Find("Checkmark")?.GetComponent<RectTransform>();
                RectTransformHelper.FullyFillParent(background);
                RectTransformHelper.FullyFillParent(check);
            }

            Toggle toggle = go.GetComponent<Toggle>();
            if (toggle)
            {
                toggle.isOn = node.Attributes.Contains("checked");
                MaskableGraphic txt = toggle.GetComponentInChildren<TextMeshProUGUI>() ?? toggle.GetComponentInChildren<Text>() as MaskableGraphic;
                if (txt != null) UnityEngine.Object.DestroyImmediate(txt.gameObject);
            }
            VisualStyleApplier.ApplyCommonStyles(go, styles, _pseudoClassStyles, _htmlFilePath);
            return go;
        }

        /// <summary> 创建 Slider </summary>
        /// <param name="node">HTML 节点</param>
        /// <param name="styles">样式</param>
        /// <param name="parent">父级</param>
        /// <returns>创建的 GameObject</returns>
        public virtual GameObject CreateSlider(HtmlNode node, Dictionary<string, string> styles, Transform parent)
        {
            GameObject go;
            if (_prefabs?.sliderPrefab != null)
                go = InstantiateFromPrefab(_prefabs?.sliderPrefab, parent, NodeHelper.GetNodeId(node));
            else
            {
                go = CreateViaMenu("GameObject/UI/Slider", parent, NodeHelper.GetNodeId(node));
                go.GetComponentInChildren<Image>().sprite = null;
            }

            Slider slider = go.GetComponent<Slider>();
            if (slider)
            {
                if (float.TryParse(node.GetAttributeValue("min", "0"), NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
                    slider.minValue = v;
                if (float.TryParse(node.GetAttributeValue("max", "100"), NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                    slider.maxValue = v;
                if (float.TryParse(node.GetAttributeValue("value", "50"), NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                    slider.value = v;
            }
            VisualStyleApplier.ApplyCommonStyles(go, styles, _pseudoClassStyles, _htmlFilePath);
            return go;
        }

        /// <summary> 创建 Dropdown </summary>
        /// <param name="node">HTML 节点</param>
        /// <param name="styles">样式</param>
        /// <param name="parent">父级</param>
        /// <returns>创建的 GameObject</returns>
        public virtual GameObject CreateDropdown(HtmlNode node, Dictionary<string, string> styles, Transform parent)
        {
            GameObject go;
            if (_prefabs?.dropdownPrefab != null)
                go = InstantiateFromPrefab(_prefabs?.dropdownPrefab, parent, NodeHelper.GetNodeId(node));
            else
            {
                go = CreateViaMenu(GetMenuPath(_isLegacyText, "Dropdown"),
                    parent, NodeHelper.GetNodeId(node));
                go.GetComponent<Image>().sprite = null;
            }

            if (_isLegacyText)
                ApplyDropdownContent<Dropdown>(go, node, styles);
            else
                ApplyDropdownContent<TMP_Dropdown>(go, node, styles);

            VisualStyleApplier.ApplyCommonStyles(go, styles, _pseudoClassStyles, _htmlFilePath);
            return go;
        }

        internal virtual void ApplyDropdownContent<T>(GameObject go, HtmlNode node, Dictionary<string, string> styles) where T : Selectable
        {
            T dropdown = go.GetComponent<T>();
            if (dropdown == null) return;
            bool isLegacy = dropdown is Dropdown;

            // 通过反射获取 captionText / itemText / options 等成员
            var dropdownType = typeof(T);
            var captionProp = dropdownType.GetProperty("captionText");
            var itemTextProp = dropdownType.GetProperty("itemText");
            var optionsProp = dropdownType.GetProperty("options");

            object captionText = captionProp?.GetValue(dropdown);
            object itemText = itemTextProp?.GetValue(dropdown);

            if (captionText is MaskableGraphic captionGraphic)
                TextStyleHandler.Apply(node, captionGraphic, styles, HorizontalAlignmentOptions.Left, isTextOverflow: _isTextOverflow);
            if (itemText is MaskableGraphic itemGraphic)
                TextStyleHandler.Apply(node, itemGraphic, styles, HorizontalAlignmentOptions.Left, isTextOverflow: _isTextOverflow);

            if (captionText is RectTransform captionRt && PaddingParser.TryParse(styles, out RectOffset rectOffset))
            {
                captionRt.offsetMin = new Vector2(rectOffset.left, rectOffset.bottom);
                captionRt.offsetMax = new Vector2(-rectOffset.right, -rectOffset.top);
            }

            // 解析 option 子标签
            if (optionsProp != null)
            {
                var optionsList = optionsProp.GetValue(dropdown) as System.Collections.IList;
                if (optionsList != null)
                {
                    // 使用反射清空列表
                    var clearMethod = optionsList.GetType().GetMethod("Clear");
                    clearMethod?.Invoke(optionsList, null);

                    var addMethod = optionsList.GetType().GetMethod("Add");
                    foreach (var option in node.SelectNodes(".//option") ?? new HtmlNodeCollection(node))
                    {
                        string optionText = NodeHelper.GetNodeText(option, styles, _isLegacyText);
                        if (isLegacy)
                            addMethod?.Invoke(optionsList, new object[] { new Dropdown.OptionData(optionText.Trim()) });
                        else
                            addMethod?.Invoke(optionsList, new object[] { new TMP_Dropdown.OptionData(optionText) });
                    }
                }
            }
        }

        /// <summary> 创建图片 </summary>
        /// <param name="node">HTML 节点</param>
        /// <param name="styles">样式</param>
        /// <param name="parent">父级</param>
        /// <returns>创建的 GameObject</returns>
        public virtual GameObject CreateImage(HtmlNode node, Dictionary<string, string> styles, Transform parent)
        {
            GameObject go = new GameObject(NodeHelper.GetNodeId(node), typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            bool success = ResourceLoader.LoadImage(go, node.GetAttributeValue("src", ""), _htmlFilePath);
            VisualStyleApplier.ApplyCommonStyles(go, styles, _pseudoClassStyles, _htmlFilePath);
            if (!success&&node.GetAttributeValue("alt", "") is string alt && !string.IsNullOrEmpty(alt))
            {
                NodeHelper.CreateInterTextIfHas(node, styles, go.transform, CreateText, alt);
            }
            return go;
        }

        /// <summary> 创建 ScrollView </summary>
        /// <param name="node">HTML 节点</param>
        /// <param name="styles">样式</param>
        /// <param name="parent">父级</param>
        /// <returns>创建的 GameObject</returns>
        public virtual GameObject CreateScrollView(HtmlNode node, Dictionary<string, string> styles, Transform parent)
        {
            GameObject go;
            if (_prefabs?.scrollViewPrefab != null)
                go = InstantiateFromPrefab(_prefabs?.scrollViewPrefab, parent, NodeHelper.GetNodeId(node));
            else
            {
                go = CreateViaMenu("GameObject/UI/Scroll View", parent, NodeHelper.GetNodeId(node));
                go.GetComponent<Image>().sprite = null;
                go.GetComponent<Image>().color = Color.clear;
            }

            ConfigureScrollRect(go, styles);
            VisualStyleApplier.ApplyCommonStyles(go, styles, _pseudoClassStyles, _htmlFilePath);
            NodeHelper.CreateInterTextIfHas(node, styles, go.GetComponentInChildren<ScrollRect>().content ?? go.transform, CreateText);
            return go;
        }

        internal virtual void ConfigureScrollRect(GameObject go, Dictionary<string, string> styles)
        {
            ScrollRect scrollRect = go.GetComponent<ScrollRect>();
            if (scrollRect == null || scrollRect.content == null) return;

            RectTransformHelper.FullyFillParent(scrollRect.content);
            RectTransform goRect = scrollRect.GetComponent<RectTransform>();
            scrollRect.horizontal = false;
            scrollRect.vertical = false;

            // overflow 属性
            if (styles.TryGetValue("overflow", out string overflow))
            {
                var split = overflow.Split(' ');
                if (split.Length > 1)
                {
                    scrollRect.horizontal = split[0].Contains("scroll") || split[0].Contains("auto");
                    scrollRect.vertical = split[1].Contains("scroll") || split[1].Contains("auto");
                }
                else
                {
                    scrollRect.horizontal = overflow.Contains("scroll") || overflow.Contains("auto");
                    scrollRect.vertical = overflow.Contains("scroll") || overflow.Contains("auto");
                }
            }
            if (styles.TryGetValue("overflow-x", out string ox))
                scrollRect.horizontal = ox.Contains("scroll") || ox.Contains("auto");
            if (styles.TryGetValue("overflow-y", out string oy))
                scrollRect.vertical = oy.Contains("scroll") || oy.Contains("auto");
            if (!scrollRect.horizontal)
            {
                scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            }
            else
            {
                if(scrollRect.content.rect.height==0&& scrollRect.horizontalScrollbar)
                    scrollRect.content.offsetMin = new Vector2(0, -(scrollRect.horizontalScrollbar.GetComponent<RectTransform>().rect.height+ scrollRect.horizontalScrollbarSpacing));
                else
                    scrollRect.content.offsetMin = new Vector2(0, -(goRect.rect.height - scrollRect.content.rect.height));
            }
            if (!scrollRect.vertical)
            {
                scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            }
            else
            {
                if (scrollRect.content.rect.width == 0 && scrollRect.verticalScrollbar)
                    scrollRect.content.offsetMax = new Vector2(scrollRect.verticalScrollbar.GetComponent<RectTransform>().rect.width + scrollRect.verticalScrollbarSpacing, 0);
                else
                    scrollRect.content.offsetMax = new Vector2(goRect.rect.width - scrollRect.content.rect.width, 0);
            }
            // 滚动条样式
            ConfigureScrollbarStyles(scrollRect);
        }

        internal virtual void ConfigureScrollbarStyles(ScrollRect scrollRect)
        {
            if (_pseudoClassStyles.ContainsKey("-webkit-scrollbar"))
            {
                var scrollbarStyle = _pseudoClassStyles["-webkit-scrollbar"];
                if (scrollbarStyle.TryGetValue("width", out string scrollbarWidth))
                    scrollRect.verticalScrollbarSpacing = -UnitParser.Parse(scrollbarWidth);
                if (scrollbarStyle.TryGetValue("height", out string scrollbarHeight))
                    scrollRect.horizontalScrollbarSpacing = -UnitParser.Parse(scrollbarHeight);
                if (ColorParser.TryParseBackgroundColor(scrollbarStyle, out Color scrollbarColor))
                {
                    var img = scrollRect.GetComponent<Image>();
                    if (img != null) img.color = scrollbarColor;
                }
            }

            if (_pseudoClassStyles.ContainsKey("-webkit-scrollbar-thumb"))
            {
                var thumbStyle = _pseudoClassStyles["-webkit-scrollbar-thumb"];
                if (ColorParser.TryParseBackgroundColor(thumbStyle, out Color thumbColor))
                {
                    SetScrollbarHandleColor(scrollRect.verticalScrollbar, thumbColor);
                    SetScrollbarHandleColor(scrollRect.horizontalScrollbar, thumbColor);
                }
            }

            if (_pseudoClassStyles.ContainsKey("-webkit-scrollbar-track"))
            {
                var trackStyle = _pseudoClassStyles["-webkit-scrollbar-track"];
                if (ColorParser.TryParseBackgroundColor(trackStyle, out Color trackColor))
                {
                    SetScrollbarTrackColor(scrollRect.verticalScrollbar, trackColor);
                    SetScrollbarTrackColor(scrollRect.horizontalScrollbar, trackColor);
                }
            }
        }
        /// <summary>
        /// 设置滚动条手柄颜色
        /// </summary>
        /// <param name="scrollbar">滚动条</param>
        /// <param name="color">颜色</param>
        public static void SetScrollbarHandleColor(Scrollbar scrollbar, Color color)
        {
            if (scrollbar == null) return;
            var img = scrollbar.handleRect?.GetComponent<Image>();
            if (img != null) img.color = color;
        }
        /// <summary>
        /// 设置滚动条轨道颜色
        /// </summary>
        /// <param name="scrollbar">滚动条</param>
        /// <param name="color">颜色</param>
        public static void SetScrollbarTrackColor(Scrollbar scrollbar, Color color)
        {
            if (scrollbar == null) return;
            var img = scrollbar.GetComponent<Image>();
            if (img != null) img.color = color;
        }

#endregion

    }

}

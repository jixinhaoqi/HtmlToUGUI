using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Xxhq.Htmltougui.Editor
{
    /// <summary>
    /// HTML 转 UGUI 编辑器窗口。
    /// 负责用户界面交互、文件监视，并协调各模块完成 HTML 到 UGUI 的转换。
    /// </summary>
    public class HtmlToUguiConverter : EditorWindow
    {
        public UiPrefabSettings Prefabs => _prefabs;
        private UiPrefabSettings _prefabs;
        private SerializedProperty _containerPrefabProp;
        private SerializedProperty _textPrefabProp;
        private SerializedProperty _buttonPrefabProp;
        private SerializedProperty _inputPrefabProp;
        private SerializedProperty _togglePrefabProp;
        private SerializedProperty _sliderPrefabProp;
        private SerializedProperty _dropdownPrefabProp;
        private SerializedProperty _scrollViewPrefabProp;
        public string HtmlFilePath { get => _htmlFilePath; set => _htmlFilePath = value; }
        private string _htmlFilePath = "";
        private string _htmlToolPath = "";

        public bool IsEnableWatcherHtml { get => _isEnableWatcherHtml; set => _isEnableWatcherHtml = value; }
        private bool _isEnableWatcherHtml;

        public bool IsAutoToUGui { get => _isAutoToUGui; set => _isAutoToUGui = value; }
        private bool _isAutoToUGui = true;

        public bool IsLegacyText { get => _isLegacyText; set => _isLegacyText = value; }
        private bool _isLegacyText;

        public int LayoutCalculatorIndex { get => _layoutCalculatorIndex; set => _layoutCalculatorIndex = value; }
        private int _layoutCalculatorIndex;

        public LayoutCalculator[] LayoutCalculatorTypes => _layoutCalculatorTypes;
        private LayoutCalculator[] _layoutCalculatorTypes;
        private string[] _layoutCalculatorTypeNames;

        public bool IsTextOverflow { get => _isTextOverflow; set => _isTextOverflow = value; }
        private bool _isTextOverflow = true;


        // CSS 继承样式属性（公开供外部访问）
        /// <summary> CSS 可继承样式属性集合 </summary>
        public HashSet<string> InheritStyles { get; } = new HashSet<string>
        {
            "font-family","font-size","font-weight","font-style","font-variant","line-height",
            "color","text-align","text-indent","text-transform","letter-spacing","word-spacing",
            "white-space","direction","visibility","cursor",
            "list-style","list-style-type","list-style-position","list-style-image",
            "border-collapse","border-spacing","caption-side","empty-cells","table-layout",
            "quotes","opacity"
        };

        private string[] _toolToolbarNames= new string[] { "HTML解构工具", "AI生成HTML提示词" };
        private int _toolToolbarIndex = 0;
        // 标签处理器注册表
        private Dictionary<string, ITagHandler> _tagHandlers;

        public string HtmlContent { get => _htmlContent; set => _htmlContent = value; }
        private string _htmlContent = "";

        private Vector2 _scrollPosition;
        private Vector2 _textAreaScrollPosition;
        private SerializedObject _so;
        private bool _isShowHtmlEditor = true;
        private int _showHtmlEditorLengthLimit = 50000;

        // 模块实例（延迟初始化）
        private CssParser _cssParser;
        private LayoutCalculator _layoutCalculator;
        private UguiElementFactory _elementFactory;

        private FileWatcherService _fileWatcher;
        private Dictionary<string, string> InheritStyleTemp = new Dictionary<string, string>();
        // 偏好配置
        private const string PREFS_UI_PREFAB_SETTINGS_PATH_KEY = "HtmlToUguiConverter_UiPrefabSettingsPath";
        private const string PREFS_HTML_FILE_PATH_KEY = "HtmlToUguiConverter_HtmlFilePath";
        private const string PREFS_IS_ENABLE_WATCHER_HTML_KEY = "HtmlToUguiConverter_IsEnableWatcherHtml";
        private const string PREFS_IS_AUTO_TO_UGUI_KEY = "HtmlToUguiConverter_IsAutoToUGui";
        private const string PREFS_IS_LEGACY_TEXT_KEY = "HtmlToUguiConverter_IsLegacyText";
        private const string PREFS_LAYOUT_CALCULATOR_INDEX_KEY = "HtmlToUguiConverter_LayoutCalculatorIndex";
        private const string PREFS_IS_TEXT_OVERFLOW_KEY = "HtmlToUguiConverter_IsTextOverflow";
        private const string PREFS_TOOL_TOOLBAR_INDEX_KEY = "HtmlToUguiConverter_ToolToolbarIndex";

        [MenuItem("Tools/HTML to UGUI Converter")]
        public static void ShowWindow() 
        {
            HtmlToUguiConverter window = GetWindow<HtmlToUguiConverter>("HTML 转 UGUI");
            //window.minSize = new Vector2(650, 600);
        }

        private void OnDisable()
        {
            _fileWatcher?.Dispose();
            _fileWatcher = null;
            EditorPrefs.SetString(PREFS_HTML_FILE_PATH_KEY, _htmlFilePath);
            EditorPrefs.SetBool(PREFS_IS_ENABLE_WATCHER_HTML_KEY, _isEnableWatcherHtml);
            EditorPrefs.SetBool(PREFS_IS_AUTO_TO_UGUI_KEY, _isAutoToUGui);
            EditorPrefs.SetBool(PREFS_IS_LEGACY_TEXT_KEY, _isLegacyText);
            EditorPrefs.SetInt(PREFS_LAYOUT_CALCULATOR_INDEX_KEY, _layoutCalculatorIndex);
            EditorPrefs.SetBool(PREFS_IS_TEXT_OVERFLOW_KEY, _isTextOverflow);
            EditorPrefs.SetInt(PREFS_TOOL_TOOLBAR_INDEX_KEY, _toolToolbarIndex);
            if(_layoutCalculatorTypes != null)
                foreach(var v in _layoutCalculatorTypes)
                    v.OnDisable();
        }

        private void OnEnable()
        {
            try
            {
                EditorStyles.helpBox.fontSize = 12;
            }catch{}
            
            string uiPrefabSettingsPath = EditorPrefs.GetString(PREFS_UI_PREFAB_SETTINGS_PATH_KEY, "");
            _htmlFilePath = EditorPrefs.GetString(PREFS_HTML_FILE_PATH_KEY, "");
            _isEnableWatcherHtml = EditorPrefs.GetBool(PREFS_IS_ENABLE_WATCHER_HTML_KEY, false);
            _isAutoToUGui = EditorPrefs.GetBool(PREFS_IS_AUTO_TO_UGUI_KEY, true);
            _isLegacyText = EditorPrefs.GetBool(PREFS_IS_LEGACY_TEXT_KEY, false);
            _layoutCalculatorIndex = EditorPrefs.GetInt(PREFS_LAYOUT_CALCULATOR_INDEX_KEY, 0);
            _isTextOverflow = EditorPrefs.GetBool(PREFS_IS_TEXT_OVERFLOW_KEY, true);
            _toolToolbarIndex = EditorPrefs.GetInt(PREFS_TOOL_TOOLBAR_INDEX_KEY, 0);

            string path = GetPackagesFullPath("Tools/HtmlTools/HTML解构工具.html");
            if (File.Exists(path))
            {
                _htmlToolPath = path;
            }

            if (!string.IsNullOrEmpty(uiPrefabSettingsPath))
            {
                _prefabs = AssetDatabase.LoadAssetAtPath<UiPrefabSettings>(uiPrefabSettingsPath);
            }
            _fileWatcher = new FileWatcherService();
            _fileWatcher.Changed += OnHtmlChanged;
            _fileWatcher.Deleted += OnHtmlDeleted;
            _fileWatcher.Renamed += OnHtmlRenamed;
            _fileWatcher.Error += OnHtmlError;
            DiscoverScripts();
        }


        private string GetPackagesFullPath(string relativePath)
        {
            string path = ResourceLoader.GetRegularPath(GetCurrentFilePath());
            if (path.Contains("/Editor/"))
            {
                if (path.Contains("/Packages/") || path.Contains("/Library/"))
                {
                    path = path.Substring(0, path.LastIndexOf("/Editor/")).TrimStart('.').TrimStart('/');
                    if (!Path.IsPathRooted(path))
                    {
                        string projectPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6);
                        path = projectPath + path;
                    }
                }
                else
                    path = path.Substring(0, path.LastIndexOf("/Editor/"));
                path = path + "/" + relativePath;
                return path;
            }
            return "";
        }

        /// <summary>
        /// 重新发现并注册布局计算器和标签处理器。
        /// </summary>
        public void DiscoverScripts()
        {
            DiscoverLayoutCalculators();
            DiscoverTagHandlers();
            if (_layoutCalculatorTypes != null)
                foreach (var v in _layoutCalculatorTypes)
                    v.OnEnable();
        }
        private void DiscoverLayoutCalculators()
        {
            var list = AssemblyHelper.CreateSupTypeInstances<LayoutCalculator>();
            // 智能布局计算器排在第一位
            if (list.FirstOrDefault(i => i.GetType().Name.StartsWith("SmartLayoutCalculator")) is var smartLayoutCalculator)
            {
                list.Remove(smartLayoutCalculator);
                list.Insert(0, smartLayoutCalculator);
            }
            _layoutCalculatorTypes = list.ToArray();
            _layoutCalculatorTypeNames = new string[_layoutCalculatorTypes.Length];
            for (int i = 0; i < _layoutCalculatorTypes.Length; i++)
            {
                _layoutCalculatorTypeNames[i] = GetLocaleContent( _layoutCalculatorTypes[i].GetType(), Application.systemLanguage);

            }
                
        }

        private void DiscoverTagHandlers()
        {
            _tagHandlers = new Dictionary<string, ITagHandler>();
            var list = AssemblyHelper.CreateSupTypeInstances<ITagHandler>();
            foreach (var t in list)
            {
                foreach (var tag in t.SupportedTags)
                    _tagHandlers[tag] = t;
            }
        }

        private string GetLocaleContent(Type type, SystemLanguage locale)
        {
            var cnAttr = type.GetCustomAttributes<LocaleAttribute>(true).FirstOrDefault(attr => attr.Language == locale && !string.IsNullOrEmpty(attr.Content));
            if (cnAttr != null)
                return cnAttr.Content;
            return type.Name;
        }
        // ========== UI 绘制 ==========
        private void OnGUI()
        {
            GUILayout.Label("将 HTML 内容转换为 UGUI 结构", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            {
                DrawUiPrefabSettingsUI();
                DrawExternalToolchainUI();
                DrawConvertSettingUI();
                EditorGUILayout.EndScrollView();
            }
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = Color.green;
            if (!string.IsNullOrEmpty(_htmlContent) && GUILayout.Button("开始转换", GUILayout.Height(30)))
                ConvertHtmlToUgui();

            GUI.backgroundColor = Color.white;
            GUILayout.Space(10);
        }

        // ========== 预设UI绘制 ==========
        private void DrawUiPrefabSettingsUI()
        {
            GUILayout.Label("1. 配置 UI 模板 (可选)", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            {
                _prefabs = (UiPrefabSettings)EditorGUILayout.ObjectField("模板文件 (SO)", _prefabs, typeof(UiPrefabSettings),false);
                if (EditorGUI.EndChangeCheck())
                {
                    string path = _prefabs != null ? AssetDatabase.GetAssetPath(_prefabs) : "";
                    EditorPrefs.SetString(PREFS_UI_PREFAB_SETTINGS_PATH_KEY, path);
                    _so = null;
                }
            }
            if (_prefabs == null)
            {
                EditorGUILayout.HelpBox(
                    "可以使用自定义预设创建对象，但需要创建并分配 UiPrefabSettings 配置文件(右键 Project 窗口 -> Create -> Html To UGUI -> UiPrefabSettings)",
                    MessageType.Info);
            }
            else
            {
                if (_so == null || _so.targetObject != _prefabs)
                {
                    _so = new SerializedObject(_prefabs);
                    _containerPrefabProp = _so.FindProperty("containerPrefab");
                    _textPrefabProp = _so.FindProperty("textPrefab");
                    _buttonPrefabProp = _so.FindProperty("buttonPrefab");
                    _inputPrefabProp = _so.FindProperty("inputPrefab");
                    _togglePrefabProp = _so.FindProperty("togglePrefab");
                    _sliderPrefabProp = _so.FindProperty("sliderPrefab");
                    _dropdownPrefabProp = _so.FindProperty("dropdownPrefab");
                    _scrollViewPrefabProp = _so.FindProperty("scrollViewPrefab");
                }

                _so.Update();
                EditorGUI.BeginChangeCheck();
                {
                    EditorGUILayout.PropertyField(_containerPrefabProp, new GUIContent("纯容器"));
                    EditorGUILayout.PropertyField(_textPrefabProp, new GUIContent("文本"));
                    EditorGUILayout.PropertyField(_buttonPrefabProp, new GUIContent("按钮"));
                    EditorGUILayout.PropertyField(_inputPrefabProp, new GUIContent("输入框"));
                    EditorGUILayout.PropertyField(_togglePrefabProp, new GUIContent("开关"));
                    EditorGUILayout.PropertyField(_sliderPrefabProp, new GUIContent("滑块"));
                    EditorGUILayout.PropertyField(_dropdownPrefabProp, new GUIContent("下拉菜单"));
                    EditorGUILayout.PropertyField(_scrollViewPrefabProp, new GUIContent("滚动视图"));
                    if (EditorGUI.EndChangeCheck())
                    {
                        _so.ApplyModifiedProperties();
                    }
                }
            }
            EditorGUILayout.Space(10);
        }

        private void DrawExternalToolchainUI()
        {
            GUILayout.Label("2. 预处理", EditorStyles.boldLabel);
            _toolToolbarIndex = GUILayout.Toolbar(_toolToolbarIndex, _toolToolbarNames);
            if (_toolToolbarIndex == 0)
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("在浏览器中打开", GUILayout.ExpandWidth(false)))
                    {
                        if (string.IsNullOrWhiteSpace(_htmlToolPath) || !File.Exists(_htmlToolPath))
                        {
                            string path = GetPackagesFullPath("Tools/HtmlTools/HTML解构工具.html");
                            if (!File.Exists(path))
                                Debug.LogError("路径无效。");
                            else
                            {
                                Debug.Log("已在浏览器中打开。");
                                _htmlToolPath = path;
                                Application.OpenURL(_htmlToolPath);
                            }
                        }
                        else
                            Application.OpenURL(_htmlToolPath);
                    }
                    EditorGUILayout.SelectableLabel(_htmlToolPath, GUILayout.Height(20));
                    GUILayout.EndHorizontal();
                }
            }
            else 
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("复制内容到剪切板：");
                    if (GUILayout.Button("SKILL_动态定位", GUILayout.ExpandWidth(false)))
                    {
                        string path = GetPackagesFullPath("Tools/HtmlTools/AI生成HTML提示词/SKILL_动态定位.md");
                        if (!File.Exists(path))
                            Debug.LogError("路径无效。");
                        else
                        {
                            Debug.Log("已复制到剪切板。");
                            string content = File.ReadAllText(path);
                            GUIUtility.systemCopyBuffer = content;
                        }
                    }
                    if (GUILayout.Button("SKILL_绝对定位", GUILayout.ExpandWidth(false)))
                    {
                        string path = GetPackagesFullPath("Tools/HtmlTools/AI生成HTML提示词/SKILL_绝对定位.md");
                        if (!File.Exists(path))
                            Debug.LogError("路径无效。");
                        else
                        {
                            Debug.Log("已复制到剪切板。");
                            string content = File.ReadAllText(path);
                            GUIUtility.systemCopyBuffer = content;
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.Space(10);
        }


        private void DrawConvertSettingUI()
        {
            GUILayout.Label("3. 转换设置", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("选择 HTML 文件", GUILayout.ExpandWidth(false)))
                {
                    _htmlFilePath = EditorUtility.OpenFilePanel("选择 HTML", "", "html");
                    GUI.FocusControl(null);
                    if (File.Exists(_htmlFilePath))
                    {
                        LoadHtmlContentByPath();
                        _isShowHtmlEditor = _htmlContent.Length <= _showHtmlEditorLengthLimit;
                        if (_isAutoToUGui && !string.IsNullOrEmpty(_htmlContent))
                            ConvertHtmlToUgui();
                    }
                }
                EditorGUILayout.SelectableLabel(_htmlFilePath, GUILayout.Height(20));

                _isAutoToUGui = GUILayout.Toggle(_isAutoToUGui, "自动转换", GUILayout.ExpandWidth(false));

                EditorGUI.BeginChangeCheck();
                {
                    if (!string.IsNullOrEmpty(_htmlFilePath))
                        _isEnableWatcherHtml = GUILayout.Toggle(_isEnableWatcherHtml, "同步文件", GUILayout.ExpandWidth(false));
                    else if (_isEnableWatcherHtml) { _isEnableWatcherHtml = false; _fileWatcher.StopWatching(); }

                    if (EditorGUI.EndChangeCheck())
                    {
                        if (_isEnableWatcherHtml)
                            _fileWatcher.StartWatching(_htmlFilePath);
                        else
                            _fileWatcher.StopWatching();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.HelpBox($"将 【预处理】 处理后的字符串粘贴到下方 【HTML内容】 输入框内，或者通过 【选择HTML文件】 载入", MessageType.Info);
            EditorGUILayout.HelpBox($"【HTML内容】 输入框的内容必须包含 【预处理】 处理后的字符串，或者所有标签都有 【绝对定位的样式】 (left、top、width、height)，才能正确处理标签布局", MessageType.Warning);
            _isShowHtmlEditor = EditorGUILayout.Foldout(_isShowHtmlEditor, $"HTML 内容 ({(_htmlContent.Length):N0} 字符)");

            _textAreaScrollPosition = EditorGUILayout.BeginScrollView(_textAreaScrollPosition, GUILayout.Height(100));
            {
                if (_isShowHtmlEditor)
                {
                    EditorGUI.BeginChangeCheck();
                    {
                        _htmlContent = EditorGUILayout.TextArea(_htmlContent, GUILayout.ExpandHeight(true));
                        if (EditorGUI.EndChangeCheck() && _htmlContent.Length > _showHtmlEditorLengthLimit)
                            _isShowHtmlEditor = false;
                    }
                }
                else if (!string.IsNullOrEmpty(_htmlContent))
                {
                    string preview = _htmlContent.Length > _showHtmlEditorLengthLimit ? _htmlContent.Substring(0, 500) + "..." : _htmlContent;
                    EditorGUILayout.HelpBox(preview, MessageType.None);
                }

                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("布局计算器:", GUILayout.ExpandWidth(false));
                _layoutCalculatorIndex = EditorGUILayout.Popup(_layoutCalculatorIndex, _layoutCalculatorTypeNames);
                EditorGUILayout.EndHorizontal();
            }
            if (_layoutCalculatorTypes?.Length > 0&& _layoutCalculatorIndex < _layoutCalculatorTypes.Length)
                _layoutCalculatorTypes[_layoutCalculatorIndex].OnGUI();
            _isLegacyText = GUILayout.Toggle(_isLegacyText, "使用旧版 Text 组件");
            _isTextOverflow = GUILayout.Toggle(_isTextOverflow, "文本溢出（如果文字超出无法正常显示，建议开启）");
            GUILayout.Space(10);
        }


        // ========== 文件系统监视 ==========
        private void OnHtmlChanged(object sender, FileSystemEventArgs e)
        {
            LoadHtmlContentByPath();
            if (_isAutoToUGui && _isEnableWatcherHtml && !string.IsNullOrEmpty(_htmlContent))
                ConvertHtmlToUgui();
            Debug.Log("HTML 文件已更改");
        }

        private void OnHtmlDeleted(object sender, FileSystemEventArgs e)
        { _htmlFilePath = ""; _fileWatcher.StopWatching(); Debug.Log("HTML 文件已删除"); }

        private void OnHtmlRenamed(object sender, RenamedEventArgs e)
        { _htmlFilePath = e.FullPath; _fileWatcher.UpdatePath(e.FullPath); Debug.Log("HTML 文件已重命名"); }

        private void OnHtmlError(object sender, ErrorEventArgs e)
        { _htmlFilePath = ""; _fileWatcher.StopWatching(); Debug.LogError($"监控出错: {e.GetException().Message}"); }

        // ========== 核心转换流程 ==========
        private void LoadHtmlContentByPath()
        {
            if(string.IsNullOrEmpty(_htmlFilePath))return;
            try { _htmlContent = File.ReadAllText(_htmlFilePath); }
            catch (Exception ex) { Debug.LogError($"读取 HTML 文件失败: {ex.Message}"); }
        }

        /// <summary>
        /// 初始化或获取模块实例（每次转换前调用以确保配置同步）
        /// </summary>
        private void EnsureModulesReady()
        {
            if (_layoutCalculatorTypes == null || _layoutCalculatorTypes.Length == 0)
            {
                Debug.LogError("未发现任何 LayoutCalculator 实现类");
                return;
            }
            int safeIndex = Mathf.Clamp(_layoutCalculatorIndex, 0, _layoutCalculatorTypes.Length - 1);
            _cssParser = new CssParser();
            _layoutCalculator = _layoutCalculatorTypes[safeIndex];
            _elementFactory = new UguiElementFactory(_prefabs, _isLegacyText, _htmlFilePath, _cssParser, _isTextOverflow);
        }

        private void ConvertHtmlToUgui()
        {
            EnsureModulesReady();

            // 加载 HTML
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(_htmlContent);

            // 创建/复用 Canvas
#if UNITY_6000_0_OR_NEWER
            GameObject rootCanvas = FindAnyObjectByType<Canvas>()?.gameObject;
#else
            GameObject rootCanvas = FindObjectOfType<Canvas>()?.gameObject;
#endif

            if (rootCanvas == null)
            {
                rootCanvas = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                rootCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                rootCanvas.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                if (GameViewResolutionHelper.GetSelectedGameViewSize(out Vector2 size))
                    rootCanvas.GetComponent<CanvasScaler>().referenceResolution = size;
                Undo.RegisterCreatedObjectUndo(rootCanvas, "Canvas");
            }

            // 创建 HTML 根容器
            if (rootCanvas.transform.Find("HTML_Content"))
                Undo.DestroyObjectImmediate(rootCanvas.transform.Find("HTML_Content").gameObject);

            GameObject htmlRoot = new GameObject("HTML_Content", typeof(RectTransform));
            htmlRoot.transform.SetParent(rootCanvas.transform, false);
            RectTransformHelper.FullyFillParent(htmlRoot.GetComponent<RectTransform>());
            Undo.RegisterCreatedObjectUndo(htmlRoot, "htmlRoot");

            // 解析 body 并开始递归
            HtmlNode bodyNode = doc.DocumentNode.SelectSingleNode("//body") ?? doc.DocumentNode.SelectSingleNode("//div");
            if (bodyNode != null)
            {
                _cssParser.ParseStyleSheet(doc);
                ParseNode(bodyNode, htmlRoot.transform);
                Selection.activeGameObject = htmlRoot;
            }
            else
            {
                Debug.LogError("HTML 文件中没有找到 <body> 标签");
                return;
            }

            Debug.Log("HTML 转换完成！");
        }

        /// <summary>
        /// 递归解析 HTML 节点并创建对应的 UGUI 元素
        /// </summary>
        private void ParseNode(HtmlNode node, Transform parent, Dictionary<string, string> parentStyle = null)
        {
            if (ElementHelper.IsExcludeNode(node)) return;
            if (!parent) return;

            var styles = _cssParser.ResolveStyles(node, parentStyle);
            if (ElementHelper.IsHideNode(styles)) return;

            string tagName = node.Name.ToLower();
            GameObject go = CreateElementForTag(tagName, node, styles, parent);

            if (go != null)
            {
                _layoutCalculator.SetAnchorAndSize(go.GetComponent<RectTransform>(), styles, node);
                // 确定子节点挂载点（ScrollView 挂在 Content 下）
                Transform childParent = go.transform;
                var scrollRect = go.GetComponent<ScrollRect>();
                if (scrollRect != null && scrollRect.content != null) childParent = scrollRect.content;

                // 过滤可继承样式传递给子节点
                if (styles.Count > 0)
                {
                    InheritStyleTemp.Clear();
                    foreach (var kv in styles)
                    {
                        if (InheritStyles.Contains(kv.Key))
                            InheritStyleTemp[kv.Key] = kv.Value;
                    }
                    styles.Clear();
                    foreach (var kv in InheritStyleTemp)
                        styles[kv.Key] = kv.Value;
                }

                foreach (var child in node.ChildNodes)
                    ParseNode(child, childParent, styles);
            }
        }

        /// <summary>
        /// 根据标签名分发到对应的元素创建方法
        /// </summary>
        private GameObject CreateElementForTag(string tagName, HtmlNode node, Dictionary<string, string> styles, Transform parent)
        {
            if (_tagHandlers.TryGetValue(tagName, out var handler))
                return handler.CreateElement(node, styles, parent, _elementFactory);

            return ElementHelper.IsScrollContainer(styles)
                ? _elementFactory.CreateScrollView(node, styles, parent)
                : _elementFactory.CreateContainer(node, styles, parent);
        }

        public static string GetCurrentFilePath([CallerFilePath] string filePath = "")
        {
            return filePath;
        }


    }
}

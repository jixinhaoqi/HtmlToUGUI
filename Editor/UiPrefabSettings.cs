using UnityEngine;
using Xxhq.Htmltougui;

namespace Xxhq.Htmltougui.Editor
{
    /// <summary>
    /// UI 预制体配置，指定各 HTML 元素类型对应的组件预制体。
    /// 在 Project 窗口右键 Create → Html To UGUI → UiPrefabSettings 创建配置资源。
    /// </summary>
    [CreateAssetMenu(fileName = "UiPrefabSettings", menuName = "Html To UGUI/UiPrefabSettings")]
    public class UiPrefabSettings : ScriptableObject
    {
        [LocaleHeader(SystemLanguage.ChineseSimplified, "预制体配置")]
        [LocaleHeader(SystemLanguage.English, "Prefab Settings")]
        /// <summary> 纯容器预制体（对应 div/span/p 等）</summary>
        public GameObject containerPrefab;
        /// <summary> 文本组件预制体</summary>
        public GameObject textPrefab;
        /// <summary> 按钮预制体</summary>
        public GameObject buttonPrefab;
        /// <summary> 输入框预制体</summary>
        public GameObject inputPrefab;
        /// <summary> 开关/复选框预制体</summary>
        public GameObject togglePrefab;
        /// <summary> 滑块预制体</summary>
        public GameObject sliderPrefab;
        /// <summary> 下拉菜单预制体</summary>
        public GameObject dropdownPrefab;
        /// <summary> 滚动视图预制体</summary>
        public GameObject scrollViewPrefab;
    }
}

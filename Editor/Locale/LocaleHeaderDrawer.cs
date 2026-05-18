using UnityEditor;
using UnityEngine;
using Xxhq.Htmltougui;

namespace Xxhq.Htmltougui.Editor
{
    /// <summary>
    /// 本地化标题的 PropertyDrawer，根据系统语言显示对应文字。
    /// </summary>
    [CustomPropertyDrawer(typeof(LocaleHeaderAttribute))]
    public class LocaleHeaderDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            if (attribute is LocaleHeaderAttribute attr && !string.IsNullOrEmpty(attr.Content) && attr.Language == Application.systemLanguage)
            {
                position.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(position, attr.Content, EditorStyles.boldLabel);
            }
        }

        public override float GetHeight()
        {
            return attribute is LocaleHeaderAttribute attr && attr.Language == Application.systemLanguage ? EditorGUIUtility.singleLineHeight * 1.2f : 0f;
        }
    }
}

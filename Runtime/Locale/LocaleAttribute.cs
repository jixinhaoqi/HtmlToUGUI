using System;

namespace Xxhq.Htmltougui
{
    /// <summary>
    /// 本地化内容特性，用于标记类型的本地化显示名称。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class LocaleAttribute : Attribute
    {
        /// <summary> 语言 </summary>
        public UnityEngine.SystemLanguage Language { get; }
        /// <summary> 本地化内容 </summary>
        public string Content { get; }

        /// <summary>
        /// 创建本地化内容特性。
        /// </summary>
        /// <param name="language">语言</param>
        /// <param name="content">显示的本地化内容</param>
        public LocaleAttribute(UnityEngine.SystemLanguage language, string content)
        {
            Content = content;
            Language = language;
        }
    }
}

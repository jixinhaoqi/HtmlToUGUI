using System;
using UnityEngine;

namespace Xxhq.Htmltougui
{
    /// <summary>
    /// 本地化标题特性，用于 Inspector 中显示本地化分组标题。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class LocaleHeaderAttribute : PropertyAttribute
    {
        /// <summary> 语言 </summary>
        public SystemLanguage Language { get; }
        /// <summary> 显示内容 </summary>
        public string Content { get; }

        /// <summary>
        /// 创建本地化标题。
        /// </summary>
        /// <param name="language">语言</param>
        /// <param name="content">内容</param>
        public LocaleHeaderAttribute(SystemLanguage language, string content)
        {
            Content = content;
            Language = language;
        }
    }
}

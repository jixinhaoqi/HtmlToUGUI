using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Xxhq.Htmltougui
{
    /// <summary>
    /// 行内样式解析器
    /// </summary>
    public static class InlineStyleParser
    {
        private static readonly Regex s_ImportantRegex = new Regex(@"\s*!important\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// 解析行内样式字符串
        /// </summary>
        /// <param name="styleString">行内样式字符串</param>
        /// <returns>解析后的样式字典</returns>
        public static Dictionary<string, string> Parse(string styleString)
        {
            var styles = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(styleString)) return styles;
            styleString = System.Net.WebUtility.HtmlDecode(styleString);
            string[] declarations = styleString.Split(';');
            foreach (var decl in declarations)
            {
                if (string.IsNullOrWhiteSpace(decl)) continue;
                if (decl.Trim().StartsWith("/*") && decl.Trim().EndsWith("*/")) continue;
                if(!decl.Contains(":")) continue;
                int index = decl.IndexOf(':');
                string key= decl.Substring(0, index).Trim();
                if (key.Contains("*/"))
                    key = key.Substring(key.IndexOf("*/") + 2);
                string val = s_ImportantRegex.Replace(decl.Substring(index + 1).Trim(),"") ;
                styles[key] = val;
            }
            return styles;
        }
    }
}

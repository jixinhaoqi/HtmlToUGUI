using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Xxhq.Htmltougui
{
    /// <summary>
    /// CSS 颜色解析工具
    /// </summary>
    public static class ColorParser
    {
        private static readonly string ColorMatchKey =
            @"#(?:[0-9A-Fa-f]{8}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{4}|[0-9A-Fa-f]{3})\b|" +
            @"\b(?:aqua|black|blue|fuchsia|gray|green|lime|maroon|navy|olive|purple|red|silver|teal|white|yellow|" +
            @"orange|aliceblue|antiquewhite|aquamarine|azure|beige|bisque|blanchedalmond|blueviolet|brown|burlywood|" +
            @"cadetblue|chartreuse|chocolate|coral|cornflowerblue|cornsilk|crimson|cyan|darkblue|darkcyan|" +
            @"darkgoldenrod|darkgray|darkgreen|darkgrey|darkkhaki|darkmagenta|darkolivegreen|darkorange|darkorchid|" +
            @"darkred|darksalmon|darkseagreen|darkslateblue|darkslategray|darkslategrey|darkturquoise|darkviolet|" +
            @"deeppink|deepskyblue|dimgray|dimgrey|dodgerblue|firebrick|floralwhite|forestgreen|gainsboro|" +
            @"ghostwhite|gold|goldenrod|greenyellow|grey|honeydew|hotpink|indianred|indigo|ivory|khaki|" +
            @"lavender|lavenderblush|lawngreen|lemonchiffon|lightblue|lightcoral|lightcyan|" +
            @"lightgoldenrodyellow|lightgray|lightgreen|lightgrey|lightpink|lightsalmon|lightseagreen|" +
            @"lightskyblue|lightslategray|lightslategrey|lightsteelblue|lightyellow|limegreen|linen|" +
            @"magenta|mediumaquamarine|mediumblue|mediumorchain|mediumpurple|mediumseagreen|mediumslateblue|" +
            @"mediumspringgreen|mediumturquoise|mediumvioletred|midnightblue|mintcream|mistyrose|moccasin|" +
            @"navajowhite|oldlace|olivedrab|orangered|orchid|palegoldenrod|palegreen|paleturquoise|" +
            @"palevioletred|papayawhip|peachpuff|peru|pink|plum|powderblue|rosybrown|royalblue|" +
            @"saddlebrown|salmon|sandybrown|seagreen|seashell|sienna|skyblue|slateblue|slategray|" +
            @"slategrey|snow|springgreen|steelblue|tan|thistle|tomato|turquoise|violet|wheat|" +
            @"whitesmoke|yellowgreen|transparent|currentcolor)\b";

        private const string BackgroundUrlKey = @"^\s*url\(\s*(?:'[^']*'|""[^""]*""|[^)\s,]+)\s*\)\s*" +
            @"(?:(?:no-repeat|repeat-x|repeat-y|repeat|space|round|top|bottom|left|right|center|cover|contain|auto|0|\d+(?:\.\d+)?(?:px|em|rem|vw|vh|%|cm|mm|in|pt|pc))\s*)" +
            @"(?:\/\s*(?:no-repeat|repeat-x|repeat-y|repeat|space|round|top|bottom|left|right|center|cover|contain|auto|0|\d+(?:\.\d+)?(?:px|em|rem|vw|vh|%|cm|mm|in|pt|pc))(?:\s+(?:no-repeat|repeat-x|repeat-y|repeat|space|round|top|bottom|left|right|center|cover|contain|auto|0|\d+(?:\.\d+)?(?:px|em|rem|vw|vh|%|cm|mm|in|pt|pc)))?)?" +
            @"(?:\s*,\s*url\(\s*(?:'[^']*'|""[^""]*""|[^)\s,]+)\s*\)\s*" +
            @"(?:(?:no-repeat|repeat-x|repeat-y|repeat|space|round|top|bottom|left|right|center|cover|contain|auto|0|\d+(?:\.\d+)?(?:px|em|rem|vw|vh|%|cm|mm|in|pt|pc))\s*)" +
            @"(?:\/\s*(?:no-repeat|repeat-x|repeat-y|repeat|space|round|top|bottom|left|right|center|cover|contain|auto|0|\d+(?:\.\d+)?(?:px|em|rem|vw|vh|%|cm|mm|in|pt|pc))(?:\s+(?:no-repeat|repeat-x|repeat-y|repeat|space|round|top|bottom|left|right|center|cover|contain|auto|0|\d+(?:\.\d+)?(?:px|em|rem|vw|vh|%|cm|mm|in|pt|pc)))?)?)*\s*$";

        private static readonly Regex s_RgbaRegex = new Regex(@"rgba?\(\s*([+-]?\d*\.?\d+%?)\s*(?:,\s*|\s+)([+-]?\d*\.?\d+%?)\s*(?:,\s*|\s+)([+-]?\d*\.?\d+%?)\s*(?:[,/]\s*([+-]?\d*\.?\d+%?))?\s*\)", RegexOptions.Compiled);
        private static readonly Regex s_OklchRegex = new Regex(@"oklch\(([\d\.]+)\s+([\d\.]+)\s+([\d\.]+)\)", RegexOptions.Compiled);
        private static readonly Regex s_ColorMatchRegex = new Regex(ColorMatchKey, RegexOptions.Compiled);
        private static readonly Regex s_BackgroundUrlRegex = new Regex(BackgroundUrlKey, RegexOptions.Compiled);
        private static readonly Regex s_CssUrlExtractRegex = new Regex(@"url\(\s*(?:'([^']*)'|""([^""]*)""|([^)\s,]+))\s*\)", RegexOptions.Compiled);
        public static Regex BackgroundUrlRegex => s_BackgroundUrlRegex;
        /// <summary>
        /// 解析 CSS 颜色值为 Unity Color
        /// </summary>
        public static Color Parse(string cssColor, Color defaultColor = default)
        {
            cssColor = cssColor.Trim();
            if (cssColor.Contains("var("))
                cssColor = "";
            if (cssColor == "none") return Color.clear;

            if (cssColor.Contains("rgb"))
            {
                var matches = s_RgbaRegex.Match(cssColor);
                if (matches.Success)
                {
                    float a = 1;
                    int r = matches.Groups[1].Value.Contains("%")
                        ? (int)(UnitParser.Parse(matches.Groups[1].Value) * 255 / 100)
                        : (int)(UnitParser.Parse(matches.Groups[1].Value));
                    int g = matches.Groups[2].Value.Contains("%")
                        ? (int)(UnitParser.Parse(matches.Groups[2].Value) * 255 / 100)
                        : (int)(UnitParser.Parse(matches.Groups[2].Value));
                    int b = matches.Groups[3].Value.Contains("%")
                        ? (int)(UnitParser.Parse(matches.Groups[3].Value) * 255 / 100)
                        : (int)(UnitParser.Parse(matches.Groups[3].Value));
                    if (matches.Groups.Count > 4 && matches.Groups[4].Success)
                        a = matches.Groups[4].Value.Contains("%")
                            ? UnitParser.Parse(matches.Groups[4].Value) / 100f
                            : UnitParser.Parse(matches.Groups[4].Value);
                    return new Color(r / 255f, g / 255f, b / 255f, a);
                }
            }

            if ((cssColor.Contains("linear-gradient") || cssColor.Contains("radial-gradient")))
            {
                var matches = s_ColorMatchRegex.Matches(cssColor);
                if (matches?.Count > 0)
                {
                    List<Color> gradientColors = new List<Color>();
                    foreach (Match match in matches)
                    {
                        Color c = Parse(match.Value, Color.black);
                        gradientColors.Add(c);
                    }
                    return new Color(
                        gradientColors.Average(c => c.r),
                        gradientColors.Average(c => c.g),
                        gradientColors.Average(c => c.b),
                        gradientColors.Average(c => c.a));
                }
            }

            if (cssColor.Contains("oklch"))
            {
                var matches = s_OklchRegex.Match(cssColor);
                if (matches.Success)
                    return OKLCHToColor(float.Parse(matches.Groups[1].Value),
                        float.Parse(matches.Groups[2].Value), float.Parse(matches.Groups[3].Value));
            }

            var endMatch = s_ColorMatchRegex.Match(cssColor);
            if (endMatch.Success)
                cssColor = endMatch.Value;

            if (ColorUtility.TryParseHtmlString(cssColor, out Color color))
                return color;

            return defaultColor;
        }
        /// <summary>
        /// 尝试解析 CSS 颜色值并返回 Color
        /// </summary>
        /// <param name="cssColor">CSS 颜色值</param>
        /// <param name="color">解析后的 Color</param>
        /// <returns>是否解析成功</returns>
        public static bool TryParseColor(string cssColor,out Color color)
        {
            color = Parse(cssColor, -1* Color.white);
            return color != -1 * Color.white;
        }

        /// <summary>
        /// 尝试从样式中解析背景颜色
        /// </summary>
        /// <param name="styles">样式字典</param>
        /// <param name="color">解析后的背景颜色</param>
        /// <returns>是否解析成功</returns>
        public static bool TryParseBackgroundColor(Dictionary<string, string> styles, out Color color)
        {
            color = Color.white;
            bool hasBackgroundColor = false;
            foreach (var kv in styles)
            {
                if (IsBackgroundColor(kv.Key) && !s_BackgroundUrlRegex.IsMatch(kv.Value))
                {
                    hasBackgroundColor = true;
                    color = Parse(kv.Value, Color.white);
                }
            }
            return hasBackgroundColor;
        }

        /// <summary>
        /// 判断是否为背景相关属性
        /// </summary>
        /// <param name="key">样式属性名</param>
        /// <returns>是否为背景相关属性</returns>
        public static bool IsBackgroundColor(string key)
        {
            return key == "background" || key == "background-image" || key == "background-color";
        }

        /// <summary>
        /// 获取背景图片 URL 匹配的正则表达式模式
        /// </summary> <returns>背景图片 URL 匹配的正则表达式模式</returns>
        public static string BackgroundUrlPattern => BackgroundUrlKey;

        /// <summary>
        /// 尝试从样式中解析背景图片 URL
        /// </summary>
        /// <param name="styles">样式字典</param>
        /// <param name="url">解析后的背景图片 URL</param>
        /// <returns>是否解析成功</returns>
        public static bool TryParseBackgroundUrl(Dictionary<string, string> styles, out string url)
        {
            url = null;
            foreach (var kv in styles)
            {
                if (IsBackgroundColor(kv.Key) && s_BackgroundUrlRegex.IsMatch(kv.Value))
                {
                    var urls = ExtractCssUrls(kv.Value);
                    if (urls.Count > 0) { url = urls[0]; return true; }
                }
            }
            return false;
        }

        /// <summary>
        /// 从 CSS 值中提取所有 url() 中的路径
        /// </summary>
        /// <param name="css">CSS 值</param>
        /// <returns>提取的 URL 列表</returns>
        public static List<string> ExtractCssUrls(string css)
        {
            var urls = new List<string>();
            var matches = s_CssUrlExtractRegex.Matches(css);
            foreach (Match m in matches)
            {
                if (m.Groups[1].Success) urls.Add(m.Groups[1].Value);
                else if (m.Groups[2].Success) urls.Add(m.Groups[2].Value);
                else if (m.Groups[3].Success) urls.Add(m.Groups[3].Value);
            }
            return urls;
        }

        private static Color OKLCHToColor(float l, float c, float h)
        {
            float hRad = h * Mathf.Deg2Rad;
            float a = c * Mathf.Cos(hRad);
            float b = c * Mathf.Sin(hRad);

            float l_ = l + 0.3963377774f * a + 0.2158037573f * b;
            float m_ = l - 0.1055613458f * a - 0.0638541728f * b;
            float s_ = l - 0.0894841775f * a - 1.2914855480f * b;

            float lCubed = l_ * l_ * l_;
            float mCubed = m_ * m_ * m_;
            float sCubed = s_ * s_ * s_;

            float r = 4.0767416621f * lCubed - 3.3077115913f * mCubed + 0.2309699292f * sCubed;
            float g = -1.2684380046f * lCubed + 2.6097574011f * mCubed - 0.3413193965f * sCubed;
            float b2 = -0.0041960863f * lCubed - 0.7034186147f * mCubed + 1.7076147010f * sCubed;

            return new Color(
                Mathf.Clamp01(ToSRGB(r)),
                Mathf.Clamp01(ToSRGB(g)),
                Mathf.Clamp01(ToSRGB(b2)),
                1f);
        }

        private static float ToSRGB(float linear)
        {
            if (linear <= 0.0031308f)
                return 12.92f * linear;
            return 1.055f * Mathf.Pow(linear, 1f / 2.4f) - 0.055f;
        }
    }
}

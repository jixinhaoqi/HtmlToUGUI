using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Xxhq.Htmltougui
{
    /// <summary>
    /// 单位解析与转换工具
    /// </summary>
    public static class UnitParser
    {
        private static readonly Regex s_NonNumberRegex = new Regex(@"[^\d\.\-]", RegexOptions.Compiled);
        private static readonly Regex s_EmRemUnitRegex = new Regex(@"-?\d+(?:\.\d+)?(?:em|rem)", RegexOptions.Compiled);
        private static readonly Regex s_CalcUnitRegex = new Regex(@"(-?\d+(?:\.\d+)?)(px|%|em|rem)", RegexOptions.Compiled);
        private static readonly Regex s_NumericSuffixRegex = new Regex(@"\d+$", RegexOptions.Compiled);

        /// <summary>
        /// 为字符串后缀数字添加单位后缀，如果字符串已经包含该后缀，则不做修改。
        /// </summary>
        /// <param name="val">要处理的字符串</param>
        /// <param name="suffix">要添加的单位后缀</param>
        /// <returns>添加后缀后的字符串</returns>
        public static string AddUnitNumericSuffix(string val, string suffix)
        {
            if (val.EndsWith(suffix))
                return val;
            if (s_NumericSuffixRegex.Match(val).Success)
                return s_NumericSuffixRegex.Replace(val, s => suffix + s);
            else
                return val + suffix;
        }
        /// <summary>
        /// 解析 CSS 单位值并转换为浮点数。支持 px, em, rem, % 等单位。
        /// </summary>
        /// <param name="val">CSS 值字符串</param>
        /// <param name="parentSize">父元素尺寸，用于 em 和 % 的相对计算</param>
        /// <param name="rootFontSize">HTML 根字体大小，用于 rem 计算，默认 16px</param>
        public static float Parse(string val, float? parentSize = null, float rootFontSize = 16f)
        {
            val = val.Trim().ToLower();
            if (string.IsNullOrEmpty(val)) return 0f;

            string cleaned = s_NonNumberRegex.Replace(val, "");
            float number = float.TryParse(cleaned, out float n) ? n : 0f;

            if (val.StartsWith("calc(") && val.EndsWith(")"))
                return EvaluateCalcExpression(val.Substring(5, val.Length - 6).Trim(), parentSize, rootFontSize);

            if (val.EndsWith("rem")) return number * rootFontSize;
            if (val.EndsWith("em")) return parentSize.HasValue ? number * parentSize.Value : number* rootFontSize;
            if (val.EndsWith("px")) return number;
            if (val.EndsWith("%")) return parentSize.HasValue ? number / 100f * parentSize.Value : number;
            return number;
        }

        /// <summary>
        /// 转换相对单位（em/rem/%）为 px 值字符串
        /// </summary>
        /// <param name="parentStyle">父元素的样式字典</param>
        /// <param name="key">当前属性的键</param>
        /// <param name="value">当前属性的值</param>
        /// <param name="rootFontSize">HTML 根字体大小，默认 16px</param>
        /// <returns></returns>
        public static string ConvertRelativeUnits(Dictionary<string, string> parentStyle, string key, string value, float rootFontSize = 16f)
        {
            if (value.Contains("em") || value.Contains("rem"))
            {
                return s_EmRemUnitRegex.Replace(value, match =>
                {
                    if (match.Value.EndsWith("rem"))
                        return Parse(match.Value, rootFontSize: rootFontSize) + "px";
                    else if (key == "font-size")
                    {
                        if (parentStyle.ContainsKey("font-size"))
                            return Parse(match.Value, Parse(parentStyle["font-size"], 16f, rootFontSize), rootFontSize) + "px";
                        else
                            return Parse(match.Value, rootFontSize: rootFontSize) + "px";
                    }
                    else if (parentStyle.ContainsKey("width") && key == "width")
                        return Parse(match.Value, Parse(parentStyle["width"], rootFontSize: rootFontSize), rootFontSize) + "px";
                    else if (parentStyle.ContainsKey("height") && key == "height")
                        return Parse(match.Value, Parse(parentStyle["height"], rootFontSize: rootFontSize), rootFontSize) + "px";
                    else if (parentStyle.ContainsKey("padding-left") && key == "padding-left")
                        return Parse(match.Value, Parse(parentStyle["padding-left"], rootFontSize: rootFontSize), rootFontSize) + "px";
                    else if (parentStyle.ContainsKey("padding-right") && key == "padding-right")
                        return Parse(match.Value, Parse(parentStyle["padding-right"], rootFontSize: rootFontSize), rootFontSize) + "px";
                    else if (parentStyle.ContainsKey("padding-top") && key == "padding-top")
                        return Parse(match.Value, Parse(parentStyle["padding-top"], rootFontSize: rootFontSize), rootFontSize) + "px";
                    else if (parentStyle.ContainsKey("padding-bottom") && key == "padding-bottom")
                        return Parse(match.Value, Parse(parentStyle["padding-bottom"], rootFontSize: rootFontSize), rootFontSize) + "px";
                    return match.Value;
                });
            }
            else if (key == "font-size" && value.Contains("%"))
            {
                if (parentStyle.ContainsKey("font-size"))
                    return Parse(value, Parse(parentStyle["font-size"], 16f, rootFontSize), rootFontSize) + "px";
                else
                    return Parse(value, rootFontSize: rootFontSize) + "px";
            }
            return value;
        }

        private static float EvaluateCalcExpression(string expr, float? parentSize, float rootFontSize)
        {
            expr = s_CalcUnitRegex.Replace(expr, match =>
            {
                float num = float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                string unit = match.Groups[2].Value;
                switch (unit)
                {
                    case "%":   return (parentSize.HasValue ? num / 100f * parentSize.Value : num).ToString("F4", CultureInfo.InvariantCulture);
                    case "px":  return num.ToString("F4", CultureInfo.InvariantCulture);
                    case "em":  return (parentSize.HasValue ? num * parentSize.Value : num).ToString("F4", CultureInfo.InvariantCulture);
                    case "rem": return (num * rootFontSize).ToString("F4", CultureInfo.InvariantCulture);
                    default:    return num.ToString("F4", CultureInfo.InvariantCulture);
                }
            });

            return EvalArithmetic(expr);
        }

        private static float EvalArithmetic(string expr)
        {
            expr = expr.Replace(" ", "").Trim();
            if (string.IsNullOrEmpty(expr)) return 0f;

            int parenIdx;
            while ((parenIdx = expr.LastIndexOf('(')) >= 0)
            {
                int closeIdx = expr.IndexOf(')', parenIdx);
                if (closeIdx < 0) return 0f;
                float inner = EvalArithmetic(expr.Substring(parenIdx + 1, closeIdx - parenIdx - 1));
                expr = expr.Substring(0, parenIdx) + inner.ToString("R", CultureInfo.InvariantCulture) + expr.Substring(closeIdx + 1);
            }

            for (int i = expr.Length - 1; i >= 1; i--)
            {
                if (expr[i] == '+' || (expr[i] == '-' && !IsUnaryMinus(expr, i)))
                {
                    float left = EvalArithmetic(expr.Substring(0, i));
                    float right = EvalArithmetic(expr.Substring(i + 1));
                    return expr[i] == '+' ? left + right : left - right;
                }
            }

            for (int i = expr.Length - 1; i >= 1; i--)
            {
                if (expr[i] == '*' || expr[i] == '/')
                {
                    float left = EvalArithmetic(expr.Substring(0, i));
                    float right = EvalArithmetic(expr.Substring(i + 1));
                    return expr[i] == '*' ? left * right : left / right;
                }
            }

            return float.TryParse(expr.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float result) ? result : 0f;
        }

        private static bool IsUnaryMinus(string expr, int index)
        {
            if (index <= 0) return true;
            char prev = expr[index - 1];
            return prev == '+' || prev == '-' || prev == '*' || prev == '/' || prev == '(';
        }
    }
}

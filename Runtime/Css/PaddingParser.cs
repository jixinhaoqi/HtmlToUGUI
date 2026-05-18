using System.Collections.Generic;
using UnityEngine;

namespace Xxhq.Htmltougui
{
    /// <summary>
    /// Padding 解析工具
    /// </summary>
    public static class PaddingParser
    {
         /// <summary>
         /// 解析 Padding
         /// </summary>
         /// <param name="styles">样式字典</param>
         /// <param name="rectOffset">解析后的 RectOffset</param>
         /// <returns>是否成功解析</returns>
        public static bool TryParse(Dictionary<string, string> styles, out RectOffset rectOffset)
        {
            int top = 0, right = 0, bottom = 0, left = 0;
            bool hasPadding = false;

            // 先处理简写 (padding)
            foreach (var kv in styles)
            {
                if (kv.Key != "padding") continue;
                hasPadding = true;
                var p = ParseShorthand(kv.Value);
                if (p.HasValue) { top = p.Value.top; right = p.Value.right; bottom = p.Value.bottom; left = p.Value.left; }
            }

            // 再处理分项 (padding-left/top/right/bottom)，覆盖简写的对应边
            foreach (var kv in styles)
            {
                if (kv.Key == "padding") continue;
                switch (kv.Key)
                {
                    case "padding-left":   left = (int)UnitParser.Parse(kv.Value); hasPadding = true; break;
                    case "padding-right":  right = (int)UnitParser.Parse(kv.Value); hasPadding = true; break;
                    case "padding-top":    top = (int)UnitParser.Parse(kv.Value); hasPadding = true; break;
                    case "padding-bottom": bottom = (int)UnitParser.Parse(kv.Value); hasPadding = true; break;
                }
            }

            rectOffset = hasPadding ? new RectOffset(left, right, top, bottom) : null;
            return hasPadding;
        }

        private static (int top, int right, int bottom, int left)? ParseShorthand(string value)
        {
            string[] parts = value.Split(' ');
            if (parts.Length == 1)
            {
                int v = (int)UnitParser.Parse(parts[0]);
                return (v, v, v, v);
            }
            if (parts.Length == 2)
            {
                int y = (int)UnitParser.Parse(parts[0]);
                int x = (int)UnitParser.Parse(parts[1]);
                return (y, x, y, x);
            }
            if (parts.Length == 3)
            {
                int t = (int)UnitParser.Parse(parts[0]);
                int x = (int)UnitParser.Parse(parts[1]);
                int b = (int)UnitParser.Parse(parts[2]);
                return (t, x, b, x);
            }
            if (parts.Length == 4)
            {
                int t = (int)UnitParser.Parse(parts[0]);
                int r = (int)UnitParser.Parse(parts[1]);
                int b = (int)UnitParser.Parse(parts[2]);
                int l = (int)UnitParser.Parse(parts[3]);
                return (t, r, b, l);
            }
            return null;
        }
    }
}

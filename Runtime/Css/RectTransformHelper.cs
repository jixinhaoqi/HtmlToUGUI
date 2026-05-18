using System.Globalization;
using UnityEngine;

namespace Xxhq.Htmltougui
{
    /// <summary>
    /// RectTransform 辅助工具
    /// </summary>
    public static class RectTransformHelper
    {
        /// <summary>
        /// 设置 RectTransform 完全填充父容器
        /// </summary>
        /// <param name="rt">要设置的 RectTransform</param>
        public static void FullyFillParent(RectTransform rt)
        {
            if(!rt) return;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// 从字符串解析 Quaternion
        /// </summary>
        /// <param name="str">字符串表示的 Quaternion</param>
        /// <returns>解析后的 Quaternion</returns>
        public static Quaternion GetRotationFromString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return Quaternion.identity;

            str = str.Trim('(', ')');
            string[] strs = str.Split(',');
            if (strs.Length != 4)
            {
                throw new UnityException("Quaternion parse error : " + str);
            }
            Quaternion v = new Quaternion();
            v.x = float.Parse(strs[0], CultureInfo.InvariantCulture);
            v.y = float.Parse(strs[1], CultureInfo.InvariantCulture);
            v.z = float.Parse(strs[2], CultureInfo.InvariantCulture);
            v.w = float.Parse(strs[3], CultureInfo.InvariantCulture);
            return v;
        }

        /// <summary>
        /// 从字符串解析 Vector3
        /// </summary>
        /// <param name="str">字符串表示的 Vector3</param>
        /// <returns>解析后的 Vector3</returns>
        public static Vector3 GetVector3FromString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return Vector3.zero;

            str = str.Trim('(', ')');
            string[] strs = str.Split(',');
            if (strs.Length != 3)
            {
                throw new UnityException("Vector3 parse error : " + str);
            }
            Vector3 v = new Vector3();
            v.x = float.Parse(strs[0], CultureInfo.InvariantCulture);
            v.y = float.Parse(strs[1], CultureInfo.InvariantCulture);
            v.z = float.Parse(strs[2], CultureInfo.InvariantCulture);
            return v;
        }

        /// <summary>
        /// 从字符串解析 Vector2
        /// </summary>
        /// <param name="str">字符串表示的 Vector2</param>
        /// <returns>解析后的 Vector2</returns>
        public static Vector2 GetVector2FromString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return Vector2.zero;

            str = str.Trim('(', ')');
            string[] strs = str.Split(',');
            if (strs.Length != 2)
            {
                throw new UnityException("Vector2 parse error : " + str);
            }
            Vector2 v = new Vector2();
            v.x = float.Parse(strs[0], CultureInfo.InvariantCulture);
            v.y = float.Parse(strs[1], CultureInfo.InvariantCulture);
            return v;
        }
    }
}

using UnityEngine;

namespace Xxhq.Htmltougui
{
    /// <summary>
    /// 拉伸布局计算器
    /// </summary>
    [Locale(SystemLanguage.ChineseSimplified, "拉伸布局计算器")]
    [Locale(SystemLanguage.English, "Stretch Layout Calculator")]
    public class StretchLayoutCalculator : LayoutCalculator
    {
        /// <inheritdoc/>
        protected override void ApplyAbsoluteLayoutStrategy(
            float relativeX, float relativeY, float selfWidth, float selfHeight,
            float parentWidth, float parentHeight,
            ref Vector2 anchorMin, ref Vector2 anchorMax, ref Vector2 anchoredPosition,
            out float? offMinX, out float? offMinY, out float? offMaxX, out float? offMaxY)
        {
            float minX = relativeX / parentWidth;
            float minY = 1 - (relativeY + selfHeight) / parentHeight;
            float maxX = (relativeX + selfWidth) / parentWidth;
            float maxY = 1 - relativeY / parentHeight;
            anchorMin.x = Mathf.Clamp01(minX);
            anchorMin.y = Mathf.Clamp01(minY);
            anchorMax.x = Mathf.Clamp01(maxX);
            anchorMax.y = Mathf.Clamp01(maxY);
            offMinX = offMinY = offMaxX = offMaxY = 0f;
        }
    }
}

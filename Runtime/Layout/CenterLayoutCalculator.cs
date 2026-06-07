using UnityEngine;

namespace Xxhq.Htmltougui
{
    /// <summary>
    /// 居中布局计算器。
    /// </summary>
    [Locale(SystemLanguage.ChineseSimplified, "居中布局计算器")]
    [Locale(SystemLanguage.English, "Center Layout Calculator")]
    public class CenterLayoutCalculator : LayoutCalculator
    {
        /// <inheritdoc/>
        public override void ApplyAbsoluteLayoutStrategy(
            float relativeX, float relativeY, float selfWidth, float selfHeight,
            float parentWidth, float parentHeight,
            ref Vector2 anchorMin, ref Vector2 anchorMax, ref Vector2 anchoredPosition,
            out float? offMinX, out float? offMinY, out float? offMaxX, out float? offMaxY)
        {
            anchorMin = anchorMax = Vector2.one * 0.5f;
            anchoredPosition.x = -parentWidth / 2 + anchoredPosition.x;
            anchoredPosition.y = parentHeight / 2 + anchoredPosition.y;
            offMinX = offMinY = offMaxX = offMaxY = null;
        }
    }
}

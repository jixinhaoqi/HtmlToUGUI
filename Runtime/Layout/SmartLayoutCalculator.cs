using UnityEngine;

namespace Xxhq.Htmltougui
{
    /// <summary>
    /// 智能布局计算器：根据元素在父容器中的位置和尺寸，
    /// 自动选择最合适的锚点/拉伸/居中策略。
    /// </summary>
    [Locale(SystemLanguage.ChineseSimplified, "智能布局计算器")]
    [Locale(SystemLanguage.English, "Smart Layout Calculator")]
    public class SmartLayoutCalculator : LayoutCalculator
    {
        /// <summary> 中心拉伸百分比阈值，当元素尺寸占比超过该值时使用拉伸锚点 </summary>
        public float StretchPercentThreshold { get; set; } = 0.5f;
        /// <summary> 边界对齐百分比阈值，用于判断元素是否贴边 </summary>
        public float BoundAlignPercentThreshold { get; set; } = 0.1f;
        /// <summary> 轴对齐百分比阈值范围 (min, max)，用于判断元素是否接近父容器边界 </summary>
        public Vector2 AxisAlignPercentThresholdRange { get; set; } = new Vector2(0.5f, 0.75f);

        public override void ApplyAbsoluteLayoutStrategy(
            float relativeX, float relativeY, float selfWidth, float selfHeight,
            float parentWidth, float parentHeight,
            ref Vector2 anchorMin, ref Vector2 anchorMax, ref Vector2 anchoredPosition,
            out float? offMinX, out float? offMinY, out float? offMaxX, out float? offMaxY)
        {
            ApplySmartAxis(relativeX, selfWidth, parentWidth, false,
                ref anchorMin.x, ref anchorMax.x, ref anchoredPosition.x,
                out offMinX, out offMaxX);

            ApplySmartAxis(relativeY, selfHeight, parentHeight, true,
                ref anchorMin.y, ref anchorMax.y, ref anchoredPosition.y,
                out offMinY, out offMaxY);
        }

        protected void ApplySmartAxis(
            float relativePos, float selfSize, float parentSize, bool isYAxis,
            ref float anchorMin, ref float anchorMax, ref float anchoredPosition,
            out float? offMin, out float? offMax)
        {
            offMin = offMax = null;

            float halfParent = parentSize / 2f;
            float centerSign = isYAxis ? 1f : -1f;
            float farEdgeAnchor = isYAxis ? 0f : 1f;
            float farEdgePosSign = isYAxis ? 1f : -1f;
            float farEdge = parentSize - relativePos - selfSize;

            if (Mathf.RoundToInt(relativePos + selfSize / 2f) == Mathf.RoundToInt(halfParent))
            {
                if (selfSize / parentSize >= StretchPercentThreshold)
                {
                    anchorMin = 0f; anchorMax = 1f;
                    if (isYAxis)
                    { offMin = farEdge; offMax = -relativePos; }
                    else
                    { offMin = relativePos; offMax = -farEdge; }
                }
                else
                {
                    anchorMin = anchorMax = 0.5f;
                    anchoredPosition = centerSign * halfParent + anchoredPosition;
                }
            }
            else
            {
                bool isBigger = selfSize >= parentSize * AxisAlignPercentThresholdRange.x
                    && selfSize < parentSize * AxisAlignPercentThresholdRange.y;
                bool isNearBound = relativePos <= parentSize * BoundAlignPercentThreshold && isBigger;
                bool isFarBound = relativePos >= (parentSize * (1 - BoundAlignPercentThreshold)) && isBigger;

                if (relativePos + selfSize <= halfParent || isNearBound)
                {
                }
                else if (relativePos >= halfParent || isFarBound)
                {
                    anchorMin = anchorMax = farEdgeAnchor;
                    anchoredPosition = farEdgePosSign * parentSize + anchoredPosition;
                }
                else
                {
                    if (selfSize >= parentSize * AxisAlignPercentThresholdRange.x)
                    {
                        anchorMin = 0f; anchorMax = 1f;
                        if (isYAxis)
                        { offMin = farEdge; offMax = -relativePos; }
                        else
                        { offMin = relativePos; offMax = -farEdge; }
                    }
                    else
                    {
                        anchorMin = anchorMax = 0.5f;
                        anchoredPosition = centerSign * halfParent + anchoredPosition;
                    }
                }
            }
        }
    }
}

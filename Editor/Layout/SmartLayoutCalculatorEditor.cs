using UnityEditor;
using UnityEngine;
namespace Xxhq.Htmltougui.Editor
{
    public class SmartLayoutCalculatorEditor : SmartLayoutCalculator
    {
        private const string PREFS_STRETCH_PERCENT_THRESHOLD_KEY = "HtmlToUguiConverter_StretchPercentThreshold";
        private const string PREFS_BOUND_ALIGN_PERCENT_THRESHOLD_KEY = "HtmlToUguiConverter_BoundAlignPercentThreshold";
        private const string PREFS_AXIS_ALIGN_PERCENT_THRESHOLD_RANGE_KEY = "HtmlToUguiConverter_AxisAlignPercentThresholdRange";

        public override void OnEnable()
        {
            StretchPercentThreshold = EditorPrefs.GetFloat(PREFS_STRETCH_PERCENT_THRESHOLD_KEY, StretchPercentThreshold);
            var rangeStr = EditorPrefs.GetString(PREFS_AXIS_ALIGN_PERCENT_THRESHOLD_RANGE_KEY, "");
            if (!string.IsNullOrEmpty(rangeStr))
                AxisAlignPercentThresholdRange = RectTransformHelper.GetVector2FromString(rangeStr);
            BoundAlignPercentThreshold = EditorPrefs.GetFloat(PREFS_BOUND_ALIGN_PERCENT_THRESHOLD_KEY, BoundAlignPercentThreshold);
        }

        public override void OnGUI()
        {
            StretchPercentThreshold = EditorGUILayout.Slider("      拉伸百分比阈值", StretchPercentThreshold, 0f, 1f);
            float min = AxisAlignPercentThresholdRange.x;
            float max = AxisAlignPercentThresholdRange.y;
            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.MinMaxSlider("      轴对齐百分比阈值范围", ref min, ref max, 0.5f, 1f);
                if (EditorGUI.EndChangeCheck())
                {
                    AxisAlignPercentThresholdRange = new Vector2(min, max);
                }
            }
        }

        public override void OnDisable()
        {
            EditorPrefs.SetFloat(PREFS_STRETCH_PERCENT_THRESHOLD_KEY, StretchPercentThreshold);
            EditorPrefs.SetString(PREFS_AXIS_ALIGN_PERCENT_THRESHOLD_RANGE_KEY, AxisAlignPercentThresholdRange.ToString());
            EditorPrefs.SetFloat(PREFS_BOUND_ALIGN_PERCENT_THRESHOLD_KEY, BoundAlignPercentThreshold);
        }
    }
}

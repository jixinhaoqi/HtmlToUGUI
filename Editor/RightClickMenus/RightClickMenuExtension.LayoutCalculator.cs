using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace Xxhq.Htmltougui.Editor
{
    public partial class HierarchyPanelRightClickExtension
    {
        const string ApplySmartLayoutMenuPath = "GameObject/Html To UGUI/应用UI智能布局";
        const string ApplyCenterLayoutMenuPath="GameObject/Html To UGUI/应用UI居中布局";
        const string ApplyStretchLayoutMenuPath="GameObject/Html To UGUI/应用UI全拉伸布局";
        const int priority = 10;

        /// <summary>
        /// 应用智能布局到选中的UI元素。
        /// </summary>
        [MenuItem(ApplySmartLayoutMenuPath, priority = priority)]
        static void ApplySmartLayoutMenu()
        {
            LayoutCalculator calculator = new SmartLayoutCalculatorEditor();
            calculator.OnEnable();
            ApplyLayoutAllItem(calculator);
        }

        [MenuItem(ApplySmartLayoutMenuPath, true, priority = priority)]
        static bool ApplySmartLayoutMenu_Validate()
        {
            return ApplyLayoutMenu_Validate();
        }

        /// <summary>
        /// 应用居中布局到选中的UI元素。
        /// </summary>

        [MenuItem(ApplyCenterLayoutMenuPath, priority = priority + 1)]
        static void ApplyCenterLayoutMenu()
        {
            LayoutCalculator calculator = new CenterLayoutCalculator();
            calculator.OnEnable();
            ApplyLayoutAllItem(calculator);
        }

        [MenuItem(ApplyCenterLayoutMenuPath, true, priority = priority + 1)]
        static bool ApplyCenterLayoutMenu_Validate()
        {
            return ApplyLayoutMenu_Validate();
        }

        /// <summary>
        /// 应用全拉伸布局到选中的UI元素。
        /// </summary>
        [MenuItem(ApplyStretchLayoutMenuPath, priority = priority + 2)]
        static void ApplyStretchLayoutMenu()
        {
            LayoutCalculator calculator = new StretchLayoutCalculator();
            calculator.OnEnable();
            ApplyLayoutAllItem(calculator);
        }

        [MenuItem(ApplyStretchLayoutMenuPath, true, priority = priority + 2)]
        static bool ApplyStretchLayoutMenu_Validate()
        {
            return ApplyLayoutMenu_Validate();
        }

        static bool ApplyLayoutMenu_Validate()
        {
            var objs = Selection.objects;
            if (objs == null || objs.Length == 0)
                return false;
            foreach (var item in objs)
            {
                GameObject gameObject= item as GameObject;
                if (gameObject==null || !gameObject.TryGetComponent<RectTransform>(out var rectTransform) || (!rectTransform.GetComponent<Canvas>() && rectTransform.parent == null))
                    return false;
            }
            return true;
        }

        static void ApplyLayoutAllItem(LayoutCalculator calculator)
        {
            var objs = Selection.objects;
            foreach (var item in objs)
            {
                if (item is GameObject gameObject && gameObject.TryGetComponent<RectTransform>(out var rectTransform))
                {
                    if (rectTransform.GetComponent<Canvas>())
                    {
                        foreach (RectTransform child in rectTransform)
                        {
                            ApplyLayoutItem(child, calculator);
                        }
                    }
                    else if (rectTransform.parent != null)
                        ApplyLayoutItem(rectTransform, calculator);
                }
            }
        }

        static void ApplyLayoutItem(RectTransform rt, LayoutCalculator calculator)
        {
            if(IsNotLayoutItem(rt)) return;
            Vector2 anchorMin = new Vector2(0, 1);
            Vector2 anchorMax = new Vector2(0, 1);
            Vector2 pivot = Vector2.one * 0.5f;
            Vector2 anchoredPosition = Vector2.zero;

            float? offsetMinX = null, offsetMinY = null, offsetMaxX = null, offsetY = null;

            float parentWidth = rt.parent.GetComponent<RectTransform>().rect.width;
            float parentHeight = rt.parent.GetComponent<RectTransform>().rect.height;

            float selfWidth = rt.rect.width;
            float selfHeight = rt.rect.height;

            Vector2 vector2= GetTopLeftRelativeToParent(rt);
            float relativeX = vector2.x;
            float relativeY = vector2.y;

            anchoredPosition.x = relativeX + selfWidth / 2f;
            anchoredPosition.y = -relativeY - selfHeight / 2f;

            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, selfWidth);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, selfHeight);

            calculator.ApplyAbsoluteLayoutStrategy(relativeX, relativeY, selfWidth, selfHeight, parentWidth, parentHeight,
                ref anchorMin, ref anchorMax, ref anchoredPosition,
                out offsetMinX, out offsetMinY, out offsetMaxX, out offsetY);

            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPosition;
            rt.offsetMin = new Vector2(offsetMinX ?? rt.offsetMin.x, offsetMinY ?? rt.offsetMin.y);
            rt.offsetMax = new Vector2(offsetMaxX ?? rt.offsetMax.x, offsetY ?? rt.offsetMax.y);
            rt = GetSkipLayoutItemIfHas(rt);
            foreach (RectTransform child in rt)
            {
                ApplyLayoutItem(child, calculator);
            }
        }

         static Vector2 GetTopLeftRelativeToParent(RectTransform rect)
        {
            // 获取四个角的**世界坐标**（顺序：左下、左上、右上、右下）
            Vector3[] childCorners = new Vector3[4];
            Vector3[] parentCorners = new Vector3[4];
            rect.GetWorldCorners(childCorners);
            rect.parent.GetComponent<RectTransform>().GetWorldCorners(parentCorners);

            // 子物体左上角（世界坐标）：x 最小，y 最大
            float childLeft = childCorners[0].x;   // 左下角 x
            float childTop = childCorners[1].y;   // 左上角 y

            // 父物体左上角（世界坐标）
            float parentLeft = parentCorners[0].x;
            float parentTop = parentCorners[1].y;

            // 转换为相对于父左上角的偏移（向下为正）
            float left = childLeft - parentLeft;
            float top = parentTop - childTop;

            return new Vector2(left, top);
        }

        static bool IsNotLayoutItem(RectTransform rt)
        {
            if (!rt.gameObject.activeInHierarchy) return true;
            if (rt.GetComponent<TMP_SubMeshUI>()) return true;
            if(rt.localEulerAngles!= Vector3.zero) return true;
            if (rt.localScale != Vector3.one) return true;
            return false;
        }

        static RectTransform GetSkipLayoutItemIfHas(RectTransform rt)
        {
            if (rt.GetComponent<ScrollRect>() is ScrollRect scrollRect&& scrollRect.content)
                return scrollRect.content;
            return rt;
        }
    } 
}

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Xxhq.Htmltougui.Editor
{
    /// <summary>
    /// Game View 分辨率辅助工具
    /// </summary>
    public static class GameViewResolutionHelper
    {
        private static readonly Type s_GameViewType;
        private static readonly PropertyInfo s_CurrentGameViewSizeProp;
        private static readonly PropertyInfo s_WidthProp;
        private static readonly PropertyInfo s_HeightProp;

        static GameViewResolutionHelper()
        {
            try
            {
                s_GameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
                if (s_GameViewType == null) return;

                s_CurrentGameViewSizeProp = s_GameViewType.GetProperty("currentGameViewSize",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (s_CurrentGameViewSizeProp == null) return;

                var gameView = EditorWindow.GetWindow(s_GameViewType);
                var gameViewSize = s_CurrentGameViewSizeProp.GetValue(gameView);
                if (gameViewSize == null) return;

                var sizeType = gameViewSize.GetType();
                s_WidthProp = sizeType.GetProperty("width", BindingFlags.Public | BindingFlags.Instance);
                s_HeightProp = sizeType.GetProperty("height", BindingFlags.Public | BindingFlags.Instance);
            }
            catch
            {
                // Silently fall back to Screen resolution.
            }
        }
        /// <summary>
        /// 获取当前 Game View 分辨率大小
        /// </summary>
        /// <param name="size">输出参数，返回当前 Game View 的分辨率大小</param>
        /// <returns>如果成功获取分辨率，返回 true；否则返回 false</returns>
        public static bool GetSelectedGameViewSize(out Vector2 size)
        {
            size = Vector2.zero;
            if (s_GameViewType == null || s_CurrentGameViewSizeProp == null || s_WidthProp == null || s_HeightProp == null)
            {
                size = new Vector2(Screen.width, Screen.height);
                return true;
            }

            try
            {
                var gameView = EditorWindow.GetWindow(s_GameViewType);
                var gameViewSize = s_CurrentGameViewSizeProp.GetValue(gameView);
                if (gameViewSize == null) return false;

                size.x = (int)s_WidthProp.GetValue(gameViewSize);
                size.y = (int)s_HeightProp.GetValue(gameViewSize);
                return true;
            }
            catch
            {
                size = new Vector2(Screen.width, Screen.height);
                return true;
            }
        }
    }
}

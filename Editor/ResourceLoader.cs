using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Xxhq.Htmltougui.Editor
{
    /// <summary>
    /// 资源加载器：负责从本地路径加载 Sprite 和 Texture
    /// </summary>
    public static class ResourceLoader
    {
        /// <summary>
        /// 从多图集加载 Sprite
        /// </summary>
        /// <param name="go">要加载图片的游戏对象</param>
        /// <param name="src">图片资源路径</param>
        /// <param name="htmlFilePath">HTML 文件路径</param>
        /// <returns>是否成功加载图片</returns>
        public static bool LoadImage(GameObject go, string src, string htmlFilePath)
        {
            if(Path.IsPathRooted(src) || src.StartsWith("http"))
                return false;
            if (string.IsNullOrEmpty(src)) return false;
            if (!string.IsNullOrWhiteSpace(htmlFilePath))
            {
                string basePath = Path.GetDirectoryName(htmlFilePath);
                src = Path.Combine(basePath, src);
            }
            src = GetRelativeAssetsOrPackagesPath(src);

            if (!AssetDatabase.IsValidFolder(Path.GetDirectoryName(src)))
            {
                Debug.LogWarning($"本地未找到图片资源: {src}");
                return false;
            }

            string atlasPath = Path.GetDirectoryName(src) + ".png";
            string atlasName = Path.GetFileNameWithoutExtension(src);
            TextureImporter texImporter = null;

            if (File.Exists(atlasPath))
                texImporter = AssetImporter.GetAtPath(atlasPath) as TextureImporter;
            else
                texImporter = AssetImporter.GetAtPath(src) as TextureImporter;

            if (texImporter == null || texImporter.textureType != TextureImporterType.Sprite) return false;

            if (go.GetComponent<Image>() is Image img)
            {
                Sprite sprite = texImporter.spriteImportMode == SpriteImportMode.Multiple
                    ? LoadSpriteFromMultiple(atlasPath, atlasName)
                    : AssetDatabase.LoadAssetAtPath<Sprite>(src);
                if (!sprite) return false;
                img.sprite = sprite;
            }
            else if (go.GetComponent<RawImage>() is RawImage rawImg)
            {
                Texture2D texture = texImporter.spriteImportMode == SpriteImportMode.Multiple
                    ? LoadSpriteFromMultiple(atlasPath, atlasName)?.texture
                    : AssetDatabase.LoadAssetAtPath<Texture2D>(src);
                if (!texture) return false;
                rawImg.texture = texture;
            }

            return true;
        }
        /// <summary>
        /// 获取相对于 Assets 或 Packages 的路径。如果找不到，则返回正则化后的字符串。
        /// </summary>
        /// <param name="fullPath">完整路径</param>
        /// <returns>相对于 Assets 或 Packages 的路径，如果找不到，则返回正则化后的字符串</returns>
        public static string GetRelativeAssetsOrPackagesPath(string fullPath)
        {
            fullPath= GetRegularPath(fullPath);
            if (fullPath.Contains("/Assets/")) fullPath = fullPath.Substring(fullPath.IndexOf("/Assets/") + 1);
            else if (fullPath.Contains("/Packages/")) fullPath = fullPath.Substring(fullPath.IndexOf("/Packages/") + 1);
            return fullPath;
        }
        /// <summary>
        /// 从多图集纹理中加载 Sprite。如果找不到，则返回 null。
        /// </summary>
        /// <param name="texturePath">多图集纹理的路径</param>
        /// <param name="spriteName">要加载的 Sprite 名称</param>
        /// <returns>如果找到，返回对应的 Sprite；否则返回 null</returns>
        public static Sprite LoadSpriteFromMultiple(string texturePath, string spriteName)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
            Sprite targetSprite = assets?.OfType<Sprite>().FirstOrDefault(s => s.name == spriteName);
            if (targetSprite == null)
                Debug.LogWarning($"在 {texturePath} 中未找到名为 {spriteName} 的 Sprite。");
            return targetSprite;
        }

        /// <summary>
        /// 获取组合路径，并将其转换为正则化后的字符串。
        /// </summary>
        /// <param name="args">多路径</param>
        /// <returns>正则化后的字符串</returns>
        public static string GetCombinePath(params string[] args)
        {
            return GetRegularPath(Path.Combine(args));
        }
        /// <summary>
        /// 获取正则化后的字符串。
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>正则化后的字符串</returns>
        public static string GetRegularPath(string path)
        {
            return path.Replace("\\", "/");
        }
        
    }
}

# HTML to UGUI 转换器

[(English)](README_EN.md)

[![Unity](https://img.shields.io/badge/Unity-2019.4%2B-black?logo=unity)](https://unity.com/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE.md)
![Version](https://img.shields.io/badge/version-0.3.0-blue)

在 Unity 编辑器中，将 HTML + CSS 内容转换为 UGUI（Canvas）层级结构。

![简介HTML转换示例](Documentation/gifs/简介HTML转换示例.gif)

## 核心功能

- **CSS 解析** — 支持 `<style>` 标签、行内样式、复合选择器（类/ID/属性/后代/子代）、伪类（`:hover`/`:active`/`:disabled`）、CSS 变量（`var()`）、相对单位（`em`/`rem`/`%`/`calc()`）
- **三种布局计算器** — **智能**（自动选择锚点/拉伸/居中）、**全拉伸**（百分比填满）、**居中**（居中轴心）
- **可插拔标签处理器** — 内置支持 `div`/`span`/`p`/`h1~h6`/`button`/`input`/`select`/`img`/`textarea`/`progress`/`meter`，可通过 `ITagHandler` 扩展
- **文件监视** — HTML 文件变更后自动重新导入并转换
- **预制体模板** — `UiPrefabSettings` ScriptableObject 可配置各组件对应的预制体

## 图片解析

工具将 `<img src="...">` 引用解析到 HTML 文件所在目录下的本地资源：
- `src="icon.png"` → 在同目录查找 `icon.png`，或者同目录下 `.spriteatlasv2` 图集/多模式碎图中匹配名称的切图
- 支持单模式 Sprite、多模式碎图（按名称自动匹配）及 `.spriteatlasv2` 图集
- 图片资源必须位于 `Assets/` 或 `Packages/` 目录下

## SpriteAtlas 工具

选中 **SpriteAtlas** 或 **Sprite(Multiple)** 资源，通过 **Assets → Html To UGUI → 2D** 菜单访问：

| 菜单项 | 说明 |
|---|---|
| SpriteAtlas → TMP_SpriteAsset | 将 SpriteAtlas 导出为 TextMeshPro SpriteAsset（`.asset`） |
| SpriteAtlas → Sprite(Multiple) | 将 SpriteAtlas 导出为 Multiple 模式的 Sprite Sheet |
| SpriteAtlas → TextureSheet | 将 SpriteAtlas 中的精灵按网格排布为单张纹理 |
| Sprite(Multiple) → Sprites | 将 Multiple 模式的精灵图集拆分为单个精灵碎图 |

> 需要在 **Project Settings → SpriteAtlas** 中启用 SpriteAtlas 功能。

## 快速开始

### 通过 Git URL安装（推荐）

1. 打开 Unity 的 **Package Manager**（Window > Package Manager）
2. 点击左上角 **+** 按钮，选择 **"Add package from git URL..."**
3. 输入以下 URL：
   ```
   https://github.com/jixinhaoqi/HtmlToUGUI.git
   ```
4. 点击 **Add**，等待导入完成

### 使用转换工具

1. 打开 **Tools → HTML to UGUI Converter**
2. 粘贴经过预处理的 HTML 内容，或选择 `.html` 文件
3. 点击 **开始转换**，在 Canvas 下生成 UGUI 层级

> **提示 — AI 生成 HTML：** 如果不借助「HTML解构工具」预处理，可在生成 HTML 时附加 [AI生成HTML提示词](Tools/HTMLTools/AI生成HTML提示词/) ：
> - **SKILL_动态定位** — 推荐，输出效果更好
> - **SKILL_绝对定位** — 若 AI 足够听话，可直接生成绝对定位版本

> **效果对比：**

<table>
  <tr>
    <td><img src="Documentation/images/SKILL_动态定位-原网页.png" alt="SKILL_动态定位 - 原网页"></td>
    <td>→</td>
    <td><img src="Documentation/images/SKILL_动态定位-转换后.png" alt="SKILL_动态定位 - 转换后"></td>
  </tr>
</table>

<table>
  <tr>
    <td><img src="Documentation/images/SKILL_绝对定位-原网页.png" alt="SKILL_绝对定位 - 原网页"></td>
    <td>→</td>
    <td><img src="Documentation/images/SKILL_绝对定位-转换后.png" alt="SKILL_绝对定位 - 转换后"></td>
  </tr>
</table>

## HTML 解构工具

[HTML解构工具使用教程](Tools/HTMLTools/HTML解构工具使用教程.md) — 了解 HTML 解构工具的三种使用方式、AI 提示词用法和 CORS 代理配置。
[在线运行工具](https://jixinhaoqi.github.io/HtmlToUGUI/)

## 示例

- [Example](Samples~/Example/)：简单的 HTML 转 UGUI 演示场景，包含完整的 `index_optimized.html`、精灵图集和 Canvas 输出。

## 依赖

| 包名 | 用途 |
|---|---|
| `com.unity.textmeshpro` | 文本渲染（TextMeshPro） |
| `com.unity.2d.sprite` | SpriteAtlas 工具 |
| HtmlAgilityPack | HTML 解析和 DOM 遍历 |

---

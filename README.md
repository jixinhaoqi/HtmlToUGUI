# HTML to UGUI Converter

[(English)](README_EN.md)

[![Unity](https://img.shields.io/badge/Unity-2019.4%2B-black?logo=unity)](https://unity.com/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE.md)
![Version](https://img.shields.io/badge/version-0.1.0-blue)

# HTML to UGUI 转换器

在 Unity 编辑器中，将 HTML + CSS 内容转换为 UGUI（Canvas）层级结构。

## 核心功能

- **CSS 解析** — 支持 `<style>` 标签、行内样式、复合选择器（类/ID/属性/后代/子代）、伪类（`:hover`/`:active`/`:disabled`）、CSS 变量（`var()`）、相对单位（`em`/`rem`/`%`/`calc()`）
- **三种布局计算器** — **智能**（自动选择锚点/拉伸/居中）、**全拉伸**（百分比填满）、**居中**（居中轴心）
- **可插拔标签处理器** — 内置支持 `div`/`span`/`p`/`h1~h6`/`button`/`input`/`select`/`img`/`textarea`，可通过 `ITagHandler` 扩展
- **文件监视** — HTML 文件变更后自动重新导入并转换
- **预制体模板** — `UiPrefabSettings` ScriptableObject 可配置各组件对应的预制体

## 图片解析

工具将 `<img src="...">` 引用解析到 HTML 文件所在目录下的本地资源：
- `src="icon.png"` → 在同目录查找 `icon.png`，或者同目录下 `.spriteatlasv2` 图集/多模式碎图中匹配名称的切图
- 支持单模式 Sprite、多模式碎图（按名称自动匹配）及 `.spriteatlasv2` 图集
- 图片资源必须位于 `Assets/` 或 `Packages/` 目录下

## SpriteAtlas 工具

选中 **SpriteAtlas** 或 **Sprite(Multiple)** 资源，通过 **Assets → HTMLToUGUI → 2D** 菜单访问：

| 菜单项 | 说明 |
|---|---|
| SpriteAtlas → TMP_SpriteAsset | 将 SpriteAtlas 导出为 TextMeshPro SpriteAsset（`.asset`） |
| SpriteAtlas → Sprite(Multiple) | 将 SpriteAtlas 导出为 Multiple 模式的 Sprite Sheet |
| SpriteAtlas → TextureSheet | 将 SpriteAtlas 中的精灵按网格排布为单张纹理 |
| Sprite(Multiple) → Sprites | 将 Multiple 模式的精灵图集拆分为单个精灵碎图 |

> 需要在 **Project Settings → SpriteAtlas** 中启用 SpriteAtlas 功能。

## 快速开始

### 通过 Unity Package Manager 安装

1. 复制 `https://github.com/jixinhaoqi/HtmlToUGUI.git`
2. 打开 Unity，进入 **Window → Package Manager**
3. 点击左上角 **+**，选择 **Add package from git URL...**
4. 粘贴链接，点击 **Add**
5. 等待下载导入完成，编译完成后即可使用

### 通过转换工具使用

1. 打开 **Tools → HTML to UGUI Converter**
2. 粘贴经过预处理的 HTML 内容，或选择 `.html` 文件
3. 点击 **开始转换**，在 Canvas 下生成 UGUI 层级

> **提示 — AI 生成 HTML：** 如果不借助「HTML解构工具」预处理，可在生成 HTML 时附加 [AI生成HTML提示词](Tools/HTMLTools/AI生成HTML提示词/) ：
> - **SKILL_动态定位** — 推荐，输出效果更好
> - **SKILL_绝对定位** — 若 AI 足够听话，可直接生成绝对定位版本


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

## 竞品对比

本工具是一种轻量级的专用方案，将 AI 生成或手写的 HTML 内容转换为原生 UGUI 层级 — 非常适合快速 UI 原型开发。

| 方案 | 优点 | 缺点 |
|---|---|---|
| **HtmlToUGUI** | 完整 CSS 引擎（选择器/伪类/变量/级联）；直接输出 UGUI，运行时零开销；可插拔标签处理器；三种布局计算器；SpriteAtlas 转换工具 | 仅编辑期（无运行时 HTML 渲染）；HTML 输入需经捆绑工具预处理；限于绝对/相对定位 |
| [UI Toolkit](https://docs.unity3d.com/Manual/UIElements.html)（Unity 官方） | 深度 Unity 集成；USS 支持类 CSS 样式；运行时和编辑器双模式；构建支持 | 输出到自有渲染器（非 UGUI）；USS 是 CSS 子集 — 无 `var()`、伪类有限、布局模型不同；已有 UGUI 项目迁移成本高 |
| [Vuplex 3D WebView](https://developer.vuplex.com/) | 完整浏览器级 HTML/CSS/JS；可在 3D/UI 中实时渲染网页；跨平台 | 运行时重依赖（内嵌 Chromium）；输出为纹理，非交互式 UGUI；内存/CPU 成本高；需付费授权 |
| [UniWebView](https://uniwebview.com/) | 移动端原生 WebView 叠加；完整 HTML/CSS/JS；维护良好 | 仅移动端（iOS/Android）；浏览器引擎开销；以叠加层渲染，未集成到 UGUI 层级；需付费授权 |

**HtmlToUGUI 的核心优势**：输出为标准 UGUI 层级，无缝兼容 Unity 输入系统、预制体、射线检测和导航 — 无额外运行时依赖，无构建体积膨胀。

> **提示 — 设计稿转 UGUI 工作流：**
> 主流设计工具的文件可通过 HTML 中间步骤转换为 UGUI：
>
> | 工具 | HTML 导出方式 |
> |---|---|
> | **Figma** | 插件如 [Figma to HTML](https://www.figma.com/community/plugin/)、[Anima](https://www.animaapp.com/)，或内置 Dev Mode → CSS/HTML |
> | **Sketch** | [Anima](https://www.animaapp.com/)、Sketch2React，或手动导出 HTML |
> | **Adobe XD** | 插件如 Web Export，或 [Export Kit](https://exportkit.com/) |
> | **Photoshop** | 内置 **文件 → 导出 → HTML**，或 [psd2code](https://github.com/miaowmiaow/psd2code/) 等工具 |
> | **AI 生成** | 大语言模型直接输出 HTML，即可投入转换 |
>
> 从以上任一方式获取 HTML 后，通过本工具的转换管线处理，即可生成原生 UGUI 层级。
---

> [与 Unity MCP/CLI 对比](Compare_MCP_CLI.md) — 了解 HtmlToUGUI 与当前流行的 MCP/CLI AI 工具在编辑器内直接生成 UGUI 的详细对比。

# HTML 解构工具使用教程

## 概述

HTML 解构工具 (`HTML解构工具.html`) 是一个离线网页工具，用于将普通的 HTML 代码转换为带有 `data-u-left`、`data-u-top`、`data-u-width`、`data-u-height` 属性的 HTML 结构。转换后的输出可直接被 HtmlToUGUI 工具转换为 UGUI 层级。

## 使用方式

### 方式一：直接粘贴 HTML

1. 在浏览器中打开 `Tools/HTMLTools/HTML解构工具.html`
2. 在左侧输入区粘贴 HTML 代码
3. 点击 **"解构 HTML"** 按钮
4. 右侧输出区生成携带 `data-u-*` 属性的 HTML
5. 将输出内容复制到 HtmlToUGUI 工具的输入框，点击 **开始转换**

### 方式二：通过 URL 加载（需 CORS 代理）

远程网页可通过 URL 加载，但涉及跨域请求时需要先启动 CORS 代理（见下方说明）。

### 方式三：选择本地 HTML 文件

点击 **"选择文件"** 按钮，选择本地的 `.html` 文件直接载入。

## AI 生成 HTML 提示词

配合大语言模型生成可直接用于转换的 HTML，无需经过 HTML 解构工具预处理。

两种提示词文件位于 `Tools/HTMLTools/AI生成HTML提示词/`：

### SKILL_动态定位（推荐）

- **文件**：`SKILL_动态定位.md`
- **原理**：使用 `getBoundingClientRect()` 获取元素的精确位置和尺寸，自动注入 `data-u-left/top/width/height` 属性
- **优点**：输出优雅，标签语义清晰，兼容性好
- **使用方式**：将 `SKILL_动态定位.md` 的完整内容作为 System Prompt 或前置指令发给 AI（如 ChatGPT、Claude 等），然后描述你想要的 UI 界面

### SKILL_绝对定位

- **文件**：`SKILL_绝对定位.md`
- **原理**：通过 CSS `position: absolute` + `left/top/width/height` 确定每个元素的位置
- **适用场景**：AI 对动态定位理解不准确时使用此方案
- **注意**：所有元素必须使用 `position: absolute`，禁止使用 `margin`、`display`、`border` 等影响布局的属性

## CORS 代理

当需要从远程 URL 加载 HTML 页面时，浏览器存在跨域（CORS）限制。`Tools/HTMLTools/CORS代理/` 目录下提供了一个轻量级的 Node.js 代理服务器。

### 启动方式

**Windows**：双击 `CORS代理.bat`
**macOS / Linux**：在终端中运行 `bash CORS代理.sh`

**方法二**：在终端中运行：
```
node cors-proxy.js
```

启动后，控制台输出：
```
✅ CORS 代理已启动：http://localhost:8888
```

### 配置代理

在 HTML 解构工具的 URL 输入框旁边的 **"自定义 CORS 代理"** 输入框中填入：
```
http://localhost:8888/
```

然后在 URL 输入框中输入目标网页地址，点击 **"加载 URL"** 即可。

### 前提条件

- 已安装 [Node.js](https://nodejs.org/) 环境
- CORS 代理仅在本地开发时使用，请勿暴露到公网

# HTML Deconstruction Tool Tutorial

## Overview

The HTML Deconstruction Tool (`HTML解构工具.html`) is an offline web page that converts ordinary HTML into an HTML structure annotated with `data-u-left`, `data-u-top`, `data-u-width`, and `data-u-height` attributes. The output is directly consumable by the HtmlToUGUI conversion pipeline.

## Usage

### Method 1: Paste HTML Directly

1. Open `Tools/HTMLTools/HTML解构工具.html` in a browser
2. Paste HTML into the left input panel
3. Click **"Deconstruct HTML"**
4. Copy the output from the right panel
5. Paste into the HtmlToUGUI window and click **Start Convert**

### Method 2: Load via URL (CORS proxy required)

Remote web pages can be fetched via URL. Cross-origin requests require a running CORS proxy (see below).

### Method 3: Load Local File

Click the **"Choose File"** button to load a local `.html` file directly.

## AI-Generated HTML Prompt Skills

To bypass the Deconstruction Tool entirely, attach the prompt skill files from `Tools/HTMLTools/AI生成HTML提示词/` to your AI prompting workflow:

### SKILL_动态定位 (Recommended)

- **File**: `SKILL_动态定位.md`
- **Approach**: Uses `getBoundingClientRect()` to capture element positions and dimensions, then injects `data-u-left/top/width/height` attributes
- **Result**: Cleaner, more semantic HTML output with better compatibility

### SKILL_绝对定位

- **File**: `SKILL_绝对定位.md`
- **Approach**: Uses CSS `position: absolute` with explicit `left/top/width/height` values
- **Note**: All elements must use `position: absolute`. Avoid `margin`, `display`, `border` and other layout-affecting properties

## CORS Proxy

Loading remote HTML pages via URL is subject to browser CORS restrictions. A lightweight Node.js proxy server is provided in `Tools/HTMLTools/CORS代理/`.

### Starting the Proxy

**Option A (Windows)**: Double-click `CORS代理.bat`
**Option A (macOS / Linux)**: Run `bash CORS代理.sh` in the terminal

**Option B**: Run in terminal:
```
node cors-proxy.js
```

Expected output:
```
✅ CORS proxy started: http://localhost:8888
```

### Configuring the Proxy

In the Deconstruction Tool, enter the following into the **"Custom CORS Proxy"** field:
```
http://localhost:8888/
```

Then enter the target URL and click **"Load URL"**.

### Prerequisites

- [Node.js](https://nodejs.org/) must be installed
- The proxy is intended for local development only; do not expose it publicly

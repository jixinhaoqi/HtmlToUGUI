# HTML to UGUI Converter

[![Unity](https://img.shields.io/badge/Unity-2019.4%2B-black?logo=unity)](https://unity.com/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE.md)
![Version](https://img.shields.io/badge/version-0.1.0-blue)

Convert HTML + CSS content into Unity UGUI (Canvas) hierarchies at Editor time.

## Core Features

- **CSS Parsing** — supports `<style>` tags, inline styles, compound selectors (class/ID/attribute/descendant/child), pseudo-classes (`:hover`/`:active`/`:disabled`), CSS variables (`var()`), and relative units (`em`/`rem`/`%`/`calc()`)
- **Three Layout Calculators** — **Smart** (auto-chooses anchor/stretch/center), **Stretch** (full percentage), **Center** (centered pivot)
- **Pluggable Tag Handlers** — built-in support for `div`/`span`/`p`/`h1~h6`/`button`/`input`/`select`/`img`/`textarea`, extendable via `ITagHandler`
- **File Watcher** — automatically re-imports and converts on HTML file changes
- **Prefab Templates** — `UiPrefabSettings` ScriptableObject configures per-component prefabs

## Image Resolution

The tool resolves `<img src="...">` references against the HTML file's directory:
- `src="icon.png"` → looks for `icon.png`, or a named sprite inside a `.spriteatlasv2` atlas, or a slice from a Multiple-mode sprite sheet in the same folder
- Supports Single-mode sprites, Multiple-mode sprite sheets, and `.spriteatlasv2` atlases
- Image assets must reside under `Assets/` or `Packages/`

## SpriteAtlas Tools

Select a **SpriteAtlas** or **Sprite(Multiple)** asset, then use **Assets → HTMLToUGUI → 2D**:

| Menu Item | Description |
|---|---|
| SpriteAtlas → TMP_SpriteAsset | Export a SpriteAtlas into a TextMeshPro SpriteAsset (`.asset`) |
| SpriteAtlas → Sprite(Multiple) | Export a SpriteAtlas into a Multiple-mode sprite sheet |
| SpriteAtlas → TextureSheet | Pack sprites from a SpriteAtlas into a single grid-based texture |
| Sprite(Multiple) → Sprites | Split a Multiple-mode sprite sheet into individual sliced sprites |

> SpriteAtlas support must be enabled in **Project Settings → SpriteAtlas**.

## Quick Start

### Install via Unity Package Manager

1. Copy `https://github.com/jixinhaoqi/HtmlToUGUI.git`
2. Open Unity, go to **Window → Package Manager**
3. Click the **+** button in the top-left corner, select **Add package from git URL...**

### Using the Converter

1. Open **Tools → HTML to UGUI Converter**
2. Paste HTML content (pre-processed by the bundled HtmlTool), or pick a `.html` file
3. Click **Start Convert** to generate the UGUI hierarchy under a Canvas

> **Tip — AI-Generated HTML:** For best results without the pre-processing tool, attach [AI prompt skills](Tools/HTMLTools/AI生成HTML提示词/) from  when generating HTML:
> - **SKILL_动态定位** — recommended, produces responsive layout output
> - **SKILL_绝对定位** — use if the model reliably follows absolute-positioning instructions


## HTML Deconstruction Tool

[HTML Deconstruction Tool Tutorial](Tools/HTMLTools/HTML解构工具使用教程.md) — learn about the three usage modes, AI prompt skills, and CORS proxy setup.
[Online running tool](https://jixinhaoqi.github.io/HtmlToUGUI/)

## Samples

- [Example](Samples~/Example/): A simple HTML-to-UGUI demo scene with a complete `index_optimized.html`, sprite atlas, and Canvas output.

## Dependencies

| Package | Purpose |
|---|---|
| `com.unity.textmeshpro` | Text rendering (TextMeshPro) |
| `com.unity.2d.sprite` | Sprite atlas utilities |
| HtmlAgilityPack | HTML parsing and DOM traversal |

## Comparison

A lightweight, purpose-built solution for converting AI-generated or hand-crafted HTML into native UGUI hierarchies — ideal for rapid UI prototyping.

| Solution | Pros | Cons |
|---|---|---|
| **HtmlToUGUI** | Full CSS engine (selectors, pseudo-classes, variables, cascade); direct UGUI output with zero runtime overhead; pluggable tag handlers; three layout calculators; SpriteAtlas conversion utilities | Editor-time only (no runtime HTML rendering); HTML input requires pre-processing via bundled tool; limited to absolute/relative positioning |
| [UI Toolkit](https://docs.unity3d.com/Manual/UIElements.html) (Unity official) | Deep Unity integration; USS supports CSS-like styling; runtime and Editor modes; supported for builds | Emits to its own renderer (not UGUI); USS is a subset of CSS — no `var()`, limited pseudo-classes, different layout model; steep migration path for existing UGUI projects |
| [Vuplex 3D WebView](https://developer.vuplex.com/) | Full browser-grade HTML/CSS/JS; renders live web content in 3D/UI; cross-platform | Heavy runtime dependency (embedded Chromium); outputs to texture, not interactive UGUI; high memory/CPU cost; requires paid license |
| [UniWebView](https://uniwebview.com/) | Native WebView overlay on mobile; full HTML/CSS/JS; well-maintained | Mobile-only (iOS/Android); browser engine overhead; renders as overlay, not integrated into UGUI hierarchy; requires paid license |

**Key advantage of HtmlToUGUI:** you get a native UGUI hierarchy that works seamlessly with Unity's input system, prefabs, raycasting, and navigation — no extra runtime dependencies, no build bloat.

> **Tip — Design Tool to UGUI Workflow:**
> Design files from popular tools can be converted to UGUI through an intermediate HTML step:
>
> | Tool | HTML Export Path |
> |---|---|
> | **Figma** | Plugins like [Figma to HTML](https://www.figma.com/community/plugin/), [Anima](https://www.animaapp.com/), or built-in Dev Mode → CSS/HTML |
> | **Sketch** | [Anima](https://www.animaapp.com/), Sketch2React, or manual HTML export |
> | **Adobe XD** | Plugins like Web Export, or [Export Kit](https://exportkit.com/) |
> | **Photoshop** | Built-in **File → Export → HTML**, or tools like [psd2code](https://github.com/miaowmiaow/psd2code/) |
> | **AI-Generated** | Large language models output HTML directly, ready for conversion |
>
> After obtaining HTML from any of the above, feed it through this tool's conversion pipeline to produce a native UGUI hierarchy.

---

> [Comparison with Unity MCP/CLI](Compare_MCP_CLI.md) — detailed comparison between HtmlToUGUI and popular MCP/CLI AI tools that generate UGUI directly in the Unity Editor.

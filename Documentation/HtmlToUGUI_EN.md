# About HtmlToUGUI

Use the HtmlToUGUI package to convert HTML + CSS content into a Unity UGUI (Canvas) hierarchy at Editor time. The package parses embedded `<style>` tags and inline styles, builds a CSS cascade, and maps the styled HTML structure to UGUI GameObjects with RectTransforms, Images, TextMeshPro text, Buttons, InputFields, Toggles, Sliders, Dropdowns, and ScrollViews.

The package also provides SpriteAtlas utilities accessible from the Project window context menu.

## Installation

Install the package via the Unity Package Manager:

1. Open **Window → Package Manager**
2. Click the **+** button → **Add package from git URL** (or add by name for local packages)
3. If the package includes Samples, import them from the Package Manager window

## Getting Started

1. Open the converter window: **Tools → HTML to UGUI Converter**
2. Configure UI templates (optional): create a `UiPrefabSettings` asset (**Assets → Create → HTMLToUGUI → UiPrefabSettings**) and assign Prefabs for each component type
3. Load HTML content:
   - Paste pre-processed HTML into the text area; or
   - Click **Choose HTML File** to load a `.html` file
4. Configure conversion settings:

   | Setting | Description |
   |---|---|
   | Layout Calculator | Smart (recommended) / Stretch / Center — affects how absolute-positioned elements map to RectTransform anchors |
   | Legacy Text | Use Unity's legacy `Text` component instead of `TextMeshProUGUI` |
   | Text Overflow | Enable to prevent single-line text from wrapping |
   | Auto Convert | Automatically triggers conversion when a new file is selected |
   | Sync File | Watches the HTML file for changes and re-converts automatically |

5. Click **Start Convert**

The generated hierarchy appears under a `Canvas` object (or reuses an existing one in the scene).

> **Tip — AI-Generated HTML:** To skip the pre-processing step entirely, attach the AI prompt skills from `Tools/HTMLTools/AI生成HTML提示词/` when asking a large language model to generate HTML:
> - **SKILL_动态定位** — recommended; produces layout-ready output
> - **SKILL_绝对定位** — yields absolute-positioned HTML that feeds directly into the converter

![Example Editor Window](images/Example.jpg)

For a detailed guide on the HTML Deconstruction Tool, including usage modes, AI prompt skills, and CORS proxy setup, see the [HTML Deconstruction Tool Tutorial](../Tools/HTMLTools/HTML解构工具使用教程.md).

## Layout Calculators

Three implementations of `LayoutCalculator` control how `data-u-left/top/width/height` attributes are translated into UGUI anchors and offsets:

| Calculator | Behavior |
|---|---|
| Smart (default) | Detects whether the element should stretch to fill, snap to edge, or center — based on configurable thresholds |
| Stretch | Maps the element's bounding rect directly to percentage-based anchor min/max |
| Center | Places the element at the center of the parent with no stretching |

Smart Calculator's thresholds can be tuned in the converter window under "Layout Calculator".

## SpriteAtlas Tools

Select a **SpriteAtlas** or **Sprite(Multiple)** asset in the Project window and right-click → **Assets → HTMLToUGUI → 2D**:

| Menu Item | Description |
|---|---|
| SpriteAtlas → TMP_SpriteAsset | Exports the atlas into a TextMeshPro SpriteAsset (`.asset`) with glyph and character tables |
| SpriteAtlas → Sprite(Multiple) | Exports the atlas as a Multiple-mode sprite sheet `.png` |
| SpriteAtlas → TextureSheet | Packs all sprites from the atlas into a single grid-based texture |
| Sprite(Multiple) → Sprites | Slices a Multiple-mode sprite texture into individual single-mode `.png` files |

The `com.unity.2d.sprite` package must be installed. SpriteAtlas support must be enabled in **Project Settings → SpriteAtlas**.

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
> | **Photoshop** | Built-in **File → Export → HTML**, or tools like [psd2code](https://psd2code.com/) |
> | **AI-Generated** | Large language models output HTML directly, ready for conversion |
>
> After obtaining HTML from any of the above, feed it through this tool's conversion pipeline to produce a native UGUI hierarchy.

## Technical Details

### Requirements

- Unity 2019.4.26f1 or higher
- `com.unity.textmeshpro` (installed automatically)
- `com.unity.2d.sprite` (installed automatically)
- HtmlAgilityPack (bundled in the package)

### Known Limitations

- HTML input must be pre-processed: every element must carry `data-u-left/top/width/height` attributes (absolute positioning), OR the HTML must be processed by the bundled "HTML解构工具" (in `Tools/HTMLTools/`)
- Image source paths must point to files inside `Assets/` or `Packages/`
- Border-radius is not yet rendered (outline via `Outline` component only)
- Hyperlink click events on `<a>` tags are not wired up
- CSS `display: flex` / `grid` layout is not simulated; only absolute and relative positioning are supported

### Package Contents

| Location | Description |
|---|---|
| `Editor/` | Converter window, element factory, resource loader, file watcher, SpriteAtlas tools |
| `Runtime/` | CSS parser, layout calculators, tag handlers, style appliers, data models (usable at runtime) |
| `Samples/Example/` | Example scene with an HTML file and sprite atlas |
| `Tools/HTMLTools/` | Pre-processing tool and helper scripts |
| `Documentation/` | This user guide |

## Document Revision History

| Date | Reason |
|---|---|
| May 18, 2026 | Initial release |

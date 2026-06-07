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
2. Configure UI templates (optional): create a `UiPrefabSettings` asset (**Assets → Create → Html To UGUI → UiPrefabSettings**) and assign Prefabs for each component type
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

> **Comparison:**

<table>
  <tr>
    <td><img src="images/SKILL_动态定位-原网页.png" alt="SKILL_动态定位 - Original"></td>
    <td>→</td>
    <td><img src="images/SKILL_动态定位-转换后.png" alt="SKILL_动态定位 - Converted"></td>
  </tr>
</table>

<table>
  <tr>
    <td><img src="images/SKILL_绝对定位-原网页.png" alt="SKILL_绝对定位 - Original"></td>
    <td>→</td>
    <td><img src="images/SKILL_绝对定位-转换后.png" alt="SKILL_绝对定位 - Converted"></td>
  </tr>
</table>

![Example Editor Window](images/Example.jpg)

![More Conversion Examples](gifs/其他HTML转换示例.gif)

For a detailed guide on the HTML Deconstruction Tool, including usage modes, AI prompt skills, and CORS proxy setup, see the [HTML Deconstruction Tool Tutorial](../Tools/HTMLTools/HTML解构工具使用教程.md).

## Layout Calculators

Three implementations of `LayoutCalculator` control how `data-u-left/top/width/height` attributes are translated into UGUI anchors and offsets:

| Calculator | Behavior |
|---|---|
| Smart (default) | Detects whether the element should stretch to fill, snap to edge, or center — based on configurable thresholds |
| Stretch | Maps the element's bounding rect directly to percentage-based anchor min/max |
| Center | Places the element at the center of the parent with no stretching |

Smart Calculator's thresholds can be tuned in the converter window under "Layout Calculator".

### HTML Element Support

Built-in tag handler mappings:

| HTML Tag | UGUI Component |
|---|---|
| `div` / `span` / `p` | Container (empty GameObject + RectTransform) |
| `h1` ~ `h6` | Container + Text (font-size scaled by heading level) |
| `button` | Button + Text |
| `input` | InputField / Toggle / Slider (based on `type` attribute) |
| `select` | Dropdown |
| `img` | Image (supports single/multiple/atlas modes) |
| `textarea` | InputField (multiline) |
| `progress` / `meter` | Slider (non-interactive) |
| ScrollView | ScrollRect (with scrollbar styling) |

ScrollView additionally supports CSS properties `scrollbar-width` (`thin`) and `scrollbar-color` (`thumb-color track-color`), as well as `::-webkit-scrollbar`/`::-webkit-scrollbar-thumb`/`::-webkit-scrollbar-track` pseudo-element styles. Content size is automatically calculated from child element positions.

### Pseudo-Class Interaction States

Supported CSS pseudo-classes and their Unity ColorBlock mappings:

| Pseudo-class | ColorBlock Property |
|---|---|
| `:enabled` / default | `normalColor` |
| `:hover` | `highlightedColor` |
| `:active` | `pressedColor` |
| `:disabled` | `disabledColor` |
| `:selected` / `:focus` / `:checked` | `selectedColor` |

Dropdown items additionally support independent pseudo-class color settings (`ApplyDropdownItemPseudoColors`), preventing color conflicts with the parent Dropdown.

## Hierarchy Right-Click Layout

Select UI elements in the Hierarchy window, right-click → **GameObject → Html To UGUI** to apply a layout calculator directly to existing UGUI elements:

| Menu Item | Description |
|---|---|
| Append Smart Layout | Recalculates anchors and offsets using the smart layout calculator |
| Append Center Layout | Centers the element and its children within the parent |
| Append Stretch Layout | Stretches the element to fill the parent |

Layout is applied recursively to all children, automatically skipping inactive objects, TMP_SubMeshUI, rotated/scaled objects, and correctly handling ScrollRect content nodes.

## SpriteAtlas Tools

Select a **SpriteAtlas** or **Sprite(Multiple)** asset in the Project window and right-click → **Assets → Html To UGUI → 2D**:

| Menu Item | Description |
|---|---|
| SpriteAtlas → TMP_SpriteAsset | Exports the atlas into a TextMeshPro SpriteAsset (`.asset`) with glyph and character tables |
| SpriteAtlas → Sprite(Multiple) | Exports the atlas as a Multiple-mode sprite sheet `.png` |
| SpriteAtlas → TextureSheet | Packs all sprites from the atlas into a single grid-based texture |
| Sprite(Multiple) → Sprites | Slices a Multiple-mode sprite texture into individual single-mode `.png` files |

The `com.unity.2d.sprite` package must be installed. SpriteAtlas support must be enabled in **Project Settings → SpriteAtlas**.

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
- Floating text (text nodes not wrapped in a tag, e.g., `<div>floating text<div></div></div>`) is not supported

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
| Jun 7, 2026 | Added HTML element support table, pseudo-class interaction states; scrollbar/progress docs |
| Jun 5, 2026 | Added Hierarchy right-click layout menu; SpriteAtlas menu refactoring |
| Jun 4, 2026 | Updated with floating text limitation; removed Comparison section |
| May 19, 2026 | Initial release |

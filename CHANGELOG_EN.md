# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.2.0] - 2026-06-05

### Added
- `calc()` support in relative positioning mode of LayoutCalculator, enabling CSS calc expressions for sizing when `data-u-*` attributes are absent
- SKILL before/after comparison image tables in README and Documentation
- Introduction GIF demo in README
- Additional conversion example GIF in Documentation

### Changed
- AI prompt SKILL files simplified and restructured: dynamic positioning adds "no floating text" constraint and removes example request; absolute positioning simplified to 5 core rules
- HTML deconstruction tool built-in example replaced with a complete login page template
- README installation instructions rewritten as Git URL method for clarity
- Documentation: added known limitation regarding floating text
- Removed "Comparison" sections and MCP/CLI comparison links from all docs
- Renamed all `HTMLToUGUI` menu paths to `Html To UGUI` in documentation

### Fixed
- UguiElementFactory Image color logic: no longer forces white when having a border but no background color
- SmartLayoutCalculatorEditor threshold label: "Stretch Percent Threshold" → "Center Stretch Percent Threshold"

## [0.1.0] - 2026-05-18

### Added
- HTML parsing with CSS cascade and inherited style resolution
- Three layout calculators: Smart (adaptive anchor/stretch/center), Stretch, Center
- Pluggable TagHandler system with built-in support for div/span/p/h1~h6/button/input/select/img/textarea
- File watcher which auto-refreshes the UGUI output when the source HTML changes
- UiPrefabSettings ScriptableObject for per-component prefab templates
- Pseudo-class style mapping (`:hover`, `:active`, `:disabled` → Selectable ColorBlock)
- CSS variable (`var()`), `calc()`, and relative unit (`em`/`rem`/`%`) support
- SpriteAtlas utilities: convert to TMP_SpriteAsset / Sprite(Multiple) / TextureSheet, and slice sprites
- Runtime/Editor assembly separation (Xxhq.Htmltougui + Xxhq.Htmltougui.Editor)

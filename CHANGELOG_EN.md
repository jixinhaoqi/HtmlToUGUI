# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

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

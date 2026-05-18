# 更新日志

格式基于 [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)，
版本遵循 [Semantic Versioning](http://semver.org/spec/v2.0.0.html)。

## [0.1.0] - 2026-05-18

### Added
- HTML 解析，支持 CSS 级联与继承样式计算
- 三种布局计算器：智能（自适应锚点/拉伸/居中）、全拉伸、居中
- 可插拔 TagHandler 系统，内置支持 div/span/p/h1~h6/button/input/select/img/textarea
- 文件监视器，HTML 源文件变更后自动刷新 UGUI 输出
- UiPrefabSettings ScriptableObject，用于配置各组件类型的预制体模板
- 伪类样式映射（`:hover`、`:active`、`:disabled` → Selectable ColorBlock）
- CSS 变量（`var()`）、`calc()` 和相对单位（`em`/`rem`/`%`）支持
- SpriteAtlas 工具集：转换为 TMP_SpriteAsset / Sprite(Multiple) / TextureSheet，以及碎图拆分
- Runtime/Editor 程序集分离（Xxhq.Htmltougui + Xxhq.Htmltougui.Editor）

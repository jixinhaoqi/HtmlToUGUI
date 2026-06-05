# 更新日志

格式基于 [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)，
版本遵循 [Semantic Versioning](http://semver.org/spec/v2.0.0.html)。

## [0.2.0] - 2026-06-04

### Added
- LayoutCalculator 相对定位模式新增 `calc()` 支持，可在无 `data-u-*` 属性时使用 CSS calc 表达式计算尺寸

### Changed
- AI 提示词 SKILL 文件精简重构：动态定位方案增加"禁止游离文本"约束、去掉示例请求；绝对定位方案简化为 5 条核心规则
- HTML 解构工具内置示例替换为完整的登录页面模板
- README 安装说明重写为 Git URL 方式，步骤更清晰
- 文档更新：增加游离文本已知限制说明

### Fixed
- 修复 UguiElementFactory Image 颜色逻辑：无背景色时不再因边框而强制白色
- 修复 SmartLayoutCalculatorEditor 阈值标签说明："拉伸百分比阈值" → "中心拉伸百分比阈值"

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

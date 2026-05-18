# Contributing to HtmlToUGUI

感谢您对 HtmlToUGUI 的关注！我们欢迎各种形式的贡献，包括报告 Bug、提交功能请求、完善文档和改进代码。

## Bug 报告

1. 在 [Issues](https://github.com/jixinhaoqi/HtmlToUGUI/issues) 中搜索是否已有相似报告
2. 如无重复，创建新的 Issue 并选择 **Bug Report** 模板
3. 提供完整信息：Unity 版本、操作系统、重现步骤、预期行为与实际行为

## 功能请求

1. 在 [Issues](https://github.com/jixinhaoqi/HtmlToUGUI/issues) 中搜索是否已有相似请求
2. 创建新的 Issue 并选择 **Feature Request** 模板
3. 清晰描述使用场景和期望功能

## 代码贡献

1. Fork 本仓库
2. 创建功能分支：`git checkout -b feat/your-feature-name`
3. 遵循现有代码风格（命名、文件组织、注释）
4. 提交 Pull Request 至 `main` 分支
5. PR 描述中说明改动内容和关联 Issue

## 代码风格约定

- 使用 C# 编码规范，遵循 Unity 包的命名惯例
- public 类/方法/属性必须添加 XML 文档注释（`/// <summary>`）
- 提交信息使用英文，格式：`type(scope): description`（如 `feat(css): support nth-child selector`）
- 不引入新的第三方依赖，除非必要
- 确保兼容最低的unity版本

---

English version:

# Contributing to HtmlToUGUI

We welcome bug reports, feature requests, documentation improvements, and code contributions.

## Bug Reports

1. Search [Issues](https://github.com/jixinhaoqi/HtmlToUGUI/issues) for duplicates
2. Open a new Issue using the **Bug Report** template
3. Include: Unity version, OS, reproduction steps, expected vs actual behavior

## Feature Requests

1. Search [Issues](https://github.com/jixinhaoqi/HtmlToUGUI/issues) for duplicates
2. Open a new Issue using the **Feature Request** template
3. Describe the use case and desired functionality clearly

## Pull Requests

1. Fork the repo
2. Create a feature branch: `git checkout -b feat/your-feature-name`
3. Follow the existing code style and naming conventions
4. Submit a PR to `main` with a clear description of changes

## Code Style

- Follow C# conventions consistent with Unity packages
- Add XML documentation (`/// <summary>`) on all public APIs
- Use English for commit messages: `type(scope): description`
- Avoid adding new third-party dependencies

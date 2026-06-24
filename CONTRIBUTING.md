# 参与贡献

欢迎提交 Pull Request 与 Issue，你的贡献对我们很有价值。

## 为何先开 Issue

在动手改代码前，建议先在 [Issues](https://github.com/Shuazijun/RocketModFix/issues) 讨论：

- **避免重复劳动**：他人可能已在处理相同问题。
- **方案对齐**：讨论有助于找到更合适的实现方式。
- **节省时间**：确保改动方向与项目目标一致。

## 贡献者须知

请尽量遵守以下原则，否则改动可能无法合并：

1. **向后兼容**
   - 改动是否会破坏现有插件或旧版 Rocket 的兼容性？
   - 若破坏兼容，通常不予接受。

2. **适用范围**
   - 改动是否适用于不同 Unturned / Rocket 版本，以及仅安装 LDM 而未安装本 fork 的环境？
   - 若会导致其他环境异常，可能无法合并。

3. **充分测试**
   - 是否在本地或测试服务器上验证？
   - 专用服务器（Windows U3DS / Linux）与常见旧插件建议至少冒烟测试。

4. **保持简单**
   - 优先采用最小、可维护的修复，避免过度设计。

## 文档语言约定（本 fork）

| 文档 | 语言 | 说明 |
|------|------|------|
| `README.md` | 简体中文 | 面向服务器管理员与玩家 |
| `CONTRIBUTING.md` | 简体中文 | 本文件 |
| `Rocket.AutoInstaller/README.md` | 简体中文 | 自动安装器说明 |
| `Module/InstallationGuides/Readme_ZH.txt` 等 | 多语言 | 随模块发布的安装摘要 |
| `Rocket/docs/` | 英文 | 上游插件 API 文档，保持英文 |
| `Module/RuntimeLibs/README.md` | 英文 | 运行时库技术说明 |

感谢你的贡献。

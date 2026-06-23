# RocketModFix

**RocketModFix** 是 Unturned 插件开发者维护的 [LDM](https://github.com/SmartlyDressedGames/Legally-Distinct-Missile) 分支。本分支不计划对 RocketMod 做大规模改动，仅进行修复和不破坏向后兼容的新功能开发，因此你无需更新现有插件。

## 兼容性

- 旧插件仍可正常使用，无需修改、重新编译或更新。
- 不计划对 API 做破坏性变更。
- 目标框架：`net481`（兼容既有插件程序集）。

## 安装

提供两种安装方式：`标准安装` 或 `自动安装器（Auto-Installer）`。推荐使用 `标准安装`。

<h3 id="standard-install">标准安装（推荐）</h3>

1. **停止服务器**：若服务器正在运行，请先关闭。
2. **删除旧版 Rocket**：删除 `Modules` 目录下的整个 `Rocket.Unturned` 文件夹（若存在）。
3. **下载最新版**：前往 [Releases 页面](https://github.com/Shuazijun/RocketModFix/releases)。
4. **展开 Assets**：若未展开，请打开「Assets」区域。
5. **下载模块**：点击 `Rocket.Unturned.zip`（或 `Rocket.Unturned.Module.zip`）。
6. **安装**：解压后打开 `Rocket.Unturned` 文件夹，将 `Rocket.Unturned` **文件夹**（不是文件夹内的内容）复制到 `Modules`；若提示替换，选择替换。

<h3 id="auto-installer">自动安装器</h3>

与标准安装相同，但更新后重启服务器即可自动获取新版本，无需手动删除旧文件或替换。

更多说明见 [Rocket.AutoInstaller/README.md](Rocket.AutoInstaller/README.md)。

1. **停止服务器**
2. **删除旧版 Rocket**（若仍存在）：删除 `Modules` 下的 `Rocket.Unturned` 文件夹
3. **下载最新版**：前往 [Releases 页面](https://github.com/Shuazijun/RocketModFix/releases)
4. **展开 Assets**
5. **下载**：点击 `Rocket.AutoInstaller.zip`
6. **安装**：解压后将 `Rocket.AutoInstaller` **文件夹**复制到 `Modules`

## 简体中文

模块包内已包含简体中文资源（`Schinese.dat`、翻译样本与配置示例）。

1. 在 `Servers/<实例>/Rocket/Rocket.config.xml` 中设置：
   ```xml
   <LanguageCode>zh</LanguageCode>
   ```
   也支持 `Schinese`、`zh-CN`（会自动映射为 `zh`）。
2. 首次启动且尚无翻译文件时，会自动从 `Modules/Rocket.Unturned/ConfigSamples/` 复制中文翻译。
3. 配置说明与示例见 `Modules/Rocket.Unturned/ConfigSamples/README.txt`。

## 本 fork 近期功能（4.25.x）

### 内嵌功能（独立配置，默认关闭）

| 功能 | 配置 | 主要命令 |
|------|------|----------|
| TPA | `Rocket.Unturned.TPA.config.xml` | `/tpa` |
| 多 Home | `Rocket.Unturned.Homes.config.xml` | `/home` `/homes` … |
| Warps | `Rocket.Unturned.Warps.config.xml` | `/warp` `/warps` `/setwarp` … |
| 命令别名 | `Rocket.Unturned.CommandAlias.config.xml` | 如 `/回家` → `/home` |
| 存档通知 | `Rocket.Unturned.AutoSave.config.xml` | 无命令 |

权限示例：`Module/ConfigSamples/Permissions.config.example.xml`（玩家 / VIP / 管理员三组）。  
完整命令表见仓库内 `docs/DEV_Commands_Permissions.md`。

### 4.24.x 修复

- Assembly 解析：启动预加载、缓存、缺失依赖去重日志
- 命令：`/vanish`、`/god`（含氧气）、`/p` 输出优化
- Steam 资料：后台异步加载
- 配置：XML/JSON 统一 UTF-8 读写

## 资源

fr34kyn01535 在 /r/RocketMod 子版块整理了原版全部插件列表：[旧仓库插件列表](https://www.reddit.com/r/rocketmod/comments/ek4i7b/)

GitHub 上的 RocketMod 组织托管了多个相关归档项目：[RocketMod（已弃用）](https://github.com/RocketMod)

插件开发文档（英文，上游保留）：[Rocket/docs](Rocket/docs/index.md)

## 历史

2019 年 12 月 20 日，Sven Mawby「fr34kyn01535」与 Enes Sadık Özbek「Trojaner」正式宣布停止维护 Rocket，并以 MIT 许可证开源。[完整告别声明](https://github.com/RocketMod/Rocket/blob/master/Farewell.md)

此后 SDG fork 了该仓库，与游戏同步维护。

2020 年 6 月 2 日，fr34kyn01535 要求该 fork 更名，以与项目保持距离。

## 致谢

[OpenMod](https://github.com/openmod/openmod) 提供了 NuGet 包与工作流参考。

## 参与贡献

见 [CONTRIBUTING.md](CONTRIBUTING.md)。

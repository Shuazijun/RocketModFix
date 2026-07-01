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

### 4.25.x 新增

- **Unity 控制台日志过滤**（`SuppressUnityConsoleWarnings`，默认 `true`）：启用后过滤 Unity 全部控制台输出（含 Log / Warning / Error / Assert / Exception 及 BoxCollider、Shader 等）；仅保留 `CommandWindow` 与 `Rocket.log`。旧键 `SuppressHeadlessGraphicsLogs` 仍兼容。
- **控制台时间戳**：所有 `CommandWindow` 控制台输出（含玩家聊天、加入/离开、指令日志等）统一前缀 `[yyyy-MM-dd HH:mm:ss]`；`Rocket.log` 格式不变。
- `vehiclelist`、`rgive`、`rvehicle` 等管理命令（见 Releases 4.25.4–4.25.8）

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

---

## 阶段概览（AI 续写）

- **目标**：过滤 Unity 控制台所有非 ERROR 级别告警
- **完成状态**：已完成
- **变更范围**：`Rocket.Unturned/Utils/UnityConsoleWarningFilter.cs`（新建）、`HeadlessLogFilter.cs`（删除）、`UnturnedSettings.cs`、`U.cs`、`ConfigOptionsReloader.cs`、`Rocket.Unturned.config.example.xml`

## 接口与钩子清单

| 符号 | 文件 | 用途 | 输入/输出 |
|------|------|------|-----------|
| `UnityConsoleWarningFilterHandler` | `Utils/UnityConsoleWarningFilter.cs` | `ILogHandler` 包装，按 `LogType` 过滤 | `LogFormat` → 非 Error/Assert/Exception 时丢弃 |
| `UnityConsoleWarningFilter.Install` | 同上 | 替换 `Debug.unityLogger.logHandler` | 无 |
| `UnityConsoleWarningFilter.TryApplyEarlyFromConfigFile` | 同上 | 启动早期读 XML 应用开关 | 读 `SuppressUnityConsoleWarnings` / 旧键 |
| `UnityConsoleWarningFilter.ApplyFromSettings` | 同上 | 配置加载后同步开关 | `bool enabled` |
| `SuppressUnityConsoleWarnings` | `Serialisation/UnturnedSettings.cs` | 配置项，默认 `true` | XML bool |

## 修复方法清单

| 问题 | 根因 | 策略 | 修改点 |
|------|------|------|--------|
| 控制台 Unity Warning/Log 噪音（BoxCollider、Shader 等） | Unity 默认输出全部 `LogType` | 启用时仅放行 `Error`/`Assert`/`Exception` | `UnityConsoleWarningFilterHandler.ShouldPassThrough` |
| 旧配置键兼容 | 用户已有 `SuppressHeadlessGraphicsLogs` | 属性别名 + 早期 XML 双键解析 | `UnturnedSettings.SuppressHeadlessGraphicsLogs` |

## 注意事项与风险

- 仅影响 Unity `Debug.unityLogger` 输出，不影响 `CommandWindow` 游戏日志、`Rocket.log`、插件 `LogException`
- 设为 `false` 可恢复完整 Unity 日志用于调试
- 热更新仅需替换 `Rocket.Unturned.dll`

## 验证与回归

- **已验证**：`dotnet build Rocket.Unturned` Release 0 错误
- **未覆盖**：实机 Dedicated 控制台肉眼确认（需部署后验证）

## AI 续写上下文

- **当前状态**：过滤逻辑已合入，默认开启
- **下一阶段待办**：（可选）版本 bump 与 Release
- **配置键**：`Servers/<id>/Rocket/Rocket.Unturned.config.xml` → `<SuppressUnityConsoleWarnings>true</SuppressUnityConsoleWarnings>`

---

## 阶段概览（控制台时间戳）

- **目标**：控制台所有展示内容统一前缀 `[yyyy-MM-dd HH:mm:ss]`
- **完成状态**：已完成
- **变更范围**：`Rocket.Unturned/Utils/CommandConsoleTimestamp.cs`（新建）、`U.cs`

## 接口与钩子清单

| 符号 | 文件 | 用途 | 输入/输出 |
|------|------|------|-----------|
| `CommandConsoleTimestamp.Install` | `Utils/CommandConsoleTimestamp.cs` | Harmony 安装控制台时间戳补丁 | 无 |
| `OutputToConsolePrefix` | 同上 | 在 `outputToConsole` 前为文本加时间戳 | `ref string value` |

## 修复方法清单

| 问题 | 根因 | 策略 | 修改点 |
|------|------|------|--------|
| 控制台无时间戳 | `CommandWindow` 输出不经 `AsyncLoggerQueue` | Harmony Prefix 补丁 `outputToConsole` | `ConsoleInputOutputBase` / `ThreadedConsoleInputOutput` / `LegacyInputOutput` |

## 注意事项与风险

- 仅影响控制台显示，不改变 `Rocket.log` 双重时间戳
- 已带 `[yyyy-MM-dd HH:mm:ss]` 前缀的行不会重复添加
- 热更新仅需替换 `Rocket.Unturned.dll`

## 验证与回归

- **已验证**：`dotnet build Rocket.Unturned` Release
- **未覆盖**：实机 Dedicated 控制台与聊天肉眼确认

## AI 续写上下文

- **当前状态**：控制台时间戳默认启用，无配置开关
- **下一阶段待办**：（可选）版本 bump 与 Release

---

## 阶段概览（4.25.9.2 Unity 全量过滤）

- **目标**：过滤 Unity 全部控制台输出（含 Assert / Exception），修复 `ConsoleWriterProxy` 绕过过滤
- **完成状态**：已完成并发布 `4.25.9.2`
- **变更范围**：`UnityConsoleWarningFilter.cs`、`ConsoleWriterProxyFilterPatches.cs`（新建）、`U.cs`、`UnturnedSettings.cs`、`build-release.ps1`

## 接口与钩子清单

| 符号 | 文件 | 用途 | 输入/输出 |
|------|------|------|-----------|
| `UnityConsoleNoiseMatcher.ShouldSuppressLogHandlerMessage` | `Utils/UnityConsoleWarningFilter.cs` | 启用时抑制全部 `ILogHandler` 级别 | `LogType` + message → bool |
| `UnityConsoleWarningFilterHandler.LogException` | 同上 | 启用时丢弃 `LogException` | Exception → void |
| `ConsoleWriterProxyFilterPatches` | `Utils/ConsoleWriterProxyFilterPatches.cs` | Harmony 拦截 `ConsoleWriterProxy.WriteLine`/`Write` | stdout 行 → 过滤或放行 |
| `SuppressUnityConsoleWarnings` | `Serialisation/UnturnedSettings.cs` | 总开关，默认 `true` | XML bool |

## 修复方法清单

| 问题 | 根因 | 策略 | 修改点 |
|------|------|------|--------|
| BoxCollider/Shader 仍输出 | Unturned `ConsoleWriterProxy` 不经 `Console.Out` | Harmony Prefix 过滤 | `ConsoleWriterProxyFilterPatches` |
| Assert/Error 仍显示 | 4.25.9.1 保留 Exception | 全级别过滤含 `LogException` | `ShouldSuppressLogHandlerMessage` / `LogException` |
| stdout Assert 行 | 无 `ERROR:` 前缀 | 匹配 `Assertion failed` / `ASSERT:` | `ShouldSuppressMessage` |

## 注意事项与风险

- **保留**：`CommandWindow` 输出（带时间戳）、`Rocket.log`、`Logger.LogException` → `CommandWindow.LogError`
- **过滤**：`Debug.Log*`、`ILogHandler` 全部级别、匹配规则的 stdout
- 调试时设 `<SuppressUnityConsoleWarnings>false</SuppressUnityConsoleWarnings>`
- 热更新替换 `Rocket.Unturned.dll`（含 `0Harmony.dll`）

## 验证与回归

- **已验证**：`build-release.ps1 -Version 4.25.9.2 -Zip` win/linux 构建成功
- **未覆盖**：实机 Dedicated / Pterodactyl 控制台肉眼确认

## AI 续写上下文

- **当前状态**：GitHub Release `4.25.9.2` 已发布
- **下一阶段待办**：实机验证过滤效果；按需调整 stdout 匹配规则
- **产物**：`dist/win-x64/Rocket.Unturned.win-x64.zip`、`dist/linux-x64/Rocket.Unturned.linux-x64.zip`

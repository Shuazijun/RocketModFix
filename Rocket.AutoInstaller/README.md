# Rocket.AutoInstaller

RocketModFix 自动安装器：从 GitHub Releases 下载并安装最新版，也支持指向本地构建产物以便开发测试。

## 快速开始

将模块文件夹放入服务器的 `Modules` 目录并启动。首次运行会自动下载并安装最新 RocketModFix。

**重要：** 使用前请删除手动安装的 `Modules/Rocket.Unturned`，避免与自动安装冲突。

## 配置

编辑模块目录下的 `config.json`：

```json
{
	"EnableCustomInstall": false,
	"CustomInstallPath": "",
	"BlockIfRocketInstalled": true,
	"AutoInstallRocketFromExtras": false,
	"EnableRetry": true,
	"EnableCaching": true
}
```

### 选项说明

**普通用户：**

- `BlockIfRocketInstalled` — 若已存在 Rocket 则阻止安装
- `EnableRetry` — 下载失败时重试（约 5 次，间隔 5 秒）
- `EnableCaching` — 启用缓存，加快启动并支持离线使用

**开发者：**

- `EnableCustomInstall` — 使用本地路径安装，不从 GitHub 下载
- `CustomInstallPath` — 本地构建路径（见下方示例）
- `AutoInstallRocketFromExtras` — 从 Extras 目录自动安装

### 本地安装路径示例

`CustomInstallPath` 可指向：

1. **Zip 文件**
   ```
   "CustomInstallPath": "C:\\Builds\\Rocket.Unturned.zip"
   ```

2. **已解压目录**（含 `Rocket.Unturned.dll`、子目录中的 dll，或目录内的 zip）

3. **HTTP/HTTPS 直链**（指向 zip）

   **注意：** GitHub Actions 构件需认证，不适合作为公开下载地址。

## 功能状态

- [x] 从 GitHub Releases 自动安装
- [x] 本地构建自动安装（`EnableCustomInstall` + `CustomInstallPath`）
- [x] 缓存与重试
- [x] 已安装 Rocket 时可选阻止（`BlockIfRocketInstalled`）

更多安装说明见仓库根目录 [README.md](../README.md)。

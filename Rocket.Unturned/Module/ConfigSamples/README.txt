# RocketModFix 配置与语言文件说明



本目录随模块安装包一并发布，路径：Modules/Rocket.Unturned/ConfigSamples/



## 简体中文设置步骤



1. 在 Servers/<实例>/Rocket/Rocket.config.xml 中设置：

   <LanguageCode>zh</LanguageCode>

   也支持：Schinese、zh-CN（会自动映射为 zh）



2. 首次启动时，若 Rocket 目录下尚无翻译文件，将自动从本目录复制：

   - Rocket.zh.translation.xml

   - Rocket.Unturned.zh.translation.xml



   英语为程序内置默认文案（U.cs）；仅需维护 zh 翻译文件即可覆盖简体中文。



3. 配置示例（含中文注释，仅供参考，不会被自动覆盖）：

   - Rocket.config.example.xml

   - Rocket.Unturned.config.example.xml

   - Permissions.config.example.xml          ← 权限组（玩家 / VIP / 管理员）

   - Rocket.Unturned.TPA.config.example.xml

   - Rocket.Unturned.Homes.config.example.xml

   - Rocket.Unturned.Warps.config.example.xml

   - Rocket.Unturned.AutoSave.config.example.xml

   - Rocket.Unturned.CommandAlias.config.example.xml



4. 权限文件部署：

   复制 Permissions.config.example.xml → Servers/<实例>/Rocket/Permissions.config.xml

   将管理员 Steam64 填入 <Group Id="admin"><Members>…</Members></Group>

   命令与权限对照见 docs/DEV_Commands_Permissions.md



5. 测试服一键初始化（不覆盖已有配置）：

   .\scripts\deploy-test-configs.ps1

   或与构建一并：.\build-release.ps1 -DeployU3DSTest -DeployTestConfigs
   强制覆盖已有配置：.\build-release.ps1 -DeployU3DSTest -DeployTestConfigs -ForceTestConfigs



## 模块显示名称（Unturned 模块列表）



- 英文：Rocket.Unturned/English.dat

- 简体中文：Rocket.Unturned/Schinese.dat



## 文件编码



请使用 UTF-8 保存所有配置与翻译文件（推荐带 BOM）。

Rocket 读写 XML/JSON 配置已统一使用 UTF-8。



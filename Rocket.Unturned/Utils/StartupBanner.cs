using SDG.Unturned;
using System.Reflection;

namespace Rocket.Unturned.Utils
{
    internal static partial class StartupBanner
    {
        private static bool logged;

        public static void Subscribe()
        {
            Level.onLevelLoaded += OnLevelLoaded;
            if (Level.isLoaded)
            {
                LogOnce();
            }
        }

        private static void OnLevelLoaded(int level)
        {
            LogOnce();
        }

        public static void LogOnce()
        {
            if (logged)
            {
                return;
            }

            logged = true;
            string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "?";

            CommandWindow.Log("============================================================");
            CommandWindow.Log(" RocketModFix — LDM / Rocket 二次开发分支");
            CommandWindow.Log(" 本产物为社区维护 fork，非 SDG 官方 Rocket 发行版");
            CommandWindow.Log("------------------------------------------------------------");
            CommandWindow.Log($" 仓库: {BuildMetadata.RepositoryUrl}");
            CommandWindow.Log($" 版本: {version}  |  提交: {BuildMetadata.GitCommit}  |  分支: {BuildMetadata.GitBranch}");
            CommandWindow.Log($" 构建时间 (UTC): {BuildMetadata.BuildTimeUtc}");
            CommandWindow.Log("------------------------------------------------------------");
            CommandWindow.Log(" 以下原第三方插件能力已内嵌为标准功能，无需额外安装:");
            CommandWindow.Log("  · 命令别名 (CommandAlias)");
            CommandWindow.Log("  · 玩家传送 (TPA)");
            CommandWindow.Log("  · 家园 / MoreHomes (Homes)");
            CommandWindow.Log("  · 传送点 (Warps)");
            CommandWindow.Log("  · 定时自动存档与可选全服通知 (AutoSave)");
            CommandWindow.Log(" ReCoding By Shuazi & Email:shuazi@ixovo.com");
            CommandWindow.Log("============================================================");
        }
    }
}

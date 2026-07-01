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
            CommandWindow.Log(" 本产物为社区维护 Fork, 非 SDG 官方 Rocket 发行版");
            CommandWindow.Log("------------------------------------------------------------");
            CommandWindow.Log($" 仓库: {BuildMetadata.RepositoryUrl}");
            CommandWindow.Log($" 版本: {version}");
            CommandWindow.Log($" 构建时间 (UTC): {BuildMetadata.BuildTimeUtc}");
            CommandWindow.Log("------------------------------------------------------------");
            CommandWindow.Log(" 模块运行时诊断 (Harmony / 过滤器):");
            foreach (string line in ModuleRuntimeDiagnostics.GetReportLines())
            {
                CommandWindow.Log("  " + line);
            }
            CommandWindow.Log("------------------------------------------------------------");
            CommandWindow.Log(" ReCoding By Shuazi   Email:shuazi@ixovo.com");
            CommandWindow.Log("============================================================");
        }
    }
}

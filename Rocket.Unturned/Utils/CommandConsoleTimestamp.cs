using HarmonyLib;
using SDG.Unturned;
using System;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Rocket.Unturned.Utils
{
    internal static class CommandConsoleTimestamp
    {
        public const string HarmonyId = "com.rocketmodfix.unturned.console-timestamp";
        private const string TimestampFormat = "yyyy-MM-dd HH:mm:ss";
        private static readonly Regex LeadingTimestamp = new Regex(
            @"^\[\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\] ",
            RegexOptions.Compiled);

        private static Harmony? harmony;
        private static bool installed;
        private static int patchedMethodCount;

        public static bool IsInstalled => installed;
        public static int PatchedMethodCount => patchedMethodCount;

        public static void Install()
        {
            if (installed)
            {
                return;
            }

            installed = true;
            patchedMethodCount = 0;
            harmony = new Harmony(HarmonyId);

            MethodInfo prefix = AccessTools.Method(typeof(CommandConsoleTimestamp), nameof(OutputToConsolePrefix))!;
            patchedMethodCount += PatchOutputToConsole(typeof(ConsoleInputOutputBase), prefix) ? 1 : 0;
            patchedMethodCount += PatchOutputToConsole(typeof(ThreadedConsoleInputOutput), prefix) ? 1 : 0;
            patchedMethodCount += PatchOutputToConsole(typeof(LegacyInputOutput), prefix) ? 1 : 0;
        }

        private static bool PatchOutputToConsole(Type type, MethodInfo prefix)
        {
            MethodInfo? target = AccessTools.Method(type, "outputToConsole", new[] { typeof(string), typeof(ConsoleColor) });
            if (target != null)
            {
                harmony!.Patch(target, prefix: new HarmonyMethod(prefix));
                return true;
            }

            return false;
        }

        private static void OutputToConsolePrefix(ref string value)
        {
            if (string.IsNullOrEmpty(value) || LeadingTimestamp.IsMatch(value))
            {
                return;
            }

            value = "[" + DateTime.Now.ToString(TimestampFormat, CultureInfo.InvariantCulture) + "] " + value;
        }
    }
}

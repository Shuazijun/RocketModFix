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

        public static void Install()
        {
            if (installed)
            {
                return;
            }

            installed = true;
            harmony = new Harmony(HarmonyId);

            MethodInfo prefix = AccessTools.Method(typeof(CommandConsoleTimestamp), nameof(OutputToConsolePrefix));
            PatchOutputToConsole(typeof(ConsoleInputOutputBase), prefix);
            PatchOutputToConsole(typeof(ThreadedConsoleInputOutput), prefix);
            PatchOutputToConsole(typeof(LegacyInputOutput), prefix);
        }

        private static void PatchOutputToConsole(Type type, MethodInfo prefix)
        {
            MethodInfo? target = AccessTools.Method(type, "outputToConsole", new[] { typeof(string), typeof(ConsoleColor) });
            if (target != null)
            {
                harmony!.Patch(target, prefix: new HarmonyMethod(prefix));
            }
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

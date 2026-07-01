using HarmonyLib;
using SDG.Unturned;
using System.Collections.Generic;

namespace Rocket.Unturned.Utils
{
    [HarmonyPatch(typeof(ConsoleInputOutputBase), nameof(ConsoleInputOutputBase.initialize))]
    internal static class ConsoleInputOutputBaseInitializePatch
    {
        [HarmonyPostfix]
        private static void Postfix(ConsoleInputOutputBase __instance)
        {
            ConsoleOutputRedirector? redirector = Traverse.Create(__instance).Field<ConsoleOutputRedirector>("outputRedirector").Value;
            if (redirector != null)
            {
                UnityConsoleWarningFilter.WrapRedirectorWriters(redirector);
            }
        }
    }
}

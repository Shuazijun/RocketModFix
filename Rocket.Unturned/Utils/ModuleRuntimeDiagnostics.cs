using HarmonyLib;
using System.Reflection;

namespace Rocket.Unturned.Utils
{
    internal static class ModuleRuntimeDiagnostics
    {
        public static string GetHarmonyLoadStatusLine()
        {
            return $"Harmony 已载入: {IsHarmonyLibLoaded()}";
        }

        private static bool IsHarmonyLibLoaded()
        {
            try
            {
                _ = typeof(Harmony).Assembly;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

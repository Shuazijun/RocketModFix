using HarmonyLib;
using SDG.Unturned;
using System.IO;
using System.Text;
using System.Threading;

namespace Rocket.Unturned.Utils
{
    [HarmonyPatch(typeof(ConsoleWriterProxy), nameof(ConsoleWriterProxy.WriteLine), new[] { typeof(string) })]
    internal static class ConsoleWriterProxyWriteLinePatch
    {
        [HarmonyPrefix]
        private static bool Prefix(string value)
        {
            if (!UnityConsoleWarningFilter.IsEnabled || value == null)
            {
                return true;
            }

            if (UnityConsoleNoiseMatcher.ShouldSuppressStdoutLine(value))
            {
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ConsoleWriterProxy), nameof(ConsoleWriterProxy.Write), new[] { typeof(char) })]
    internal static class ConsoleWriterProxyWriteCharPatch
    {
        private static readonly ThreadLocal<StringBuilder> LineBuffers = new ThreadLocal<StringBuilder>(() => new StringBuilder());

        [HarmonyPrefix]
        private static bool Prefix(ConsoleWriterProxy __instance, char value)
        {
            if (!UnityConsoleWarningFilter.IsEnabled)
            {
                return true;
            }

            StringBuilder buffer = LineBuffers.Value!;
            if (value == '\n')
            {
                string line = buffer.ToString();
                buffer.Clear();
                if (!string.IsNullOrEmpty(line) && !UnityConsoleNoiseMatcher.ShouldSuppressStdoutLine(line))
                {
                    ConsoleWriterProxyFilterSupport.WriteLine(__instance, line);
                }

                return false;
            }

            if (value != '\r')
            {
                buffer.Append(value);
            }

            return false;
        }
    }

    internal static class ConsoleWriterProxyFilterSupport
    {
        public static void WriteLine(ConsoleWriterProxy instance, string line)
        {
            StreamWriter? customWriter = Traverse.Create(instance).Field<StreamWriter>("customWriter").Value;
            TextWriter? defaultWriter = Traverse.Create(instance).Field<TextWriter>("defaultConsoleWriter").Value;
            customWriter?.WriteLine(line);
            defaultWriter?.WriteLine(line);
        }
    }
}

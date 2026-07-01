using HarmonyLib;
using SDG.Unturned;
using System.IO;
using System.Text;

namespace Rocket.Unturned.Utils
{
    [HarmonyPatch(typeof(ConsoleOutputRedirector), nameof(ConsoleOutputRedirector.enable), new[] { typeof(bool) })]
    internal static class ConsoleOutputRedirectorEnablePatch
    {
        private static void Postfix(ConsoleOutputRedirector __instance)
        {
            UnityConsoleWarningFilter.Install();
            UnityConsoleWarningFilter.WrapRedirectorWriters(__instance);
        }
    }

    internal sealed class FilteringStreamWriter : StreamWriter
    {
        private readonly StringBuilder lineBuffer = new StringBuilder();

        public FilteringStreamWriter(Stream stream, Encoding encoding)
            : base(stream, encoding)
        {
            AutoFlush = true;
        }

        public bool Enabled { get; set; } = true;

        public override void Write(char value)
        {
            if (!Enabled)
            {
                base.Write(value);
                return;
            }

            if (value == '\n')
            {
                FlushBufferedLine();
                return;
            }

            if (value != '\r')
            {
                lineBuffer.Append(value);
            }
        }

        public override void Write(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            foreach (char character in value)
            {
                Write(character);
            }
        }

        public override void WriteLine(string? value)
        {
            if (!Enabled)
            {
                base.WriteLine(value);
                return;
            }

            if (string.IsNullOrEmpty(value))
            {
                base.WriteLine();
                return;
            }

            if (!UnityConsoleNoiseMatcher.ShouldSuppressStdoutLine(value))
            {
                base.WriteLine(value);
            }
        }

        public override void WriteLine()
        {
            if (!Enabled)
            {
                base.WriteLine();
                return;
            }

            FlushBufferedLine();
        }

        public override void Flush()
        {
            FlushBufferedLine();
            base.Flush();
        }

        private void FlushBufferedLine()
        {
            if (lineBuffer.Length == 0)
            {
                return;
            }

            string line = lineBuffer.ToString();
            lineBuffer.Clear();
            if (!Enabled || !UnityConsoleNoiseMatcher.ShouldSuppressStdoutLine(line))
            {
                base.WriteLine(line);
            }
        }
    }
}

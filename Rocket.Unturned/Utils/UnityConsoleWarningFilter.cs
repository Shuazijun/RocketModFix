using HarmonyLib;
using SDG.Unturned;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Rocket.Unturned.Utils
{
    internal static class UnityConsoleNoiseMatcher
    {
        public static bool ShouldSuppressStdoutLine(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            if (message.Length >= 21
                && message[0] == '['
                && char.IsDigit(message[1])
                && message.IndexOf("] ", 10, StringComparison.Ordinal) > 0)
            {
                return false;
            }

            return ShouldSuppressMessage(message);
        }

        public static bool ShouldSuppressLogHandlerMessage(LogType logType, string message)
        {
            return true;
        }

        private static bool ShouldSuppressMessage(string message)
        {
            if (message.StartsWith("WARNING:", StringComparison.OrdinalIgnoreCase)
                || message.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase)
                || message.StartsWith("ASSERT:", StringComparison.OrdinalIgnoreCase)
                || message.IndexOf("Assertion failed", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (message.IndexOf("BoxCollider does not support negative", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (message.IndexOf("effective box size has been forced positive", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (message.IndexOf("use the convex MeshCollider", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (message.IndexOf("Scene hierarchy path", StringComparison.OrdinalIgnoreCase) >= 0
                && message.IndexOf("MeshCollider", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (message.IndexOf("The animation state", StringComparison.OrdinalIgnoreCase) >= 0
                && (message.IndexOf("could not be played", StringComparison.OrdinalIgnoreCase) >= 0
                    || message.IndexOf("Please attach an animation clip", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return true;
            }

            if (message.IndexOf("Shader", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (message.IndexOf("fallback shader", StringComparison.OrdinalIgnoreCase) >= 0
                    || message.IndexOf("not supported on this GPU", StringComparison.OrdinalIgnoreCase) >= 0
                    || message.IndexOf("All subshaders removed", StringComparison.OrdinalIgnoreCase) >= 0
                    || message.StartsWith("ERROR: Shader", StringComparison.OrdinalIgnoreCase)
                    || message.StartsWith("WARNING: Shader", StringComparison.OrdinalIgnoreCase)
                    || message.StartsWith("Shader '", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            if (message.StartsWith("Unloading ", StringComparison.Ordinal)
                && (message.IndexOf("Unused Serialized files", StringComparison.OrdinalIgnoreCase) >= 0
                    || message.IndexOf("unused Assets to reduce memory usage", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return true;
            }

            if (message.StartsWith("Total:", StringComparison.Ordinal)
                && message.IndexOf("FindLiveObjects:", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (message.IndexOf("Forcing GfxDevice: Null", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("NullGfxDevice", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("graphics device is Null", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
        }
    }

    internal sealed class UnityConsoleWarningFilterHandler : ILogHandler
    {
        private readonly ILogHandler defaultHandler;

        public UnityConsoleWarningFilterHandler(ILogHandler defaultHandler)
        {
            this.defaultHandler = defaultHandler;
        }

        public bool Enabled { get; set; }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            if (Enabled)
            {
                string message = SafeFormat(format, args);
                if (UnityConsoleNoiseMatcher.ShouldSuppressLogHandlerMessage(logType, message))
                {
                    return;
                }
            }

            defaultHandler.LogFormat(logType, context, format, args);
        }

        public void LogException(Exception exception, UnityEngine.Object context)
        {
            if (Enabled)
            {
                return;
            }

            defaultHandler.LogException(exception, context);
        }

        private static string SafeFormat(string format, object[] args)
        {
            if (string.IsNullOrEmpty(format))
            {
                return string.Empty;
            }

            if (args == null || args.Length == 0)
            {
                return format;
            }

            try
            {
                return string.Format(format, args);
            }
            catch (FormatException)
            {
                return format;
            }
        }
    }

    internal sealed class FilteredConsoleWriter : TextWriter
    {
        private readonly TextWriter inner;
        private readonly StringBuilder lineBuffer = new StringBuilder();

        public FilteredConsoleWriter(TextWriter inner)
        {
            this.inner = inner;
        }

        public bool Enabled { get; set; } = true;

        public override Encoding Encoding => inner.Encoding;

        public override IFormatProvider FormatProvider => inner.FormatProvider;

        public override string NewLine
        {
            get => inner.NewLine;
            set => inner.NewLine = value;
        }

        public override void Write(char value)
        {
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
            if (!string.IsNullOrEmpty(value))
            {
                Write(value);
            }

            FlushBufferedLine();
        }

        public override void WriteLine()
        {
            FlushBufferedLine();
        }

        public override void Flush()
        {
            FlushBufferedLine();
            inner.Flush();
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
                inner.WriteLine(line);
            }
        }
    }

    internal static class UnityConsoleWarningFilter
    {
        public const string HarmonyId = "com.rocketmodfix.unturned.unity-console-warning-filter";
        private static UnityConsoleWarningFilterHandler? handler;
        private static FilteredConsoleWriter? stdoutWriter;
        private static FilteredConsoleWriter? stderrWriter;
        private static bool? lastApplied;
        private static Harmony? harmony;
        private static bool harmonyInstalled;
        private static bool loggedEnabledMessage;

        public static bool IsEnabled => !lastApplied.HasValue || lastApplied.Value;

        public static void Install()
        {
            InstallLogHandler();
            InstallHarmony();
            EnsureConsoleStreamsWrapped();
            Apply(true);
        }

        public static void TryApplyEarlyFromConfigFile()
        {
            string serverId = Dedicator.serverID;
            if (string.IsNullOrEmpty(serverId))
            {
                Apply(true);
                return;
            }

            Apply(ReadEnabledFlagFromConfig(serverId));
        }

        public static void ApplyFromSettings(bool enabled)
        {
            Apply(enabled);
        }

        internal static void EnsureConsoleStreamsWrapped()
        {
            WrapStream(ref stdoutWriter, Console.Out, Console.SetOut);
            WrapStream(ref stderrWriter, Console.Error, Console.SetError);
        }

        private static void WrapStream(ref FilteredConsoleWriter? holder, TextWriter current, Action<TextWriter> setter)
        {
            if (current is FilteredConsoleWriter existing)
            {
                holder ??= existing;
                return;
            }

            holder = new FilteredConsoleWriter(current);
            if (lastApplied.HasValue)
            {
                holder.Enabled = lastApplied.Value;
            }

            setter(holder);
        }

        private static void InstallLogHandler()
        {
            ILogHandler current = Debug.unityLogger.logHandler;
            if (current is UnityConsoleWarningFilterHandler existing)
            {
                handler = existing;
                return;
            }

            handler = new UnityConsoleWarningFilterHandler(current);
            Debug.unityLogger.logHandler = handler;
        }

        private static void InstallHarmony()
        {
            if (harmonyInstalled)
            {
                return;
            }

            harmonyInstalled = true;
            harmony = new Harmony(HarmonyId);
            harmony.CreateClassProcessor(typeof(ConsoleWriterProxyWriteLinePatch)).Patch();
            harmony.CreateClassProcessor(typeof(ConsoleWriterProxyWriteCharPatch)).Patch();

            MethodInfo? postfix = AccessTools.Method(typeof(UnityConsoleWarningFilter), nameof(ConsoleOutputRedirectorPostfix));
            MethodInfo? target = AccessTools.Method(typeof(ConsoleOutputRedirector), nameof(ConsoleOutputRedirector.enable));
            if (postfix != null && target != null)
            {
                harmony.Patch(target, postfix: new HarmonyMethod(postfix));
            }
        }

        private static void ConsoleOutputRedirectorPostfix()
        {
            EnsureConsoleStreamsWrapped();
        }

        private static void Apply(bool enabled)
        {
            InstallLogHandler();
            EnsureConsoleStreamsWrapped();

            bool stateChanged = !lastApplied.HasValue || lastApplied.Value != enabled;
            lastApplied = enabled;
            handler!.Enabled = enabled;
            stdoutWriter!.Enabled = enabled;
            stderrWriter!.Enabled = enabled;

            if (enabled && stateChanged && !loggedEnabledMessage)
            {
                TryLogEnabledMessage();
            }
        }

        internal static void TryLogEnabledMessage()
        {
            try
            {
                if (Dedicator.commandWindow != null)
                {
                    CommandWindow.Log("Unity console warning filter enabled (Rocket.Unturned.config.xml).");
                    loggedEnabledMessage = true;
                }
            }
            catch
            {
            }
        }

        private static bool ReadEnabledFlagFromConfig(string serverId)
        {
            try
            {
                string path = Path.Combine(ReadWrite.PATH, "Servers", serverId, "Rocket", Environment.SettingsFile);
                if (!File.Exists(path))
                {
                    return true;
                }

                string xml = File.ReadAllText(path);
                if (ContainsXmlBool(xml, "SuppressUnityConsoleWarnings", false)
                    || ContainsXmlBool(xml, "SuppressHeadlessGraphicsLogs", false))
                {
                    return false;
                }

                if (ContainsXmlBool(xml, "SuppressUnityConsoleWarnings", true)
                    || ContainsXmlBool(xml, "SuppressHeadlessGraphicsLogs", true))
                {
                    return true;
                }

                return true;
            }
            catch
            {
                return true;
            }
        }

        private static bool ContainsXmlBool(string xml, string elementName, bool value)
        {
            string open = "<" + elementName + ">" + (value ? "true" : "false") + "</" + elementName + ">";
            return xml.IndexOf(open, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}

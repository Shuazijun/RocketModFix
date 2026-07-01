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

            if (HasCommandWindowTimestamp(message))
            {
                return false;
            }

            return true;
        }

        public static bool HasCommandWindowTimestamp(string message)
        {
            return message.Length >= 21
                && message[0] == '['
                && char.IsDigit(message[1])
                && message.IndexOf("] ", 10, StringComparison.Ordinal) > 0;
        }

        public static bool ShouldSuppressLogHandlerMessage(LogType logType, string message)
        {
            return true;
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
        private static FilteringStreamWriter? redirectorStdoutWriter;

        public static bool IsEnabled => !lastApplied.HasValue || lastApplied.Value;

        public static void Install()
        {
            InstallLogHandler();
            InstallHarmony();
            EnsureConsoleStreamsWrapped();
            TryWrapActiveConsoleOutProxy();
            Apply(true);
        }

        internal static void WrapRedirectorWriters(ConsoleOutputRedirector redirector)
        {
            ConsoleWriterProxy? proxy = Traverse.Create(redirector).Field<ConsoleWriterProxy>("proxyWriter").Value;
            if (proxy != null)
            {
                WrapConsoleWriterProxy(proxy);
            }
        }

        internal static void TryWrapActiveConsoleOutProxy()
        {
            if (Console.Out is ConsoleWriterProxy proxy)
            {
                WrapConsoleWriterProxy(proxy);
            }
        }

        private static void WrapConsoleWriterProxy(ConsoleWriterProxy proxy)
        {
            Traverse proxyTraverse = Traverse.Create(proxy);
            StreamWriter? customWriter = proxyTraverse.Field<StreamWriter>("customWriter").Value;
            if (customWriter != null && customWriter is not FilteringStreamWriter)
            {
                redirectorStdoutWriter = new FilteringStreamWriter(customWriter.BaseStream, customWriter.Encoding)
                {
                    Enabled = IsEnabled
                };
                proxyTraverse.Field("customWriter").SetValue(redirectorStdoutWriter);
            }
            else if (customWriter is FilteringStreamWriter existingStdout)
            {
                redirectorStdoutWriter = existingStdout;
            }

            TextWriter? defaultWriter = proxyTraverse.Field<TextWriter>("defaultConsoleWriter").Value;
            if (defaultWriter != null && defaultWriter is not FilteredConsoleWriter)
            {
                FilteredConsoleWriter wrapped = new FilteredConsoleWriter(defaultWriter);
                if (lastApplied.HasValue)
                {
                    wrapped.Enabled = lastApplied.Value;
                }

                proxyTraverse.Field("defaultConsoleWriter").SetValue(wrapped);
            }
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
            harmony.CreateClassProcessor(typeof(ConsoleOutputRedirectorEnablePatch)).Patch();
        }

        private static void Apply(bool enabled)
        {
            InstallLogHandler();
            EnsureConsoleStreamsWrapped();
            TryWrapActiveConsoleOutProxy();

            bool stateChanged = !lastApplied.HasValue || lastApplied.Value != enabled;
            lastApplied = enabled;
            handler!.Enabled = enabled;
            stdoutWriter!.Enabled = enabled;
            stderrWriter!.Enabled = enabled;
            if (redirectorStdoutWriter != null)
            {
                redirectorStdoutWriter.Enabled = enabled;
            }

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

using HarmonyLib;
using SDG.Unturned;
using System;
using System.Collections.Generic;
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

            foreach (char character in value!)
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

        internal static bool IsRedirectorStdoutFiltered()
        {
            return redirectorStdoutWriter != null && redirectorStdoutWriter.Enabled;
        }

        internal static bool IsStdoutFilterWrapped()
        {
            return ConsoleTextWriterHelper.ContainsFilteredWriter(Console.Out);
        }

        public static void Install()
        {
            ModuleRuntimeDiagnostics.MarkFilterInstallCalled();
            InstallLogHandler();
            InstallHarmony();
            EnsureConsoleStreamsWrapped();
            TryWrapAllOutputRedirectors();
            TryWrapActiveConsoleOutProxy();
            EnsureLevelLoadedHook();
            Apply(true);
        }

        internal static void WrapRedirectorWriters(ConsoleOutputRedirector redirector)
        {
            Traverse redirectorTraverse = Traverse.Create(redirector);
            StreamWriter? standardWriter = redirectorTraverse.Field<StreamWriter>("standardOutputWriter").Value;
            if (standardWriter != null && standardWriter is not FilteringStreamWriter)
            {
                redirectorStdoutWriter = new FilteringStreamWriter(standardWriter.BaseStream, standardWriter.Encoding)
                {
                    Enabled = IsEnabled
                };
                redirectorTraverse.Field("standardOutputWriter").SetValue(redirectorStdoutWriter);
                if (ConsoleTextWriterHelper.ReferencesWriter(Console.Out, standardWriter))
                {
                    Console.SetOut(redirectorStdoutWriter);
                }
            }
            else if (standardWriter is FilteringStreamWriter existingStdout)
            {
                redirectorStdoutWriter = existingStdout;
                existingStdout.Enabled = IsEnabled;
            }

            ConsoleWriterProxy? proxy = redirectorTraverse.Field<ConsoleWriterProxy>("proxyWriter").Value;
            if (proxy != null)
            {
                WrapConsoleWriterProxy(proxy);
            }
        }

        internal static void TryWrapAllOutputRedirectors()
        {
            try
            {
                if (Dedicator.commandWindow == null)
                {
                    return;
                }

                List<ICommandInputOutput>? handlers = Traverse.Create(Dedicator.commandWindow)
                    .Field<List<ICommandInputOutput>>("ioHandlers")
                    .Value;
                if (handlers == null)
                {
                    return;
                }

                foreach (ICommandInputOutput handler in handlers)
                {
                    if (handler is not ConsoleInputOutputBase consoleHandler)
                    {
                        continue;
                    }

                    ConsoleOutputRedirector? redirector = Traverse.Create(consoleHandler)
                        .Field<ConsoleOutputRedirector>("outputRedirector")
                        .Value;
                    if (redirector != null)
                    {
                        WrapRedirectorWriters(redirector);
                    }
                }
            }
            catch
            {
            }
        }

        private static void EnsureLevelLoadedHook()
        {
            if (levelLoadedHooked)
            {
                return;
            }

            levelLoadedHooked = true;
            Level.onLevelLoaded += OnLevelLoadedRefresh;
        }

        private static bool levelLoadedHooked;

        private static void OnLevelLoadedRefresh(int level)
        {
            InstallLogHandler();
            EnsureConsoleStreamsWrapped();
            TryWrapAllOutputRedirectors();
            TryWrapActiveConsoleOutProxy();
            if (lastApplied.HasValue)
            {
                Apply(lastApplied.Value);
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
            if (ConsoleTextWriterHelper.ContainsFilteredWriter(current))
            {
                TextWriter unwrapped = ConsoleTextWriterHelper.Unwrap(current);
                if (unwrapped is FilteredConsoleWriter existing)
                {
                    holder ??= existing;
                    return;
                }
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

            try
            {
                harmony = new Harmony(HarmonyId);
                PatchConsoleFilterMethod(
                    "ConsoleWriterProxy.WriteLine",
                    typeof(ConsoleWriterProxy),
                    nameof(ConsoleWriterProxy.WriteLine),
                    new[] { typeof(string) },
                    typeof(ConsoleWriterProxyWriteLinePatch),
                    "Prefix",
                    postfix: false);
                PatchConsoleFilterMethod(
                    "ConsoleWriterProxy.Write",
                    typeof(ConsoleWriterProxy),
                    nameof(ConsoleWriterProxy.Write),
                    new[] { typeof(char) },
                    typeof(ConsoleWriterProxyWriteCharPatch),
                    "Prefix",
                    postfix: false);
                PatchConsoleFilterMethod(
                    "ConsoleOutputRedirector.enable",
                    typeof(ConsoleOutputRedirector),
                    nameof(ConsoleOutputRedirector.enable),
                    new[] { typeof(bool) },
                    typeof(ConsoleOutputRedirectorEnablePatch),
                    "Postfix",
                    postfix: true);
                PatchConsoleFilterMethod(
                    "ConsoleInputOutputBase.initialize",
                    typeof(ConsoleInputOutputBase),
                    nameof(ConsoleInputOutputBase.initialize),
                    new[] { typeof(CommandWindow) },
                    typeof(ConsoleInputOutputBaseInitializePatch),
                    "Postfix",
                    postfix: true);
                harmonyInstalled = true;
            }
            catch (Exception ex)
            {
                ModuleRuntimeDiagnostics.RecordError("InstallHarmony: " + ex.Message);
            }
        }

        private static void PatchConsoleFilterMethod(
            string label,
            Type targetType,
            string targetMethod,
            Type[] argumentTypes,
            Type patchType,
            string patchMethodName,
            bool postfix)
        {
            MethodInfo? method = AccessTools.Method(targetType, targetMethod, argumentTypes);
            if (method == null)
            {
                ModuleRuntimeDiagnostics.RecordPatchResult(label, targetFound: false, patched: false);
                ModuleRuntimeDiagnostics.RecordError(label + ": target method not found");
                return;
            }

            MethodInfo? patch = AccessTools.Method(patchType, patchMethodName);
            if (patch == null)
            {
                ModuleRuntimeDiagnostics.RecordPatchResult(label, targetFound: true, patched: false, "patch method missing");
                ModuleRuntimeDiagnostics.RecordError(label + ": patch method not found");
                return;
            }

            try
            {
                if (postfix)
                {
                    harmony!.Patch(method, postfix: new HarmonyMethod(patch));
                }
                else
                {
                    harmony!.Patch(method, prefix: new HarmonyMethod(patch));
                }

                bool applied = Harmony.GetPatchInfo(method) != null;
                ModuleRuntimeDiagnostics.RecordPatchResult(label, targetFound: true, patched: applied);
            }
            catch (Exception ex)
            {
                ModuleRuntimeDiagnostics.RecordPatchResult(label, targetFound: true, patched: false, ex.Message);
                ModuleRuntimeDiagnostics.RecordError(label + ": " + ex.Message);
            }
        }

        private static void Apply(bool enabled)
        {
            InstallLogHandler();
            EnsureConsoleStreamsWrapped();
            TryWrapAllOutputRedirectors();
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
                bool hasNewKey = xml.IndexOf("<SuppressUnityConsoleWarnings>", StringComparison.OrdinalIgnoreCase) >= 0;
                if (hasNewKey)
                {
                    if (UnturnedSettingsConfigHelper.ContainsXmlBool(xml, "SuppressUnityConsoleWarnings", false))
                    {
                        return false;
                    }

                    if (UnturnedSettingsConfigHelper.ContainsXmlBool(xml, "SuppressUnityConsoleWarnings", true))
                    {
                        return true;
                    }

                    return true;
                }

                if (UnturnedSettingsConfigHelper.ContainsXmlBool(xml, "SuppressHeadlessGraphicsLogs", false))
                {
                    return false;
                }

                if (UnturnedSettingsConfigHelper.ContainsXmlBool(xml, "SuppressHeadlessGraphicsLogs", true))
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
            return UnturnedSettingsConfigHelper.ContainsXmlBool(xml, elementName, value);
        }
    }
}

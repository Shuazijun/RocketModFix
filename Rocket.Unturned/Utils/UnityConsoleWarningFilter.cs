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
            if (logType == LogType.Error || logType == LogType.Assert || logType == LogType.Exception)
            {
                return ShouldSuppressMessage(message);
            }

            return true;
        }

        private static bool ShouldSuppressMessage(string message)
        {
            if (message.StartsWith("WARNING:", StringComparison.OrdinalIgnoreCase))
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

            if (message.IndexOf("The animation state", StringComparison.OrdinalIgnoreCase) >= 0
                && message.IndexOf("could not be played", StringComparison.OrdinalIgnoreCase) >= 0)
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
            inner.Write(value);
        }

        public override void Write(string? value)
        {
            if (value != null && Enabled && UnityConsoleNoiseMatcher.ShouldSuppressStdoutLine(value))
            {
                return;
            }

            inner.Write(value);
        }

        public override void WriteLine(string? value)
        {
            if (value != null && Enabled && UnityConsoleNoiseMatcher.ShouldSuppressStdoutLine(value))
            {
                return;
            }

            inner.WriteLine(value);
        }

        public override void WriteLine()
        {
            inner.WriteLine();
        }

        public override void Flush()
        {
            inner.Flush();
        }
    }

    internal static class UnityConsoleWarningFilter
    {
        public const string HarmonyId = "com.rocketmodfix.unturned.unity-console-warning-filter";
        private static UnityConsoleWarningFilterHandler? handler;
        private static FilteredConsoleWriter? consoleWriter;
        private static bool? lastApplied;
        private static Harmony? harmony;
        private static bool harmonyInstalled;

        public static void Install()
        {
            InstallLogHandler();
            InstallHarmony();
            EnsureConsoleOutWrapped();
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

        internal static void EnsureConsoleOutWrapped()
        {
            TextWriter current = Console.Out;
            if (current is FilteredConsoleWriter existing)
            {
                if (consoleWriter == null)
                {
                    consoleWriter = existing;
                }

                return;
            }

            consoleWriter = new FilteredConsoleWriter(current);
            if (lastApplied.HasValue)
            {
                consoleWriter.Enabled = lastApplied.Value;
            }

            Console.SetOut(consoleWriter);
        }

        private static void InstallLogHandler()
        {
            if (handler != null)
            {
                return;
            }

            handler = new UnityConsoleWarningFilterHandler(Debug.unityLogger.logHandler);
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
            MethodInfo? postfix = AccessTools.Method(typeof(UnityConsoleWarningFilter), nameof(ConsoleOutputRedirectorPostfix));
            MethodInfo? target = AccessTools.Method(typeof(ConsoleOutputRedirector), nameof(ConsoleOutputRedirector.enable));
            if (postfix != null && target != null)
            {
                harmony.Patch(target, postfix: new HarmonyMethod(postfix));
            }
        }

        private static void ConsoleOutputRedirectorPostfix()
        {
            EnsureConsoleOutWrapped();
        }

        private static void Apply(bool enabled)
        {
            InstallLogHandler();
            EnsureConsoleOutWrapped();

            if (lastApplied.HasValue && lastApplied.Value == enabled)
            {
                handler!.Enabled = enabled;
                consoleWriter!.Enabled = enabled;
                return;
            }

            lastApplied = enabled;
            handler!.Enabled = enabled;
            consoleWriter!.Enabled = enabled;

            if (enabled)
            {
                CommandWindow.Log("Unity console warning filter enabled (Rocket.Unturned.config.xml).");
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

using SDG.Unturned;
using System;
using System.IO;
using UnityEngine;

namespace Rocket.Unturned.Utils
{
    internal sealed class HeadlessLogFilterHandler : ILogHandler
    {
        private readonly ILogHandler defaultHandler;

        public HeadlessLogFilterHandler(ILogHandler defaultHandler)
        {
            this.defaultHandler = defaultHandler;
        }

        public bool Enabled { get; set; }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            if (!Enabled)
            {
                defaultHandler.LogFormat(logType, context, format, args);
                return;
            }

            string message = format;
            if (args != null && args.Length > 0)
            {
                try
                {
                    message = string.Format(format, args);
                }
                catch (FormatException)
                {
                    message = format;
                }
            }

            if (ShouldSuppress(message))
            {
                return;
            }

            defaultHandler.LogFormat(logType, context, format, args);
        }

        public void LogException(Exception exception, UnityEngine.Object context)
        {
            defaultHandler.LogException(exception, context);
        }

        private static bool ShouldSuppress(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return false;
            }

            if (message.StartsWith("Shader '", StringComparison.Ordinal))
            {
                if (message.IndexOf("fallback shader", StringComparison.OrdinalIgnoreCase) >= 0
                    || message.IndexOf("not found", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            if (message.IndexOf("ERROR: Shader", StringComparison.Ordinal) >= 0
                || message.IndexOf("WARNING: Shader", StringComparison.Ordinal) >= 0)
            {
                return true;
            }

            if (message.IndexOf("shader is not supported on this GPU", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("All subshaders removed", StringComparison.Ordinal) >= 0
                || message.IndexOf("#pragma only_renderers", StringComparison.Ordinal) >= 0
                || message.IndexOf("forgotten turning Fallback off", StringComparison.Ordinal) >= 0)
            {
                return true;
            }

            if (message.IndexOf("Forcing GfxDevice: Null", StringComparison.Ordinal) >= 0
                || message.IndexOf("NullGfxDevice", StringComparison.Ordinal) >= 0
                || message.IndexOf("Renderer: Null Device", StringComparison.Ordinal) >= 0
                || message.IndexOf("graphics device is Null", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
        }
    }

    internal static class HeadlessLogFilter
    {
        private static HeadlessLogFilterHandler? handler;
        private static bool? lastApplied;

        public static void Install()
        {
            if (handler != null)
            {
                return;
            }

            handler = new HeadlessLogFilterHandler(Debug.unityLogger.logHandler);
            Debug.unityLogger.logHandler = handler;
        }

        public static void TryApplyEarlyFromConfigFile()
        {
            string serverId = Dedicator.serverID;
            if (string.IsNullOrEmpty(serverId))
            {
                return;
            }

            Apply(ReadSuppressFlagFromConfig(serverId));
        }

        public static void ApplyFromSettings(bool enabled)
        {
            Apply(enabled);
        }

        private static void Apply(bool enabled)
        {
            Install();

            if (lastApplied.HasValue && lastApplied.Value == enabled)
            {
                handler!.Enabled = enabled;
                return;
            }

            lastApplied = enabled;
            handler!.Enabled = enabled;

            if (enabled)
            {
                CommandWindow.Log("Headless graphics log suppression enabled (Rocket.Unturned.config.xml).");
            }
        }

        private static bool ReadSuppressFlagFromConfig(string serverId)
        {
            try
            {
                string path = Path.Combine(ReadWrite.PATH, "Servers", serverId, "Rocket", Environment.SettingsFile);
                if (!File.Exists(path))
                {
                    return false;
                }

                string xml = File.ReadAllText(path);
                return xml.IndexOf("<SuppressHeadlessGraphicsLogs>true</SuppressHeadlessGraphicsLogs>", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch
            {
                return false;
            }
        }
    }
}

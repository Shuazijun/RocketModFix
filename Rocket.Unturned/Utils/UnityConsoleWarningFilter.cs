using SDG.Unturned;
using System;
using System.IO;
using UnityEngine;

namespace Rocket.Unturned.Utils
{
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
            if (Enabled && !ShouldPassThrough(logType))
            {
                return;
            }

            defaultHandler.LogFormat(logType, context, format, args);
        }

        public void LogException(Exception exception, UnityEngine.Object context)
        {
            defaultHandler.LogException(exception, context);
        }

        private static bool ShouldPassThrough(LogType logType)
        {
            return logType == LogType.Error
                || logType == LogType.Assert
                || logType == LogType.Exception;
        }
    }

    internal static class UnityConsoleWarningFilter
    {
        private static UnityConsoleWarningFilterHandler? handler;
        private static bool? lastApplied;

        public static void Install()
        {
            if (handler != null)
            {
                return;
            }

            handler = new UnityConsoleWarningFilterHandler(Debug.unityLogger.logHandler);
            Debug.unityLogger.logHandler = handler;
        }

        public static void TryApplyEarlyFromConfigFile()
        {
            string serverId = Dedicator.serverID;
            if (string.IsNullOrEmpty(serverId))
            {
                return;
            }

            Apply(ReadEnabledFlagFromConfig(serverId));
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

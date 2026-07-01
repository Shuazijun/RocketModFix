using Rocket.Unturned.Serialisation;
using SDG.Unturned;
using System;
using System.IO;

namespace Rocket.Unturned.Utils
{
    internal static class UnturnedSettingsConfigHelper
    {
        public static string GetSettingsFilePath()
        {
            string serverId = Dedicator.serverID;
            if (string.IsNullOrEmpty(serverId))
            {
                return string.Empty;
            }

            return Path.Combine(ReadWrite.PATH, "Servers", serverId, "Rocket", Environment.SettingsFile);
        }

        public static void NormalizeSuppressUnityConsoleWarnings(UnturnedSettings settings)
        {
            string path = GetSettingsFilePath();
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return;
            }

            string xml = File.ReadAllText(path);
            bool hasNewKey = xml.IndexOf("<SuppressUnityConsoleWarnings>", StringComparison.OrdinalIgnoreCase) >= 0;
            bool hasLegacyKey = xml.IndexOf("<SuppressHeadlessGraphicsLogs>", StringComparison.OrdinalIgnoreCase) >= 0;

            if (hasNewKey)
            {
                return;
            }

            if (ContainsXmlBool(xml, "SuppressHeadlessGraphicsLogs", true))
            {
                settings.SuppressUnityConsoleWarnings = true;
            }
            else if (ContainsXmlBool(xml, "SuppressHeadlessGraphicsLogs", false))
            {
                settings.SuppressUnityConsoleWarnings = false;
            }
        }

        public static string DescribeFilterConfig(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return "配置文件: (未找到，使用默认 true)";
            }

            string xml = File.ReadAllText(path);
            bool newTrue = ContainsXmlBool(xml, "SuppressUnityConsoleWarnings", true);
            bool newFalse = ContainsXmlBool(xml, "SuppressUnityConsoleWarnings", false);
            bool legacyTrue = ContainsXmlBool(xml, "SuppressHeadlessGraphicsLogs", true);
            bool legacyFalse = ContainsXmlBool(xml, "SuppressHeadlessGraphicsLogs", false);

            return $"配置文件: {path} | SuppressUnityConsoleWarnings={FormatTri(newTrue, newFalse)} | SuppressHeadlessGraphicsLogs(legacy)={FormatTri(legacyTrue, legacyFalse)}";
        }

        private static string FormatTri(bool isTrue, bool isFalse)
        {
            if (isTrue)
            {
                return "true";
            }

            if (isFalse)
            {
                return "false";
            }

            return "(未设置)";
        }

        internal static bool ContainsXmlBool(string xml, string elementName, bool value)
        {
            string open = "<" + elementName + ">" + (value ? "true" : "false") + "</" + elementName + ">";
            return xml.IndexOf(open, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}

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
    internal static class ModuleRuntimeDiagnostics
    {
        private static readonly List<string> installErrors = new List<string>();
        private static readonly Dictionary<string, string> patchResults = new Dictionary<string, string>();

        public static bool ModuleInitializerExecuted { get; set; }
        public static bool FilterInstallCalled { get; set; }
        public static DateTime? FilterInstallUtc { get; private set; }

        public static void MarkFilterInstallCalled()
        {
            FilterInstallCalled = true;
            FilterInstallUtc ??= DateTime.UtcNow;
        }

        public static void RecordPatchResult(string name, bool targetFound, bool patched, string? detail = null)
        {
            string status = !targetFound ? "target-missing" : (patched ? "ok" : "failed");
            if (!string.IsNullOrEmpty(detail))
            {
                status += " (" + detail + ")";
            }

            patchResults[name] = status;
        }

        public static void RecordError(string message)
        {
            installErrors.Add(message);
        }

        public static IEnumerable<string> GetReportLines()
        {
            UnityConsoleWarningFilter.EnsureConsoleStreamsWrapped();
            UnityConsoleWarningFilter.TryWrapAllOutputRedirectors();
            UnityConsoleWarningFilter.TryWrapActiveConsoleOutProxy();

            Assembly executing = Assembly.GetExecutingAssembly();
            string moduleDir = Path.GetDirectoryName(executing.Location) ?? "?";
            string harmonyPath = Path.Combine(moduleDir, "0Harmony.dll");

            yield return $"模块目录: {moduleDir}";
            yield return $"0Harmony.dll 存在: {File.Exists(harmonyPath)}";
            yield return $"HarmonyLib 已加载: {IsHarmonyLibLoaded(out string? harmonyVersion)} ({harmonyVersion ?? "n/a"})";
            yield return $"ModuleInitializer 已执行: {ModuleInitializerExecuted}";
            yield return $"过滤器 Install 已调用: {FilterInstallCalled} (UTC: {FilterInstallUtc:O})";
            yield return $"SuppressUnityConsoleWarnings: {UnityConsoleWarningFilter.IsEnabled}";
            yield return UnturnedSettingsConfigHelper.DescribeFilterConfig(UnturnedSettingsConfigHelper.GetSettingsFilePath());
            if (!UnityConsoleWarningFilter.IsEnabled)
            {
                yield return ">>> 过滤器已关闭：请在 Rocket.Unturned.config.xml 设置 <SuppressUnityConsoleWarnings>true</SuppressUnityConsoleWarnings>，并删除冲突的 <SuppressHeadlessGraphicsLogs>false</SuppressHeadlessGraphicsLogs>";
            }
            yield return $"ILogHandler 已包装: {Debug.unityLogger.logHandler is UnityConsoleWarningFilterHandler}";
            yield return $"Console.Out 类型: {Console.Out?.GetType().FullName ?? "null"}";
            yield return $"Console.Error 类型: {Console.Error?.GetType().FullName ?? "null"}";
            yield return $"stdout 已过滤包装: {UnityConsoleWarningFilter.IsStdoutFilterWrapped()} (Console.Out={Console.Out?.GetType().Name})";
            yield return $"redirector stdout 已过滤: {UnityConsoleWarningFilter.IsRedirectorStdoutFiltered()}";

            foreach (KeyValuePair<string, string> patch in patchResults)
            {
                yield return $"Harmony 补丁 [{patch.Key}]: {patch.Value}";
            }

            yield return $"ConsoleWriterProxy.WriteLine 已打补丁: {IsMethodPatched(typeof(ConsoleWriterProxy), nameof(ConsoleWriterProxy.WriteLine), new[] { typeof(string) }, UnityConsoleWarningFilter.HarmonyId)}";
            yield return $"ConsoleWriterProxy.Write 已打补丁: {IsMethodPatched(typeof(ConsoleWriterProxy), nameof(ConsoleWriterProxy.Write), new[] { typeof(char) }, UnityConsoleWarningFilter.HarmonyId)}";
            yield return $"ConsoleOutputRedirector.enable 已打补丁: {IsMethodPatched(typeof(ConsoleOutputRedirector), nameof(ConsoleOutputRedirector.enable), new[] { typeof(bool) }, UnityConsoleWarningFilter.HarmonyId)}";
            yield return $"CommandConsoleTimestamp 已安装: {CommandConsoleTimestamp.IsInstalled}";
            yield return $"CommandConsoleTimestamp 补丁数: {CommandConsoleTimestamp.PatchedMethodCount}";

            if (installErrors.Count == 0)
            {
                yield return "安装错误: (无)";
            }
            else
            {
                yield return $"安装错误 ({installErrors.Count}):";
                foreach (string error in installErrors)
                {
                    yield return "  - " + error;
                }
            }
        }

        private static bool IsHarmonyLibLoaded(out string? version)
        {
            version = null;
            try
            {
                Assembly harmonyAssembly = typeof(Harmony).Assembly;
                version = harmonyAssembly.GetName().Version?.ToString();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsMethodPatched(Type type, string methodName, Type[] args, string harmonyId)
        {
            try
            {
                MethodInfo? method = AccessTools.Method(type, methodName, args);
                if (method == null)
                {
                    return false;
                }

                HarmonyLib.Patches? info = Harmony.GetPatchInfo(method);
                if (info == null)
                {
                    return false;
                }

                foreach (Patch patch in info.Prefixes)
                {
                    if (patch.owner == harmonyId)
                    {
                        return true;
                    }
                }

                foreach (Patch patch in info.Postfixes)
                {
                    if (patch.owner == harmonyId)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}

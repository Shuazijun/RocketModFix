using Rocket.Core;
using Rocket.Unturned.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rocket.Unturned.Utils
{
    internal static class ConfigOptionsReloader
    {
        private static readonly string[] AllModules =
        {
            "unturned",
            "tpa",
            "homes",
            "warps",
            "autosave",
            "aliases",
            "translation",
            "rocket",
            "permissions"
        };

        private static readonly Dictionary<string, string> ModuleAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "unturned", "unturned" },
            { "main", "unturned" },
            { "tpa", "tpa" },
            { "homes", "homes" },
            { "home", "homes" },
            { "warps", "warps" },
            { "warp", "warps" },
            { "autosave", "autosave" },
            { "aliases", "aliases" },
            { "alias", "aliases" },
            { "commandalias", "aliases" },
            { "translation", "translation" },
            { "i18n", "translation" },
            { "lang", "translation" },
            { "rocket", "rocket" },
            { "core", "rocket" },
            { "permissions", "permissions" },
            { "perms", "permissions" },
            { "permission", "permissions" }
        };

        public static string FormatModuleList()
        {
            return string.Join(", ", AllModules.Concat(new[] { "all" }).ToArray());
        }

        public static bool TryNormalizeModules(IEnumerable<string> inputs, out HashSet<string> modules, out List<string> unknown)
        {
            modules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            unknown = new List<string>();

            foreach (string raw in inputs)
            {
                if (string.IsNullOrWhiteSpace(raw))
                {
                    continue;
                }

                string input = raw.Trim();
                if (input.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (string module in AllModules)
                    {
                        modules.Add(module);
                    }

                    continue;
                }

                if (ModuleAliases.TryGetValue(input, out string? normalized))
                {
                    modules.Add(normalized);
                    continue;
                }

                unknown.Add(input);
            }

            return unknown.Count == 0;
        }

        public static IReadOnlyList<string> ReloadAll()
        {
            return ReloadModules(AllModules);
        }

        public static IReadOnlyList<string> ReloadModules(IEnumerable<string> modules)
        {
            HashSet<string> moduleSet = modules is HashSet<string> existing
                ? existing
                : new HashSet<string>(modules, StringComparer.OrdinalIgnoreCase);

            List<string> reloaded = new List<string>();
            bool teleportSettingsChanged = false;

            if (moduleSet.Contains("rocket"))
            {
                R.Settings.Load();
                R.Translation.Load();
                ApplyMaxFrames();
                reloaded.Add("rocket");
            }

            if (moduleSet.Contains("permissions"))
            {
                R.Permissions.Reload();
                reloaded.Add("permissions");
            }

            if (moduleSet.Contains("translation"))
            {
                U.Translation.Load();
                reloaded.Add("translation");
            }

            if (moduleSet.Contains("unturned"))
            {
                U.Settings.Load();
                HeadlessLogFilter.ApplyFromSettings(U.Settings.Instance.SuppressHeadlessGraphicsLogs);
                reloaded.Add("unturned");
            }

            if (moduleSet.Contains("tpa"))
            {
                U.TpaSettings.Load();
                reloaded.Add("tpa");
                teleportSettingsChanged = true;
            }

            if (moduleSet.Contains("homes"))
            {
                U.HomeSettings.Load();
                reloaded.Add("homes");
                teleportSettingsChanged = true;
            }

            if (moduleSet.Contains("warps"))
            {
                U.WarpSettings.Load();
                reloaded.Add("warps");
                teleportSettingsChanged = true;
            }

            if (moduleSet.Contains("autosave"))
            {
                U.AutoSaveSettings.Load();
                reloaded.Add("autosave");
            }

            if (moduleSet.Contains("aliases"))
            {
                U.CommandAliasSettings.Load();
                CommandAliasResolver.Load(U.CommandAliasSettings.Instance);
                reloaded.Add("aliases");
            }

            if (teleportSettingsChanged)
            {
                U.Instance.ApplyTeleportServices();
            }

            if (moduleSet.Contains("autosave"))
            {
                AutomaticSaveWatchdog.Instance?.ApplySettings();
            }

            Core.Logging.Logger.Log($"[ReloadOptions] Reloaded: {string.Join(", ", reloaded.ToArray())}");
            return reloaded;
        }

        private static void ApplyMaxFrames()
        {
            int maxFrames = R.Settings.Instance.MaxFrames;
            if (maxFrames < 10 && maxFrames != -1)
            {
                maxFrames = 10;
            }

            Application.targetFrameRate = maxFrames;
        }
    }
}

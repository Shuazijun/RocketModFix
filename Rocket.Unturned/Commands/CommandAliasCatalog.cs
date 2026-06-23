using Rocket.API;
using Rocket.Core;
using Rocket.Core.Assets;
using Rocket.Core.Commands;
using Rocket.Core.Logging;
using Rocket.Unturned.Serialisation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rocket.Unturned.Commands
{
    internal static class CommandAliasCatalog
    {
        private sealed class CanonicalCommand
        {
            public string Name = "";
            public string Class = "";
            public string Syntax = "";
            public string Permissions = "";
            public string AllowedCaller = "";
            public string Source = "";
        }

        public static void Sync(XMLFileAsset<CommandAliasSettings> settingsAsset)
        {
            if (settingsAsset?.Instance == null)
            {
                return;
            }

            List<CanonicalCommand> canonicalCommands = ScanCanonicalCommands();
            Dictionary<string, CommandAliasCommandEntry> existingByKey = settingsAsset.Instance.Commands
                .Where(entry => !string.IsNullOrWhiteSpace(entry.Name) && !string.IsNullOrWhiteSpace(entry.Class))
                .GroupBy(entry => BuildKey(entry.Name, entry.Class), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            List<CommandAliasCommandEntry> merged = new List<CommandAliasCommandEntry>();

            foreach (CanonicalCommand canonical in canonicalCommands.OrderBy(command => command.Name, StringComparer.OrdinalIgnoreCase))
            {
                string key = BuildKey(canonical.Name, canonical.Class);
                if (existingByKey.TryGetValue(key, out CommandAliasCommandEntry? existing))
                {
                    existing.Name = canonical.Name;
                    existing.Class = canonical.Class;
                    existing.Syntax = canonical.Syntax;
                    existing.Permissions = canonical.Permissions;
                    existing.AllowedCaller = canonical.AllowedCaller;
                    merged.Add(existing);
                    existingByKey.Remove(key);
                }
                else
                {
                    merged.Add(new CommandAliasCommandEntry
                    {
                        Name = canonical.Name,
                        Class = canonical.Class,
                        Syntax = canonical.Syntax,
                        Permissions = canonical.Permissions,
                        AllowedCaller = canonical.AllowedCaller,
                        Aliases = new List<CommandAliasAliasEntry>()
                    });
                }
            }

            foreach (CommandAliasCommandEntry orphan in existingByKey.Values)
            {
                Logger.LogWarning($"[CommandAlias] Command '{orphan.Name}' ({orphan.Class}) is no longer registered; aliases were dropped.");
            }

            settingsAsset.Instance.Commands = merged;
            settingsAsset.Save();
            CommandAliasResolver.Load(settingsAsset.Instance);
        }

        private static List<CanonicalCommand> ScanCanonicalCommands()
        {
            List<CanonicalCommand> results = new List<CanonicalCommand>();
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (R.Commands?.Commands == null)
            {
                return results;
            }

            foreach (RocketCommandManager.RegisteredRocketCommand registered in R.Commands.Commands)
            {
                IRocketCommand command = registered.Command;
                if (!string.Equals(registered.Name, command.Name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string className = registered.Type.FullName ?? command.GetType().FullName ?? "";
                string key = BuildKey(command.Name, className);
                if (!seen.Add(key))
                {
                    continue;
                }

                results.Add(new CanonicalCommand
                {
                    Name = command.Name,
                    Class = className,
                    Syntax = command.Syntax ?? "",
                    Permissions = string.Join(", ", command.Permissions ?? new List<string>()),
                    AllowedCaller = command.AllowedCaller.ToString(),
                    Source = ResolveSource(registered.Type.Assembly)
                });
            }

            return results;
        }

        private static string ResolveSource(System.Reflection.Assembly assembly)
        {
            string assemblyName = assembly.GetName().Name ?? "";
            if (assemblyName.Equals("Rocket.Unturned", StringComparison.OrdinalIgnoreCase))
            {
                return "Rocket.Unturned";
            }

            try
            {
                IRocketPlugin? plugin = R.Plugins.GetPlugins().FirstOrDefault(candidate =>
                    candidate != null && candidate.GetType().Assembly == assembly);
                if (plugin != null && !string.IsNullOrWhiteSpace(plugin.Name))
                {
                    return plugin.Name;
                }
            }
            catch
            {
                // ignore plugin lookup failures during early startup
            }

            return assemblyName;
        }

        private static string BuildKey(string name, string className)
        {
            return $"{name}|{className}";
        }
    }
}

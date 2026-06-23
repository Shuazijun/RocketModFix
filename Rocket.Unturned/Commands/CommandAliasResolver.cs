using Rocket.Core.Logging;
using Rocket.Unturned.Serialisation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Rocket.Unturned.Commands
{
    internal static class CommandAliasResolver
    {
        private sealed class AliasDefinition
        {
            public string ParentCommand = "";
            public string? Target;
            public string? ExpandsTo;
        }

        private static readonly Dictionary<string, AliasDefinition> aliasMap = new Dictionary<string, AliasDefinition>(StringComparer.OrdinalIgnoreCase);
        private static bool loaded;

        public static void Load(CommandAliasSettings settings)
        {
            aliasMap.Clear();
            loaded = false;

            if (!settings.Enabled)
            {
                return;
            }

            HashSet<string> usedAliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> commandNames = new HashSet<string>(
                settings.Commands.Select(command => command.Name),
                StringComparer.OrdinalIgnoreCase);

            foreach (CommandAliasCommandEntry commandEntry in settings.Commands)
            {
                if (string.IsNullOrWhiteSpace(commandEntry.Name))
                {
                    continue;
                }

                commandNames.Add(commandEntry.Name);

                foreach (CommandAliasAliasEntry aliasEntry in commandEntry.Aliases)
                {
                    string aliasText = aliasEntry.Value?.Trim() ?? "";
                    if (string.IsNullOrWhiteSpace(aliasText) || aliasText.Any(char.IsWhiteSpace))
                    {
                        Logger.LogWarning($"[CommandAlias] Ignoring invalid alias for command '{commandEntry.Name}'.");
                        continue;
                    }

                    if (commandNames.Contains(aliasText))
                    {
                        Logger.LogWarning($"[CommandAlias] Alias '{aliasText}' conflicts with a command name and was ignored.");
                        continue;
                    }

                    if (!usedAliases.Add(aliasText))
                    {
                        Logger.LogWarning($"[CommandAlias] Duplicate alias '{aliasText}' was ignored.");
                        continue;
                    }

                    string? expandsTo = string.IsNullOrWhiteSpace(aliasEntry.ExpandsTo) ? null : aliasEntry.ExpandsTo.Trim();
                    string? target = string.IsNullOrWhiteSpace(aliasEntry.Target) ? null : aliasEntry.Target.Trim();

                    if (expandsTo != null && target != null)
                    {
                        Logger.LogWarning($"[CommandAlias] Alias '{aliasText}' has both ExpandsTo and Target; using ExpandsTo.");
                        target = null;
                    }

                    aliasMap[aliasText] = new AliasDefinition
                    {
                        ParentCommand = commandEntry.Name,
                        Target = target,
                        ExpandsTo = expandsTo
                    };
                }
            }

            loaded = true;
            Logger.Log($"[CommandAlias] Loaded {aliasMap.Count} alias(es) for {settings.Commands.Count} command(s).");
        }

        public static string ExpandCommandLine(string commandLine)
        {
            if (!loaded || string.IsNullOrWhiteSpace(commandLine))
            {
                return commandLine;
            }

            string[] tokens = Tokenize(commandLine);
            if (tokens.Length == 0)
            {
                return commandLine;
            }

            if (!aliasMap.TryGetValue(tokens[0], out AliasDefinition definition))
            {
                return commandLine;
            }

            List<string> expandedTokens = BuildExpandedTokens(definition, tokens.Skip(1).ToArray());
            return string.Join(" ", expandedTokens);
        }

        public static string ResolvePermissionCommandName(string commandLine)
        {
            if (!loaded || string.IsNullOrWhiteSpace(commandLine))
            {
                return ExtractFirstToken(commandLine);
            }

            string[] tokens = Tokenize(commandLine);
            if (tokens.Length == 0)
            {
                return "";
            }

            if (!aliasMap.TryGetValue(tokens[0], out AliasDefinition definition))
            {
                return tokens[0];
            }

            if (!string.IsNullOrWhiteSpace(definition.ExpandsTo))
            {
                string[] expanded = definition.ExpandsTo!.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                return expanded.Length > 0 ? expanded[0] : definition.ParentCommand;
            }

            return definition.ParentCommand;
        }

        private static List<string> BuildExpandedTokens(AliasDefinition definition, string[] trailingTokens)
        {
            List<string> expandedTokens = new List<string>();

            if (!string.IsNullOrWhiteSpace(definition.ExpandsTo))
            {
                expandedTokens.AddRange(definition.ExpandsTo!.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            }
            else
            {
                expandedTokens.Add(definition.ParentCommand);
                if (!string.IsNullOrWhiteSpace(definition.Target))
                {
                    expandedTokens.AddRange(definition.Target!.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                }
            }

            expandedTokens.AddRange(trailingTokens);
            return expandedTokens;
        }

        private static string ExtractFirstToken(string commandLine)
        {
            string[] tokens = Tokenize(commandLine);
            return tokens.Length > 0 ? tokens[0] : "";
        }

        private static string[] Tokenize(string commandLine)
        {
            string trimmed = commandLine.TrimStart('/');
            return Regex.Matches(trimmed, @"[""](.+?)[""]|([^ ]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture)
                .Cast<Match>()
                .Select(match => match.Value.Trim('"').Trim())
                .Where(token => !string.IsNullOrEmpty(token))
                .ToArray();
        }
    }
}

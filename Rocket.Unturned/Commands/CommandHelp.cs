using Rocket.API;
using Rocket.Core;
using Rocket.Core.Commands;
using Rocket.Core.Plugins;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rocket.Unturned.Chat;

namespace Rocket.Unturned.Commands
{
    public class CommandHelp : IRocketCommand
    {
        private const int NameColumnWidth = 20;
        private const int DetailMaxLength = 96;

        private static readonly Assembly UnturnedAssembly = typeof(CommandHelp).Assembly;
        private static readonly Assembly CoreAssembly = typeof(RocketCommandManager).Assembly;

        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "help";

        public string Help => "Lists server commands or shows help for one command";

        public string Syntax => "[command]";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "rocket.help" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "[Vanilla]");
                foreach (Command vanilla in Commander.commands.OrderBy(c => c.command))
                {
                    SayHelpLine(caller, vanilla.command, FormatVanillaDetail(vanilla));
                }

                UnturnedChat.Say(caller, "---");
                UnturnedChat.Say(caller, "[Rocket]");
                foreach (IRocketCommand rocketCommand in GetRocketCommands().OrderBy(c => c.Name))
                {
                    if (string.Equals(rocketCommand.Name, Name, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    SayHelpLine(caller, rocketCommand.Name, FormatRocketDetail(rocketCommand));
                }
                UnturnedChat.Say(caller, "---");

                foreach (IRocketPlugin plugin in R.Plugins.GetPlugins())
                {
                    Assembly pluginAssembly = plugin.GetType().Assembly;
                    if (pluginAssembly == UnturnedAssembly || pluginAssembly == CoreAssembly)
                    {
                        continue;
                    }

                    IEnumerable<IRocketCommand> pluginCommands = GetCommandsFromAssembly(pluginAssembly).ToList();
                    if (!pluginCommands.Any())
                    {
                        continue;
                    }

                    UnturnedChat.Say(caller, "[" + pluginAssembly.GetName().Name + "]");
                    foreach (IRocketCommand pluginCommand in pluginCommands.OrderBy(c => c.Name))
                    {
                        SayHelpLine(caller, pluginCommand.Name, FormatRocketDetail(pluginCommand));
                    }
                    UnturnedChat.Say(caller, "---");
                }
            }
            else
            {
                IRocketCommand? cmd = R.Commands.Commands
                    .FirstOrDefault(c => string.Equals(c.Name, command[0], StringComparison.OrdinalIgnoreCase));
                if (cmd != null)
                {
                    string commandName = RocketCommandManager.GetCommandAssembly(cmd).GetName().Name + " / " + cmd.Name;
                    string detail = FormatRocketDetail(cmd);

                    UnturnedChat.Say(caller, "[" + commandName + "]");
                    if (!string.IsNullOrEmpty(detail))
                    {
                        UnturnedChat.Say(caller, cmd.Name + "\t\t" + detail);
                    }
                    else if (!string.IsNullOrWhiteSpace(cmd.Syntax))
                    {
                        UnturnedChat.Say(caller, cmd.Name + "\t\t" + cmd.Syntax);
                    }

                    if (!string.IsNullOrWhiteSpace(cmd.Help))
                    {
                        UnturnedChat.Say(caller, cmd.Help);
                    }
                }
                else
                {
                    Command? vanilla = Commander.commands
                        .FirstOrDefault(c => string.Equals(c.command, command[0], StringComparison.OrdinalIgnoreCase));
                    if (vanilla != null)
                    {
                        UnturnedChat.Say(caller, "[Vanilla / " + vanilla.command + "]");
                        string detail = FormatVanillaDetail(vanilla);
                        if (!string.IsNullOrEmpty(detail))
                        {
                            UnturnedChat.Say(caller, vanilla.command + "\t\t" + detail);
                        }

                        if (!string.IsNullOrWhiteSpace(vanilla.help))
                        {
                            UnturnedChat.Say(caller, vanilla.help);
                        }
                    }
                    else
                    {
                        UnturnedChat.Say(caller, U.Translate("command_not_found"));
                    }
                }
            }
        }

        private static IEnumerable<IRocketCommand> GetRocketCommands()
        {
            return R.Commands.Commands.Where(c =>
            {
                if (IsVanillaWrapper(c))
                {
                    return false;
                }

                Assembly assembly = RocketCommandManager.GetCommandAssembly(c);
                return assembly == CoreAssembly || assembly == UnturnedAssembly;
            });
        }

        private static bool IsVanillaWrapper(IRocketCommand command)
        {
            if (command is RocketCommandManager.RegisteredRocketCommand registered)
            {
                return registered.Command is UnturnedCommands.UnturnedVanillaCommand;
            }

            return command is UnturnedCommands.UnturnedVanillaCommand;
        }

        private static IEnumerable<IRocketCommand> GetCommandsFromAssembly(Assembly assembly)
        {
            return R.Commands.Commands.Where(c => RocketCommandManager.GetCommandAssembly(c) == assembly);
        }

        private static void SayHelpLine(IRocketPlayer caller, string commandName, string detail)
        {
            string line = commandName.ToLower().PadRight(NameColumnWidth, ' ');
            if (!string.IsNullOrWhiteSpace(detail))
            {
                line += detail;
            }

            UnturnedChat.Say(caller, line);
        }

        private static string FormatVanillaDetail(Command command)
        {
            string info = (command.info ?? "").Trim();
            string name = command.command.Trim();

            if (info.StartsWith("/" + name, StringComparison.OrdinalIgnoreCase))
            {
                info = info.Substring(name.Length + 1).TrimStart();
            }
            else if (info.StartsWith(name, StringComparison.OrdinalIgnoreCase))
            {
                info = info.Substring(name.Length).TrimStart('/', ' ');
            }
            else
            {
                info = info.TrimStart('/', ' ');
            }

            if (!string.IsNullOrWhiteSpace(info))
            {
                return Truncate(info.ToLowerInvariant());
            }

            return Truncate((command.help ?? "").Trim());
        }

        private static string FormatRocketDetail(IRocketCommand command)
        {
            string name = command.Name.Trim();
            string syntax = NormalizeSyntax(name, command.Syntax);
            if (!string.IsNullOrWhiteSpace(syntax))
            {
                return Truncate(syntax.ToLowerInvariant());
            }

            return Truncate((command.Help ?? "").Trim());
        }

        private static string NormalizeSyntax(string commandName, string? syntax)
        {
            string value = (syntax ?? "").Trim();
            if (value.Length == 0)
            {
                return "";
            }

            if (string.Equals(value, commandName, StringComparison.OrdinalIgnoreCase))
            {
                return "";
            }

            if (value.StartsWith("/" + commandName, StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(commandName.Length + 1).TrimStart();
            }
            else if (value.StartsWith(commandName + " ", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(commandName.Length).TrimStart();
            }

            return value;
        }

        private static string Truncate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }

            value = value.Replace('\r', ' ').Replace('\n', ' ').Trim();
            if (value.Length <= DetailMaxLength)
            {
                return value;
            }

            return value.Substring(0, DetailMaxLength - 3) + "...";
        }
    }
}

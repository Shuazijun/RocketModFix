using Rocket.API;
using Rocket.API.Extensions;
using Rocket.Core;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rocket.Unturned.Commands
{
    public class CommandP : IRocketCommand
    {
        private const int PermissionsPerLine = 8;

        public AllowedCaller AllowedCaller
        {
            get
            {
                return AllowedCaller.Both;
            }
        }

        public string Name
        {
            get { return "p"; }
        }

        public string Help
        {
            get { return "Sets a Rocket permission group of a specific player"; }
        }

        public string Syntax
        {
            get { return "<player> [group] | reload"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>() { "permissions" }; }
        }

        public List<string> Permissions
        {
            get { return new List<string>() { "rocket.p", "rocket.permissions" }; }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 1 && command[0].ToLower() == "reload" && caller.HasPermission("p.reload"))
            {
                R.Permissions.Reload();
                UnturnedChat.Say(caller, U.Translate("command_p_permissions_reload"));
                return;
            }

            if (command.Length == 0 && caller is not ConsolePlayer)
            {
                SayGroups(caller, "Your", caller);
                SayPermissions(caller, "Your", caller);
            }
            else if (command.Length == 1)
            {

                IRocketPlayer? player = command.GetUnturnedPlayerParameter(0);
                if (player == null) player = command.GetRocketPlayerParameter(0);
                if (player != null)
                {
                    SayGroups(caller, player.DisplayName + "'s", player);
                    SayPermissions(caller, player.DisplayName + "'s", player);
                }
                else
                {
                    UnturnedChat.Say(caller, U.Translate("command_generic_invalid_parameter"));
                    return;
                }
            }
            else if (command.Length == 3)
            {
                string? c = command.GetStringParameter(0)?.ToLower();

                IRocketPlayer? player = command.GetUnturnedPlayerParameter(1);
                if (player == null) player = command.GetRocketPlayerParameter(1);

                string? groupName = command.GetStringParameter(2);

                switch (c)
                {
                    case "add":
                        if (caller.HasPermission("p.add") && player != null && groupName != null)
                        {
                            switch (R.Permissions.AddPlayerToGroup(groupName, player))
                            {
                                case RocketPermissionsProviderResult.Success:
                                    UnturnedChat.Say(caller, U.Translate("command_p_group_player_added", player.DisplayName, groupName));
                                    return;
                                case RocketPermissionsProviderResult.DuplicateEntry:
                                    UnturnedChat.Say(caller, U.Translate("command_p_duplicate_entry", player.DisplayName, groupName));
                                    return;
                                case RocketPermissionsProviderResult.GroupNotFound:
                                    UnturnedChat.Say(caller, U.Translate("command_p_group_not_found", player.DisplayName, groupName));
                                    return;
                                case RocketPermissionsProviderResult.PlayerNotFound: // - does not work
                                    UnturnedChat.Say(caller, U.Translate("command_p_player_not_found", player.DisplayName, groupName));
                                    return;
                                default:
                                    UnturnedChat.Say(caller, U.Translate("command_p_unknown_error", player.DisplayName, groupName));
                                    return;
                            }
                        }
                        UnturnedChat.Say(caller, U.Translate("command_p_player_not_found", command[1], "group_not_found")); // the solution to the problem
                        return;
                    case "remove":
                        if (caller.HasPermission("p.remove") && player != null && groupName != null)
                        {
                            switch (Core.R.Permissions.RemovePlayerFromGroup(groupName, player))
                            {
                                case RocketPermissionsProviderResult.Success:
                                    UnturnedChat.Say(caller, U.Translate("command_p_group_player_removed", player.DisplayName, groupName));
                                    return;
                                case RocketPermissionsProviderResult.DuplicateEntry:
                                    UnturnedChat.Say(caller, U.Translate("command_p_duplicate_entry", player.DisplayName, groupName));
                                    return;
                                case RocketPermissionsProviderResult.GroupNotFound:
                                    UnturnedChat.Say(caller, U.Translate("command_p_group_not_found", player.DisplayName, groupName));
                                    return;
                                case RocketPermissionsProviderResult.PlayerNotFound: // - does not work
                                    UnturnedChat.Say(caller, U.Translate("command_p_player_not_found", player.DisplayName, groupName));
                                    return;
                                default:
                                    UnturnedChat.Say(caller, U.Translate("command_p_unknown_error", player.DisplayName, groupName));
                                    return;
                            }
                        }
                        UnturnedChat.Say(caller, U.Translate("command_p_player_not_found", command[1], "group_not_found")); // the solution to the problem
                        return;
                    default:
                        UnturnedChat.Say(caller, U.Translate("command_generic_invalid_parameter"));
                        throw new WrongUsageOfCommandException(caller, this);
                }
            }
            else
            {
                UnturnedChat.Say(caller, U.Translate("command_generic_invalid_parameter"));
                throw new WrongUsageOfCommandException(caller, this);
            }
        }

        private static void SayGroups(IRocketPlayer caller, string ownerLabel, IRocketPlayer target)
        {
            string[] groups = R.Permissions.GetGroups(target, true).Select(g => g.DisplayName).ToArray();
            if (groups.Length == 0)
            {
                UnturnedChat.Say(caller, U.Translate("command_p_groups_private", ownerLabel, "(none)"));
                return;
            }

            UnturnedChat.Say(caller, U.Translate("command_p_groups_private", ownerLabel, string.Join(", ", groups)));
        }

        private static void SayPermissions(IRocketPlayer caller, string ownerLabel, IRocketPlayer target)
        {
            List<string> permissions = R.Permissions.GetPermissions(target)
                .Select(p => p.Name + (p.Cooldown != 0 ? "(" + p.Cooldown + ")" : ""))
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (permissions.Count == 0)
            {
                UnturnedChat.Say(caller, U.Translate("command_p_permissions_private", ownerLabel, "(none)"));
                return;
            }

            UnturnedChat.Say(caller, U.Translate("command_p_permissions_private", ownerLabel, $"({permissions.Count})"));
            for (int i = 0; i < permissions.Count; i += PermissionsPerLine)
            {
                string chunk = string.Join(", ", permissions.Skip(i).Take(PermissionsPerLine));
                UnturnedChat.Say(caller, chunk);
            }
        }
    }
}

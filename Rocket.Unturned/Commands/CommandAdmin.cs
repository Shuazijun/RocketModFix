using Rocket.API;
using Rocket.Core;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using Rocket.API.Extensions;
using Rocket.Unturned.Helpers;
using SDG.Unturned;
using Steamworks;

namespace Rocket.Unturned.Commands
{
    public class CommandAdmin : IRocketCommand
    {
        public AllowedCaller AllowedCaller
        {
            get
            {
                return AllowedCaller.Both;
            }
        }

        public string Name
        {
            get { return "admin"; }
        }

        public string Help
        {
            get { return "Give a player admin privileges";}
        }

        public string Syntax
        {
            get { return "<player>"; }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "rocket.admin" };
            }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (R.Settings.Instance.WebPermissions.Enabled)
            {
                UnturnedChat.Say(caller, $"This command is disabled because {nameof(R.Settings.Instance.WebPermissions)} is enabled.");
                return;
            }

            var playerName = command.GetStringParameter(0);
            if (playerName == null)
            {
                caller.ThrowWrongUsage(this, U.Translate("command_generic_invalid_parameter"));
            }
            if (RocketUtilities.TryGetSteamIdFromText(playerName, out var steamId) == false)
            {
                caller.ThrowWrongUsage(this, U.Translate("command_admin_player_invalid", playerName));
            }
            var targetPlayer = UnturnedPlayer.FromCSteamID(steamId!.Value);
            var targetPlayerName = targetPlayer?.Player != null
                ? targetPlayer.CharacterName
                : steamId.Value.ToString();
            if (SteamAdminlist.checkAdmin(steamId.Value))
            {
                UnturnedChat.Say(caller, U.Translate("command_admin_player_is_admin", targetPlayerName));
                return;
            }

            if (RocketUtilities.TryGetSteamIdFromText(caller.Id, out var callerSteamId) == false)
            {
                callerSteamId = CSteamID.Nil;
            }

            SteamAdminlist.admin(steamId.Value, callerSteamId!.Value);
            UnturnedChat.Say(caller, U.Translate("command_admin_success", targetPlayerName));
        }
    }
}
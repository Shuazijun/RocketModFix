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
    public class CommandUnadmin : IRocketCommand
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
            get { return "unadmin"; }
        }

        public string Help
        {
            get { return "Revoke a players admin privileges"; }
        }

        public string Syntax
        {
            get { return "<player>"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public List<string> Permissions
        {
            get { return new List<string>() { "rocket.unadmin" }; }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (!R.Settings.Instance.WebPermissions.Enabled)
            {
                var playerName = command.GetStringParameter(0);
                if (playerName == null)
                {
                    caller.ThrowWrongUsage(this, U.Translate("command_generic_invalid_parameter"));
                }
                if (RocketUtilities.TryGetSteamIdFromText(playerName, out var steamId) == false)
                {
                    caller.ThrowWrongUsage(this, U.Translate("command_unadmin_player_invalid", playerName));
                }
                var targetPlayer = UnturnedPlayer.FromCSteamID(steamId!.Value);
                var targetPlayerName = targetPlayer?.Player != null
                    ? targetPlayer.CharacterName
                    : steamId.Value.ToString();
                if (SteamAdminlist.checkAdmin(steamId!.Value) == false)
                {
                    UnturnedChat.Say(caller, U.Translate("command_unadmin_player_is_not_admin", targetPlayerName));
                    return;
                }

                SteamAdminlist.unadmin(steamId.Value);
                UnturnedChat.Say(caller, U.Translate("command_unadmin_success", targetPlayerName));
            }
        }
    }
}
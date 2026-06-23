using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Homes;
using Rocket.Unturned.Homes.Models;
using Rocket.Unturned.Player;
using Rocket.Unturned.Teleport;
using System.Collections.Generic;
using System.Text;

namespace Rocket.Unturned.Commands
{
    public sealed class CommandHomes : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "homes";

        public string Help => "Lists your claimed beds";

        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string> { "rocket.homes" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (!U.HomeSettings.Instance.Enabled || HomeRegistry.Instance == null)
            {
                UnturnedChat.Say(caller, U.Translate("command_not_found"));
                return;
            }

            UnturnedPlayer player = (UnturnedPlayer)caller;
            PlayerHomesRecord playerData = HomeRegistry.Instance.GetOrCreatePlayer(player.CSteamID);

            if (playerData.Homes.Count == 0)
            {
                TeleportMessageHelper.SendHomes(caller, "homes_no_homes");
                return;
            }

            StringBuilder sb = new StringBuilder(U.Translate(
                "homes_list",
                playerData.Homes.Count,
                VipHomeLimits.GetMaxHomes(player.Id)));

            foreach (PlayerHomeEntry home in playerData.Homes)
            {
                sb.Append(home.Name).Append(", ");
            }

            TeleportMessageHelper.SendHomesRaw(caller, sb.ToString().TrimEnd(',', ' '));
        }
    }
}

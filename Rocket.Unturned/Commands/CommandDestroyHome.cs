using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Homes;
using Rocket.Unturned.Homes.Models;
using Rocket.Unturned.Player;
using Rocket.Unturned.Teleport;
using System.Collections.Generic;
using System.Linq;

namespace Rocket.Unturned.Commands
{
    public sealed class CommandDestroyHome : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "destroyhome";

        public string Help => "Destroys a bed and removes it from your home list";

        public string Syntax => "<name>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string> { "rocket.destroyhome" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (!U.HomeSettings.Instance.Enabled || HomeRegistry.Instance == null)
            {
                UnturnedChat.Say(caller, U.Translate("command_not_found"));
                return;
            }

            UnturnedPlayer player = (UnturnedPlayer)caller;
            string? homeName = command.ElementAtOrDefault(0);
            if (string.IsNullOrEmpty(homeName))
            {
                TeleportMessageHelper.SendHomes(caller, "homes_destroy_format");
                return;
            }

            PlayerHomeEntry? home = HomeRegistry.Instance.GetPlayerHome(player.CSteamID, homeName);
            if (home == null)
            {
                TeleportMessageHelper.SendHomes(caller, "homes_not_found", homeName);
                return;
            }

            string removedName = home.Name;
            HomeRegistry.Instance.RemoveHome(player.CSteamID, home);
            home.DestroyBed();
            HomeRegistry.Instance.SaveData();
            TeleportMessageHelper.SendHomes(caller, "homes_destroy_success", removedName);
        }
    }
}

using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Homes;
using Rocket.Unturned.Homes.Models;
using Rocket.Unturned.Player;
using Rocket.Unturned.Teleport;
using System.Collections.Generic;

namespace Rocket.Unturned.Commands
{
    public sealed class CommandRenameHome : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "renamehome";

        public string Help => "Renames one of your homes";

        public string Syntax => "<current name> <new name>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string> { "rocket.renamehome" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (!U.HomeSettings.Instance.Enabled || HomeRegistry.Instance == null)
            {
                UnturnedChat.Say(caller, U.Translate("command_not_found"));
                return;
            }

            UnturnedPlayer player = (UnturnedPlayer)caller;
            if (command.Length < 2)
            {
                TeleportMessageHelper.SendHomes(caller, "homes_rename_format");
                return;
            }

            PlayerHomeEntry? home = HomeRegistry.Instance.GetPlayerHome(player.CSteamID, command[0]);
            if (home == null)
            {
                TeleportMessageHelper.SendHomes(caller, "homes_not_found", command[0]);
                return;
            }

            if (HomeRegistry.Instance.GetPlayerHome(player.CSteamID, command[1]) != null)
            {
                TeleportMessageHelper.SendHomes(caller, "homes_already_exists", command[1]);
                return;
            }

            string oldName = home.Name;
            home.Name = command[1];
            HomeRegistry.Instance.SaveData();
            TeleportMessageHelper.SendHomes(caller, "homes_rename_success", oldName, command[1]);
        }
    }
}

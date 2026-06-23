using Rocket.API;
using Rocket.Unturned.Homes;
using Rocket.Unturned.Teleport;
using System.Collections.Generic;

namespace Rocket.Unturned.Commands
{
    public sealed class CommandRestoreHomes : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Console;

        public string Name => "restorehomes";

        public string Help => "Rebuilds the homes database from claimed beds on the map";

        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string> { "rocket.restorehomes" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (!U.HomeSettings.Instance.Enabled || HomeRegistry.Instance == null)
            {
                TeleportMessageHelper.SendHomes(caller, "command_not_found");
                return;
            }

            int count = HomeRegistry.Instance.RestoreAllHomesFromWorld();
            TeleportMessageHelper.SendHomes(caller, "homes_restore_success", count.ToString("N0"));
        }
    }
}

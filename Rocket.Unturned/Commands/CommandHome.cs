using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Homes;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using UnityEngine;

namespace Rocket.Unturned.Commands
{
    public class CommandHome : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "home";

        public string Help => "Teleports you to your bed";

        public string Syntax => "[name]";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string> { "rocket.home" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;

            if (U.HomeSettings.Instance.Enabled && HomeRegistry.Instance != null)
            {
                string? homeName = command.Length > 0 ? command[0] : null;
                HomeRegistry.Instance.TeleportToHome(player, homeName);
                return;
            }

            Vector3 pos;
            byte rot;
            if (!BarricadeManager.tryGetBed(player.CSteamID, out pos, out rot))
            {
                caller.ThrowWrongUsage(this, U.Translate("command_bed_no_bed_found_private"));
            }

            pos.y += 0.5f;
            float yaw = MeasurementTool.byteToAngle(rot);
            player.Player.teleportToLocationUnsafe(pos, yaw);
        }
    }
}

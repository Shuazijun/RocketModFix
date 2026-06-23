using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Commands;
using Rocket.Unturned.Player;
using Rocket.Unturned.Teleport;
using Rocket.Unturned.Warps;
using Rocket.Unturned.Warps.Models;
using SDG.Unturned;
using Steamworks;
using System.Collections.Generic;

namespace Rocket.Unturned.Commands
{
    public sealed class CommandWarp : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "warp";

        public string Help => "Teleport to a named warp point";

        public string Syntax => "<warpname> [player]";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string> { "rocket.warp" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (!U.WarpSettings.Instance.Enabled || WarpRegistry.Instance == null)
            {
                UnturnedChat.Say(caller, U.Translate("command_not_found"));
                return;
            }

            if (command.Length == 0 || command.Length > 2)
            {
                TeleportMessageHelper.SendWarps(caller, "warp_help");
                return;
            }

            WarpPoint? warp = WarpRegistry.Instance.GetWarp(command[0]);
            if (warp == null)
            {
                TeleportMessageHelper.SendWarps(caller, "warp_not_found", command[0]);
                return;
            }

            UnturnedPlayer? target = command.GetUnturnedPlayerParameter(1);
            if (target != null)
            {
                if (!CanWarpOther(caller))
                {
                    TeleportMessageHelper.SendWarps(caller, "warp_other_denied");
                    return;
                }

                WarpTeleportService.TeleportTargetImmediately(caller, target, warp);
                return;
            }

            if (command.Length == 2)
            {
                TeleportMessageHelper.SendWarps(caller, "warp_player_not_found");
                return;
            }

            if (caller is ConsolePlayer)
            {
                TeleportMessageHelper.SendWarps(caller, "warp_console_need_player");
                return;
            }

            WarpTeleportService.TeleportCaller((UnturnedPlayer)caller, warp);
        }

        private static bool CanWarpOther(IRocketPlayer caller)
        {
            if (caller is ConsolePlayer)
            {
                return true;
            }

            if (caller.HasPermission("rocket.warp.other"))
            {
                return true;
            }

            if (ulong.TryParse(caller.Id, out ulong steamId))
            {
                return SteamAdminlist.checkAdmin(new CSteamID(steamId));
            }

            return false;
        }
    }
}

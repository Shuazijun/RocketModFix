using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Rocket.Unturned.Teleport;
using Rocket.Unturned.Warps;
using Rocket.Unturned.Warps.Models;
using SDG.Unturned;
using System.Collections.Generic;
using UnityEngine;

namespace Rocket.Unturned.Commands
{
    public sealed class CommandSetWarp : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "setwarp";

        public string Help => "Create a warp at your current location";

        public string Syntax => "<name>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string> { "rocket.setwarp" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (!U.WarpSettings.Instance.Enabled || WarpRegistry.Instance == null)
            {
                UnturnedChat.Say(caller, U.Translate("command_not_found"));
                return;
            }

            if (command.Length != 1)
            {
                TeleportMessageHelper.SendWarps(caller, "warp_set_help");
                return;
            }

            string warpName = WarpNameSanitizer.Sanitize(command[0]);
            if (string.IsNullOrWhiteSpace(warpName))
            {
                TeleportMessageHelper.SendWarps(caller, "warp_set_failed");
                return;
            }

            UnturnedPlayer player = (UnturnedPlayer)caller;
            Vector3 position = player.Position;
            WarpPoint warp = new WarpPoint
            {
                Name = warpName,
                World = Provider.map.ToLowerInvariant(),
                X = position.x,
                Y = position.y,
                Z = position.z,
                Rotation = player.Rotation,
                SetterCharName = WarpNameSanitizer.Sanitize(player.CharacterName),
                SetterSteamName = WarpNameSanitizer.Sanitize(player.SteamName),
                SetterSteamId = player.CSteamID.m_SteamID
            };

            if (WarpRegistry.Instance.SetWarp(warp))
            {
                TeleportMessageHelper.SendWarps(caller, "warp_set_success");
            }
            else
            {
                TeleportMessageHelper.SendWarps(caller, "warp_set_failed");
            }
        }
    }
}

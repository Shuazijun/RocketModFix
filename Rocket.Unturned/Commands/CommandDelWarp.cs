using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Teleport;
using Rocket.Unturned.Warps;
using System.Collections.Generic;

namespace Rocket.Unturned.Commands
{
    public sealed class CommandDelWarp : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "delwarp";

        public string Help => "Delete a warp on the current map";

        public string Syntax => "<name>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string> { "rocket.delwarp" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (!U.WarpSettings.Instance.Enabled || WarpRegistry.Instance == null)
            {
                UnturnedChat.Say(caller, U.Translate("command_not_found"));
                return;
            }

            if (command.Length != 1)
            {
                TeleportMessageHelper.SendWarps(caller, "warp_del_help");
                return;
            }

            if (WarpRegistry.Instance.RemoveWarp(command[0]))
            {
                TeleportMessageHelper.SendWarps(caller, "warp_del_success");
            }
            else
            {
                TeleportMessageHelper.SendWarps(caller, "warp_del_not_found");
            }
        }
    }
}

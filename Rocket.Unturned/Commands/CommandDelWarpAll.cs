using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Teleport;
using Rocket.Unturned.Warps;
using System.Collections.Generic;

namespace Rocket.Unturned.Commands
{
    public sealed class CommandDelWarpAll : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "delwarpall";

        public string Help => "Delete all warps for a map";

        public string Syntax => "<mapname>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string> { "rocket.delwarpall" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (!U.WarpSettings.Instance.Enabled || WarpRegistry.Instance == null)
            {
                UnturnedChat.Say(caller, U.Translate("command_not_found"));
                return;
            }

            if (command.Length != 1)
            {
                TeleportMessageHelper.SendWarps(caller, "warp_del_all_help");
                return;
            }

            int count = WarpRegistry.Instance.RemoveAllForMap(command[0]);
            if (count == 0)
            {
                TeleportMessageHelper.SendWarps(caller, "warps_empty");
                return;
            }

            TeleportMessageHelper.SendWarps(caller, "warp_del_all_success", count.ToString());
        }
    }
}

using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Teleport;
using Rocket.Unturned.Warps;
using Rocket.Unturned.Warps.Models;
using System.Collections.Generic;
using System.Linq;

namespace Rocket.Unturned.Commands
{
    public sealed class CommandWarps : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "warps";

        public string Help => "List warp points on the current map";

        public string Syntax => "[filter]";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string> { "rocket.warps" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (!U.WarpSettings.Instance.Enabled || WarpRegistry.Instance == null)
            {
                UnturnedChat.Say(caller, U.Translate("command_not_found"));
                return;
            }

            if (command.Length == 1 && command[0].Equals("help", System.StringComparison.OrdinalIgnoreCase))
            {
                TeleportMessageHelper.SendWarps(caller, "warps_help");
                return;
            }

            string? filter = command.Length > 0 ? command[0] : null;
            List<WarpPoint> warps = WarpRegistry.Instance.SearchWarps(filter);
            if (warps.Count == 0)
            {
                TeleportMessageHelper.SendWarps(caller, "warps_empty");
                return;
            }

            TeleportMessageHelper.SendWarps(caller, "warps_list_header", warps.Count.ToString());
            TeleportMessageHelper.SendWarps(caller, "warps_list", string.Join(", ", warps.Select(warp => warp.Name).ToArray()));
        }
    }
}

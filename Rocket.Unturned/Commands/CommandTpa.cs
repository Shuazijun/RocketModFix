using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Rocket.Unturned.Teleport;
using System.Collections.Generic;

namespace Rocket.Unturned.Commands
{
    public sealed class CommandTpa : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "tpa";

        public string Help => "Send or manage teleport requests to other players";

        public string Syntax => "<player|accept|deny|cancel>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string> { "rocket.tpa" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (!U.TpaSettings.Instance.Enabled || TpaService.Instance == null)
            {
                UnturnedChat.Say(caller, U.Translate("command_not_found"));
                return;
            }

            if (command.Length < 1)
            {
                TeleportMessageHelper.SendTpa(caller, "tpa_help");
                return;
            }

            UnturnedPlayer player = (UnturnedPlayer)caller;
            string cmd = command[0].ToLower();

            if (cmd == "accept" || cmd == "a")
            {
                TpaService.Instance.AcceptRequest(player);
                return;
            }

            if (cmd == "cancel" || cmd == "c")
            {
                TpaService.Instance.CancelRequest(player);
                return;
            }

            if (cmd == "deny" || cmd == "d")
            {
                TpaService.Instance.DenyRequest(player);
                return;
            }

            UnturnedPlayer? target = UnturnedPlayer.FromName(command[0]);
            if (target == null)
            {
                TeleportMessageHelper.SendTpa(caller, "tpa_target_not_found");
                return;
            }

            TpaService.Instance.SendRequest(player, target);
        }
    }
}

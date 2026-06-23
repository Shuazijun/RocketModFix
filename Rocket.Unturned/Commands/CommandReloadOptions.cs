using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rocket.Unturned.Commands
{
    public sealed class CommandReloadOptions : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "reloadoptions";

        public string Help => "Reload RocketModFix configuration files without restarting the server";

        public string Syntax => "<module> [module ...] | all";

        public List<string> Aliases => new List<string> { "reloadconfig", "reloadcfg" };

        public List<string> Permissions => new List<string> { "rocket.reloadoptions" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, U.Translate("command_reloadoptions_usage"));
                UnturnedChat.Say(caller, U.Translate("command_reloadoptions_modules", ConfigOptionsReloader.FormatModuleList()));
                return;
            }

            if (!ConfigOptionsReloader.TryNormalizeModules(command, out HashSet<string> modules, out List<string> unknown))
            {
                UnturnedChat.Say(caller, U.Translate("command_reloadoptions_unknown", string.Join(", ", unknown.ToArray())));
                UnturnedChat.Say(caller, U.Translate("command_reloadoptions_modules", ConfigOptionsReloader.FormatModuleList()));
                return;
            }

            if (modules.Count == 0)
            {
                UnturnedChat.Say(caller, U.Translate("command_reloadoptions_usage"));
                UnturnedChat.Say(caller, U.Translate("command_reloadoptions_modules", ConfigOptionsReloader.FormatModuleList()));
                return;
            }

            try
            {
                bool reloadAll = command.Any(argument => argument.Equals("all", StringComparison.OrdinalIgnoreCase));
                IReadOnlyList<string> reloaded = ConfigOptionsReloader.ReloadModules(modules);

                if (reloadAll)
                {
                    UnturnedChat.Say(caller, U.Translate("command_reloadoptions_success_all"));
                }
                else
                {
                    UnturnedChat.Say(caller, U.Translate("command_reloadoptions_success", string.Join(", ", reloaded.ToArray())));
                }
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, U.Translate("command_reloadoptions_failed"));
                Logger.LogException(ex, "reloadoptions failed");
            }
        }
    }
}

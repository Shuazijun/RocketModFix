using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Rocket.Unturned.Vehicles;
using SDG.Unturned;
using Steamworks;
using System.Collections.Generic;

namespace Rocket.Unturned.Commands
{
    public sealed class CommandRVehicle : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "rvehicle";

        public string Help => "Spawn a vehicle for a player without server cheats (same syntax as vanilla /vehicle)";

        public string Syntax => "<Player/>Vehicle";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "rocket.rvehicle" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                caller.ThrowWrongUsage(this, U.Translate("command_generic_invalid_parameter"));
            }

            string[] components = Parser.getComponentsFromSerial(string.Join("/", command), '/');
            if (components.Length < 1 || components.Length > 2)
            {
                caller.ThrowWrongUsage(this, U.Translate("command_generic_invalid_parameter"));
            }

            bool targetIsExecutor = false;
            SteamPlayer? steamPlayer = null;

            if (!PlayerTool.tryGetSteamPlayer(components[0], out steamPlayer))
            {
                CSteamID executorId = CSteamID.Nil;
                if (caller is UnturnedPlayer executor)
                {
                    executorId = executor.CSteamID;
                }

                steamPlayer = PlayerTool.getSteamPlayer(executorId);
                if (steamPlayer == null)
                {
                    UnturnedChat.Say(caller, U.Translate("command_rvehicle_player_not_found", components[0]), UnityEngine.Color.red);
                    return;
                }

                targetIsExecutor = true;
            }

            string vehicleToken = components[targetIsExecutor ? 0 : 1];
            UnturnedPlayer target = UnturnedPlayer.FromPlayer(steamPlayer.player);
            VehicleAsset? vehicleAsset = UnturnedVehicleSpawn.ResolveVehicleAsset(vehicleToken);

            if (vehicleAsset == null)
            {
                UnturnedChat.Say(caller, U.Translate("command_rvehicle_not_found", vehicleToken), UnityEngine.Color.red);
                return;
            }

            if (U.Settings.Instance.EnableVehicleBlacklist && !target.HasPermission("vehicleblacklist.bypass"))
            {
                if (target.HasPermission("vehicle." + vehicleAsset.id))
                {
                    UnturnedChat.Say(caller, U.Translate("command_v_blacklisted"), UnityEngine.Color.red);
                    return;
                }
            }

            InteractableVehicle? spawned = UnturnedVehicleSpawn.SpawnForPlayer(target.Player, vehicleAsset);
            if (spawned == null)
            {
                UnturnedChat.Say(caller, U.Translate("command_rvehicle_failed", target.DisplayName, vehicleAsset.vehicleName, vehicleAsset.id), UnityEngine.Color.red);
                return;
            }

            Logger.Log(U.Translate("command_rvehicle_spawn_console", caller.DisplayName, target.DisplayName, vehicleAsset.id));
            NotifySpawnSuccess(caller, target, vehicleAsset.vehicleName, vehicleAsset.id);
        }

        private static void NotifySpawnSuccess(IRocketPlayer caller, UnturnedPlayer target, string vehicleName, ushort vehicleId)
        {
            if (caller is UnturnedPlayer callerPlayer && callerPlayer.CSteamID == target.CSteamID)
            {
                UnturnedChat.Say(caller, U.Translate("command_rvehicle_spawn_self", vehicleName, vehicleId));
                return;
            }

            UnturnedChat.Say(caller, U.Translate("command_rvehicle_spawn_executor", target.DisplayName, vehicleName, vehicleId));
            UnturnedChat.Say(target, U.Translate("command_rvehicle_spawn_target", vehicleName, vehicleId));
        }
    }
}

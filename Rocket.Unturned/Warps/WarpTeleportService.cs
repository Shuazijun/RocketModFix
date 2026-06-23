using Rocket.API;
using Rocket.Core.Utils;
using Rocket.Unturned.Player;
using Rocket.Unturned.Serialisation;
using Rocket.Unturned.Teleport;
using Rocket.Unturned.Warps.Models;
using SDG.Unturned;
using Steamworks;
using System;
using UnityEngine;

namespace Rocket.Unturned.Warps
{
    internal static class WarpTeleportService
    {
        public static void TeleportCaller(UnturnedPlayer player, WarpPoint warp)
        {
            if (!ValidateWarp(player, warp))
            {
                return;
            }

            WarpSettings settings = U.WarpSettings.Instance;
            float delay = VipWarpDelays.GetWarpDelay(player.Id);

            if (delay > 0)
            {
                if (settings.CancelOnMove)
                {
                    TeleportMessageHelper.SendWarps(player, "warp_delay_nomove", warp.Name, delay.ToString("N0"));
                }
                else
                {
                    TeleportMessageHelper.SendWarps(player, "warp_delay", warp.Name, delay.ToString("N0"));
                }
            }

            WarpTeleportSession session = new WarpTeleportSession();

            if (settings.CancelOnMove && delay > 0 && MovementCancelWatcher.Instance != null)
            {
                MovementCancelWatcher.Instance.AddPlayer(player.Player, () =>
                {
                    session.IsCanceled = true;
                    TeleportMessageHelper.SendWarps(player, "warp_canceled_moved", warp.Name);
                }, settings.MoveMaxDistance);
            }

            TaskDispatcher.QueueOnMainThread(() =>
            {
                if (session.IsCanceled)
                {
                    MovementCancelWatcher.Instance?.RemovePlayer(player.Player);
                    return;
                }

                MovementCancelWatcher.Instance?.RemovePlayer(player.Player);

                if (!ValidateWarp(player, warp))
                {
                    return;
                }

                if (!PerformTeleport(player, warp))
                {
                    return;
                }

                TeleportMessageHelper.SendWarps(player, "warp_success", warp.Name);
            }, delay);
        }

        public static void TeleportTargetImmediately(IRocketPlayer caller, UnturnedPlayer target, WarpPoint warp)
        {
            if (U.WarpSettings.Instance.BlockInVehicle &&
                (target.Stance == EPlayerStance.DRIVING || target.Stance == EPlayerStance.SITTING))
            {
                TeleportMessageHelper.SendWarps(caller, "warp_while_driving");
                return;
            }

            if (!PerformTeleport(target, warp))
            {
                return;
            }

            TeleportMessageHelper.SendWarps(caller, "warp_other_success", target.CharacterName, warp.Name);
            TeleportMessageHelper.SendWarps(target, "warp_success", warp.Name);
            Core.Logging.Logger.Log(U.Translate("warp_other_log", caller.DisplayName, caller.Id, target.CharacterName, warp.Name));
        }

        private static bool ValidateWarp(UnturnedPlayer player, WarpPoint warp)
        {
            WarpSettings settings = U.WarpSettings.Instance;

            if (player.Dead || player.Health <= 0)
            {
                TeleportMessageHelper.SendWarps(player, "warp_canceled_died", warp.Name);
                return false;
            }

            if (settings.BlockInVehicle &&
                (player.Stance == EPlayerStance.DRIVING || player.Stance == EPlayerStance.SITTING))
            {
                TeleportMessageHelper.SendWarps(player, "warp_while_driving");
                return false;
            }

            if (CombatRaidTracker.Instance != null)
            {
                if (!settings.AllowCombat && CombatRaidTracker.Instance.IsInCombat(player.CSteamID))
                {
                    TeleportMessageHelper.SendWarps(player, "warp_while_combat");
                    return false;
                }

                if (!settings.AllowRaid && CombatRaidTracker.Instance.IsInRaid(player.CSteamID))
                {
                    TeleportMessageHelper.SendWarps(player, "warp_while_raid");
                    return false;
                }
            }

            return true;
        }

        private static bool PerformTeleport(UnturnedPlayer player, WarpPoint warp)
        {
            WarpRegistry? registry = WarpRegistry.Instance;
            if (registry == null)
            {
                return false;
            }

            Vector3 destination = registry.GetLocation(warp);
            if (U.WarpSettings.Instance.UseUnsafeTeleport)
            {
                player.Player.teleportToLocationUnsafe(destination, warp.Rotation);
                return true;
            }

            return player.Player.teleportToLocation(destination, warp.Rotation);
        }

        private sealed class WarpTeleportSession
        {
            public bool IsCanceled;
        }
    }
}

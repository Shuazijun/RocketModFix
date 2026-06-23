// Adapted from RestoreMonarchy Teleportation TPARequest (MIT).
using Rocket.Core.Utils;
using Rocket.Unturned.Player;
using Rocket.Unturned.Serialisation;
using SDG.Unturned;
using Steamworks;
using System;
using UnityEngine;

namespace Rocket.Unturned.Teleport
{
    internal sealed class TpaRequest
    {
        public CSteamID Sender { get; }
        public CSteamID Target { get; }
        public DateTime ExpireDate { get; }
        public bool IsCanceled { get; private set; }
        public UnturnedPlayer SenderPlayer { get; }
        public UnturnedPlayer TargetPlayer { get; }

        public bool IsExpired => ExpireDate < DateTime.Now;

        public TpaRequest(CSteamID sender, CSteamID target)
        {
            Sender = sender;
            Target = target;
            SenderPlayer = UnturnedPlayer.FromCSteamID(sender)!;
            TargetPlayer = UnturnedPlayer.FromCSteamID(target)!;

            double duration = U.TpaSettings.Instance.TpaDuration;
            if (duration <= 0)
            {
                duration = 30;
            }
            ExpireDate = DateTime.Now.AddSeconds(duration);
        }

        public void Execute(double delay)
        {
            TpaSettings settings = U.TpaSettings.Instance;

            if (delay > 0)
            {
                TeleportMessageHelper.SendTpa(SenderPlayer, "tpa_delay", TargetPlayer.DisplayName, delay);
                if (settings.CancelOnMove)
                {
                    MovementCancelWatcher.Instance.AddPlayer(SenderPlayer.Player, () =>
                    {
                        TeleportMessageHelper.SendTpa(SenderPlayer, "tpa_canceled_you_moved");
                        TeleportMessageHelper.SendTpa(TargetPlayer, "tpa_canceled_sender_moved", SenderPlayer.DisplayName);
                        Cancel();
                    }, settings.MoveMaxDistance);
                }
            }

            TaskDispatcher.QueueOnMainThread(() =>
            {
                if (IsCanceled)
                {
                    return;
                }

                MovementCancelWatcher.Instance.RemovePlayer(SenderPlayer.Player);

                if (!Validate(true))
                {
                    TpaService.Instance!.ClearSenderCooldown(Sender);
                    return;
                }

                if (settings.UseUnsafeTeleport)
                {
                    SenderPlayer.Player.teleportToLocationUnsafe(TargetPlayer.Position, TargetPlayer.Rotation);
                }
                else
                {
                    SenderPlayer.Teleport(TargetPlayer);
                }

                TeleportMessageHelper.SendTpa(SenderPlayer, "tpa_success", TargetPlayer.DisplayName);
            }, (float)delay);
        }

        public bool Validate(bool isFinal = false)
        {
            CombatRaidTracker tracker = CombatRaidTracker.Instance;

            if (tracker.IsInCombat(Sender))
            {
                TeleportMessageHelper.SendTpa(SenderPlayer, "tpa_while_combat_you");
                if (isFinal)
                {
                    TeleportMessageHelper.SendTpa(TargetPlayer, "tpa_while_combat", SenderPlayer.DisplayName);
                }
                return false;
            }

            if (tracker.IsInRaid(Sender))
            {
                TeleportMessageHelper.SendTpa(SenderPlayer, "tpa_while_raid_you");
                if (isFinal)
                {
                    TeleportMessageHelper.SendTpa(TargetPlayer, "tpa_while_raid", SenderPlayer.DisplayName);
                }
                return false;
            }

            if (SenderPlayer.Dead || (TargetPlayer.Dead && isFinal))
            {
                TeleportMessageHelper.SendTpa(SenderPlayer, "tpa_dead");
                if (isFinal)
                {
                    TeleportMessageHelper.SendTpa(TargetPlayer, "tpa_dead");
                }
                return false;
            }

            if (CombatRaidTracker.IsPlayerInCave(TargetPlayer))
            {
                TeleportMessageHelper.SendTpa(SenderPlayer, "tpa_cave", TargetPlayer.DisplayName);
                if (isFinal)
                {
                    TeleportMessageHelper.SendTpa(TargetPlayer, "tpa_cave_you");
                }
                return false;
            }

            if (SenderPlayer.IsInVehicle)
            {
                TeleportMessageHelper.SendTpa(SenderPlayer, "tpa_vehicle_you");
                if (isFinal)
                {
                    TeleportMessageHelper.SendTpa(TargetPlayer, "tpa_vehicle", SenderPlayer.DisplayName);
                }
                return false;
            }

            return true;
        }

        public void Cancel()
        {
            IsCanceled = true;
        }
    }
}

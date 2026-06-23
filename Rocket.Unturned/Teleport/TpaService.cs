// Adapted from RestoreMonarchy Teleportation (MIT).
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using Rocket.Unturned.Serialisation;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rocket.Unturned.Teleport
{
    internal sealed class TpaService : MonoBehaviour
    {
        public static TpaService? Instance { get; private set; }

        private readonly List<TpaRequest> requests = new List<TpaRequest>();
        private readonly Dictionary<CSteamID, DateTime> cooldowns = new Dictionary<CSteamID, DateTime>();

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            MovementCancelWatcher.Instance.SetMoveMaxDistance(U.TpaSettings.Instance.MoveMaxDistance);
            U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
            InvokeRepeating(nameof(RemoveExpiredRequests), 5f, 5f);
        }

        private void OnDisable()
        {
            U.Events.OnPlayerDisconnected -= OnPlayerDisconnected;
            CancelInvoke(nameof(RemoveExpiredRequests));
            requests.Clear();
            cooldowns.Clear();
            Instance = null;
        }

        private void OnPlayerDisconnected(UnturnedPlayer player)
        {
            ClearPlayerRequests(player.CSteamID);
            CombatRaidTracker.Instance.StopCombat(player.CSteamID);
            CombatRaidTracker.Instance.StopRaid(player.CSteamID);
        }

        private void RemoveExpiredRequests()
        {
            requests.RemoveAll(x => x.IsExpired);
        }

        public void SendRequest(UnturnedPlayer sender, UnturnedPlayer target)
        {
            if (sender.Id == target.Id)
            {
                TeleportMessageHelper.SendTpa(sender, "tpa_yourself");
                return;
            }

            TpaSettings settings = U.TpaSettings.Instance;
            if (cooldowns.TryGetValue(sender.CSteamID, out DateTime lastUse))
            {
                double secondsElapsed = (DateTime.Now - lastUse).TotalSeconds;
                if (secondsElapsed < settings.TpaCooldown)
                {
                    double timeLeft = Math.Round(settings.TpaCooldown - secondsElapsed);
                    TeleportMessageHelper.SendTpa(sender, "tpa_cooldown", timeLeft.ToString("N0"));
                    return;
                }
            }

            if (requests.Exists(x => x.Sender == sender.CSteamID && x.Target == target.CSteamID))
            {
                TeleportMessageHelper.SendTpa(sender, "tpa_duplicate");
                return;
            }

            TpaRequest request = new TpaRequest(sender.CSteamID, target.CSteamID);
            if (!request.Validate())
            {
                return;
            }

            requests.Add(request);
            cooldowns[sender.CSteamID] = DateTime.Now;

            TeleportMessageHelper.SendTpa(sender, "tpa_sent", target.DisplayName);
            TeleportMessageHelper.SendTpa(target, "tpa_receive", sender.DisplayName);
        }

        public void AcceptRequest(UnturnedPlayer caller)
        {
            requests.RemoveAll(x => x.IsExpired);

            TpaRequest? request = requests.FirstOrDefault(x => x.Target == caller.CSteamID);
            if (request == null)
            {
                TeleportMessageHelper.SendTpa(caller, "tpa_no_request");
                return;
            }

            TeleportMessageHelper.SendTpa(caller, "tpa_accepted", request.SenderPlayer.CharacterName);
            request.Execute(U.TpaSettings.Instance.TpaDelay);
            requests.Remove(request);
        }

        public void CancelRequest(UnturnedPlayer caller)
        {
            TpaRequest? request = requests.FirstOrDefault(x => x.Sender == caller.CSteamID);
            if (request == null)
            {
                TeleportMessageHelper.SendTpa(caller, "tpa_no_sent_request");
                return;
            }

            TeleportMessageHelper.SendTpa(caller, "tpa_canceled", request.TargetPlayer.DisplayName);
            requests.Remove(request);
        }

        public void DenyRequest(UnturnedPlayer caller)
        {
            TpaRequest? request = requests.FirstOrDefault(x => x.Target == caller.CSteamID);
            if (request == null)
            {
                TeleportMessageHelper.SendTpa(caller, "tpa_no_request");
                return;
            }

            TeleportMessageHelper.SendTpa(caller, "tpa_denied", request.SenderPlayer.DisplayName);
            requests.Remove(request);
        }

        public void ClearPlayerRequests(CSteamID steamId)
        {
            requests.RemoveAll(x => x.Sender == steamId || x.Target == steamId);
        }

        public void ClearSenderCooldown(CSteamID steamId)
        {
            cooldowns.Remove(steamId);
        }
    }
}

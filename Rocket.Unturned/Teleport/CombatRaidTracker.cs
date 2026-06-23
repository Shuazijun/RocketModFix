// Combat/raid tracking adapted from RestoreMonarchy Teleportation (MIT).
using Rocket.Core.Utils;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using Rocket.Unturned.Serialisation;
using SDG.Unturned;
using Steamworks;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

namespace Rocket.Unturned.Teleport
{
    internal sealed class CombatRaidTracker : MonoBehaviour
    {
        public static CombatRaidTracker Instance = null!;

        private readonly Dictionary<CSteamID, Timer> combatPlayers = new Dictionary<CSteamID, Timer>();
        private readonly Dictionary<CSteamID, Timer> raidPlayers = new Dictionary<CSteamID, Timer>();

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
            DamageTool.damagePlayerRequested += OnDamagePlayerRequested;
            UnturnedPlayerEvents.OnPlayerDeath += OnPlayerDeath;
            BarricadeManager.onDamageBarricadeRequested += OnBarricadeDamaged;
            StructureManager.onDamageStructureRequested += OnStructureDamaged;
        }

        private void OnDisable()
        {
            U.Events.OnPlayerDisconnected -= OnPlayerDisconnected;
            DamageTool.damagePlayerRequested -= OnDamagePlayerRequested;
            UnturnedPlayerEvents.OnPlayerDeath -= OnPlayerDeath;
            BarricadeManager.onDamageBarricadeRequested -= OnBarricadeDamaged;
            StructureManager.onDamageStructureRequested -= OnStructureDamaged;

            foreach (Timer timer in combatPlayers.Values)
            {
                timer.Dispose();
            }
            foreach (Timer timer in raidPlayers.Values)
            {
                timer.Dispose();
            }
            combatPlayers.Clear();
            raidPlayers.Clear();
        }

        private void OnPlayerDisconnected(UnturnedPlayer player)
        {
            StopCombat(player.CSteamID);
            StopRaid(player.CSteamID);
        }

        private void OnDamagePlayerRequested(ref DamagePlayerParameters parameters, ref bool shouldAllow)
        {
            TpaSettings settings = U.TpaSettings.Instance;
            if (settings.AllowCombat)
            {
                return;
            }

            SDG.Unturned.Player? killerPlayer = PlayerTool.getPlayer(parameters.killer);
            if (parameters.player.life.isDead || killerPlayer == null || killerPlayer == parameters.player)
            {
                return;
            }

            StartCombat(parameters.killer);
            StartCombat(parameters.player.channel.owner.playerID.steamID);
        }

        private void OnPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
            StopCombat(player.CSteamID);
        }

        private void OnStructureDamaged(CSteamID instigatorSteamID, Transform structureTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            TryStartRaid(instigatorSteamID, () =>
            {
                StructureDrop drop = StructureManager.FindStructureByRootTransform(structureTransform);
                StructureData data = drop.GetServersideData();
                return data;
            }, (data, steamPlayer) =>
                data.owner == instigatorSteamID.m_SteamID || data.group == steamPlayer.player.quests.groupID.m_SteamID,
            (data) =>
                Provider.clients.Exists(x => x.playerID.steamID.m_SteamID == data.owner || x.player.quests.groupID.m_SteamID == data.group));
        }

        private void OnBarricadeDamaged(CSteamID instigatorSteamID, Transform barricadeTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            TryStartRaid(instigatorSteamID, () =>
            {
                BarricadeDrop drop = BarricadeManager.FindBarricadeByRootTransform(barricadeTransform);
                BarricadeData data = drop.GetServersideData();
                return data;
            }, (data, steamPlayer) =>
                data.owner == instigatorSteamID.m_SteamID || data.group == steamPlayer.player.quests.groupID.m_SteamID,
            (data) =>
                Provider.clients.Exists(x => x.playerID.steamID.m_SteamID == data.owner || x.player.quests.groupID.m_SteamID == data.group));
        }

        private void TryStartRaid<T>(CSteamID instigatorSteamID, System.Func<T> getData, System.Func<T, SteamPlayer, bool> isOwner, System.Func<T, bool> ownerOnline) where T : class
        {
            TpaSettings settings = U.TpaSettings.Instance;
            if (settings.AllowRaid)
            {
                return;
            }

            SteamPlayer? steamPlayer = PlayerTool.getSteamPlayer(instigatorSteamID);
            if (steamPlayer == null)
            {
                return;
            }

            T data = getData();
            if (data == null || isOwner(data, steamPlayer) || !ownerOnline(data))
            {
                return;
            }

            StartRaid(instigatorSteamID);
        }

        public bool IsInCombat(CSteamID steamId)
        {
            if (U.TpaSettings.Instance.AllowCombat)
            {
                return false;
            }

            return combatPlayers.TryGetValue(steamId, out Timer? timer) && timer.Enabled;
        }

        public bool IsInRaid(CSteamID steamId)
        {
            if (U.TpaSettings.Instance.AllowRaid)
            {
                return false;
            }

            return raidPlayers.TryGetValue(steamId, out Timer? timer) && timer.Enabled;
        }

        public static bool IsPlayerInCave(UnturnedPlayer player)
        {
            if (U.TpaSettings.Instance.AllowCave)
            {
                return false;
            }

            Vector3 position = player.Position;
            float height = LevelGround.getHeight(position);
            return height > position.y;
        }

        public void StartCombat(CSteamID steamId)
        {
            TpaSettings settings = U.TpaSettings.Instance;
            if (combatPlayers.TryGetValue(steamId, out Timer? timer))
            {
                timer.Stop();
                timer.Start();
                return;
            }

            timer = new Timer(settings.CombatDuration * 1000);
            combatPlayers.Add(steamId, timer);
            timer.AutoReset = false;
            timer.Elapsed += (_, _) =>
            {
                TaskDispatcher.QueueOnMainThread(() => StopCombat(steamId));
            };
            timer.Start();
            TeleportMessageHelper.SendTpa(UnturnedPlayer.FromCSteamID(steamId), "tpa_combat_start");
        }

        public void StopCombat(CSteamID steamId)
        {
            if (!combatPlayers.TryGetValue(steamId, out Timer? timer))
            {
                return;
            }

            timer.Dispose();
            combatPlayers.Remove(steamId);
            TeleportMessageHelper.SendTpa(UnturnedPlayer.FromCSteamID(steamId), "tpa_combat_expire");
        }

        public void StartRaid(CSteamID steamId)
        {
            TpaSettings settings = U.TpaSettings.Instance;
            if (raidPlayers.TryGetValue(steamId, out Timer? timer))
            {
                timer.Stop();
                timer.Start();
                return;
            }

            timer = new Timer(settings.RaidDuration * 1000);
            raidPlayers.Add(steamId, timer);
            timer.AutoReset = false;
            timer.Elapsed += (_, _) =>
            {
                TaskDispatcher.QueueOnMainThread(() => StopRaid(steamId));
            };
            timer.Start();
            TeleportMessageHelper.SendTpa(UnturnedPlayer.FromCSteamID(steamId), "tpa_raid_start");
        }

        public void StopRaid(CSteamID steamId)
        {
            if (!raidPlayers.TryGetValue(steamId, out Timer? timer))
            {
                return;
            }

            timer.Dispose();
            raidPlayers.Remove(steamId);
            TeleportMessageHelper.SendTpa(UnturnedPlayer.FromCSteamID(steamId), "tpa_raid_expire");
        }
    }
}

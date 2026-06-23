// Adapted from RestoreMonarchy MoreHomes DataService / HomesHelper (MIT).
using HarmonyLib;
using Newtonsoft.Json;
using Rocket.Core.Utils;
using Rocket.Unturned.Homes.Models;
using Rocket.Unturned.Player;
using Rocket.Unturned.Serialisation;
using Rocket.Unturned.Teleport;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Rocket.Unturned.Homes
{
    internal sealed class HomeRegistry : MonoBehaviour
    {
        public const string HarmonyId = "com.rocketmodfix.unturned.homes";
        private const string DataFileName = "RocketHomes.json";

        public static HomeRegistry? Instance { get; private set; }

        private readonly List<PlayerHomesRecord> playersData = new List<PlayerHomesRecord>();
        private readonly Dictionary<string, DateTime> playerCooldowns = new Dictionary<string, DateTime>();
        private Harmony? harmony;
        private string dataFilePath = "";

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            harmony = new Harmony(HarmonyId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            SaveManager.onPostSave += SaveData;
            U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
            InvokeRepeating(nameof(RemoveExpiredCooldowns), 300f, 300f);

            if (Level.isLoaded)
            {
                InitializeData();
            }
            else
            {
                Level.onLevelLoaded += OnLevelLoaded;
            }
        }

        private void OnDisable()
        {
            Level.onLevelLoaded -= OnLevelLoaded;
            SaveManager.onPostSave -= SaveData;
            U.Events.OnPlayerDisconnected -= OnPlayerDisconnected;
            CancelInvoke(nameof(RemoveExpiredCooldowns));

            harmony?.UnpatchAll(HarmonyId);
            harmony = null;
            Instance = null;
        }

        private void OnDestroy()
        {
            SaveData();
        }

        private void OnLevelLoaded(int level)
        {
            InitializeData();
        }

        private void OnPlayerDisconnected(UnturnedPlayer player)
        {
            playerCooldowns.Remove(player.Id);
        }

        private void RemoveExpiredCooldowns()
        {
            foreach (KeyValuePair<string, DateTime> cooldown in playerCooldowns.ToList())
            {
                if (cooldown.Value < DateTime.Now)
                {
                    playerCooldowns.Remove(cooldown.Key);
                }
            }
        }

        private void InitializeData()
        {
            string directory = Path.Combine(ReadWrite.PATH, "Servers", Dedicator.serverID, "Level", Provider.map);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            dataFilePath = Path.Combine(directory, DataFileName);
            LoadData();
            ReloadBedReferences();
        }

        private void LoadData()
        {
            playersData.Clear();
            if (!File.Exists(dataFilePath))
            {
                return;
            }

            try
            {
                List<PlayerHomesRecord>? loaded = JsonConvert.DeserializeObject<List<PlayerHomesRecord>>(File.ReadAllText(dataFilePath));
                if (loaded != null)
                {
                    playersData.AddRange(loaded);
                }
            }
            catch (Exception ex)
            {
                Core.Logging.Logger.LogException(ex, "Failed to load homes data: " + dataFilePath);
            }
        }

        public void SaveData()
        {
            if (string.IsNullOrEmpty(dataFilePath))
            {
                return;
            }

            foreach (PlayerHomesRecord player in playersData)
            {
                foreach (PlayerHomeEntry home in player.Homes)
                {
                    if (home.InteractableBed != null)
                    {
                        home.Position = new StoredPosition(home.LivePosition);
                    }
                }
            }

            try
            {
                string? directory = Path.GetDirectoryName(dataFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(dataFilePath, JsonConvert.SerializeObject(playersData, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Core.Logging.Logger.LogException(ex, "Failed to save homes data: " + dataFilePath);
            }
        }

        public void ReloadBedReferences()
        {
            List<InteractableBed> beds = CollectBeds();

            foreach (InteractableBed bed in beds)
            {
                if (bed.owner == CSteamID.Nil)
                {
                    continue;
                }

                PlayerHomeEntry? home = GetPlayerHome(bed.owner, bed.transform.position);
                if (home != null)
                {
                    home.InteractableBed = bed;
                }
                else
                {
                    PlayerHomesRecord player = GetOrCreatePlayer(bed.owner);
                    home = new PlayerHomeEntry(player.GetUniqueHomeName(), bed);
                    player.Homes.Add(home);
                }
            }

            PruneInvalidHomes();
        }

        private static List<InteractableBed> CollectBeds()
        {
            List<InteractableBed> beds = new List<InteractableBed>();

            foreach (BarricadeRegion region in BarricadeManager.regions)
            {
                foreach (BarricadeDrop drop in region.drops)
                {
                    if (drop.interactable is InteractableBed interactableBed)
                    {
                        beds.Add(interactableBed);
                    }
                }
            }

            foreach (VehicleBarricadeRegion region in BarricadeManager.vehicleRegions)
            {
                foreach (BarricadeDrop drop in region.drops)
                {
                    if (drop.interactable is InteractableBed interactableBed)
                    {
                        beds.Add(interactableBed);
                    }
                }
            }

            return beds;
        }

        private void PruneInvalidHomes()
        {
            foreach (PlayerHomesRecord player in playersData.ToList())
            {
                foreach (PlayerHomeEntry home in player.Homes.ToList())
                {
                    if (home.InteractableBed == null)
                    {
                        player.Homes.Remove(home);
                    }
                }

                if (player.Homes.Count == 0)
                {
                    playersData.Remove(player);
                }
            }
        }

        public PlayerHomesRecord GetOrCreatePlayer(CSteamID steamId)
        {
            PlayerHomesRecord? player = playersData.FirstOrDefault(x => x.PlayerId == steamId.m_SteamID);
            if (player == null)
            {
                player = new PlayerHomesRecord(steamId.m_SteamID);
                playersData.Add(player);
            }

            return player;
        }

        public PlayerHomeEntry? GetPlayerHome(CSteamID steamId, string? name = null)
        {
            PlayerHomesRecord player = GetOrCreatePlayer(steamId);
            return player.Homes.FirstOrDefault(x => name == null || x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public PlayerHomeEntry? GetPlayerHome(CSteamID steamId, InteractableBed interactableBed)
        {
            PlayerHomesRecord player = GetOrCreatePlayer(steamId);
            return player.Homes.FirstOrDefault(x => x.InteractableBed == interactableBed);
        }

        public PlayerHomeEntry? GetPlayerHome(CSteamID steamId, Vector3 position)
        {
            PlayerHomesRecord player = GetOrCreatePlayer(steamId);
            return player.Homes.FirstOrDefault(x =>
                Math.Abs(x.Position.X - position.x) <= 1f
                && Math.Abs(x.Position.Y - position.y) <= 1f
                && Math.Abs(x.Position.Z - position.z) <= 1f);
        }

        public bool RemoveHome(CSteamID steamId, PlayerHomeEntry home)
        {
            PlayerHomesRecord player = GetOrCreatePlayer(steamId);
            bool removed = player.Homes.Remove(home);
            if (player.Homes.Count == 0)
            {
                playersData.Remove(player);
            }
            return removed;
        }

        public void ClearHomes()
        {
            playersData.Clear();
        }

        public int RestoreAllHomesFromWorld()
        {
            ClearHomes();
            int count = 0;

            foreach (InteractableBed bed in CollectBeds())
            {
                if (!bed.isClaimed || bed.owner == CSteamID.Nil)
                {
                    continue;
                }

                PlayerHomesRecord player = GetOrCreatePlayer(bed.owner);
                PlayerHomeEntry home = new PlayerHomeEntry(player.GetUniqueHomeName(), bed);
                player.Homes.Add(home);
                count++;
            }

            SaveData();
            return count;
        }

        public void TeleportToHome(UnturnedPlayer player, string? homeName)
        {
            PlayerHomeEntry? home = GetPlayerHome(player.CSteamID, homeName);
            if (home == null)
            {
                TeleportMessageHelper.SendHomes(player, "homes_no_home");
                return;
            }

            if (!ValidateTeleport(player, home))
            {
                return;
            }

            if (playerCooldowns.TryGetValue(player.Id, out DateTime cooldownExpire) && cooldownExpire > DateTime.Now)
            {
                double secondsLeft = Math.Round((cooldownExpire - DateTime.Now).TotalSeconds);
                TeleportMessageHelper.SendHomes(player, "homes_cooldown", secondsLeft.ToString("N0"));
                return;
            }

            HomeSettings settings = U.HomeSettings.Instance;
            playerCooldowns[player.Id] = DateTime.Now.AddSeconds(VipHomeLimits.GetHomeCooldown(player.Id));
            float delay = VipHomeLimits.GetHomeDelay(player.Id);

            if (delay > 0)
            {
                TeleportMessageHelper.SendHomes(player, "homes_delay_warn", home.Name, delay);
            }

            HomeTeleportSession session = new HomeTeleportSession();

            if (settings.CancelOnMove && delay > 0)
            {
                MovementCancelWatcher.Instance.AddPlayer(player.Player, () =>
                {
                    session.IsCanceled = true;
                    TeleportMessageHelper.SendHomes(player, "homes_canceled_you_moved");
                }, settings.MoveMaxDistance);
            }

            TaskDispatcher.QueueOnMainThread(() =>
            {
                if (session.IsCanceled)
                {
                    MovementCancelWatcher.Instance.RemovePlayer(player.Player);
                    playerCooldowns.Remove(player.Id);
                    return;
                }

                MovementCancelWatcher.Instance.RemovePlayer(player.Player);

                if (!ValidateTeleport(player, home))
                {
                    playerCooldowns.Remove(player.Id);
                    return;
                }

                Vector3 destination = home.LivePosition + new Vector3(0f, settings.TeleportHeight, 0f);
                if (!player.Player.teleportToLocation(destination, player.Rotation))
                {
                    TeleportMessageHelper.SendHomes(player, "homes_teleport_failed", home.Name);
                    playerCooldowns.Remove(player.Id);
                    return;
                }

                TeleportMessageHelper.SendHomes(player, "homes_success", home.Name);
            }, delay);
        }

        private bool ValidateTeleport(UnturnedPlayer player, PlayerHomeEntry home)
        {
            if (!home.IsValidForOwner(player.CSteamID))
            {
                RemoveHome(player.CSteamID, home);
                TeleportMessageHelper.SendHomes(player, "homes_bed_destroyed");
                return false;
            }

            if (player.Stance == EPlayerStance.DRIVING)
            {
                TeleportMessageHelper.SendHomes(player, "homes_while_driving");
                return false;
            }

            if (CombatRaidTracker.Instance != null)
            {
                if (CombatRaidTracker.Instance.IsInCombat(player.CSteamID))
                {
                    TeleportMessageHelper.SendHomes(player, "homes_while_combat");
                    return false;
                }

                if (CombatRaidTracker.Instance.IsInRaid(player.CSteamID))
                {
                    TeleportMessageHelper.SendHomes(player, "homes_while_raid");
                    return false;
                }
            }

            HomeSettings settings = U.HomeSettings.Instance;
            if (settings.BlockUnderground)
            {
                Vector3 position = home.LivePosition;
                float height = LevelGround.getHeight(position);
                if (height > position.y)
                {
                    TeleportMessageHelper.SendHomes(player, "homes_underground", home.Name);
                    return false;
                }
            }

            return true;
        }

        private sealed class HomeTeleportSession
        {
            public bool IsCanceled;
        }
    }
}

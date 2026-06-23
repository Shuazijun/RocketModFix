// Adapted from RocketModPlugins/Warps behavior reference.
using Newtonsoft.Json;
using Rocket.Core.Utils;
using Rocket.Unturned.Serialisation;
using Rocket.Unturned.Teleport;
using Rocket.Unturned.Warps.Models;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Rocket.Unturned.Warps
{
    internal sealed class WarpRegistry : MonoBehaviour
    {
        private const string DataFileName = "RocketWarps.json";

        public static WarpRegistry? Instance { get; private set; }

        private readonly List<WarpPoint> warps = new List<WarpPoint>();
        private string dataFilePath = "";

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            SaveManager.onPostSave += SaveData;
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

        public void ReloadForMap()
        {
            InitializeData();
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
        }

        private void LoadData()
        {
            warps.Clear();
            if (!File.Exists(dataFilePath))
            {
                return;
            }

            try
            {
                List<WarpPoint>? loaded = JsonConvert.DeserializeObject<List<WarpPoint>>(File.ReadAllText(dataFilePath));
                if (loaded != null)
                {
                    warps.AddRange(loaded.Where(warp => string.Equals(warp.World, Provider.map, StringComparison.OrdinalIgnoreCase)));
                }
            }
            catch (Exception ex)
            {
                Core.Logging.Logger.LogException(ex, "Failed to load RocketWarps.json");
            }
        }

        private void SaveData()
        {
            if (string.IsNullOrEmpty(dataFilePath))
            {
                return;
            }

            try
            {
                string? directory = Path.GetDirectoryName(dataFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(dataFilePath, JsonConvert.SerializeObject(warps, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Core.Logging.Logger.LogException(ex, "Failed to save RocketWarps.json");
            }
        }

        public WarpPoint? GetWarp(string name)
        {
            return warps.FirstOrDefault(warp =>
                string.Equals(warp.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public List<WarpPoint> SearchWarps(string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                return warps.OrderBy(warp => warp.Name, StringComparer.OrdinalIgnoreCase).ToList();
            }

            return warps
                .Where(warp => warp.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(warp => warp.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public bool SetWarp(WarpPoint warp)
        {
            warps.RemoveAll(existing => string.Equals(existing.Name, warp.Name, StringComparison.OrdinalIgnoreCase));
            warps.Add(warp);
            SaveData();
            return true;
        }

        public bool RemoveWarp(string name)
        {
            int removed = warps.RemoveAll(warp => string.Equals(warp.Name, name, StringComparison.OrdinalIgnoreCase));
            if (removed > 0)
            {
                SaveData();
                return true;
            }

            return false;
        }

        public int RemoveAllForMap(string mapName)
        {
            string normalizedMap = mapName.ToLowerInvariant();
            string currentMap = Provider.map.ToLowerInvariant();

            if (string.Equals(normalizedMap, currentMap, StringComparison.Ordinal))
            {
                int count = warps.Count;
                warps.Clear();
                SaveData();
                return count;
            }

            string otherMapPath = Path.Combine(ReadWrite.PATH, "Servers", Dedicator.serverID, "Level", mapName, DataFileName);
            if (!File.Exists(otherMapPath))
            {
                return 0;
            }

            try
            {
                List<WarpPoint>? loaded = JsonConvert.DeserializeObject<List<WarpPoint>>(File.ReadAllText(otherMapPath));
                if (loaded == null || loaded.Count == 0)
                {
                    return 0;
                }

                int removed = loaded.RemoveAll(warp => string.Equals(warp.World, mapName, StringComparison.OrdinalIgnoreCase));
                File.WriteAllText(otherMapPath, JsonConvert.SerializeObject(loaded, Formatting.Indented));
                return removed;
            }
            catch (Exception ex)
            {
                Core.Logging.Logger.LogException(ex, $"Failed to clear warps for map '{mapName}'");
                return 0;
            }
        }

        public Vector3 GetLocation(WarpPoint warp)
        {
            return new Vector3(warp.X, warp.Y, warp.Z);
        }
    }
}

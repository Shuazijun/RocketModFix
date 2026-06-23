using Newtonsoft.Json;
using SDG.Unturned;
using Steamworks;
using System.Reflection;
using UnityEngine;

namespace Rocket.Unturned.Homes.Models
{
    public sealed class PlayerHomeEntry
    {
        private static readonly MethodInfo ServerSetBedOwnerInternal = typeof(BarricadeManager).GetMethod(
            "ServerSetBedOwnerInternal",
            BindingFlags.Static | BindingFlags.NonPublic)!;

        public PlayerHomeEntry() { }

        public PlayerHomeEntry(string name, InteractableBed interactableBed)
        {
            Name = name;
            InteractableBed = interactableBed;
            Position = new StoredPosition(interactableBed.transform.position);
        }

        public string Name { get; set; } = "";

        public StoredPosition Position { get; set; } = new StoredPosition();

        [JsonIgnore]
        public Vector3 LivePosition =>
            InteractableBed != null ? InteractableBed.transform.position : Position.ToVector3();

        [JsonIgnore]
        public InteractableBed? InteractableBed { get; set; }

        public void Claim(SDG.Unturned.Player player)
        {
            if (InteractableBed == null)
            {
                return;
            }

            if (!BarricadeManager.tryGetRegion(InteractableBed.transform, out byte x, out byte y, out ushort plant, out BarricadeRegion region))
            {
                return;
            }

            ServerSetBedOwnerInternal.Invoke(null, new object[] { InteractableBed, x, y, plant, region, player.channel.owner.playerID.steamID });
        }

        public void Unclaim()
        {
            if (InteractableBed == null)
            {
                return;
            }

            BarricadeManager.ServerUnclaimBed(InteractableBed);
        }

        public void DestroyBed()
        {
            if (InteractableBed == null)
            {
                return;
            }

            BarricadeDrop? drop = BarricadeManager.FindBarricadeByRootTransform(InteractableBed.transform);
            if (drop == null)
            {
                return;
            }

            if (!BarricadeManager.tryGetRegion(InteractableBed.transform, out byte x, out byte y, out ushort plant, out _))
            {
                return;
            }

            Unclaim();
            BarricadeManager.destroyBarricade(drop, x, y, plant);
        }

        public bool IsValidForOwner(CSteamID owner)
        {
            return InteractableBed != null
                && InteractableBed.isActiveAndEnabled
                && InteractableBed.owner == owner;
        }
    }
}

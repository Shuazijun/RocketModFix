using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Rocket.Unturned.Vehicles
{
    internal static class UnturnedVehicleSpawn
    {
        private const float ForwardDistance = 10f;
        private const float GroundClearance = 0.75f;

        public static InteractableVehicle? SpawnForPlayer(SDG.Unturned.Player player, Asset asset)
        {
            if (player == null || asset == null)
            {
                return null;
            }

            VehicleAsset? vehicleAsset = VehicleTool.HandleRedirects(asset);
            if (vehicleAsset == null)
            {
                return null;
            }

            Vector3 position = GetSafeSpawnPosition(player);
            Quaternion rotation = Quaternion.Euler(0f, player.transform.rotation.eulerAngles.y, 0f);
            return VehicleManager.spawnVehicleV2(vehicleAsset, position, rotation);
        }

        public static VehicleAsset? ResolveVehicleAsset(string input)
        {
            input = input.Trim();
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            if (Guid.TryParse(input, out Guid guid))
            {
                return VehicleTool.HandleRedirects(Assets.find(guid));
            }

            if (ushort.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out ushort vehicleId))
            {
                return VehicleTool.FindVehicleByLegacyIdAndHandleRedirects(vehicleId);
            }

            List<VehicleAsset> assets = new List<VehicleAsset>();
            Assets.find(assets);

            foreach (VehicleAsset vehicle in assets)
            {
                if (string.Equals(input, vehicle.name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return VehicleTool.HandleRedirects(vehicle);
                }
            }

            foreach (VehicleAsset vehicle in assets)
            {
                if (string.Equals(input, vehicle.vehicleName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return VehicleTool.HandleRedirects(vehicle);
                }
            }

            foreach (VehicleAsset vehicle in assets)
            {
                if (vehicle.name != null && vehicle.name.IndexOf(input, StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    return VehicleTool.HandleRedirects(vehicle);
                }
            }

            foreach (VehicleAsset vehicle in assets)
            {
                if (vehicle.vehicleName != null && vehicle.vehicleName.IndexOf(input, StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    return VehicleTool.HandleRedirects(vehicle);
                }
            }

            return null;
        }

        private static Vector3 GetSafeSpawnPosition(SDG.Unturned.Player player)
        {
            Vector3 forward = player.transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.01f)
            {
                forward = Vector3.forward;
            }
            else
            {
                forward.Normalize();
            }

            Vector3 position = player.transform.position + forward * ForwardDistance;
            Vector3 rayOrigin = position + Vector3.up * 32f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 64f, RayMasks.BLOCK_VEHICLE))
            {
                position.y = hit.point.y + GroundClearance;
            }
            else
            {
                position.y = player.transform.position.y + GroundClearance;
            }

            return position;
        }
    }
}

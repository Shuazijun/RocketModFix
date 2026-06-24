using System.Collections.Generic;
using System.Linq;
using SDG.Unturned;

namespace Rocket.Unturned.Vehicles
{
    public static class UnturnedVehicles
    {
        public sealed class VehicleAssetExportSnapshot
        {
            public IReadOnlyList<VehicleAsset> Vehicles;
            public int SkippedCount;

            public VehicleAssetExportSnapshot(IReadOnlyList<VehicleAsset> vehicles, int skippedCount)
            {
                Vehicles = vehicles;
                SkippedCount = skippedCount;
            }
        }

        public static VehicleAssetExportSnapshot GetVehicleAssetExportSnapshot()
        {
            List<VehicleAsset> assets = new List<VehicleAsset>();
            Assets.find(assets);

            List<VehicleAsset> valid = assets
                .Where(IsExportableVehicle)
                .OrderBy(v => v.id)
                .ToList();

            int skipped = assets.Count(asset => asset == null || !IsExportableVehicle(asset));
            return new VehicleAssetExportSnapshot(valid, skipped);
        }

        public static IReadOnlyList<VehicleAsset> GetRegisteredVehicleAssets()
        {
            return GetVehicleAssetExportSnapshot().Vehicles;
        }

        private static bool IsExportableVehicle(VehicleAsset? asset)
        {
            return asset != null
                && asset.id > 0
                && !string.IsNullOrWhiteSpace(asset.vehicleName);
        }
    }
}

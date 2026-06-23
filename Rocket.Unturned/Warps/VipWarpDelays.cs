using Rocket.API;
using Rocket.Core;
using Rocket.Unturned.Serialisation;
using System.Linq;

namespace Rocket.Unturned.Warps
{
    internal static class VipWarpDelays
    {
        public static int GetWarpDelay(string playerId)
        {
            WarpSettings settings = U.WarpSettings.Instance;
            IRocketPlayer rocketPlayer = new RocketPlayer(playerId);
            int minDelay = settings.DefaultWarpDelay;

            foreach (VipPermissionEntry vip in settings.VipDelays)
            {
                if (rocketPlayer.HasPermission(vip.PermissionTag))
                {
                    minDelay = minDelay > vip.Value ? vip.Value : minDelay;
                }
            }

            return minDelay;
        }
    }
}

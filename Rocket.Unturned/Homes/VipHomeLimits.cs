using Rocket.API;
using Rocket.Core;
using Rocket.Unturned.Serialisation;
using System.Linq;

namespace Rocket.Unturned.Homes
{
    internal static class VipHomeLimits
    {
        public static int GetMaxHomes(string playerId)
        {
            HomeSettings settings = U.HomeSettings.Instance;
            IRocketPlayer rocketPlayer = new RocketPlayer(playerId);
            int maxHomes = settings.DefaultMaxHomes;

            foreach (VipPermissionEntry vip in settings.VipMaxHomes)
            {
                if (rocketPlayer.HasPermission(vip.PermissionTag))
                {
                    maxHomes = maxHomes < vip.Value ? vip.Value : maxHomes;
                }
            }

            return maxHomes;
        }

        public static int GetHomeDelay(string playerId)
        {
            HomeSettings settings = U.HomeSettings.Instance;
            IRocketPlayer rocketPlayer = new RocketPlayer(playerId);
            int minDelay = settings.DefaultHomeDelay;

            foreach (VipPermissionEntry vip in settings.VipDelays)
            {
                if (rocketPlayer.HasPermission(vip.PermissionTag))
                {
                    minDelay = minDelay > vip.Value ? vip.Value : minDelay;
                }
            }

            return minDelay;
        }

        public static int GetHomeCooldown(string playerId)
        {
            HomeSettings settings = U.HomeSettings.Instance;
            IRocketPlayer rocketPlayer = new RocketPlayer(playerId);
            int minCooldown = settings.DefaultHomeCooldown;

            foreach (VipPermissionEntry vip in settings.VipCooldowns)
            {
                if (rocketPlayer.HasPermission(vip.PermissionTag))
                {
                    minCooldown = minCooldown > vip.Value ? vip.Value : minCooldown;
                }
            }

            return minCooldown;
        }
    }
}

// Bed claim hooks adapted from RestoreMonarchy MoreHomes (MIT).
using HarmonyLib;
using Rocket.Unturned.Homes.Models;
using Rocket.Unturned.Player;
using Rocket.Unturned.Teleport;
using SDG.Unturned;
using Steamworks;
using System.Linq;

namespace Rocket.Unturned.Homes.Patches
{
    [HarmonyPatch(typeof(InteractableBed))]
    internal static class InteractableBedPatches
    {
        [HarmonyPatch(nameof(InteractableBed.ReceiveClaimRequest))]
        [HarmonyPrefix]
        private static bool ReceiveClaimRequestPrefix(InteractableBed __instance, in ServerInvocationContext context)
        {
            if (__instance == null || HomeRegistry.Instance == null || !U.HomeSettings.Instance.Enabled)
            {
                return true;
            }

            if (!BarricadeManager.tryGetRegion(__instance.transform, out byte x, out byte y, out ushort plant, out BarricadeRegion region))
            {
                return false;
            }

            SDG.Unturned.Player? player = context.GetPlayer();
            if (player == null || player.life.isDead)
            {
                return false;
            }

            if ((__instance.transform.position - player.transform.position).sqrMagnitude > 400f)
            {
                return false;
            }

            CSteamID steamId = player.channel.owner.playerID.steamID;
            if (!__instance.isClaimable || !__instance.checkClaim(steamId))
            {
                return true;
            }

            if (__instance.isClaimed)
            {
                PlayerHomeEntry? home = HomeRegistry.Instance.GetPlayerHome(steamId, __instance);
                if (home != null)
                {
                    HomeRegistry.Instance.RemoveHome(steamId, home);
                    home.Unclaim();
                }
                else
                {
                    BarricadeManager.ServerUnclaimBed(__instance);
                }
            }
            else
            {
                UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromPlayer(player);
                PlayerHomesRecord playerData = HomeRegistry.Instance.GetOrCreatePlayer(steamId);
                int maxHomes = VipHomeLimits.GetMaxHomes(unturnedPlayer.Id);

                if (maxHomes == 1 && playerData.Homes.Count == 1)
                {
                    foreach (PlayerHomeEntry existing in playerData.Homes.ToArray())
                    {
                        HomeRegistry.Instance.RemoveHome(steamId, existing);
                        existing.Unclaim();
                    }
                }
                else if (maxHomes <= playerData.Homes.Count)
                {
                    TeleportMessageHelper.SendHomes(unturnedPlayer, "homes_max_warn");
                    return false;
                }

                PlayerHomeEntry playerHome = new PlayerHomeEntry(playerData.GetUniqueHomeName(), __instance);
                playerData.Homes.Add(playerHome);
                playerHome.Claim(player);
                TeleportMessageHelper.SendHomes(unturnedPlayer, "homes_claimed", playerHome.Name);
            }

            return false;
        }
    }
}

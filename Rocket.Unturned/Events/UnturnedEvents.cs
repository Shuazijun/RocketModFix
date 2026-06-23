using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Rocket.Core.Extensions;
using Rocket.API;
using Rocket.API.Extensions;
using Rocket.Core.Logging;

namespace Rocket.Unturned.Events
{
    public sealed class UnturnedEvents : MonoBehaviour, IRocketImplementationEvents
    {
        private static UnturnedEvents Instance = null!;
        private void Awake()
        {
            Instance = this;
            Provider.onServerDisconnected += (CSteamID r) => {
                if (r != CSteamID.Nil)
                {
                    UnturnedPlayer? disconnected = UnturnedPlayer.FromCSteamID(r);
                    if (disconnected != null)
                    {
                        OnPlayerDisconnected.TryInvoke(disconnected);
                    }
                }
            };
            Provider.onServerShutdown += () => { onShutdown.TryInvoke(); };
            Provider.onServerConnected += (CSteamID r) => {
                if (r != CSteamID.Nil)
                {
                    UnturnedPlayer? p = UnturnedPlayer.FromCSteamID(r);
                    if (p == null)
                    {
                        return;
                    }
                    p.Player.gameObject.TryAddComponent<UnturnedPlayerFeatures>();
                    p.Player.gameObject.TryAddComponent<UnturnedPlayerMovement>();
                    p.Player.gameObject.TryAddComponent<UnturnedPlayerEvents>();
                    OnBeforePlayerConnected.TryInvoke(p);
                }
            };
            DamageTool.damagePlayerRequested += (ref DamagePlayerParameters parameters, ref bool shouldAllow) =>
            {
                if (OnPlayerDamaged != null && parameters.player != null && parameters.killer != CSteamID.Nil)
                {
                    UnturnedPlayer getterDamage = UnturnedPlayer.FromPlayer(parameters.player);
                    UnturnedPlayer? senderDamage = UnturnedPlayer.FromCSteamID(parameters.killer);
                    if (senderDamage == null)
                    {
                        return;
                    }
                    bool canDamage = shouldAllow;
                    OnPlayerDamaged.TryInvoke(getterDamage, parameters.cause, parameters.limb, senderDamage, parameters.direction, parameters.damage, parameters.times, canDamage);
                    shouldAllow = canDamage;
                }
            };
        }

        public delegate void PlayerDisconnected(UnturnedPlayer player);
        public event PlayerDisconnected OnPlayerDisconnected = null!;

        public delegate void OnPlayerGetDamage(UnturnedPlayer player, ref EDeathCause cause, ref ELimb limb, ref UnturnedPlayer killer, ref Vector3 direction, ref float damage, ref float times, ref bool canDamage);
        public static event OnPlayerGetDamage OnPlayerDamaged = null!;

        private event ImplementationShutdown onShutdown = null!;
        public event ImplementationShutdown OnShutdown
        {
            add
            {
                onShutdown += value;
            }

            remove
            {
                onShutdown -= value;
            }
        }

        internal static void triggerOnPlayerConnected(UnturnedPlayer player)
        {
            Instance.OnPlayerConnected.TryInvoke(player);
        }

        public delegate void PlayerConnected(UnturnedPlayer player);
        public event PlayerConnected OnPlayerConnected = null!;
        public event PlayerConnected OnBeforePlayerConnected = null!;
    }
}
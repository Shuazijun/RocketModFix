using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace Rocket.Unturned.Effects
{
    internal static class UnturnedEffectUtility
    {
        public static bool TryCreateParameters(ushort effectId, out TriggerEffectParameters parameters)
        {
            if (Assets.find(EAssetType.EFFECT, effectId) is EffectAsset effectAsset)
            {
                parameters = new TriggerEffectParameters(effectAsset);
                return true;
            }

            parameters = default;
            return false;
        }

        public static void Trigger(ushort effectId, Vector3 position, SteamPlayer relevantPlayer)
        {
            if (!TryCreateParameters(effectId, out TriggerEffectParameters parameters))
            {
                return;
            }

            parameters.position = position;
            parameters.SetRelevantPlayer(relevantPlayer.transportConnection);
            EffectManager.triggerEffect(parameters);
        }

        public static void TriggerGlobal(ushort effectId, Vector3 position, float relevantDistance = 1024f)
        {
            if (!TryCreateParameters(effectId, out TriggerEffectParameters parameters))
            {
                return;
            }

            parameters.position = position;
            parameters.relevantDistance = relevantDistance;
            EffectManager.triggerEffect(parameters);
        }
    }
}

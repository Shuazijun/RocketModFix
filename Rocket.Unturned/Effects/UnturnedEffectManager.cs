using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;

namespace Rocket.Unturned.Effects
{
    public class UnturnedEffect
    {
        public UnturnedEffect(string type, ushort effectID, bool global)
        {
            this.Type = type;
            this.EffectID = effectID;
            this.Global = global;
        }
        public string Type;
        public ushort EffectID;
        public bool Global;

        public void Trigger(UnturnedPlayer player)
        {
            Vector3 position = player.Player.transform.position;
            if (!Global)
            {
                UnturnedEffectUtility.Trigger(EffectID, position, player.Player.channel.owner);
            }
            else
            {
                UnturnedEffectUtility.TriggerGlobal(EffectID, position);
            }
        }

        public void Trigger(Vector3 position)
        {
            UnturnedEffectUtility.TriggerGlobal(EffectID, position);
        }
    }
}

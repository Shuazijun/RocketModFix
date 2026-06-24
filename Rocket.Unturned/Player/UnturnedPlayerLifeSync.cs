using SDG.NetTransport;
using SDG.Unturned;
using System;
using System.Reflection;

namespace Rocket.Unturned.Player
{
    internal static class UnturnedPlayerLifeSync
    {
        private static readonly FieldInfo SendLifeStatsField = typeof(PlayerLife).GetField(
            "SendLifeStats",
            BindingFlags.Static | BindingFlags.NonPublic)!;

        private static readonly MethodInfo SendLifeStatsInvoke = SendLifeStatsField.FieldType.GetMethod("Invoke")!;

        public static void ServerRefillOxygen(PlayerLife life)
        {
            if (life == null || life.isDead || !life.IsAlive)
            {
                return;
            }

            byte missing = (byte)Math.Max(0, 100 - life.oxygen);
            if (missing > 0)
            {
                life.askBreath(missing);
            }

            ServerSyncLifeStats(life);
        }

        private static void ServerSyncLifeStats(PlayerLife life)
        {
            object sendLifeStats = SendLifeStatsField.GetValue(null)!;
            SendLifeStatsInvoke.Invoke(sendLifeStats, new object[]
            {
                life.GetNetId(),
                ENetReliability.Reliable,
                life.player.channel.GetOwnerTransportConnection(),
                life.health,
                life.food,
                life.water,
                life.virus,
                life.oxygen,
                life.isBleeding,
                life.isBroken
            });
        }
    }
}

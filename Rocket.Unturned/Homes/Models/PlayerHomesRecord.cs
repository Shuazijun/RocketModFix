using System;
using System.Collections.Generic;
using System.Linq;

namespace Rocket.Unturned.Homes.Models
{
    public sealed class PlayerHomesRecord
    {
        public PlayerHomesRecord()
        {
            Homes = new List<PlayerHomeEntry>();
        }

        public PlayerHomesRecord(ulong playerId)
        {
            PlayerId = playerId;
            DefaultHomeName = "bed";
            Homes = new List<PlayerHomeEntry>();
        }

        public ulong PlayerId { get; set; }

        public string DefaultHomeName { get; set; } = "bed";

        public List<PlayerHomeEntry> Homes { get; set; }

        public string GetUniqueHomeName()
        {
            int num = 1;
            while (Homes.Exists(x => x.Name.Equals(DefaultHomeName + num, StringComparison.OrdinalIgnoreCase)))
            {
                num++;
            }
            return DefaultHomeName + num;
        }
    }
}

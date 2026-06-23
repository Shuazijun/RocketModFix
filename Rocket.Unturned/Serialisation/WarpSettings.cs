using Rocket.API;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Rocket.Unturned.Serialisation
{
    public sealed class WarpSettings : IDefaultable
    {
        [XmlElement("Enabled")]
        public bool Enabled;

        [XmlElement("MessageColor")]
        public string MessageColor = "cyan";

        [XmlElement("MessageIconUrl")]
        public string MessageIconUrl = "";

        [XmlElement("DefaultWarpDelay")]
        public int DefaultWarpDelay = 10;

        [XmlElement("CancelOnMove")]
        public bool CancelOnMove = true;

        [XmlElement("MoveMaxDistance")]
        public float MoveMaxDistance = 0.5f;

        [XmlElement("BlockInVehicle")]
        public bool BlockInVehicle = true;

        [XmlElement("UseUnsafeTeleport")]
        public bool UseUnsafeTeleport;

        [XmlElement("AllowCombat")]
        public bool AllowCombat;

        [XmlElement("AllowRaid")]
        public bool AllowRaid;

        [XmlArray("VipDelays")]
        [XmlArrayItem("VIPPermission")]
        public List<VipPermissionEntry> VipDelays = new List<VipPermissionEntry>();

        public void LoadDefaults()
        {
            Enabled = false;
            MessageColor = "cyan";
            MessageIconUrl = "";
            DefaultWarpDelay = 10;
            CancelOnMove = true;
            MoveMaxDistance = 0.5f;
            BlockInVehicle = true;
            UseUnsafeTeleport = false;
            AllowCombat = false;
            AllowRaid = false;
            VipDelays = new List<VipPermissionEntry>
            {
                new VipPermissionEntry { PermissionTag = "rocket.warp.vip", Value = 5 },
                new VipPermissionEntry { PermissionTag = "rocket.warp.star", Value = 3 }
            };
        }
    }
}
